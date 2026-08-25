using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// A maximal interval over which a sequence takes the same infinite value.
/// </summary>
/// <param name="StartTime">The time the infinite part starts at.</param>
/// <param name="EndTime">The time the infinite part ends at.</param>
/// <param name="IsStartIncluded">True if the sequence is already infinite at <paramref name="StartTime"/>.</param>
/// <param name="IsEndIncluded">True if the sequence is still infinite at <paramref name="EndTime"/>.</param>
/// <param name="IsPlusInfinite">True if the value is $+\infty$, false if it is $-\infty$.</param>
/// <remarks>
/// <paramref name="IsStartIncluded"/> and <paramref name="IsEndIncluded"/> are informational: no backend draws the areas with a border, since at a curve's weight one reads as a segment of the curve itself.
/// They are kept because they are correct and cheap, and because open versus closed is a property of the region rather than of how it is drawn.
/// </remarks>
public readonly record struct InfiniteRegion(
    Rational StartTime,
    Rational EndTime,
    bool IsStartIncluded,
    bool IsEndIncluded,
    bool IsPlusInfinite
);

/// <summary>
/// Algorithms to locate the infinite parts of a sequence, so that plots can mark them.
/// </summary>
public static class InfiniteRegions
{
    /// <summary>
    /// Enumerates the maximal intervals over which <paramref name="sequence"/> is infinite.
    /// </summary>
    /// <param name="sequence">The sequence to inspect.</param>
    /// <remarks>
    /// Runs of $+\infty$ and $-\infty$ are never merged, so each region has a single sign.
    /// An endpoint is included if the element at it is a <see cref="Point"/>, excluded if it is a <see cref="Segment"/>.
    /// </remarks>
    public static IEnumerable<InfiniteRegion> EnumerateInfiniteRegions(this Sequence sequence)
    {
        Element? runStart = null;
        Element? runEnd = null;
        var isRunPlusInfinite = false;

        foreach (var element in sequence.Elements)
        {
            if (element.IsFinite)
            {
                if (runStart is not null)
                {
                    yield return BuildRegion(runStart, runEnd!, isRunPlusInfinite);
                    runStart = null;
                }
                continue;
            }

            // a run breaks when the sign changes, so that the two infinities are never merged
            if (runStart is not null && element.IsPlusInfinite != isRunPlusInfinite)
            {
                yield return BuildRegion(runStart, runEnd!, isRunPlusInfinite);
                runStart = null;
            }

            if (runStart is null)
            {
                runStart = element;
                isRunPlusInfinite = element.IsPlusInfinite;
            }

            runEnd = element;
        }

        if (runStart is not null)
            yield return BuildRegion(runStart, runEnd!, isRunPlusInfinite);
    }

    private static InfiniteRegion BuildRegion(Element start, Element end, bool isPlusInfinite)
    {
        return new InfiniteRegion(
            StartTime: start.StartTime,
            EndTime: end.EndTime,
            IsStartIncluded: start is Point,
            IsEndIncluded: end is Point,
            IsPlusInfinite: isPlusInfinite
        );
    }

    /// <summary>
    /// Enumerates the infinite regions of <paramref name="sequence"/>, clipped to the plot's x-axis limits.
    /// </summary>
    /// <param name="sequence">The sequence to inspect.</param>
    /// <param name="xLimit">The x-axis limits of the plot.</param>
    /// <param name="continuesPastEnd">True if the sequence is a cut of a curve that goes on past it.</param>
    /// <remarks>
    /// When <paramref name="continuesPastEnd"/> is set, a region reaching the end of the sequence's definition is extended to the right axis limit, since stopping the area at the cut would suggest the value ends there.
    /// It is not set when sequences are plotted directly: a sequence ends where it ends, and drawing past it would claim a value that is not known.
    /// Nothing is extended to the left, where there is no sample to claim a value from either way.
    /// Regions falling entirely outside <paramref name="xLimit"/> are dropped.
    /// </remarks>
    public static IEnumerable<InfiniteRegion> EnumerateVisibleInfiniteRegions(
        this Sequence sequence,
        Interval xLimit,
        bool continuesPastEnd)
    {
        foreach (var region in sequence.EnumerateInfiniteRegions())
        {
            var startTime = region.StartTime;
            var endTime = region.EndTime;
            var isStartIncluded = region.IsStartIncluded;
            var isEndIncluded = region.IsEndIncluded;

            // the sequence is a cut of a curve that continues, so extend to the axis rather than to the cut.
            // only to the right: before the first sample there is nothing to claim the value of,
            // and a point has no width, so a region opening at one starts at DefinedFrom without covering it
            if (continuesPastEnd && endTime >= sequence.DefinedUntil && xLimit.Upper > endTime)
            {
                endTime = xLimit.Upper;
                isEndIncluded = false;
            }

            // clipping to the axis hides the endpoint, so it is no longer the region's own
            if (startTime < xLimit.Lower)
            {
                startTime = xLimit.Lower;
                isStartIncluded = false;
            }

            if (endTime > xLimit.Upper)
            {
                endTime = xLimit.Upper;
                isEndIncluded = false;
            }

            if (startTime > endTime)
                continue;

            yield return new InfiniteRegion(
                startTime, endTime, isStartIncluded, isEndIncluded, region.IsPlusInfinite);
        }
    }
}
