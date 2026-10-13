using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Deviations
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Deviations(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Curve f, Curve g, Rational hDev, Rational time)> HDevKnownCases =
    [
        (
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new RateLatencyServiceCurve(4, 3),
            hDev: 4,
            0
        ),
        // With no rate the service curve is 0 everywhere, and never reaches the burst.
        // hDev(f, g, t) is 0 at t = 0 and +infinity for every t > 0, so the deviation is +infinity, measured at 0.
        (
            f: new SigmaRhoArrivalCurve(1, 0),
            g: new RateLatencyServiceCurve(0, 0),
            hDev: Rational.PlusInfinity,
            0
        ),
        // As above, with the rate-latency curve of a tandem that includes a server with no rate.
        (
            f: new SigmaRhoArrivalCurve(1, 0),
            g: new RateLatencyServiceCurve(0, 1).Convolution(new RateLatencyServiceCurve(5, 2)),
            hDev: Rational.PlusInfinity,
            0
        ),
        // As above, with a rate-latency curve scaled by 0.
        (
            f: new SigmaRhoArrivalCurve(1, 0),
            g: new RateLatencyServiceCurve(4, 1).Scale(0),
            hDev: Rational.PlusInfinity,
            0
        ),
        // With no burst and no rate the arrival curve is 0 everywhere, so it never exceeds g.
        (
            f: new SigmaRhoArrivalCurve(0, 0),
            g: new RateLatencyServiceCurve(2, 4),
            hDev: 0,
            0
        ),
        // The same against service curves with no rate, which are 0 everywhere as well.
        (
            f: new SigmaRhoArrivalCurve(0, 0),
            g: new RateLatencyServiceCurve(0, 0),
            hDev: 0,
            0
        ),
        (
            f: new SigmaRhoArrivalCurve(0, 0),
            g: new RateLatencyServiceCurve(0, 3),
            hDev: 0,
            0
        ),
        // With a rate and no burst, f(t) > 0 for every t > 0, which a service curve with no rate never reaches:
        // hDev(f, g, t) is +infinity for every t > 0, and is measured at 0.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(0, 0),
            hDev: Rational.PlusInfinity,
            0
        ),
        (
            f: new SigmaRhoArrivalCurve(2, 1),
            g: new RateLatencyServiceCurve(0, 3),
            hDev: Rational.PlusInfinity,
            0
        ),
        // Equal curves: hDev(f, g, t) is 0 at every t.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(1, 0),
            hDev: 0,
            0
        ),
        (
            f: new RateLatencyServiceCurve(2, 3),
            g: new RateLatencyServiceCurve(2, 3),
            hDev: 0,
            0
        ),
        (
            f: new StairCurve(1, 1),
            g: new StairCurve(1, 1),
            hDev: 0,
            0
        ),
        // The same rate, g delayed by 3: hDev(f, g, t) is 3 for every t > 0, first approached right after 0.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(1, 3),
            hDev: 3,
            0
        ),
        // A burst of 2 at the same rate, with no latency: hDev(f, g, t) is 2 for every t > 0.
        (
            f: new SigmaRhoArrivalCurve(2, 1),
            g: new RateLatencyServiceCurve(1, 0),
            hDev: 2,
            0
        ),
        // Against a delay of 2, which is +infinity after it: hDev(f, g, t) is 2 - t up to 2, first approached right after 0.
        (
            f: new SigmaRhoArrivalCurve(1, 1),
            g: new DelayServiceCurve(2),
            hDev: 2,
            0
        ),
        // With no burst the deviation approaches the latency as t goes to 0 from the right.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(2, 4),
            hDev: 4,
            0
        ),
        // A right-continuous staircase arrival curve, as a request bound function RBF(t) = (1 + floor(t/T)) * C is.
        // The deviation is attained on the first step, where it is lpi(g)(C).
        (
            f: new StairCurve(30, 100).DelayBy(0).ToRightContinuous(),
            g: new RateLatencyServiceCurve(1, 0),
            hDev: 30,
            0
        ),
        (
            f: new StairCurve(30, 100).DelayBy(0).ToRightContinuous(),
            g: new RateLatencyServiceCurve(2, 5),
            hDev: 20,
            0
        ),
        (
            f: new StairCurve(30, 100).DelayBy(0).ToRightContinuous(),
            g: new DelayServiceCurve(3),
            hDev: 3,
            0
        ),
        (
            // a service curve with an infinite rate at the origin, which serves the first step at once
            f: new StairCurve(30, 100).DelayBy(0).ToRightContinuous(),
            g: new SigmaRhoArrivalCurve(35, 1),
            hDev: 0,
            0
        ),
        (
            // the left-continuous staircase, for contrast, is not constant over its first period, being 0 at the origin
            f: new StairCurve(30, 100).DelayBy(0),
            g: new RateLatencyServiceCurve(1, 0),
            hDev: 30,
            0
        ),
        (
            f: new SigmaRhoArrivalCurve(4, 5),
            g: new RateLatencyServiceCurve(4, 3),
            hDev: Rational.PlusInfinity,
            Rational.PlusInfinity
        ),
        (
            // Example from [DNC18] Figure 5.7
            f: new SigmaRhoArrivalCurve(1, 1),
            g: Curve.Minimum(
                new RateLatencyServiceCurve(3, 0),
                new RateLatencyServiceCurve(3, 4) + 3
            ),
            2,
            2
        ),
        (
            // Variation on the example from [DNC18] Figure 5.7
            f: new SigmaRhoArrivalCurve(1, 2),
            g: Curve.Minimum(
                new RateLatencyServiceCurve(3, 0),
                new RateLatencyServiceCurve(3, 4) + 3
            ),
            3,
            1
        ),
        (
            f: new SigmaRhoArrivalCurve(2, 0),
            g: new RateLatencyServiceCurve(2, 2),
            3,
            0
        ),
        (
            f: Curve.Minimum(
                new RateLatencyServiceCurve(4, 0),
                new ConstantCurve(12)
            ),
            g: Curve.Minimum(
                new RateLatencyServiceCurve(3, 3),
                new ConstantCurve(12)
            ),
            4,
            3
        ),
        (
            // under-dimensioned server => unbounded delay
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new RateLatencyServiceCurve(2, 3),
            hDev: Rational.PlusInfinity,
            Rational.PlusInfinity
        ),
        (
            // constant server and no burst => no delay
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(2, 0),
            hDev: 0,
            0
        ),
        (
            // edge case: strictly positive service curve, always strictly greater than arrival curve
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(2, 0).VerticalShift(1, false),
            hDev: 0,
            0
        ),
        (
            // same rate, token-bucket
            f: new SigmaRhoArrivalCurve(20, 5),
            g: new SigmaRhoArrivalCurve(10, 5),
            hDev: 2,
            0
        ),
        (
            // same rate, rate-latency
            f: new RateLatencyServiceCurve(5, 10),
            g: new RateLatencyServiceCurve(5, 20),
            hDev: 10,
            10
        ),
        (
            // hdev corresponds to a jump
            f: new Curve(new Sequence([
                    new Point(time: 0, value: 2),
                    new Segment
                    (
                        startTime: 0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 4),
                        endTime: 4
                    ),
                    new Point(time: 4, value: 4),
                    new Segment
                    (
                        startTime: 4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 2),
                        endTime: 6
                    )
                ]),
                4, 2, 1
            ),
            g: new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 8),
                    Point.Zero(8),
                    new Segment
                    (
                        startTime: 8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(time: 11, value: 7),
                    new Segment
                    (
                        startTime: 11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1),
                        endTime: 12
                    )
                ]),
                11, 1, 1
            ),
            11,
            0
        ),
        (
            // hdev corresponds to a jump, order of curves inverted
            f: new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 8),
                    Point.Zero(8),
                    new Segment
                    (
                        startTime: 8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(time: 11, value: 7),
                    new Segment
                    (
                        startTime: 11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1),
                        endTime: 12
                    )
                ]),
                11, 1, 1
            ),
            g: new Curve(new Sequence([
                    new Point(time: 0, value: 2),
                    new Segment
                    (
                        startTime: 0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 4),
                        endTime: 4
                    ),
                    new Point(time: 4, value: 4),
                    new Segment
                    (
                        startTime: 4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 2),
                        endTime: 6
                    )
                ]),
                4, 2, 1
            ),
            Rational.PlusInfinity,
            Rational.PlusInfinity
        ),
        (
            // hdev corresponds to a jump, order of curves inverted
            f: new Curve(new Sequence([
                    Point.Origin(),
                    Segment.Zero(0, 8),
                    Point.Zero(8),
                    new Segment
                    (
                        startTime: 8,
                        rightLimitAtStartTime: 0,
                        slope: new Rational(1, 3),
                        endTime: 11
                    ),
                    new Point(time: 11, value: 7),
                    new Segment
                    (
                        startTime: 11,
                        rightLimitAtStartTime: 7,
                        slope: new Rational(1, 2),
                        endTime: 13
                    )
                ]),
                11, 2, 1
            ),
            g: new Curve(new Sequence([
                    new Point(time: 0, value: 2),
                    new Segment
                    (
                        startTime: 0,
                        rightLimitAtStartTime: 2,
                        slope: new Rational(1, 4),
                        endTime: 4
                    ),
                    new Point(time: 4, value: 4),
                    new Segment
                    (
                        startTime: 4,
                        rightLimitAtStartTime: 4,
                        slope: new Rational(1, 2),
                        endTime: 6
                    )
                ]),
                4, 2, 1
            ),
            0,
            0
        ),
    ];
    
    public static IEnumerable<object[]> GetHorizontalDeviationTestCases()
    {
        var additionalTestcases = new List<(Curve f, Curve g, Rational expected)>
        {
            #if BIG_RATIONAL
            (
                f: Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}"),
                g: Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}"),
                new Rational(115102801965691,149850048000)
            ),
            (
                f: new Curve(Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}")),
                g: new Curve(Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}")),
                new Rational(115102801965691,149850048000)
            )
            #endif
        };

        var testcases = HDevKnownCases.Select(tuple =>
            {
                var (f, g, hdev, _) = tuple;
                return (f, g, hdev);
            })
            .Concat(additionalTestcases);
        
        foreach (var (f, g, expected) in testcases)
        {
            yield return new object[] { f, g, expected };
            yield return new object[] { new Curve(f), new Curve(g), expected }; // repeat the test as generic Curves
            yield return new object[] { new Curve(f).Optimize(), new Curve(g).Optimize(), expected }; // repeat the test as generic and minimized Curves
        }
    }

    public static IEnumerable<object[]> GetHorizontalDeviationArgTestCases()
    {
        foreach (var (f, g, _, expected) in HDevKnownCases)
        {
            yield return new object[] { f, g, expected };
            yield return new object[] { new Curve(f), new Curve(g), expected }; // repeat the test as generic Curves
            yield return new object[] { new Curve(f).Optimize(), new Curve(g).Optimize(), expected }; // repeat the test as generic and minimized Curves
        }
    }
    
    public static List<(Curve f, Curve g, Rational vDev, Rational time)> VDevKnownCases =
    [
        (
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new RateLatencyServiceCurve(4, 3),
            vDev: 13,
            time: 3
        ),
        // Both 0 everywhere.
        (
            f: new SigmaRhoArrivalCurve(0, 0),
            g: new RateLatencyServiceCurve(0, 0),
            vDev: 0,
            time: 0
        ),
        // f is 0 everywhere: f - g is 0 up to the latency, and negative after.
        (
            f: new SigmaRhoArrivalCurve(0, 0),
            g: new RateLatencyServiceCurve(2, 4),
            vDev: 0,
            time: 0
        ),
        // g is 0 everywhere: f - g is f, which is the burst right after 0, or grows without bound with a rate.
        (
            f: new SigmaRhoArrivalCurve(1, 0),
            g: new RateLatencyServiceCurve(0, 0),
            vDev: 1,
            time: 0
        ),
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(0, 0),
            vDev: Rational.PlusInfinity,
            time: Rational.PlusInfinity
        ),
        // Equal curves: f - g is 0 everywhere.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(1, 0),
            vDev: 0,
            time: 0
        ),
        (
            f: new StairCurve(1, 1),
            g: new StairCurve(1, 1),
            vDev: 0,
            time: 0
        ),
        // The same rate, g delayed by 3: f - g is t up to 3, then 3.
        (
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(1, 3),
            vDev: 3,
            time: 3
        ),
        // Against a delay of 2: f - g is f up to 2, and -infinity after.
        (
            f: new SigmaRhoArrivalCurve(1, 1),
            g: new DelayServiceCurve(2),
            vDev: 3,
            time: 2
        ),
        (
            f: new SigmaRhoArrivalCurve(4, 5),
            g: new RateLatencyServiceCurve(4, 3),
            vDev: Rational.PlusInfinity,
            time: Rational.PlusInfinity
        ),
        (
            // Example from [DNC18] Figure 5.7
            f: new SigmaRhoArrivalCurve(1, 1),
            g: Curve.Minimum(
                new RateLatencyServiceCurve(3, 0),
                new RateLatencyServiceCurve(3, 4) + 3
            ),
            2,
            4
        ),
        (
            // Variation on the example from [DNC18] Figure 5.7
            f: new SigmaRhoArrivalCurve(1, 2),
            g: Curve.Minimum(
                new RateLatencyServiceCurve(3, 0),
                new RateLatencyServiceCurve(3, 4) + 3
            ),
            6,
            4
        ),
        (
            f: new SigmaRhoArrivalCurve(2, 0),
            g: new RateLatencyServiceCurve(2, 2),
            2,
            0
        ),
        (
            // sup is not attained
            f: new SigmaRhoArrivalCurve(4, 2),
            g: new RateLatencyServiceCurve(3, 0),
            4,
            0
        ),
        (
            // under-dimensioned server => unbounded backlog
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new RateLatencyServiceCurve(2, 3),
            Rational.PlusInfinity,
            Rational.PlusInfinity
        ),
        (
            // constant server and no burst => no backlog
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(2, 0),
            0,
            0
        ),
        (
            // constant server and no burst => no backlog
            f: new SigmaRhoArrivalCurve(0, 0.5m),
            g: new RateLatencyServiceCurve(2, 0),
            0,
            0
        ),
        (
            // edge case: strictly positive service curve, always strictly greater than arrival curve => backlog allowance
            f: new SigmaRhoArrivalCurve(0, 1),
            g: new RateLatencyServiceCurve(2, 0).VerticalShift(1, false),
            -1,
            0
        ),
        (
            // edge case: strictly positive service curve, always strictly greater than arrival curve => backlog allowance
            f: new SigmaRhoArrivalCurve(0, 0.5m),
            g: new RateLatencyServiceCurve(2, 0).VerticalShift(1, false),
            -1,
            0
        ),
        (
            // same rate, token-bucket
            f: new SigmaRhoArrivalCurve(20, 5),
            g: new SigmaRhoArrivalCurve(10, 5),
            vDev: 10,
            0
        ),
        (
            // same rate, rate-latency
            f: new RateLatencyServiceCurve(5, 10),
            g: new RateLatencyServiceCurve(5, 20),
            vDev: 50,
            20
        ),
        (
            // same rate, rate-latency
            f: new RateLatencyServiceCurve(new Rational(275, 7), new Rational(249, 44)),
            g: new RateLatencyServiceCurve(new Rational(275, 7), new Rational(35, 4)),
            vDev: new Rational(850,7),
            new Rational(35, 4)
        ),
    ];

    public static IEnumerable<object[]> GetVerticalDeviationTestCases()
    {
        foreach (var (f, g, expected, _) in VDevKnownCases)
        {
            yield return new object[] { f, g, expected };
            yield return new object[] { new Curve(f), new Curve(g), expected }; // repeat the test as generic Curves
            yield return new object[] { new Curve(f).Optimize(), new Curve(g).Optimize(), expected }; // repeat the test as generic and minimized Curves
        }
    }

    public static IEnumerable<object[]> GetVerticalDeviationArgTestCases()
    {
        foreach (var (f, g, _, expected) in VDevKnownCases)
        {
            yield return new object[] { f, g, expected };
            yield return new object[] { new Curve(f), new Curve(g), expected }; // repeat the test as generic Curves
            yield return new object[] { new Curve(f).Optimize(), new Curve(g).Optimize(), expected }; // repeat the test as generic and minimized Curves
        }
    }

    public static List<(Curve f, Curve g, Rational zDev)> ZDeviationKnownCases = 
    [
        // Example following [HCS24] Figure 3 
        (
            f: new RateLatencyServiceCurve(0.4m, 1),
            g: new RateLatencyServiceCurve(1, 2).VerticalShift(-2, false),
            zDev: 8
        )
    ];

    public static IEnumerable<object[]> GetZDeviationTestCases() 
        => ZDeviationKnownCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetHorizontalDeviationTestCases))]
    public void HorizontalDeviationTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.HorizontalDeviation(a, b);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetHorizontalDeviationArgTestCases))]
    public void HorizontalDeviationArgTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.HorizontalDeviationMeasuredAt(a, b);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// First operands that become $+\infty$, as delay service curves and curves computed from them do,
    /// with $hDev(f, g)$, the time it is measured at, and $hDev(f, g, t)$ at some times, computed by hand from
    /// $hDev(f, g, t) = \inf\{ d \ge 0 \mid f(t) \le g(t + d) \}$:
    /// where $f(t) = +\infty$, that is the time $g$ takes to become $+\infty$, or $+\infty$ if it never does.
    /// </summary>
    public static IEnumerable<object[]> InfiniteFirstOperandCases()
    {
        var d1 = new DelayServiceCurve(1);
        var d2 = new DelayServiceCurve(2);
        var d5 = new DelayServiceCurve(5);
        var rate = new RateLatencyServiceCurve(1, 0);
        // 2 + t over ]0, 3], then +inf
        var srUntil3 = Curve.Maximum(new SigmaRhoArrivalCurve(2, 1), new DelayServiceCurve(3));
        // 2 on [0, 2[, then +inf
        var twoUntil2 = new Curve(
            new Sequence([ new Point(0, 2), Segment.Constant(0, 2, 2), Point.PlusInfinite(2), Segment.PlusInfinite(2, 3) ]), 2, 1, 0);
        // 1 at 0, 3 on ]0, 2], then +inf
        var oneThenThreeUntil2 = new Curve(
            new Sequence([ new Point(0, 1), Segment.Constant(0, 2, 3), new Point(2, 3), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3), Segment.PlusInfinite(3, 4) ]), 3, 1, 0);
        // 4t up to 2, then 8
        var rampTo8 = new Curve(
            new Sequence([ Point.Origin(), new Segment(0, 2, 0, 4), new Point(2, 8), Segment.Constant(2, 3, 8) ]), 2, 1, 0);
        var half = new Rational(1, 2);
        var cases = new List<(string id, Curve f, Curve g, Rational hdev, Rational at, (Rational t, Rational v)[] values)>
        {
            // after 1, f is +inf and the rate never is
            ("delay, rate", d1, rate, Rational.PlusInfinity, 1, [(half, 0), (2, Rational.PlusInfinity)]),
            // after 1, f is +inf, and g becomes so after 2
            ("delay, longer delay", d1, d2, 1, 1, [(half, 0), (new Rational(3, 2), half), (3, 0)]),
            ("delay, shorter delay", d2, d1, 0, 0, [(half, 0), (new Rational(3, 2), 0), (3, 0)]),
            ("delay, same delay", d1, d1, 0, 0, [(half, 0), (2, 0)]),
            // before 3, g = delta_5 takes 5 - t to reach f; after it, f is +inf and g takes 5 - t too
            ("finite then infinite, delay", srUntil3, d5, 5, 0, [(1, 4), (4, 1), (6, 0)]),
            // f(0) = 2, then 2 on ]0, 2[, then +inf: at 0, g = delta_4 takes 4 to reach 2
            ("positive at 0, then infinite", twoUntil2, new DelayServiceCurve(4), 4, 0, [(0, 4), (1, 3), (3, 1), (5, 0)]),
            // f(0) = 1 below the 3 it takes on ]0, 2[, then +inf: g = 4t takes 1/4 to reach 1, and never becomes +inf
            ("below its limit at 0, then infinite", oneThenThreeUntil2, rampTo8, Rational.PlusInfinity, 2, [(0, new Rational(1, 4)), (1, 0), (3, Rational.PlusInfinity)]),
        };
        foreach (var (id, f, g, hdev, at, values) in cases)
        {
            yield return [id, f, g, hdev, at, values];
            yield return [$"{id}, as generic curves", new Curve(f), new Curve(g), hdev, at, values];
        }
    }

    /// <summary>
    /// Checks the horizontal deviation of curves whose first operand becomes $+\infty$ against known values.
    /// </summary>
    [Theory]
    [MemberData(nameof(InfiniteFirstOperandCases))]
    public void HorizontalDeviationWithAnInfiniteFirstOperand(string id, Curve f, Curve g, Rational hdev, Rational at, (Rational t, Rational v)[] values)
    {
        Assert.True(Curve.HorizontalDeviation(f, g) == hdev, $"{id}: hDev {Curve.HorizontalDeviation(f, g)}, not {hdev}");
        Assert.True(Curve.HorizontalDeviationMeasuredAt(f, g) == at, $"{id}: measured at {Curve.HorizontalDeviationMeasuredAt(f, g)}, not {at}");
        var function = Curve.HorizontalDeviationFunction(f, g);
        foreach (var (t, v) in values)
            Assert.True(function.ValueAt(t) == v, $"{id}: {function.ValueAt(t)} at {t}, not {v}");
    }

    [Theory]
    [MemberData(nameof(GetVerticalDeviationTestCases))]
    public void VerticalDeviationTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.VerticalDeviation(a, b);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetVerticalDeviationTestCases))]
    public void VerticalDeviationTest_Deconvolution(Curve a, Curve b, Rational expected)
    {
        var deconvolution = Curve.Deconvolution(a, b);
        var result = deconvolution.ValueAt(0);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetVerticalDeviationArgTestCases))]
    public void VerticalDeviationArgTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.VerticalDeviationMeasuredAt(a, b);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// The settings are parallelism switches, so they change how the deviation is computed and not what it is.
    /// The cases cover both branches:
    /// the closed form for a sigma-rho against a rate-latency, and the general one over the difference of the two curves, which is where the settings reach.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetVerticalDeviationTestCases))]
    public void VerticalDeviationIsTheSameWithSettingsAsWithout(Curve a, Curve b, Rational expected)
    {
        var sequential = Curve.VerticalDeviation(a, b, new ComputationSettings { UseParallelism = false });
        var parallel = Curve.VerticalDeviation(a, b, new ComputationSettings { UseParallelism = true });

        Assert.Equal(expected, sequential);
        Assert.Equal(expected, parallel);
    }

    /// <inheritdoc cref="VerticalDeviationIsTheSameWithSettingsAsWithout"/>
    [Theory]
    [MemberData(nameof(GetVerticalDeviationArgTestCases))]
    public void VerticalDeviationMeasuredAtIsTheSameWithSettingsAsWithout(Curve a, Curve b, Rational expected)
    {
        var sequential = Curve.VerticalDeviationMeasuredAt(a, b, new ComputationSettings { UseParallelism = false });
        var parallel = Curve.VerticalDeviationMeasuredAt(a, b, new ComputationSettings { UseParallelism = true });

        Assert.Equal(expected, sequential);
        Assert.Equal(expected, parallel);
    }

    [Theory]
    [MemberData(nameof(GetZDeviationTestCases))]
    public void ZDeviationTest(Curve f, Curve g, Rational expected)
    {
        var result = Curve.ZDeviation(f, g);
        Assert.Equal(expected, result);
    }

    public static List<(Curve f, Curve g)> NegativeFirstOperandTuples =
    [
        (
            // negative at the origin alone, where the minimum with a shaper is 0 and the vertical shift takes it below zero
            Curve.Minimum(
                new StairCurve(30, 100).DelayBy(0) + new StairCurve(10, 100).DelayBy(0),
                new SigmaRhoArrivalCurve(35, 1)
            ).VerticalShift(-30),
            new RateLatencyServiceCurve(1, 0)
        ),
        (
            // negative over an interval
            new SigmaRhoArrivalCurve(0, 2) - 7,
            new RateLatencyServiceCurve(2, 5)
        ),
        (
            new StairCurve(30, 100).DelayBy(0).ToRightContinuous() - 50,
            new RateLatencyServiceCurve(1, 0)
        ),
    ];

    public static IEnumerable<object[]> GetNegativeFirstOperandTestCases()
        => NegativeFirstOperandTuples.ToXUnitTestCases();

    /// <summary>
    /// The first operand must be non-negative, and the check is at every point.
    /// The rejection must name that first argument, the one the caller wrote.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetNegativeFirstOperandTestCases))]
    public void HorizontalDeviationRejectsANegativeFirstOperand(Curve f, Curve g)
    {
        Assert.False(f.IsNonNegative);

        foreach (var call in new Func<object>[]
                 {
                     () => Curve.HorizontalDeviation(f, g),
                     () => Curve.HorizontalDeviationMeasuredAt(f, g),
                     () => Curve.HorizontalDeviationFunction(f, g)
                 })
        {
            var thrown = Assert.Throws<ArgumentException>(() => call());
            Assert.Contains("first argument", thrown.Message);
        }
    }

    /// <summary>
    /// Clamping the first operand at zero does not change the horizontal deviation:
    /// for any $g \ge 0$ and any $t$ where $f(t) &lt; 0$, both $f(t)$ and $0$ lie below $g(t)$ and the infimum is 0 either way.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetNegativeFirstOperandTestCases))]
    public void ClampingTheFirstOperandPreservesTheHorizontalDeviation(Curve f, Curve g)
    {
        var clamped = f.ToNonNegative();
        Assert.True(clamped.IsNonNegative);
        Assert.Equal(f.IsNonDecreasing, clamped.IsNonDecreasing);

        var hDev = Curve.HorizontalDeviation(clamped, g);

        // where f is already non-negative the two operands agree, so they ask the same crossing of g
        foreach (var t in new Rational[] { 1, 2, new Rational(7, 2), 10, 100, 250 })
            if (f.ValueAt(t) >= 0)
                Assert.Equal(f.ValueAt(t), clamped.ValueAt(t));

        Assert.True(hDev >= 0);
    }

    public static IEnumerable<object[]> GetDominanceTestCases()
    {
        var testcases = new List<(Curve ac, Curve sc_a, Curve sc_b)>
        {
            (
                ac: new SigmaRhoArrivalCurve(1, 3),
                sc_a: new RateLatencyServiceCurve(5, 2),
                sc_b: new RateLatencyServiceCurve(4, 3)
            ),
            #if BIG_RATIONAL
            (
                ac: Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}"),
                sc_a: Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}"),
                sc_b: Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":780469,\"den\":64000000},\"latency\":{\"num\":27439,\"den\":40}}")
            ),
            #endif
        };

        foreach (var (ac, sc_a, sc_b) in testcases)
            yield return new object[] { ac, sc_a, sc_b };
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    public void DominanceVsHDev(Curve ac, Curve sc_a, Curve sc_b)
    {
        var (dominance, dominated_sc, dominant_sc) = Curve.Dominance(sc_a, sc_b);
        if (!dominance || ac.PseudoPeriodSlope > dominant_sc.PseudoPeriodSlope)
            throw new InvalidOperationException("Invalid test arguments");

        var dominant_hdev = Curve.HorizontalDeviation(ac, dominant_sc);
        var dominated_hdev = Curve.HorizontalDeviation(ac, dominated_sc);
        Assert.True(dominated_hdev >= dominant_hdev);
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    public void DominanceVsHDev_AsGeneric(Curve ac, Curve sc_a, Curve sc_b)
    {
        ac = new Curve(ac);
        sc_a = new Curve(sc_a);
        sc_b = new Curve(sc_b);
        var (dominance, dominated_sc, dominant_sc) = Curve.Dominance(sc_a, sc_b);
        if (!dominance || ac.PseudoPeriodSlope > dominant_sc.PseudoPeriodSlope)
            throw new InvalidOperationException("Invalid test arguments");

        var dominant_hdev = Curve.HorizontalDeviation(ac, dominant_sc);
        var dominated_hdev = Curve.HorizontalDeviation(ac, dominated_sc);
        Assert.True(dominated_hdev >= dominant_hdev);
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    public void DominanceVsVDev(Curve ac, Curve sc_a, Curve sc_b)
    {
        var (dominance, dominated_sc, dominant_sc) = Curve.Dominance(sc_a, sc_b);
        if (!dominance || ac.PseudoPeriodSlope > dominant_sc.PseudoPeriodSlope)
            throw new InvalidOperationException("Invalid test arguments");

        var dominant_hdev = Curve.HorizontalDeviation(ac, dominant_sc);
        var dominated_hdev = Curve.HorizontalDeviation(ac, dominated_sc);
        Assert.True(dominated_hdev >= dominant_hdev);
    }

    [Theory]
    [MemberData(nameof(GetDominanceTestCases))]
    public void DominanceVsVDev_AsGeneric(Curve ac, Curve sc_a, Curve sc_b)
    {
        ac = new Curve(ac);
        sc_a = new Curve(sc_a);
        sc_b = new Curve(sc_b);
        var (dominance, dominated_sc, dominant_sc) = Curve.Dominance(sc_a, sc_b);
        if (!dominance || ac.PseudoPeriodSlope > dominant_sc.PseudoPeriodSlope)
            throw new InvalidOperationException("Invalid test arguments");

        var dominant_vdev = Curve.VerticalDeviation(ac, dominant_sc);
        var dominated_vdev = Curve.VerticalDeviation(ac, dominated_sc);
        Assert.True(dominated_vdev >= dominant_vdev);
    }

    /// <summary>
    /// Curves used to check <see cref="Curve.HorizontalDeviationFunction"/> against independent computations of the same quantity.
    /// </summary>
    public static List<Curve> PropertyCurves =
    [
        new SigmaRhoArrivalCurve(3, 1),
        new SigmaRhoArrivalCurve(0, 2),
        new RateLatencyServiceCurve(2, 3),
        new RateLatencyServiceCurve(1, 1),
        new Curve(
            new Sequence([
                Point.Origin(), Segment.Zero(0, 2), new Point(2, 0), new Segment(2, 3, 0, 2),
                new Point(3, 2), Segment.Constant(3, 5, 2), new Point(5, 2), new Segment(5, 6, 2, 2)
            ]), 3, 3, 4),
        new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 4), new Segment(2, 4, 4, 1)
            ]), 2, 2, 2),
    ];

    public static IEnumerable<object[]> PropertyCurvePairs =>
        PropertyCurves.SelectMany(f => PropertyCurves.Select(g => new object[] { f, g }));

    public static IEnumerable<object[]> PropertyCurveTestCases => PropertyCurves.Select(f => new object[] { f });

    /// <summary>
    /// $\inf\{ x : g(x) \ge v \}$, found by walking the elements of $g$ and solving within each.
    /// Deliberately independent of the pseudo-inverse and composition the operator is built from.
    /// </summary>
    private static Rational? FirstCrossing(Curve g, Rational v, Rational horizon)
    {
        foreach (var e in g.Cut(0, horizon, true, true).Elements)
        {
            switch (e)
            {
                case Point p:
                    if (p.Value >= v) return p.Time;
                    break;

                case Segment seg:
                    if (seg.LeftLimitAtEndTime < v) break;
                    if (seg.Slope == 0) return seg.StartTime;
                    var x = seg.StartTime + (v - seg.RightLimitAtStartTime) / seg.Slope;
                    return x <= seg.StartTime ? seg.StartTime : x;
            }
        }
        return null;
    }

    [Theory]
    [MemberData(nameof(PropertyCurvePairs))]
    public void HorizontalDeviationFunctionMatchesTheCrossing(Curve f, Curve g)
    {
        var horizon = new Rational(200);
        var h = Curve.HorizontalDeviationFunction(f, g);

        for (var i = 0; i <= 24; i++)
        {
            var t = new Rational(i, 2);
            var crossing = FirstCrossing(g, f.ValueAt(t), horizon);
            if (crossing is null)
                continue;   // g does not reach f(t) below the horizon

            var expected = crossing.Value - t;
            if (expected < 0) expected = 0;
            Assert.Equal(expected, h.ValueAt(t));
        }
    }

    /// <summary>
    /// For a token bucket against a rate-latency curve the deviation has a closed form, whose supremum is the familiar delay bound $T + \sigma / R$.
    /// </summary>
    [Theory]
    [InlineData(3, 1, 2, 3)]
    [InlineData(5, 2, 4, 1)]
    [InlineData(0, 1, 3, 2)]
    public void HorizontalDeviationFunctionOfATokenBucketIsKnown(int sigma, int rho, int rate, int latency)
    {
        var a = new SigmaRhoArrivalCurve(sigma, rho);
        var b = new RateLatencyServiceCurve(rate, latency);
        var h = Curve.HorizontalDeviationFunction(a, b);

        for (var i = 1; i <= 8; i++)
        {
            var t = new Rational(i, 2);
            var expected = latency + (sigma + rho * t) / rate - t;
            if (expected < 0) expected = 0;
            Assert.Equal(expected, h.ValueAt(t));
        }

        Assert.Equal(latency + new Rational(sigma, rate), Curve.HorizontalDeviation(a, b));
    }

    [Theory]
    [MemberData(nameof(PropertyCurveTestCases))]
    public void HorizontalDeviationFunctionOfACurveFromItselfIsZero(Curve f)
    {
        Assert.True(Curve.HorizontalDeviationFunction(f, f).IsZero);
    }

    /// <summary>
    /// Raising or delaying the second operand moves every crossing in the direction the deviation follows.
    /// Unlike the deviation between sequences, this one is monotone in $g$:
    /// there is no overlap of images to move.
    /// </summary>
    [Theory]
    [MemberData(nameof(PropertyCurvePairs))]
    public void HorizontalDeviationFunctionIsMonotoneInItsSecondOperand(Curve f, Curve g)
    {
        var h = Curve.HorizontalDeviationFunction(f, g);
        Assert.True(h.IsNonNegative);
        Assert.True(Curve.HorizontalDeviationFunction(f, g + 1) <= h);
        Assert.True(Curve.HorizontalDeviationFunction(f, g.DelayBy(new Rational(3, 2))) >= h);
    }

    #region Every strategy

    private static readonly ComputationSettings ByPseudoInverses = new() { HorizontalDeviationStrategy = HorizontalDeviationStrategy.PseudoInverseGap };
    private static readonly ComputationSettings ByComposition = new() { HorizontalDeviationStrategy = HorizontalDeviationStrategy.Composition };
    private static readonly ComputationSettings[] EveryStrategy = Enum.GetValues<HorizontalDeviationStrategy>()
        .Select(strategy => new ComputationSettings { HorizontalDeviationStrategy = strategy })
        .ToArray();

    /// <summary>
    /// $-\infty$ on $[0, 1[$, then $t$.
    /// </summary>
    private static readonly Curve MinusThenRate = new(
        new Sequence([ Point.MinusInfinite(0), Segment.MinusInfinite(0, 1), new Point(1, 1), new Segment(1, 2, 1, 1) ]),
        1, 1, 1);

    /// <summary>
    /// Pairs whose horizontal deviation is computed by hand, from $hDev(f, g, t) = \inf\{ d \ge 0 \mid f(t) \le g(t + d) \}$,
    /// where an operand is bounded, takes the value $+\infty$ or $-\infty$, or the first is $+\infty$ everywhere.
    /// </summary>
    public static IEnumerable<object[]> BoundedAndInfiniteOperandCases()
    {
        var rate = new RateLatencyServiceCurve(1, 0);
        var halfRate = new RateLatencyServiceCurve(new Rational(1, 2), 0);
        var cases = new List<(string id, Curve f, Curve g, Rational hdev)>
        {
            // g stays at 3, so never reaches f(t) = t for t > 3
            ("g bounded", rate, Curve.Minimum(rate, new ConstantCurve(3)), Rational.PlusInfinity),
            // g = t/2 up to 3 reaches f(t) = t at 2t for t <= 2, and f = 2 at 4 after
            ("both bounded", Curve.Minimum(rate, new ConstantCurve(2)), Curve.Minimum(halfRate, new ConstantCurve(3)), 2),
            // g = delta_3 reaches every height above 0 at 3, and f = min(t, 2) is above 0 from 0 on
            ("f bounded, g reaches +inf", Curve.Minimum(rate, new ConstantCurve(2)), new DelayServiceCurve(3), 3),
            // f = floor(t) up to 3, taking 3 at 3, then +inf: g = delta_5 takes 5 - t to reach f(t) = floor(t) >= 1, at most 4 at t = 1; the rate-latency curve never reaches +inf
            ("f jumps to +inf above its left limit, g = delta_5", Curve.Maximum(rate.Floor(), new DelayServiceCurve(3)), new DelayServiceCurve(5), 4),
            ("f jumps to +inf above its left limit, g never +inf", Curve.Maximum(rate.Floor(), new DelayServiceCurve(3)), new RateLatencyServiceCurve(2, 1), Rational.PlusInfinity),
            // f becomes +inf after 1, and g never does
            ("f reaches +inf, g bounded", new DelayServiceCurve(1), Curve.Minimum(rate, new ConstantCurve(3)), Rational.PlusInfinity),
            // f is +inf everywhere, and g = delta_2 becomes so after 2
            ("f +inf everywhere", Curve.PlusInfinite(), new DelayServiceCurve(2), 2),
            ("f +inf everywhere, g never +inf", Curve.PlusInfinite(), rate, Rational.PlusInfinity),
            // g is -inf before 1, so reaches f(t) = t only from 1
            ("g -inf at first", rate, MinusThenRate, 1),
            // g is +inf everywhere, so it is above f at once
            ("g +inf everywhere", rate, Curve.PlusInfinite(), 0),
        };
        foreach (var (id, f, g, hdev) in cases)
            yield return [id, f, g, hdev];
    }

    /// <summary>
    /// Every pair of curves whose horizontal deviation is known: the known cases, those whose first operand becomes $+\infty$, and the bounded and infinite operands.
    /// </summary>
    public static IEnumerable<object[]> AllKnownHDevs()
    {
        foreach (var testCase in GetHorizontalDeviationTestCases())
            yield return ["known", testCase[0], testCase[1], testCase[2]];
        foreach (var testCase in InfiniteFirstOperandCases())
            yield return [testCase[0], testCase[1], testCase[2], testCase[3]];
        foreach (var testCase in BoundedAndInfiniteOperandCases())
            yield return testCase;
    }

    /// <summary>
    /// Every strategy gives the known value, see <see cref="HorizontalDeviationStrategy"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllKnownHDevs))]
    public void HorizontalDeviationIsTheSameUnderEveryStrategy(string id, Curve f, Curve g, Rational expected)
    {
        foreach (var settings in EveryStrategy)
        {
            var result = Curve.HorizontalDeviation(f, g, settings);
            Assert.True(result == expected, $"{id}: {result} by {settings.HorizontalDeviationStrategy}, not {expected}");
        }
    }

    /// <summary>
    /// The strategies agree on the property pairs, as generic curves, so that none takes the closed form of a token bucket against a rate-latency curve.
    /// </summary>
    [Theory]
    [MemberData(nameof(PropertyCurvePairs))]
    public void HorizontalDeviationStrategiesAgree(Curve f, Curve g)
    {
        var expected = Curve.HorizontalDeviation(new Curve(f), new Curve(g), ByPseudoInverses);
        foreach (var settings in EveryStrategy)
            Assert.Equal(expected, Curve.HorizontalDeviation(new Curve(f), new Curve(g), settings));
    }

    /// <summary>
    /// The known pairs the hypotheses of [PS22] Lemma 25 hold for: non-decreasing curves with finite values that are not bounded above,
    /// recognised by their lower pseudo-inverses being finite everywhere.
    /// </summary>
    public static IEnumerable<object[]> PollexSlomkaLemma25Cases()
    {
        foreach (var testCase in AllKnownHDevs())
        {
            var (f, g) = ((Curve) testCase[1], (Curve) testCase[2]);
            if (f.IsFinite && g.IsFinite && g.IsNonNegative && f.LowerPseudoInverse().IsFinite && g.LowerPseudoInverse().IsFinite)
                yield return testCase;
        }
    }

    /// <summary>
    /// [PS22] Lemma 25: for non-decreasing curves with finite values that are not bounded above,
    /// the horizontal deviation is the vertical deviation between the lower pseudo-inverses of the second and the first operand.
    /// It is taken here through <see cref="Curve.VerticalDeviation(Curve, Curve, ComputationSettings?)"/>.
    /// </summary>
    [Theory]
    [MemberData(nameof(PollexSlomkaLemma25Cases))]
    public void HorizontalDeviationIsTheVerticalDeviationBetweenPseudoInverses(string id, Curve f, Curve g, Rational expected)
    {
        var vdev = Curve.VerticalDeviation(g.LowerPseudoInverse(), f.LowerPseudoInverse());
        Assert.True(vdev == expected, $"{id}: {vdev} between the pseudo-inverses, not {expected}");
    }

    /// <summary>
    /// The known pairs with a finite horizontal deviation, between curves with finite values and a non-negative second operand.
    /// </summary>
    public static IEnumerable<object[]> DominatingShiftCases()
    {
        foreach (var testCase in AllKnownHDevs())
        {
            var (f, g, hdev) = ((Curve) testCase[1], (Curve) testCase[2], (Rational) testCase[3]);
            if (f.IsFinite && g.IsNonNegative && hdev.IsFinite)
                yield return testCase;
        }
    }

    /// <summary>
    /// [PS22] Lemma 54: for non-decreasing curves, the horizontal deviation is the smallest shift of the first operand that the second dominates,
    /// $\inf\{ \mu \ge 0 \mid \forall t, f(t) \le g(t + \mu) \}$, which is the same as delaying $f$ by $\mu$ and staying below $g$.
    /// The infimum need not be attained, so the shifts $d + \epsilon$ and $d - \epsilon$ are checked.
    /// </summary>
    [Theory]
    [MemberData(nameof(DominatingShiftCases))]
    public void HorizontalDeviationIsTheSmallestDominatingShift(string id, Curve f, Curve g, Rational expected)
    {
        var epsilon = new Rational(1, 1000);
        Assert.True(f.DelayBy(expected + epsilon) <= g, $"{id}: f delayed by {expected} + {epsilon} is not below g");
        if (expected > epsilon)
            Assert.False(f.DelayBy(expected - epsilon) <= g, $"{id}: f delayed by {expected} - {epsilon} is below g");
    }

    /// <summary>
    /// Pairs of curves with plateaus and jumps, whose upper pseudo-inverses are finite and whose first operand is non-negative.
    /// Chosen out of seeded random ones by covering, with the fewest and smallest, each kind of tail, continuity, plateau and jump, the relation of the two, and whether the deviation is 0, finite or $+\infty$.
    /// </summary>
    private static readonly (Curve f, Curve g)[] UpperAndLowerPseudoInverseCurves =
    [
        (
            new StairCurve(1, 3),
            new StairCurve(1, 4)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,4,new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 4, pseudoPeriodHeight: 6),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,5,3,new Rational(1, 4)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 5, pseudoPeriodHeight: new Rational(17, 4))
        ),
        (
            new RateLatencyServiceCurve(1, 0),
            new RateLatencyServiceCurve(1, 5)
        ),
        (
            new StairCurve(2, 2),
            new RateLatencyServiceCurve(new Rational(1, 3), 3)
        ),
        (
            new RateLatencyServiceCurve(new Rational(2, 3), 1),
            new RateLatencyServiceCurve(4, 0)
        ),
        (
            new RateLatencyServiceCurve(new Rational(4, 3), 1),
            new SigmaRhoArrivalCurve(3, new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,0), new Point(1,0), new Segment(1,2,4,0), new Point(2,4), new Segment(2,4,7,0), new Point(4,7), new Segment(4,5,10,0) }), pseudoPeriodStart: 1, pseudoPeriodLength: 4, pseudoPeriodHeight: 10),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,3,0), new Point(3,3), new Segment(3,4,8,2), new Point(4,10), new Segment(4,6,10,2), new Point(6,14), new Segment(6,10,17,2) }), pseudoPeriodStart: 4, pseudoPeriodLength: 6, pseudoPeriodHeight: 15)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,5,0), new Point(1,5), new Segment(1,2,9,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: 9),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,4,0), new Point(1,4), new Segment(1,3,5,0), new Point(3,5), new Segment(3,5,9,0), new Point(5,9), new Segment(5,6,10,0), new Point(6,10), new Segment(6,9,14,0), new Point(9,14), new Segment(9,12,19,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 12, pseudoPeriodHeight: 19)
        )
    ];

    public static IEnumerable<object[]> UpperAndLowerPseudoInverseCurvePairs()
        => UpperAndLowerPseudoInverseCurves.Select(p => new object[] { p.f, p.g });

    /// <summary>
    /// Pairs of curves, some raised at the origin, some bounded or $+\infty$ after a delay.
    /// Chosen out of seeded random ones by covering, with the fewest and smallest, each kind of tail, continuity, plateau and jump, the relation of the two, and whether the deviation is 0, finite or $+\infty$.
    /// </summary>
    private static readonly (Curve f, Curve g)[] CurvesOfAnyKind =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,3,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,3,0), new Point(1,3), new Segment(1,2,3,0) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 0)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,0,0), new Point(2,0), new Segment(2,8,2,3) }), pseudoPeriodStart: 2, pseudoPeriodLength: 6, pseudoPeriodHeight: 20),
            new StairCurve(1, 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,6,2,0), new Point(6,2), new Segment(6,7,2,new Rational(1, 2)) }), pseudoPeriodStart: 6, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(1, 2)),
            new SigmaRhoArrivalCurve(0, 4)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 4),0,4), new Point(new Rational(3, 4),3), new Segment(new Rational(3, 4),new Rational(7, 4),3,0) }), pseudoPeriodStart: new Rational(3, 4), pseudoPeriodLength: 1, pseudoPeriodHeight: 0),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,2,0), new Point(4,2), new Segment(4,5,2,new Rational(1, 4)), new Point(5,new Rational(9, 4)), new Segment(5,9,new Rational(17, 4),new Rational(1, 4)) }), pseudoPeriodStart: 4, pseudoPeriodLength: 5, pseudoPeriodHeight: new Rational(13, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,0), new Point(3,0), new Segment(3,8,4,0) }), pseudoPeriodStart: 3, pseudoPeriodLength: 5, pseudoPeriodHeight: 4),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,4), new Segment(0,2,4,0), new Point(2,4), new Segment(2,3,7,new Rational(3, 2)), new Point(3,new Rational(17, 2)), new Segment(3,4,new Rational(17, 2),new Rational(3, 2)) }), pseudoPeriodStart: 3, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,5,1,0), new Point(5,1), new Segment(5,7,1,new Rational(4, 3)), new Point(7,new Rational(11, 3)), new Segment(7,14,new Rational(1, 0),0), new Point(14,new Rational(1, 0)), new Segment(14,21,new Rational(1, 0),0) }), pseudoPeriodStart: 14, pseudoPeriodLength: 7, pseudoPeriodHeight: 0),
            new StairCurve(3, 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,6,0), new Point(1,6), new Segment(1,2,9,new Rational(2, 3)), new Point(2,new Rational(29, 3)), new Segment(2,3,new Rational(38, 3),new Rational(2, 3)), new Point(3,new Rational(40, 3)), new Segment(3,4,new Rational(58, 3),new Rational(2, 3)) }), pseudoPeriodStart: 1, pseudoPeriodLength: 3, pseudoPeriodHeight: 14),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,4,0,0), new Point(4,0), new Segment(4,7,2,0) }), pseudoPeriodStart: 4, pseudoPeriodLength: 3, pseudoPeriodHeight: 2)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,3,0), new Point(3,3), new Segment(3,4,4,0), new Point(4,4), new Segment(4,8,new Rational(1, 0),0), new Point(8,new Rational(1, 0)), new Segment(8,12,new Rational(1, 0),0) }), pseudoPeriodStart: 8, pseudoPeriodLength: 4, pseudoPeriodHeight: 0),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,4,new Rational(1, 2)), new Point(3,new Rational(11, 2)), new Segment(3,6,new Rational(1, 0),0), new Point(6,new Rational(1, 0)), new Segment(6,9,new Rational(1, 0),0) }), pseudoPeriodStart: 6, pseudoPeriodLength: 3, pseudoPeriodHeight: 0)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,7,0), new Point(2,7), new Segment(2,4,11,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 4, pseudoPeriodHeight: 11),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,3,3,0), new Point(3,3), new Segment(3,5,5,0), new Point(5,5), new Segment(5,7,6,0), new Point(7,6), new Segment(7,10,8,0), new Point(10,8), new Segment(10,11,9,0), new Point(11,9), new Segment(11,15,11,0), new Point(15,11), new Segment(15,19,14,0), new Point(19,14), new Segment(19,20,16,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 20, pseudoPeriodHeight: 14)
        )
    ];

    public static IEnumerable<object[]> CurvePairsOfAnyKind()
        => CurvesOfAnyKind.Select(p => new object[] { p.f, p.g });

    /// <summary>
    /// Checks that every strategy gives the same horizontal deviation on curves of any kind: raised at the origin, bounded, or $+\infty$ after a delay.
    /// </summary>
    [Theory]
    [MemberData(nameof(CurvePairsOfAnyKind))]
    public void HorizontalDeviationIsTheSameUnderEveryStrategyOnCurvesOfAnyKind(Curve f, Curve g)
    {
        var expected = Curve.HorizontalDeviation(new Curve(f), new Curve(g), ByPseudoInverses);
        foreach (var settings in EveryStrategy)
            Assert.Equal(expected, Curve.HorizontalDeviation(new Curve(f), new Curve(g), settings));
    }

    /// <summary>
    /// With $f(0) = 0$, the gap between the upper pseudo-inverses gives the same deviation as the one between the lower pseudo-inverses, and as composition, [TBP-SEQ-DEV] § A.2.
    /// </summary>
    [Theory]
    [MemberData(nameof(UpperAndLowerPseudoInverseCurvePairs))]
    public void HorizontalDeviationIsTheSameWithUpperAndLowerPseudoInverses(Curve f, Curve g)
    {
        var byUpper = HorizontalDeviationStrategies.UpperDeconvolutionAlone(f, g);
        var byLower = Curve.HorizontalDeviation(new Curve(f), new Curve(g), ByPseudoInverses);
        var byComposition = Curve.HorizontalDeviation(new Curve(f), new Curve(g), ByComposition);
        Assert.Equal(byLower, byComposition);
        Assert.Equal(byLower, byUpper);
    }

    /// <summary>
    /// With $f(0) > 0$ the upper pseudo-inverse of $f$ is $-\infty$ below $f(0)$, and the gap between the upper pseudo-inverses is $+\infty$ although the deviation is finite:
    /// $f(t) = 2 + t/2$ with $f(0) = 2$, against a rate-latency curve, has deviation 4, as it has with $f(0) = 0$ and $f(0^+) = 2$, [TBP-SEQ-DEV] § A.2.
    /// </summary>
    [Fact]
    public void UpperPseudoInverseFormIsInfiniteWhenTheFirstOperandStartsAbove0()
    {
        var g = new RateLatencyServiceCurve(1, 2);
        var f = new Curve(new Sequence([ new Point(0, 2), new Segment(0, 1, 2, new Rational(1, 2)) ]), 0, 1, new Rational(1, 2));
        var fFrom0Plus = new SigmaRhoArrivalCurve(2, new Rational(1, 2));

        foreach (var settings in EveryStrategy)
            Assert.Equal(4, Curve.HorizontalDeviation(f, g, settings));
        Assert.False(f.UpperPseudoInverse().IsFinite);
        Assert.Equal(Rational.PlusInfinity, HorizontalDeviationStrategies.UpperDeconvolutionAlone(f, g));
        Assert.Equal(4, HorizontalDeviationStrategies.UpperDeconvolutionAlone(fFrom0Plus, g));
    }

    /// <summary>
    /// A first operand that takes the value $-\infty$ is negative, and rejected under every strategy.
    /// </summary>
    [Fact]
    public void MinusInfiniteFirstOperandIsRejectedUnderEveryStrategy()
    {
        foreach (var settings in EveryStrategy)
            Assert.Throws<ArgumentException>(() => Curve.HorizontalDeviation(MinusThenRate, new RateLatencyServiceCurve(1, 0), settings));
    }

    #endregion Every strategy

    /// <summary>
    /// The example of the introduction of [TBP-SEQ-DEV]: a token bucket $\alpha(t) = 2 + t/2$, $\alpha(0) = 0$, and a rate-latency $\beta(t) = (t - 2)^+$.
    /// At $t = 1$, $vDev(\alpha, \beta, 1) = 5/2$ and $hDev(\alpha, \beta, 1) = 7/2$; the suprema are $vDev(\alpha, \beta) = 3$, at the latency, and $hDev(\alpha, \beta) = 4$, approached as $t$ tends to 0.
    /// </summary>
    [Fact]
    public void TheIntroductionExampleOfTheReport()
    {
        var half = new Rational(1, 2);
        var alpha = new SigmaRhoArrivalCurve(2, half);
        var beta = new RateLatencyServiceCurve(1, 2);
        foreach (var (f, g) in new[] { ((Curve) alpha, (Curve) beta), (new Curve(alpha), new Curve(beta)) })
        {
            Assert.Equal(new Rational(5, 2), (f - g).ValueAt(1));
            Assert.Equal(new Rational(7, 2), Curve.HorizontalDeviationFunction(f, g).ValueAt(1));
            Assert.Equal(3, Curve.VerticalDeviation(f, g));
            Assert.Equal(2, Curve.VerticalDeviationMeasuredAt(f, g));
            foreach (var settings in EveryStrategy)
                Assert.Equal(4, Curve.HorizontalDeviation(f, g, settings));
        }
    }

    /// <summary>
    /// Pairs of curves taking the value $+\infty$ or $-\infty$, with their vertical deviation, [TBP-SEQ-DEV] § 3:
    /// where one curve alone is infinite, or the two are infinite with opposite signs, the difference is that of the infinite terms.
    /// </summary>
    public static IEnumerable<object[]> InfiniteVDevCases()
    {
        var rate = new RateLatencyServiceCurve(1, 0);
        var delta1 = new DelayServiceCurve(1);
        var delta2 = new DelayServiceCurve(2);
        yield return ["g +inf after 2", rate, delta2, (Rational) 2];
        yield return ["f +inf after 2", delta2, rate, Rational.PlusInfinity];
        yield return ["f -inf before 1", MinusThenRate, rate, (Rational) 0];
        yield return ["g -inf before 1", rate, MinusThenRate, Rational.PlusInfinity];
        yield return ["f +inf after 1, g -inf before 1", delta1, MinusThenRate, Rational.PlusInfinity];
        // f(1) - g(1) = 1, and -inf before and after
        yield return ["f -inf before 1, g +inf after 1", MinusThenRate, delta1, (Rational) 1];
    }

    [Theory]
    [MemberData(nameof(InfiniteVDevCases))]
    public void VerticalDeviationOverInfiniteValues(string id, Curve f, Curve g, Rational expected)
    {
        var vdev = Curve.VerticalDeviation(f, g);
        Assert.True(vdev == expected, $"{id}: {vdev}, not {expected}");
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 3: where both curves are $+\infty$, or both $-\infty$, at some time, their difference there has no value, and neither has the deviation.
    /// </summary>
    [Fact]
    public void VerticalDeviationIsUndefinedWhereBothAreTheSameInfinity()
    {
        Assert.Throws<UndeterminedResultException>(() => Curve.VerticalDeviation(new DelayServiceCurve(1), new DelayServiceCurve(2)));
        Assert.Throws<UndeterminedResultException>(() => Curve.VerticalDeviation(MinusThenRate, MinusThenRate));
    }
}
