using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class DeconvolutionNonNegative
{
    private static Curve Affine(Rational origin, Rational slope)
        => new(new Sequence([new Point(0, origin), new Segment(0, 1, origin, slope)]), 0, 1, slope);

    [Theory]
    [InlineData(-1, -2)]
    [InlineData(-2, -2)]
    [InlineData(3, 2)]
    public void OriginBoundAvoidsComputingTheDeconvolution(int leftOrigin, int rightOrigin)
    {
        var left = Affine(leftOrigin, 1);
        var right = Affine(rightOrigin, 2);
        var expression = left.ToExpression().Deconvolution(right.ToExpression());

        Assert.True(Curve.Deconvolution(left, right).IsNonNegative);
        Assert.True(expression.IsNonNegative);
        Assert.False(expression.IsComputed);
    }

    [Fact]
    public void TheRightOperandNeedNotBeNonDecreasing()
    {
        var left = Affine(-1, 1);
        var right = Affine(-2, -1);
        var expression = left.ToExpression().Deconvolution(right.ToExpression());

        Assert.False(right.IsNonDecreasing);
        Assert.True(Curve.Deconvolution(left, right).IsNonNegative);
        Assert.True(expression.IsNonNegative);
        Assert.False(expression.IsComputed);
    }

    [Theory]
    [InlineData(0, 1, 1, 2)]
    [InlineData(1, -1, 0, 0)]
    public void AnUnmetBoundUsesTheComputedCurve(int leftOrigin, int leftSlope, int rightOrigin, int rightSlope)
    {
        var left = Affine(leftOrigin, leftSlope);
        var right = Affine(rightOrigin, rightSlope);
        var expression = left.ToExpression().Deconvolution(right.ToExpression());

        Assert.False(Curve.Deconvolution(left, right).IsNonNegative);
        Assert.False(expression.IsNonNegative);
        Assert.True(expression.IsComputed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EqualInfiniteOriginsDoNotEstablishTheBound(bool positive)
    {
        var curve = positive ? Curve.PlusInfinite() : Curve.MinusInfinite();
        var expression = curve.ToExpression().Deconvolution(curve.ToExpression());
        var expected = Record.Exception(() => Curve.Deconvolution(curve, curve).IsNonNegative);
        var actual = Record.Exception(() => expression.IsNonNegative);

        Assert.NotNull(expected);
        Assert.NotNull(actual);
        Assert.Equal(expected.GetType(), actual.GetType());
    }
}
