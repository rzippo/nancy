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
    public static List<(Curve f, Curve g, Rational hDev, Rational time)> HDevKnownCases =
    [
        (
            f: new SigmaRhoArrivalCurve(4, 3),
            g: new RateLatencyServiceCurve(4, 3),
            hDev: 4,
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
        
        var a_upi = a.UpperPseudoInverse();
        var b_upi = b.UpperPseudoInverse();
        // todo: document source for this result
        var hDev_1 = Curve.MaxPlusDeconvolution(a_upi, b_upi)
            .Negate()
            .ToNonNegative()
            .ValueAt(0);

        var b_lpi = b.LowerPseudoInverse();
        // [DNC18] Proposition 5.14
        var hDev_2 = b_lpi
            .Composition(a)
            .Deconvolution(new RateLatencyServiceCurve(1, 0))
            .ValueAt(0);

        // Derived from [DNC18] Lemma 5.2 and similar, in principle, to Proposition 5.14
        var hDev_3 = b_lpi
            .Composition(a)
            .Subtraction(new RateLatencyServiceCurve(1, 0))
            .SupValue();

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

    [Theory]
    [MemberData(nameof(GetZDeviationTestCases))]
    public void ZDeviationTest(Curve f, Curve g, Rational expected)
    {
        var result = Curve.ZDeviation(f, g);
        Assert.Equal(expected, result);
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
}