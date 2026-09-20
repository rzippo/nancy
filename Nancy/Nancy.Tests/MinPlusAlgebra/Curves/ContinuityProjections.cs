using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
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

    /// <summary>
    /// Property 4.4 in [Gui24]:
    /// the projections of a non-decreasing curve are non-decreasing, and land in the left- and right-continuous sets respectively.
    /// </summary>
    /// <param name="f">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(MonotonyTestCases_2))]
    public void StabilityOfNonDecreasingSubsets(Curve f)
    {
        var f_l = f.ToLeftContinuous();
        var f_r = f.ToRightContinuous();

        Assert.True(f_l.IsNonDecreasing);
        Assert.True(f_l.IsLeftContinuous);
        Assert.True(f_r.IsNonDecreasing);
        Assert.True(f_r.IsRightContinuous);
    }

    /// <summary>
    /// Times worth sampling a projection at:
    /// every breakpoint of the curve and of both its projections, the midpoints between them, and a grid reaching past the second pseudo-period.
    /// </summary>
    public static IEnumerable<Rational> SampleTimes(Curve f)
    {
        var horizon = f.PseudoPeriodStart + 2 * f.PseudoPeriodLength + 1;
        var times = new List<Rational>();
        foreach (var c in new[] { f, f.ToLeftContinuous(), f.ToRightContinuous() })
            times.AddRange(c.Cut(0, horizon, true, true).EnumerateBreakpoints().Select(bp => bp.center.Time));

        var breakpoints = times.Distinct().OrderBy(x => x).ToList();
        for (var i = 0; i + 1 < breakpoints.Count; i++)
            times.Add((breakpoints[i] + breakpoints[i + 1]) / 2);
        for (var i = 0; i <= 20; i++)
            times.Add(horizon * new Rational(i, 20));

        return times.Distinct().Where(x => x >= 0);
    }

    /// <summary>
    /// Definition 4.1 and Remark 4.2 in [Gui24], which the rest of this file rests on:
    /// the left projection carries the left limit, except at the origin where it carries the value, and the right projection carries the right limit.
    /// </summary>
    /// <param name="f">Any curve of the corpus.</param>
    [Theory]
    [MemberData(nameof(Testcases))]
    public void ProjectionsAreTheOneSidedLimits(Curve f)
    {
        var f_l = f.ToLeftContinuous();
        var f_r = f.ToRightContinuous();

        Assert.Equal(f.ValueAt(0), f_l.ValueAt(0));

        foreach (var time in SampleTimes(f))
        {
            if (time > 0)
                Assert.Equal(f.LeftLimitAt(time), f_l.ValueAt(time));
            Assert.Equal(f.RightLimitAt(time), f_r.ValueAt(time));
        }
    }

    /// <summary>
    /// A projection moves the value at a time, and leaves both one-sided limits where they were.
    /// This is what lets Property 4.2 and Property 4.3 in [Gui24] compose the two operators.
    /// </summary>
    /// <param name="f">Any curve of the corpus.</param>
    [Theory]
    [MemberData(nameof(Testcases))]
    public void ProjectionsKeepTheOneSidedLimits(Curve f)
    {
        var f_l = f.ToLeftContinuous();
        var f_r = f.ToRightContinuous();

        foreach (var time in SampleTimes(f))
        {
            if (time > 0)
            {
                Assert.Equal(f.LeftLimitAt(time), f_l.LeftLimitAt(time));
                Assert.Equal(f.LeftLimitAt(time), f_r.LeftLimitAt(time));
            }
            Assert.Equal(f.RightLimitAt(time), f_l.RightLimitAt(time));
            Assert.Equal(f.RightLimitAt(time), f_r.RightLimitAt(time));
        }
    }

    /// <summary>
    /// Property 4.5, equation 4.4, in [Gui24]: the "overdot" set is stable under both projections.
    /// Membership is four conditions, and the origin is only two of them:
    /// the projection must also be non-decreasing and continuous from its own side.
    /// </summary>
    /// <param name="f">An "overdot" curve.</param>
    [Theory]
    [MemberData(nameof(CompositionTestCases))]
    public void StabilityOfOverdotSubsets(Curve f)
    {
        var f_l = f.ToLeftContinuous();
        var f_r = f.ToRightContinuous();

        foreach (var projection in new[] { f_l, f_r })
        {
            Assert.Equal(0, projection.ValueAt(0));
            Assert.Equal(0, projection.RightLimitAt(0));
            Assert.True(projection.IsNonDecreasing);
        }

        Assert.True(f_l.IsLeftContinuous);
        Assert.True(f_r.IsRightContinuous);
    }

    public static IEnumerable<Curve> ZeroAtOriginCurves =
        NonDecreasingCurves.Where(f => f.ValueAt(0) == 0);

    public static IEnumerable<object[]> ZeroAtOriginTestCases =
        ZeroAtOriginCurves.ToXUnitTestCases();

    /// <summary>
    /// Property 4.5, equation 4.5, in [Gui24]:
    /// the left projection keeps a curve zero at the origin, while the right projection carries the right limit there instead.
    /// </summary>
    /// <param name="f">A non-decreasing curve, zero at the origin.</param>
    [Theory]
    [MemberData(nameof(ZeroAtOriginTestCases))]
    public void StabilityOfZeroAtOriginSubsets(Curve f)
    {
        var f_l = f.ToLeftContinuous();
        Assert.Equal(0, f_l.ValueAt(0));
        Assert.True(f_l.IsNonDecreasing);
        Assert.True(f_l.IsLeftContinuous);

        var f_r = f.ToRightContinuous();
        Assert.Equal(f.RightLimitAt(0), f_r.ValueAt(0));
    }

    /// <summary>
    /// The right projection of a curve zero at the origin need not be zero there, which is the non-inclusion of Property 4.5, equation 4.5, in [Gui24].
    /// </summary>
    [Fact]
    public void RightProjectionOfACurveZeroAtTheOriginCanBePositiveThere()
    {
        var f = new SigmaRhoArrivalCurve(3, 1);
        var f_r = f.ToRightContinuous();

        Assert.Equal(0, f.ValueAt(0));
        Assert.Equal(3, f_r.ValueAt(0));

        // the value at the origin is the only condition that fails: the rest of R0^ still holds
        Assert.True(f_r.IsNonDecreasing);
        Assert.True(f_r.IsRightContinuous);
    }

    public static IEnumerable<(Curve f, Curve g)> DeviationPairs =
        NonDecreasingCurves.SelectMany(f =>
            NonDecreasingCurves.Select(g => (f, g))
                .Where(pair => pair.g <= pair.f)
        );

    public static IEnumerable<object[]> DeviationTestCases =
        DeviationPairs.ToXUnitTestCases();

    /// <summary>
    /// Equation 4.17 in [Gui24], the stronger form of the earlier result it reports:
    /// the horizontal deviation is the same as that of either projection of both operands.
    /// </summary>
    /// <param name="f">A non-decreasing curve, lower-bounded by <paramref name="g"/>.</param>
    /// <param name="g">A non-decreasing curve, upper-bounded by <paramref name="f"/>.</param>
    [Theory]
    [MemberData(nameof(DeviationTestCases))]
    public void HorizontalDeviationIsTheSameUnderEitherProjection(Curve f, Curve g)
    {
        var hDev = Curve.HorizontalDeviation(f, g);

        Assert.Equal(hDev, Curve.HorizontalDeviation(f.ToLeftContinuous(), g.ToLeftContinuous()));
        Assert.Equal(hDev, Curve.HorizontalDeviation(f.ToRightContinuous(), g.ToRightContinuous()));
    }

    public static IEnumerable<(Curve f, Curve g)> OverdotDeviationPairs =
        OverdotCurves.SelectMany(f =>
            OverdotCurves.Select(g => (f, g))
                .Where(pair => pair.g <= pair.f)
        );

    public static IEnumerable<object[]> OverdotDeviationTestCases =
        OverdotDeviationPairs.ToXUnitTestCases();

    /// <summary>
    /// Equation 4.16 in [Gui24]:
    /// the vertical deviation is the same under either projection, as long as both operands are projected the same way.
    /// </summary>
    /// <param name="f">An "overdot" curve, lower-bounded by <paramref name="g"/>.</param>
    /// <param name="g">An "overdot" curve, upper-bounded by <paramref name="f"/>.</param>
    [Theory]
    [MemberData(nameof(OverdotDeviationTestCases))]
    public void VerticalDeviationIsTheSameUnderMatchedProjections(Curve f, Curve g)
    {
        Assert.Equal(
            Curve.VerticalDeviation(f.ToLeftContinuous(), g.ToLeftContinuous()),
            Curve.VerticalDeviation(f.ToRightContinuous(), g.ToRightContinuous())
        );
    }

    /// <summary>
    /// The example of Figure 4.7 in [Gui24], where the arrival curve is right-continuous and the departure curve left-continuous:
    /// the backlog is then 1.5, against the 0.5 both curves give when read with the same continuity.
    /// </summary>
    [Fact]
    public void MixedContinuityChangesTheVerticalDeviation()
    {
        // all the data arrives at once at t = 1, and the curve carries that instant's value
        var a = new Curve(
            baseSequence: new Sequence([
                Point.Origin(), Segment.Zero(0, 1),
                new Point(1, new Rational(3, 2)), Segment.Constant(1, 2, new Rational(3, 2))
            ]),
            pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 0
        );
        // it leaves between t = 1 and t = 2, and the curve is still 0 at the instant it starts
        var d = new Curve(
            baseSequence: new Sequence([
                Point.Origin(), Segment.Zero(0, 1), Point.Zero(1),
                new Segment(1, 2, 1, new Rational(1, 2)),
                new Point(2, new Rational(3, 2)), Segment.Constant(2, 3, new Rational(3, 2))
            ]),
            pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 0
        );

        Assert.True(a.IsRightContinuous);
        Assert.True(d.IsLeftContinuous);
        Assert.True(d <= a);

        Assert.Equal(new Rational(3, 2), Curve.VerticalDeviation(a, d));
        Assert.Equal(new Rational(1, 2), Curve.VerticalDeviation(a, d.ToRightContinuous()));
        Assert.Equal(new Rational(1, 2), Curve.VerticalDeviation(a.ToLeftContinuous(), d));
    }

    /// <summary>
    /// Equation 4.16 in [Gui24] is stated over the "overdot" set, and the restriction carries its weight:
    /// two staircases that differ only in their continuity at the origin are outside it, and their projections give 30 on one side and 0 on the other.
    /// </summary>
    [Fact]
    public void VerticalDeviationUnderProjectionsDependsOnTheValueAtTheOrigin()
    {
        var f = new StairCurve(30, 100).DelayBy(0).ToRightContinuous();
        var g = new StairCurve(30, 100).DelayBy(0);

        Assert.True(g <= f);
        Assert.NotEqual(0, f.ValueAt(0));       // f is outside the "overdot" set
        Assert.NotEqual(0, g.RightLimitAt(0));  // and so is g

        Assert.Equal(30, Curve.VerticalDeviation(f.ToLeftContinuous(), g.ToLeftContinuous()));
        Assert.Equal(0, Curve.VerticalDeviation(f.ToRightContinuous(), g.ToRightContinuous()));

        // the horizontal deviation, by contrast, is the same either way, as 4.17 has it
        var hDev = Curve.HorizontalDeviation(f, g);
        Assert.Equal(hDev, Curve.HorizontalDeviation(f.ToLeftContinuous(), g.ToLeftContinuous()));
        Assert.Equal(hDev, Curve.HorizontalDeviation(f.ToRightContinuous(), g.ToRightContinuous()));
    }

    public static IEnumerable<(Curve f, Curve g)> BoundPairs =
        NonDecreasingCurves.Where(f => f.IsNonNegative)
            .SelectMany(f => NonDecreasingCurves.Select(g => (f, g)));

    public static IEnumerable<object[]> BoundTestCases =
        BoundPairs.ToXUnitTestCases();

    /// <summary>
    /// Section 4.6 in [Gui24]:
    /// left-projecting the service curve leaves the delay bound where it was, since by 4.17 the deviation is that of both projections.
    /// </summary>
    /// <param name="f">A non-negative, non-decreasing curve.</param>
    /// <param name="g">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(BoundTestCases))]
    public void LeftProjectingTheSecondOperandKeepsTheHorizontalDeviation(Curve f, Curve g)
    {
        Assert.Equal(
            Curve.HorizontalDeviation(f, g),
            Curve.HorizontalDeviation(f, g.ToLeftContinuous())
        );
    }

    /// <summary>
    /// Section 4.6 in [Gui24]:
    /// left-projecting the service curve can only raise the backlog bound, since the projection lowers the curve it is subtracted from.
    /// </summary>
    /// <param name="f">A non-negative, non-decreasing curve.</param>
    /// <param name="g">A non-decreasing curve.</param>
    [Theory]
    [MemberData(nameof(BoundTestCases))]
    public void LeftProjectingTheSecondOperandDoesNotDecreaseTheVerticalDeviation(Curve f, Curve g)
    {
        Assert.True(
            Curve.VerticalDeviation(f, g.ToLeftContinuous()) >= Curve.VerticalDeviation(f, g)
        );
    }
}
