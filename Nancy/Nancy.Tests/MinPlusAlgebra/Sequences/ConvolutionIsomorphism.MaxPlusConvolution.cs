using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public partial class ConvolutionIsomorphism
{
    public static IEnumerable<(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling, Rational T, Rational d)> IsospeedMaxPlusConvolutions()
    {
        foreach (var (cf, cg) in Curves.ConvolutionIsomorphism.MaxPlusConvolutionPairs())
        {
            Curve f, g; // actual curves to which isospeed is applied
            
            var tstar_f = cf.BaseSequence.LastPlateauStart;
            var tstar_g = cg.BaseSequence.LastPlateauStart;
            
            // todo: fill in reference
            // If Lemma X does not apply, workaround according to Remark Y in [TBP23]
            var fpstarCut = cf
                .CutAsEnumerable(cf.PseudoPeriodStart, tstar_f + cf.PseudoPeriodLength)
                .Fill(0, cf.PseudoPeriodStart, fillWith: Rational.MinusInfinity)
                .ToSequence();
            f = new Curve(
                fpstarCut,
                tstar_f,
                cf.PseudoPeriodLength,
                cf.PseudoPeriodHeight
            );

            var gpstarCut = cg
                .CutAsEnumerable(cg.PseudoPeriodStart, tstar_g + cg.PseudoPeriodLength)
                .Fill(0, cg.PseudoPeriodStart, fillWith: Rational.MinusInfinity)
                .ToSequence();
            g = new Curve(
                gpstarCut,
                tstar_g,
                cg.PseudoPeriodLength,
                cg.PseudoPeriodHeight
            );
            
            // todo: fill in reference
            // from here on, f and g satisfy Lemma X 
            
            var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
            var k_c_f = lcm_c / f.PseudoPeriodHeight;
            var k_c_g = lcm_c / g.PseudoPeriodHeight;
            var lcm_d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var d = Rational.Min(lcm_d, Rational.Min(k_c_f * f.PseudoPeriodLength, k_c_g * g.PseudoPeriodLength));
            var c = d * Rational.Max(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

            var tf = f.PseudoPeriodStart;
            var tg = g.PseudoPeriodStart;
            var T = tf + tg + lcm_d;

            var fCut = lcm_d <= k_c_f * f.PseudoPeriodLength 
                ? f.Cut(tf, tf + 2 * lcm_d, isEndIncluded: false)
                : f.Cut(tf, tf + 2 * k_c_f * f.PseudoPeriodLength, isEndIncluded: true);
            var gCut = lcm_d <= k_c_g * g.PseudoPeriodLength 
                ? g.Cut(tg, tg + 2 * lcm_d, isEndIncluded: false)
                : g.Cut(tg, tg + 2 * k_c_g * g.PseudoPeriodLength, isEndIncluded: true);

            var cutEnd = tf + tg + lcm_d + d;
            var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;

            yield return (fCut, gCut, cutEnd, cutCeiling, T, d); 
        }
    }
    
    public static IEnumerable<object[]> IsospeedMaxPlusConvolutionsTestCases()
    {
        foreach(var (f, g, cutEnd, cutCeiling, T, d) in IsospeedMaxPlusConvolutions())
        {
            yield return new object[] { f, g, cutEnd, cutCeiling, T, d };
        }
    }

    [Theory]
    [MemberData(nameof(IsospeedMaxPlusConvolutionsTestCases))]
    public void MaxPlusConvolution_Isospeed_Equivalence(Sequence f, Sequence g, Rational cutEnd, Rational cutCeiling,
        Rational T, Rational d)
    {
        // this test verifies that, for the cuts given by isospeed, we can compute the by-sequence convolution also via isomorphism

        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");
        output.WriteLine($"var cutEnd = {cutEnd}; var cutCeiling = {cutCeiling}; var T = {T}; var d = {d};");

        var settings = ComputationSettings.Default() with {
            UseBySequenceConvolutionIsomorphismOptimization = false
        };

        // direct algorithm, using max-plus convolution
        var direct_result = Sequence.MaxPlusConvolution(f, g, settings, cutEnd, cutCeiling, true, false, false);

        // inverse algorithm, using min-plus convolution
        var tb_f_prime = f.LastPlateauStart;
        var tb_g_prime = g.LastPlateauStart;

        var f_lpi = f.LowerPseudoInverse(false);
        var g_lpi = g.LowerPseudoInverse(false);
        var minp = Sequence.Convolution(
            f_lpi, g_lpi,
            cutEnd: cutCeiling, cutCeiling: cutEnd,
            isEndIncluded: true, isCeilingIncluded: true,
            settings: settings);
        var inverse_raw = minp.UpperPseudoInverse(false);
        if (inverse_raw.DefinedUntil < cutEnd)
        {
            // this means that, in minp, there was a discontinuity over the cutCeiling
            // hence, there should be a constant segment until cutEnd
            // todo: does this affect min-plus as well?
            var missingSegment = Segment.Constant(
                inverse_raw.DefinedUntil,
                (Rational) cutEnd,
                ((Point) inverse_raw.Elements.Last()).Value
            );
            inverse_raw = inverse_raw.Elements
                .Append(missingSegment)
                .ToSequence();
        }

        // todo: rename in the actual alg as well
        Sequence inverse;
        if (tb_f_prime == f.DefinedUntil && tb_g_prime == g.DefinedUntil)
        {
            inverse = inverse_raw;
        }
        else
        {
            // todo: can be optimized by applying horizontal and vertical filtering 
            var missingElements = new List<Element> { };
            if (tb_f_prime < f.DefinedUntil)
            {
                var pf = f.GetElementAt(tb_f_prime);
                var sf = f.GetSegmentAfter(tb_f_prime);
                foreach (var eg in g.Elements)
                {
                    missingElements.AddRange(Element.MaxPlusConvolution(pf, eg));
                    missingElements.AddRange(Element.MaxPlusConvolution(sf, eg));
                }
            }

            if (tb_g_prime < g.DefinedUntil)
            {
                var pg = g.GetElementAt(tb_g_prime);
                var sg = g.GetSegmentAfter(tb_g_prime);
                foreach (var ef in f.Elements)
                {
                    missingElements.AddRange(Element.MaxPlusConvolution(ef, pg));
                    missingElements.AddRange(Element.MaxPlusConvolution(ef, sg));
                }
            }

            var upperEnvelope = missingElements.UpperEnvelope();
            var ext = upperEnvelope.ToSequence(
                fillFrom: upperEnvelope.First().StartTime, 
                fillTo: upperEnvelope.Last().EndTime, 
                fillWith: Rational.MinusInfinity
            );
            inverse = Sequence.Maximum(ext, inverse_raw, false);
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

    public static IEnumerable<(Sequence f, Sequence g)> RightContinuousConvolutions()
    {
        var sequences = RightContinuousExamples().Concat(ContinuousExamples());

        var pairs = sequences.SelectMany(
            f => sequences.Select(
                g => (f, g)
            )
        );
        return pairs;
    }

    public static IEnumerable<object[]> RightContinuousConvolutionTestcases()
    {
        foreach (var (f, g) in RightContinuousConvolutions())
        {
            yield return new object[] {f, g};
        }
    }

    [Theory]
    [MemberData(nameof(RightContinuousConvolutionTestcases))]
    public void MaxPlusConvolution_Equivalence(Sequence f, Sequence g)
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

        var direct = Sequence.MaxPlusConvolution(f, g).Cut(cutStart, cutEnd);

        var tb_f_prime = f.LastPlateauStart;
        var tb_g_prime = g.LastPlateauStart;

        var f_lpi = f.LowerPseudoInverse(false);
        var g_lpi = g.LowerPseudoInverse(false);

        var minp = Sequence.Convolution(f_lpi, g_lpi);
        var inverse_raw = minp.UpperPseudoInverse(false);

        output.WriteLine($"var direct = {direct.ToCodeString()};");
        output.WriteLine($"var inverse_raw = {inverse_raw.ToCodeString()};");

        if (tb_f_prime == tb_f && tb_g_prime == tb_g)
        {
            var inverse = inverse_raw.Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, inverse));
        }
        else
        {
            // note: does not handle left-open sequences
            var missingElements = new List<Element> { };
            if (tb_f_prime < tb_f)
            {
                var pf = f.GetElementAt(tb_f_prime);
                var sf = f.GetSegmentAfter(tb_f_prime);
                foreach (var eg in g.Elements)
                {
                    missingElements.AddRange(Element.MaxPlusConvolution(pf, eg));
                    missingElements.AddRange(Element.MaxPlusConvolution(sf, eg));
                }
            }

            if (tb_g_prime < tb_g)
            {
                var pg = g.GetElementAt(tb_g_prime);
                var sg = g.GetSegmentAfter(tb_g_prime);
                foreach (var ef in f.Elements)
                {
                    missingElements.AddRange(Element.MaxPlusConvolution(ef, pg));
                    missingElements.AddRange(Element.MaxPlusConvolution(ef, sg));
                }
            }

            var upperEnvelope = missingElements.UpperEnvelope();
            var ext = upperEnvelope.ToSequence(
                fillFrom: upperEnvelope.First().StartTime, 
                fillTo: upperEnvelope.Last().EndTime, 
                fillWith: Rational.MinusInfinity
            );
            var reconstructed = Sequence.Maximum(ext, inverse_raw, false)
                .Cut(cutStart, cutEnd);
            Assert.True(Sequence.Equivalent(direct, reconstructed));
        }
    }
}