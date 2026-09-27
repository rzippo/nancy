using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// With <see cref="CacheSettings.ClearOperandsWhenComputed"/>, a node that has just computed clears the values of the unnamed operands it read, unless they are leaves or cheap.
/// </summary>
public class ClearOperandsWhenComputed
{
    private static readonly Curve A =
        new(new Sequence([Point.Origin(), new Segment(0, 2, 0, 1)]), 0, 2, 2);

    private static readonly Curve C = new SigmaRhoArrivalCurve(5, 1);

    private static readonly Curve D = new SigmaRhoArrivalCurve(1, 3);

    // A negative threshold counts every value as expensive, so clearing is observable on small curves.
    private static readonly ExpressionSettings Clearing =
        new() { CacheSettings = new CacheSettings { ClearOperandsWhenComputed = true, CheapCacheElementThreshold = -1 } };

    private static readonly ExpressionSettings ClearingWithDefaultThreshold =
        new() { CacheSettings = new CacheSettings { ClearOperandsWhenComputed = true } };

    private static CurveExpression Leaf(Curve curve, string name) => curve.ToExpression(name);

    private static CurveExpression Convolution()
        => Expressions.Convolution(Leaf(A, "a"), Leaf(C, "c"));

    /// <summary>
    /// A root over an unnamed operand, and over a named one which has an unnamed operand of its own.
    /// </summary>
    private static (CurveExpression root, CurveExpression unnamed, CurveExpression named, CurveExpression belowNamed, CurveExpression leaf)
        BuildTree()
    {
        var unnamed = Convolution();
        var belowNamed = Convolution();
        var named = Expressions.Deconvolution(belowNamed, Leaf(D, "d")).WithName("x");
        var leaf = Leaf(D, "e");
        var root = Expressions.Minimum(Expressions.Deconvolution(unnamed, leaf), named);
        return (root, unnamed, named, belowNamed, leaf);
    }

    [Fact]
    public void OnlyTheValuesOfUnnamedIntermediatesAreCleared()
    {
        var (root, unnamed, named, belowNamed, leaf) = BuildTree();

        root.Compute(Clearing);

        Assert.True(root.IsComputed);
        Assert.False(unnamed.IsComputed);
        Assert.True(named.IsComputed);
        Assert.False(belowNamed.IsComputed);
        Assert.True(leaf.IsComputed);
    }

    [Fact]
    public void TheValueIsTheOneComputedWithoutClearing()
    {
        var withClearing = BuildTree().root.Compute(Clearing);
        var without = BuildTree().root.Compute();

        Assert.True(Curve.Equivalent(without, withClearing));
    }

    [Fact]
    public void AnUnnamedLeafKeepsItsValue()
    {
        var leaf = Expressions.FromCurve(D, "");
        Assert.True(string.IsNullOrEmpty(leaf.Name));
        var root = Expressions.Deconvolution(Convolution(), leaf);

        root.Compute(Clearing);

        Assert.True(leaf.IsComputed);
    }

    [Fact]
    public void ACheapOperandKeepsItsValue()
    {
        var operand = Convolution();
        var root = Expressions.Deconvolution(operand, Leaf(D, "d"));

        root.Compute(ClearingWithDefaultThreshold);

        Assert.True(operand.IsComputed);
    }

    [Fact]
    public void AnOperandIsClearedByItsParentAsSoonAsTheParentHasComputed()
    {
        var operand = Convolution();
        var parent = Expressions.Deconvolution(operand, Leaf(D, "d"));

        parent.Compute(Clearing);

        Assert.True(parent.IsComputed);
        Assert.False(operand.IsComputed);
    }

    [Fact]
    public void AnUnnamedOperandSharedByTwoParentsIsComputedAgainForTheSecond()
    {
        var shared = Convolution();
        var root = Expressions.Minimum(
            Expressions.Deconvolution(shared, Leaf(D, "d")),
            Expressions.Convolution(shared, Leaf(D, "e")));

        var withClearing = root.Compute(Clearing);

        Assert.False(shared.IsComputed);
        var sharedAgain = Convolution();
        var without = Expressions.Minimum(
            Expressions.Deconvolution(sharedAgain, Leaf(D, "d")),
            Expressions.Convolution(sharedAgain, Leaf(D, "e"))).Compute();
        Assert.True(Curve.Equivalent(without, withClearing));
    }

    [Fact]
    public void AFlattenedNAryNodeClearsTheOperandsItRead()
    {
        var first = Convolution();
        var second = Convolution();
        var third = Convolution();
        var root = Expressions.Addition(Expressions.Addition(first, second), third);

        root.Compute(Clearing);

        Assert.False(first.IsComputed);
        Assert.False(second.IsComputed);
        Assert.False(third.IsComputed);
    }

    [Fact]
    public void ARationalNodeClearsItsCurveOperands()
    {
        var operand = Convolution();
        var root = Expressions.VerticalDeviation(operand, Leaf(D, "d"));

        root.Compute(Clearing);

        Assert.False(operand.IsComputed);
    }

    [Fact]
    public void ASequenceNodeClearsItsCurveOperand()
    {
        var operand = Convolution();
        var root = operand.Cut(new Interval(0, 10, true, false));

        root.Compute(Clearing);

        Assert.False(operand.IsComputed);
    }

    [Fact]
    public void ACacheHitClearsNothing()
    {
        var operand = Convolution();
        var root = Expressions.Deconvolution(operand, Leaf(D, "d"));
        root.Compute();

        root.Compute(Clearing);

        Assert.True(operand.IsComputed);
    }

    [Fact]
    public void WithoutTheSwitchNothingIsCleared()
    {
        var (root, unnamed, _, belowNamed, _) = BuildTree();

        root.Compute(new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheElementThreshold = -1 } });

        Assert.True(unnamed.IsComputed);
        Assert.True(belowNamed.IsComputed);
    }
}
