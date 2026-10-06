using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Curves;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurvePseudoInverseInvolutivePropertiesOverInterval
{
    private readonly ITestOutputHelper output;

    public CurvePseudoInverseInvolutivePropertiesOverInterval(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<Curve> LeftContinuousFunctions()
    {
        foreach (var objArray in LowerPseudoInverse.LeftContinuousTestCases())
        {
            var f = (Curve) objArray[0];
            // yield return new object[] { f };
            yield return f;
        }

        foreach (var curve in ConvolutionIsomorphism.LeftContinuousExamples)
        {
            yield return curve;
        }
    }

    public static IEnumerable<(Curve, Rational)> LeftContinuousFunctionAndCutPoint()
    {
        foreach (var f in LeftContinuousFunctions())
        {
            var a1 = f.PseudoPeriodStart;
            yield return (f, a1);
            if (f.PseudoPeriodStart > 0)
            {
                var a2 = f.PseudoPeriodStart / 2;
                yield return (f, a2);
            }

            var a3 = f.PseudoPeriodStart + 1.5m * f.PseudoPeriodLength;
            yield return (f, a3);
        }
    }

    public static IEnumerable<object[]> LpiOfUpiOfLeftContinuousOverIntervalTestCases()
    {
        foreach (var (f, a) in LeftContinuousFunctionAndCutPoint())
        {
            yield return new object[] { f, a };
        }
    }

    [Theory]
    [MemberData(nameof(LpiOfUpiOfLeftContinuousOverIntervalTestCases))]
    public void LpiOfUpiOfLeftContinuousOverInterval(Curve f, Rational a)
    {
        Assert.True(f.IsLeftContinuous);
        var (_, f_D) = f.Decompose(a, minDecomposition: true);
        var f_a = f.ValueAt(a);

        var upiD = f_D.UpperPseudoInverseOverInterval(a);
        var lpiD = upiD.LowerPseudoInverseOverInterval(f_a);

        if (f.GetSegmentAfter(a).IsConstant && f.IsRightContinuousAt(a))
        {
            var a_prime = f.GetSegmentAfter(a).EndTime;
            Assert.True(Curve.EquivalentAfter(f_D, lpiD, a_prime));
            Assert.True(Sequence.Equivalent(
                Sequence.PlusInfinite(0, a_prime),
                lpiD.Cut(0, a_prime)
            ));
        }
        else
        {
            Assert.True(Curve.Equivalent(f_D, lpiD));   
        }
    }

    public static IEnumerable<Curve> RightContinuousFunctions()
    {
        foreach (var objArray in UpperPseudoInverse.RightContinuousTestCases())
        {
            var f = (Curve) objArray[0];
            // yield return new object[] { f };
            yield return f;
        }

        foreach (var objArray in ConvolutionIsomorphism.MaxPlusIsomorphismTestCases())
        {
            var curves = (Curve[]) objArray[0];
            foreach (var f in curves)
            {
                yield return f;
                // yield return new object[] { curve };
            }
        }
    }

    public static IEnumerable<(Curve, Rational)> RightContinuousFunctionAndCutPoint()
    {
        foreach (var f in RightContinuousFunctions())
        {
            var a1 = f.PseudoPeriodStart;
            yield return (f, a1);
            if (f.PseudoPeriodStart > 0)
            {
                var a2 = f.PseudoPeriodStart / 2;
                yield return (f, a2);
            }

            var a3 = f.PseudoPeriodStart + 1.5m * f.PseudoPeriodLength;
            yield return (f, a3);
        }
    }

    public static IEnumerable<object[]> UpiOfLpiOfRightContinuousOverIntervalTestCases()
    {
        foreach (var (f, a) in RightContinuousFunctionAndCutPoint())
        {
            yield return new object[] { f, a };
        }
    }

    [Theory]
    [MemberData(nameof(UpiOfLpiOfRightContinuousOverIntervalTestCases))]
    public void UpiOfLpiOfRightContinuousOverInterval(Curve f, Rational a)
    {
        Assert.True(f.IsRightContinuous);
        var (_, f_D) = f.Decompose(a, minDecomposition: false);
        var f_a = f.ValueAt(a);

        var lpiD = f_D.LowerPseudoInverseOverInterval(a);
        var upiD = lpiD.UpperPseudoInverseOverInterval(f_a);

        Assert.True(Curve.Equivalent(f_D, upiD));
    }

    /// <summary>
    /// Left-continuous, non-decreasing curves, the start $a$ of an interval, and $a' = \sup\{ t \ge a \mid f(t) = f(a) \}$.
    /// The first three have $f(a) = 0$ and a plateau from $a$, so that the lower pseudo-inverse is taken over $[0, +\infty[$.
    /// </summary>
    public static List<(Curve f, Rational a, Rational aPrime)> InvolutionStartingAtAHeightOfZeroCases =
    [
        // 0 up to 2, then slope 1
        (new RateLatencyServiceCurve(1, 2), 0, 2),
        (new RateLatencyServiceCurve(1, 2), 1, 2),
        (new RateLatencyServiceCurve(1, 2), 2, 2),
        // 0 up to 1, then 1 up to 3, then slope 1: $f(a) = 0$ with a plateau that ends before the curve moves on
        (new Curve(new Sequence([ Point.Origin(), Segment.Constant(0, 1, 0), new Point(1, 0), new Segment(1, 2, 0, 1), new Point(2, 1), new Segment(2, 3, 1, 1) ]), 1, 2, 2), 0, 1),
        // for contrast, $f(0) > 0$: the lower pseudo-inverse is taken over $[2, +\infty[$
        (new Curve(new Sequence([ new Point(0, 2), new Segment(0, 1, 2, 1) ]), 0, 1, 1), 0, 0),
    ];

    public static IEnumerable<object[]> InvolutionStartingAtAHeightOfZeroTestCases()
        => InvolutionStartingAtAHeightOfZeroCases.ToXUnitTestCases();

    /// <summary>
    /// [ZNS26] Proposition 23: for left-continuous $f$ non-decreasing over $[a, +\infty[$, the lower pseudo-inverse over $[f(a), +\infty[$ of the upper pseudo-inverse of $f$ over $[a, +\infty[$
    /// is $f|^\wedge_{[a', +\infty[}$, which is $+\infty$ below $a'$, also when $f(a) = 0$ and the interval starts at 0.
    /// </summary>
    [Theory]
    [MemberData(nameof(InvolutionStartingAtAHeightOfZeroTestCases))]
    public void LpiOfUpiOverIntervalStartingAtAHeightOfZero(Curve f, Rational a, Rational aPrime)
    {
        Assert.True(f.IsLeftContinuous);
        var (_, f_D) = f.Decompose(a, minDecomposition: true);
        var f_a = f.ValueAt(a);

        var upiD = f_D.UpperPseudoInverseOverInterval(a);
        var lpiD = upiD.LowerPseudoInverseOverInterval(f_a);

        var (_, expected) = f.Decompose(aPrime, minDecomposition: true);
        Assert.True(Curve.Equivalent(expected, lpiD));
    }
}