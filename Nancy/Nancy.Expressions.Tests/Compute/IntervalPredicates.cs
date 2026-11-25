using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// The predicates over an interval of curve expressions agree with the library, on a leaf and on an operation, over bounded and unbounded intervals.
/// </summary>
public class IntervalPredicates
{
    // f(t) = 3 - t: decreasing, positive before 3.
    private static readonly Curve Descending =
        new(new Sequence([new Point(0, 3), new Segment(0, 1, 3, -1)]), 0, 1, -1);

    // f(t) = 3 from the origin included, so strictly positive everywhere.
    private static readonly Curve Three =
        new(new Sequence([new Point(0, 3), new Segment(0, 1, 3, 0)]), 0, 1, 0);

    private static readonly Curve RightContinuousJump = new(new Sequence([
        Point.Origin(), new Segment(0, 1, 0, 0),
        new Point(1, 1), new Segment(1, 2, 1, 0)
    ]), 1, 1, 0);

    private static readonly List<(string Name, Curve Curve)> Curves =
    [
        ("three", Three),
        ("rate-latency", new RateLatencyServiceCurve(2, 3)),
        ("token bucket", new SigmaRhoArrivalCurve(2, 1)),
        ("descending", Descending),
        ("right-continuous jump", RightContinuousJump),
    ];

    private static readonly List<(string Name, Rational Start, Rational? End, bool IsStartIncluded, bool IsEndIncluded)> Intervals =
    [
        ("[0, 2]", 0, 2, true, true),
        ("]0, 2[", 0, 2, false, false),
        ("[1, 5[", 1, 5, true, false),
        ("[4, +inf[", 4, null, true, false),
    ];

    private static readonly List<(string Name, Rational Start, Rational? End, bool IsStartIncluded, bool IsEndIncluded)> InvalidIntervals =
    [
        ("start after end", 5, 2, true, true),
        ("negative start", -1, 2, true, false),
        ("infinite start", Rational.PlusInfinity, null, true, false),
        ("open single point", 2, 2, true, false),
    ];

    private static readonly ExpressionSettings ClearableValue =
        new() { CacheSettings = new CacheSettings { CheapCacheElementThreshold = -1 } };

    private static CurveExpression Clearable(Curve curve)
        => curve.Negate().ToExpression("g").Negate(settings: ClearableValue);

    private static IEnumerable<Curve> CurvesAndNegations()
        => Curves.SelectMany(item => new[] { item.Curve, item.Curve.Negate() });

    public static IEnumerable<object[]> IntervalCases()
        => CurvesAndNegations().SelectMany(curve => Intervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Fact]
    public void TheJumpIsRightContinuousAndNotLeftContinuous()
    {
        Assert.True(RightContinuousJump.IsRightContinuous);
        Assert.False(RightContinuousJump.IsLeftContinuous);
    }

