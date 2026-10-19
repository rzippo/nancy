using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class BoundaryValues
{
    private readonly ITestOutputHelper output;

    public BoundaryValues(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Curve curve, Rational inf, Rational? min, Rational sup, Rational? max)> CurvesAndBoundaries =
    [
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 9),
                    Point.Zero(9),
                    new Segment(9, 67, 0, 160),
                    new Point(67, 9280),
                    new Segment(67, 117, 9280, 160)
                ]),
                67,
                50,
                8000
            ),
            0,
            0,
            Rational.PlusInfinity,
            null
        ),
        (
            new Curve(
                baseSequence: new Sequence( [
                    new Point(0, 0), new Segment(0, 32, 0, new Rational(25, 24)), new Point(32, new Rational(100, 3)),
                    new Segment(32, 224, new Rational(100, 3), 0), new Point(224, new Rational(100, 3)),
                    new Segment(224, 232, new Rational(100, 3), new Rational(25, 16)),
                    new Point(232, new Rational(275, 6)),
                    new Segment(232, 256, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(256, new Rational(100, 3)), new Segment(256, 320, new Rational(100, 3), 0),
                    new Point(320, new Rational(100, 3)),
                    new Segment(320, 332, new Rational(100, 3), new Rational(25, 96)),
                    new Point(332, new Rational(175, 4)),
                    new Segment(332, 352, new Rational(175, 4), new Rational(-25, 48)),
                    new Point(352, new Rational(100, 3)), new Segment(352, 416, new Rational(100, 3), 0),
                    new Point(416, new Rational(100, 3)),
                    new Segment(416, 432, new Rational(100, 3), new Rational(25, 128)),
                    new Point(432, new Rational(125, 3)),
                    new Segment(432, 448, new Rational(125, 3), new Rational(-25, 48)),
                    new Point(448, new Rational(100, 3)), new Segment(448, 480, new Rational(100, 3), 0),
                    new Point(480, new Rational(100, 3)),
                    new Segment(480, 482, new Rational(100, 3), new Rational(25, 16)),
                    new Point(482, new Rational(275, 6)),
                    new Segment(482, 506, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(506, new Rational(100, 3)), new Segment(506, 512, new Rational(100, 3), 0),
                    new Point(512, new Rational(100, 3)),
                    new Segment(512, 532, new Rational(100, 3), new Rational(5, 32)),
                    new Point(532, new Rational(475, 12)),
                    new Segment(532, 544, new Rational(475, 12), new Rational(-25, 48)),
                    new Point(544, new Rational(100, 3)), new Segment(544, 576, new Rational(100, 3), 0),
                    new Point(576, new Rational(100, 3)),
                    new Segment(576, 582, new Rational(100, 3), new Rational(25, 48)),
                    new Point(582, new Rational(275, 6)),
                    new Segment(582, 606, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(606, new Rational(100, 3)), new Segment(606, 608, new Rational(100, 3), 0),
                    new Point(608, new Rational(100, 3)),
                    new Segment(608, 632, new Rational(100, 3), new Rational(25, 192)),
                    new Point(632, new Rational(75, 2)),
                    new Segment(632, 640, new Rational(75, 2), new Rational(-25, 48)),
                    new Point(640, new Rational(100, 3)), new Segment(640, 672, new Rational(100, 3), 0),
                    new Point(672, new Rational(100, 3)),
                    new Segment(672, 682, new Rational(100, 3), new Rational(5, 16)),
                    new Point(682, new Rational(1075, 24)),
                    new Segment(682, 704, new Rational(1075, 24), new Rational(-25, 48)),
                    new Point(704, new Rational(100, 3)), new Segment(704, 768, new Rational(100, 3), 0),
                    new Point(768, new Rational(100, 3)),
                    new Segment(768, 782, new Rational(100, 3), new Rational(25, 112)),
                    new Point(782, new Rational(1025, 24)),
                    new Segment(782, 800, new Rational(1025, 24), new Rational(-25, 48)),
                    new Point(800, new Rational(100, 3)), new Segment(800, 864, new Rational(100, 3), 0),
                    new Point(864, new Rational(100, 3)),
                    new Segment(864, 882, new Rational(100, 3), new Rational(25, 144)),
                    new Point(882, new Rational(325, 8)),
                    new Segment(882, 896, new Rational(325, 8), new Rational(-25, 48)),
                    new Point(896, new Rational(100, 3)), new Segment(896, 928, new Rational(100, 3), 0),
                    new Point(928, new Rational(100, 3)),
                    new Segment(928, 932, new Rational(100, 3), new Rational(25, 32)),
                    new Point(932, new Rational(275, 6)),
                    new Segment(932, 956, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(956, new Rational(100, 3)), new Segment(956, 960, new Rational(100, 3), 0),
                    new Point(960, new Rational(100, 3)),
                    new Segment(960, 982, new Rational(100, 3), new Rational(25, 176)),
                    new Point(982, new Rational(925, 24)),
                    new Segment(982, 992, new Rational(925, 24), new Rational(-25, 48)),
                    new Point(992, new Rational(100, 3)), new Segment(992, 1024, new Rational(100, 3), 0),
                    new Point(1024, new Rational(100, 3)),
                    new Segment(1024, 1032, new Rational(100, 3), new Rational(25, 64))
                ]),
                pseudoPeriodStart: 232,
                pseudoPeriodLength: 800,
                pseudoPeriodHeight: 0
            ),
            0,
            0,
            new Rational(275, 6),
            new Rational(275, 6)
        )
    ];

    public static IEnumerable<object[]> GetBoundaryValuesTestCases()
        => CurvesAndBoundaries.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBoundaryValuesTestCases))]
    public void BoundaryValuesTest(Curve curve, Rational inf, Rational? min, Rational sup, Rational? max)
    {
        Assert.Equal(inf, curve.InfValue());
        Assert.Equal(min, curve.MinValue());
        Assert.Equal(sup, curve.SupValue());
        Assert.Equal(max, curve.MaxValue());
    }

    public static List<(Curve curve, Rational? min, Rational? minArg)> CurvesAndMinima =
    [
        // increasing: the minimum is attained at the origin
        (new RateLatencyServiceCurve(1, 0), 0, 0),
        // increasing after a plateau: the minimum is first attained at the origin
        (new RateLatencyServiceCurve(5, 2), 0, 0),
        // increasing with a burst: the minimum is attained at the origin only
        (new SigmaRhoArrivalCurve(1, 1), 0, 0),
        // constant after a dip
        (
            new Curve(
                baseSequence: new Sequence([
                    new Point(0, 2),
                    new Segment(0, 1, 2, -1),
                    new Point(1, 1),
                    Segment.Constant(1, 2, 1)
                ]),
                pseudoPeriodStart: 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            1,
            1
        ),
        // decreasing without bound: there is no minimum
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 0, -1)
                ]),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: -1
            ),
            null,
            null
        ),
        // the difference of a token bucket and a faster rate-latency curve: decreasing without bound after the latency
        (new SigmaRhoArrivalCurve(4, 1) - new RateLatencyServiceCurve(2, 3), null, null),
    ];

    public static IEnumerable<object[]> GetMinArgTestCases()
        => CurvesAndMinima.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetMinArgTestCases))]
    public void MinArgTest(Curve curve, Rational? min, Rational? minArg)
    {
        Assert.Equal(min, curve.MinValue());
        Assert.Equal(minArg, curve.MinArg());
        // for a finite curve, a minimum exists exactly when an argument attaining it does
        Assert.Equal(curve.MinValue() is null, curve.MinArg() is null);
        if (minArg is { } t)
            Assert.Equal(min, curve.ValueAt(t));
    }

    public static List<(
        Curve curve,
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded,
        bool isEndIncluded,
        Rational inf,
        Rational? min,
        Rational sup,
        Rational? max
        )> CurvesIntervalsAndBoundaries =
    [
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 9),
                    Point.Zero(9),
                    new Segment(9, 67, 0, 160),
                    new Point(67, 9280),
                    new Segment(67, 117, 9280, 160)
                ]),
                67,
                50,
                8000
            ),
            0,
            117,
            true,
            false,
            0,
            0,
            17280,
            null
        ),
        (
            new Curve(
                baseSequence: new Sequence( [
                    new Point(0, 0),
                    new Segment(0, 32, 0, new Rational(25, 24)),
                    new Point(32, new Rational(100, 3)),
                    new Segment(32, 224, new Rational(100, 3), 0),
                    new Point(224, new Rational(100, 3)),
                    new Segment(224, 232, new Rational(100, 3), new Rational(25, 16)),
                    new Point(232, new Rational(275, 6)),
                    new Segment(232, 256, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(256, new Rational(100, 3)),
                    new Segment(256, 320, new Rational(100, 3), 0),
                    new Point(320, new Rational(100, 3)),
                    new Segment(320, 332, new Rational(100, 3), new Rational(25, 96)),
                    new Point(332, new Rational(175, 4)),
                    new Segment(332, 352, new Rational(175, 4), new Rational(-25, 48)),
                    new Point(352, new Rational(100, 3)),
                    new Segment(352, 416, new Rational(100, 3), 0),
                    new Point(416, new Rational(100, 3)),
                    new Segment(416, 432, new Rational(100, 3), new Rational(25, 128)),
                    new Point(432, new Rational(125, 3)),
                    new Segment(432, 448, new Rational(125, 3), new Rational(-25, 48)),
                    new Point(448, new Rational(100, 3)),
                    new Segment(448, 480, new Rational(100, 3), 0),
                    new Point(480, new Rational(100, 3)),
                    new Segment(480, 482, new Rational(100, 3), new Rational(25, 16)),
                    new Point(482, new Rational(275, 6)),
                    new Segment(482, 506, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(506, new Rational(100, 3)),
                    new Segment(506, 512, new Rational(100, 3), 0),
                    new Point(512, new Rational(100, 3)),
                    new Segment(512, 532, new Rational(100, 3), new Rational(5, 32)),
                    new Point(532, new Rational(475, 12)),
                    new Segment(532, 544, new Rational(475, 12), new Rational(-25, 48)),
                    new Point(544, new Rational(100, 3)),
                    new Segment(544, 576, new Rational(100, 3), 0),
                    new Point(576, new Rational(100, 3)),
                    new Segment(576, 582, new Rational(100, 3), new Rational(25, 48)),
                    new Point(582, new Rational(275, 6)),
                    new Segment(582, 606, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(606, new Rational(100, 3)),
                    new Segment(606, 608, new Rational(100, 3), 0),
                    new Point(608, new Rational(100, 3)),
                    new Segment(608, 632, new Rational(100, 3), new Rational(25, 192)),
                    new Point(632, new Rational(75, 2)),
                    new Segment(632, 640, new Rational(75, 2), new Rational(-25, 48)),
                    new Point(640, new Rational(100, 3)),
                    new Segment(640, 672, new Rational(100, 3), 0),
                    new Point(672, new Rational(100, 3)),
                    new Segment(672, 682, new Rational(100, 3), new Rational(5, 16)),
                    new Point(682, new Rational(1075, 24)),
                    new Segment(682, 704, new Rational(1075, 24), new Rational(-25, 48)),
                    new Point(704, new Rational(100, 3)),
                    new Segment(704, 768, new Rational(100, 3), 0),
                    new Point(768, new Rational(100, 3)),
                    new Segment(768, 782, new Rational(100, 3), new Rational(25, 112)),
                    new Point(782, new Rational(1025, 24)),
                    new Segment(782, 800, new Rational(1025, 24), new Rational(-25, 48)),
                    new Point(800, new Rational(100, 3)),
                    new Segment(800, 864, new Rational(100, 3), 0),
                    new Point(864, new Rational(100, 3)),
                    new Segment(864, 882, new Rational(100, 3), new Rational(25, 144)),
                    new Point(882, new Rational(325, 8)),
                    new Segment(882, 896, new Rational(325, 8), new Rational(-25, 48)),
                    new Point(896, new Rational(100, 3)),
                    new Segment(896, 928, new Rational(100, 3), 0),
                    new Point(928, new Rational(100, 3)),
                    new Segment(928, 932, new Rational(100, 3), new Rational(25, 32)),
                    new Point(932, new Rational(275, 6)),
                    new Segment(932, 956, new Rational(275, 6), new Rational(-25, 48)),
                    new Point(956, new Rational(100, 3)),
                    new Segment(956, 960, new Rational(100, 3), 0),
                    new Point(960, new Rational(100, 3)),
                    new Segment(960, 982, new Rational(100, 3), new Rational(25, 176)),
                    new Point(982, new Rational(925, 24)),
                    new Segment(982, 992, new Rational(925, 24), new Rational(-25, 48)),
                    new Point(992, new Rational(100, 3)),
                    new Segment(992, 1024, new Rational(100, 3), 0),
                    new Point(1024, new Rational(100, 3)),
                    new Segment(1024, 1032, new Rational(100, 3), new Rational(25, 64))
                ]),
                pseudoPeriodStart: 232,
                pseudoPeriodLength: 800,
                pseudoPeriodHeight: 0
            ),
            0,
            1032,
            false,
            true,
            0,
            null,
            new Rational(275, 6),
            new Rational(275, 6)
        ),
        (
            new RateLatencyServiceCurve(10, 10),
            0,
            10,
            false,
            false,
            0,
            0,
            0,
            0
        )
    ];

    public static IEnumerable<object[]> GetBoundaryValuesOverIntervalCases()
        => CurvesIntervalsAndBoundaries.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBoundaryValuesOverIntervalCases))]
    public void BoundaryValuesOverIntervalTest(
        Curve curve,
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded,
        bool isEndIncluded,
        Rational inf,
        Rational? min,
        Rational sup,
        Rational? max
    )
    {
        Assert.Equal(inf, curve.InfValueOverInterval(cutStart, cutEnd, isStartIncluded, isEndIncluded));
        Assert.Equal(min, curve.MinValueOverInterval(cutStart, cutEnd, isStartIncluded, isEndIncluded));
        Assert.Equal(sup, curve.SupValueOverInterval(cutStart, cutEnd, isStartIncluded, isEndIncluded));
        Assert.Equal(max, curve.MaxValueOverInterval(cutStart, cutEnd, isStartIncluded, isEndIncluded));
    }

    public static IEnumerable<object[]> GetInfiniteIntervalCases()
    {
        Curve[] curves = [new RateLatencyServiceCurve(2, 3), new StairCurve(1, new Rational(1, 2)).DelayBy(1)];
        foreach (var curve in curves)
        {
            yield return new object[] { curve, (Rational) 0, Rational.PlusInfinity };
            yield return new object[] { curve, (Rational) 5, Rational.PlusInfinity };
            yield return new object[] { curve, Rational.PlusInfinity, Rational.PlusInfinity };
        }
    }

    /// <summary>
    /// The boundary values over an interval are defined for a finite interval, so an infinite end is rejected for every one of them.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetInfiniteIntervalCases))]
    public void BoundaryValuesOverInfiniteIntervalThrow(Curve curve, Rational cutStart, Rational cutEnd)
    {
        Assert.Throws<ArgumentException>(() => curve.InfValueOverInterval(cutStart, cutEnd));
        Assert.Throws<ArgumentException>(() => curve.MinValueOverInterval(cutStart, cutEnd));
        Assert.Throws<ArgumentException>(() => curve.SupValueOverInterval(cutStart, cutEnd));
        Assert.Throws<ArgumentException>(() => curve.MaxValueOverInterval(cutStart, cutEnd));
    }

}
