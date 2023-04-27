using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class Step
{
    [Theory]
    [InlineData(5, 4)]
    [InlineData(3, 7)]
    public void StepCtor(decimal value, decimal stepTime)
    {
        StepCurve curve = new StepCurve(value: value, stepTime: stepTime);

        Assert.True(curve.IsFinite);
        Assert.False(curve.IsZero);
        Assert.False(curve.IsContinuous);
        Assert.False(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(stepTime, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));

        Assert.Equal(0, curve.LeftLimitAt(stepTime));
        Assert.Equal(0, curve.ValueAt(stepTime));
        Assert.Equal(value, curve.RightLimitAt(stepTime));

        Assert.Equal(value, curve.RightLimitAt(stepTime));
        Assert.Equal(value, curve.RightLimitAt(stepTime + 2));
        Assert.Equal(value, curve.RightLimitAt(stepTime + 5.3m));
        Assert.Equal(value, curve.RightLimitAt(stepTime + 17));
        Assert.Equal(value, curve.RightLimitAt(stepTime +128.3m));
    }
}