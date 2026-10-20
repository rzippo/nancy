using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks each algorithm of the horizontal deviation implemented alone, with no correction, over every combination of kinds of curves,
/// and that it fails exactly where the mathematics says it does;
/// then that <see cref="Curve.HorizontalDeviation"/> gives the expected value under every <see cref="HorizontalDeviationStrategy"/>.
/// </summary>
public class HorizontalDeviationStrategies
{
    private static readonly Rational Inf = Rational.PlusInfinity;

    private static Curve C(Element[] elements, Rational T, Rational d, Rational c) => new(new Sequence(elements), T, d, c);

    private static readonly Dictionary<string, Curve> Fs = new()
    {
        ["unbounded, f(0) = 0"] = new RateLatencyServiceCurve(2, 1),
        ["unbounded, f(0) > 0"] = C([new Point(0, 2), new Segment(0, 1, 2, 1)], 0, 1, 1),
        ["staircase"] = new StairCurve(1, 2),
        ["bounded, f(0) = 0"] = Curve.Minimum(new RateLatencyServiceCurve(5, 0), new ConstantCurve(5)),
        ["bounded, f(0) > 0"] = C([new Point(0, 1), new Segment(0, 1, 1, 4), new Point(1, 5), Segment.Constant(1, 2, 5)], 1, 1, 0),
        ["zero"] = Curve.Zero(),
        ["0 up to 1, then t - 1"] = new RateLatencyServiceCurve(1, 1),
        ["0 up to 3, then 1"] = C([Point.Origin(), Segment.Zero(0, 3), new Point(3, 0), Segment.Constant(3, 4, 1), new Point(4, 1), Segment.Constant(4, 5, 1)], 4, 1, 0),
        ["+inf after 2, f(0) = 0"] = C([Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)], 3, 1, 0),
        ["+inf after 2, f(0) > 0"] = C([new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)], 3, 1, 0),
    };

    private static readonly Dictionary<string, Curve> Gs = new()
    {
        ["unbounded"] = new RateLatencyServiceCurve(1, 2),
        ["bounded at 10"] = Curve.Minimum(new RateLatencyServiceCurve(5, 2), new ConstantCurve(10)),
        ["bounded at 5"] = Curve.Minimum(new RateLatencyServiceCurve(5, 2), new ConstantCurve(5)),
        ["bounded at 3"] = Curve.Minimum(new RateLatencyServiceCurve(5, 2), new ConstantCurve(3)),
        ["bounded at 10, faster"] = Curve.Minimum(new RateLatencyServiceCurve(10, 0), new ConstantCurve(10)),
        ["+inf after 3"] = new DelayServiceCurve(3),
        ["negative at first"] = new RateLatencyServiceCurve(1, 0).VerticalShift(-1),
        // -1 up to 2, then t - 1: across 0 by a jump, at which it is -1 or 1; their lower pseudo-inverses are the same, their upper ones are not
        ["over 0 by a jump, -1 at it"] = C([new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, -1), new Segment(2, 3, 1, 1), new Point(3, 2), new Segment(3, 4, 2, 1)], 3, 1, 1),
        ["over 0 by a jump, 1 at it"] = C([new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, 1), new Segment(2, 3, 1, 1), new Point(3, 2), new Segment(3, 4, 2, 1)], 3, 1, 1),
    };

    /// <summary>
    /// The deviation of each pair, computed by hand as the supremum of $g^{-1}_\downarrow(y) - f^{-1}_\downarrow(y)$ over the heights of $f$.
    /// </summary>
    public static List<(string f, string g, Rational expected)> Grid =
    [
        ("unbounded, f(0) = 0", "unbounded", Inf), ("unbounded, f(0) = 0", "bounded at 10", Inf), ("unbounded, f(0) = 0", "bounded at 5", Inf),
        ("unbounded, f(0) = 0", "bounded at 3", Inf), ("unbounded, f(0) = 0", "+inf after 3", 2), ("unbounded, f(0) = 0", "negative at first", Inf),
        ("unbounded, f(0) = 0", "bounded at 10, faster", Inf), ("unbounded, f(0) = 0", "over 0 by a jump, -1 at it", Inf), ("unbounded, f(0) = 0", "over 0 by a jump, 1 at it", Inf),
        ("unbounded, f(0) > 0", "unbounded", 4), ("unbounded, f(0) > 0", "bounded at 10", Inf), ("unbounded, f(0) > 0", "bounded at 5", Inf),
        ("unbounded, f(0) > 0", "bounded at 3", Inf), ("unbounded, f(0) > 0", "+inf after 3", 3), ("unbounded, f(0) > 0", "negative at first", 3),
        ("unbounded, f(0) > 0", "bounded at 10, faster", Inf), ("unbounded, f(0) > 0", "over 0 by a jump, -1 at it", 3), ("unbounded, f(0) > 0", "over 0 by a jump, 1 at it", 3),
        ("staircase", "unbounded", 3), ("staircase", "bounded at 10", Inf), ("staircase", "bounded at 5", Inf),
        ("staircase", "bounded at 3", Inf), ("staircase", "+inf after 3", 3), ("staircase", "negative at first", 2),
        ("staircase", "bounded at 10, faster", Inf), ("staircase", "over 0 by a jump, -1 at it", 2), ("staircase", "over 0 by a jump, 1 at it", 2),
        ("bounded, f(0) = 0", "unbounded", 6), ("bounded, f(0) = 0", "bounded at 10", 2), ("bounded, f(0) = 0", "bounded at 5", 2),
        ("bounded, f(0) = 0", "bounded at 3", Inf), ("bounded, f(0) = 0", "+inf after 3", 3), ("bounded, f(0) = 0", "negative at first", 5),
        ("bounded, f(0) = 0", "bounded at 10, faster", 0), ("bounded, f(0) = 0", "over 0 by a jump, -1 at it", 5), ("bounded, f(0) = 0", "over 0 by a jump, 1 at it", 5),
        ("bounded, f(0) > 0", "unbounded", 6), ("bounded, f(0) > 0", "bounded at 10", new Rational(11, 5)), ("bounded, f(0) > 0", "bounded at 5", new Rational(11, 5)),
        ("bounded, f(0) > 0", "bounded at 3", Inf), ("bounded, f(0) > 0", "+inf after 3", 3), ("bounded, f(0) > 0", "negative at first", 5),
        ("bounded, f(0) > 0", "bounded at 10, faster", new Rational(1, 10)), ("bounded, f(0) > 0", "over 0 by a jump, -1 at it", 5), ("bounded, f(0) > 0", "over 0 by a jump, 1 at it", 5),
        ("zero", "unbounded", 0), ("zero", "bounded at 10", 0), ("zero", "bounded at 5", 0),
        ("zero", "bounded at 3", 0), ("zero", "+inf after 3", 0), ("zero", "negative at first", 1),
        ("zero", "bounded at 10, faster", 0), ("zero", "over 0 by a jump, -1 at it", 2), ("zero", "over 0 by a jump, 1 at it", 2),
        ("0 up to 1, then t - 1", "unbounded", 1), ("0 up to 1, then t - 1", "bounded at 10", Inf), ("0 up to 1, then t - 1", "bounded at 5", Inf),
        ("0 up to 1, then t - 1", "bounded at 3", Inf), ("0 up to 1, then t - 1", "+inf after 3", 2), ("0 up to 1, then t - 1", "negative at first", 1),
        ("0 up to 1, then t - 1", "bounded at 10, faster", Inf), ("0 up to 1, then t - 1", "over 0 by a jump, -1 at it", 2), ("0 up to 1, then t - 1", "over 0 by a jump, 1 at it", 2),
        ("0 up to 3, then 1", "unbounded", 0), ("0 up to 3, then 1", "bounded at 10", 0), ("0 up to 3, then 1", "bounded at 5", 0),
        ("0 up to 3, then 1", "bounded at 3", 0), ("0 up to 3, then 1", "+inf after 3", 0), ("0 up to 3, then 1", "negative at first", 1),
        ("0 up to 3, then 1", "bounded at 10, faster", 0), ("0 up to 3, then 1", "over 0 by a jump, -1 at it", 2), ("0 up to 3, then 1", "over 0 by a jump, 1 at it", 2),
        ("+inf after 2, f(0) = 0", "unbounded", Inf), ("+inf after 2, f(0) = 0", "bounded at 10", Inf), ("+inf after 2, f(0) = 0", "bounded at 5", Inf),
        ("+inf after 2, f(0) = 0", "bounded at 3", Inf), ("+inf after 2, f(0) = 0", "+inf after 3", 3), ("+inf after 2, f(0) = 0", "negative at first", Inf),
        ("+inf after 2, f(0) = 0", "bounded at 10, faster", Inf), ("+inf after 2, f(0) = 0", "over 0 by a jump, -1 at it", Inf), ("+inf after 2, f(0) = 0", "over 0 by a jump, 1 at it", Inf),
        ("+inf after 2, f(0) > 0", "unbounded", Inf), ("+inf after 2, f(0) > 0", "bounded at 10", Inf), ("+inf after 2, f(0) > 0", "bounded at 5", Inf),
        ("+inf after 2, f(0) > 0", "bounded at 3", Inf), ("+inf after 2, f(0) > 0", "+inf after 3", 3), ("+inf after 2, f(0) > 0", "negative at first", Inf),
        ("+inf after 2, f(0) > 0", "bounded at 10, faster", Inf), ("+inf after 2, f(0) > 0", "over 0 by a jump, -1 at it", Inf), ("+inf after 2, f(0) > 0", "over 0 by a jump, 1 at it", Inf),
    ];

    public static IEnumerable<object[]> GridData() => Grid.Select(c => new object[] { c.f, c.g, c.expected });

    private static readonly Curve Identity = new RateLatencyServiceCurve(1, 0);

    // the algorithms alone, with no correction

    internal static Rational LowerGapAlone(Curve f, Curve g)
        => Curve.Subtraction(g.LowerPseudoInverse(), f.LowerPseudoInverse()).SupValue();

    private static Rational CompositionAlone(Curve f, Curve g)
        => g.LowerPseudoInverse().Composition(f).Subtraction(Identity).SupValue();

    private static Rational CompositionDeconvolutionAlone(Curve f, Curve g)
        => g.LowerPseudoInverse().Composition(f).Deconvolution(Identity).ValueAt(0);

    internal static Rational UpperDeconvolutionAlone(Curve f, Curve g)
        => Curve.MaxPlusDeconvolution(f.UpperPseudoInverse(), g.UpperPseudoInverse()).Negate().ToNonNegative().ValueAt(0);

    private static bool BothBounded(Curve f, Curve g) => f.IsUltimatelyConstant && g.IsUltimatelyConstant;

    // f is 0 on an interval from 0, so that its upper pseudo-inverse at 0 is the end of that interval
    private static bool StaysAtZero(Curve f) => f.UpperPseudoInverse().ValueAt(0) > 0;

    /// <summary>
    /// The lower gap alone is undefined where both curves are bounded: above both tops both lower pseudo-inverses are $+\infty$, and their gap is $+\infty - (+\infty)$.
    /// </summary>
    private static void AssertLowerGapAlone(Curve f, Curve g, Rational expected)
    {
        if (BothBounded(f, g))
            Assert.Throws<UndeterminedResultException>(() => LowerGapAlone(f, g));
        else
            Assert.Equal(expected, LowerGapAlone(f, g));
    }

    /// <summary>
    /// The compositions alone are undefined where $f$ takes $+\infty$: the lower pseudo-inverse of $g$ cannot be sampled at $+\infty$, and the composition rejects it.
    /// </summary>
    private static void AssertCompositionsAlone(Curve f, Curve g, Rational expected)
    {
        if (!f.IsFinite)
        {
            Assert.Throws<ArgumentException>(() => CompositionAlone(f, g));
            Assert.Throws<ArgumentException>(() => CompositionDeconvolutionAlone(f, g));
        }
        else
        {
            Assert.Equal(expected, CompositionAlone(f, g));
            Assert.Equal(expected, CompositionDeconvolutionAlone(f, g));
        }
    }

    /// <summary>
    /// Checks the uncorrected upper-pseudo-inverse deconvolution against the expected horizontal deviation.
    /// For bounded operands it should throw; otherwise it should return $+\infty$ when $f(0) > 0$, or zero when $f = 0$.
    /// When $f$ stays at zero after the origin and $g(0) < 0$, its result should not exceed the expected deviation.
    /// In the remaining cases it should equal the expected deviation.
    /// </summary>
    private static void AssertUpperDeconvolutionAlone(Curve f, Curve g, Rational expected)
    {
        if (BothBounded(f, g))
            Assert.Throws<UndeterminedResultException>(() => UpperDeconvolutionAlone(f, g));
        else if (f.ValueAt(0) > 0)
            Assert.Equal(Inf, UpperDeconvolutionAlone(f, g));
        else if (f.IsZero)
            Assert.Equal(0, UpperDeconvolutionAlone(f, g));
        else if (StaysAtZero(f) && g.ValueAt(0) < 0)
            Assert.True(UpperDeconvolutionAlone(f, g) <= expected);
        else
            Assert.Equal(expected, UpperDeconvolutionAlone(f, g));
    }

    [Theory]
    [MemberData(nameof(GridData))]
    public void LowerGapAloneFailsOnlyWhereBothAreBounded(string fKind, string gKind, Rational expected)
        => AssertLowerGapAlone(Fs[fKind], Gs[gKind], expected);

    [Theory]
    [MemberData(nameof(GridData))]
    public void CompositionsAloneFailOnlyWhereFTakesPlusInfinity(string fKind, string gKind, Rational expected)
        => AssertCompositionsAlone(Fs[fKind], Gs[gKind], expected);

    [Theory]
    [MemberData(nameof(GridData))]
    public void UpperDeconvolutionAloneFailsOnlyWhereTheMathematicsSays(string fKind, string gKind, Rational expected)
        => AssertUpperDeconvolutionAlone(Fs[fKind], Gs[gKind], expected);

    [Theory]
    [MemberData(nameof(Deviations.AllKnownHDevs), MemberType = typeof(Deviations))]
    public void LowerGapAloneFailsOnlyWhereBothAreBoundedOnTheKnownDeviations(string id, Curve f, Curve g, Rational expected)
    {
        Assert.False(string.IsNullOrEmpty(id));
        AssertLowerGapAlone(f, g, expected);
    }

    [Theory]
    [MemberData(nameof(Deviations.AllKnownHDevs), MemberType = typeof(Deviations))]
    public void CompositionsAloneFailOnlyWhereFTakesPlusInfinityOnTheKnownDeviations(string id, Curve f, Curve g, Rational expected)
    {
        Assert.False(string.IsNullOrEmpty(id));
        AssertCompositionsAlone(f, g, expected);
    }

    [Theory]
    [MemberData(nameof(Deviations.AllKnownHDevs), MemberType = typeof(Deviations))]
    public void UpperDeconvolutionAloneFailsOnlyWhereTheMathematicsSaysOnTheKnownDeviations(string id, Curve f, Curve g, Rational expected)
    {
        Assert.False(string.IsNullOrEmpty(id));
        AssertUpperDeconvolutionAlone(f, g, expected);
    }

    /// <summary>
    /// Provides grid pairs for which the uncorrected upper-pseudo-inverse deconvolution falls short of the horizontal deviation.
    /// Each case includes the value of that deconvolution, computed by hand.
    /// </summary>
    public static IEnumerable<object[]> FallsShortData() =>
    [
        ["0 up to 1, then t - 1", "negative at first", new Rational(0)],
        ["0 up to 1, then t - 1", "over 0 by a jump, -1 at it", new Rational(1)],
        ["0 up to 1, then t - 1", "over 0 by a jump, 1 at it", new Rational(1)],
        ["0 up to 3, then 1", "negative at first", new Rational(0)],
        ["0 up to 3, then 1", "over 0 by a jump, -1 at it", new Rational(0)],
        ["0 up to 3, then 1", "over 0 by a jump, 1 at it", new Rational(0)],
    ];

    [Theory]
    [MemberData(nameof(FallsShortData))]
    public void UpperDeconvolutionAloneFallsShortWhereFStaysAtZeroAndGStartsNegative(string fKind, string gKind, Rational alone)
    {
        var expected = Grid.Single(c => c.f == fKind && c.g == gKind).expected;
        Assert.True(alone < expected);
        Assert.Equal(alone, UpperDeconvolutionAlone(Fs[fKind], Gs[gKind]));
    }

    public static IEnumerable<object[]> GridUnderEveryStrategy()
        => Grid.SelectMany(c => Enum.GetValues<HorizontalDeviationStrategy>().Select(s => new object[] { c.f, c.g, c.expected, s }));

    /// <summary>
    /// With the corrections of each strategy, the deviation is the expected one on every pair: the strategies differ only in performance.
    /// </summary>
    [Theory]
    [MemberData(nameof(GridUnderEveryStrategy))]
    public void EveryStrategyGivesTheExpectedValue(string fKind, string gKind, Rational expected, HorizontalDeviationStrategy strategy)
    {
        var settings = new ComputationSettings { HorizontalDeviationStrategy = strategy };
        Assert.Equal(expected, Curve.HorizontalDeviation(Fs[fKind], Gs[gKind], settings));
    }
}
