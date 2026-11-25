using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class ScaledCurveShape
{
    public static IEnumerable<object[]> Factors()
    {
        foreach (var factor in new Rational[] { 0, 2, -2, Rational.PlusInfinity, Rational.MinusInfinity })
            yield return [factor];
    }

    [Theory]
    [MemberData(nameof(Factors))]
    public void KnownZeroIsConcaveAndConvexWithoutComputingTheScale(Rational factor)
    {
        var operand = Curve.Zero().ToExpression();
        Assert.True(operand.IsZero);
        Assert.True(operand.ZeroIfKnown);
        var concave = operand.Scale(factor);
        var convex = operand.Scale(factor);

        Assert.True(concave.IsConcave);
        Assert.True(convex.IsConvex);
        Assert.False(concave.IsComputed);
        Assert.False(convex.IsComputed);
    }

    [Fact]
    public void ZeroFactorLeavesAnUnknownOperandUnevaluated()
    {
        var operand = Expressions.Placeholder("f");
        Assert.Null(operand.ZeroIfKnown);
        var scaled = operand.Scale(0);

        Assert.True(scaled.IsConcave);
        Assert.True(scaled.IsConvex);
        Assert.Null(operand.ZeroIfKnown);
        Assert.False(operand.IsComputed);
        Assert.False(scaled.IsComputed);
    }

    [Fact]
    public void KnownNonzeroUsesTheInfiniteFactorFallback()
    {
        var operand = new Curve(new Sequence([new Point(0, 1), new Segment(0, 1, 1, 0)]), 0, 1, 0).ToExpression();
        Assert.False(operand.IsZero);
        Assert.False(operand.ZeroIfKnown);
        var concave = operand.Scale(Rational.PlusInfinity);
        var convex = operand.Scale(Rational.PlusInfinity);

        Assert.False(concave.IsConcave);
        Assert.False(convex.IsConvex);
        Assert.True(concave.IsComputed);
        Assert.True(convex.IsComputed);
    }
}
