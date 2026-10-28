using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks horizontal deviation from pseudo-inverse streams against the other strategies for operands with different periods.
/// </summary>
public class HorizontalDeviation
{
    /// <summary>
    /// Pairs whose lower pseudo-inverses have periods that are not multiples of each other: the heights of a period of the staircases, 7 and 13, and 4 and 6.
    /// </summary>
    public static List<(string Name, Curve Left, Curve Right)> IncommensurablePeriodPairs() =>
    [
        ("7 per 3 against 13 per 5", new StairCurve(7, 3), new StairCurve(13, 5)),
        ("7 per 3 against 13 per 5, delayed", new StairCurve(7, 3), new StairCurve(13, 5).DelayBy(4)),
        ("4 per 3 against 6 per 4", new StairCurve(4, 3), new StairCurve(6, 4)),
        ("equal rates, 7 per 3 against 14 per 6", new StairCurve(7, 3), new StairCurve(14, 6).DelayBy(1)),
        ("g slower than f, 13 per 5 against 7 per 3", new StairCurve(13, 5), new StairCurve(7, 3)),
        ("rate-latency against a staircase", new RateLatencyServiceCurve(2, 1), new StairCurve(7, 3)),
        ("a staircase against a rate-latency curve", new StairCurve(7, 3), new RateLatencyServiceCurve(5, 1)),
    ];

    public static IEnumerable<object[]> PairsWithIncommensurablePeriods() => IncommensurablePeriodPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PairsWithIncommensurablePeriods))]
    public void TheDeviationOfTheLowerGapEqualsTheOtherStrategies(string name, Curve f, Curve g)
    {
        Assert.False(string.IsNullOrEmpty(name));
        var gap = Curve.HorizontalDeviation(f, g, new ComputationSettings { HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap });

        foreach (var strategy in new[]
                 {
                     HorizontalDeviationStrategy.PseudoInverseDeconvolution,
                     HorizontalDeviationStrategy.Composition,
                     HorizontalDeviationStrategy.CompositionDeconvolution
                 })
        {
            var other = Curve.HorizontalDeviation(f, g, new ComputationSettings { HorizontalDeviationStrategy = strategy });
            Assert.True(gap == other, $"{name}: {gap} under PseudoInverseGap, {other} under {strategy}");
        }
    }

    [Theory]
    [MemberData(nameof(PairsWithIncommensurablePeriods))]
    public void TheDeviationOfTheLowerGapEqualsTheSupremumOfTheDifferenceOfThePseudoInverses(string name, Curve f, Curve g)
    {
        Assert.False(string.IsNullOrEmpty(name));
        var expected = Curve.Subtraction(g.LowerPseudoInverse(), f.LowerPseudoInverse())
            .SupValue();
        var actual = Curve.HorizontalDeviation(f, g, new ComputationSettings { HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap });
        Assert.True(expected == actual, $"{name}: {actual}, not {expected}");
    }
}
