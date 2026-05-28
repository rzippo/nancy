using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class ConvexCurveTests
{
    [Fact]
    public void Constructor_ValidConvexCurve_Succeeds()
    {
        var inner = new RateLatencyServiceCurve(rate: 4, latency: 3);
        var cvx = new ConvexCurve(inner);
        Assert.True(cvx.IsConvex);
    }

    [Fact]
    public void Constructor_NonConvexCurve_Throws()
    {
        var nonConvex = new Curve(
            baseSequence: new Sequence(new Element[]
            {
                Point.Origin(),
                new Segment(0, 3, 0, 1),
                new Point(3, 3),
                new Segment(3, 5, 3, -1),
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 0
        );
        Assert.Throws<InvalidOperationException>(() => new ConvexCurve(nonConvex));
    }

    [Fact]
    public void Addition_WithConvex_ReturnsConvex()
    {
        var a = new ConvexCurve(new RateLatencyServiceCurve(rate: 4, latency: 3));
        var b = new RateLatencyServiceCurve(rate: 5, latency: 2);
        var result = a.Addition(b);
        Assert.IsType<ConvexCurve>(result);
        Assert.True(Curve.Equivalent(
            Curve.Addition(a, b),
            result
        ));
    }

    [Fact]
    public void Addition_WithNonConvex_ReturnsCurve()
    {
        var a = new ConvexCurve(new RateLatencyServiceCurve(rate: 4, latency: 3));
        var b = new Curve(
            baseSequence: new Sequence(new Element[]
            {
                Point.Origin(),
                new Segment(0, 5, 0, 1),
            }),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 5,
            pseudoPeriodHeight: 5
        );
        var result = a.Addition(b);
        Assert.IsNotType<ConvexCurve>(result);
    }

    [Fact]
    public void Maximum_WithConvex_ReturnsConvex()
    {
        var a = new ConvexCurve(new RateLatencyServiceCurve(rate: 4, latency: 3));
        var b = new ConvexCurve(new RateLatencyServiceCurve(rate: 5, latency: 2));
        var result = a.Maximum(b);
        Assert.IsType<ConvexCurve>(result);
        Assert.True(Curve.Equivalent(
            Curve.Maximum(a, b),
            result
        ));
    }

    [Fact]
    public void Convolution_WithConvex_ReturnsConvex()
    {
        var a = new ConvexCurve(new RateLatencyServiceCurve(rate: 4, latency: 3));
        var b = new ConvexCurve(new RateLatencyServiceCurve(rate: 5, latency: 2));
        var result = a.Convolution(b);
        Assert.IsType<ConvexCurve>(result);
        Assert.True(Curve.Equivalent(
            Curve.Convolution(a, b),
            result
        ));
    }
}
