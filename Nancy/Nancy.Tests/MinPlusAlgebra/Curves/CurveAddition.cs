using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveAddition
{
    [Fact]
    public void SigmaRho_RateLatency()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve sum = arrival + service;

        Assert.False(sum.IsContinuous);
        Assert.True(sum.IsContinuousExceptOrigin);
        Assert.True(sum.IsLeftContinuous);
        Assert.True(sum.IsFinite);
        Assert.True(sum.IsUltimatelyPlain);
        Assert.True(sum.IsUltimatelyAffine);

        Assert.Equal(0, sum.ValueAt(0));
        Assert.Equal(150, sum.ValueAt(10));
        Assert.Equal(400, sum.ValueAt(20));
        //Assert.Equal(250, fun.ValueAt(30));

        Assert.Equal(5, sum.GetSegmentAfter(3).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(13).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(23).Slope);
    }

    [Fact]
    public void SigmaRho_RateLatency_Commutative()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve sum = service + arrival;

        Assert.False(sum.IsContinuous);
        Assert.True(sum.IsContinuousExceptOrigin);
        Assert.True(sum.IsLeftContinuous);
        Assert.True(sum.IsFinite);
        Assert.True(sum.IsUltimatelyPlain);
        Assert.True(sum.IsUltimatelyAffine);

        Assert.Equal(0, sum.ValueAt(0));
        Assert.Equal(150, sum.ValueAt(10));
        Assert.Equal(400, sum.ValueAt(20));
        //Assert.Equal(250, fun.ValueAt(30));

        Assert.Equal(5, sum.GetSegmentAfter(3).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(13).Slope);
        Assert.Equal(25, sum.GetSegmentAfter(23).Slope);
    }
}