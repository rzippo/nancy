using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class RateLatency
{
    public static List<(Rational latency, Rational rate)> RateLatencyCtorCases =
    [
        (0, 5),
        (5, 10),
        (14.5m, new Rational(20, 3))
    ];

    public static IEnumerable<object[]> GetRateLatencyCtorCases()
        => RateLatencyCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRateLatencyCtorCases))]
    public void RateLatencyCtor(Rational latency, Rational rate)
    {
        RateLatencyServiceCurve curve = new RateLatencyServiceCurve(rate, latency);

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsRightContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.True(curve.IsUltimatelyAffine);
        Assert.Equal(latency, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(0, curve.ValueAt(latency));
        Assert.Equal(0, curve.RightLimitAt(latency));
        if (latency > 0)
            Assert.Equal(0, curve.LeftLimitAt(latency));

        Assert.Equal(rate, curve.ValueAt(latency + 1));
        Assert.Equal(2 * rate, curve.ValueAt(latency + 2));
        Assert.Equal(10.5m * rate, curve.ValueAt(latency + 10.5m));
        Assert.Equal(110 * rate, curve.ValueAt(latency + 110));

        Assert.Equal(!(curve.FirstNonZeroTime > 0), curve.IsSubAdditive);
        Assert.True(curve.IsSuperAdditive);
    }
}
