using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Settings given to <c>Compute</c> reach every operand and win over a node's own, group by group.
/// </summary>
/// <remarks>
/// Which settings a node computed under is read off its value:
/// with <see cref="ComputationSettings.UseRepresentationMinimization"/> off, a convolution of these operands keeps two elements that <see cref="Curve.Optimize"/> would merge away.
/// </remarks>
public class ComputeUnderGivenSettings
{
    // Affine, but described over a pseudo-period of 2.
    private static readonly Curve A =
        new(new Sequence([Point.Origin(), new Segment(0, 2, 0, 1)]), 0, 2, 2);

    private static readonly Curve C = new SigmaRhoArrivalCurve(5, 1);

    private static readonly Curve D = new SigmaRhoArrivalCurve(1, 3);

    private static readonly ExpressionSettings Unminimized =
        new() { ComputationSettings = new ComputationSettings { UseRepresentationMinimization = false } };

    private static readonly ExpressionSettings Minimized =
        new() { ComputationSettings = new ComputationSettings { UseRepresentationMinimization = true } };

    private static readonly ExpressionSettings CacheOnly =
        new() { CacheSettings = new CacheSettings() };

    private static bool IsMinimal(Curve curve)
        => curve.BaseSequence.Count == curve.Optimize().BaseSequence.Count;

    private static CurveExpression Convolution(ExpressionSettings own = null)
        => Expressions.Convolution(A.ToExpression("a"), C.ToExpression("c"), settings: own);

    [Fact]
    public void TheOperandsAreUnminimizedOnlyWhenComputedSo()
    {
        // pins the observable the other tests rely on
        Assert.True(IsMinimal(Convolution().Compute()));
        Assert.False(IsMinimal(Convolution().Compute(Unminimized)));
    }

    [Fact]
    public void TheSettingsReachEveryNodeOfACurveTree()
    {
        var convolution = Convolution();
        var root = Expressions.Minimum(convolution, D.ToExpression("d"));

        var value = root.Compute(Unminimized);

        Assert.False(IsMinimal(value));
        Assert.False(IsMinimal(convolution.Value));
    }

    [Fact]
    public void TheSettingsReachACurveOperandOfARationalNode()
    {
        var convolution = Convolution();
        var root = Expressions.VerticalDeviation(convolution, D.ToExpression("d"));

        root.Compute(Unminimized);

        Assert.False(IsMinimal(convolution.Value));
    }

    [Fact]
    public void TheSettingsReachACurveOperandOfASequenceNode()
    {
        var convolution = Convolution();
        var root = convolution.Cut(new Interval(0, 10, true, false));

        root.Compute(Unminimized);

        Assert.False(IsMinimal(convolution.Value));
    }

    [Fact]
    public void TheSettingsWinOverANodesOwn()
    {
        Assert.True(IsMinimal(Convolution(own: Unminimized).Compute(Minimized)));
        Assert.False(IsMinimal(Convolution(own: Minimized).Compute(Unminimized)));
    }

    [Fact]
    public void AGroupTheSettingsLeaveNullIsTheNodesOwn()
    {
        Assert.False(IsMinimal(Convolution(own: Unminimized).Compute(CacheOnly)));
    }

    [Fact]
    public void WithoutSettingsEachNodeComputesUnderItsOwn()
    {
        var convolution = Convolution();
        var root = Expressions.Minimum(convolution, D.ToExpression("d"), settings: Unminimized);

        var value = root.Compute();

        Assert.False(IsMinimal(value));
        Assert.True(IsMinimal(convolution.Value));
    }

    [Fact]
    public void ACachedNodeReturnsItsValueAndVisitsNoOperand()
    {
        // a negative threshold makes the operand's value clearable whatever its size
        var convolution = Convolution(own: new() { CacheSettings = new CacheSettings { CheapCacheElementThreshold = -1 } });
        var root = Expressions.Minimum(convolution, D.ToExpression("d"));
        var first = root.Compute();
        convolution.ClearValueCache(CacheClearScope.SelfOnly);

        var second = root.Compute(Unminimized);

        Assert.Same(first, second);
        Assert.False(convolution.IsComputed);
    }
}
