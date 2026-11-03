using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
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

    [Fact]
    public void AnInfiniteValueAtAFiniteTimeIsTheMaximum()
    {
        var curve = new Curve(new Sequence([Point.PlusInfinite(0), new Segment(0, 1, 0, 1)]), 0, 1, 1);
        Assert.Equal(Rational.PlusInfinity, curve.MaxValue());
        Assert.Equal(0, curve.MaxArg());
    }

    [Fact]
    public void AMinusInfiniteValueAtAFiniteTimeIsTheMinimum()
    {
        var curve = new Curve(new Sequence([Point.MinusInfinite(0), new Segment(0, 1, 0, -1)]), 0, 1, -1);
        Assert.Equal(Rational.MinusInfinity, curve.MinValue());
        Assert.Equal(0, curve.MinArg());
    }

    [Fact]
    public void AnUltimatelyInfiniteCurveAttainsItsMaximum()
    {
        var curve = new DelayServiceCurve(2);
        Assert.Equal(Rational.PlusInfinity, curve.MaxValue());
        var arg = curve.MaxArg();
        Assert.NotNull(arg);
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(arg.Value));
    }

    [Fact]
    public void AnUltimatelyMinusInfiniteCurveAttainsItsMinimum()
    {
        var curve = -new DelayServiceCurve(2);
        Assert.Equal(Rational.MinusInfinity, curve.MinValue());
        var arg = curve.MinArg();
        Assert.NotNull(arg);
        Assert.Equal(Rational.MinusInfinity, curve.ValueAt(arg.Value));
    }

    [Fact]
    public void AnUnboundedFiniteCurveHasNoMaximum()
    {
        var curve = new RateLatencyServiceCurve(1, 1);
        Assert.Null(curve.MaxValue());
        Assert.Null(curve.MaxArg());
    }

    [Fact]
    public void AnUnboundedFiniteCurveHasNoMinimum()
    {
        var curve = -new RateLatencyServiceCurve(1, 1);
        Assert.Null(curve.MinValue());
        Assert.Null(curve.MinArg());
    }
}
