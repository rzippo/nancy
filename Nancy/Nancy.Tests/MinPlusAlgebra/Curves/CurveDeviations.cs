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
                a: Curve.Minimum(
                    new RateLatencyServiceCurve(4, 0),
                    new ConstantCurve(12)
                ),
                b: Curve.Minimum(
                    new RateLatencyServiceCurve(3, 3),
                    new ConstantCurve(12)
                ),
                4
            )
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
            .MaxValue();

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
}