using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveDeviations
{
    public static IEnumerable<object[]> GetHorizontalTestCases()
    {
        var testcases = new List<(Curve a, Curve b, Rational expected)>
        {
            (
                a: new SigmaRhoArrivalCurve(4, 3),
                b: new RateLatencyServiceCurve(4, 3),
                expected: 4
            ),
            (
                a: new Curve( new SigmaRhoArrivalCurve(4, 3)),
                b: new Curve( new RateLatencyServiceCurve(4, 3)),
                expected: 4
            ),
            (
                a: new SigmaRhoArrivalCurve(4, 5),
                b: new RateLatencyServiceCurve(4, 3),
                expected: Rational.PlusInfinity
            ),
            (
                a: new Curve(new SigmaRhoArrivalCurve(4, 5)),
                b: new Curve(new RateLatencyServiceCurve(4, 3)),
                expected: Rational.PlusInfinity
            ),
            (
                // Example from [2] Figure 5.7
                a: new SigmaRhoArrivalCurve(1, 1),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 0),
                    new RateLatencyServiceCurve(3, 4) + 3
                ),
                2
            ),
            (
                // Variation on the example from [2] Figure 5.7
                a: new SigmaRhoArrivalCurve(1, 2),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 0),
                    new RateLatencyServiceCurve(3, 4) + 3
                ),
                3
            ),
            (
                a: new SigmaRhoArrivalCurve(2, 0),
                b: new RateLatencyServiceCurve(2, 2),
                3
            ),
            (
                a: Curve.Minimum(
                    new RateLatencyServiceCurve(4, 0),
                    new ConstantCurve(12)
                ),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 3),
                    new ConstantCurve(12)
                ),
                4
            ),
            #if BIG_RATIONAL
            (
                a: Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}"),
                b: Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}"),
                new Rational(115102801965691,149850048000)
            ),
            (
                a: new Curve(Curve.FromJson("{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":{\"num\":1,\"den\":1},\"rho\":{\"num\":2441407,\"den\":1000000000}}")),
                b: new Curve(Curve.FromJson("{\"type\":\"rateLatencyServiceCurve\",\"rate\":{\"num\":149850048000,\"den\":12309415288891},\"latency\":{\"num\":27439,\"den\":40}}")),
                new Rational(115102801965691,149850048000)
            )
            #endif
        };

        foreach (var (a, b, expected) in testcases)
            yield return new object[] { a, b, expected };
    }

    public static IEnumerable<object[]> GetVerticalTestCases()
    {
        var testcases = new List<(Curve a, Curve b, Rational expected)>
        {
            (
                a: new SigmaRhoArrivalCurve(4, 3),
                b: new RateLatencyServiceCurve(4, 3),
                expected: 13
            ),
            (
                a: new Curve( new SigmaRhoArrivalCurve(4, 3)),
                b: new Curve( new RateLatencyServiceCurve(4, 3)),
                expected: 13
            ),
            (
                a: new SigmaRhoArrivalCurve(4, 5),
                b: new RateLatencyServiceCurve(4, 3),
                expected: Rational.PlusInfinity
            ),
            (
                a: new Curve(new SigmaRhoArrivalCurve(4, 5)),
                b: new Curve(new RateLatencyServiceCurve(4, 3)),
                expected: Rational.PlusInfinity
            ),
            (
                // Example from [2] Figure 5.7
                a: new SigmaRhoArrivalCurve(1, 1),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 0),
                    new RateLatencyServiceCurve(3, 4) + 3
                ),
                2
            ),
            (
                // Variation on the example from [2] Figure 5.7
                a: new SigmaRhoArrivalCurve(1, 2),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 0),
                    new RateLatencyServiceCurve(3, 4) + 3
                ),
                6
            ),
            (
                a: new SigmaRhoArrivalCurve(2, 0),
                b: new RateLatencyServiceCurve(2, 2),
                2
            )
        };

        foreach (var (a, b, expected) in testcases)
            yield return new object[] { a, b, expected };
    }
    
    [Theory]
    [MemberData(nameof(GetHorizontalTestCases))]
    public void HorizontalDeviationTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.HorizontalDeviation(a, b);
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [MemberData(nameof(GetHorizontalTestCases))]
    public void HorizontalDeviationAlternativesTest(Curve a, Curve b, Rational expected)
    {
        // the following are mathematically equivalent methods to compute hdev(a, b)
        var a_upi = a.UpperPseudoInverse();
        var b_upi = b.UpperPseudoInverse();
        var hDev_1 = -Curve.MaxPlusDeconvolution(a_upi, b_upi).ValueAt(0);
        var b_lpi = b.LowerPseudoInverse();
        var hDev_2 = b_lpi
            .Composition(a)
            .Deconvolution(new RateLatencyServiceCurve(1, 0))
            .ValueAt(0);
        var hDev_3 = b_lpi
            .Composition(a)
            .Subtraction(new RateLatencyServiceCurve(1, 0))
            .SupValue();

        Assert.Equal(hDev_1, hDev_2);
        Assert.Equal(hDev_2, hDev_3);
        Assert.Equal(expected, hDev_3);
    }
    
    [Theory]
    [MemberData(nameof(GetVerticalTestCases))]
    public void VerticalDeviationTest(Curve a, Curve b, Rational expected)
    {
        var result = Curve.VerticalDeviation(a, b);
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