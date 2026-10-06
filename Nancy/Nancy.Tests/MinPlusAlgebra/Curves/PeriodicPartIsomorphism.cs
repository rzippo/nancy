using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the (·,+) convolutions of the periodic parts of curves, which are $-\infty$ or $+\infty$ before their start, under the settings that use the isomorphism and under those that do not,
/// and the isomorphism itself, [ZNS26] Theorems 31 and 32.
/// </summary>
public class PeriodicPartIsomorphism
{
    private static readonly ComputationSettings WithoutIsomorphism = ComputationSettings.Default() with
    {
        UseConvolutionIsospeedOptimization = false,
        UseConvolutionSuperIsospeedOptimization = false,
        UseBySequenceConvolutionIsospeedOptimization = false
    };

    /// <summary>
    /// Pairs of right-continuous, non-decreasing curves with finite values, that are not ultimately constant.
    /// </summary>
    public static List<(Curve f, Curve g)> Pairs =
    [
        (
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 0), new Point(1, 1), new Segment(1, 2, 1, 1) ]), 1, 1, 1),
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1) ]), 0, 1, 1)
        ),
        (
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 3), new Segment(2, 3, 3, 2) ]), 2, 1, 2),
            new Curve(new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 2) ]), 0, 1, 2)
        ),
    ];

    public static IEnumerable<object[]> PairTestCases()
        => Pairs.ToXUnitTestCases();

    /// <summary>
    /// The (max,+) convolution of the periodic parts $f^\vee_p$ and $g^\vee_p$, which are $-\infty$ before $T_f$ and $T_g$,
    /// is the same with and without the isomorphism.
    /// </summary>
    [Theory]
    [MemberData(nameof(PairTestCases))]
    public void MaxPlusConvolutionOfPeriodicPartsIsTheSameWithTheIsomorphism(Curve f, Curve g)
    {
        var f_p = f.Decompose(f.PseudoPeriodStart, minDecomposition: false).right;
        var g_p = g.Decompose(g.PseudoPeriodStart, minDecomposition: false).right;

        var expected = Curve.MaxPlusConvolution(f_p, g_p, WithoutIsomorphism);
        var result = Curve.MaxPlusConvolution(f_p, g_p);
        Assert.True(Curve.Equivalent(expected, result));
    }

    /// <summary>
    /// Pairs of non-negative curves with finite values that are not ultimately constant, chosen out of seeded random ones by covering, with the fewest and smallest,
    /// each kind of tail, start at 0 or above it, continuity, plateau at the start and at the end of the period, jump and fractional value, and the relation of the periods and slopes of the two,
    /// among them operands that start above 0 with no transient.
    /// </summary>
    private static readonly (Curve f, Curve g)[] SelectedPairs =
    [
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),2,new Rational(1, 2)), new Point(new Rational(1, 2),new Rational(13, 4)), new Segment(new Rational(1, 2),1,new Rational(13, 4),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(9, 4)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(5, 4),3,2), new Point(new Rational(5, 4),new Rational(11, 2)), new Segment(new Rational(5, 4),new Rational(5, 2),new Rational(11, 2),new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(41, 8))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,0,1), new Point(1,1), new Segment(1,2,1,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,1,1,1), new Point(1,2), new Segment(1,2,2,1) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1)
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(1, 2),3,0), new Point(new Rational(1, 2),4), new Segment(new Rational(1, 2),new Rational(3, 2),4,2), new Point(new Rational(3, 2),6), new Segment(new Rational(3, 2),new Rational(5, 2),6,2) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,new Rational(1, 3)), new Point(new Rational(3, 2),new Rational(1, 2)), new Segment(new Rational(3, 2),3,new Rational(3, 2),new Rational(1, 2)) }), pseudoPeriodStart: 0, pseudoPeriodLength: 3, pseudoPeriodHeight: new Rational(9, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 4),0,new Rational(1, 2)), new Point(new Rational(1, 4),new Rational(1, 8)), new Segment(new Rational(1, 4),new Rational(1, 2),new Rational(1, 8),0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(1, 8)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 2),0,2), new Point(new Rational(1, 2),1), new Segment(new Rational(1, 2),new Rational(3, 4),1,new Rational(1, 2)), new Point(new Rational(3, 4),new Rational(17, 8)), new Segment(new Rational(3, 4),1,new Rational(17, 8),new Rational(3, 2)) }), pseudoPeriodStart: new Rational(1, 2), pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(5, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,new Rational(3, 2),2,new Rational(3, 2)), new Point(new Rational(3, 2),new Rational(25, 4)), new Segment(new Rational(3, 2),new Rational(5, 2),new Rational(25, 4),new Rational(1, 2)), new Point(new Rational(5, 2),new Rational(27, 4)), new Segment(new Rational(5, 2),new Rational(11, 2),new Rational(27, 4),2), new Point(new Rational(11, 2),new Rational(51, 4)), new Segment(new Rational(11, 2),new Rational(13, 2),new Rational(51, 4),2) }), pseudoPeriodStart: new Rational(11, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(1, 4),0,0), new Point(new Rational(1, 4),1), new Segment(new Rational(1, 4),new Rational(1, 2),1,1) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(5, 4))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,1,2,2), new Point(1,4), new Segment(1,2,4,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,2,1,new Rational(1, 3)), new Point(2,new Rational(5, 3)), new Segment(2,new Rational(7, 2),new Rational(5, 3),0), new Point(new Rational(7, 2),new Rational(5, 3)), new Segment(new Rational(7, 2),new Rational(9, 2),new Rational(5, 3),new Rational(1, 2)), new Point(new Rational(9, 2),new Rational(13, 6)), new Segment(new Rational(9, 2),new Rational(19, 4),new Rational(13, 6),2), new Point(new Rational(19, 4),new Rational(8, 3)), new Segment(new Rational(19, 4),5,new Rational(8, 3),0) }), pseudoPeriodStart: new Rational(9, 2), pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: new Rational(1, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),1,2), new Point(new Rational(3, 2),6), new Segment(new Rational(3, 2),3,6,2), new Point(3,9), new Segment(3,4,9,0), new Point(4,11), new Segment(4,5,11,0), new Point(5,11), new Segment(5,6,11,new Rational(1, 2)) }), pseudoPeriodStart: 4, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(1, 2)),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,2,2,new Rational(1, 2)), new Point(2,4), new Segment(2,new Rational(5, 2),4,1), new Point(new Rational(5, 2),new Rational(9, 2)), new Segment(new Rational(5, 2),3,new Rational(9, 2),0) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: new Rational(3, 2))
        ),
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,new Rational(1, 2),1,1), new Point(new Rational(1, 2),new Rational(5, 2)), new Segment(new Rational(1, 2),2,new Rational(5, 2),new Rational(3, 2)), new Point(2,new Rational(19, 4)), new Segment(2,new Rational(5, 2),new Rational(19, 4),0), new Point(new Rational(5, 2),new Rational(23, 4)), new Segment(new Rational(5, 2),3,new Rational(23, 4),0) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,3,new Rational(1, 3)), new Point(1,new Rational(16, 3)), new Segment(1,3,new Rational(19, 3),0), new Point(3,new Rational(25, 3)), new Segment(3,4,new Rational(25, 3),1), new Point(4,new Rational(28, 3)), new Segment(4,new Rational(21, 4),new Rational(31, 3),new Rational(1, 3)), new Point(new Rational(21, 4),new Rational(47, 4)), new Segment(new Rational(21, 4),new Rational(13, 2),new Rational(47, 4),0) }), pseudoPeriodStart: 4, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(29, 12))
        )
    ];

    public static IEnumerable<object[]> SelectedPairTestCases()
        => Pairs.Concat(SelectedPairs).Select(p => new object[] { p.f, p.g });

    /// <summary>
    /// [ZNS26] Theorem 31: for left-continuous operands, $(f^\wedge_p \otimes g^\wedge_p)^{-1}_{\uparrow, [T_f + T_g, +\infty[} = f^{-1}_{\uparrow, p} \overline{\otimes} g^{-1}_{\uparrow, p}$.
    /// </summary>
    [Theory]
    [MemberData(nameof(SelectedPairTestCases))]
    public void UpperPseudoInverseOfTheConvolutionOfPeriodicParts(Curve f0, Curve g0)
    {
        var (f, g) = (f0.ToLeftContinuous(), g0.ToLeftContinuous());
        var (T_f, T_g) = (f.PseudoPeriodStart, g.PseudoPeriodStart);

        var convolution = Curve.Convolution(f.Decompose(T_f).right, g.Decompose(T_g).right, WithoutIsomorphism);
        var left = convolution.UpperPseudoInverseOverInterval(T_f + T_g);
        var right = Curve.MaxPlusConvolution(
            f.UpperPseudoInverseOverInterval(T_f), g.UpperPseudoInverseOverInterval(T_g), WithoutIsomorphism);
        Assert.True(Curve.Equivalent(left, right));
    }

    /// <summary>
    /// [ZNS26] Theorem 32: for right-continuous operands, $(f^\vee_p \otimes g^\vee_p)^{-1}_{\downarrow, [T_f + T_g, +\infty[} = f^{-1}_{\downarrow, p} \otimes g^{-1}_{\downarrow, p}$,
    /// with the lower pseudo-inverses over an interval of Definition 19, which are $+\infty$ below $f(a)$, also for $a = 0$.
    /// </summary>
    [Theory]
    [MemberData(nameof(SelectedPairTestCases))]
    public void LowerPseudoInverseOfTheConvolutionOfPeriodicParts(Curve f0, Curve g0)
    {
        var (f, g) = (f0.ToRightContinuous(), g0.ToRightContinuous());
        var (T_f, T_g) = (f.PseudoPeriodStart, g.PseudoPeriodStart);

        var convolution = Curve.MaxPlusConvolution(
            f.Decompose(T_f, minDecomposition: false).right, g.Decompose(T_g, minDecomposition: false).right, WithoutIsomorphism);
        var left = convolution.LowerPseudoInverseOverInterval(T_f + T_g);
        var right = Curve.Convolution(
            f.LowerPseudoInverseOverInterval(T_f), g.LowerPseudoInverseOverInterval(T_g), WithoutIsomorphism);
        Assert.True(Curve.Equivalent(left, right));
    }

    /// <summary>
    /// The (min,+) convolution of the periodic parts $f^\wedge_p$ and $g^\wedge_p$, and the (max,+) one of $f^\vee_p$ and $g^\vee_p$,
    /// are the same under the default settings, which use the isomorphism, and under those that do not.
    /// </summary>
    [Theory]
    [MemberData(nameof(SelectedPairTestCases))]
    public void ConvolutionsOfPeriodicPartsAreTheSameWithTheIsomorphism(Curve f0, Curve g0)
    {
        var (f, g) = (f0.ToRightContinuous(), g0.ToRightContinuous());
        var (T_f, T_g) = (f.PseudoPeriodStart, g.PseudoPeriodStart);

        var (f_min, g_min) = (f.Decompose(T_f).right, g.Decompose(T_g).right);
        Assert.True(Curve.Equivalent(
            Curve.Convolution(f_min, g_min, WithoutIsomorphism),
            Curve.Convolution(f_min, g_min)));

        var (f_max, g_max) = (f.Decompose(T_f, minDecomposition: false).right, g.Decompose(T_g, minDecomposition: false).right);
        Assert.True(Curve.Equivalent(
            Curve.MaxPlusConvolution(f_max, g_max, WithoutIsomorphism),
            Curve.MaxPlusConvolution(f_max, g_max)));
    }
}
