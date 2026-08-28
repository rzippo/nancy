using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Tests of the (min,+) and (max,+) deconvolutions over operands that take infinite values.
/// </summary>
/// <remarks>
/// The operands here are <em>plain</em> in the sense of [BT08] Definition 1, so they are inside the class the algorithms are designed for, except for <see cref="InteriorPlus"/> where noted.
/// [BT08] p. 7 admits values in $\overline{\mathbb{R}} = \mathbb{R} \cup \{-\infty, +\infty\}$ and leaves the combinations reaching $(\pm\infty) - (\pm\infty)$ undefined, which this library reports rather than deciding them through the conventions of [DNC18] Proposition 2.1.
/// The (max,+) deconvolution is computed as $-((-f) \oslash (-g))$, see [DNC18] Section 2.4, so each case below is mirrored to cover that path as well.
/// </remarks>
public class DeconvolutionWithInfinities
{
    /// <summary>
    /// 0 up to 1, then $+\infty$: plain, per [BT08] Definition 1.
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
    /// 0, then $+\infty$ over $[1, 2[$, then 0 again: the infinity is interior, so this is <em>not</em> plain, per [BT08] Definition 1.
    /// The deconvolutions are still defined for it.
    /// </summary>
    private static Curve InteriorPlus =>
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                Point.PlusInfinite(1),
                Segment.PlusInfinite(1, 2),
                Point.Zero(2),
                Segment.Zero(2, 3)
            ]),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );

    private static readonly Rational Before = new(1, 2);

    private static readonly Rational After = new(3, 2);

    public static List<(Curve f, Curve g)> SameInfinities =
    [
        // the terms past the later infinity reach $(+\infty) - (+\infty)$, or dually $(-\infty) - (-\infty)$,
        // which has no value without an absorbing convention
        (f: PlainPlus, g: PlainPlus),
        (f: PlainMinus, g: PlainMinus),
        (f: Curve.PlusInfinite(), g: Curve.PlusInfinite()),
        (f: Curve.MinusInfinite(), g: Curve.MinusInfinite())
    ];

    public static IEnumerable<object[]> SameInfinitiesTestCases()
        => SameInfinities.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SameInfinitiesTestCases))]
    public void DeconvolutionsOfTheSameInfinity_AreUndefined(Curve f, Curve g)
    {
        Assert.Throws<UndeterminedResultException>(() => Curve.Deconvolution(f, g));
        Assert.Throws<UndeterminedResultException>(() => Curve.MaxPlusDeconvolution(f, g));
    }

    public static List<(Curve f, Curve g, Rational time, Rational expected)> DeconvolutionValues =
    [
        // $(f \oslash 0)(t) = \sup_{u \ge 0} f(t + u)$, which is $-\infty$ once $t$ is past 1
        (f: PlainMinus, g: Curve.Zero(), time: Before, expected: 0),
        (f: PlainMinus, g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        // the same supremum reaches $+\infty$ at every $t$, since the tail of $f$ is always in it
        (f: PlainPlus, g: Curve.Zero(), time: Before, expected: Rational.PlusInfinity),
        (f: PlainPlus, g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        // the same, for a representation whose pseudo-period starts well past the first $-\infty$
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 0),
                    Point.MinusInfinite(1),
                    Segment.MinusInfinite(1, 2),
                    Point.MinusInfinite(2),
                    Segment.MinusInfinite(2, 3),
                    Point.MinusInfinite(3),
                    Segment.MinusInfinite(3, 4)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            g: Curve.Zero(),
            time: Before,
            expected: 0
        ),
        (
            f: new Curve(
                baseSequence: new Sequence([
                    Point.Origin(),
                    Segment.Constant(0, 1, 0),
                    Point.MinusInfinite(1),
                    Segment.MinusInfinite(1, 2),
                    Point.MinusInfinite(2),
                    Segment.MinusInfinite(2, 3),
                    Point.MinusInfinite(3),
                    Segment.MinusInfinite(3, 4)
                ]),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            ),
            g: Curve.Zero(),
            time: After,
            expected: Rational.MinusInfinity
        ),
        // a $g$ reaching $-\infty$ contributes $+\infty$ to the supremum
        (f: Curve.Zero(), g: PlainMinus, time: Before, expected: Rational.PlusInfinity),
        (f: Curve.Zero(), g: PlainMinus, time: After, expected: Rational.PlusInfinity),
        // a $g$ reaching $+\infty$ only makes those terms $-\infty$, so the head decides
        (f: Curve.Zero(), g: PlainPlus, time: Before, expected: 0),
        // opposite infinities are defined here: $(-\infty) - (+\infty) = -\infty$
        (f: PlainMinus, g: PlainPlus, time: Before, expected: 0),
        (f: PlainMinus, g: PlainPlus, time: After, expected: Rational.MinusInfinity),
        // an operand that is infinite everywhere decides the result
        (f: Curve.MinusInfinite(), g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        (f: Curve.Zero(), g: Curve.PlusInfinite(), time: After, expected: Rational.MinusInfinity),
        // an $f$ with an interior $-\infty$:
        // the $g$ makes the terms past 1 lose, so the result is the supremum of $f$ over $[t, t + 1[$,
        // which no pair covers at $t = 1$
        (f: -InteriorPlus, g: PlainPlus, time: 0, expected: 0),
        (f: -InteriorPlus, g: PlainPlus, time: 1, expected: Rational.MinusInfinity),
        (f: -InteriorPlus, g: PlainPlus, time: After, expected: 0)
    ];

    public static IEnumerable<object[]> DeconvolutionValuesTestCases()
        => DeconvolutionValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DeconvolutionValuesTestCases))]
    public void Deconvolution_Values(Curve f, Curve g, Rational time, Rational expected)
    {
        Assert.Equal(expected, Curve.Deconvolution(f, g).ValueAt(time));
    }

    public static List<(Curve f, Curve g, Rational time, Rational expected)> MaxPlusDeconvolutionValues =
    [
        // $(f \overline{\oslash} 0)(t) = \inf_{u \ge 0} f(t + u)$, which is $+\infty$ once $t$ is past 1
        (f: PlainPlus, g: Curve.Zero(), time: Before, expected: 0),
        (f: PlainPlus, g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        // the same infimum reaches $-\infty$ at every $t$, since the tail of $f$ is always in it
        (f: PlainMinus, g: Curve.Zero(), time: Before, expected: Rational.MinusInfinity),
        (f: PlainMinus, g: Curve.Zero(), time: After, expected: Rational.MinusInfinity),
        // a $g$ reaching $+\infty$ contributes $-\infty$ to the infimum
        (f: Curve.Zero(), g: PlainPlus, time: Before, expected: Rational.MinusInfinity),
        (f: Curve.Zero(), g: PlainPlus, time: After, expected: Rational.MinusInfinity),
        // a $g$ reaching $-\infty$ only makes those terms $+\infty$, so the head decides
        (f: Curve.Zero(), g: PlainMinus, time: Before, expected: 0),
        // opposite infinities are defined here: $(+\infty) - (-\infty) = +\infty$
        (f: PlainPlus, g: PlainMinus, time: Before, expected: 0),
        (f: PlainPlus, g: PlainMinus, time: After, expected: Rational.PlusInfinity),
        // an operand that is infinite everywhere decides the result
        (f: Curve.PlusInfinite(), g: Curve.Zero(), time: After, expected: Rational.PlusInfinity),
        (f: Curve.Zero(), g: Curve.MinusInfinite(), time: After, expected: Rational.PlusInfinity),
        // dual of the interior case above: the infimum of $f$ over $[t, t + 1[$
        (f: InteriorPlus, g: PlainMinus, time: 0, expected: 0),
        (f: InteriorPlus, g: PlainMinus, time: 1, expected: Rational.PlusInfinity),
        (f: InteriorPlus, g: PlainMinus, time: After, expected: 0)
    ];

    public static IEnumerable<object[]> MaxPlusDeconvolutionValuesTestCases()
        => MaxPlusDeconvolutionValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MaxPlusDeconvolutionValuesTestCases))]
    public void MaxPlusDeconvolution_Values(Curve f, Curve g, Rational time, Rational expected)
    {
        Assert.Equal(expected, Curve.MaxPlusDeconvolution(f, g).ValueAt(time));
    }

    [Fact]
    public void ElementLevelDeconvolution_PropagatesInfinities()
    {
        // an infinite operand makes the difference constant, whichever side it is on
        var infiniteA = Segment.PlusInfinite(1, 2).Deconvolution(Segment.Constant(0, 2, 0));
        var infiniteB = Segment.Constant(1, 2, 0).Deconvolution(Segment.PlusInfinite(0, 2));
        var infinitePoint = new Point(1, Rational.PlusInfinity).Deconvolution(Segment.Constant(0, 2, 0));

        Assert.All(infiniteA, element => Assert.True(element.IsPlusInfinite));
        Assert.All(infiniteB, element => Assert.True(element.IsMinusInfinite));
        Assert.True(infinitePoint.IsPlusInfinite);
    }

    [Fact]
    public void ElementLevelDeconvolution_OfTheSameInfinity_IsUndefined()
    {
        // the difference reaches $(+\infty) - (+\infty)$
        Assert.Throws<UndeterminedResultException>(
            () => Segment.PlusInfinite(1, 2).Deconvolution(Segment.PlusInfinite(0, 2)).ToList());
    }
}
