using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class IsWellDefinedOperations
{
    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void SuperAdditiveClosureOfALinearCurveIsWellDefined()
    {
        var service = new RateLatencyServiceCurve(3, 0).ToExpression("s");

        Assert.True(service.SuperAdditiveClosure().IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void LowerPseudoInverseOfAnArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.LowerPseudoInverse().IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void UpperPseudoInverseOfAnArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.UpperPseudoInverse().IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void MaxPlusConvolutionOfLinearCurvesIsWellDefined()
    {
        var service = new RateLatencyServiceCurve(3, 0).ToExpression("s");

        Assert.True(Expressions.MaxPlusConvolution(service, service).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void MaxPlusDeconvolutionOfLinearCurvesIsWellDefined()
    {
        var service = new RateLatencyServiceCurve(3, 0).ToExpression("s");

        Assert.True(Expressions.MaxPlusDeconvolution(service, service).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void CompositionOfLinearCurvesIsWellDefined()
    {
        var service = new RateLatencyServiceCurve(3, 0).ToExpression("s");

        Assert.True(Expressions.Composition(service, service).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void DelayedArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.DelayBy(1).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void ForwardedArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.ForwardBy(1).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void HorizontallyShiftedArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.HorizontalShift(1).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void VerticallyShiftedArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.VerticalShift(1).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void ScaledArrivalCurveIsWellDefined()
    {
        var arrival = new SigmaRhoArrivalCurve(2, 1).ToExpression("a");

        Assert.True(arrival.Scale(2).IsWellDefined);
    }

    [Fact(Skip = "IsWellDefined visitor case is not implemented yet; tracked as nancy-expressions#215.")]
    public void DeconvolutionOfPositiveInfiniteCurvesIsNotWellDefined()
    {
        var left = Curve.PlusInfinite().ToExpression("f");
        var right = Curve.PlusInfinite().ToExpression("g");

        Assert.False(Expressions.Deconvolution(left, right).IsWellDefined);
    }
}
