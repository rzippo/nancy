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

    private static object[] ReadProperties(Curve c) =>
    [
        c.HasPlusInfinity, c.HasMinusInfinity, c.IsPassingThroughOrigin,
        c.IsLeftContinuous, c.IsRightContinuous, c.IsNonNegative,
        c.IsNonDecreasing, c.IsIncreasing, c.IsSubAdditive, c.IsSuperAdditive,
        c.BaseSequence.IsLeftContinuous, c.BaseSequence.IsRightContinuous,
        c.BaseSequence.IsNonDecreasing, c.BaseSequence.IsIncreasing
    ];

    private static bool?[] CachedFields(Curve c) =>
    [
        c._hasPlusInfinity, c._hasMinusInfinity, c._isPassingThroughOrigin,
        c._isLeftContinuous, c._isRightContinuous, c._isNonNegative,
        c._isNonDecreasing, c._isIncreasing, c._IsSubAdditive, c._IsSuperAdditive,
        c.BaseSequence._isLeftContinuous, c.BaseSequence._isRightContinuous,
        c.BaseSequence._isNonDecreasing, c.BaseSequence._isIncreasing
    ];

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void EveryCachedPropertyIsForgotten(Curve curve)
    {
        ReadProperties(curve);
        curve.ResetCachedProperties();

        Assert.All(CachedFields(curve), Assert.Null);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void EveryPropertyReportsWhatItDidBefore(Curve curve)
    {
        var before = ReadProperties(curve);
        var copy = new Curve(curve);

        curve.ResetCachedProperties();

        Assert.Equal(before, ReadProperties(curve));
        Assert.True(Curve.Equivalent(copy, curve));
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutAfterTheResetCarriesNoCachedProperty(Curve curve)
    {
        ReadProperties(curve);
        curve.ResetCachedProperties();

        var cut = curve.Cut(0, 10);

        Assert.Null(cut._isNonDecreasing);
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

        Assert.Equal(known, curve._IsSubAdditive);
    }
}
