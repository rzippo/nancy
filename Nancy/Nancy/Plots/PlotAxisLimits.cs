using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Axis limits for a plot.
/// </summary>
/// <param name="XLimit">Range for the x-axis.</param>
/// <param name="YLimit">Range for the y-axis.</param>
public readonly record struct PlotAxisLimits(Interval XLimit, Interval YLimit)
{
    /// <summary>
    /// True if <see cref="YLimit"/> was extended upwards to make room for a $+\infty$ area.
    /// </summary>
    /// <remarks>
    /// A backend post-processing the limits must not undo that extension, or the area is lost.
    /// </remarks>
    public bool HasPlusInfinityBand { get; init; }

    /// <summary>
    /// True if <see cref="YLimit"/> was extended downwards to make room for a $-\infty$ area.
    /// </summary>
    /// <inheritdoc cref="HasPlusInfinityBand" path="/remarks"/>
    public bool HasMinusInfinityBand { get; init; }

    /// <summary>
    /// The y-axis range <see cref="YLimit"/> was extended by, on each side that has an area.
    /// </summary>
    public Rational InfinityBandHeight { get; init; }

    /// <summary>
    /// The vertical extent of the area marking $+\infty$, which reaches from the x-axis to the top of the plot.
    /// </summary>
    /// <remarks>
    /// Growing out of the x-axis is what tells the two signs apart, and reaching the edge is what conveys that the value goes on.
    /// </remarks>
    public Interval PlusInfinityBand => new (
        Rational.Max(0, YLimit.Lower),
        YLimit.Upper,
        isLowerIncluded: true,
        isUpperIncluded: true);

    /// <summary>
    /// The vertical extent of the area marking $-\infty$, which reaches from the x-axis to the bottom of the plot.
    /// </summary>
    /// <inheritdoc cref="PlusInfinityBand" path="/remarks"/>
    public Interval MinusInfinityBand => new (
        YLimit.Lower,
        Rational.Min(0, YLimit.Upper),
        isLowerIncluded: true,
        isUpperIncluded: true);
}

/// <summary>
/// Algorithms used to compute default plot axis limits.
/// </summary>
public static class PlotAxisLimitAlgorithms
{
    /// <summary>
    /// Computes the interval used to sample curves before plotting them as sequences.
    /// </summary>
    public static Interval GetCurveSamplingXLimit(
        IReadOnlyCollection<Curve> curves,
        PlotSettings settings)
    {
        if (curves.Count == 0)
            throw new ArgumentException("Empty curve collection.", nameof(curves));

        if (settings.XLimit is { Upper.IsFinite: true } xLimit && xLimit.Upper >= 0)
        {
            return xLimit.Lower >= 0
                ? xLimit
                : new Interval(
                    0,
                    xLimit.Upper,
                    isLowerIncluded: true,
                    isUpperIncluded: xLimit.IsUpperIncluded);
        }

        var rightEdge = curves.Max(c => c.SecondPseudoPeriodEnd);
        return new Interval(0, rightEdge, isLowerIncluded: true, isUpperIncluded: true);
    }

    /// <summary>
    /// Suggests the axis limits for already-sampled sequences.
    /// Explicit finite limits in <paramref name="settings"/> take precedence.
    /// </summary>
    /// <param name="sequences">The sequences to be plotted.</param>
    /// <param name="settings">The settings of the plot.</param>
    /// <param name="continuesPastEnd">True if the sequences are cuts of curves that go on past them.</param>
    /// <remarks>
    /// This is the default framing policy, which a renderer may use as it is, adjust, or replace.
    /// Framing belongs to the renderer: how much room a curve needs to stay clear of the frame, and whether it needs any, depends on the medium.
    /// The steps are kept separate below so that a renderer can compose its own from the same parts.
    /// </remarks>
    public static PlotAxisLimits SuggestAxisLimits(
        IReadOnlyCollection<Sequence> sequences,
        PlotSettings settings,
        bool continuesPastEnd = false)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("Empty sequence collection.", nameof(sequences));

        var finiteXLimit = GetDefaultSequenceXLimit(sequences);

        var hasPlusInfinity = false;
        var hasMinusInfinity = false;
        foreach (var sequence in sequences)
        {
            hasPlusInfinity |= sequence.HasPlusInfinity;
            hasMinusInfinity |= sequence.HasMinusInfinity;
        }

