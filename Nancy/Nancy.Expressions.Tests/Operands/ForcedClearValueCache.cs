using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

/// <summary>
/// A forced <c>ClearValueCache</c> discards everything a computation left behind, so that computing again repeats all of its work.
/// </summary>
public class ForcedClearValueCache
{
    /// <summary>
    /// A curve that counts how often its cached properties are reset.
    /// </summary>
    private class ResetCountingCurve(Curve other) : Curve(other)
    {
        public int Resets { get; private set; }

        public override void ResetCachedProperties()
        {
            Resets++;
            base.ResetCachedProperties();
        }
    }

    private static readonly Curve A =
        new(new Sequence([Point.Origin(), new Segment(0, 2, 0, 1)]), 0, 2, 2);

    private static readonly Curve C = new SigmaRhoArrivalCurve(5, 1);

    private static readonly Curve D = new SigmaRhoArrivalCurve(1, 3);

    private static (CurveExpression root, CurveExpression intermediate, CurveExpression leftLeaf, CurveExpression rightLeaf) BuildTree()
    {
        var leftLeaf = A.ToExpression("a");
        var rightLeaf = C.ToExpression("c");
        var intermediate = Expressions.Convolution(leftLeaf, rightLeaf);
        var root = Expressions.Deconvolution(intermediate, D.ToExpression("d"));
        return (root, intermediate, leftLeaf, rightLeaf);
    }

    [Fact]
    public void EveryValueButALeafsIsCleared()
    {
        var (root, intermediate, leftLeaf, rightLeaf) = BuildTree();
        root.Compute();
        // both values are cheap under the default threshold, so an ordinary clear keeps them
        root.ClearValueCache();
        Assert.True(root.IsComputed);
        Assert.True(intermediate.IsComputed);

        root.ClearValueCache(force: true);

        Assert.False(root.IsComputed);
        Assert.False(intermediate.IsComputed);
        Assert.True(leftLeaf.IsComputed);
        Assert.True(rightLeaf.IsComputed);
    }

    [Fact]
    public void ARationalNodesValueIsCleared()
    {
        var root = Expressions.VerticalDeviation(A.ToExpression("a"), D.ToExpression("d"));
        root.Compute();

        root.ClearValueCache(force: true);

        Assert.False(root.IsComputed);
    }

    [Fact]
    public void ASequenceNodesValueIsCleared()
    {
        var leaf = A.Cut(0, 4).ToExpression("s");
        var root = leaf.Negate();
        root.Compute();

        root.ClearValueCache(force: true);

        Assert.False(root.IsComputed);
        Assert.True(leaf.IsComputed);
    }

    [Fact]
    public void EveryCachedPredicateIsCleared()
    {
        var (root, intermediate, leftLeaf, _) = BuildTree();
        CurveExpression[] nodes = [root, intermediate, leftLeaf];
        foreach (var node in nodes)
        {
            _ = node.IsSubAdditive;
            _ = node.IsNonNegative;
            Assert.NotNull(node.SubAdditiveIfKnown);
            Assert.NotNull(node.NonNegativeIfKnown);
        }

        root.ClearValueCache(force: true);

        Assert.All(nodes, node =>
        {
            Assert.Null(node.SubAdditiveIfKnown);
            Assert.Null(node.NonNegativeIfKnown);
        });
    }

    [Fact]
    public void ALeafsCurveForgetsItsCachedProperties()
    {
        var curve = new ResetCountingCurve(C);
        var root = Expressions.Convolution(A.ToExpression("a"), curve.ToExpression("c"));
        root.Compute();

        root.ClearValueCache();
        Assert.Equal(0, curve.Resets);

        root.ClearValueCache(force: true);
        Assert.Equal(1, curve.Resets);
    }

    [Fact]
    public void ComputingAgainRecomputesEveryIntermediate()
    {
        var (root, intermediate, _, _) = BuildTree();
        var first = root.Compute();
        var firstIntermediate = intermediate.Value;

        root.ClearValueCache(force: true);
        var second = root.Compute();

        Assert.NotSame(first, second);
        Assert.NotSame(firstIntermediate, intermediate.Value);
        Assert.True(Curve.Equivalent(first, second));
    }

    [Fact]
    public void TheScopeStillStopsAtANamedChild()
    {
        var named = Expressions.Convolution(A.ToExpression("a"), C.ToExpression("c")).WithName("x");
        var root = Expressions.Deconvolution(named, D.ToExpression("d"));
        root.Compute();
        Assert.True(named.IsComputed);

        root.ClearValueCache(CacheClearScope.SubtreeUntilNamed, force: true);

        Assert.False(root.IsComputed);
        Assert.True(named.IsComputed);
    }
}
