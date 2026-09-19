using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Tests for the vertical deviation between sequences, [TBP-EB-FRTC] EB-FRTC-SEQ-D1,
/// and for its agreeing with the deviation between the curves the operands are restrictions of,
/// [TBP-EB-FRTC] EB-FRTC-SEQ-V1.
/// </summary>
public class VerticalDeviation
{
    /// <summary>
    /// Pairs of sequences and the value of $vDev$ over them.
    /// The definition subtracts without a positive part, so a sequence below the other gives a negative result,
    /// as it does for curves.
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
    ];

    public static IEnumerable<object[]> KnownVDevsTestCases => KnownVDevs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownVDevsTestCases))]
    public void VerticalDeviationValues(Sequence a, Sequence b, Rational expected)
    {
        Assert.Equal(expected, Sequence.VerticalDeviation(a, b));
    }

    [Fact]
    public void DisjointDomainsAreRejected()
    {
        var a = new Sequence([ new Point(0, 0), new Segment(0, 2, 0, 1), new Point(2, 2) ]);
        var b = new Sequence([ new Point(5, 5), new Segment(5, 7, 5, 1), new Point(7, 7) ]);
        Assert.Throws<ArgumentException>(() => Sequence.VerticalDeviation(a, b));
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
    /// [TBP-EB-FRTC] EB-FRTC-SEQ-V1: over the intersection of the two domains, the deviation between the
    /// restrictions is the deviation between the curves, with no further hypothesis.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidityCasesTestCases))]
    public void VerticalDeviationFunctionMatchesTheCurvesOverTheSharedDomain(Curve f, Curve g, Interval xF, Interval xG)
    {
        var fCut = f.Cut(xF.Lower, xF.Upper, xF.IsLowerIncluded, xF.IsUpperIncluded);
        var gCut = g.Cut(xG.Lower, xG.Upper, xG.IsLowerIncluded, xG.IsUpperIncluded);

        var vdevFunctionSequence = Sequence.VerticalDeviationFunction(fCut, gCut);
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

        var v = Sequence.VerticalDeviationFunction(a, b);
        var w = Sequence.VerticalDeviationFunction(b, a);
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
        var v = Sequence.VerticalDeviationFunction(a, b);
        var raised = Sequence.VerticalDeviationFunction(a + shift, b);
        foreach (var t in SampleTimes(v))
            Assert.Equal(v.ValueAt(t) + shift, raised.ValueAt(t));
    }
}
