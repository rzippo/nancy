using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveEquivalence
{
    [Fact]
    public void SameCurves()
    {
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        Curve c2 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);

        Assert.True(c1.Equivalent(c2));
        Assert.True(c1.EquivalentExceptOrigin(c2));
    }

    [Fact]
    public void DifferentCurves()
    {
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        Curve c2 = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Assert.False(c1.Equivalent(c2));
        Assert.False(c1.EquivalentExceptOrigin(c2));
    }
}