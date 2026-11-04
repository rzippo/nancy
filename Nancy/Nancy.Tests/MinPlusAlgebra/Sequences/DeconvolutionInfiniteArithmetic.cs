using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class DeconvolutionInfiniteArithmetic
{
    public static IEnumerable<ComputationSettings> Settings =>
    [
        new() { UseParallelism = false },
        new() { UseParallelism = false, UseParallelDeconvolution = true, DeconvolutionParallelizationThreshold = 0 }
    ];

    public static IEnumerable<(Element A, Element B, ComputationSettings Settings)> UndefinedPairs()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var a in new Element[] { new Point(0, value), Segment.Constant(0, 2, value) })
        foreach (var b in new Element[] { new Point(0, value), Segment.Constant(0, 2, value) })
        foreach (var settings in Settings)
            yield return (a, b, settings);
    }

    public static IEnumerable<object[]> UndefinedCases() => UndefinedPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UndefinedCases))]
    public void MinPlusRejectsUndefinedDifferences(Element a, Element b, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => new Sequence([a]).Deconvolution(new Sequence([b]), settings: settings));

    [Theory]
    [MemberData(nameof(UndefinedCases))]
    public void MaxPlusRejectsUndefinedDifferences(Element a, Element b, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => new Sequence([a]).MaxPlusDeconvolution(new Sequence([b]), settings));

    public static IEnumerable<(Sequence Input, ComputationSettings Settings)> MixedInputs()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var settings in Settings)
            yield return (new Sequence([Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Constant(1, 2, value), new Point(2, value)]), settings);
    }

    public static IEnumerable<object[]> MixedCases() => MixedInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MixedCases))]
    public void MinPlusRejectsUndefinedTermsBesideFiniteCandidates(Sequence input, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => input.Deconvolution(input, settings: settings));

    [Theory]
    [MemberData(nameof(MixedCases))]
    public void MaxPlusRejectsUndefinedTermsBesideFiniteCandidates(Sequence input, ComputationSettings settings)
        => Assert.Throws<UndeterminedResultException>(() => input.MaxPlusDeconvolution(input, settings));

    public static IEnumerable<(Sequence Input, ComputationSettings Settings)> IdentityInputs()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var input in new[]
        {
            new Sequence([new Point(0, value)]),
            new Sequence([Segment.Constant(0, 2, value)]),
            new Sequence([Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Constant(1, 2, value)]),
            new Sequence([new Point(0, value), Segment.Constant(0, 1, value), Point.Zero(1), Segment.Zero(1, 2)]),
            new Sequence([Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Constant(1, 2, value), Point.Zero(2), Segment.Zero(2, 3)]),
            new Sequence([Point.Origin(), Segment.Zero(0, 1), new Point(1, value), Segment.Zero(1, 2), Point.Zero(2)])
        })
        foreach (var settings in Settings)
            yield return (input, settings);
    }

    public static IEnumerable<object[]> IdentityCases() => IdentityInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(IdentityCases))]
    public void MinPlusPointIdentityPreservesInfiniteRegions(Sequence input, ComputationSettings settings)
    {
        var result = input.Deconvolution(new Sequence([Point.Origin()]), settings: settings);
        Assert.True(Sequence.Equivalent(input, result));
        Assert.Equal(input.IsLeftClosed, result.IsLeftClosed);
        Assert.Equal(input.IsRightClosed, result.IsRightClosed);
    }

    [Theory]
    [MemberData(nameof(IdentityCases))]
    public void MaxPlusPointIdentityPreservesInfiniteRegions(Sequence input, ComputationSettings settings)
    {
        var result = input.MaxPlusDeconvolution(new Sequence([Point.Origin()]), settings);
        Assert.True(Sequence.Equivalent(input, result));
        Assert.Equal(input.IsLeftClosed, result.IsLeftClosed);
        Assert.Equal(input.IsRightClosed, result.IsRightClosed);
    }

    public static IEnumerable<object[]> SettingsCases() => Settings.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void APointPairAtTheExcludedCutEndDoesNotInvalidateTheResult(ComputationSettings settings)
    {
        var a = new Sequence([Point.Origin(), Segment.Zero(0, 1), Point.PlusInfinite(1)]);
        var b = new Sequence([Point.PlusInfinite(0), Segment.Zero(0, 1), Point.Zero(1)]);
        var result = a.Deconvolution(b, 0, 1, settings);
        Assert.True(result.ValueAt(0).IsPlusInfinite);
        Assert.True(result.ValueAt(new Rational(1, 2)).IsPlusInfinite);
        Assert.False(result.IsDefinedAt(1));
        Assert.Throws<UndeterminedResultException>(() => a.Deconvolution(b, 1, 2, settings));
    }

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void APointPairBeforeTheCutDoesNotInvalidateTheResult(ComputationSettings settings)
    {
        var a = new Sequence([Point.PlusInfinite(0), Segment.Zero(0, 2), Point.Zero(2)]);
        var b = new Sequence([Point.PlusInfinite(0), Segment.Zero(0, 1), Point.Zero(1)]);
        var result = a.Deconvolution(b, 1, 2, settings);
        Assert.Equal(0, result.ValueAt(1));
        Assert.Equal(0, result.ValueAt(new Rational(3, 2)));
        Assert.Throws<UndeterminedResultException>(() => a.Deconvolution(b, 0, 1, settings));
    }

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void AnOpenPairEndAtTheCutStartDoesNotInvalidateTheResult(ComputationSettings settings)
    {
        var a = new Sequence([Point.Origin(), Segment.PlusInfinite(0, 1), Point.Zero(1), Segment.Zero(1, 2)]);
        var b = new Sequence([Point.PlusInfinite(0), Segment.Zero(0, 2)]);
        var result = a.Deconvolution(b, 1, 2, settings);
        Assert.Equal(0, result.ValueAt(1));
        Assert.Equal(0, result.ValueAt(new Rational(3, 2)));
    }

    [Theory]
    [MemberData(nameof(SettingsCases))]
    public void AWindowAfterAllPairsIsFilledWithMinusInfinity(ComputationSettings settings)
    {
        var input = new Sequence([Point.Origin(), Segment.Zero(0, 1)]);
        var result = input.Deconvolution(input, 2, 3, settings);
        Assert.True(result.IsMinusInfinite);
        Assert.Equal(2, result.DefinedFrom);
        Assert.Equal(3, result.DefinedUntil);
    }
}
