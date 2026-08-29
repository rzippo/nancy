using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// The Curve side of each of these behaviours was already covered, while measured coverage showed the Rational counterparts reached by nothing.
// Each case here has a Curve-side twin elsewhere.
public class RationalExpressionParity
{
    private static RationalExpression A => Expressions.FromRational(new Rational(1, 2), "a");
    private static RationalExpression B => Expressions.FromRational(new Rational(1, 3), "b");

    [Fact]
    public void AppendMergesTheOperandsOfAnUnnamedExpressionOfTheSameOperator()
    {
        var left = (RationalNAryExpression)new RationalAdditionExpression([A, B], "");
        var right = new RationalAdditionExpression([A, B], "");

        var merged = (RationalNAryExpression)left.Append(right);

        Assert.Equal(4, merged.Operands.Count);
    }

    [Fact]
    public void AppendKeepsANamedExpressionOfTheSameOperatorWhole()
    {
        var left = (RationalNAryExpression)new RationalAdditionExpression([A, B], "");
        var right = new RationalAdditionExpression([A, B], "named");

        var appended = (RationalNAryExpression)left.Append(right);

        Assert.Equal(3, appended.Operands.Count);
    }

    [Fact]
    public void EqualsRejectsNull()
    {
        var expression = (RationalNAryExpression)new RationalAdditionExpression([A, B], "");

        Assert.False(expression.Equals(null));
    }

    [Fact]
    public void EqualsRejectsADifferentOperandCount()
    {
        var two = (RationalNAryExpression)new RationalAdditionExpression([A, B], "");
        var three = new RationalAdditionExpression([A, B, A], "");

        Assert.False(two.Equals(three));
    }

    [Fact]
    public void ClearValueCacheWithSelfOnlyLeavesDescendantsAlone()
    {
        var operand = Expressions.FromCurve(new ConstantCurve(1), "c");
        var deviation = Expressions.HorizontalDeviation(operand, operand);
        deviation.ComputeWithoutResult();
        operand.ComputeWithoutResult();

        deviation.ClearValueCache(CacheClearScope.SelfOnly);

        Assert.True(operand.IsComputed);
    }

    [Fact]
    public void ClearValueCacheStopsAtANamedChild()
    {
        var named = Expressions.FromCurve(new ConstantCurve(1), "c")
            .Addition(Expressions.FromCurve(new ConstantCurve(2), "d"))
            .WithName("named");
        var deviation = Expressions.HorizontalDeviation(named, named);
        named.ComputeWithoutResult();
        Assert.True(named.IsComputed);

        deviation.ClearValueCache(CacheClearScope.SubtreeUntilNamed);

        Assert.True(named.IsComputed);
    }

    [Fact]
    public void ARationalLeafReportsItsCacheCheap()
    {
        var leaf = new RationalNumberExpression(new Rational(1, 2));
        leaf.ComputeWithoutResult();

        leaf.ClearValueCache();

        Assert.True(leaf.IsComputed);
    }

    // RationalExpression.ValueCacheIsCheap is always true, so the guarded clear inside ClearValueCache never fires for any type in the library.
    // It is virtual, and this pins that an override reaches the clear, which is what makes the guard worth keeping.
    private record ExpensiveRationalSum : RationalAdditionExpression
    {
        public ExpensiveRationalSum(IReadOnlyCollection<IGenericExpression<Rational>> operands)
            : base(operands) { }

        protected override bool ValueCacheIsCheap => false;
    }

    [Fact]
    public void AnOverrideReportingItsCacheExpensiveHasItCleared()
    {
        var expression = new ExpensiveRationalSum([A, B]);
        expression.ComputeWithoutResult();
        Assert.True(expression.IsComputed);

        expression.ClearValueCache(CacheClearScope.SelfOnly);

        Assert.False(expression.IsComputed);
    }

    [Fact]
    public void AnUncomputedCurveExpressionReportsItsCacheCheap()
    {
        var expression = Expressions.FromCurve(new ConstantCurve(1), "a")
            .Addition(Expressions.FromCurve(new ConstantCurve(2), "b"));
        Assert.False(expression.IsComputed);

        expression.ClearValueCache();

        Assert.False(expression.IsComputed);
    }
}
