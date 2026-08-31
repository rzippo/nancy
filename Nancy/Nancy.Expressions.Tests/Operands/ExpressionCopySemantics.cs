using System.Collections.Generic;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// A copy carries what the caller set and leaves the caches behind, so an expression built by changing an operand cannot answer with the original's value.
public class ExpressionCopySemantics
{
    private static readonly ExpressionSettings Settings =
        new() { ComputationSettings = new ComputationSettings { UseParallelism = false } };

    private static CurveExpression Leaf(int value, string name)
        => new ConcreteCurveExpression(new ConstantCurve(value), name);

    // Each case computes the expression, then derives a new one from it.
    public static IEnumerable<object[]> Derivations()
    {
        yield return Case("with, on a binary operand", () =>
        {
            var original = new DeconvolutionExpression(Leaf(1, "a"), Leaf(2, "b"), "d", Settings);
            original.ComputeWithoutResult();
            return (original, original with { LeftOperand = Leaf(3, "c") });
        });

        yield return Case("with, on a unary operand", () =>
        {
            var original = new NegateExpression(Leaf(1, "a"), "n", Settings);
            original.ComputeWithoutResult();
            return (original, original with { Operand = Leaf(3, "c") });
        });

        yield return Case("ReplaceByPosition, on an n-ary operand", () =>
        {
            var original = (CurveExpression)Leaf(1, "a").Addition(Leaf(2, "b"), "s", Settings);
            original.ComputeWithoutResult();
            return (original, original.ReplaceByPosition(new ExpressionPosition(["0"]), new ConstantCurve(100), "big"));
        });

        static object[] Case(string label, System.Func<(CurveExpression, CurveExpression)> make)
            => [label, make];
    }

    [Theory]
    [MemberData(nameof(Derivations))]
    public void ADerivedExpressionStartsWithoutTheOriginalsCaches(
        string label, System.Func<(CurveExpression Original, CurveExpression Derived)> make)
    {
        var (original, derived) = make();

        Assert.True(original.IsComputed);
        Assert.False(derived.IsComputed);
    }

    [Theory]
    [MemberData(nameof(Derivations))]
    public void ADerivedExpressionKeepsTheSettingsTheCallerSet(
        string label, System.Func<(CurveExpression Original, CurveExpression Derived)> make)
    {
        var (original, derived) = make();

        Assert.Same(Settings, original.Settings);
        Assert.Same(Settings, derived.Settings);
    }

    [Fact]
    public void ReplacingAnOperandChangesTheValueAndLeavesTheOriginalAlone()
    {
        var original = (CurveExpression)Leaf(1, "a").Addition(Leaf(2, "b"), "s", Settings);
        original.ComputeWithoutResult();

        var replaced = original.ReplaceByPosition(new ExpressionPosition(["0"]), new ConstantCurve(100), "big");

        Assert.Equal(102, replaced.Value.ValueAt(1));
        Assert.Equal(3, original.Value.ValueAt(1));
    }

    // Renaming is the one derivation that may keep the caches, the value being independent of the name.
    [Fact]
    public void RenamingKeepsTheComputedValue()
    {
        var original = (CurveExpression)Leaf(1, "a").Addition(Leaf(2, "b"), "s", Settings);
        original.ComputeWithoutResult();

        var renamed = original.WithName("renamed");

        Assert.True(renamed.IsComputed);
        Assert.Equal(original.Value, renamed.Value);
        Assert.Same(Settings, renamed.Settings);
    }
}
