using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
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

    [Theory]
    [InlineData(-3)]
    [InlineData(-1)]
    public void NegativeConstantCurve_IsNotSubAdditive(Rational value)
    {
        ConstantCurve curve = new ConstantCurve(value: value);

        Assert.False(curve.IsSubAdditive);
        Assert.False(curve.IsRegularSubAdditive);
    }

    [Theory]
    [InlineData(-3)]
    [InlineData(-1)]
    public void NegativeConstantCurve_ConvolvesByTheDefinition(Rational value)
    {
        // the shortcut of a sub-additive curve would decide this through the minimum, giving value
        ConstantCurve curve = new ConstantCurve(value: value);

        var selfConvolution = Curve.Convolution(curve, curve);

        Assert.Equal(0, selfConvolution.ValueAt(0));
        Assert.Equal(2 * value, selfConvolution.ValueAt(1));
        Assert.Equal(2 * value, selfConvolution.ValueAt(17));
    }

    [Theory]
    [MemberData(nameof(GetConstantCtorCases))]
    public void NonNegativeConstantCurve_IsItsOwnSubAdditiveClosure(Rational value)
    {
        ConstantCurve curve = new ConstantCurve(value: value);

        Assert.True(curve.IsSubAdditive);
        Assert.True(curve.IsRegularSubAdditive);
        Assert.True(Curve.Equivalent(curve, curve.SubAdditiveClosure()));
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

    [Fact]
    public void MinusInfiniteCurve()
    {
        ConstantCurve curve = new ConstantCurve(value: Rational.MinusInfinity);

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(Rational.MinusInfinity, curve.RightLimitAt(0));
        Assert.Equal(Rational.MinusInfinity, curve.ValueAt(curve.PseudoPeriodStart));
        Assert.Equal(Rational.MinusInfinity, curve.ValueAt(curve.SecondPseudoPeriodEnd));
        Assert.Equal(Rational.MinusInfinity, curve.ValueAt(17));

        Assert.True(curve.IsPlain);
        Assert.True(Curve.Equivalent(curve, Curve.MinusInfinite().WithOriginAt(0)));
    }

    [Fact]
    public void MinusInfiniteCurve_IsItsOwnSubAdditiveClosure()
    {
        // at -infinity both f(t+s) and f(t) + f(s) are -infinity, so the property holds
        ConstantCurve curve = new ConstantCurve(value: Rational.MinusInfinity);

        Assert.True(curve.IsSubAdditive);
        Assert.True(curve.IsRegularSubAdditive);
        Assert.True(Curve.Equivalent(curve, curve.SubAdditiveClosure()));
    }

    [Fact]
    public void VerticalShift_DefaultDoesNotDependOnTheDeclaredType()
    {
        ConstantCurve curve = new ConstantCurve(value: 5);

        var throughType = curve.VerticalShift(3);
        var throughCurve = ((Curve)curve).VerticalShift(3);

        // the origin is shifted either way, as Curve declares
        Assert.Equal(3, throughType.ValueAt(0));
        Assert.Equal(8, throughType.ValueAt(1));
        Assert.True(Curve.Equivalent(throughType, throughCurve));
    }

    [Fact]
    public void VerticalShift_ExceptOriginKeepsTheType()
    {
        ConstantCurve curve = new ConstantCurve(value: 5);

        var shifted = curve.VerticalShift(3, exceptOrigin: true);

        Assert.Equal(0, shifted.ValueAt(0));
        Assert.Equal(8, shifted.ValueAt(1));
        Assert.IsType<ConstantCurve>(shifted);
    }
}
