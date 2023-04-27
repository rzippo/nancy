using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class RaisedRateLatency
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new (Rational latency, Rational rate, Rational bufferShift)[]
        {
            (0, 5, 0),
            (0, 5, 5),
            (5, 10, 0),
            (5, 10, 8),
            (14.5m, new Rational(20, 3), 4)
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] { testCase.latency, testCase.rate, testCase.bufferShift };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
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
    [MemberData(nameof(GetTestCases))]
    public void RaisedRateLatencyCtor_WithZeroOrigin(Rational latency, Rational rate, Rational bufferShift)
    {
        RaisedRateLatencyServiceCurve curve = new RaisedRateLatencyServiceCurve(rate, latency, bufferShift, withZeroOrigin: true);

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
    [MemberData(nameof(GetTestCases))]
    public void FromSum_ConstantCurve_Closure(Rational delay, Rational rate, Rational bufferShift)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, delay);
        var constantCurve = new ConstantCurve(bufferShift);

        var raisedRateLatency = rateLatency + constantCurve;

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
    [MemberData(nameof(GetTestCases))]
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
}