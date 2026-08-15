using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class RaisedRateLatency
{
    public static List<(Rational latency, Rational rate, Rational bufferShift)> RaisedRateLatencyCtorCases =
    [
        (0, 5, 0),
        (0, 5, 5),
        (5, 10, 0),
        (5, 10, 8),
        (14.5m, new Rational(20, 3), 4)
    ];

    public static IEnumerable<object[]> GetRaisedRateLatencyCtorCases()
        => RaisedRateLatencyCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void RaisedRateLatencyCtor(Rational latency, Rational rate, Rational bufferShift)
    {
        RaisedRateLatencyServiceCurve curve = new RaisedRateLatencyServiceCurve(rate, latency, bufferShift);

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsRightContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.True(curve.IsUltimatelyAffine);
        if (bufferShift == 0)
            Assert.Equal(latency, curve.FirstNonZeroTime);

        Assert.Equal(bufferShift, curve.ValueAt(0));
        Assert.Equal(bufferShift, curve.ValueAt(latency));
        Assert.Equal(bufferShift, curve.RightLimitAt(latency));
        if (latency > 0)
            Assert.Equal(bufferShift, curve.LeftLimitAt(latency));

        Assert.Equal(bufferShift + rate, curve.ValueAt(latency + 1));
        Assert.Equal(bufferShift + 2 * rate, curve.ValueAt(latency + 2));
        Assert.Equal(bufferShift + 10.5m * rate, curve.ValueAt(latency + 10.5m));
        Assert.Equal(bufferShift + 110 * rate, curve.ValueAt(latency + 110));
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void RaisedRateLatencyCtor_WithZeroOrigin(Rational latency, Rational rate, Rational bufferShift)
    {
        var curve = new RaisedRateLatencyServiceCurve(rate, latency, bufferShift).WithZeroOrigin();

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        if (bufferShift == 0)
        {
            Assert.True(curve.IsContinuous);
            Assert.True(curve.IsRightContinuous);
        }
        else
        {
            Assert.False(curve.IsContinuous);
            Assert.False(curve.IsRightContinuous);
        }
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.True(curve.IsUltimatelyAffine);
        if (bufferShift == 0)
            Assert.Equal(latency, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(latency > 0 ? bufferShift : 0, curve.ValueAt(latency));
        Assert.Equal(bufferShift, curve.RightLimitAt(latency));
        if (latency > 0)
            Assert.Equal(bufferShift, curve.LeftLimitAt(latency));

        Assert.Equal(bufferShift + rate, curve.ValueAt(latency + 1));
        Assert.Equal(bufferShift + 2 * rate, curve.ValueAt(latency + 2));
        Assert.Equal(bufferShift + 10.5m * rate, curve.ValueAt(latency + 10.5m));
        Assert.Equal(bufferShift + 110 * rate, curve.ValueAt(latency + 110));
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void FromSum_ConstantCurve_Closure(Rational delay, Rational rate, Rational bufferShift)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, delay);
        var constantCurve = new ConstantCurve(bufferShift);

        var raisedRateLatency = rateLatency + constantCurve;

        Assert.True(raisedRateLatency.IsFinite);
        Assert.False(raisedRateLatency.IsZero);
        // a constant curve is 0 at the origin, so the sum is not raised there and jumps unless the shift is 0
        if (bufferShift == 0)
        {
            Assert.True(raisedRateLatency.IsContinuous);
            Assert.True(raisedRateLatency.IsRightContinuous);
        }
        else
        {
            Assert.False(raisedRateLatency.IsContinuous);
            Assert.False(raisedRateLatency.IsRightContinuous);
        }
        Assert.True(raisedRateLatency.IsContinuousExceptOrigin);
        Assert.True(raisedRateLatency.IsLeftContinuous);
        Assert.True(raisedRateLatency.IsUltimatelyPlain);
        Assert.True(raisedRateLatency.IsUltimatelyAffine);
        if (bufferShift == 0)
            Assert.Equal(delay, raisedRateLatency.FirstNonZeroTime);

        Assert.Equal(0, raisedRateLatency.ValueAt(0));
        Assert.Equal(delay > 0 ? bufferShift : 0, raisedRateLatency.ValueAt(delay));
        Assert.Equal(bufferShift, raisedRateLatency.RightLimitAt(delay));
        if (delay > 0)
            Assert.Equal(bufferShift, raisedRateLatency.LeftLimitAt(delay));

        Assert.Equal(bufferShift + rate, raisedRateLatency.ValueAt(delay + 1));
        Assert.Equal(bufferShift + 2 * rate, raisedRateLatency.ValueAt(delay + 2));
        Assert.Equal(bufferShift + 10.5m * rate, raisedRateLatency.ValueAt(delay + 10.5m));
        Assert.Equal(bufferShift + 110 * rate, raisedRateLatency.ValueAt(delay + 110));

        var closure = raisedRateLatency.SubAdditiveClosure();

        if (bufferShift == 0 && delay > 0)
        {
            Assert.True(closure.IsFinite);
            Assert.True(closure.IsZero);
        }
        else
        {
            Assert.True(closure.IsFinite);
            Assert.False(closure.IsZero);
            if (bufferShift == 0)
            {
                Assert.True(closure.IsContinuous);
                Assert.True(closure.IsRightContinuous);
            }
            else
            {
                Assert.False(closure.IsContinuous);
                Assert.False(closure.IsRightContinuous);
            }
            Assert.True(closure.IsContinuousExceptOrigin);
            Assert.True(closure.IsLeftContinuous);
            Assert.True(closure.PseudoPeriodSlope > 0);
        }            
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void FromSum_Constant_Closure(Rational delay, Rational rate, Rational bufferShift)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, delay);

        var raisedRateLatency = rateLatency + bufferShift;

        Assert.True(raisedRateLatency.IsFinite);
        Assert.False(raisedRateLatency.IsZero);
        Assert.True(raisedRateLatency.IsContinuous);
        Assert.True(raisedRateLatency.IsRightContinuous);
        Assert.True(raisedRateLatency.IsContinuousExceptOrigin);
        Assert.True(raisedRateLatency.IsLeftContinuous);
        Assert.True(raisedRateLatency.IsUltimatelyPlain);
        Assert.True(raisedRateLatency.IsUltimatelyAffine);
        if (bufferShift == 0)
            Assert.Equal(delay, raisedRateLatency.FirstNonZeroTime);

        Assert.Equal(bufferShift, raisedRateLatency.ValueAt(0));
        Assert.Equal(bufferShift, raisedRateLatency.ValueAt(delay));
        Assert.Equal(bufferShift, raisedRateLatency.RightLimitAt(delay));
        if (delay > 0)
            Assert.Equal(bufferShift, raisedRateLatency.LeftLimitAt(delay));

        Assert.Equal(bufferShift + rate, raisedRateLatency.ValueAt(delay + 1));
        Assert.Equal(bufferShift + 2 * rate, raisedRateLatency.ValueAt(delay + 2));
        Assert.Equal(bufferShift + 10.5m * rate, raisedRateLatency.ValueAt(delay + 10.5m));
        Assert.Equal(bufferShift + 110 * rate, raisedRateLatency.ValueAt(delay + 110));

        var closure = raisedRateLatency.SubAdditiveClosure();

        if (bufferShift == 0 && delay > 0)
        {
            Assert.True(closure.IsFinite);
            Assert.True(closure.IsZero);
        }
        else
        {
            Assert.True(closure.IsFinite);
            Assert.False(closure.IsZero);
            if (bufferShift == 0)
            {
                Assert.True(closure.IsContinuous);
                Assert.True(closure.IsRightContinuous);
            }
            else
            {
                Assert.False(closure.IsContinuous);
                Assert.False(closure.IsRightContinuous);
            }
            Assert.True(closure.IsContinuousExceptOrigin);
            Assert.True(closure.IsLeftContinuous);
            Assert.True(closure.PseudoPeriodSlope > 0);
        }
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void SumWithConstantCurve_MatchesTheDefinition(Rational latency, Rational rate, Rational bufferShift)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, latency);
        var constantCurve = new ConstantCurve(bufferShift);

        var typed = rateLatency + constantCurve;
        var commuted = constantCurve + rateLatency;
        var byDefinition = new Curve(rateLatency) + new Curve(constantCurve);

        Assert.IsType<RaisedRateLatencyServiceCurve>(typed);
        Assert.True(Curve.Equivalent(byDefinition, typed));
        Assert.True(Curve.Equivalent(byDefinition, commuted));
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void VerticalShift_MatchesTheDefinition(Rational latency, Rational rate, Rational bufferShift)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, latency);
        var byDefinition = new Curve(rateLatency);

        foreach (var exceptOrigin in new[] { true, false })
        {
            var typed = rateLatency.VerticalShift(bufferShift, exceptOrigin);

            Assert.True(Curve.Equivalent(byDefinition.VerticalShift(bufferShift, exceptOrigin), typed));
            if (bufferShift > 0)
                Assert.IsType<RaisedRateLatencyServiceCurve>(typed);
        }
    }

    [Theory]
    [MemberData(nameof(GetRaisedRateLatencyCtorCases))]
    public void BothOrigins_ShareTheSubAdditiveClosure(Rational latency, Rational rate, Rational bufferShift)
    {
        var raised = new RaisedRateLatencyServiceCurve(rate, latency, bufferShift, withZeroOrigin: false);
        var zeroOrigin = new RaisedRateLatencyServiceCurve(rate, latency, bufferShift, withZeroOrigin: true);

        Assert.Equal(bufferShift, raised.ValueAt(0));
        Assert.Equal(0, zeroOrigin.ValueAt(0));
        Assert.Equal(raised.RightLimitAt(0), zeroOrigin.RightLimitAt(0));

        Assert.True(Curve.Equivalent(raised.SubAdditiveClosure(), zeroOrigin.SubAdditiveClosure()));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(-1)]
    public void SubAdditiveClosure_NegativeBufferShift_DoesNotUseTheClosedForm(int bufferShift)
    {
        var curve = new RaisedRateLatencyServiceCurve(rate: 2, latency: 3, bufferShift: bufferShift);

        // the curve is negative at the origin, which Curve.SubAdditiveClosure reports rather than computes
        Assert.True(curve.ValueAt(0) < 0);
        Assert.Throws<InvalidOperationException>(() => curve.SubAdditiveClosure());
    }
}
