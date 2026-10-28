using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves.Streams;

/// <summary>
/// Checks the lower and upper pseudo-inverse streams against cuts of their curve representations.
/// </summary>
public class PseudoInverses
{
    public static IEnumerable<Curve> PseudoInverseCases()
    {
        Curve[] curves =
        [
            new StairCurve(7, 3), new RateLatencyServiceCurve(3, 5),
            Curve.Minimum(new RateLatencyServiceCurve(5, 2), new ConstantCurve(10)),
            new StairCurve(7, 3).DelayBy(2), new ConstantCurve(3),
            new RateLatencyServiceCurve(1, 0).VerticalShift(-2, exceptOrigin: false),
            Curve.PlusInfinite(), Curve.MinusInfinite()
        ];
        foreach (var curve in curves)
            yield return curve;
    }

    public static IEnumerable<object[]> Cases() => PseudoInverseCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void LowerPseudoInverseStreamsEqualMaterializedCuts(Curve curve)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var inverse = curve.LowerPseudoInverse();
        foreach (Rational end in new[] { 0, 1, 7, 25, 100 })
        {
            var expected = inverse.Cut(0, end, true, true, settings);
            var actual = (curve.LowerPseudoInverseAsEnumerable(end, settings))
                .ToList();
            Assert.True(actual.AreUninterruptedSequence());
            Assert.Equal(0, actual.First().StartTime);
            Assert.Equal(end, actual.Last().EndTime);
            Assert.True(Sequence.Equivalent(expected, actual.ToSequence()));
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UpperPseudoInverseStreamsEqualMaterializedCuts(Curve curve)
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var inverse = curve.UpperPseudoInverse();
        foreach (Rational end in new[] { 0, 1, 7, 25, 100 })
        {
            var expected = inverse.Cut(0, end, true, true, settings);
            var actual = (curve.UpperPseudoInverseAsEnumerable(end, settings))
                .ToList();
            Assert.True(actual.AreUninterruptedSequence());
            Assert.Equal(0, actual.First().StartTime);
            Assert.Equal(end, actual.Last().EndTime);
            Assert.True(Sequence.Equivalent(expected, actual.ToSequence()));
        }
    }

    [Fact]
    public void LowerPseudoInverseRejectsDecreasingCurves()
    {
        var decreasing = new Curve(
            new Sequence([new Point(0, 2), new Segment(0, 1, 2, -1), new Point(1, 1)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: -1);
        Assert.Throws<ArgumentException>(() => decreasing.LowerPseudoInverseAsEnumerable(5));
    }

    [Fact]
    public void UpperPseudoInverseRejectsDecreasingCurves()
    {
        var decreasing = new Curve(
            new Sequence([new Point(0, 2), new Segment(0, 1, 2, -1), new Point(1, 1)]),
            pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: -1);
        Assert.Throws<ArgumentException>(() => decreasing.UpperPseudoInverseAsEnumerable(5));
    }

    [Fact]
    public void LowerPseudoInverseProducesAFinitePrefixOfALargeExtension()
    {
        var curve = new StairCurve(7, 3);
        var stream = curve.LowerPseudoInverseAsEnumerable(1_000_000_000);
        Assert.Equal(3, stream.Take(3)
            .Count());
    }

    [Fact]
    public void UpperPseudoInverseProducesAFinitePrefixOfALargeExtension()
    {
        var curve = new StairCurve(7, 3);
        var stream = curve.UpperPseudoInverseAsEnumerable(1_000_000_000);
        Assert.Equal(3, stream.Take(3)
            .Count());
    }
}
