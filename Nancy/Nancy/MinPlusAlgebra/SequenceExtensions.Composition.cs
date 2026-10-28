using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// Enumerates the composition $f(g(t))$ over the support of a finite-valued, non-negative, non-decreasing inner stream.
    /// </summary>
    /// <param name="f">The outer nonempty uninterrupted stream, in time order, covering the image of the inner operand.</param>
    /// <param name="g">The inner nonempty uninterrupted stream in time order.</param>
    /// <returns>The composition in time order, including preimages of outer breakpoints, before merging redundant elements.</returns>
    /// <remarks>
    /// <para>
    /// This is an incremental realization of the breakpoint/preimage partition in [ZNS23b] Section 5.2.
    /// The non-decreasing inner values let an <see cref="ElementCursor"/> walk through the outer function in argument order.
    /// Each increasing inner segment maps outer breakpoints back to its time axis; jumps skip unused outer levels.
    /// Points and constant inner segments use the attained outer value, which may differ from its one-sided limits.
    /// </para>
    /// <para>
    /// Each operand is traversed at most once, with checks on consumed structure, inner values and outer image coverage.
    /// An unread invalid tail does not cause an exception when enumeration stops early.
    /// A finite-valued inner segment may have unbounded support and limit $+\infty$ at its open end.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand violates uninterrupted time order, an operand is empty, the inner operand is negative, decreasing or infinite, or the outer operand does not cover its image.</exception>
    public static IEnumerable<Element> Composition(this IEnumerable<Element> f, IEnumerable<Element> g)
    {
        using var outer = new ElementCursor(f);
        if (!outer.MoveNext())
            throw new ArgumentException("The outer operand is empty.");

        Point SampleOuter(Rational time)
        {
            try
            {
                return outer.PointAt(time);
            }
            catch (Exception e) when (e is BeforeLeftEndpointException or AfterRightEndpointException)
            {
                throw new ArgumentException("The outer operand does not cover the image of the inner operand.", e);
            }
        }

        Segment OuterSegmentAfter(Rational time)
        {
            try
            {
                return outer.SegmentAfter(time);
            }
            catch (Exception e) when (e is BeforeLeftEndpointException or AfterRightEndpointException)
            {
                throw new ArgumentException("The outer operand does not cover the image of the inner operand.", e);
            }
        }

        Rational? previousInnerValue = null;
        using var innerCursor = new ElementCursor(g);
        while (innerCursor.MoveNext())
        {
            var element = innerCursor.Current;
            var startValue = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (startValue.IsInfinite || startValue < 0 || previousInnerValue is { } old && old > startValue)
                throw new ArgumentException("The inner operand must be finite, non-negative and non-decreasing.");
            if (element is Point innerPoint)
            {
                var sample = SampleOuter(startValue);
                yield return new Point(innerPoint.Time, sample.Value);
                previousInnerValue = startValue;
                continue;
            }

            var inner = (Segment) element;
            if (inner.Slope < 0)
                throw new ArgumentException("The inner operand must be finite, non-negative and non-decreasing.");
            if (inner.IsConstant)
            {
                // The inner segment is constant at startValue, so the composition is constant at $f(\texttt{startValue})$
                // No outer breakpoint splits this interval; see [TBP-COMP-SEQ-EXT], constant inner operand.
                var sample = SampleOuter(startValue);
                yield return Segment.Constant(inner.StartTime, inner.EndTime, sample.Value);
            }
            else
            {
                // Positive slope maps this open time interval bijectively onto its open interval of levels.
                // The final level may be $+\infty$; it is a limit, not a point to sample.
                var level = startValue;
                var finalLevel = inner.LeftLimitAtEndTime;
                while (level < finalLevel)
                {
                    var outerSegment = OuterSegmentAfter(level);
                    var nextLevel = Rational.Min(finalLevel, outerSegment.EndTime);
                    var timeStart = inner.StartTime + (level - startValue) / inner.Slope;
                    var timeEnd = inner.StartTime + (nextLevel - startValue) / inner.Slope;
                    yield return new Segment(timeStart, timeEnd, outerSegment.RightLimitAt(level), outerSegment.Slope * inner.Slope);

                    // The inner segment excludes its endpoint; a following inner point owns that value.
                    if (nextLevel == finalLevel)
                    {
                        break;
                    }

                    // An interior crossing attains this outer boundary, so its point value must be preserved.
                    // The next SegmentAfter call can then move past the sampled point; subsequent point queries have larger levels.
                    var sample = SampleOuter(nextLevel);
                    yield return new Point(timeEnd, sample.Value);
                    level = nextLevel;
                }
            }
            previousInnerValue = inner.LeftLimitAtEndTime;
        }
        if (previousInnerValue is null)
            throw new ArgumentException("The inner operand is empty.");
    }
}
