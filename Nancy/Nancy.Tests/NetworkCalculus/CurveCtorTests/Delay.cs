using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class Delay
{
    [Fact]
    public void DelayCtor()
    {
        Rational delay = 5;
        DelayServiceCurve curve = new DelayServiceCurve(delay);

        Assert.False(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.False(curve.IsContinuous);
        Assert.False(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.False(curve.IsRightContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.True(curve.IsUltimatelyInfinite);
        Assert.False(curve.IsUltimatelyAffine);
        Assert.True(curve.IsSuperAdditive);
        Assert.Equal(delay, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(0, curve.LeftLimitAt(5));
        Assert.Equal(0, curve.ValueAt(5));

        Assert.Equal(Rational.PlusInfinity, curve.RightLimitAt(5));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(6));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(8));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(15));
    }

    [Fact]
    public void ZeroCtor()
    {
        DelayServiceCurve curve = new DelayServiceCurve(0);

        Assert.False(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.False(curve.IsContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.False(curve.IsRightContinuous);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.True(curve.IsUltimatelyInfinite);
        Assert.False(curve.IsUltimatelyAffine);
        Assert.True(curve.IsSuperAdditive);
        Assert.Equal(0, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));

        Assert.Equal(Rational.PlusInfinity, curve.RightLimitAt(0));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(6));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(8));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(15));
    }
}