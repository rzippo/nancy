using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class PeriodStartInfimum
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PeriodStartInfimum(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve curve, Rational expected)> PeriodStartInfimumKnownCases =
    [
        (
            new RateLatencyServiceCurve(4, 3),
            3
        ),
        (
            new SigmaRhoArrivalCurve(4, 3),
            0
        ),
        (
            new SigmaRhoArrivalCurve(4, 0),
            0
        ),
        (
            new DelayServiceCurve(5),
            5
        ),
        (
            Curve.Minimum(new ConstantCurve(3), new DelayServiceCurve(5)),
            5
        ),
        (
            new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    new Point(1, 0),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    Segment.Constant(2, 3, 2),
                    new Point(3, 2),
                    Segment.Constant(3, 4, 2)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            2
        ),
        (
            new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 1),
                    new Point(1, 3),
                    Segment.Constant(1, 2, 3)
                ]),
                1,
                1,
                2
            ),
            0
        ),
        // The last transient segment, slope 1 on (1, 3), continues the last segment of the period, slope 1 on (4, 5), only over (2, 3):
        // at t in (1, 2], t + 2 falls in the period's first segment, slope 2 on (3, 4), where f(t + 2) differs from f(t) + 6.
        (TwoSegmentPeriod, 2),
        // The same curve, with one more period in its transient.
        (new Curve(TwoSegmentPeriod.Cut(0, 7), 5, 2, 6), 2),
        // 0 on [0, 1], t on (1, 2), +inf on [2, 3], t on (3, 4), with a period of length 2 and height 2: f(3) = +inf is not f(1) + 2.
        (
            new Curve(new Sequence([
                    Point.Origin(), Segment.Zero(0, 1), new Point(1, 0), new Segment(1, 2, 1, 1),
                    Point.PlusInfinite(2), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3), new Segment(3, 4, 3, 1)
                ]),
                2, 2, 2),
            1
        ),
        // 0 on [0, 1], +inf on (1, 2), 5 on [2, 3], +inf on (3, 4), with a period of length 2 and height 0: f(3) = 5 is not f(1).
        (
            new Curve(new Sequence([
                    Point.Origin(), Segment.Zero(0, 1), new Point(1, 0), Segment.PlusInfinite(1, 2),
                    new Point(2, 5), Segment.Constant(2, 3, 5), new Point(3, 5), Segment.PlusInfinite(3, 4)
                ]),
                2, 2, 0),
            1
        ),
        // 0 on [0, 2), +inf at 2, 5 on (2, 3), with a period of length 1 and height 1: f(T) is infinite, and the last segments do not continue each other.
        (
            new Curve(new Sequence([
                    Point.Origin(), Segment.Zero(0, 2), Point.PlusInfinite(2), Segment.Constant(2, 3, 5)
                ]),
                2, 1, 1),
            2
        ),
    ];

    /// <summary>
    /// 2 at 0, slope 3/2 on (0, 1), 7/2 at 1, slope 1 on (1, 3), 11/2 at 3, then a period of length 2 and height 6:
    /// slope 2 on (3, 4) from 13/2, 17/2 at 4, slope 1 on (4, 5) from 21/2.
    /// </summary>
    private static Curve TwoSegmentPeriod => new(
        new Sequence([
            new Point(0, 2), new Segment(0, 1, 2, new Rational(3, 2)),
            new Point(1, new Rational(7, 2)), new Segment(1, 3, new Rational(7, 2), 1),
            new Point(3, new Rational(11, 2)), new Segment(3, 4, new Rational(13, 2), 2),
            new Point(4, new Rational(17, 2)), new Segment(4, 5, new Rational(21, 2), 1)
        ]),
        3, 2, 6);

    public static IEnumerable<object[]> PeriodStartInfimumTestCases()
        => PeriodStartInfimumKnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PeriodStartInfimumTestCases))]
    public void EquivalenceToExpected(Curve curve, Rational expected)
    {
        _testOutputHelper.WriteLine($"var curve = {curve.ToCodeString()};");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()};");
        var t_l = curve.PseudoPeriodStartInfimum;
        _testOutputHelper.WriteLine($"var t_l = {t_l.ToCodeString()};");
        Assert.Equal(expected, t_l);
    }
}