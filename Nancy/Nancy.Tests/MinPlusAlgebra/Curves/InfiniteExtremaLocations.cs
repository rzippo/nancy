using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class InfiniteExtremaLocations
{
    [Fact]
    public void ADecreasingRampApproachesItsInfimumAtPositiveInfinity()
    {
        var curve = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, -1);
        Assert.Equal(Rational.PlusInfinity, curve.InfArg());
    }

    [Fact]
    public void AnInfiniteSupremumHasItsFirstFiniteBoundaryTime()
    {
        var curve = new Curve(new Sequence([Point.PlusInfinite(0), new Segment(0, 1, 0, 1)]), 0, 1, 1);
        Assert.Equal(0, curve.SupArg());
    }

    [Fact]
    public void AnInfiniteInfimumHasItsFirstFiniteBoundaryTime()
    {
        var curve = new Curve(new Sequence([Point.MinusInfinite(0), new Segment(0, 1, 0, -1)]), 0, 1, -1);
        Assert.Equal(0, curve.InfArg());
    }
}
