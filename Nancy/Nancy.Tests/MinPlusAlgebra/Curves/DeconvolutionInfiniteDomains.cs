using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class DeconvolutionInfiniteDomains
{
    private static Curve EndingInfinity(Rational value, bool endIncluded) => new(new Sequence([
        new Point(0, value), Segment.Constant(0, 1, value), new Point(1, endIncluded ? value : 0),
        Segment.Zero(1, 2), Point.Zero(2), Segment.Zero(2, 3)
    ]), 2, 1, 0);

    private static Curve StartingInfinity(Rational value, bool startIncluded) => new(new Sequence([
        Point.Origin(), Segment.Zero(0, 1), new Point(1, startIncluded ? value : 0),
        Segment.Constant(1, 2, value), new Point(2, value), Segment.Constant(2, 3, value)
    ]), 2, 1, 0);

    public static IEnumerable<(Curve A, Curve B, Curve MinExpected, Curve MaxExpected, ComputationSettings Settings)> SeparatedPairs()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var endIncluded in new[] { false, true })
        foreach (var startIncluded in new[] { false, true })
        {
            if (endIncluded && startIncluded)
                continue;
            foreach (var settings in Sequences.DeconvolutionInfiniteArithmetic.Settings)
                yield return (EndingInfinity(value, endIncluded), StartingInfinity(value, startIncluded),
                    value.IsPlusInfinite ? EndingInfinity(value, endIncluded) : Curve.PlusInfinite(),
                    value.IsMinusInfinite ? EndingInfinity(value, endIncluded) : Curve.MinusInfinite(), settings);
        }
    }

    public static IEnumerable<object[]> MinSeparatedCases() => SeparatedPairs()
        .Select(p => (p.A, p.B, p.MinExpected, p.Settings))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> MaxSeparatedCases() => SeparatedPairs()
        .Select(p => (p.A, p.B, p.MaxExpected, p.Settings))
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MinSeparatedCases))]
    public void MinPlusAllowsSameInfinitiesThatCannotMeet(Curve a, Curve b, Curve expected, ComputationSettings settings)
    {
        var result = a.Deconvolution(b, settings);
        Assert.True(Curve.Equivalent(expected, result));
    }

    [Theory]
    [MemberData(nameof(MaxSeparatedCases))]
    public void MaxPlusAllowsSameInfinitiesThatCannotMeet(Curve a, Curve b, Curve expected, ComputationSettings settings)
    {
        var result = a.MaxPlusDeconvolution(b, settings);
        Assert.True(Curve.Equivalent(expected, result));
    }

    public static IEnumerable<(Curve A, Curve B, ComputationSettings Settings)> UndefinedPairs()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var settings in Sequences.DeconvolutionInfiniteArithmetic.Settings)
        {
            yield return (EndingInfinity(value, true), StartingInfinity(value, true), settings);
            var recurring = new Curve(new Sequence([
                Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Zero(1, 2)
            ]), 1, 1, 0);
            var late = new Curve(new Sequence([
                Point.Origin(), Segment.Zero(0, 10), new Point(10, value), Segment.Constant(10, 11, value)
            ]), 10, 1, 0);
            yield return (recurring, late, settings);
            var infiniteTail = new Curve(new Sequence([
                Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Constant(1, 2, value)
            ]), 1, 1, 0);
            yield return (infiniteTail, late, settings);
        }
    }

    public static IEnumerable<object[]> UndefinedCases() => UndefinedPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UndefinedCases))]
    public void MinPlusRejectsMeetingOrRecurringInfinities(Curve a, Curve b, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => a.Deconvolution(b, settings));

    [Theory]
    [MemberData(nameof(UndefinedCases))]
    public void MaxPlusRejectsMeetingOrRecurringInfinities(Curve a, Curve b, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => a.MaxPlusDeconvolution(b, settings));

    public static IEnumerable<object[]> SettingsCases() => Sequences.DeconvolutionInfiniteArithmetic.Settings.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void MinPlusZeroDelayIdentityPreservesAMinusInfinitePrefix(ComputationSettings settings)
    {
        var input = EndingInfinity(Rational.MinusInfinity, false);
        var result = input.Deconvolution(new DelayServiceCurve(0), settings);
        Assert.True(Curve.Equivalent(input, result));
    }

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void MaxPlusZeroDelayIdentityPreservesAPlusInfinitePrefix(ComputationSettings settings)
    {
        var input = EndingInfinity(Rational.PlusInfinity, false);
        var result = input.MaxPlusDeconvolution(-new DelayServiceCurve(0), settings);
        Assert.True(Curve.Equivalent(input, result));
    }
}
