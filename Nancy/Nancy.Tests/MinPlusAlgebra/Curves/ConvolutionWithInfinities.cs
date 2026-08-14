using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Tests of the (min,+) and (max,+) convolutions over operands that take infinite values.
/// </summary>
/// <remarks>
/// All operands here are <em>plain</em> in the sense of [BT08] Definition 1, so they are inside the class the algorithms are designed for.
/// [BT08] p. 7 admits values in $\overline{\mathbb{R}} = \mathbb{R} \cup \{-\infty, +\infty\}$ and leaves the combinations reaching $(+\infty) + (-\infty)$ undefined.
/// [DNC18] Proposition 2.1 instead makes them total, since the zero element of the (min,+) dioid is $+\infty$ and absorbs, so $(+\infty) + (-\infty) = +\infty$ there, and dually $-\infty$ absorbs in (max,+).
/// This library adopts neither convention, so it follows [BT08] and reports the undefined combinations rather than deciding them.
/// </remarks>
public class ConvolutionWithInfinities
{
    /// <summary>
    /// 0 up to 1, then $+\infty$: plain, per [BT08] Definition 1.
    /// The periodic part is $+\infty$ throughout, so the height is 0 as in Curve.PlusInfinite(), and the $+\infty$ slope comes from the infinite-periodic-sequence special case of PseudoPeriodSlope.
    /// </summary>
    private static Curve PlainPlus =>
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                Point.PlusInfinite(1),
                Segment.PlusInfinite(1, 2)
            ]),
            pseudoPeriodStart: 1,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );

    /// <summary>
    /// 0 up to 1, then $-\infty$: plain, per [BT08] Definition 1.
    /// The periodic part is $-\infty$ throughout, so the height is 0 as in Curve.MinusInfinite(), and the $-\infty$ slope comes from the infinite-periodic-sequence special case of PseudoPeriodSlope.
    /// </summary>
    private static Curve PlainMinus =>
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                Point.MinusInfinite(1),
                Segment.MinusInfinite(1, 2)
            ]),
            pseudoPeriodStart: 1,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );

    /// <summary>
    /// <see cref="PlainMinus"/> as a sub-additive curve, which it is, so that the optimizations of [ZS23] apply.
    /// </summary>
    private static SubAdditiveCurve SubAdditiveMinus =>
        new SubAdditiveCurve(
            PlainMinus.BaseSequence,
            PlainMinus.PseudoPeriodStart,
            PlainMinus.PseudoPeriodLength,
            PlainMinus.PseudoPeriodHeight
        );

    /// <summary>
    /// A zero delay as a sub-additive curve, which it is, so that the optimizations of [ZS23] apply.
    /// </summary>
    private static SubAdditiveCurve SubAdditivePlus =>
        new SubAdditiveCurve(
            new DelayServiceCurve(0).BaseSequence,
            new DelayServiceCurve(0).PseudoPeriodStart,
            new DelayServiceCurve(0).PseudoPeriodLength,
            new DelayServiceCurve(0).PseudoPeriodHeight
        );

    private static readonly Rational Before = new(1, 2);

    private static readonly Rational After = new(3, 2);

    public static List<Curve> Operands = [PlainPlus, PlainMinus, new DelayServiceCurve(1)];

    public static IEnumerable<object[]> OperandsTestCases()
        => Operands.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(OperandsTestCases))]
    public void Operands_ArePlain(Curve operand)
    {
        Assert.True(operand.IsPlain);
    }

    public static List<(Curve f, Curve g)> OppositeInfinities =
    [
        // the pairs that put the two infinities together reach $(+\infty) + (-\infty)$,
        // which has no value without an absorbing convention
        (f: PlainPlus, g: PlainMinus),
        (f: PlainMinus, g: PlainPlus),
        (f: Curve.PlusInfinite(), g: Curve.MinusInfinite()),
        (f: Curve.MinusInfinite(), g: Curve.PlusInfinite()),
        // a zero delay is the unit element, but its $+\infty$ still pairs with the $-\infty$ of the other operand
        (f: new DelayServiceCurve(0), g: PlainMinus),
        (f: PlainMinus, g: new DelayServiceCurve(0)),
        (f: new DelayServiceCurve(0), g: Curve.MinusInfinite()),
        (f: Curve.MinusInfinite(), g: new DelayServiceCurve(0)),
        // the same, for the optimizations that apply to sub-additive operands
        (f: SubAdditivePlus, g: SubAdditiveMinus),
        (f: SubAdditiveMinus, g: SubAdditivePlus)
    ];

    public static IEnumerable<object[]> OppositeInfinitiesTestCases()
        => OppositeInfinities.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(OppositeInfinitiesTestCases))]
    public void ConvolutionsOfOppositeInfinities_AreUndefined(Curve f, Curve g)
    {
        Assert.Throws<UndeterminedResultException>(() => Curve.Convolution(f, g));
        Assert.Throws<UndeterminedResultException>(() => Curve.MaxPlusConvolution(f, g));
    }

    public static List<(Curve f, Curve g, Rational time, Rational expected)> ConvolutionValues =
    [
        // $(f \otimes 0)(t) = \inf_{0 \le s \le t} f(s)$, which is $-\infty$ from 1 on
        (f: PlainMinus, g: Curve.Zero(), time: Before, expected: 0),
        (f: PlainMinus, g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        // $+\infty$ never wins an infimum, so discarding it is sound here
        (f: PlainPlus, g: Curve.Zero(), time: After, expected: 0),
        // only $-\infty$ occurs, and $(-\infty) + (-\infty)$ is defined
        (f: PlainMinus, g: PlainMinus, time: After, expected: Rational.MinusInfinity),
        // the opposite of a zero delay is $-\infty$ off the origin,
        // so for every $t > 0$ the pair $(0, t)$ makes the infimum $-\infty$
        (f: Curve.Zero(), g: -new DelayServiceCurve(0), time: 0, expected: 0),
        (f: Curve.Zero(), g: -new DelayServiceCurve(0), time: Before, expected: Rational.MinusInfinity),
        // an operand that is infinite everywhere decides the result, since every pair reaches it
        (f: Curve.PlusInfinite(), g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        (f: Curve.MinusInfinite(), g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        (f: Curve.PlusInfinite(), g: Curve.PlusInfinite(), time: After, expected: Rational.PlusInfinity),
        (f: Curve.MinusInfinite(), g: Curve.MinusInfinite(), time: After, expected: Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> ConvolutionValuesTestCases()
        => ConvolutionValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ConvolutionValuesTestCases))]
    public void Convolution_Values(Curve f, Curve g, Rational time, Rational expected)
    {
        Assert.Equal(expected, Curve.Convolution(f, g).ValueAt(time));
    }

    public static List<(Curve f, Curve g, Rational time, Rational expected)> MaxPlusConvolutionValues =
    [
        // $(f \overline{\otimes} 0)(t) = \sup_{0 \le s \le t} f(s)$, which is $+\infty$ from 1 on
        (f: PlainPlus, g: Curve.Zero(), time: Before, expected: 0),
        (f: PlainPlus, g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        // $-\infty$ never wins a supremum, so discarding it is sound here
        (f: PlainMinus, g: Curve.Zero(), time: After, expected: 0),
        // only $+\infty$ occurs, and $(+\infty) + (+\infty)$ is defined
        (f: PlainPlus, g: PlainPlus, time: After, expected: Rational.PlusInfinity),
        // a zero delay is $+\infty$ off the origin,
        // so for every $t > 0$ the pair $(0, t)$ makes the supremum $+\infty$
        (f: Curve.Zero(), g: new DelayServiceCurve(0), time: 0, expected: 0),
        (f: Curve.Zero(), g: new DelayServiceCurve(0), time: Before, expected: Rational.PlusInfinity),
        // an operand that is infinite everywhere decides the result, since every pair reaches it
        (f: Curve.PlusInfinite(), g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        (f: Curve.MinusInfinite(), g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        (f: Curve.PlusInfinite(), g: Curve.PlusInfinite(), time: After, expected: Rational.PlusInfinity),
        (f: Curve.MinusInfinite(), g: Curve.MinusInfinite(), time: After, expected: Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> MaxPlusConvolutionValuesTestCases()
        => MaxPlusConvolutionValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionValuesTestCases))]
    public void MaxPlusConvolution_Values(Curve f, Curve g, Rational time, Rational expected)
    {
        Assert.Equal(expected, Curve.MaxPlusConvolution(f, g).ValueAt(time));
    }

    public static List<(Curve f, Curve g, Curve expected)> ConvolutionEquivalences =
    [
        // a zero delay is the neutral element of the (min,+) convolution, see [DNC18] Definition 3.1
        (f: new RateLatencyServiceCurve(2, 1), g: new DelayServiceCurve(0), expected: new RateLatencyServiceCurve(2, 1))
    ];

    public static IEnumerable<object[]> ConvolutionEquivalencesTestCases()
        => ConvolutionEquivalences.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ConvolutionEquivalencesTestCases))]
    public void Convolution_Equivalences(Curve f, Curve g, Curve expected)
    {
        Assert.True(Curve.Equivalent(expected, Curve.Convolution(f, g)));
    }

    public static List<(Curve f, Curve g, Curve expected)> MaxPlusConvolutionEquivalences =
    [
        // a delay service curve is non-decreasing, so its running supremum is the curve itself
        (f: new DelayServiceCurve(1), g: Curve.Zero(), expected: new DelayServiceCurve(1))
    ];

    public static IEnumerable<object[]> MaxPlusConvolutionEquivalencesTestCases()
        => MaxPlusConvolutionEquivalences.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MaxPlusConvolutionEquivalencesTestCases))]
    public void MaxPlusConvolution_Equivalences(Curve f, Curve g, Curve expected)
    {
        Assert.True(Curve.Equivalent(expected, Curve.MaxPlusConvolution(f, g)));
    }

    [Fact]
    public void ElementLevelConvolutions_PropagateInfinities()
    {
        // the element-level operations propagate the infinity that wins, whichever side it is on
        var maxPlus = Element.MaxPlusConvolution(Segment.PlusInfinite(1, 2), Segment.Constant(0, 2, 0));
        var minPlus = Element.Convolution(Segment.MinusInfinite(1, 2), Segment.Constant(0, 2, 0));

        Assert.All(maxPlus, element => Assert.True(element.IsPlusInfinite));
        Assert.All(minPlus, element => Assert.True(element.IsMinusInfinite));
    }
}
