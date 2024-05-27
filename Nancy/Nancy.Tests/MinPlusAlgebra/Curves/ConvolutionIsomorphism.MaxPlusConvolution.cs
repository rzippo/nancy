using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public partial class ConvolutionIsomorphism
{
    public static List<(Curve f, Curve g)> SelectedMaxPlusConvolutionPairs = new List<(Curve f, Curve g)>
    {
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,79), new Segment(0,32,79,0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 32,pseudoPeriodHeight: 79),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,83), new Segment(0,60,83,0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 60,pseudoPeriodHeight: 83)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,21), new Segment(0,28,21,0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 28,pseudoPeriodHeight: 21),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,37), new Segment(0,52,37,0) }),pseudoPeriodStart: 0,pseudoPeriodLength: 52,pseudoPeriodHeight: 37)
        )
    };

    public static IEnumerable<(Curve f, Curve g)> MaxPlusConvolutionPairs()
    {
        var cc_testcases = ContinuousExamples.SelectMany(
            f => ContinuousExamples.Select(g => (f, g))
        );
        var rc_testcases = ContinuousExamples.SelectMany(
            f => RightContinuousExamples.Select(g => (f, g))
        );
        var rr_testcases = RightContinuousExamples.SelectMany(
            f => RightContinuousExamples.Select(g => (f, g))
        );
        var testcases = cc_testcases
            .Concat(rc_testcases)
            .Concat(rr_testcases)
            .Concat(SelectedMaxPlusConvolutionPairs);

        return testcases;
    }

    public static IEnumerable<object[]> MaxPlusConvolutionIsomorphismPairTestCases()
    {
        foreach (var (a, b) in MaxPlusConvolutionPairs())
        {
            yield return new object[] { a, b };
        }
    }

    public static IEnumerable<object[]> MaxPlusIsomorphismTestCases()
    {
        var cc_testcases = ContinuousExamples.SelectMany(
            a => ContinuousExamples.Select(b => new[] { a, b })
        );
        var rc_testcases = ContinuousExamples.SelectMany(
            a => RightContinuousExamples.Select(b => new[] { a, b })
        );
        var rr_testcases = RightContinuousExamples.SelectMany(
            a => RightContinuousExamples.Select(b => new[] { a, b })
        );
        var testcases = cc_testcases
            .Concat(rc_testcases)
            .Concat(rr_testcases);

        foreach (var curves in testcases)
        {
            yield return new object[] { curves };
        }
    }

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionIsomorphismPairTestCases))]
    void MaxPlusConvolutionEquivalence(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsRightContinuous);
        Assert.True(g.IsRightContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(g.IsNonDecreasing);

        var maxp = Curve.MaxPlusConvolution(f, g, noIsospeedSettings);
        var iso = Curve.MaxPlusConvolution(f, g, convolutionIsospeedSettings);
        var f_lpi = f.LowerPseudoInverse();
        var g_lpi = g.LowerPseudoInverse();
        var lpis_minp = Curve.Convolution(f_lpi, g_lpi, noIsospeedSettings);
        var minp = lpis_minp.UpperPseudoInverse();

        output.WriteLine($"var maxp = {maxp.ToCodeString()};");
        output.WriteLine($"var minp = {minp.ToCodeString()};");
        output.WriteLine($"var iso = {iso.ToCodeString()};");

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(iso, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
    }

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionIsomorphismPairTestCases))]
    void MaxPlusConvolutionEquivalenceWithDelay(Curve f, Curve g)
    {
        var f_d = f.DelayBy(f.PseudoPeriodHeight * f.PseudoPeriodLength);
        var g_d = g.DelayBy(g.PseudoPeriodHeight * g.PseudoPeriodLength);

        output.WriteLine($"var f_d = {f_d.ToCodeString()};");
        output.WriteLine($"var g_d = {g_d.ToCodeString()};");

        Assert.True(f_d.IsRightContinuous);
        Assert.True(g_d.IsRightContinuous);
        Assert.True(f_d.IsNonDecreasing);
        Assert.True(g_d.IsNonDecreasing);

        var maxp = Curve.MaxPlusConvolution(f_d, g_d, noIsospeedSettings);
        var iso = Curve.MaxPlusConvolution(f_d, g_d, convolutionIsospeedSettings);
        var f_d_lpi = f_d.LowerPseudoInverse();
        var g_d_lpi = g_d.LowerPseudoInverse();
        var lpis_minp = Curve.Convolution(f_d_lpi, g_d_lpi, noIsospeedSettings);
        var minp = lpis_minp.UpperPseudoInverse();

        output.WriteLine($"var maxp = {maxp.ToCodeString()};");
        output.WriteLine($"var minp = {minp.ToCodeString()};");
        output.WriteLine($"var iso = {iso.ToCodeString()};");

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(iso, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
    }

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionIsomorphismPairTestCases))]
    void MaxPlusConvolutionSuperIsospeedCheck(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsRightContinuous);
        Assert.True(g.IsRightContinuous);
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

        var d_star = k_c_f * d_f < k_c_g * d_g ?
            // based on d_f
            Rational.GreatestCommonDivisor(k_d_f, k_c_f) * d_f :
            // based on d_g
            Rational.GreatestCommonDivisor(k_d_g, k_c_g) * d_g;

        var convolution = Curve.MaxPlusConvolution(f, g, convolutionIsospeedSettings with { UseRepresentationMinimization = true });
        if (!convolution.IsUltimatelyAffine)
        {
            var d_min = convolution.PseudoPeriodLength;
            var k = d_star / d_min;
            Assert.True(k.IsInteger);
        }
    }
    
    [Theory]
    [MemberData(nameof(MaxPlusConvolutionIsomorphismPairTestCases))]
    void MaxPlusConvolutionSuperIsospeedEquivalence(Curve f, Curve g)
    {
        output.WriteLine($"var f = {f.ToCodeString()};");
        output.WriteLine($"var g = {g.ToCodeString()};");

        Assert.True(f.IsRightContinuous);
        Assert.True(g.IsRightContinuous);
        Assert.True(f.IsNonDecreasing);
        Assert.True(g.IsNonDecreasing);

        var iso = Curve.MaxPlusConvolution(f, g, convolutionIsospeedSettings);
        var superIso = Curve.MaxPlusConvolution(f, g, convolutionSuperIsospeedSettings);
        
        Assert.True(Curve.Equivalent(iso, superIso));
    }
}