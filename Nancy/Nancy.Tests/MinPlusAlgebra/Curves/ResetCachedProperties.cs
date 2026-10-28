using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ResetCachedProperties
{
    // The constants are copied to a plain Curve: a ConstantCurve knows its sub-additivity from its value, which a reset keeps.
    public static List<Curve> Curves =
    [
        new Curve(Curve.Zero()),
        new Curve(Curve.PlusInfinite()),
        new RateLatencyServiceCurve(2, 3),
        new SigmaRhoArrivalCurve(4, 1),
        new DelayServiceCurve(3),
        new StairCurve(2, 3),
        -new RateLatencyServiceCurve(2, 3)
    ];

    public static IEnumerable<object[]> CurveCases()
        => Curves.ToXUnitTestCases();

    private static bool[] ReadProperties(Curve value) =>
    [
        value.IsFinite,
        value.IsZero,
        value.IsPlusInfinite,
        value.IsMinusInfinite,
        value.HasPlusInfinity,
        value.HasMinusInfinity,
        value.IsPassingThroughOrigin,
        value.IsContinuous,
        value.IsContinuousExceptOrigin,
        value.IsLeftContinuous,
        value.IsRightContinuous,
        value.IsNonNegative,
        value.IsNonPositive,
        value.IsPositive,
        value.IsNegative,
        value.IsConstant,
        value.IsAffine,
        value.IsNonDecreasing,
        value.IsIncreasing,
        value.IsNonIncreasing,
        value.IsDecreasing,
        value.IsUltimatelyFinite,
        value.IsUltimatelyPlusInfinite,
        value.IsUltimatelyMinusInfinite,
        value.IsUltimatelyInfinite,
        value.IsUltimatelyPlain,
        value.IsPlain,
        value.IsUltimatelyAffine,
        value.IsUltimatelyConstant,
        value.IsSubAdditive,
        value.IsRegularSubAdditive,
        value.IsSuperAdditive,
        value.IsRegularSuperAdditive,
        value.IsConcave,
        value.IsRegularConcave,
        value.IsConvex,
        value.IsRegularConvex,
        value.HasTransient
    ];

    private static bool? Cached(object? value) => value switch
    {
        Lazy<bool> { IsValueCreated: true } cache => cache.Value,
        bool known => known,
        _ => null
    };

    private static bool?[] CachedFields(Curve c) =>
    [
        Cached(c._isFinite),
        Cached(c._isZero),
        Cached(c._isPlusInfinite),
        Cached(c._isMinusInfinite),
        Cached(c._hasPlusInfinity),
        Cached(c._hasMinusInfinity),
        Cached(c._isPassingThroughOrigin),
        Cached(c._isContinuous),
        Cached(c._isContinuousExceptOrigin),
        Cached(c._isLeftContinuous),
        Cached(c._isRightContinuous),
        Cached(c._isNonNegative),
        Cached(c._isNonPositive),
        Cached(c._isPositive),
        Cached(c._isNegative),
        Cached(c._isConstant),
        Cached(c._isAffine),
        Cached(c._isNonDecreasing),
        Cached(c._isIncreasing),
        Cached(c._isNonIncreasing),
        Cached(c._isDecreasing),
        Cached(c._isUltimatelyFinite),
        Cached(c._isUltimatelyPlusInfinite),
        Cached(c._isUltimatelyMinusInfinite),
        Cached(c._isUltimatelyInfinite),
        Cached(c._isUltimatelyPlain),
        Cached(c._isPlain),
        Cached(c._isUltimatelyAffine),
        Cached(c._isUltimatelyConstant),
        Cached(c._IsSubAdditive),
        Cached(c._isRegularSubAdditive),
        Cached(c._IsSuperAdditive),
        Cached(c._isRegularSuperAdditive),
        Cached(c._isConcave),
        Cached(c._isRegularConcave),
        Cached(c._isConvex),
        Cached(c._isRegularConvex),
        Cached(c._hasTransient)
    ];

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void EveryCachedPropertyIsForgotten(Curve curve)
    {
        curve = new Curve(curve);
        ReadProperties(curve);
        curve.ResetCachedProperties();

        Assert.All(CachedFields(curve), Assert.Null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void EveryPropertyReportsWhatItDidBefore(Curve curve)
    {
        curve = new Curve(curve);
        var before = ReadProperties(curve);
        Assert.Equal(before, ReadProperties(curve));
        var copy = new Curve(curve);

        curve.ResetCachedProperties();

        Assert.Equal(before, ReadProperties(curve));
        Assert.True(Curve.Equivalent(copy, curve));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutAfterTheResetCarriesNoCachedProperty(Curve curve)
    {
        curve = new Curve(curve);
        ReadProperties(curve);
        curve.ResetCachedProperties();

        var cut = curve.Cut(0, 10);

        Assert.Null(cut._isNonDecreasing);
        Assert.Null(cut._isIncreasing);
        Assert.Null(cut._isNonIncreasing);
        Assert.Null(cut._isDecreasing);
        Assert.Null(cut._isNonNegative);
        Assert.Null(cut._isNonPositive);
        Assert.Null(cut._isPositive);
        Assert.Null(cut._isNegative);
        Assert.Null(cut._isLeftContinuous);
        Assert.Null(cut._isRightContinuous);
    }

    [Fact]
    public void ACurveKeepsASubAdditivityKnownByItsType()
    {
        var curve = new SubAdditiveCurve(new SigmaRhoArrivalCurve(4, 1));
        curve.ResetCachedProperties();

        Assert.True(curve.IsSubAdditive);
    }

    public static IEnumerable<object[]> ConstantCases()
        => new List<(Rational value, bool? known)>
        {
            (3, true),
            (0, true),
            (Rational.PlusInfinity, true),
            (Rational.MinusInfinity, true),
            (-3, null)
        }.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ConstantCases))]
    public void AConstantCurveKeepsTheSubAdditivityItsValueImplies(Rational value, bool? known)
    {
        var curve = new ConstantCurve(value);
        _ = curve.IsSubAdditive;
        curve.ResetCachedProperties();

        Assert.Equal(known, Cached(curve._IsSubAdditive));
    }
}
