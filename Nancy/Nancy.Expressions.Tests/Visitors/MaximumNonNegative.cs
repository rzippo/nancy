using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class MaximumNonNegative
{
    private static Curve Constant(int value)
        => new(new Sequence([new Point(0, value), new Segment(0, 1, value, 0)]), 0, 1, 0);

    private static Curve Alternating(int first, int second)
        => new(new Sequence([
            new Point(0, first), new Segment(0, 1, first, 0),
            new Point(1, second), new Segment(1, 2, second, 0)
        ]), 0, 2, 0);

    public static IEnumerable<object[]> NegativeOperands()
    {
        yield return [new[] { Constant(-1), Constant(-2) }];
        yield return [new[] { Curve.MinusInfinite(), Constant(-1) }];
        yield return [new[] { Constant(-2), Constant(-1), Curve.MinusInfinite() }];
    }

    [Theory]
    [MemberData(nameof(NegativeOperands))]
    public void StrictlyNegativeOperandsAvoidComputingTheMaximum(Curve[] operands)
    {
        Assert.All(operands, operand => Assert.True(operand.IsNegative));
        var expression = operands[0].ToExpression().Maximum(operands[1].ToExpression());
        for (var i = 2; i < operands.Length; i++)
            expression = expression.Maximum(operands[i].ToExpression());

        Assert.False(Curve.Maximum(operands).IsNonNegative);
        Assert.False(expression.IsNonNegative);
        Assert.False(expression.IsComputed);
    }

    [Theory]
    [InlineData(-1, 0, 0, -1, true)]
    [InlineData(-2, 0, -1, 0, false)]
    public void NonPositiveOperandsThatReachZeroUseTheFallback(
        int leftFirst, int leftSecond, int rightFirst, int rightSecond, bool expected)
    {
        var left = Alternating(leftFirst, leftSecond);
        var right = Alternating(rightFirst, rightSecond);
        Assert.True(left.IsNonPositive);
        Assert.True(right.IsNonPositive);
        Assert.False(left.IsNonNegative);
        Assert.False(right.IsNonNegative);
        Assert.False(left.IsNegative);
        Assert.False(right.IsNegative);
        var expression = left.ToExpression().Maximum(right.ToExpression());

        Assert.Equal(expected, Curve.Maximum(left, right).IsNonNegative);
        Assert.Equal(expected, expression.IsNonNegative);
        Assert.True(expression.IsComputed);
    }

    [Fact]
    public void ANonNegativeOperandDoesNotInspectLaterOperands()
    {
        var unknown = Expressions.Placeholder("g");
        var expression = Curve.Zero().ToExpression().Maximum(unknown);

        Assert.True(expression.IsNonNegative);
        Assert.False(expression.IsComputed);
        Assert.False(unknown.IsComputed);
        Assert.Null(unknown.NegativeIfKnown);
    }
}
