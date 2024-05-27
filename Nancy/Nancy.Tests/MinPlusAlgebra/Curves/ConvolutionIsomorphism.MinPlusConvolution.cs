using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public partial class ConvolutionIsomorphism
{
    public static IEnumerable<(Curve f, Curve g)> MinPlusConvolutionPairs()
    {
        var cc_testcases = ContinuousExamples.SelectMany(
            f => ContinuousExamples.Select(g => (f, g))
        );
        var lc_testcases = ContinuousExamples.SelectMany(
            f => LeftContinuousExamples.Select(g => (f, g))
        );
        var ll_testcases = LeftContinuousExamples.SelectMany(
            f => LeftContinuousExamples.Select(g => (f, g))
        );
        var testcases = cc_testcases
            .Concat(lc_testcases)
            .Concat(ll_testcases);

        return testcases;
    }

    public static IEnumerable<object[]> MinPlusConvolutionIsomorphismPairTestCases()
        => MinPlusConvolutionPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairTestCases))]
    void MinPlusConvolutionIsomorphismPairEquivalence(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsLeftContinuous);
        Assert.True(g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(g.IsNonDecreasing);

        var minp = Curve.Convolution(f, g, noIsospeedSettings);
        var iso = Curve.Convolution(f, g, convolutionIsospeedSettings);
        var f_upi = f.UpperPseudoInverse();
        var g_upi = g.UpperPseudoInverse();
        var upis_maxp = Curve.MaxPlusConvolution(f_upi, g_upi, noIsospeedSettings);
        var maxp = upis_maxp.LowerPseudoInverse();

        output.WriteLine($"var minp = {minp.ToCodeString()};");
        output.WriteLine($"var maxp = {maxp.ToCodeString()};");
        output.WriteLine($"var iso = {iso.ToCodeString()};");

        Assert.True(iso.IsLeftContinuous);

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
        Assert.True(Curve.Equivalent(iso, maxp));
    }

    [Theory]
    [MemberData(nameof(ConvolutionsSetTestCases))]
    void MinPlusConvolutionIsomorphismSetEquivalence(Curve[] curves)
    {
        foreach (var c in curves)
            output.WriteLine(c.ToCodeString());

        Assert.True(curves.All(c => c.IsLeftContinuous));
        Assert.True(curves.All(c => c.IsNonDecreasing));

        var minp = curves.Convolution(noIsospeedSettings);
        var iso = curves.Convolution(convolutionIsospeedSettings);
        var upis = curves
            .Select(c => c.UpperPseudoInverse())
            .ToList();
        var upis_maxp = upis
            .MaxPlusConvolution(noIsospeedSettings);
        var maxp = upis_maxp
            .LowerPseudoInverse();

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
        Assert.True(Curve.Equivalent(iso, maxp));
    }

    [Theory]
    [MemberData(nameof(ConvolutionsSetTestCases))]
    void MinPlusConvolutionIsomorphismSetEquivalenceWithDelay(Curve[] curves)
    {
        if (curves.Any(c => c.ValueAt(0) > 0))
            return; // otherwise, the delayed curve is not left-continuous

        var delayed = curves
            .Select(c => c.DelayBy(c.PseudoPeriodHeight * c.PseudoPeriodLength))
            .ToList();

        Assert.True(delayed.All(c => c.IsLeftContinuous));
        Assert.True(delayed.All(c => c.IsNonDecreasing));

        var minp = delayed.Convolution(noIsospeedSettings);
        var iso = delayed.Convolution(convolutionIsospeedSettings);
        var maxp = delayed
            .Select(c => c.UpperPseudoInverse())
            .MaxPlusConvolution(noIsospeedSettings)
            .LowerPseudoInverse();

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
        Assert.True(Curve.Equivalent(iso, maxp));
    }

    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairTestCases))]
    void MinPlusConvolutionSuperIsospeedCheck(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsLeftContinuous);
        Assert.True(g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(g.IsNonDecreasing);

        var d_f = f.PseudoPeriodLength;
        var d_g = g.PseudoPeriodLength;
        var lcm_d = Rational.LeastCommonMultiple(d_f, d_g);
        var k_d_f = lcm_d / d_f;
        var k_d_g = lcm_d / d_g;

        var c_f = f.PseudoPeriodHeight;
        var c_g = g.PseudoPeriodHeight;
        var lcm_c = Rational.LeastCommonMultiple(c_f, c_g);
        var k_c_f = lcm_c / c_f;
        var k_c_g = lcm_c / c_g;

        var d_star = k_c_f * d_f > k_c_g * d_g ?
            // based on d_f
            Rational.GreatestCommonDivisor(k_d_f, k_c_f) * d_f :
            // based on d_g
            Rational.GreatestCommonDivisor(k_d_g, k_c_g) * d_g;

        var convolution = Curve.Convolution(f, g, convolutionIsospeedSettings with { UseRepresentationMinimization = true });
        if (!convolution.IsUltimatelyAffine)
        {
            var d_min = convolution.PseudoPeriodLength;
            var k = d_star / d_min;
            Assert.True(k.IsInteger);
        }
    }
    
    [Theory]
    [MemberData(nameof(MinPlusConvolutionIsomorphismPairTestCases))]
    void MinPlusConvolutionSuperIsospeedEquivalence(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsLeftContinuous);
        Assert.True(g.IsLeftContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(g.IsNonDecreasing);

        var iso = Curve.Convolution(f, g, convolutionIsospeedSettings);
        var superIso = Curve.Convolution(f, g, convolutionSuperIsospeedSettings);
        
        Assert.True(Curve.Equivalent(iso, superIso));
    }
}