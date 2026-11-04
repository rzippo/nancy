using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ExtremaOverInterval
{
    public record Case(Curve Curve, Rational Start, Rational End, bool LeftClosed, bool RightClosed,
        (Rational Value, Rational Time) Sup, (Rational Value, Rational Time) Inf, Rational? Max, Rational? Min);

    private static Curve Affine(Rational intercept, Rational slope)
        => new(new Sequence([new Point(0, intercept), new Segment(0, 1, intercept, slope)]), 0, 1, slope);

    public static IEnumerable<Case> Cases()
    {
        foreach (var left in new[] { false, true })
        foreach (var right in new[] { false, true })
        {
            yield return new(Affine(7, 2), 2, 4, left, right, (15, 4), (11, 2), right ? 15 : null, left ? 11 : null);
            yield return new(Affine(7, -2), 2, 4, left, right, (3, 2), (-1, 4), left ? 3 : null, right ? -1 : null);
            yield return new(Affine(5, 0), 2, 4, left, right, (5, 2), (5, 2), 5, 5);
            yield return new(Curve.PlusInfinite(), 2, 4, left, right,
                (Rational.PlusInfinity, 2), (Rational.PlusInfinity, 2), Rational.PlusInfinity, Rational.PlusInfinity);
            yield return new(Curve.MinusInfinite(), 2, 4, left, right,
                (Rational.MinusInfinity, 2), (Rational.MinusInfinity, 2), Rational.MinusInfinity, Rational.MinusInfinity);
        }
        foreach (var time in new Rational[] { 0, 2, 4 })
        {
            var value = 7 + 2 * time;
            yield return new(Affine(7, 2), time, time, true, true, (value, time), (value, time), value, value);
        }

        var periodic = new Curve(new Sequence([
            new Point(0, 3), Segment.Constant(0, 1, 1), new Point(1, 5), Segment.Constant(1, 2, 1)
        ]), 0, 2, 0);
        yield return new(periodic, 2, 6, false, false, (5, 3), (1, 2), 5, 1);
        yield return new(periodic, 3, 5, true, true, (5, 3), (1, 3), 5, 1);
        yield return new(periodic, 3, 5, false, false, (3, 4), (1, 3), 3, 1);
        yield return new(periodic, 3, 5, true, false, (5, 3), (1, 3), 5, 1);

        var infinitePoints = new Curve(new Sequence([
            Point.Origin(), Segment.Zero(0, 1), Point.PlusInfinite(1), Segment.Zero(1, 2),
            Point.MinusInfinite(2), Segment.Zero(2, 3), Point.Zero(3), Segment.Zero(3, 4)
        ]), 3, 1, 0);
        yield return new(infinitePoints, 0, 3, true, false, (Rational.PlusInfinity, 1), (Rational.MinusInfinity, 2), Rational.PlusInfinity, Rational.MinusInfinity);
        yield return new(infinitePoints, 1, 2, false, false, (0, 1), (0, 1), 0, 0);
        yield return new(infinitePoints, 1, 2, true, false, (Rational.PlusInfinity, 1), (0, 1), Rational.PlusInfinity, 0);
        yield return new(infinitePoints, 1, 2, false, true, (0, 1), (Rational.MinusInfinity, 2), 0, Rational.MinusInfinity);

        // The supremum is a left limit at an interior breakpoint, not an attained value.
        var limit = new Curve(new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 3), Point.Zero(1), new Segment(1, 2, 0, 1), Point.Zero(2), Segment.Zero(2, 3)
        ]), 2, 1, 0);
        yield return new(limit, 0, 2, true, false, (3, 1), (0, 0), null, 0);
        yield return new(-limit, 0, 2, true, false, (0, 0), (-3, 1), 0, null);
    }

    public static IEnumerable<ComputationSettings?> Settings => [null, new() { UseParallelism = false }];

    public static IEnumerable<object[]> SupCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Sup, s)))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> InfCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Inf, s)))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> SupArgCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Sup.Time, s)))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> InfArgCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Inf.Time, s)))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> MaxCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Max, s)))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> MinCases() => Cases()
        .SelectMany(c => Settings.Select(s => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Min, s)))
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SupCases))]
    public void SupValueAndArgReturnsTheBoundAndEarliestBoundary(Curve curve, Rational start, Rational end, bool left, bool right, (Rational Value, Rational Time) expected, ComputationSettings? settings)
        => Assert.Equal(expected, curve.SupValueAndArgOverInterval(start, end, left, right, settings));

    [Theory]
    [MemberData(nameof(InfCases))]
    public void InfValueAndArgReturnsTheBoundAndEarliestBoundary(Curve curve, Rational start, Rational end, bool left, bool right, (Rational Value, Rational Time) expected, ComputationSettings? settings)
        => Assert.Equal(expected, curve.InfValueAndArgOverInterval(start, end, left, right, settings));

    [Theory]
    [MemberData(nameof(SupArgCases))]
    public void SupArgReturnsTheEarliestBoundary(Curve curve, Rational start, Rational end, bool left, bool right, Rational expected, ComputationSettings? settings)
        => Assert.Equal(expected, curve.SupArgOverInterval(start, end, left, right, settings));

    [Theory]
    [MemberData(nameof(InfArgCases))]
    public void InfArgReturnsTheEarliestBoundary(Curve curve, Rational start, Rational end, bool left, bool right, Rational expected, ComputationSettings? settings)
        => Assert.Equal(expected, curve.InfArgOverInterval(start, end, left, right, settings));

    [Theory]
    [MemberData(nameof(MaxCases))]
    public void MaxArgReturnsAnAttainingTimeOrNull(Curve curve, Rational start, Rational end, bool left, bool right, Rational? expectedValue, ComputationSettings? settings)
    {
        var time = curve.MaxArgOverInterval(start, end, left, right, settings);
        AssertAttainment(curve, start, end, left, right, expectedValue, time);
    }

    [Theory]
    [MemberData(nameof(MinCases))]
    public void MinArgReturnsAnAttainingTimeOrNull(Curve curve, Rational start, Rational end, bool left, bool right, Rational? expectedValue, ComputationSettings? settings)
    {
        var time = curve.MinArgOverInterval(start, end, left, right, settings);
        AssertAttainment(curve, start, end, left, right, expectedValue, time);
    }

    private static void AssertAttainment(Curve curve, Rational start, Rational end, bool left, bool right, Rational? expectedValue, Rational? time)
    {
        if (expectedValue is null)
        {
            Assert.Null(time);
            return;
        }
        Assert.NotNull(time);
        Assert.True(time > start || left && time == start);
        Assert.True(time < end || right && time == end);
        Assert.Equal(expectedValue.Value, curve.ValueAt(time.Value));
    }

    public static IEnumerable<(Rational Start, Rational End, bool Left, bool Right)> InvalidIntervals()
    {
        foreach (var interval in IntervalPropertyCaches.InvalidIntervals)
            yield return interval;
        yield return (Rational.MinusInfinity, 0, true, false);
        yield return (0, Rational.PlusInfinity, true, false);
        yield return (0, Rational.MinusInfinity, true, false);
        yield return (Rational.MinusInfinity, Rational.PlusInfinity, false, false);
    }

    public static IEnumerable<object[]> InvalidCases() => InvalidIntervals().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void SupArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().SupArgOverInterval(start, end, left, right));

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void InfArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().InfArgOverInterval(start, end, left, right));

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void SupValueAndArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().SupValueAndArgOverInterval(start, end, left, right));

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void InfValueAndArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().InfValueAndArgOverInterval(start, end, left, right));

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void MaxArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().MaxArgOverInterval(start, end, left, right));

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void MinArgRejectsInvalidOrInfiniteIntervals(Rational start, Rational end, bool left, bool right)
        => Assert.Throws<ArgumentException>(() => Curve.Zero().MinArgOverInterval(start, end, left, right));
}
