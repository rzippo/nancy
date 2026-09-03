using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class ClearValueCacheTests
{
    // A trivially small ConstantCurve is "cheap" under the default threshold.
    // A negative threshold forces not-cheap whatever the actual size, the element count never being negative, so the tests can observe clearing without a curve wide enough to cross the real default.
    private static readonly ExpressionSettings AlwaysNotCheap =
        new() { CacheSettings = new CacheSettings { CheapCacheElementThreshold = -1 } };

    // A named composite child.
    // N-ary Compute() goes through FlattenOperands, which recurses straight past a named intermediate to the true leaves beneath it.
    // Computing the root therefore leaves a named child's own cache unpopulated, and it must be forced explicitly.
    private static (CurveExpression root, CurveExpression namedChild) BuildRootWithNamedChild()
    {
        var a = new ConcreteCurveExpression(new ConstantCurve(1), "a");
        var b = new ConcreteCurveExpression(new ConstantCurve(2), "b");
        var namedChild = a.Addition(b, settings: AlwaysNotCheap).WithName("namedChild");
        var c = new ConcreteCurveExpression(new ConstantCurve(3), "c");
        var root = namedChild.Addition(c, settings: AlwaysNotCheap);
        Assert.Equal(2, ((CurveNAryExpression)root).Operands.Count); // pins namedChild was not flattened away
        return (root, namedChild);
    }

    [Fact]
    public void SelfOnlyClearsOnlyTheTargetNode()
    {
        var (root, namedChild) = BuildRootWithNamedChild();
        namedChild.ComputeWithoutResult();
        root.ComputeWithoutResult();
        Assert.True(root.IsComputed);
        Assert.True(namedChild.IsComputed);

        root.ClearValueCache(CacheClearScope.SelfOnly);

        Assert.False(root.IsComputed);
        Assert.True(namedChild.IsComputed);
    }

    [Fact]
    public void SubtreeClearsThroughANamedChildRegardless()
    {
        var (root, namedChild) = BuildRootWithNamedChild();
        namedChild.ComputeWithoutResult();
        root.ComputeWithoutResult();
        Assert.True(root.IsComputed);
        Assert.True(namedChild.IsComputed);

        root.ClearValueCache(CacheClearScope.Subtree);

        Assert.False(root.IsComputed);
        Assert.False(namedChild.IsComputed);
    }

    [Fact]
    public void SubtreeUntilNamedStopsAtANamedChild()
    {
        var (root, namedChild) = BuildRootWithNamedChild();
        namedChild.ComputeWithoutResult();
        root.ComputeWithoutResult();
        Assert.True(root.IsComputed);
        Assert.True(namedChild.IsComputed);

        root.ClearValueCache(CacheClearScope.SubtreeUntilNamed);

        Assert.False(root.IsComputed);
        Assert.True(namedChild.IsComputed);
    }

    [Fact]
    public void ALeafsValueIsNeverClearedUnderAnyScope()
    {
        var leaf = new ConcreteCurveExpression(new ConstantCurve(1), "a", AlwaysNotCheap);
        Assert.True(leaf.IsComputed); // a leaf's Value is set at construction, always already computed

        leaf.ClearValueCache(CacheClearScope.Subtree);

        Assert.True(leaf.IsComputed);
        Assert.Equal(new ConstantCurve(1), leaf.Value);
    }

    [Fact]
    public void ClearingRecursesAcrossTheCurveRationalBoundary()
    {
        var a = new ConcreteCurveExpression(new ConstantCurve(1), "a");
        var b = new ConcreteCurveExpression(new ConstantCurve(2), "b");
        var left = a.Addition(b, settings: AlwaysNotCheap); // composite, forced not-cheap
        var right = new ConcreteCurveExpression(new ConstantCurve(5), "right");
        var deviation = (RationalExpression)Expressions.HorizontalDeviation(left, right);
        deviation.ComputeWithoutResult(); // binary Compute() reads LeftOperand/RightOperand.Value
        // directly, not through FlattenOperands, so this does cascade into left's own cache.
        Assert.True(left.IsComputed);
        Assert.True(deviation.IsComputed);

        deviation.ClearValueCache(CacheClearScope.Subtree);

        // The Rational result is always cheap, so it survives; its composite Curve operand does not.
        Assert.True(deviation.IsComputed);
        Assert.False(left.IsComputed);
    }

    [Fact]
    public void ACheapNodeUnderTheDefaultThresholdIsNotCleared()
    {
        var a = new ConcreteCurveExpression(new ConstantCurve(1), "a");
        var b = new ConcreteCurveExpression(new ConstantCurve(2), "b");
        var root = a.Addition(b); // default settings: a two-segment sum is well under the threshold
        root.ComputeWithoutResult();
        Assert.True(root.IsComputed);

        root.ClearValueCache(CacheClearScope.Subtree);

        Assert.True(root.IsComputed);
    }
}
