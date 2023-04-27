using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class SigmaRho
{
    [Fact]
    public void SigmaRhoCtor()
    {
        SigmaRhoArrivalCurve curve = new SigmaRhoArrivalCurve(sigma: 5, rho: 10);

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.False(curve.IsContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(0, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(5, curve.RightLimitAt(0));
        Assert.Equal(55, curve.ValueAt(5));
        Assert.Equal(105, curve.ValueAt(10));            
    }
}