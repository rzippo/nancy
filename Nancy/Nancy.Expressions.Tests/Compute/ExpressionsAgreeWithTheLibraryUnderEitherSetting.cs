using System;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// An expression computes the same value under either parallelism setting, and the same value as the call it stands for made directly on the library.
/// The settings change how the work is done and not its result, so these checks hold whether or not the settings reach the library.
/// </summary>
public class ExpressionsAgreeWithTheLibraryUnderEitherSetting
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(4, 3);
    private static readonly Curve B = new RateLatencyServiceCurve(4, 3);
    private static readonly Curve Staircase = new StairCurve(3, 5);
    private static readonly Curve Constant = new ConstantCurve(5);
    private static readonly Sequence SequenceA =
        new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SequenceB =
        new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);
    private static readonly Interval Window = new(1, 4, true, true);

    private static ExpressionSettings Sequential =>
        new() { ComputationSettings = new ComputationSettings { UseParallelism = false } };

    private static ExpressionSettings Parallel =>
        new() { ComputationSettings = new ComputationSettings { UseParallelism = true } };

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression Se => Expressions.FromCurve(Staircase, "s");
    private static CurveExpression Ce => Expressions.FromCurve(Constant, "c");

    public static TheoryData<string> RationalCases => new(
        "hDev", "vDev", "zDev", "sequence hDev", "sequence vDev",
        "value at", "sup value", "inf value", "max value", "min value");

    private static Rational ComputeRational(string which, ExpressionSettings s)
        => which switch
        {
            "hDev" => Expressions.HorizontalDeviation(Ae, Be, settings: s).Compute(),
            "vDev" => Expressions.VerticalDeviation(Ae, Be, settings: s).Compute(),
            "zDev" => Expressions.ZDeviation(Ae, Be, settings: s).Compute(),
            "sequence hDev" => Expressions
                .HorizontalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: s).Compute(),
            "sequence vDev" => Expressions
                .VerticalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: s).Compute(),
            "value at" => Ae.ValueAt(Expressions.FromRational(2, "t"), settings: s).Compute(),
            "sup value" => Se.SupValue(settings: s).Compute(),
            "inf value" => Se.InfValue(settings: s).Compute(),
            "max value" => Ce.MaxValue(settings: s).Compute(),
            "min value" => Ce.MinValue(settings: s).Compute(),
            _ => throw new ArgumentOutOfRangeException(nameof(which), which, "Unknown case.")
        };

    [Theory]
    [MemberData(nameof(RationalCases))]
    public void ARationalExpressionComputesTheSameUnderEitherSetting(string which)
        => Assert.Equal(ComputeRational(which, Sequential), ComputeRational(which, Parallel));

    public static TheoryData<string> CurveCases => new(
        "upper non-decreasing", "lower non-decreasing", "upper non-increasing", "lower non-increasing",
        "subtraction");

    private static Curve ComputeCurve(string which, ExpressionSettings s)
        => which switch
        {
            "upper non-decreasing" => Se.ToUpperNonDecreasing(settings: s).Compute(),
            "lower non-decreasing" => Se.ToLowerNonDecreasing(settings: s).Compute(),
            "upper non-increasing" => Se.ToUpperNonIncreasing(settings: s).Compute(),
            "lower non-increasing" => Se.ToLowerNonIncreasing(settings: s).Compute(),
            "subtraction" => Ae.Subtraction(Be, settings: s).Compute(),
            _ => throw new ArgumentOutOfRangeException(nameof(which), which, "Unknown case.")
        };

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACurveExpressionComputesTheSameUnderEitherSetting(string which)
        => Assert.True(ComputeCurve(which, Sequential).Equivalent(ComputeCurve(which, Parallel)));

    public static TheoryData<string> SequenceCases => new("cut", "cut to neighbourhood");

    private static Sequence ComputeSequence(string which, ExpressionSettings s)
        => which switch
        {
            "cut" => Se.Cut(Window, settings: s).Compute(),
            "cut to neighbourhood" => Se.CutToNeighbourhood(1, 4, settings: s).Compute(),
            _ => throw new ArgumentOutOfRangeException(nameof(which), which, "Unknown case.")
        };

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void ASequenceExpressionComputesTheSameUnderEitherSetting(string which)
        => Assert.True(ComputeSequence(which, Sequential).Equivalent(ComputeSequence(which, Parallel)));

    /// <summary>
    /// The value an expression computes is the value the same call on the library gives.
    /// </summary>
    [Fact]
    public void AnExpressionAgreesWithTheLibraryCallItStandsFor()
    {
        var settings = new ComputationSettings { UseParallelism = false };
        var s = new ExpressionSettings { ComputationSettings = settings };

        Assert.Equal(
            Curve.HorizontalDeviation(A, B, settings),
            Expressions.HorizontalDeviation(Ae, Be, settings: s).Compute());
        Assert.Equal(
            Curve.VerticalDeviation(A, B, settings),
            Expressions.VerticalDeviation(Ae, Be, settings: s).Compute());
        Assert.Equal(
            Curve.ZDeviation(A, B, settings),
            Expressions.ZDeviation(Ae, Be, settings: s).Compute());
        Assert.Equal(
            Sequence.HorizontalDeviation(SequenceA, SequenceB, settings),
            Expressions.HorizontalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: s)
                .Compute());
        Assert.Equal(
            A.ValueAt(2, settings),
            Ae.ValueAt(Expressions.FromRational(2, "t"), settings: s).Compute());
        Assert.Equal(
            Staircase.SupValue(settings),
            Se.SupValue(settings: s).Compute());
        Assert.True(
            Staircase.ToUpperNonDecreasing(settings)
                .Equivalent(Se.ToUpperNonDecreasing(settings: s).Compute()));
        Assert.True(
            Staircase.Cut(Window, settings)
                .Equivalent(Se.Cut(Window, settings: s).Compute()));
        Assert.True(
            Staircase.CutToNeighbourhood(1, 4, settings: settings)
                .Equivalent(Se.CutToNeighbourhood(1, 4, settings: s).Compute()));
    }
}
