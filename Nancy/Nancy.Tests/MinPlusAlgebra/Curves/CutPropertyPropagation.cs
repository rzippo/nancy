using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// A cut of a curve keeps each predicate the curve is known to satisfy everywhere, and nothing else.
/// </summary>
public class CutPropertyPropagation
{
    private static Curve Ramp(Rational offset, Rational slope)
        => new Curve(new Sequence([new Point(0, offset), new Segment(0, 1, offset, slope)]), 0, 1, slope);

    // f(t) = t, f(t) = t + 1, f(t) = -t, f(t) = -t - 1
    public static List<Curve> Curves =
    [
        Ramp(0, 1),
        Ramp(1, 1),
        Ramp(0, -1),
        Ramp(-1, -1)
    ];

    public static IEnumerable<object[]> CurveCases()
        => Curves.ToXUnitTestCases();

    /// <summary>Initializes the predicates that, true of a curve, are true of any of its cuts.</summary>
    private static Curve WithKnownPredicates(Curve curve)
    {
        curve = new Curve(curve);
        _ = curve.IsNonDecreasing;
        _ = curve.IsIncreasing;
        _ = curve.IsNonIncreasing;
        _ = curve.IsDecreasing;
        _ = curve.IsNonNegative;
        _ = curve.IsNonPositive;
        _ = curve.IsPositive;
        _ = curve.IsNegative;
        _ = curve.IsLeftContinuous;
        _ = curve.IsRightContinuous;
        return curve;
    }

    private static void AssertInherited(bool known, Lazy<bool>? cached)
    {
        if (known)
            Assert.True(cached is { IsValueCreated: true, Value: true });
        else
            Assert.False(cached is { IsValueCreated: true });
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsNonDecreasingWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsNonDecreasing, cut._isNonDecreasing);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsNonDecreasing, cut.IsNonDecreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsIncreasingWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsIncreasing, cut._isIncreasing);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsIncreasing, cut.IsIncreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsNonIncreasingWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsNonIncreasing, cut._isNonIncreasing);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsNonIncreasing, cut.IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsDecreasingWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsDecreasing, cut._isDecreasing);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsDecreasing, cut.IsDecreasing);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsNonNegativeWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsNonNegative, cut._isNonNegative);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsNonNegative, cut.IsNonNegative);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsNonPositiveWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsNonPositive, cut._isNonPositive);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsNonPositive, cut.IsNonPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsPositiveWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsPositive, cut._isPositive);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsPositive, cut.IsPositive);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsNegativeWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsNegative, cut._isNegative);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsNegative, cut.IsNegative);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsLeftContinuousWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsLeftContinuous, cut._isLeftContinuous);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsLeftContinuous, cut.IsLeftContinuous);
    }

    [Theory]
    [MemberData(nameof(CurveCases))]
    public void ACutInheritsIsRightContinuousWhenKnownTrue(Curve curve)
    {
        curve = WithKnownPredicates(curve);
        var cut = curve.Cut(0, 3);
        AssertInherited(curve.IsRightContinuous, cut._isRightContinuous);
        var fresh = new Sequence(cut.Elements);
        Assert.Equal(fresh.IsRightContinuous, cut.IsRightContinuous);
    }
}
