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
    [MemberData(nameof(GetHorizontalDeviationTestCases))]
    public void HorizontalDeviationAlternativesTest(Curve a, Curve b, Rational expected)
    {
        // the following are mathematically equivalent methods to compute hdev(a, b)
        _testOutputHelper.WriteLine($"var a = {a.ToCodeString()}");
        _testOutputHelper.WriteLine($"var b = {b.ToCodeString()}");
        _testOutputHelper.WriteLine($"var expected = {expected.ToCodeString()}");

        // todo: document source for this result
        // This alternative is well-defined only if the pseudo-inverses are finite: 
        // otherwise the (max,+) deconvolution reaches +infty - (+infty) or -infty - (-infty), 
        // and whatever it returns is not the deviation.
        // The UPI is UltimatelyPlusInfinite if the curve is UC, and is -infty in 0 if f(0) > 0
        var a_upi = a.UpperPseudoInverse();
        var b_upi = b.UpperPseudoInverse();
        var doHDev1 = a_upi.IsFinite && b_upi.IsFinite;
        Rational hDev_1 = 0;
        if (doHDev1)
        {
            hDev_1 = Curve.MaxPlusDeconvolution(a_upi, b_upi)
                .Negate()
                .ToNonNegative()
                .ValueAt(0);
            _testOutputHelper.WriteLine($"var hDev_1 = {hDev_1.ToCodeString()}");
        }

        var b_lpi = b.LowerPseudoInverse();
        // [DNC18] Proposition 5.14
        var hDev_2 = b_lpi
            .Composition(a)
            .Deconvolution(new RateLatencyServiceCurve(1, 0))
            .ValueAt(0);
        _testOutputHelper.WriteLine($"var hDev_2 = {hDev_2.ToCodeString()}");

        // Derived from [DNC18] Lemma 5.2 and similar, in principle, to Proposition 5.14
        var hDev_3 = b_lpi
            .Composition(a)
            .Subtraction(new RateLatencyServiceCurve(1, 0))
            .SupValue();
        _testOutputHelper.WriteLine($"var hDev_3 = {hDev_3.ToCodeString()}");

        if (doHDev1)
            Assert.Equal(hDev_1, hDev_2);
        Assert.Equal(hDev_2, hDev_3);
        Assert.Equal(expected, hDev_3);
    }

    [Theory]
    [MemberData(nameof(GetHorizontalDeviationArgTestCases))]
    public void HorizontalDeviationArgTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.HorizontalDeviationMeasuredAt(a, b);
        Assert.Equal(expected, result);
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
    /// Curves used to check <see cref="Curve.HorizontalDeviationFunction"/> against
    /// independent computations of the same quantity.
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
    /// For a token bucket against a rate-latency curve the deviation has a closed form,
    /// whose supremum is the familiar delay bound $T + \sigma / R$.
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
    /// Raising or delaying the second operand moves every crossing in the direction the
    /// deviation follows. Unlike the deviation between sequences, this one is monotone in $g$:
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
}
