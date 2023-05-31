using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public partial class ConvolutionIsomorphismOverInterval
{
    public static IEnumerable<object[]> MaxPlusIsomorphismTestCases()
    {
        var cc_testcases = ContinuousExamples.SelectMany(
            a => ContinuousExamples.Select(b => (a, b))
        );
        var rc_testcases = ContinuousExamples.SelectMany(
            a => RightContinuousExamples.Select(b => (a, b))
        );
        var rr_testcases = RightContinuousExamples.SelectMany(
            a => RightContinuousExamples.Select(b => (a, b))
        );
        var testcases = cc_testcases
            .Concat(rc_testcases)
            .Concat(rr_testcases);

        foreach (var (a, b) in testcases)
        {
            yield return new object[] { a, b };
        }
    }

    [Theory]
    [MemberData(nameof(MaxPlusIsomorphismTestCases))]
    public void MaxPlusPeriodConvolutionEquivalence(Curve f, Curve g)
    {
        // This test verifies that 
        // f_p \overtimes g_p = \upiD { [f(T_f) + g(T_g), +\infty[ }{ \lpiD{ [T_f, +\infty[ }{f_p} \otimes \lpiD{ [T_g, +\infty[ }{g_p} } .
        // In other words, by verifying the above we get that the isomorphism applies not just on the whole (max,+) convolution,
        // but also on its period-by-period component.

        // Interestingly, this theorem does not require any change of T_f, T_g at all.
        // This is because we are not interested in the result of LpiDecompositionStrongEquivalence, as we are limiting the LPI to f_p.
        // Even in the presence of a constant segment, its earliest time will assumed to be T_f.

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsRightContinuous && g.IsRightContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        // var f_firstPeriodSegment = f.GetSegmentAfter(f.PseudoPeriodStart);
        // var T_f = f_firstPeriodSegment.IsConstant ? f_firstPeriodSegment.EndTime : f.PseudoPeriodStart;
        var T_f = f.PseudoPeriodStart;
        var (_, f_p) =  f.Decompose(T_f, minDecomposition: false);

        // var g_firstPeriodSegment = g.GetSegmentAfter(g.PseudoPeriodStart);
        // var T_g = g_firstPeriodSegment.IsConstant ? g_firstPeriodSegment.EndTime : g.PseudoPeriodStart;
        var T_g = g.PseudoPeriodStart;
        var (_, g_p) = g.Decompose(T_g, minDecomposition: false);

        var conv_maxP = Curve.MaxPlusConvolution(f_p, g_p, noIsomorphismSettings);
        var f_p_lpi = f_p.LowerPseudoInverseOverInterval(T_f);
        var g_p_lpi = g_p.LowerPseudoInverseOverInterval(T_g);
        var lpi_conv = Curve.Convolution(f_p_lpi, g_p_lpi, noIsomorphismSettings);
        var conv_minP = lpi_conv.UpperPseudoInverseOverInterval(f.ValueAt(T_f) + g.ValueAt(T_g));
        Assert.True(Curve.Equivalent(conv_maxP, conv_minP));
    }

    // There is no need for a MaxPlusPeriodConvolutionStrongEquivalence property, since the one we have is already "strong":
    // we do not need any delay at all, the isomorphism is always valid.

    [Theory]
    [MemberData(nameof(MaxPlusIsomorphismTestCases))]
    public void MaxPlusConvolutionWithIsomorphismOverInterval(Curve f, Curve g)
    {
        // This test collects all the other results to show a decomposition approach for the max-plus convolution
        // which then uses the isomorphism for the periodic-periodic convolution

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsRightContinuous && g.IsRightContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var T_g = g.PseudoPeriodStart;

        var (f_t, f_p) =  f.Decompose(T_f, minDecomposition: false);
        var (g_t, g_p) = g.Decompose(T_g, minDecomposition: false);

        // In case of no-transient, we replace null with the max-plus null element, i.e. -inf curves
        f_t ??= Curve.MinusInfinite();
        g_t ??= Curve.MinusInfinite();

        // Usual method, adapted from [BT07] for max-plus convolution
        var conv_tt = Curve.MaxPlusConvolution(f_t, g_t, noIsomorphismSettings);
        var conv_tp = Curve.MaxPlusConvolution(f_t, g_p, noIsomorphismSettings);
        var conv_pt = Curve.MaxPlusConvolution(f_p, g_t, noIsomorphismSettings);
        var conv_pp_maxP = Curve.MaxPlusConvolution(f_p, g_p, noIsomorphismSettings);
        var conv_maxP = Curve.Maximum(new[] {conv_tt, conv_tp, conv_pt, conv_pp_maxP}, noIsomorphismSettings);

        // New method, using the isomorphism only over the pp convolution
        var f_p_lpi = f_p.LowerPseudoInverseOverInterval(T_f);
        var g_p_lpi = g_p.LowerPseudoInverseOverInterval(T_g);
        var lpi_conv = Curve.Convolution(f_p_lpi, g_p_lpi, noIsomorphismSettings);
        var conv_pp_minP = lpi_conv.UpperPseudoInverseOverInterval(f.ValueAt(T_f) + g.ValueAt(T_g));
        var conv_minP = Curve.Maximum(new[] {conv_tt, conv_tp, conv_pt, conv_pp_minP}, noIsomorphismSettings);

        var where = Curve.FindFirstInequivalence(conv_maxP, conv_minP);
        Assert.True(Curve.Equivalent(conv_maxP, conv_minP));
    }

}