using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
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
    {
        foreach (var (a, b) in MinPlusConvolutionPairs())
        {
            yield return new object[] { a, b };
        }
    }

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

        var minp = Curve.Convolution(f, g, noIsomorphismSettings);
        var iso = Curve.Convolution(f, g, convolutionIsomorphismSettings);
        var f_upi = f.UpperPseudoInverse();
        var g_upi = g.UpperPseudoInverse();
        var upis_maxp = Curve.MaxPlusConvolution(f_upi, g_upi, noIsomorphismSettings);
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

        var minp = curves.Convolution(noIsomorphismSettings);
        var iso = curves.Convolution(convolutionIsomorphismSettings);
        var upis = curves
            .Select(c => c.UpperPseudoInverse())
            .ToList();
        var upis_maxp = upis
            .MaxPlusConvolution(noIsomorphismSettings);
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

        var minp = delayed.Convolution(noIsomorphismSettings);
        var iso = delayed.Convolution(convolutionIsomorphismSettings);
        var maxp = delayed
            .Select(c => c.UpperPseudoInverse())
            .MaxPlusConvolution(noIsomorphismSettings)
            .LowerPseudoInverse();

        var where = Curve.FindFirstInequivalence(minp, maxp);
        Assert.True(Curve.Equivalent(minp, maxp));
        Assert.True(Curve.Equivalent(minp, iso));
        Assert.True(Curve.Equivalent(iso, maxp));
    }
}