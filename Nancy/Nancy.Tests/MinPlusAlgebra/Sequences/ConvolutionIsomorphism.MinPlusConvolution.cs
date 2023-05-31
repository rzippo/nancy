using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public partial class ConvolutionIsomorphism
{
    public static IEnumerable<(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)> IsospeedMinPlusConvolutions()
    {
        foreach (var (f, g) in Curves.ConvolutionIsomorphism.MinPlusConvolutionPairs())
        {
            var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
            var k_c_f = lcm_c / f.PseudoPeriodHeight;
            var k_c_g = lcm_c / g.PseudoPeriodHeight;
            var lcm_d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var d = Rational.Min(lcm_d, Rational.Max(k_c_f * f.PseudoPeriodLength, k_c_g * g.PseudoPeriodLength));
            var c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

            var tf = f.PseudoPeriodStart;
            var tg = g.PseudoPeriodStart;
            var T = tf + tg + lcm_d;
   
            var fSegmentAfterTf = f.GetSegmentAfter(tf);
            var tf_prime = (f.IsRightContinuousAt(tf) && fSegmentAfterTf.IsConstant) ? fSegmentAfterTf.EndTime : tf;
            var fCutEnd_minp = tf + 2 * lcm_d;
            var fCutEnd_iso = tf_prime + 2 * k_c_f * f.PseudoPeriodLength;
            var fCut = fCutEnd_minp <= fCutEnd_iso 
                ? f.Cut(tf, fCutEnd_minp, isEndIncluded: false)
                : f.Cut(tf, fCutEnd_iso, isEndIncluded: true);

            var gSegmentAfterTg = g.GetSegmentAfter(tg);
            var tg_prime = (g.IsRightContinuousAt(tg) && gSegmentAfterTg.IsConstant) ? gSegmentAfterTg.EndTime : tg;
            var gCutEnd_minp = tg + 2 * lcm_d;
            var gCutEnd_iso = tg_prime + 2 * k_c_g * g.PseudoPeriodLength;
            var gCut = gCutEnd_minp <= gCutEnd_iso 
                ? g.Cut(tg, gCutEnd_minp, isEndIncluded: false)
                : g.Cut(tg, gCutEnd_iso, isEndIncluded: true);

            var cutEnd = tf + tg + lcm_d + d;
            var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;

            yield return (fCut, gCut, cutEnd, cutCeiling, T, d); 
        }
    }

    public static IEnumerable<object[]> IsospeedMinPlusConvolutionsTestCases()
    {
        foreach(var (f, g, cutEnd, cutCeiling, T, d) in IsospeedMinPlusConvolutions())
        {
            yield return new object[] { f, g, cutEnd, cutCeiling, T, d };
        }
    }

    [Theory]
    [MemberData(nameof(IsospeedMinPlusConvolutionsTestCases))]
    public void MinPlusConvolution_Isospeed_Equivalence(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)
    {   
        // this test verifies that, for the cuts given by isospeed, we can compute the by-sequence convolution also via isomorphism

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        var settings = ComputationSettings.Default() with {
            UseBySequenceConvolutionIsomorphismOptimization = false
        };

        // direct algorithm, using min-plus convolution
        var direct_result = Sequence.Convolution(f, g, settings, cutEnd, cutCeiling, true, false, false);

        // inverse algorithm, using max-plus convolution
        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);
        var maxp = Sequence.MaxPlusConvolution(
            f_upi, g_upi,
            cutEnd: cutCeiling, cutCeiling: cutEnd,
            isEndIncluded: true, isCeilingIncluded: true,
            settings: settings);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        // todo: rename in the actual alg as well
        Sequence inverse;
        if (ta_f_prime == f.DefinedFrom && ta_g_prime == g.DefinedFrom)
        {
            inverse = inverse_raw;
        }
        else
        {
            // note: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(f.DefinedFrom) + g.ValueAt(g.DefinedFrom),
                f.DefinedFrom + g.DefinedFrom,
                ta_f_prime + ta_g_prime
            );
            inverse = Sequence.Minimum(ext, inverse_raw, false);
        }

        var inverse_result = inverse.Elements
            .CutWithCeiling(cutCeiling, false)
            .ToSequence();

        // results of the two methods, to be cut
        output.WriteLine($"var direct_result = {direct_result.ToCodeString()};");
        output.WriteLine($"var inverse_result = {inverse_result.ToCodeString()};");
        
        // cut of the two methods
        var direct_result_end = Rational.Min(
            direct_result.Elements.Last(e => e.IsFinite).EndTime,
            T + d
        );
        var direct_result_cut = direct_result.Cut(f.DefinedFrom + g.DefinedFrom, direct_result_end);
        var inverse_result_end = Rational.Min(
            inverse_result.Elements.Last(e => e.IsFinite).EndTime,
            T + d
        );
        var inverse_result_cut = inverse_result.Cut(f.DefinedFrom + g.DefinedFrom, inverse_result_end);
        output.WriteLine($"var direct_result_cut = {direct_result_cut.ToCodeString()};");
        output.WriteLine($"var inverse_result_cut = {inverse_result_cut.ToCodeString()};");
        
        Assert.True(Sequence.Equivalent(direct_result_cut, inverse_result_cut));
    }

    public static IEnumerable<(Sequence f, Sequence g)> LeftContinuousConvolutions()
    {
        var sequences = LeftContinuousExamples().Concat(ContinuousExamples());

        var pairs = sequences.SelectMany(
            f => sequences.Select(
                g => (f, g)
            )
        );
        return pairs;
    }

    public static IEnumerable<object[]> LeftContinuousConvolutionTestcases()
    {
        foreach (var (f, g) in LeftContinuousConvolutions())
        {
            yield return new object[] {f, g};
        }
    }

    [Theory]
    [MemberData(nameof(LeftContinuousConvolutionTestcases))]
    public void MinPlusConvolution_Generalization_Equivalence(Sequence f, Sequence g)
    {
        // this test verifies a conjecture that generalizes the isomorphism for by-sequence convolution
        // it states that the result of by-sequence convolution is 'valid', in the general case, only for the smaller of the lengths,
        // and within that the isomorphism can be applied
        
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        // for simplicity, we only support this case for now
        Assert.True(f.IsLeftClosed);
        Assert.True(g.IsLeftClosed);
        Assert.True(f.IsRightOpen);
        Assert.True(g.IsRightOpen);

        var ta_f = f.DefinedFrom;
        var ta_g = g.DefinedFrom;
        var tb_f = f.DefinedUntil;
        var tb_g = g.DefinedUntil;

        var lf = tb_f - ta_f;
        var lg = tb_g - ta_g;
        var length = Rational.Min(lf, lg);
        var cutStart = ta_f + ta_g;
        var cutEnd = ta_f + ta_g + length;

        var direct = Sequence.Convolution(f, g).Cut(cutStart, cutEnd);

        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);

        var maxp = Sequence.MaxPlusConvolution(f_upi, g_upi); //.Cut(vcutStart, vcutEnd, isEndIncluded: true);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        output.WriteLine($"var direct = {direct.ToCodeString()};");
        output.WriteLine($"var inverse_raw = {inverse_raw.ToCodeString()};");

        if (ta_f_prime == ta_f && ta_g_prime == ta_g)
        {
            var inverse = inverse_raw.Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, inverse));
        }
        else
        {
            // todo: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(ta_f) + g.ValueAt(ta_g), 
                ta_f + ta_g,
                ta_f_prime + ta_g_prime
            );
            var reconstructed = Sequence.Minimum(ext, inverse_raw, false)
                .Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, reconstructed));
        }
    }

    public static List<(Sequence a, Sequence b, Rational cutEnd)> SingleCutConvolutions()
    {
        var testcases = new List<(Sequence a, Sequence b, Rational cutEnd)>()
        {
            (
                new Sequence(new List<Element>{ new Point(2,1), new Segment(2,3,1,0), new Point(3,1), new Segment(3,4,1,1), new Point(4,2), new Segment(4,5,2,0), new Point(5,2), new Segment(5,6,2,1), new Point(6,3), new Segment(6,7,3,0), new Point(7,3), new Segment(7,8,3,1), new Point(8,4), new Segment(8,9,4,0), new Point(9,4), new Segment(9,10,4,1), new Point(10,5), new Segment(10,11,5,0), new Point(11,5), new Segment(11,12,5,1), new Point(12,6), new Segment(12,13,6,0), new Point(13,6), new Segment(13,14,6,1) }),
                new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,2,1), new Point(1,3), new Segment(1,3,3,0), new Point(3,3), new Segment(3,4,3,1), new Point(4,4), new Segment(4,6,4,0), new Point(6,4), new Segment(6,7,4,1), new Point(7,5), new Segment(7,9,5,0), new Point(9,5), new Segment(9,10,5,1), new Point(10,6), new Segment(10,12,6,0) }),
                14
            )
        };
        return testcases;
    }

    public static IEnumerable<object[]> SingleCutConvolutionTestcases()
    {
        foreach (var (a, b, cutEnd) in SingleCutConvolutions())
        {
            yield return new object[] {a, b, cutEnd};
        }
    }

    [Theory]
    [MemberData(nameof(SingleCutConvolutionTestcases))]
    public void MinPlusConvolution_SingleCut_Equivalence(Sequence f, Sequence g, Rational cutEnd)
    {
        // this test verifies a conjecture that generalizes the isomorphism for by-sequence convolution
        // it states that the result of by-sequence convolution is 'valid, in the general case, only for the smaller of the lengths,
        // and within that the isomorphism can be applied

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        var settings = ComputationSettings.Default() with {UseBySequenceConvolutionIsomorphismOptimization = false};

        // for simplicity, we only support this case for now
        Assert.True(f.IsLeftClosed);
        Assert.True(g.IsLeftClosed);
        Assert.True(f.IsRightOpen);
        Assert.True(g.IsRightOpen);

        var ta_f = f.DefinedFrom;
        var ta_g = g.DefinedFrom;
        var tb_f = f.DefinedUntil;
        var tb_g = g.DefinedUntil;

        var lf = tb_f - ta_f;
        var lg = tb_g - ta_g;
        var length = Rational.Min(lf, lg);
        var cutStart = ta_f + ta_g;
        var equivCutEnd = ta_f + ta_g + length;
        var cutCeiling = Rational.PlusInfinity;

        if (cutEnd > equivCutEnd)
            throw new InvalidOperationException();

        var direct = Sequence.Convolution(f, g, settings).Cut(cutStart, cutEnd);

        var ta_f_prime = f.FirstPlateauEnd;
        var ta_g_prime = g.FirstPlateauEnd;

        var f_upi = f.UpperPseudoInverse(false);
        var g_upi = g.UpperPseudoInverse(false);

        var maxp = Sequence.MaxPlusConvolution(
            f_upi, g_upi,
            cutEnd: cutCeiling, cutCeiling: cutEnd,
            isEndIncluded: true, isCeilingIncluded: true,
            settings: settings);
        var inverse_raw = maxp.LowerPseudoInverse(false);

        output.WriteLine($"var direct = {direct.ToCodeString()};");
        output.WriteLine($"var inverse_raw = {inverse_raw.ToCodeString()};");

        if (ta_f_prime == ta_f && ta_g_prime == ta_g)
        {
            var inverse = inverse_raw.Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, inverse));
        }
        else
        {
            // todo: does not handle left-open sequences
            var ext = Sequence.Constant(
                f.ValueAt(ta_f) + g.ValueAt(ta_g), 
                ta_f + ta_g,
                ta_f_prime + ta_g_prime
            );
            var reconstructed = Sequence.Minimum(ext, inverse_raw, false)
                .Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, reconstructed));
        }
    }
}