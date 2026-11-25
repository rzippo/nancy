using System.Linq;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class SymmetricExtremumSigns
{
    private static Curve Constant(Rational value)
        => value.IsMinusInfinite ? Curve.MinusInfinite()
            : value.IsPlusInfinite ? Curve.PlusInfinite()
            : new(new Sequence([new Point(0, value), new Segment(0, 1, value, 0)]), 0, 1, 0);

    private static Curve Alternating(int first, int second)
        => new(new Sequence([
            new Point(0, first), new Segment(0, 1, first, 0),
            new Point(1, second), new Segment(1, 2, second, 0)
        ]), 0, 2, 0);

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NonNegativeMinimumOperands()
    {
        yield return ([Constant(0), Constant(Rational.PlusInfinity)], true, false);
        yield return ([Constant(0), Constant(-1)], false, false);
    }

    public static IEnumerable<object[]> NonNegativeMinimumCases()
        => NonNegativeMinimumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NonNegativeMinimumCases))]
    public void NonNegativeMinimumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Minimum(operand.ToExpression());
        Assert.Equal(expected, Curve.Minimum(operands).IsNonNegative);
        Assert.Equal(expected, expression.IsNonNegative);
        Assert.Equal(computed, expression.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NonNegativeMaximumOperands()
    {
        yield return ([Constant(-1), Constant(0)], true, false);
        yield return ([Constant(-1), Constant(-1), Constant(-Rational.PlusInfinity)], false, false);
        yield return ([Alternating(0, -1), Alternating(-1, 0)], true, true);
        yield return ([Alternating(0, -1), Alternating(0, -2)], false, true);
    }

    public static IEnumerable<object[]> NonNegativeMaximumCases()
        => NonNegativeMaximumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NonNegativeMaximumCases))]
    public void NonNegativeMaximumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Maximum(operand.ToExpression());
        Assert.Equal(expected, Curve.Maximum(operands).IsNonNegative);
        Assert.Equal(expected, expression.IsNonNegative);
        Assert.Equal(computed, expression.IsComputed);
    }

    [Fact]
    public void NonNegativeMaximumLeavesLaterOperandsUnevaluatedAfterASufficientOperand()
    {
        var unknown = Expressions.Placeholder("g");
        var expression = Constant(0).ToExpression().Maximum(unknown);
        Assert.True(expression.IsNonNegative);
        Assert.False(expression.IsComputed);
        Assert.False(unknown.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NonPositiveMinimumOperands()
    {
        yield return ([Constant(1), Constant(0)], true, false);
        yield return ([Constant(1), Constant(1), Constant(-Rational.MinusInfinity)], false, false);
        yield return ([Alternating(0, 1), Alternating(1, 0)], true, true);
        yield return ([Alternating(0, 1), Alternating(0, 2)], false, true);
    }

    public static IEnumerable<object[]> NonPositiveMinimumCases()
        => NonPositiveMinimumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NonPositiveMinimumCases))]
    public void NonPositiveMinimumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Minimum(operand.ToExpression());
        Assert.Equal(expected, Curve.Minimum(operands).IsNonPositive);
        Assert.Equal(expected, expression.IsNonPositive);
        Assert.Equal(computed, expression.IsComputed);
    }

    [Fact]
    public void NonPositiveMinimumLeavesLaterOperandsUnevaluatedAfterASufficientOperand()
    {
        var unknown = Expressions.Placeholder("g");
        var expression = Constant(0).ToExpression().Minimum(unknown);
        Assert.True(expression.IsNonPositive);
        Assert.False(expression.IsComputed);
        Assert.False(unknown.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NonPositiveMaximumOperands()
    {
        yield return ([Constant(0), Constant(Rational.MinusInfinity)], true, false);
        yield return ([Constant(0), Constant(1)], false, false);
    }

    public static IEnumerable<object[]> NonPositiveMaximumCases()
        => NonPositiveMaximumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NonPositiveMaximumCases))]
    public void NonPositiveMaximumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Maximum(operand.ToExpression());
        Assert.Equal(expected, Curve.Maximum(operands).IsNonPositive);
        Assert.Equal(expected, expression.IsNonPositive);
        Assert.Equal(computed, expression.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> PositiveMinimumOperands()
    {
        yield return ([Constant(1), Constant(Rational.PlusInfinity)], true, false);
        yield return ([Constant(1), Constant(0)], false, false);
    }

    public static IEnumerable<object[]> PositiveMinimumCases()
        => PositiveMinimumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(PositiveMinimumCases))]
    public void PositiveMinimumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Minimum(operand.ToExpression());
        Assert.Equal(expected, Curve.Minimum(operands).IsPositive);
        Assert.Equal(expected, expression.IsPositive);
        Assert.Equal(computed, expression.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> PositiveMaximumOperands()
    {
        yield return ([Constant(0), Constant(1)], true, false);
        yield return ([Constant(0), Constant(0), Constant(-Rational.PlusInfinity)], false, false);
        yield return ([Alternating(0, 1), Alternating(1, 0)], true, true);
        yield return ([Alternating(0, 1), Alternating(0, 2)], false, true);
    }

    public static IEnumerable<object[]> PositiveMaximumCases()
        => PositiveMaximumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(PositiveMaximumCases))]
    public void PositiveMaximumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Maximum(operand.ToExpression());
        Assert.Equal(expected, Curve.Maximum(operands).IsPositive);
        Assert.Equal(expected, expression.IsPositive);
        Assert.Equal(computed, expression.IsComputed);
    }

    [Fact]
    public void PositiveMaximumLeavesLaterOperandsUnevaluatedAfterASufficientOperand()
    {
        var unknown = Expressions.Placeholder("g");
        var expression = Constant(1).ToExpression().Maximum(unknown);
        Assert.True(expression.IsPositive);
        Assert.False(expression.IsComputed);
        Assert.False(unknown.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NegativeMinimumOperands()
    {
        yield return ([Constant(0), Constant(-1)], true, false);
        yield return ([Constant(0), Constant(0), Constant(-Rational.MinusInfinity)], false, false);
        yield return ([Alternating(0, -1), Alternating(-1, 0)], true, true);
        yield return ([Alternating(0, -1), Alternating(0, -2)], false, true);
    }

    public static IEnumerable<object[]> NegativeMinimumCases()
        => NegativeMinimumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NegativeMinimumCases))]
    public void NegativeMinimumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Minimum(operand.ToExpression());
        Assert.Equal(expected, Curve.Minimum(operands).IsNegative);
        Assert.Equal(expected, expression.IsNegative);
        Assert.Equal(computed, expression.IsComputed);
    }

    [Fact]
    public void NegativeMinimumLeavesLaterOperandsUnevaluatedAfterASufficientOperand()
    {
        var unknown = Expressions.Placeholder("g");
        var expression = Constant(-1).ToExpression().Minimum(unknown);
        Assert.True(expression.IsNegative);
        Assert.False(expression.IsComputed);
        Assert.False(unknown.IsComputed);
    }

    private static IEnumerable<(Curve[] Operands, bool Expected, bool Computed)> NegativeMaximumOperands()
    {
        yield return ([Constant(-1), Constant(Rational.MinusInfinity)], true, false);
        yield return ([Constant(-1), Constant(0)], false, false);
    }

    public static IEnumerable<object[]> NegativeMaximumCases()
        => NegativeMaximumOperands().Select(item => new object[] { item.Operands, item.Expected, item.Computed });

    [Theory]
    [MemberData(nameof(NegativeMaximumCases))]
    public void NegativeMaximumUsesTheShortcutOrTheComputedCurve(Curve[] operands, bool expected, bool computed)
    {
        CurveExpression expression = operands[0].ToExpression();
        foreach (var operand in operands.Skip(1))
            expression = expression.Maximum(operand.ToExpression());
        Assert.Equal(expected, Curve.Maximum(operands).IsNegative);
        Assert.Equal(expected, expression.IsNegative);
        Assert.Equal(computed, expression.IsComputed);
    }

}
