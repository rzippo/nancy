using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Computes composition from inverse images and sorted breakpoint times as an independent reference for the forward sweep.
/// </summary>
internal static class MaterializedCompositionReference
{
    public static Sequence Composition(Sequence f, Sequence g)
    {
        if (!g.IsNonNegative)
            throw new ArgumentException("Inner sequence of composition must be non-negative");
        if (!g.IsNonDecreasing)
            throw new ArgumentException("Inner sequence of composition must be non-decreasing");
        if (!g.IsFinite)
            throw new ArgumentException("Inner sequence of composition must be finite");

        if (g.Count == 1)
        {
            // single-element edge cases
            var gElement = g.Elements.Single();
            if (gElement is Point pg)
            {
                if(!f.IsDefinedAt(pg.Value))
                    throw new ArgumentException("Outer sequence of composition must be defined over image of inner sequence (case gP)"); // [TBP-COMP-SEQ-EXT] gP
                return new Sequence([
                    new Point(pg.Time, f.ValueAt(pg.Value))
                ]);
            }
            else if (gElement is Segment sg)
            {
                if (sg.IsConstant)
                {
                    if(!f.IsDefinedAt(sg.RightLimitAtStartTime))
                        throw new ArgumentException("Outer sequence of composition must be defined over image of inner sequence (case gSc)"); // [TBP-COMP-SEQ-EXT] gSc
                    return new Sequence([
                        Segment.Constant(sg.StartTime, sg.EndTime, f.ValueAt(sg.RightLimitAtStartTime))
                    ]);
                }
                else
                {
                    if(!f.IsDefinedAfter(sg.RightLimitAtStartTime))
                        throw new ArgumentException("Outer sequence of composition must be defined over image of inner sequence (case gSiL)"); // [TBP-COMP-SEQ-EXT] gSiL
                    if(!f.IsDefinedBefore(sg.LeftLimitAtEndTime))
                        throw new ArgumentException("Outer sequence of composition must be defined over image of inner sequence (case gSiR)"); // [TBP-COMP-SEQ-EXT] gSiR
                    var fCut = f.CutAsEnumerable(sg.RightLimitAtStartTime, sg.LeftLimitAtEndTime, false, false);
                    var sequence = fCut.Select<Element, Element>(ef =>
                    {
                        if (ef is Segment sf)
                            return new Segment(
                                sg.StartTime + (sf.StartTime - sg.RightLimitAtStartTime) / sg.Slope,
                                sg.StartTime + (sf.EndTime - sg.RightLimitAtStartTime) / sg.Slope,
                                sf.RightLimitAtStartTime,
                                sg.Slope * sf.Slope
                            );
                        else if (ef is Point pf)
                            return new Point(
                                sg.StartTime + (pf.Time - sg.RightLimitAtStartTime) / sg.Slope,
                                pf.Value
                            );
                        else
                            throw new InvalidCastException();
                    })
                        .ToSequence();
                    return sequence;
                }
            }
            else
                throw new InvalidCastException();
        }
        else
        {
            var gImageStart = g.IsLeftClosed ?
                g.ValueAt(g.DefinedFrom) :
                g.RightLimitAt(g.DefinedFrom);

            var gImageEnd = g.IsRightClosed ?
                g.ValueAt(g.DefinedUntil) :
                g.LeftLimitAt(g.DefinedUntil);

            // The six boundary rules of [TBP-COMP-SEQ-EXT], cases gLC, gLOc, gLOi, gRC, gROc and gROi:
            // which of defined-at, defined-after and defined-before is required of f depends on two things:
            // whether the boundary of g is open, and whether the segment there is constant.
            if (g.IsLeftClosed)
            {
                if (!f.IsDefinedAt(gImageStart))
                    throw new ArgumentException(
                        "Outer sequence of composition must be defined over image of inner sequence (case gLC)");
            }
            else // g is left-open
            {
                var gs = (Segment) g.Elements[0];
                if (gs.IsConstant)
                {
                    if(!f.IsDefinedAt(gImageStart))
                        throw new ArgumentException(
                            "Outer sequence of composition must be defined over image of inner sequence (case gLOc)");
                }
                else // gs is increasing segment
                {
                    if(!f.IsDefinedAfter(gImageStart))
                        throw new ArgumentException(
                            "Outer sequence of composition must be defined over image of inner sequence (case gLOi)");
                }
            }

            if (g.IsRightClosed)
            {
                if (!f.IsDefinedAt(gImageEnd))
                    throw new ArgumentException(
                        "Outer sequence of composition must be defined over image of inner sequence (case gRC)");
            }
            else // g is right-open
            {
                var gs = (Segment) g.Elements[^1];
                if (gs.IsConstant)
                {
                    if(!f.IsDefinedAt(gImageEnd))
                        throw new ArgumentException(
                            "Outer sequence of composition must be defined over image of inner sequence (case gROc)");
                }
                else // gs is increasing segment
                {
                    if(!f.IsDefinedBefore(gImageEnd))
                        throw new ArgumentException(
                            "Outer sequence of composition must be defined over image of inner sequence (case gROi)");
                }
            }

            var gTimes = g.EnumerateBreakpoints()
                .Select(bp => bp.center.Time);
            if(g.IsLeftOpen)
                gTimes = gTimes.Prepend(g.DefinedFrom);
            if(g.IsRightOpen)
                gTimes = gTimes.Append(g.DefinedUntil);
            // Where the image of g is a single value, the result is constant.
            // The outer sequence contributes no breakpoint of its own, so there is nothing to cut.
            // [TBP-COMP-SEQ-EXT], constant inner operand.
            IEnumerable<Rational> fTimes;
            if (gImageStart < gImageEnd)
            {
                var gInverse = g.LowerPseudoInverse();
                fTimes = f.Cut(gImageStart, gImageEnd, false, false)
                    .EnumerateBreakpoints()
                    .Select(bp => gInverse.ValueAt(bp.center.Time));
            }
            else
            {
                fTimes = [];
            }

            var times = gTimes.Concat(fTimes)
                .OrderBy(t => t)
                .OrderedDistinct();
            #if DEBUG
            var elements = EnumerateComposition(times).ToList();
            var merged = elements.Merge();
            var result = new Sequence(merged);
            #else
            var result = EnumerateComposition(times)
                .MergeAsEnumerable()
                .ToSequence();
            #endif
            return result;

            IEnumerable<Element> EnumerateComposition(IEnumerable<Rational> times)
            {
                // The following loop works between two different breakpoints, computing the composition between those.
                Rational? prevTime = null;
                int lastIndexF = 0, lastIndexG = 0;
                foreach (var time in times)
                {
                    if (prevTime is { } pTime)
                    {
                        if (prevTime > g.DefinedFrom || g.IsLeftClosed)
                        {
                            var gValue = g.ValueAt(pTime);
                            var p = new Point(pTime, f.ValueAt(gValue));
                            yield return p;
                        }

                        var gRightLimit = g.RightLimitAt(pTime);
                        var (gSegment, gIndex) = g.GetSegmentAfter_Linear(pTime, lastIndexG, cutToTime: false);
                        lastIndexG = gIndex;
                        if (gSegment.Slope != 0)
                        {
                            var (fSegment, fIndex) = f.GetSegmentAfter_Linear(gRightLimit, lastIndexF, cutToTime: false);
                            lastIndexF = fIndex;
                            var s = new Segment(
                                pTime,
                                time,
                                fSegment.RightLimitAt(gRightLimit),
                                gSegment.Slope * fSegment.Slope
                            );
                            yield return s;
                        }
                        else
                        {
                            var (fElement, fIndex) = f.GetElementAt_Linear(gRightLimit, lastIndexF);
                            lastIndexF = fIndex;
                            var s = new Segment(
                                pTime,
                                time,
                                fElement.ValueAt(gRightLimit),
                                0
                            );
                            yield return s;
                        }

                        if (time == g.DefinedUntil && g.IsRightClosed)
                        {
                            var gValue = g.ValueAt(time);
                            var p = new Point(time, f.ValueAt(gValue));
                            yield return p;
                        }
                    }

                    prevTime = time;
                }
            }
        }
    }
}
