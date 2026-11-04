using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class SignPredicatesOverInterval
{
    private static Curve Affine(Rational intercept, Rational slope)
        => new(new Sequence([new Point(0, intercept), new Segment(0, 1, intercept, slope)]), 0, 1, slope);

    private static Curve InfinitePeriod(Rational value, Rational height)
        => new(new Sequence([new Point(0, value), Segment.Constant(0, 1, value)]), 0, 1, height);

    public static IEnumerable<(Curve Curve, Rational Start, Rational? End, bool LeftClosed, bool RightClosed,
        bool NonPositive, bool Positive, bool Negative)> Cases()
    {
        // f(t) = t - 2, with its only zero at t = 2.
        yield return (Affine(-2, 1), 0, 2, true, true, true, false, false);
        yield return (Affine(-2, 1), 0, 2, true, false, true, false, true);
        yield return (Affine(-2, 1), 0, 3, true, false, false, false, false);
        yield return (Affine(-2, 1), 2, 3, false, true, false, true, false);
        yield return (Affine(-2, 1), 2, null, false, false, false, true, false);
        yield return (Affine(-2, 1), 2, null, true, false, false, false, false);
        yield return (Affine(-2, 1), 3, null, true, false, false, true, false);
        yield return (Affine(-2, 1), 2, 2, true, true, true, false, false);
        yield return (Affine(-2, 1), 1, 1, true, true, true, false, true);
        yield return (Affine(-2, 1), 3, 3, true, true, false, true, false);
        // f(t) = 2 - t, including an unattained zero bound on either side.
        yield return (Affine(2, -1), 0, 2, true, false, false, true, false);
        yield return (Affine(2, -1), 0, 2, true, true, false, false, false);
        yield return (Affine(2, -1), 2, null, false, false, true, false, true);
        yield return (Affine(2, -1), 2, null, true, false, true, false, false);
        yield return (Affine(2, -1), 3, null, true, false, true, false, true);
        // Violations lie well beyond the first period inspected after the start.
        yield return (Affine(-100, 1), 2, null, true, false, false, false, false);
        yield return (Affine(100, -1), 2, null, true, false, false, false, false);
        yield return (Affine(0, 0), 2, null, false, true, true, false, false);
        yield return (Affine(1, 0), 2, null, false, true, false, true, false);
        yield return (Affine(-1, 0), 2, null, false, true, true, false, true);

        foreach (var height in new Rational[] { -1, 0, 1 })
        foreach (var start in new Rational[] { 0, 3 })
        {
            yield return (InfinitePeriod(Rational.PlusInfinity, height), start, null, true, false, false, true, false);
            yield return (InfinitePeriod(Rational.MinusInfinity, height), start, null, true, false, true, false, true);
        }

        var infiniteGain = new Curve(new Sequence([new Point(0, 1), Segment.Constant(0, 1, 1)]), 0, 1, Rational.PlusInfinity);
        yield return (infiniteGain, 0, null, true, false, false, true, false);
        yield return (infiniteGain, 3, Rational.PlusInfinity, false, true, false, true, false);
        yield return (-infiniteGain, 0, null, true, false, true, false, true);
        yield return (-infiniteGain, 3, Rational.PlusInfinity, false, true, true, false, true);

        // A periodic zero is excluded at the start but reappears one period later.
        var saw = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 0);
        yield return (saw, 0, 1, false, false, false, true, false);
        yield return (saw, 0, 1, false, true, false, false, false);
        yield return (saw, 0, null, false, false, false, false, false);
        yield return (-saw, 0, 1, false, false, true, false, true);
        yield return (-saw, 0, 1, false, true, true, false, false);
        yield return (-saw, 0, null, false, false, true, false, false);

        var isolatedZero = new Curve(new Sequence([
            new Point(0, 1), Segment.Constant(0, 1, 1), Point.Zero(1),
            Segment.Constant(1, 2, 1), new Point(2, 1), Segment.Constant(2, 3, 1)
        ]), 2, 1, 0);
        yield return (isolatedZero, 0, 2, true, false, false, false, false);
        yield return (isolatedZero, 1, null, false, false, false, true, false);
        yield return (-isolatedZero, 0, 2, true, false, true, false, false);
        yield return (-isolatedZero, 1, null, false, false, true, false, true);

        // Adverse height is harmless on an infinite period, but not on its transient.
        var transient = new Curve(new Sequence([
            new Point(0, -1), Segment.Constant(0, 1, -1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
        ]), 1, 1, -1);
        yield return (transient, 0, null, true, false, false, false, false);
        yield return (transient, new Rational(1, 2), null, true, false, false, false, false);
        yield return (transient, 1, null, true, false, false, true, false);
        yield return (-transient, 0, null, true, false, false, false, false);
        yield return (-transient, new Rational(1, 2), null, true, false, false, false, false);
        yield return (-transient, 1, null, true, false, true, false, true);

        var mixed = new Curve(new Sequence([Point.MinusInfinite(0), Segment.PlusInfinite(0, 1)]), 0, 1, -1);
        yield return (mixed, 0, 1, false, false, false, true, false);
        yield return (mixed, 0, null, false, false, false, false, false);
        yield return (-mixed, 0, 1, false, false, true, false, true);
        yield return (-mixed, 0, null, false, false, false, false, false);

        // A finite point amid otherwise infinite values eventually changes sign.
        var finitePoint = new Curve(new Sequence([new Point(0, 100), Segment.PlusInfinite(0, 1)]), 0, 1, -1);
        yield return (finitePoint, new Rational(1, 2), null, false, false, false, false, false);
        yield return (-finitePoint, new Rational(1, 2), null, false, false, false, false, false);
    }

    public static IEnumerable<object[]> NonPositiveCases() => Cases()
        .Select(c => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.NonPositive))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> PositiveCases() => Cases()
        .Select(c => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Positive))
        .ToXUnitTestCases();

    public static IEnumerable<object[]> NegativeCases() => Cases()
        .Select(c => (c.Curve, c.Start, c.End, c.LeftClosed, c.RightClosed, c.Negative))
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NonPositiveCases))]
    public void NonPositiveRespectsTheIntervalWithColdAndWarmCaches(Curve curve, Rational start, Rational? end, bool leftClosed, bool rightClosed, bool expected)
    {
        curve = new Curve(curve.BaseSequence, curve.PseudoPeriodStart, curve.PseudoPeriodLength, curve.PseudoPeriodHeight);
        Assert.Equal(expected, curve.IsNonPositiveOverInterval(start, end, leftClosed, rightClosed));
        _ = curve.IsNonPositive;
        Assert.Equal(expected, curve.IsNonPositiveOverInterval(start, end, leftClosed, rightClosed));
    }

    [Theory]
    [MemberData(nameof(PositiveCases))]
    public void PositiveRespectsTheIntervalWithColdAndWarmCaches(Curve curve, Rational start, Rational? end, bool leftClosed, bool rightClosed, bool expected)
    {
        curve = new Curve(curve.BaseSequence, curve.PseudoPeriodStart, curve.PseudoPeriodLength, curve.PseudoPeriodHeight);
        Assert.Equal(expected, curve.IsPositiveOverInterval(start, end, leftClosed, rightClosed));
        _ = curve.IsPositive;
        Assert.Equal(expected, curve.IsPositiveOverInterval(start, end, leftClosed, rightClosed));
    }

    [Theory]
    [MemberData(nameof(NegativeCases))]
    public void NegativeRespectsTheIntervalWithColdAndWarmCaches(Curve curve, Rational start, Rational? end, bool leftClosed, bool rightClosed, bool expected)
    {
        curve = new Curve(curve.BaseSequence, curve.PseudoPeriodStart, curve.PseudoPeriodLength, curve.PseudoPeriodHeight);
        Assert.Equal(expected, curve.IsNegativeOverInterval(start, end, leftClosed, rightClosed));
        _ = curve.IsNegative;
        Assert.Equal(expected, curve.IsNegativeOverInterval(start, end, leftClosed, rightClosed));
    }

    public static IEnumerable<object[]> InvalidIntervals() => IntervalPropertyCaches.InvalidIntervals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidIntervals))]
    public void NonPositiveRejectsInvalidIntervalsWithColdAndWarmCaches(Rational start, Rational end, bool leftClosed, bool rightClosed)
    {
        var curve = Affine(-1, 0);
        Assert.Throws<ArgumentException>(() => curve.IsNonPositiveOverInterval(start, end, leftClosed, rightClosed));
        Assert.True(curve.IsNonPositive);
        Assert.Throws<ArgumentException>(() => curve.IsNonPositiveOverInterval(start, end, leftClosed, rightClosed));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervals))]
    public void PositiveRejectsInvalidIntervalsWithColdAndWarmCaches(Rational start, Rational end, bool leftClosed, bool rightClosed)
    {
        var curve = Affine(1, 0);
        Assert.Throws<ArgumentException>(() => curve.IsPositiveOverInterval(start, end, leftClosed, rightClosed));
        Assert.True(curve.IsPositive);
        Assert.Throws<ArgumentException>(() => curve.IsPositiveOverInterval(start, end, leftClosed, rightClosed));
    }

    [Theory]
    [MemberData(nameof(InvalidIntervals))]
    public void NegativeRejectsInvalidIntervalsWithColdAndWarmCaches(Rational start, Rational end, bool leftClosed, bool rightClosed)
    {
        var curve = Affine(-1, 0);
        Assert.Throws<ArgumentException>(() => curve.IsNegativeOverInterval(start, end, leftClosed, rightClosed));
        Assert.True(curve.IsNegative);
        Assert.Throws<ArgumentException>(() => curve.IsNegativeOverInterval(start, end, leftClosed, rightClosed));
    }
}
