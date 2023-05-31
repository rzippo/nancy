using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.CurvesOptimization;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public partial class ConvolutionIsomorphismOverInterval
{
    public static IEnumerable<object[]> UpiDecompositionEquivalenceTestCases()
    {
        var testcases = ContinuousExamples.Concat(LeftContinuousExamples);

        foreach (var c in testcases)
            yield return new object[] { c };
    }

    [Theory]
    [MemberData(nameof(UpiDecompositionEquivalenceTestCases))]
    public void UpiDecompositionEquivalence(Curve f)
    {
        // This test verifies that 
        // \upiD{ [0, T[ }{f_t} \vee \upiD{ [T, +\infty[ }{f_p} = \upi{f} = \upi{ f_t \wedge f_p } .
        // In other words, by decomposing (in (min,+) sense) and computing the \upiD of the parts we do not lose information on the whole.

        output.WriteLine(f.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var (f_t, f_p) = f.Decompose(T_f);
        var f_t_upi = f_t?.UpperPseudoInverseOverInterval(0, T_f);
        var f_p_upi = f_p.UpperPseudoInverseOverInterval(T_f);
        var upi_recomposed = f_t_upi == null ? f_p_upi : Curve.Maximum(f_t_upi, f_p_upi); 

        var upi = f.UpperPseudoInverse();

        Assert.True(Curve.Equivalent(upi, upi_recomposed));
    }

    [Theory]
    [MemberData(nameof(UpiDecompositionEquivalenceTestCases))]
    public void UpiDecompositionStrongEquivalence(Curve f)
    {
        // This test verifies that 
        // \upiD{ [0, T[ }{f_t} = \upi{f}_t, \upiD{ [T, +\infty[ }{f_p} = \upi{f}_p .
        // In other words, computing the \upiD of the transient and periodic parts is the same as computing the \upi of the whole
        // and then take its transient and periodic parts.

        // An exception is given by T = 0, f(0) > 0:
        // in this case, the function does not have a transient, but \upi{f}_t is the gap [0, f(0)[.

        // Otherwise, the theorem can be verified only if T is delayed so that the period does not start with a constant segment.
        // If not, the \upiD of the periodic part has a transient part itself, which alters the result.

        output.WriteLine(f.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var firstPeriodSegment = f.GetSegmentAfter(f.PseudoPeriodStart);
        var T_f_prime = firstPeriodSegment.IsConstant ? firstPeriodSegment.EndTime : f.PseudoPeriodStart;
        var f_T_f_prime = f.ValueAt(T_f_prime);

        var (f_t, f_p) = f.Decompose(T_f_prime);
        var f_t_upi = f_t?.UpperPseudoInverseOverInterval(0, T_f_prime);
        var f_p_upi = f_p.UpperPseudoInverseOverInterval(T_f_prime);

        var upi = f.UpperPseudoInverse();
        var (upi_t, upi_p) = upi.Decompose(f_T_f_prime, minDecomposition: false);

        if (f_t_upi == null || upi_t == null)
        {
            if (T_f == 0 && f.ValueAt(0) > 0)
            {
                Assert.Null(f_t_upi);
                Assert.NotNull(upi_t);
                Assert.True(Curve.Equivalent(Curve.MinusInfinite(), upi_t!));
                Assert.Equal(f.ValueAt(0), upi_p.FirstFiniteTime);
            }
            else
            {
                Assert.True(f_t_upi == upi_t);
            }
        }
        else
            // The cut is done since if f_t ends with a constant segment, f_t_upi will have extra information, namely f_t_upi(f_T_f)
            Assert.True(Sequence.Equivalent(
                f_t_upi.Cut(0, f_T_f_prime),
                upi_t.Cut(0, f_T_f_prime))
            );
        Assert.True(Curve.Equivalent(f_p_upi, upi_p));
    }

    public static IEnumerable<object[]> LpiDecompositionEquivalenceTestCases()
    {
        var testcases = ContinuousExamples.Concat(RightContinuousExamples);

        foreach (var c in testcases)
            yield return new object[] { c };
    }

    [Theory]
    [MemberData(nameof(LpiDecompositionEquivalenceTestCases))]
    public void LpiDecompositionEquivalence(Curve f)
    {
        // This test verifies that 
        // \lpiD{ [0, T[ }{f_t} \wedge \lpiD{ [T, +\infty[ }{f_p} = \lpi{f} = \lpi{ f_t \vee f_p } .
        // In other words, by decomposing (in (max,+) sense) and computing the \lpiD of the parts we do not lose information on the whole.

        output.WriteLine(f.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsRightContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsNonNegative);

        // The lpi decomposition needs to use [0, T_f] for the transient since, being the function right-continuous,
        // it may have a jump a the left of T_f.
        // In that case, using [0, T_f[ means that neither f_t nor f_p include said jump,
        // and so neither \lpiD{f_t} nor \lpiD{f_p} will have the constant segment representing it. 
        var T_f = f.PseudoPeriodStart;
        var (f_t, f_p) = f.Decompose(T_f, leftIncludesEndPoint: true, minDecomposition: false);
        var f_t_lpi = f_t?.LowerPseudoInverseOverInterval(0, T_f, isEndIncluded: true);
        var f_p_lpi = f_p.LowerPseudoInverseOverInterval(T_f);
        var lpi_recomposed = f_t_lpi == null ? f_p_lpi : Curve.Minimum(f_t_lpi, f_p_lpi); 

        var lpi = f.LowerPseudoInverse();

        Assert.True(Curve.Equivalent(lpi, lpi_recomposed));
    }

    [Theory]
    [MemberData(nameof(LpiDecompositionEquivalenceTestCases))]
    public void LpiDecompositionStrongEquivalence(Curve f)
    {
        // This test verifies that 
        // \lpiD{ [0, T[ }{f_t} = \lpi{f}_t, \lpiD{ [T, +\infty[ }{f_p} = \lpi{f}_p .
        // In other words, computing the \lpiD of the transient and periodic parts is the same as computing the \lpi of the whole
        // and then take its transient and periodic parts.

        // An exception is given by T = 0, f(0) > 0:
        // in this case, the function does not have a transient, but \lpi{f}_t is the zero segment [0, f(0)[,
        // which will also appear as the transient part of \lpi{f_p}

        // Otherwise, the theorem can be verified only if T is delayed so that there is no left-discontinuity at T with 
        // a constant segment that starts in the transient.
        // If not, the \lpiD of the periodic part has a transient part itself, which alters the result.

        output.WriteLine(f.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsRightContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var lastTransientSegment =  f.PseudoPeriodStart == 0 ? null : f.GetSegmentBefore(f.PseudoPeriodStart); 
        var firstPeriodSegment = f.GetSegmentAfter(f.PseudoPeriodStart);
        var secondPeriodSegment = f.GetSegmentAfter(firstPeriodSegment.EndTime);
        var T_f_prime = 
            // we need to delay T_f to the right point if the transient exists or ends with a constant, and the curve is continuous at its end 
            lastTransientSegment != null && lastTransientSegment.IsConstant && f.IsContinuousAt(f.PseudoPeriodStart) ?
                // if the transient ends with a segment but the period does not, it suffices to skip the first period segment
                // the same happens if the first period segment is constant but there is a jump between it and the next one
            !firstPeriodSegment.IsConstant || !f.IsContinuousAt(firstPeriodSegment.EndTime) ?
                firstPeriodSegment.EndTime :
                // otherwise, we need to skip two
                secondPeriodSegment.EndTime
            // otherwise, we can use the current period start
            : f.PseudoPeriodStart;
        var f_T_f_prime = f.ValueAt(T_f_prime);

        // The lpi decomposition needs to use [0, T_f] for the transient since, being the function right-continuous,
        // it may have a jump a the left of T_f.
        // In that case, using [0, T_f[ means that neither f_t nor f_p include said jump,
        // and so neither \lpiD{f_t} nor \lpiD{f_p} will have the constant segment representing it.
        var (f_t, f_p) = f.Decompose(T_f_prime, leftIncludesEndPoint: true, minDecomposition: false);
        var f_t_lpi = f_t?.LowerPseudoInverseOverInterval(0, T_f_prime, isEndIncluded: true);
        var f_p_lpi = f_p.LowerPseudoInverseOverInterval(T_f_prime);

        var lpi = f.LowerPseudoInverse();
        var (lpi_t, lpi_p) = lpi.Decompose(f_T_f_prime, minDecomposition: true);

        var valueAtZero = f.ValueAt(0);
        if (f_t_lpi == null || lpi_t == null)
        {
            if (T_f == 0 && valueAtZero > 0)
            {
                Assert.Null(f_t_lpi);
                Assert.NotNull(lpi_t);
                Assert.True(Curve.Equivalent(
                    Curve.Minimum(Curve.PlusInfinite(), Sequence.Zero(0, valueAtZero)), 
                    lpi_t!)
                );
                Assert.Equal(valueAtZero, lpi_p.FirstFiniteTime);
            }
            else
            {
                Assert.True(f_t_lpi == lpi_t);
            }
        }
        else
            // The cut is done since if f_t ends with a constant segment, f_t_lpi will have extra information, namely f_t_lpi(f_T_f)
            Assert.True(Sequence.Equivalent(
                f_t_lpi.Cut(0, f_T_f_prime),
                lpi_t.Cut(0, f_T_f_prime))
            );

        if (T_f == 0 && valueAtZero > 0)
        {
            Assert.True(Curve.EquivalentAfter(f_p_lpi, lpi_p, valueAtZero));
            Assert.True(Sequence.Equivalent(
                Sequence.Zero(0, valueAtZero), 
                f_p_lpi.Cut(0, valueAtZero))
            );
            Assert.True(Sequence.Equivalent(
                Sequence.PlusInfinite(0, valueAtZero), 
                lpi_p.Cut(0, valueAtZero))
            );
        }
        else
            Assert.True(Curve.Equivalent(f_p_lpi, lpi_p));
    }

    [Theory]
    [MemberData(nameof(LpiDecompositionEquivalenceTestCases))]
    public void LpiImprovedPeriodStart(Curve f)
    {
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsRightContinuous);

        f = f.Optimize();
        var tf = f.PseudoPeriodStart;
        var df = f.PseudoPeriodLength;
        var ftf = f.ValueAt(tf);
        var ftfdf = f.ValueAt(tf + df);

        var lpi = f.LowerPseudoInverse();
        var tstar_1 = lpi.ValueAt(ftf);
        var tstar_2 = lpi.ValueAt(ftfdf);

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var lpi = {lpi.ToCodeString()};");

        if (tstar_2 == tstar_1 + df)
        {
            output.WriteLine("Lemma conditions verified.");
            Assert.True(lpi.PseudoPeriodStart <= ftf);
        }
        else
        {
            output.WriteLine("Lemma conditions not verified.");
            Assert.True(lpi.PseudoPeriodStart <= ftfdf);
            Assert.True(ftf <= lpi.PseudoPeriodStart);
        }
    }

    public static IEnumerable<object[]> LpiOfUpiAlwaysImprovedPeriodStartTestCases()
    {
        var testcases = ContinuousExamples.Concat(LeftContinuousExamples);

        foreach (var c in testcases)
            yield return new object[] { c };
    }

    [Theory]
    [MemberData(nameof(LpiOfUpiAlwaysImprovedPeriodStartTestCases))]
    public void LpiOfUpiAlwaysImprovedPeriodStart(Curve f)
    {
        Assert.True(f.IsNonDecreasing);
        Assert.True(f.IsLeftContinuous);

        f = f.Optimize();
        var tf = f.PseudoPeriodStart;
        var ftf = f.ValueAt(tf);
        var tfdf = tf + f.PseudoPeriodLength;
        var ftfdf = f.ValueAt(tfdf);

        var upi = f.UpperPseudoInverse();
        Assert.True(upi.IsRightContinuous);

        var t_upi = upi.PseudoPeriodStart;
        var d_upi = upi.PseudoPeriodLength;
        var upi_t = upi.ValueAt(t_upi);
        var upi_td = upi.ValueAt(t_upi + d_upi);

        var lpi_upi = upi.LowerPseudoInverse();
        var tstar_1 = lpi_upi.ValueAt(upi_t);
        var tstar_2 = lpi_upi.ValueAt(upi_td);

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var upi = {upi.ToCodeString()};");
        output.WriteLine($"var lpi_upi = {lpi_upi.ToCodeString()};");

        Assert.Equal(ftf, tstar_1);
        Assert.Equal(ftfdf, tstar_2);
        Assert.Equal(lpi_upi.PseudoPeriodStart, tf);
    }
}