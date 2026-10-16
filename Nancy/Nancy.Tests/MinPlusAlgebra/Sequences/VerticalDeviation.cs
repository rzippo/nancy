using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Tests for the vertical deviation between sequences, [TBP-EB-FRTC] EB-FRTC-SEQ-D1.
/// And for its agreeing with the deviation between the curves the operands are restrictions of, [TBP-EB-FRTC] EB-FRTC-SEQ-V1.
/// </summary>
public class VerticalDeviation
{
    /// <summary>
    /// Pairs of sequences and the value of $vDev$ over them.
    /// The definition subtracts without a positive part, so a sequence below the other gives a negative result, as it does for curves.
    /// </summary>
    public static List<(Sequence a, Sequence b, Rational expected)> KnownVDevs =
    [
        // a above b throughout
        (
            new Sequence([ new Point(0, 4), new Segment(0, 4, 4, 1), new Point(4, 8) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4) ]),
            4
        ),
        // a below b throughout: the deviation is negative
        (
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4) ]),
            new Sequence([ new Point(0, 4), new Segment(0, 4, 4, 1), new Point(4, 8) ]),
            -4
        ),
        // the two cross, so the supremum is attained at the right end
        (
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 2), new Point(4, 8) ]),
            new Sequence([ new Point(0, 3), new Segment(0, 4, 3, 1), new Point(4, 7) ]),
            1
        ),
        // equal operands
        (
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4) ]),
            0
        ),
        // the domains overlap only in part, and only the overlap is measured:
        // over [2, 4] the difference is 2, while over [0, 2[, where b is not defined, it would be larger
        (
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 2), new Point(4, 8) ]),
            new Sequence([ new Point(2, 2), new Segment(2, 4, 2, 2), new Point(4, 6) ]),
            2
        ),
        // the domains overlap in a single point
        (
            new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2) ]),
            new Sequence([ new Point(2, 5), new Segment(2, 4, 5, 1), new Point(4, 7) ]),
            -3
        ),
        // a decreasing, so the supremum is attained at the left end
        (
            new Sequence([ new Point(0, 4), new Segment(0, 4, 4, -1), new Point(4, 0) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 0), new Point(4, 0) ]),
            4
        ),
        // a negative throughout
        (
            new Sequence([ new Point(0, -2), new Segment(0, 4, -2, 0), new Point(4, -2) ]),
            new Sequence([ new Point(0, 0), new Segment(0, 4, 0, 1), new Point(4, 4) ]),
            -2
        ),
        // both negative and decreasing, b faster, so the difference grows towards the right end
        (
            new Sequence([ new Point(0, -1), new Segment(0, 4, -1, -1), new Point(4, -5) ]),
            new Sequence([ new Point(0, -3), new Segment(0, 4, -3, -2), new Point(4, -11) ]),
            6
        ),
    ];

    public static IEnumerable<object[]> KnownVDevsTestCases => KnownVDevs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownVDevsTestCases))]
    public void VerticalDeviationValues(Sequence a, Sequence b, Rational expected)
    {
        Assert.Equal(expected, Sequence.VerticalDeviation(a, b));
    }

    private static Interval I(Rational lower, Rational upper, bool isLowerIncluded, bool isUpperIncluded)
        => new(lower, upper, isLowerIncluded, isUpperIncluded);

    private static readonly bool[] Ends = [true, false];

    /// <summary>
    /// The domains of the two operands, named by the cases of [TBP-SEQ-DEV] § 5, and the shared domain $I$ the result is defined over, or null where $I$ is empty.
    /// The expected $I$ is written out, not computed: its lower end is the larger of the two lower ends, included where every operand whose end it is includes it, and likewise the upper end.
    /// </summary>
    public static IEnumerable<object?[]> SharedDomainCases()
    {
        // V-gap: disjoint, with a gap
        yield return ["V-gap", I(0, 1, true, true), I(2, 3, true, true), null];
        yield return ["V-gap, open", I(0, 1, false, false), I(2, 3, false, false), null];

        // V-touch: the domains meet at 1, which is shared only if both include it
        foreach (var ea in Ends)
        foreach (var sb in Ends)
            yield return [$"V-touch, e_a {ea}, s_b {sb}", I(0, 1, true, ea), I(1, 2, sb, true), ea && sb ? I(1, 1, true, true) : null];

        // V-overlap: X runs from s_b to e_a, with their inclusions
        foreach (var sb in Ends)
        foreach (var ea in Ends)
            yield return [$"V-overlap, s_b {sb}, e_a {ea}", I(0, 2, true, ea), I(1, 3, sb, true), I(1, 2, sb, ea)];
        // ... and the two outer ends do not matter
        yield return ["V-overlap, outer ends open", I(0, 2, false, true), I(1, 3, true, false), I(1, 2, true, true)];

        // V-nested: X is the inner domain
        foreach (var sb in Ends)
        foreach (var eb in Ends)
            yield return [$"V-nested, s_b {sb}, e_b {eb}", I(0, 4, true, true), I(1, 2, sb, eb), I(1, 2, sb, eb)];
        yield return ["V-nested, outer open", I(0, 4, false, false), I(1, 2, true, true), I(1, 2, true, true)];

        // V-shared-start: the common lower end is shared only if both include it
        foreach (var sa in Ends)
        foreach (var sb in Ends)
        foreach (var ea in Ends)
            yield return [$"V-shared-start, s_a {sa}, s_b {sb}, e_a {ea}", I(1, 2, sa, ea), I(1, 3, sb, true), I(1, 2, sa && sb, ea)];

        // V-shared-end: symmetric
        foreach (var sb in Ends)
        foreach (var ea in Ends)
        foreach (var eb in Ends)
            yield return [$"V-shared-end, s_b {sb}, e_a {ea}, e_b {eb}", I(0, 3, true, ea), I(1, 3, sb, eb), I(1, 3, sb, ea && eb)];

        // V-equal: the same bounds, each end shared only if both include it
        foreach (var sa in Ends)
        foreach (var ea in Ends)
        foreach (var sb in Ends)
        foreach (var eb in Ends)
            yield return [$"V-equal, a {sa} {ea}, b {sb} {eb}", I(1, 3, sa, ea), I(1, 3, sb, eb), I(1, 3, sa && sb, ea && eb)];

        // V-point: a single-point operand
        yield return ["V-point, inside", I(2, 2, true, true), I(1, 3, true, true), I(2, 2, true, true)];
        yield return ["V-point, at a closed end", I(2, 2, true, true), I(2, 3, true, true), I(2, 2, true, true)];
        yield return ["V-point, at an open end", I(2, 2, true, true), I(2, 3, false, true), null];
        yield return ["V-point, outside", I(2, 2, true, true), I(3, 4, true, true), null];
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 3: the result is defined over the shared domain $I$, with its ends, and there agrees with the deviation between the curves;
    /// where $I$ is empty the deviation is undefined, and the method throws.
    /// </summary>
    [Theory]
    [MemberData(nameof(SharedDomainCases))]
    public void DifferenceIsDefinedOverTheSharedDomain(string id, Interval xA, Interval xB, Interval? expected)
    {
        var f = new SigmaRhoArrivalCurve(3, 2);
        var g = new RateLatencyServiceCurve(1, 2);
        var a = f.Cut(xA);
        var b = g.Cut(xB);
        Assert.True(a.Support.SetEquals(xA), $"{id}: a is over {a.Support}, not {xA}");
        Assert.True(b.Support.SetEquals(xB), $"{id}: b is over {b.Support}, not {xB}");

        if (expected is not { } x)
        {
            Assert.Throws<ArgumentException>(() => Sequence.Subtraction(a, b));
            Assert.Throws<ArgumentException>(() => Sequence.VerticalDeviation(a, b));
            return;
        }

        var vdev = Sequence.Subtraction(a, b);
        Assert.True(vdev.Support.SetEquals(x), $"{id}: defined over {vdev.Support}, not {x}");
        Assert.True((f - g).Match(vdev), $"{id}: differs from the curves");
    }

    /// <summary>
    /// Curves, and the intervals to restrict them over.
    /// </summary>
    public static List<(Curve f, Curve g, Interval xF, Interval xG)> ValidityCases =
    [
        (new SigmaRhoArrivalCurve(3, 2), new RateLatencyServiceCurve(1, 2), new Interval(0, 10), new Interval(0, 10)),
        (new SigmaRhoArrivalCurve(3, 2), new RateLatencyServiceCurve(1, 2), new Interval(2, 10), new Interval(4, 8)),
        (new RateLatencyServiceCurve(1, 2), new SigmaRhoArrivalCurve(3, 2), new Interval(0, 10), new Interval(3, 7)),
        (new SigmaRhoArrivalCurve(0, 1), new SigmaRhoArrivalCurve(0, 1), new Interval(1, 6), new Interval(3, 9)),
    ];

    public static IEnumerable<object[]> ValidityCasesTestCases => ValidityCases.ToXUnitTestCases();

    /// <summary>
    /// Operands taking the value $+\infty$, named by the cases of [TBP-SEQ-DEV] § 5, with the value of $vDev(a, b, t)$ at some times, and its supremum.
    /// The letter in a name is the operand that takes the value $+\infty$.
    /// </summary>
    public static IEnumerable<object[]> InfiniteValueCases()
    {
        var h2 = new DelayServiceCurve(2).Cut(0, 4, true, true);
        var w = new RateLatencyServiceCurve(1, 0).Cut(0, 4, true, true);
        yield return ["VI-a-infinite", h2, w, new (Rational t, Rational v)[] { (1, -1), (3, Rational.PlusInfinity) }, Rational.PlusInfinity];
        yield return ["VI-b-infinite", w, h2, new (Rational t, Rational v)[] { (1, 1), (3, Rational.MinusInfinity) }, (Rational) 2];
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 3: with infinite values, the deviation subtracts at each time as for finite ones.
    /// </summary>
    [Theory]
    [MemberData(nameof(InfiniteValueCases))]
    public void VerticalDeviationOverInfiniteValues(string id, Sequence a, Sequence b, (Rational t, Rational v)[] values, Rational sup)
    {
        var vdev = Sequence.Subtraction(a, b);
        foreach (var (t, v) in values)
            Assert.True(vdev.ValueAt(t) == v, $"{id}: {vdev.ValueAt(t)} at {t}, not {v}");
        Assert.True(Sequence.VerticalDeviation(a, b) == sup, $"{id}: supremum {Sequence.VerticalDeviation(a, b)}, not {sup}");
    }

    /// <summary>
    /// Operands that are both $+\infty$ at some shared time, named by the cases of [TBP-SEQ-DEV] § 5.
    /// </summary>
    public static IEnumerable<object[]> BothInfiniteCases()
    {
        var h1 = new DelayServiceCurve(1).Cut(0, 4, true, true);
        var h2 = new DelayServiceCurve(2).Cut(0, 4, true, true);
        yield return ["VI-both-infinite", h2, h1];
        yield return ["VI-both-infinite, swapped", h1, h2];
        yield return ["VI-both-infinite, at one time", new Sequence([ new Point(0, 0), new Segment(0, 1, 0, 1), Point.PlusInfinite(1) ]), Sequence.PlusInfinite(1, 2, true, true)];
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 3: where both operands are $+\infty$ at a shared time, their difference is undefined there, and so is the deviation: the methods throw.
    /// </summary>
    [Theory]
    [MemberData(nameof(BothInfiniteCases))]
    public void BothInfiniteAtASharedTimeIsRejected(string _, Sequence a, Sequence b)
    {
        Assert.Throws<UndeterminedResultException>(() => Sequence.Subtraction(a, b));
        Assert.Throws<UndeterminedResultException>(() => Sequence.VerticalDeviation(a, b));
    }

    /// <summary>
    /// $-\infty$ on $[0, 1[$, then $t$ up to 4.
    /// </summary>
    private static readonly Sequence MinusThenT = new([
        Point.MinusInfinite(0), Segment.MinusInfinite(0, 1), new Point(1, 1), new Segment(1, 4, 1, 1), new Point(4, 4)
    ]);

    /// <summary>
    /// Operands taking the value $-\infty$, [TBP-SEQ-DEV] § 3, with the value of $vDev(a, b, t)$ at some times, and its supremum:
    /// where one operand alone is infinite, or the two are infinite with opposite signs, the difference is that of the infinite terms.
    /// </summary>
    public static IEnumerable<object[]> MinusInfiniteValueCases()
    {
        var w = new RateLatencyServiceCurve(1, 0).Cut(0, 4, true, true);
        var h2 = new DelayServiceCurve(2).Cut(0, 4, true, true);
        yield return ["a is -inf where b is finite", MinusThenT, w, new (Rational t, Rational v)[] { (0, Rational.MinusInfinity), (2, 0) }, (Rational) 0];
        yield return ["b is -inf where a is finite", w, MinusThenT, new (Rational t, Rational v)[] { (0, Rational.PlusInfinity), (2, 0) }, Rational.PlusInfinity];
        yield return ["a is -inf where b is finite, and finite where b is +inf", MinusThenT, h2,
            new (Rational t, Rational v)[] { (0, Rational.MinusInfinity), (1, 1), (2, 2), (3, Rational.MinusInfinity) }, (Rational) 2];
        yield return ["a is +inf where b is finite, and finite where b is -inf", h2, MinusThenT,
            new (Rational t, Rational v)[] { (0, Rational.PlusInfinity), (2, -2), (3, Rational.PlusInfinity) }, Rational.PlusInfinity];
        yield return ["a is -inf where b is +inf", Sequence.MinusInfinite(0, 2, true, true), h2,
            new (Rational t, Rational v)[] { (1, Rational.MinusInfinity), (2, Rational.MinusInfinity) }, Rational.MinusInfinity];
        yield return ["a is +inf where b is -inf", Sequence.PlusInfinite(0, 1, true, false), MinusThenT,
            new (Rational t, Rational v)[] { (0, Rational.PlusInfinity), (new Rational(1, 2), Rational.PlusInfinity) }, Rational.PlusInfinity];
    }

    [Theory]
    [MemberData(nameof(MinusInfiniteValueCases))]
    public void VerticalDeviationOverMinusInfiniteValues(string id, Sequence a, Sequence b, (Rational t, Rational v)[] values, Rational sup)
    {
        var vdev = Sequence.Subtraction(a, b);
        foreach (var (t, v) in values)
            Assert.True(vdev.ValueAt(t) == v, $"{id}: {vdev.ValueAt(t)} at {t}, not {v}");
        Assert.True(Sequence.VerticalDeviation(a, b) == sup, $"{id}: supremum {Sequence.VerticalDeviation(a, b)}, not {sup}");
    }

    /// <summary>
    /// [TBP-SEQ-DEV] § 3: where both operands are $-\infty$ at a shared time, as where both are $+\infty$, the difference is undefined, and the methods throw.
    /// </summary>
    [Fact]
    public void BothMinusInfiniteAtASharedTimeIsRejected()
    {
        var minusOn02 = Sequence.MinusInfinite(0, 2, true, true);
        foreach (var (a, b) in new[] { (MinusThenT, MinusThenT), (MinusThenT, minusOn02), (minusOn02, MinusThenT) })
        {
            Assert.Throws<UndeterminedResultException>(() => Sequence.Subtraction(a, b));
            Assert.Throws<UndeterminedResultException>(() => Sequence.VerticalDeviation(a, b));
        }
    }

    /// <summary>
    /// Curves taking the value $+\infty$ or $-\infty$, and the domains to restrict them to,
    /// with whether some time has both curves at the same infinity, where the curves' deviation is undefined.
    /// </summary>
    public static IEnumerable<object[]> InfiniteCurveCases()
    {
        var w = new RateLatencyServiceCurve(1, 0);
        var delta1 = new DelayServiceCurve(1);
        var delta2 = new DelayServiceCurve(2);
        // -inf on [0, 1[, then t
        var minusThenT = new Curve(
            new Sequence([ Point.MinusInfinite(0), Segment.MinusInfinite(0, 1), new Point(1, 1), new Segment(1, 2, 1, 1) ]),
            1, 1, 1);
        yield return ["VI-a-infinite, curves", delta2, w, I(0, 4, true, true), I(0, 4, true, true), false];
        yield return ["VI-b-infinite, curves", w, delta2, I(0, 4, true, true), I(0, 4, true, true), false];
        // the cuts are both +inf only after 2, which the shared domain [0, 2] excludes
        yield return ["VI-both-infinite, curves, cut before", delta2, delta1, I(0, 2, true, true), I(1, 3, true, true), true];
        yield return ["a is -inf, curves", minusThenT, w, I(0, 4, true, true), I(0, 4, true, true), false];
        yield return ["b is -inf, curves", w, minusThenT, I(0, 4, true, true), I(0, 4, true, true), false];
        yield return ["a is +inf where b is -inf, curves", delta1, minusThenT, I(0, 4, true, true), I(0, 4, true, true), false];
        // the cuts are both -inf only before 1, which the shared domain [1, 3] excludes
        yield return ["both -inf, curves, cut after", minusThenT, minusThenT, I(0, 3, true, true), I(1, 4, true, true), true];
    }

    /// <summary>
    /// Checks that the vertical deviation between cuts of curves with infinite values matches the curves', [TBP-SEQ-DEV] Theorem 17.
    /// Over the shared domain, the deviation between the restrictions is the curves' at every time.
    /// Where no time has both curves at the same infinity, the curves' deviation is defined, and at least that between the restrictions.
    /// Where some time does, the curves' deviation is undefined, and the method throws.
    /// </summary>
    [Theory]
    [MemberData(nameof(InfiniteCurveCases))]
    public void VerticalDeviationMatchesTheCurvesOverInfiniteValues(string id, Curve f, Curve g, Interval xA, Interval xB, bool curvesUndefined)
    {
        var a = f.Cut(xA);
        var b = g.Cut(xB);
        var vdev = Sequence.Subtraction(a, b);
        var shared = Interval.Intersection(xA, xB)!.Value;
        for (var i = 0; i <= 16; i++)
        {
            var t = shared.Lower + (shared.Upper - shared.Lower) * new Rational(i, 16);
            if (!vdev.IsDefinedAt(t))
                continue;
            var (x, y) = (f.ValueAt(t), g.ValueAt(t));
            var expected = x.IsPlusInfinite || y.IsMinusInfinite ? Rational.PlusInfinity
                : x.IsMinusInfinite || y.IsPlusInfinite ? Rational.MinusInfinity
                : x - y;
            Assert.True(vdev.ValueAt(t) == expected, $"{id}: {vdev.ValueAt(t)} at {t}, the curves {expected}");
        }

        if (curvesUndefined)
            Assert.Throws<UndeterminedResultException>(() => Curve.VerticalDeviation(f, g));
        else
            Assert.True(Sequence.VerticalDeviation(a, b) <= Curve.VerticalDeviation(f, g), $"{id}: above the curves' deviation");
    }

    /// <summary>
    /// [TBP-EB-FRTC] EB-FRTC-SEQ-V1:
    /// over the intersection of the two domains, the deviation between the restrictions is the deviation between the curves, with no further hypothesis.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidityCasesTestCases))]
    public void DifferenceMatchesTheCurvesOverTheSharedDomain(Curve f, Curve g, Interval xF, Interval xG)
    {
        var fCut = f.Cut(xF.Lower, xF.Upper, xF.IsLowerIncluded, xF.IsUpperIncluded);
        var gCut = g.Cut(xG.Lower, xG.Upper, xG.IsLowerIncluded, xG.IsUpperIncluded);

        var vdevFunctionSequence = Sequence.Subtraction(fCut, gCut);
        var vdevFunctionCurves = f - g;

        var overlap = Interval.Intersection(fCut.Support, gCut.Support)!.Value;
        Assert.Equal(overlap.Lower, vdevFunctionSequence.DefinedFrom);
        Assert.Equal(overlap.Upper, vdevFunctionSequence.DefinedUntil);
        Assert.True(vdevFunctionCurves.Match(vdevFunctionSequence));
    }

    /// <summary>
    /// Non-negative, non-decreasing sequences over assorted domains and shapes, used by the property tests below.
    /// </summary>
    public static List<Sequence> PropertySequences =
    [
        new Sequence([ Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6) ]),
        new Sequence([ Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12) ]),
        new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 2), new Point(2, 4), new Segment(2, 4, 4, 0),
            new Point(4, 4), new Segment(4, 6, 4, 1), new Point(6, 6) ]),
        new Sequence([
            Point.Origin(), new Segment(0, 3, 0, 1), new Point(3, 3), new Segment(3, 6, 5, 1),
            new Point(6, 8) ]),
        new Sequence([ new Point(2, 1), new Segment(2, 6, 1, 1), new Point(6, 5) ]),
        new Sequence([ new Segment(1, 5, 2, 1), new Point(5, 6) ]),
    ];

    public static IEnumerable<object[]> PropertySequencePairs =>
        PropertySequences.SelectMany(a => PropertySequences.Select(b => new object[] { a, b }));

    private static IEnumerable<Rational> SampleTimes(Sequence s)
    {
        var times = new List<Rational>();
        for (var i = 0; i <= 20; i++)
            times.Add(s.DefinedFrom + (s.DefinedUntil - s.DefinedFrom) * new Rational(i, 20));
        times.AddRange(s.EnumerateBreakpoints().Select(bp => bp.center.Time));
        return times.Distinct().Where(s.IsDefinedAt);
    }

    /// <summary>
    /// $vDev(a, b, t) = -vDev(b, a, t)$, since the definition subtracts without a positive part.
    /// </summary>
    [Theory]
    [MemberData(nameof(PropertySequencePairs))]
    public void VerticalDeviationIsAntisymmetric(Sequence a, Sequence b)
    {
        if (Interval.Intersection(a.Support, b.Support) is null) return;

        var v = Sequence.Subtraction(a, b);
        var w = Sequence.Subtraction(b, a);
        foreach (var t in SampleTimes(v))
            Assert.Equal(-w.ValueAt(t), v.ValueAt(t));
    }

    /// <summary>
    /// Raising the first operand raises the deviation by as much.
    /// </summary>
    [Theory]
    [MemberData(nameof(PropertySequencePairs))]
    public void RaisingTheFirstOperandRaisesTheDeviation(Sequence a, Sequence b)
    {
        if (Interval.Intersection(a.Support, b.Support) is null) return;

        var shift = new Rational(7, 2);
        var v = Sequence.Subtraction(a, b);
        var raised = Sequence.Subtraction(a + shift, b);
        foreach (var t in SampleTimes(v))
            Assert.Equal(v.ValueAt(t) + shift, raised.ValueAt(t));
    }
}
