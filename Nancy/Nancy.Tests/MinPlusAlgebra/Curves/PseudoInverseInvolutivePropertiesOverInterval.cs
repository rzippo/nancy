using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Curves;
using Xunit;
using Xunit.Abstractions;

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
}