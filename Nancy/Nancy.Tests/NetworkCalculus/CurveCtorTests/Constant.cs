using System.Collections.Generic;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class Constant
{
    public static List<Rational> ConstantCtorCases =
    [
        5,
        10,
        8,
        14.5m,
        new Rational(20, 3),
        Rational.PlusInfinity
    ];

    public static IEnumerable<object[]> GetConstantCtorCases()
        => ConstantCtorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetConstantCtorCases))]
    public void ConstantCtor(Rational value)
    {
        ConstantCurve curve = new ConstantCurve(value: value);

        Assert.False(curve.IsZero);
        Assert.False(curve.IsContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(value.IsFinite ? curve.IsUltimatelyConstant : !curve.IsUltimatelyConstant);
        Assert.True(curve.IsUltimatelyPlain);
        Assert.Equal(value.IsFinite, curve.IsUltimatelyAffine);
        Assert.Equal(0, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(value, curve.RightLimitAt(0));

        Assert.Equal(value, curve.ValueAt(curve.PseudoPeriodStart));
        Assert.Equal(value, curve.ValueAt(curve.FirstPseudoPeriodEnd));
        Assert.Equal(value, curve.ValueAt(curve.SecondPseudoPeriodEnd));
        Assert.Equal(value, curve.ValueAt(6));
        Assert.Equal(value, curve.ValueAt(12));
        Assert.Equal(value, curve.ValueAt(17));
        Assert.Equal(value, curve.ValueAt(128.3m));
    }

    [Fact]
    public void ZeroCurve()
    {
        ConstantCurve curve = new ConstantCurve(value: 0);

        Assert.True(curve.IsZero);
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsRightContinuous);
        Assert.True(curve.IsContinuousExceptOrigin);
        Assert.True(curve.IsLeftContinuous);
        Assert.Equal(Rational.PlusInfinity, curve.FirstNonZeroTime);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(0, curve.RightLimitAt(0));

        Assert.Equal(0, curve.ValueAt(curve.PseudoPeriodStart));
        Assert.Equal(0, curve.ValueAt(curve.FirstPseudoPeriodEnd));
        Assert.Equal(0, curve.ValueAt(curve.SecondPseudoPeriodEnd));
        Assert.Equal(0, curve.ValueAt(6));
        Assert.Equal(0, curve.ValueAt(12));
        Assert.Equal(0, curve.ValueAt(17));
        Assert.Equal(0, curve.ValueAt(128.3m));

        var shifted = curve.VerticalShift(3);
        Assert.True(shifted.IsUltimatelyConstant);
        shifted = curve.Optimize().VerticalShift(3);
        Assert.True(shifted.IsUltimatelyConstant);
    }
}
