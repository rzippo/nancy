using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class SuperAdditiveClosure
{
    private readonly ITestOutputHelper output;

    public SuperAdditiveClosure(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void PureDelay(int delay)
    {
        var curve = new DelayServiceCurve(delay);

        var closure = curve.SuperAdditiveClosure();
        Assert.True(Curve.Equivalent(closure, curve));
    }

    public static List<Curve> PositiveAtOriginRightLimit =
    [
        // constant 3
        new Curve(
            new Sequence([Point.Origin(), Segment.Constant(0, 1, 3), new Point(1, 3), Segment.Constant(1, 2, 3)]),
            pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 0),
        // starts at 3, then decreases with slope -1
        new Curve(
            new Sequence([Point.Origin(), new Segment(0, 1, 3, -1), new Point(1, 2), new Segment(1, 2, 2, -1)]),
            pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: -1)
    ];

    public static IEnumerable<object[]> GetPositiveAtOriginRightLimit()
        => PositiveAtOriginRightLimit.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetPositiveAtOriginRightLimit))]
    public void ClosureOfCurvePositiveAtOriginRightLimit(Curve operand)
    {
        // splitting any t > 0 into k parts gives k * f(t/k), which diverges when f(0+) > 0
        Assert.True(operand.RightLimitAt(0) > 0);

        var closure = operand.SuperAdditiveClosure();

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(17));
        Assert.True(new Curve(closure).IsRegularSuperAdditive);
    }

    [Theory]
    [InlineData(3, 2, 5, 1)]
    [InlineData(2, 1, 4, 3)]
    public void ClosureOfTokenBucketMinusRateLatency(int rate, int latency, int sigma, int rho)
    {
        // the difference rises by sigma at the origin, whichever way its tail goes
        var beta = new RateLatencyServiceCurve(rate: rate, latency: latency);
        var alpha = new SigmaRhoArrivalCurve(sigma: sigma, rho: rho);
        var operand = alpha - beta;

        Assert.Equal(0, operand.ValueAt(0));
        Assert.Equal(sigma, operand.RightLimitAt(0));

        var closure = operand.SuperAdditiveClosure();

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(17));
        Assert.True(new Curve(closure).IsRegularSuperAdditive);
    }

    // note: super-additive closure and super-additive property are not well defined enough for infinite parts as in the pure delay
    // we rely on the super class properties in the meantime 

    // [Theory]
    // [InlineData(0)]
    // //[InlineData(5)]
    // public void PureDelay_AsGeneric(int delay)
    // {
    //     var delayCurve = new DelayServiceCurve(delay);
    //     var curve = new Curve(delayCurve);  // ensures no optimization is involved
    //
    //     var closure = curve.SuperAdditiveClosure();
    //     Assert.True(Curve.Equivalent(closure, curve));
    // }

    public static readonly List<(Curve curve, Curve closure)> KnownClosures = new()
    {
        (
            new RateLatencyServiceCurve(1, 1),
            new RateLatencyServiceCurve(1, 1)
        ),
        (
            new Curve(new RateLatencyServiceCurve(1, 1)),
            new Curve(new RateLatencyServiceCurve(1, 1))
        ),
        (
            new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0),
                    new Segment(0, 3, 0, 0),
                    new Point(3, 5),
                    new Segment(3, 4, 5, 1),
                    new Point(4, 6), new Segment(4, 5, 6, 1)
                }), 
                pseudoPeriodStart: 4, 
                pseudoPeriodLength: 1, 
                pseudoPeriodHeight: 1
            ),
            new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0, 0),
                    new Segment(0, 3, 0, 0),
                    new Point(3, 5),
                    new Segment(3, 6, 5, 1),
                }), 
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 3,
                pseudoPeriodHeight: 5
            )
        ),
        (
            new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0,0), 
                    new Segment(0,5,0,0), 
                    new Point(5,12), 
                    new Segment(5,15,12,new Rational(1, 10))
                }),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 10,
                pseudoPeriodHeight: 1
            ),
            new Curve(
                baseSequence: new Sequence(new List<Element>
                {
                    new Point(0,0), 
                    new Segment(0,5,0,0), 
                    new Point(5,12), 
                    new Segment(5,10,12,new Rational(1, 10))
                }),
                pseudoPeriodStart: 5,
                pseudoPeriodLength: 5,
                pseudoPeriodHeight: 12
            )
        )
    };

    public static IEnumerable<object[]> KnownClosuresTestCases()
    {
        foreach (var (curve, closure) in KnownClosures)
        {
            yield return new object[] { curve, closure };
        }
    }

    [Theory]
    [MemberData(nameof(KnownClosuresTestCases))]
    public void KnownClosuresTest(Curve curve, Curve expected)
    {
        var closure = curve.SuperAdditiveClosure();
        Assert.True(Curve.Equivalent(expected, closure));
    }

    public static IEnumerable<object[]> ClosureDominanceTestCases()
    {
        foreach (var (curve, _) in KnownClosures)
        {
            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(ClosureDominanceTestCases))]
    public void ClosureDominanceTest(Curve curve)
    {
        var closure = curve.SuperAdditiveClosure();
        Assert.True(closure >= curve);
    }
}