        // the infinities tell which way the curve goes even where they are not drawn,
        // which is the only hint available when every finite value is 0
        var prefersRoomBelow = hasMinusInfinity && !hasPlusInfinity;

        var xLimit = GetFiniteLimit(settings.XLimit) ?? ApplySignedMargin(
            EnsureNonDegenerate(finiteXLimit, Rational.One, prefersRoomBelow: false),
            settings.RelativeXAxisMargin);

        if (GetFiniteLimit(settings.YLimit) is { } explicitYLimit)
            return new PlotAxisLimits(xLimit, explicitYLimit);

        // a curve carried on to the edge reaches past its last sampled value,
        // so the value it reaches there is part of what has to fit
        var finiteYLimit = GetDefaultSequenceYLimit(sequences);
        foreach (var sequence in sequences)
        {
            if (sequence.GetTrailingContinuation(xLimit, continuesPastEnd) is { } trailing)
                finiteYLimit = new Interval(
                    Rational.Min(finiteYLimit.Lower, trailing.Value),
                    Rational.Max(finiteYLimit.Upper, trailing.Value),
                    isLowerIncluded: true,
                    isUpperIncluded: true);
        }

        var drawsAreas = settings.InfinityStrategy == InfinityStrategy.Areas &&
                         (hasPlusInfinity || hasMinusInfinity);

        var bandHeight = Rational.Zero;
        var yRange = finiteYLimit;
        if (drawsAreas)
        {
            // the areas need room of their own, which then doubles as the range a margin can scale against
            bandHeight = GetInfinityBandHeight(xLimit, finiteYLimit, settings);
            yRange = new Interval(
                hasMinusInfinity ? yRange.Lower - bandHeight : yRange.Lower,
                hasPlusInfinity ? yRange.Upper + bandHeight : yRange.Upper,
                isLowerIncluded: true,
                isUpperIncluded: true);
        }
        else
        {
            yRange = EnsureNonDegenerate(yRange, Rational.One, prefersRoomBelow);
        }

