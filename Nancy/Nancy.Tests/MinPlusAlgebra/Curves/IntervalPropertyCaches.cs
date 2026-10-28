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

    // f(t) = slope * t
    private static Curve Ramp(Rational slope)
        => new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, slope)]), 0, 1, slope);

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

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void IncreasingRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Ramp(1);
        Assert.Throws<ArgumentException>(() => curve.IsIncreasingOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsIncreasing);
        Assert.Throws<ArgumentException>(() => curve.IsIncreasingOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void IncreasingAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Ramp(1);
        Assert.True(curve.IsIncreasingOverInterval(time, time, true, true));
        Assert.True(curve.IsIncreasing);
        Assert.True(curve.IsIncreasingOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void NonIncreasingRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsNonIncreasingOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsNonIncreasing);
        Assert.Throws<ArgumentException>(() => curve.IsNonIncreasingOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void NonIncreasingAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsNonIncreasingOverInterval(time, time, true, true));
        Assert.True(curve.IsNonIncreasing);
        Assert.True(curve.IsNonIncreasingOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void DecreasingRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Ramp(-1);
        Assert.Throws<ArgumentException>(() => curve.IsDecreasingOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsDecreasing);
        Assert.Throws<ArgumentException>(() => curve.IsDecreasingOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void DecreasingAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Ramp(-1);
        Assert.True(curve.IsDecreasingOverInterval(time, time, true, true));
        Assert.True(curve.IsDecreasing);
        Assert.True(curve.IsDecreasingOverInterval(time, time, true, true));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervalCases))]
    public void ContinuousRejectsInvalidIntervalsWithColdAndWarmCaches(
        Rational start, Rational end, bool startIncluded, bool endIncluded)
    {
        var curve = Curve.Zero();
        Assert.Throws<ArgumentException>(() => curve.IsContinuousOverInterval(start, end, startIncluded, endIncluded));
        Assert.True(curve.IsContinuous);
        Assert.Throws<ArgumentException>(() => curve.IsContinuousOverInterval(start, end, startIncluded, endIncluded));
    }

    [Theory]
    [MemberData(nameof(SingletonCases))]
    public void ContinuousAcceptsClosedSingletonsWithColdAndWarmCaches(Rational time)
    {
        var curve = Curve.Zero();
        Assert.True(curve.IsContinuousOverInterval(time, time, true, true));
        Assert.True(curve.IsContinuous);
        Assert.True(curve.IsContinuousOverInterval(time, time, true, true));
    }

}
