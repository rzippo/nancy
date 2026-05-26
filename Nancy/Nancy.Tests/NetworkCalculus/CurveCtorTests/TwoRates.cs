using System;
using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class TwoRates
{
    public static List<(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)> TwoRatesCtorCases =
    [
        (0, 5, 7, 9),
        (5, 5, 7, 10),
        (14.5m, 6.33m, new Rational(44, 3), new Rational(20, 3)),
        (5, 10, 5, 3)
    ];

    public static IEnumerable<object[]> GetTwoRatesCtorCases()
        => TwoRatesCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetTwoRatesCtorCases))]
    public void TwoRatesCtor(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)
    {
        TwoRatesServiceCurve curve = new TwoRatesServiceCurve(delay, transientRate, transientEnd, steadyRate);

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsRightContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(delay, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(0, curve.ValueAt(delay));
        Assert.Equal(0, curve.RightLimitAt(delay));
        if (delay > 0)
            Assert.Equal(0, curve.LeftLimitAt(delay));

        var midTransientDiff = (transientEnd - delay) * 0.5m;
        Assert.Equal(transientRate * midTransientDiff, curve.ValueAt(delay + midTransientDiff));
        var endTransientValue = transientRate * (transientEnd - delay);
        Assert.Equal(endTransientValue, curve.ValueAt(transientEnd));

        Assert.Equal(endTransientValue + steadyRate, curve.ValueAt(transientEnd + 1));
        Assert.Equal(endTransientValue + 2 * steadyRate, curve.ValueAt(transientEnd + 2));
        Assert.Equal(endTransientValue + 10.5m * steadyRate, curve.ValueAt(transientEnd + 10.5m));
        Assert.Equal(endTransientValue + 110 * steadyRate, curve.ValueAt(transientEnd + 110));

        if (delay == transientEnd)
        {
            Assert.False(curve.IsSubAdditive);
            Assert.True(curve.IsSuperAdditive);
        }
        else if (curve.FirstNonZeroTime > 0)
        {
            Assert.False(curve.IsSubAdditive);
            Assert.Equal(steadyRate > transientRate, curve.IsSuperAdditive);
        }
        else
        {
            Assert.Equal(steadyRate < transientEnd, curve.IsSubAdditive);
            Assert.Equal(steadyRate > transientRate, curve.IsSuperAdditive);
        }
    }

    public static List<(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)> InvalidCtorCases =
    [
        (10, 5, 5, 3)
    ];

    public static IEnumerable<object[]> GetInvalidCtorCases()
        => InvalidCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidCtorCases))]
    public void TwoRatesCtor_InvalidParameters_ThrowsDescriptive(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new TwoRatesServiceCurve(delay, transientRate, transientEnd, steadyRate)
        );
        Assert.Contains("Delay time must precede transient end time", exception.Message);
    }
}