        return new PlotAxisLimits(
            xLimit,
            ApplySignedMargin(yRange, settings.RelativeYAxisMargin, prefersRoomBelow))
        {
            HasPlusInfinityBand = drawsAreas && hasPlusInfinity,
            HasMinusInfinityBand = drawsAreas && hasMinusInfinity,
            InfinityBandHeight = bandHeight
        };
    }

    /// <inheritdoc cref="SuggestAxisLimits"/>
    [Obsolete("Renamed to SuggestAxisLimits, which says that the renderer is free to frame otherwise.")]
    public static PlotAxisLimits GetSequenceAxisLimits(
        IReadOnlyCollection<Sequence> sequences,
        PlotSettings settings)
        => SuggestAxisLimits(sequences, settings);

    /// <summary>
    /// Computes the y-axis range to reserve for the areas marking infinite values.
    /// </summary>
    /// <param name="xLimit">The x-axis limits already computed for the plot.</param>
    /// <param name="finiteYLimit">The y-axis limits over the finite values, before any margin is applied.</param>
    /// <param name="settings">The settings of the plot.</param>
    /// <remarks>
    /// The area is scaled against the finite y-axis range, unless that range is degenerate.
    /// A <c>DelayServiceCurve</c> has 0 as its only finite value, so there is nothing to scale against, and the x-axis range is used instead, which keeps the plot from collapsing to a strip.
    /// </remarks>
    public static Rational GetInfinityBandHeight(
        Interval xLimit,
        Interval finiteYLimit,
        PlotSettings settings)
    {
        var finiteYRange = finiteYLimit.Upper - finiteYLimit.Lower;
        if (finiteYRange > 0)
            return finiteYRange * settings.RelativeInfinityBandHeight;

        var xRange = xLimit.Upper - xLimit.Lower;
        return xRange > 0
            ? xRange * settings.RelativeInfinityBandHeightFromXAxis
            : Rational.One;
    }

    /// <summary>
    /// Opens up an interval of a single value, on the side the values occupy.
    /// </summary>
    /// <param name="limit">The interval to open up.</param>
    /// <param name="fallbackLength">The length to open it up by.</param>
    /// <param name="prefersRoomBelow">Which side to open when the value is 0, and the interval itself does not say.</param>
    /// <remarks>
    /// A single value has no range, so a range has to be invented before any margin can scale against one.
    /// Keeping the two apart is what stops a margin rule from having to special-case degeneracy.
    /// An interval that already has a length is returned unchanged.
    /// </remarks>
    public static Interval EnsureNonDegenerate(
        Interval limit,
        Rational fallbackLength,
        bool prefersRoomBelow)
    {
        if (!limit.Lower.IsFinite || !limit.Upper.IsFinite || limit.Upper > limit.Lower)
            return limit;

        return prefersRoomBelow || limit.Upper < 0
            ? new Interval(
                limit.Lower - fallbackLength, limit.Upper,
                isLowerIncluded: true, isUpperIncluded: true)
            : new Interval(
                limit.Lower, limit.Upper + fallbackLength,
                isLowerIncluded: true, isUpperIncluded: true);
    }

    /// <summary>
    /// Adds a margin around the given interval, larger on the side the values occupy.
    /// </summary>
    /// <param name="limit">The interval to add the margin to.</param>
    /// <param name="relativeMargin">The margin, as a ratio over the interval's length.</param>
    /// <param name="prefersRoomBelow">Which side to treat as occupied when every value is 0, and the interval itself does not say.</param>
    /// <remarks>
    /// A non-negative curve is given its room above and only half as much below, so that the axis still reads as starting at 0, and a non-positive one the other way around.
    /// A curve crossing 0 occupies both sides and is given the same margin on each.
    /// An interval of a single value is returned unchanged: open it with <see cref="EnsureNonDegenerate"/> first.
    /// </remarks>
    public static Interval ApplySignedMargin(
        Interval limit,
        double relativeMargin,
        bool prefersRoomBelow = false)
    {
        if (relativeMargin < 0)
            throw new ArgumentOutOfRangeException(
                nameof(relativeMargin),
                relativeMargin,
                "Relative axis margins cannot be negative.");

        if (relativeMargin == 0 || !limit.Lower.IsFinite || !limit.Upper.IsFinite)
            return limit;

        var length = limit.Upper - limit.Lower;
        if (length <= 0)
            return limit;

        var large = length * relativeMargin;
        var small = large / 2;

        if (limit.Lower < 0 && limit.Upper > 0)
            return Expand(limit, large, large);

        return limit.Upper <= 0 && (limit.Lower < 0 || prefersRoomBelow)
            ? Expand(limit, large, small)
            : Expand(limit, small, large);
    }

    private static Interval Expand(Interval limit, Rational below, Rational above)
        => new (
            limit.Lower - below,
            limit.Upper + above,
            isLowerIncluded: true,
            isUpperIncluded: true);

    /// <summary>
    /// Adds a relative margin around the given interval.
    /// </summary>
    public static Interval ApplyRelativeMargin(Interval limit, double relativeMargin)
    {
        if (relativeMargin < 0)
            throw new ArgumentOutOfRangeException(
                nameof(relativeMargin),
                relativeMargin,
                "Relative axis margins cannot be negative.");

        if (relativeMargin == 0 || !limit.Lower.IsFinite || !limit.Upper.IsFinite)
            return limit;

        var length = limit.Upper - limit.Lower;
        var adjustment = length > 0
            ? length * relativeMargin
            : Rational.One;

        return new Interval(
            limit.Lower - adjustment,
            limit.Upper + adjustment,
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static Interval? GetFiniteLimit(Interval? limit)
    {
        return limit is { Lower.IsFinite: true, Upper.IsFinite: true } finiteLimit
            ? finiteLimit
            : null;
    }

    private static Interval GetDefaultSequenceXLimit(IEnumerable<Sequence> sequences)
    {
        var finiteXValues = sequences
            .SelectMany(s => new[] { s.DefinedFrom, s.DefinedUntil })
            .Where(x => x.IsFinite)
            .ToList();

        if (finiteXValues.Count == 0)
            throw new ArgumentException("Cannot compute x-axis limits: no finite x values were found.");

        return new Interval(
            finiteXValues.Min(),
            finiteXValues.Max(),
            isLowerIncluded: true,
            isUpperIncluded: true);
    }

    private static Interval GetDefaultSequenceYLimit(IEnumerable<Sequence> sequences)
    {
        var finiteYValues = sequences
            .SelectMany(s => s.Elements.GetElementsBoundaryValues())
            .Where(y => y.IsFinite)
            .ToList();

        if (finiteYValues.Count == 0)
            throw new ArgumentException("Cannot compute y-axis limits: no finite y values were found.");

        return new Interval(
            finiteYValues.Min(),
            finiteYValues.Max(),
            isLowerIncluded: true,
            isUpperIncluded: true);
    }
}
