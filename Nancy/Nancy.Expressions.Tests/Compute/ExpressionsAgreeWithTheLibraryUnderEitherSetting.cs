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

    [Fact]
    public void RationalHDevComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Expressions.HorizontalDeviation(Ae, Be, settings: Sequential).Compute(), Expressions.HorizontalDeviation(Ae, Be, settings: Parallel).Compute());
    }

    [Fact]
    public void RationalVDevComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Expressions.VerticalDeviation(Ae, Be, settings: Sequential).Compute(), Expressions.VerticalDeviation(Ae, Be, settings: Parallel).Compute());
    }

    [Fact]
    public void RationalZDevComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Expressions.ZDeviation(Ae, Be, settings: Sequential).Compute(), Expressions.ZDeviation(Ae, Be, settings: Parallel).Compute());
    }

    [Fact]
    public void RationalSequenceHDevComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Expressions .HorizontalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: Sequential).Compute(), Expressions .HorizontalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: Parallel).Compute());
    }

    [Fact]
    public void RationalSequenceVDevComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Expressions .VerticalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: Sequential).Compute(), Expressions .VerticalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: Parallel).Compute());
    }

    [Fact]
    public void RationalValueAtComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Ae.ValueAt(Expressions.FromRational(2, "t"), settings: Sequential).Compute(), Ae.ValueAt(Expressions.FromRational(2, "t"), settings: Parallel).Compute());
    }

    [Fact]
    public void RationalSupValueComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Se.SupValue(settings: Sequential).Compute(), Se.SupValue(settings: Parallel).Compute());
    }

    [Fact]
    public void RationalInfValueComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Se.InfValue(settings: Sequential).Compute(), Se.InfValue(settings: Parallel).Compute());
    }

    [Fact]
    public void RationalMaxValueComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Ce.MaxValue(settings: Sequential).Compute(), Ce.MaxValue(settings: Parallel).Compute());
    }

    [Fact]
    public void RationalMinValueComputesTheSameUnderEitherSetting()
    {
        Assert.Equal(Ce.MinValue(settings: Sequential).Compute(), Ce.MinValue(settings: Parallel).Compute());
    }

    [Fact]
    public void CurveUpperNonDecreasingComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.ToUpperNonDecreasing(settings: Sequential).Compute().Equivalent(Se.ToUpperNonDecreasing(settings: Parallel).Compute()));
    }

    [Fact]
    public void CurveLowerNonDecreasingComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.ToLowerNonDecreasing(settings: Sequential).Compute().Equivalent(Se.ToLowerNonDecreasing(settings: Parallel).Compute()));
    }

    [Fact]
    public void CurveUpperNonIncreasingComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.ToUpperNonIncreasing(settings: Sequential).Compute().Equivalent(Se.ToUpperNonIncreasing(settings: Parallel).Compute()));
    }

    [Fact]
    public void CurveLowerNonIncreasingComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.ToLowerNonIncreasing(settings: Sequential).Compute().Equivalent(Se.ToLowerNonIncreasing(settings: Parallel).Compute()));
    }

    [Fact]
    public void CurveSubtractionComputesTheSameUnderEitherSetting()
    {
        Assert.True(Ae.Subtraction(Be, settings: Sequential).Compute().Equivalent(Ae.Subtraction(Be, settings: Parallel).Compute()));
    }

    [Fact]
    public void SequenceCutComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.Cut(Window, settings: Sequential).Compute().Equivalent(Se.Cut(Window, settings: Parallel).Compute()));
    }

    [Fact]
    public void SequenceCutToNeighbourhoodComputesTheSameUnderEitherSetting()
    {
        Assert.True(Se.CutToNeighbourhood(1, 4, settings: Sequential).Compute().Equivalent(Se.CutToNeighbourhood(1, 4, settings: Parallel).Compute()));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HorizontalDeviationAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Curve.HorizontalDeviation(A, B, settings);
        var actual = Expressions.HorizontalDeviation(Ae, Be, settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VerticalDeviationAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Curve.VerticalDeviation(A, B, settings);
        var actual = Expressions.VerticalDeviation(Ae, Be, settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZDeviationAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Curve.ZDeviation(A, B, settings);
        var actual = Expressions.ZDeviation(Ae, Be, settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SequenceHorizontalDeviationAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Sequence.HorizontalDeviation(SequenceA, SequenceB, settings);
        var actual = Expressions.HorizontalDeviation(SequenceA.ToExpression("a"), SequenceB.ToExpression("b"), settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValueAtAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = A.ValueAt(2, settings);
        var actual = Ae.ValueAt(Expressions.FromRational(2, "t"), settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SupValueAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Staircase.SupValue(settings);
        var actual = Se.SupValue(settings: s).Compute();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UpperNonDecreasingAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Staircase.ToUpperNonDecreasing(settings);
        var actual = Se.ToUpperNonDecreasing(settings: s).Compute();

        Assert.True(expected.Equivalent(actual));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CutAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Staircase.Cut(Window, settings);
        var actual = Se.Cut(Window, settings: s).Compute();

        Assert.True(expected.Equivalent(actual));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CutToNeighbourhoodAgreesWithNancy(bool useParallelism)
    {
        var settings = new ComputationSettings { UseParallelism = useParallelism };
        var s = new ExpressionSettings { ComputationSettings = settings };
        var expected = Staircase.CutToNeighbourhood(1, 4, settings: settings);
        var actual = Se.CutToNeighbourhood(1, 4, settings: s).Compute();

        Assert.True(expected.Equivalent(actual));
    }

}
