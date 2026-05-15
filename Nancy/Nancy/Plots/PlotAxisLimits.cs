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
public readonly record struct PlotAxisLimits(Interval XLimit, Interval YLimit);

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
    /// Computes the default axis limits for already-sampled sequences.
    /// Explicit finite limits in <paramref name="settings"/> take precedence.
    /// </summary>
    public static PlotAxisLimits GetSequenceAxisLimits(
        IReadOnlyCollection<Sequence> sequences,
        PlotSettings settings)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("Empty sequence collection.", nameof(sequences));

        var xLimit = GetFiniteLimit(settings.XLimit) ?? ApplyRelativeMargin(
            GetDefaultSequenceXLimit(sequences),
            settings.RelativeXAxisMargin);
        var yLimit = GetFiniteLimit(settings.YLimit) ?? ApplyRelativeMargin(
            GetDefaultSequenceYLimit(sequences),
            settings.RelativeYAxisMargin);

        return new PlotAxisLimits(xLimit, yLimit);
    }

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
