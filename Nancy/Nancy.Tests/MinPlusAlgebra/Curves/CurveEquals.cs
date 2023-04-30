using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveEquals
{
    [Fact]
    public void SameCurves()
    {
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        Curve c2 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);

        Assert.True(c1.Equals(c2));
    }

    [Fact]
    public void DifferentCurves()
    {
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        Curve c2 = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Assert.False(c1.Equals(c2));
    }

    [Fact]
    public void NullObject()
    {
#nullable disable
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        object o = null;

        Assert.False(c1.Equals(o));
#nullable restore
    }

    [Fact]
    public void TypeMismatch()
    {
        Curve c1 = new SigmaRhoArrivalCurve(sigma: 100, rho: 5);
        Point p = new Point(5, 3);

        Assert.False(c1.Equals(p));
    }
}