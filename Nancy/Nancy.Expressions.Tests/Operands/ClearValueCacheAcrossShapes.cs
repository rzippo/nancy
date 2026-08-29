using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// ClearValueCache descends through EnumerateChildren, whose switch has an arm per arity and per operand-type combination.
// The other ClearValueCache tests only reach the n-ary and deviation arms, so these walk the unary and binary ones, including the crossings between the two hierarchies.
public class ClearValueCacheAcrossShapes
{
    private static readonly ExpressionSettings AlwaysNotCheap =
        new() { CacheSettings = new CacheSettings { CheapCacheSegmentThreshold = -1 } };

    private static CurveExpression Operand(int value, string name)
        => new ConcreteCurveExpression(new ConstantCurve(value), name);

    private static CurveExpression NamedSum(string name)
        => Operand(1, "a").Addition(Operand(2, "b"), settings: AlwaysNotCheap).WithName(name);

    [Fact]
    public void AUnaryExpressionClearsItsOperand()
    {
        var operand = NamedSum("operand");
        var expression = new NegateExpression(operand, "", AlwaysNotCheap);
        operand.ComputeWithoutResult();
        expression.ComputeWithoutResult();

        expression.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(expression.IsComputed);
        Assert.False(operand.IsComputed);
    }

    [Fact]
    public void ABinaryCurveExpressionClearsBothOperands()
    {
        var left = NamedSum("left");
        var right = NamedSum("right");
        var expression = new DeconvolutionExpression(left, right, "", AlwaysNotCheap);
        left.ComputeWithoutResult();
        right.ComputeWithoutResult();
        expression.ComputeWithoutResult();

        expression.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(expression.IsComputed);
        Assert.False(left.IsComputed);
        Assert.False(right.IsComputed);
    }

    // The right operand is a RationalExpression, so the walk crosses from one hierarchy to the other.

    [Fact]
    public void ACurveExpressionWithARationalOperandClearsTheCurveSide()
    {
        var left = NamedSum("left");
        var right = Expressions.FromRational(new Rational(2), "factor");
        var expression = new ScaleExpression(left, right, "", AlwaysNotCheap);
        left.ComputeWithoutResult();
        right.ComputeWithoutResult();
        expression.ComputeWithoutResult();

        expression.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(expression.IsComputed);
        Assert.False(left.IsComputed);
        // The Rational operand reports its own cache cheap, so it keeps it.
        Assert.True(right.IsComputed);
    }

    [Fact]
    public void ARationalUnaryExpressionOverACurveClearsTheCurve()
    {
        var operand = NamedSum("operand");
        var expression = Expressions.MaxValue(operand);
        operand.ComputeWithoutResult();
        expression.ComputeWithoutResult();

        expression.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(operand.IsComputed);
    }

    [Fact]
    public void ARationalBinaryExpressionOverRationalsIsWalkedWithoutClearingThem()
    {
        var left = Expressions.FromRational(new Rational(1, 2), "l");
        var right = Expressions.FromRational(new Rational(1, 3), "r");
        var expression = left.Subtraction(right);
        expression.ComputeWithoutResult();

        expression.ClearValueCache(CacheClearScope.Subtree);

        // Every node here is a Rational, and each reports its cache cheap, so the walk changes nothing.
        Assert.True(expression.IsComputed);
    }
}
