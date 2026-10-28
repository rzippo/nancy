using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class IntervalPropertyCaches
{
    public static IEnumerable<(Rational Start, Rational End, bool StartIncluded, bool EndIncluded)> InvalidIntervals =>
    [
        (0, 0, false, false),
        (0, 0, false, true),
        (0, 0, true, false),
        (1, 1, false, false),
        (1, 1, false, true),
        (1, 1, true, false),
        (-1, 1, true, true),
        (2, 1, true, true),
        (Rational.PlusInfinity, Rational.PlusInfinity, true, true)
    ];

    public static IEnumerable<object[]> InvalidIntervalCases() => InvalidIntervals.ToXUnitTestCases();

    public static IEnumerable<Rational> SingletonTimes => [0, 1];
    public static IEnumerable<object[]> SingletonCases() => SingletonTimes.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void NonNegativeRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsNonNegativeOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsNonNegative);
        Assert.Throws<ArgumentException>(() => curve.IsNonNegativeOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void NonNegativeAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsNonNegativeOverInterval(time, time, true, true));
        Assert.True(curve.IsNonNegative);
        Assert.True(curve.IsNonNegativeOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void NonDecreasingRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsNonDecreasingOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsNonDecreasing);
        Assert.Throws<ArgumentException>(() => curve.IsNonDecreasingOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void NonDecreasingAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsNonDecreasingOverInterval(time, time, true, true));
        Assert.True(curve.IsNonDecreasing);
        Assert.True(curve.IsNonDecreasingOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void LeftContinuousRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsLeftContinuousOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsLeftContinuous);
        Assert.Throws<ArgumentException>(() => curve.IsLeftContinuousOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void LeftContinuousAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsLeftContinuousOverInterval(time, time, true, true));
        Assert.True(curve.IsLeftContinuous);
        Assert.True(curve.IsLeftContinuousOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void RightContinuousRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsRightContinuousOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsRightContinuous);
        Assert.Throws<ArgumentException>(() => curve.IsRightContinuousOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void RightContinuousAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsRightContinuousOverInterval(time, time, true, true));
        Assert.True(curve.IsRightContinuous);
        Assert.True(curve.IsRightContinuousOverInterval(time, time, true, true));
    }

}
