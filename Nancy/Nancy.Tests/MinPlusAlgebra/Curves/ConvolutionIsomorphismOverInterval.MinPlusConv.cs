using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public partial class ConvolutionIsomorphismOverInterval
{
    public static IEnumerable<(Curve a, Curve b)> MinPlusConvolutionIsomorphismPairs()
    {
        var cc_pairs = ContinuousExamples.SelectMany(
            a => ContinuousExamples.Select(b => (a, b))
        );
        var lc_pairs = ContinuousExamples.SelectMany(
            a => LeftContinuousExamples.Select(b => (a, b))
        );
        var ll_pairs = LeftContinuousExamples.SelectMany(
            a => LeftContinuousExamples.Select(b => (a, b))
        );
        var pairs = cc_pairs
            .Concat(lc_pairs)
            .Concat(ll_pairs);

        // var sb = new StringBuilder();
        // foreach (var pair in pairs)
        // {
        //     sb.AppendLine($"({pair.a.ToCodeString()}, {pair.b.ToCodeString()}),");
        // }
        // var str = sb.ToString();

        return pairs;
    }

    public static IEnumerable<object[]> MinPlusConvolutionIsomorphismPairsTestCases()
    {
        foreach (var (a, b) in MinPlusConvolutionIsomorphismPairs())
        {
            yield return new object[] { a, b };
        }
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairsTestCases))]
    public void MinPlusPeriodConvolutionEquivalence_UpiOnly(Curve f, Curve g)
    {
        // This test verifies that 
        // \upiD{ f_p \otimes g_p }{ [T_f + T_g, +\infty[ } = \upiD{ [T_f, +\infty[ }{f_p} \overtimes \upiD{ [T_g, +\infty[ }{g_p} .
        // In other words, by verifying the above we get that the isomorphism applies not just on the whole (min,+) convolution,
        // but also on its period-by-period component.

        // This does not require any extra hypothesis since the information loss (in case of constant segments at the start)
        // is the same for both left- and right-hand side.

        // todo: add reference to the formal theorem

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous && g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var f_rightCont_at_T_f = f.IsRightContinuousAt(T_f);
        var f_firstPeriodSegment = f.GetSegmentAfter(T_f);
        var T_f_prime = f_rightCont_at_T_f && f_firstPeriodSegment.IsConstant ? 
            f_firstPeriodSegment.EndTime : T_f;
        var (_, f_p) =  f.Decompose(T_f);

        var T_g = g.PseudoPeriodStart;
        var g_rightCont_at_T_g = g.IsRightContinuousAt(T_g);
        var g_firstPeriodSegment = g.GetSegmentAfter(T_g);
        var T_g_prime = g_rightCont_at_T_g && g_firstPeriodSegment.IsConstant ?
            g_firstPeriodSegment.EndTime : T_g;
        var (_, g_p) = g.Decompose(T_g);

        var conv_minP = Curve.Convolution(f_p, g_p, noIsomorphismSettings);
        var conv_minP_upi = conv_minP.UpperPseudoInverseOverInterval(T_f + T_g);

        var f_p_upi = f_p.UpperPseudoInverseOverInterval(T_f);
        var g_p_upi = g_p.UpperPseudoInverseOverInterval(T_g);
        var conv_maxP_upi = Curve.MaxPlusConvolution(f_p_upi, g_p_upi, noIsomorphismSettings);

        Assert.True(Curve.Equivalent(conv_minP_upi, conv_maxP_upi));
        Assert.Equal(T_f_prime + T_g_prime, conv_minP_upi.ValueAt(f.ValueAt(T_f) + g.ValueAt(T_g)));
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairsTestCases))]
    public void MinPlusPeriodConvolutionEquivalence_InfoLoss(Curve f, Curve g)
    {
        // This test verifies that 
        // f_p \otimes g_p = \lpiD { [f(T_f) + g(T_g), +\infty[ }{ \upiD{ [T_f, +\infty[ }{f_p} \overtimes \upiD{ [T_g, +\infty[ }{g_p} } .
        // In other words, by verifying the above we get that the isomorphism applies not just on the whole (min,+) convolution,
        // but also on its period-by-period component.

        // The theorem can be verified only if the periods do not start with a point and a constant segment without a jump in-between.
        // Otherwise, the right-hand side has an information loss which alters the result.

        // In this test, we do not alter anything and verify the information loss.

        // todo: add reference to the formal theorem
        // With reference to Theorem XXX, this test verifies that the reconstruction operator is needed,
        // and the missing pieces are indeed those that would be addressed by said operator.

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous && g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var f_rightCont_at_T_f = f.IsRightContinuousAt(T_f);
        var f_firstPeriodSegment = f.GetSegmentAfter(T_f);
        var T_f_prime = f_rightCont_at_T_f && f_firstPeriodSegment.IsConstant ? 
            f_firstPeriodSegment.EndTime : T_f;
        var (_, f_p) =  f.Decompose(T_f);

        var T_g = g.PseudoPeriodStart;
        var g_rightCont_at_T_g = g.IsRightContinuousAt(T_g);
        var g_firstPeriodSegment = g.GetSegmentAfter(T_g);
        var T_g_prime = g_rightCont_at_T_g && g_firstPeriodSegment.IsConstant ?
            g_firstPeriodSegment.EndTime : T_g;
        // var T_g_prime = T_g; will not work
        var (_, g_p) = g.Decompose(T_g);

        var conv_minP = Curve.Convolution(f_p, g_p, noIsomorphismSettings);

        var f_p_upi = f_p.UpperPseudoInverseOverInterval(T_f);
        var g_p_upi = g_p.UpperPseudoInverseOverInterval(T_g);
        var upi_conv = Curve.MaxPlusConvolution(f_p_upi, g_p_upi, noIsomorphismSettings);
        var conv_maxP = upi_conv.LowerPseudoInverseOverInterval(f.ValueAt(T_f_prime) + g.ValueAt(T_g_prime));

        if (T_f == T_f_prime && T_g == T_g_prime)
        {
            // no information loss expected, so the results are the same
            Assert.True(Curve.Equivalent(conv_minP, conv_maxP));    
        }
        else
        {
            // the constant segments cause an information loss.
            Assert.False(Curve.Equivalent(conv_minP, conv_maxP));
            Assert.True(Curve.EquivalentAfter(conv_minP, conv_maxP, T_f_prime + T_g_prime));
            // the part the we lose is a predictable constant segment
            Assert.True(Sequence.Equivalent(
                Sequence.Constant(f.ValueAt(T_f) + g.ValueAt(T_g), T_f + T_g, T_f_prime + T_g_prime),
                conv_minP.Cut(T_f + T_g, T_f_prime + T_g_prime)
            ));
            Assert.True(Sequence.Equivalent(
                Sequence.PlusInfinite(T_f + T_g, T_f_prime + T_g_prime),
                conv_maxP.Cut(T_f + T_g, T_f_prime + T_g_prime)
            ));
        }
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairsTestCases))]
    public void MinPlusPeriodConvolutionEquivalence_FixUsingDelay(Curve f, Curve g)
    {
        // This test verifies that 
        // f_p \otimes g_p = \lpiD { [f(T_f) + g(T_g), +\infty[ }{ \upiD{ [T_f, +\infty[ }{f_p} \overtimes \upiD{ [T_g, +\infty[ }{g_p} } .
        // In other words, by verifying the above we get that the isomorphism applies not just on the whole (min,+) convolution,
        // but also on its period-by-period component.

        // The theorem can be verified only if the periods do not start with a point and a constant segment without a jump in-between.
        // Otherwise, the right-hand side has an information loss which alters the result.

        // The above is avoided by delaying T_f and T_g into T_f' and T_g'.

        // todo: add reference to the formal theorem
        // With reference to Theorem XXX, this test is avoiding the use of the reconstruction operator,
        // using instead the delaying of pseudo-period starts so that a = a'

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous && g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var f_rightCont_at_T_f = f.IsRightContinuousAt(T_f);
        var f_firstPeriodSegment = f.GetSegmentAfter(T_f);
        var T_f_prime = f_rightCont_at_T_f && f_firstPeriodSegment.IsConstant ? 
            f_firstPeriodSegment.EndTime : T_f;
        // var T_f_prime = T_f; will not work
        var (_, f_p) =  f.Decompose(T_f_prime);

        var T_g = g.PseudoPeriodStart;
        var g_rightCont_at_T_g = g.IsRightContinuousAt(T_g);
        var g_firstPeriodSegment = g.GetSegmentAfter(T_g);
        var T_g_prime = g_rightCont_at_T_g && g_firstPeriodSegment.IsConstant ?
            g_firstPeriodSegment.EndTime : T_g;
        // var T_g_prime = T_g; will not work
        var (_, g_p) = g.Decompose(T_g_prime);

        var conv_minP = Curve.Convolution(f_p, g_p, noIsomorphismSettings);
        var f_p_upi = f_p.UpperPseudoInverseOverInterval(T_f_prime);
        var g_p_upi = g_p.UpperPseudoInverseOverInterval(T_g_prime);
        var upi_conv = Curve.MaxPlusConvolution(f_p_upi, g_p_upi, noIsomorphismSettings);
        var conv_maxP = upi_conv.LowerPseudoInverseOverInterval(f.ValueAt(T_f_prime) + g.ValueAt(T_g_prime));
        Assert.True(Curve.Equivalent(conv_minP, conv_maxP));
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairsTestCases))]
    public void MinPlusPeriodConvolutionStrongEquivalence(Curve f, Curve g)
    {
        // This test verifies that the delay of T_f and T_g, as stated in MinPlusPeriodConvolutionEquivalence,
        // does not affect the result being computed, as only a predictable constant segment is being left out.

        // Given a decomposition of f, f_p, computed using T_f, consider f_p' computed using T_f', which is the least T such that
        // f_p' does not start with a point and a constant segment without a jump in-between.
        // Similarly for g: g_p and T_g, g_p' T_g' . Then, 
        // f_p \otimes g_p = \begin{cases}
        // \lpiD { [f(T_f') + g(T_g'), +\infty[ }{ \upiD{ [T_f', +\infty[ }{f_p'} \overtimes \upiD{ [T_g', +\infty[ }{g_p'} } , if t >= T_f' + T_g'
        // f(T_f) + g(T_g) , if t \in [ T_f + T_g, T_f' + T_g' [
        // \end{cases}.

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous && g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var T_g = g.PseudoPeriodStart;
        var (_, f_p) =  f.Decompose(T_f);
        var (_, g_p) = g.Decompose(T_g);

        var f_rightCont_at_T_f = f.IsRightContinuousAt(T_f);
        var f_firstPeriodSegment = f.GetSegmentAfter(T_f);
        var T_f_prime = f_rightCont_at_T_f && f_firstPeriodSegment.IsConstant ? 
            f_firstPeriodSegment.EndTime : T_f;
        var (_, f_p_prime) =  f.Decompose(T_f_prime);

        var g_rightCont_at_T_g = g.IsRightContinuousAt(T_g);
        var g_firstPeriodSegment = g.GetSegmentAfter(T_g);
        var T_g_prime = g_rightCont_at_T_g && g_firstPeriodSegment.IsConstant ?
            g_firstPeriodSegment.EndTime : T_g;
        var (_, g_p_prime) = g.Decompose(T_g_prime);

        var conv_minP = Curve.Convolution(f_p, g_p, noIsomorphismSettings);
        var f_p_upi = f_p_prime.UpperPseudoInverseOverInterval(T_f_prime);
        var g_p_upi = g_p_prime.UpperPseudoInverseOverInterval(T_g_prime);
        var upi_conv = Curve.MaxPlusConvolution(f_p_upi, g_p_upi, noIsomorphismSettings);
        var conv_maxP = upi_conv.LowerPseudoInverseOverInterval(f.ValueAt(T_f_prime) + g.ValueAt(T_g_prime));

        if (T_f_prime + T_g_prime > T_f + T_g)
        {
            var where = Curve.FindFirstInequivalenceAfter(conv_minP, conv_maxP, T_f_prime + T_g_prime);
            Assert.True(Curve.EquivalentAfter(conv_minP, conv_maxP, T_f_prime + T_g_prime));
            var f_T_f = f.ValueAt(T_f);
            var g_T_g = g.ValueAt(T_g);
            Assert.Equal(f_T_f + g_T_g, conv_minP.ValueAt(T_f + T_g));
            var gapCut = conv_minP.Cut(T_f + T_g, T_f_prime + T_g_prime, false, false).Optimize();
            Assert.Equal(1, gapCut.Count);
            var gapSegment = (Segment) gapCut.Elements.Single();
            Assert.True(gapSegment.IsConstant);
            Assert.Equal(f_T_f + g_T_g, gapSegment.RightLimitAtStartTime);
        }
        else
        {
            var where = Curve.FindFirstInequivalence(conv_minP, conv_maxP);
            Assert.True(Curve.Equivalent(conv_minP, conv_maxP));
        }
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairsTestCases))]
    public void MinPlusConvolutionWithIsomorphismOverInterval(Curve f, Curve g)
    {
        // This test collects all the other results to show a decomposition approach for the min-plus convolution
        // which then uses the isomorphism for the periodic-periodic convolution

        output.WriteLine(f.ToCodeString());
        output.WriteLine(g.ToCodeString());

        // necessary hypotheses
        Assert.True(f.IsLeftContinuous && g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing && g.IsNonDecreasing);
        Assert.True(f.IsNonNegative && g.IsNonNegative);

        var T_f = f.PseudoPeriodStart;
        var T_g = g.PseudoPeriodStart;

        var (f_t, f_p) =  f.Decompose(T_f);
        var (g_t, g_p) = g.Decompose(T_g);

        // In case of no-transient, we replace null with the min-plus null element, i.e. +inf curves
        f_t ??= Curve.PlusInfinite();
        g_t ??= Curve.PlusInfinite();

        // Usual method, as in [BT07]
        var conv_tt = Curve.Convolution(f_t, g_t, noIsomorphismSettings);
        var conv_tp = Curve.Convolution(f_t, g_p, noIsomorphismSettings);
        var conv_pt = Curve.Convolution(f_p, g_t, noIsomorphismSettings);
        var conv_pp_minP = Curve.Convolution(f_p, g_p, noIsomorphismSettings);
        var conv_minP = Curve.Minimum(new[] {conv_tt, conv_tp, conv_pt, conv_pp_minP}, noIsomorphismSettings);

        Assert.Equal(f.ValueAt(0) + g.ValueAt(0), conv_minP.ValueAt(0));

        // New method, using the isomorphism only over the pp convolution
        var f_rightCont_at_T_f = f.IsRightContinuousAt(T_f);
        var f_firstPeriodSegment = f.GetSegmentAfter(T_f);
        var T_f_prime = f_rightCont_at_T_f && f_firstPeriodSegment.IsConstant ? 
            f_firstPeriodSegment.EndTime : T_f;
        var (_, f_p_prime) =  f.Decompose(T_f_prime);

        var g_rightCont_at_T_g = g.IsRightContinuousAt(T_g);
        var g_firstPeriodSegment = g.GetSegmentAfter(T_g);
        var T_g_prime = g_rightCont_at_T_g && g_firstPeriodSegment.IsConstant ?
            g_firstPeriodSegment.EndTime : T_g;
        var (_, g_p_prime) = g.Decompose(T_g_prime);

        var f_p_upi = f_p_prime.UpperPseudoInverseOverInterval(T_f_prime);
        var g_p_upi = g_p_prime.UpperPseudoInverseOverInterval(T_g_prime);
        var upi_conv = Curve.MaxPlusConvolution(f_p_upi, g_p_upi, noIsomorphismSettings);
        var conv_pp_maxP_raw = upi_conv.LowerPseudoInverseOverInterval(f.ValueAt(T_f_prime) + g.ValueAt(T_g_prime));

        // if T_f_prime + T_g_prime > T_f + T_g, prepend with the predictable constant segment
        var conv_pp_maxP = T_f_prime + T_g_prime > T_f + T_g 
            ? Curve.Minimum(
                conv_pp_maxP_raw, 
                Sequence.Constant(f.ValueAt(T_f) + g.ValueAt(T_g), T_f + T_g, T_f_prime + T_g_prime, true, false)
            ) 
            : conv_pp_maxP_raw;

        var conv_maxP = Curve.Minimum(new[] {conv_tt, conv_tp, conv_pt, conv_pp_maxP}, noIsomorphismSettings);

        var where = Curve.FindFirstInequivalence(conv_minP, conv_maxP);
        Assert.True(Curve.Equivalent(conv_minP, conv_maxP));
    }

}