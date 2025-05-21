using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ContinuityProjections
{
    public static IEnumerable<Curve> Curves =
        ConvolutionIsomorphism.ContinuousExamples
            .Concat(ConvolutionIsomorphism.LeftContinuousExamples)
            .Concat(ConvolutionIsomorphism.RightContinuousExamples);

    public static IEnumerable<object[]> Testcases =
        Curves.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(Testcases))]
    public void ToLeftContinuousIsLeftContinuous(Curve curve)
    {
        var result = curve.ToLeftContinuous();
        Assert.True(result.IsLeftContinuous);
        
        if(curve.IsLeftContinuous)
            Assert.True(Curve.Equivalent(curve, result));
    }
    
    [Theory]
    [MemberData(nameof(Testcases))]
    public void ToRightContinuousIsRightContinuous(Curve curve)
    {
        var result = curve.ToRightContinuous();
        Assert.True(result.IsRightContinuous);
        
        if(curve.IsRightContinuous)
            Assert.True(Curve.Equivalent(curve, result));
    }

    public static IEnumerable<Curve> NonDecreasingCurves = 
        Curves.Where(f => f.IsNonDecreasing);

    public static IEnumerable<(Curve f, Curve g)> MonotonyPairs =
        NonDecreasingCurves.SelectMany(f =>
            NonDecreasingCurves.Select(g => (f, g))
                .Where(pair => pair.f <= pair.g)
        );

    public static IEnumerable<object[]> MonotonyTestCases_1 =
        MonotonyPairs.ToXUnitTestCases();
    
    /// <summary>
    /// Property 4.2, first statement, in [Gui24].
    /// </summary>
    /// <param name="f">A non-decreasing curve. Upper-bounded by <paramref name="g"/>.</param>
    /// <param name="g">A non-decreasing curve. Lower-bounded by <paramref name="f"/>.</param>
    [Theory]
    [MemberData(nameof(MonotonyTestCases_1))]
    public void MonotonyOfProjections_1(Curve f, Curve g)
    {
        // 
        var f_r = f.ToRightContinuous();
        var g_r = g.ToRightContinuous();
        Assert.True(f_r <= g_r);
        
        var f_l = f.ToLeftContinuous();
        var g_l = g.ToLeftContinuous();
        Assert.True(f_l <= g_l);
    }
    
    public static IEnumerable<object[]> MonotonyTestCases_2 =
        NonDecreasingCurves.ToXUnitTestCases();
    
    /// <summary>
    /// Property 4.2, second statement, in [Gui24].
    /// </summary>
    /// <param name="f">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(MonotonyTestCases_2))]
    public void MonotonyOfProjections_2(Curve f)
    {
        var f_r = f.ToRightContinuous();
        var f_l = f.ToLeftContinuous();
        
        Assert.True(f_l <= f);
        Assert.True(f <= f_r);
    }

    /// <summary>
    /// This subset is defined in p. 137 of [Gui24],
    /// however the property was not given a specific name.
    /// </summary>
    public static IEnumerable<Curve> OverdotCurves =
        Curves
            .Where(f => f.ValueAt(0) == 0)
            .Where(f => f.RightLimitAt(0) == 0);
    
    public static IEnumerable<object[]> CompositionTestCases =
        OverdotCurves.ToXUnitTestCases();
    
    /// <summary>
    /// Property 4.3, first statement, in [Gui24] 
    /// </summary>
    /// <param name="f">An "overdot" curve.</param>
    [Theory]
    [MemberData(nameof(CompositionTestCases))]
    public void CompositionOfProjections_1(Curve f)
    {
        var f_r = f.ToRightContinuous();
        var f_l = f.ToLeftContinuous();
        var f_r_l = f_r.ToLeftContinuous();
        
        Assert.True(Curve.Equivalent(f_r_l, f_l));
    }
    
    /// <summary>
    /// Property 4.3, second statement, in [Gui24].
    /// </summary>
    /// <param name="f">An "overdot" curve.</param>
    [Theory]
    [MemberData(nameof(CompositionTestCases))]
    public void CompositionOfProjections_2(Curve f)
    {
        var f_r = f.ToRightContinuous();
        var f_l = f.ToLeftContinuous();
        var f_l_r = f_l.ToRightContinuous();
        
        Assert.True(Curve.Equivalent(f_l_r, f_r));
    }
    
    public static IEnumerable<(Curve f, Curve g)> ConvolutionProjectionsPairs_1 =
        NonDecreasingCurves.SelectMany(f =>
            NonDecreasingCurves.Select(g => (f, g)));
    
    public static IEnumerable<object[]> ConvolutionProjectionsTestCases_1 =
        ConvolutionProjectionsPairs_1.ToXUnitTestCases();
    
    /// <summary>
    /// Theorem 4.1, equation 4.6, in [Gui24].
    /// </summary>
    /// <param name="f">A non-decreasing curve.</param>
    /// <param name="g">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(ConvolutionProjectionsTestCases_1))]
    public void ConvolutionProjections_1(Curve f, Curve g)
    {
        var fg = Curve.Convolution(f, g);
        var fg_l = fg.ToLeftContinuous();

        var f_l = f.ToLeftContinuous();
        var g_l = g.ToLeftContinuous();
        var f_l_conv_g_l = Curve.Convolution(f_l, g_l);
        
        Assert.True(Curve.Equivalent(fg_l, f_l_conv_g_l));
    }
    
    public static IEnumerable<(Curve f, Curve g)> ConvolutionProjectionsPairs_2 =
        NonDecreasingCurves
            .Where(fp => fp.RightLimitAt(0) == fp.ValueAt(0))
            .SelectMany(fp =>
                NonDecreasingCurves.Select(g => (fp, g)));
    
    public static IEnumerable<object[]> ConvolutionProjectionsTestCases_2 =
        ConvolutionProjectionsPairs_2.ToXUnitTestCases();
    
    /// <summary>
    /// Theorem 4.1, equation 4.7, in [Gui24].
    /// </summary>
    /// <param name="fp">A non-decreasing curve, where $fp(0+) = fp(0)$.</param>
    /// <param name="g">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(ConvolutionProjectionsTestCases_2))]
    public void ConvolutionProjections_2(Curve fp, Curve g)
    {
        var fpg = Curve.Convolution(fp, g);
        var fg_r = fpg.ToRightContinuous();

        var fp_r = fp.ToRightContinuous();
        var fp_r_conv_g = Curve.Convolution(fp_r, g);
        
        Assert.True(Curve.Equivalent(fg_r, fp_r_conv_g));
    }
}