    [Fact]
    public void DescendingIsAffineAndDecreasesThroughZeroAtThree()
    {
        Assert.True(Descending.IsAffine);
        Assert.True(Descending.IsDecreasing);
        Assert.Equal(new Rational(5, 2), Descending.ValueAt(new Rational(1, 2)));
        Assert.Equal(Rational.Zero, Descending.ValueAt(3));
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsNonNegativeOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsNonNegativeOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsNonNegativeOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> NonNegativeCurves()
        => CurvesAndNegations().Where(curve => curve.IsNonNegative);

    public static IEnumerable<object[]> NonNegativeCacheCases()
        => NonNegativeCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsNonNegativeOverIntervalHasACacheCase()
        => Assert.NotEmpty(NonNegativeCurves());

    [Theory]
    [MemberData(nameof(NonNegativeCacheCases))]
    public void IsNonNegativeOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonNegative);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsNonNegativeOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> NonNegativeInvalidIntervalCases()
        => NonNegativeCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(NonNegativeInvalidIntervalCases))]
    public void IsNonNegativeOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonNegative);
        var expected = Record.Exception(() => curve.IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsNonPositiveOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsNonPositiveOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsNonPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsNonPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsNonPositiveOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsNonPositiveOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> NonPositiveCurves()
        => CurvesAndNegations().Where(curve => curve.IsNonPositive);

    public static IEnumerable<object[]> NonPositiveCacheCases()
        => NonPositiveCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsNonPositiveOverIntervalHasACacheCase()
        => Assert.NotEmpty(NonPositiveCurves());

    [Theory]
    [MemberData(nameof(NonPositiveCacheCases))]
    public void IsNonPositiveOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonPositive);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsNonPositiveOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> NonPositiveInvalidIntervalCases()
        => NonPositiveCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(NonPositiveInvalidIntervalCases))]
    public void IsNonPositiveOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonPositive);
        var expected = Record.Exception(() => curve.IsNonPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsNonPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsPositiveOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsPositiveOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsPositiveOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsPositiveOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> PositiveCurves()
        => CurvesAndNegations().Where(curve => curve.IsPositive);

    public static IEnumerable<object[]> PositiveCacheCases()
        => PositiveCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsPositiveOverIntervalHasACacheCase()
        => Assert.NotEmpty(PositiveCurves());

    [Theory]
    [MemberData(nameof(PositiveCacheCases))]
    public void IsPositiveOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsPositive);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsPositiveOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> PositiveInvalidIntervalCases()
        => PositiveCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(PositiveInvalidIntervalCases))]
    public void IsPositiveOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsPositive);
        var expected = Record.Exception(() => curve.IsPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsPositiveOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsNegativeOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsNegativeOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsNegativeOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsNegativeOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> NegativeCurves()
        => CurvesAndNegations().Where(curve => curve.IsNegative);

    public static IEnumerable<object[]> NegativeCacheCases()
        => NegativeCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsNegativeOverIntervalHasACacheCase()
        => Assert.NotEmpty(NegativeCurves());

    [Theory]
    [MemberData(nameof(NegativeCacheCases))]
    public void IsNegativeOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNegative);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsNegativeOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> NegativeInvalidIntervalCases()
        => NegativeCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(NegativeInvalidIntervalCases))]
    public void IsNegativeOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNegative);
        var expected = Record.Exception(() => curve.IsNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsNegativeOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsNonDecreasingOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsNonDecreasingOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsNonDecreasingOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> NonDecreasingCurves()
        => CurvesAndNegations().Where(curve => curve.IsNonDecreasing);

    public static IEnumerable<object[]> NonDecreasingCacheCases()
        => NonDecreasingCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsNonDecreasingOverIntervalHasACacheCase()
        => Assert.NotEmpty(NonDecreasingCurves());

    [Theory]
    [MemberData(nameof(NonDecreasingCacheCases))]
    public void IsNonDecreasingOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonDecreasing);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsNonDecreasingOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> NonDecreasingInvalidIntervalCases()
        => NonDecreasingCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(NonDecreasingInvalidIntervalCases))]
    public void IsNonDecreasingOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonDecreasing);
        var expected = Record.Exception(() => curve.IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsIncreasingOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsIncreasingOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsIncreasingOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> IncreasingCurves()
        => CurvesAndNegations().Where(curve => curve.IsIncreasing);

    public static IEnumerable<object[]> IncreasingCacheCases()
        => IncreasingCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsIncreasingOverIntervalHasACacheCase()
        => Assert.NotEmpty(IncreasingCurves());

    [Theory]
    [MemberData(nameof(IncreasingCacheCases))]
    public void IsIncreasingOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsIncreasing);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsIncreasingOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> IncreasingInvalidIntervalCases()
        => IncreasingCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(IncreasingInvalidIntervalCases))]
    public void IsIncreasingOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsIncreasing);
        var expected = Record.Exception(() => curve.IsIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsNonIncreasingOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsNonIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsNonIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsNonIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsNonIncreasingOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsNonIncreasingOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> NonIncreasingCurves()
        => CurvesAndNegations().Where(curve => curve.IsNonIncreasing);

    public static IEnumerable<object[]> NonIncreasingCacheCases()
        => NonIncreasingCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsNonIncreasingOverIntervalHasACacheCase()
        => Assert.NotEmpty(NonIncreasingCurves());

    [Theory]
    [MemberData(nameof(NonIncreasingCacheCases))]
    public void IsNonIncreasingOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonIncreasing);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsNonIncreasingOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> NonIncreasingInvalidIntervalCases()
        => NonIncreasingCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(NonIncreasingInvalidIntervalCases))]
    public void IsNonIncreasingOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsNonIncreasing);
        var expected = Record.Exception(() => curve.IsNonIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsNonIncreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsDecreasingOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsDecreasingOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsDecreasingOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> DecreasingCurves()
        => CurvesAndNegations().Where(curve => curve.IsDecreasing);

    public static IEnumerable<object[]> DecreasingCacheCases()
        => DecreasingCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsDecreasingOverIntervalHasACacheCase()
        => Assert.NotEmpty(DecreasingCurves());

    [Theory]
    [MemberData(nameof(DecreasingCacheCases))]
    public void IsDecreasingOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsDecreasing);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsDecreasingOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> DecreasingInvalidIntervalCases()
        => DecreasingCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(DecreasingInvalidIntervalCases))]
    public void IsDecreasingOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsDecreasing);
        var expected = Record.Exception(() => curve.IsDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsContinuousOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsContinuousOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsContinuousOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsContinuousOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> ContinuousCurves()
        => CurvesAndNegations().Where(curve => curve.IsContinuous);

    public static IEnumerable<object[]> ContinuousCacheCases()
        => ContinuousCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsContinuousOverIntervalHasACacheCase()
        => Assert.NotEmpty(ContinuousCurves());

    [Theory]
    [MemberData(nameof(ContinuousCacheCases))]
    public void IsContinuousOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsLeftContinuous && expression.IsRightContinuous);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsContinuousOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> ContinuousInvalidIntervalCases()
        => ContinuousCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(ContinuousInvalidIntervalCases))]
    public void IsContinuousOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsLeftContinuous && expression.IsRightContinuous);
        var expected = Record.Exception(() => curve.IsContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsLeftContinuousOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsLeftContinuousOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsLeftContinuousOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> LeftContinuousCurves()
        => CurvesAndNegations().Where(curve => curve.IsLeftContinuous);

    public static IEnumerable<object[]> LeftContinuousCacheCases()
        => LeftContinuousCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsLeftContinuousOverIntervalHasACacheCase()
        => Assert.NotEmpty(LeftContinuousCurves());

    [Theory]
    [MemberData(nameof(LeftContinuousCacheCases))]
    public void IsLeftContinuousOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsLeftContinuous);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsLeftContinuousOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> LeftContinuousInvalidIntervalCases()
        => LeftContinuousCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(LeftContinuousInvalidIntervalCases))]
    public void IsLeftContinuousOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsLeftContinuous);
        var expected = Record.Exception(() => curve.IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

    [Theory]
    [MemberData(nameof(IntervalCases))]
    public void IsRightContinuousOverIntervalAgreesWithTheCurve(Curve curve, Rational start, Rational? end,
        bool isStartIncluded, bool isEndIncluded)
    {
        var expected = curve.IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, curve.ToExpression().IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.Equal(expected, curve.Negate().ToExpression().Negate().IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    [Fact]
    public void IsRightContinuousOverIntervalTakesBothAnswers()
    {
        var answers = CurvesAndNegations()
            .SelectMany(curve => Intervals.Select(interval => curve.IsRightContinuousOverInterval(
                interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded)))
            .ToArray();
        Assert.Contains(true, answers);
        Assert.Contains(false, answers);
    }

    private static IEnumerable<Curve> RightContinuousCurves()
        => CurvesAndNegations().Where(curve => curve.IsRightContinuous);

    public static IEnumerable<object[]> RightContinuousCacheCases()
        => RightContinuousCurves().Select(curve => new object[] { curve });

    [Fact]
    public void IsRightContinuousOverIntervalHasACacheCase()
        => Assert.NotEmpty(RightContinuousCurves());

    [Theory]
    [MemberData(nameof(RightContinuousCacheCases))]
    public void IsRightContinuousOverIntervalUsesACompletedCache(Curve curve)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsRightContinuous);
        expression.ClearValueCache(CacheClearScope.SelfOnly);
        Assert.False(expression.IsComputed);

        Assert.True(expression.IsRightContinuousOverInterval(1, 5, true, false));
        Assert.False(expression.IsComputed);
    }

    public static IEnumerable<object[]> RightContinuousInvalidIntervalCases()
        => RightContinuousCurves().SelectMany(curve => InvalidIntervals.Select(interval => new object[]
        {
            curve, interval.Start, interval.End, interval.IsStartIncluded, interval.IsEndIncluded
        }));

    [Theory]
    [MemberData(nameof(RightContinuousInvalidIntervalCases))]
    public void IsRightContinuousOverIntervalRejectsInvalidIntervalsWithACompletedCache(Curve curve,
        Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded)
    {
        var expression = Clearable(curve);
        Assert.True(expression.IsRightContinuous);
        var expected = Record.Exception(() => curve.IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        var actual = Record.Exception(() => expression.IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
        Assert.NotNull(expected);
        Assert.IsType(expected.GetType(), actual);
        Assert.Equal(expected.Message, actual.Message);
    }

}
