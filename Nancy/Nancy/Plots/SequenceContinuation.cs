using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// The point a sequence's last finite line is carried to, when the data goes on past the plot.
/// </summary>
/// <param name="Time">The time to carry the line to, which is the right edge of the plot.</param>
/// <param name="Value">The value the line reaches there, following the slope it ended with.</param>
public readonly record struct TrailingContinuation(Rational Time, Rational Value);

/// <summary>
/// Algorithms deciding how a sequence meets the edge of a plot.
/// </summary>
/// <remarks>
/// A sequence obtained by cutting a curve stops at the cut, which is an artifact of plotting rather than a property of the data.
/// Marking that stop reads as the curve ending there, so the line is carried on to the edge instead.
/// A sequence plotted directly gets none of this: it ends where it ends, and nothing is known past it.
/// This is the same rule <see cref="InfiniteRegions.EnumerateVisibleInfiniteRegions"/> applies to the areas, kept here so that every renderer decides it the same way.
/// </remarks>
public static class SequenceContinuation
{
    /// <summary>
    /// Computes where the last finite line of <paramref name="sequence"/> should be carried to.
    /// </summary>
    /// <param name="sequence">The sequence being plotted.</param>
    /// <param name="xLimit">The x-axis limits of the plot.</param>
    /// <param name="continuesPastEnd">True if the sequence is a cut of a curve that goes on past it.</param>
    /// <returns>The point to carry the line to, or null if the line should stop where the sequence does.</returns>
    /// <remarks>
    /// The mark at the sequence's end is dropped whenever this returns a point, since that end is the cut rather than the curve's own.
    /// Nothing is returned when the sequence ends on an infinite element: there is no finite line to carry, and the area marks the value instead.
    /// </remarks>
    public static TrailingContinuation? GetTrailingContinuation(
        this Sequence sequence,
        Interval xLimit,
        bool continuesPastEnd)
    {
        if (!continuesPastEnd)
            return null;

        if (!xLimit.Upper.IsFinite || xLimit.Upper <= sequence.DefinedUntil)
            return null;

        var last = sequence.Elements.LastOrDefault();
        if (last is null || last.IsInfinite)
            return null;

        var lastSegment = sequence.Elements
            .OfType<Segment>()
            .LastOrDefault(s => s.IsFinite);
        if (lastSegment is null)
            return null;

        var endValue = last switch
        {
            Point point => point.Value,
            Segment segment => segment.LeftLimitAtEndTime,
            _ => Rational.Zero
        };
        if (!endValue.IsFinite)
            return null;

        return new TrailingContinuation(
            xLimit.Upper,
            endValue + lastSegment.Slope * (xLimit.Upper - sequence.DefinedUntil));
    }
}
