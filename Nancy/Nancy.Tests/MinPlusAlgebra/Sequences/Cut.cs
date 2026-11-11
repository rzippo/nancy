using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Cut
{
    //Start closed, end open
    private static Sequence a = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30)
    });

    //Start open, end open
    private static Sequence b = new Sequence(new Element[]
    {
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30)
    });

    //Start open, end closed
    private static Sequence c = new Sequence(new Element[]
    {
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30)
    });

    //Start closed, end closed
    private static Sequence d = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30)
    });

    //Long. Start closed, end closed
    private static Sequence e = new Sequence(new Element[]
    {
        Point.Origin(),
        Segment.Zero(0, 10),
        Point.Zero(10),
        Segment.Zero(10, 20),
        Point.Zero(20),
        Segment.Zero(20, 30),
        Point.Zero(30),
        Segment.Zero(30, 40),
        Point.Zero(40),
        Segment.Zero(40, 50),
        Point.Zero(50)
    });

    //Start closed, end open, with slopes and a jump at every breakpoint
    private static Sequence v = new Sequence(new Element[]
    {
        new Point(0, 1),
        new Segment(0, 10, 2, 1),
        new Point(10, 5),
        new Segment(10, 20, 7, new Rational(-1, 2)),
        new Point(20, 4),
        new Segment(20, 30, 0, 2)
    });

    public static IEnumerable<object[]> GetSuccessTestCases()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
                []
                {
                    // Within endpoints
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartIncluded: true, isEndIncluded: true),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartIncluded: false, isEndIncluded: false),
                    (sequence: a, cutStart: 5, cutEnd: 15, isStartIncluded: true, isEndIncluded: false),

                    // At endpoints
                    (sequence: a, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: false),
                    (sequence: b, cutStart: 0, cutEnd: 30, isStartIncluded: false, isEndIncluded: false),
                    (sequence: c, cutStart: 0, cutEnd: 30, isStartIncluded: false, isEndIncluded: true),
                    (sequence: d, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: true),

                    // At segments endpoints
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartIncluded: false, isEndIncluded: false),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 10, cutEnd: 20, isStartIncluded: true, isEndIncluded: true),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartIncluded: true, isEndIncluded: false),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartIncluded: false, isEndIncluded: false),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartIncluded: false, isEndIncluded: true),
                    (sequence: e, cutStart: 10, cutEnd: 40, isStartIncluded: true, isEndIncluded: true),

                    // Within segments endpoints
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartIncluded: false, isEndIncluded: false),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 15, cutEnd: 25, isStartIncluded: true, isEndIncluded: true),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartIncluded: true, isEndIncluded: false),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartIncluded: false, isEndIncluded: false),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartIncluded: false, isEndIncluded: true),
                    (sequence: e, cutStart: 15, cutEnd: 45, isStartIncluded: true, isEndIncluded: true),

                    // Within one segment
                    (sequence: a, cutStart: 12, cutEnd: 18, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 12, cutEnd: 18, isStartIncluded: false, isEndIncluded: false),
                    (sequence: a, cutStart: 12, cutEnd: 18, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 12, cutEnd: 18, isStartIncluded: true, isEndIncluded: true),

                    // Within one segment, one side at its endpoint
                    (sequence: a, cutStart: 10, cutEnd: 15, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 10, cutEnd: 15, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 12, cutEnd: 20, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 12, cutEnd: 20, isStartIncluded: false, isEndIncluded: true),

                    // With slopes and jumps
                    (sequence: v, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: false),
                    (sequence: v, cutStart: 5, cutEnd: 25, isStartIncluded: true, isEndIncluded: true),
                    (sequence: v, cutStart: 5, cutEnd: 25, isStartIncluded: false, isEndIncluded: false),
                    (sequence: v, cutStart: 10, cutEnd: 20, isStartIncluded: true, isEndIncluded: true),
                    (sequence: v, cutStart: 10, cutEnd: 20, isStartIncluded: false, isEndIncluded: false),
                    (sequence: v, cutStart: 12, cutEnd: 18, isStartIncluded: true, isEndIncluded: true),
                    (sequence: v, cutStart: 12, cutEnd: 18, isStartIncluded: false, isEndIncluded: false),
                    (sequence: v, cutStart: 10, cutEnd: 15, isStartIncluded: false, isEndIncluded: true),
                    (sequence: v, cutStart: 12, cutEnd: 20, isStartIncluded: true, isEndIncluded: false),
                    (sequence: v, cutStart: 12, cutEnd: 20, isStartIncluded: false, isEndIncluded: true),
                    (sequence: v, cutStart: 20, cutEnd: 20, isStartIncluded: true, isEndIncluded: true),

                    // Matching, both inclusive
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartIncluded: true, isEndIncluded: true)
                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartIncluded,
                testCase.isEndIncluded
            };
        }
    }

    public static IEnumerable<object[]> FromOtherTests()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
                []
                {
                    (
                        sequence: new Sequence(new Element[]
                        {
                            Point.Zero(2),
                            Segment.Zero(2, 3),
                            Point.Zero(3),
                            Segment.Zero(3, 6),
                            Point.Zero(6),
                            Segment.Zero(6, 7),
                            Point.Zero(7),
                            Segment.Zero(7, 8),
                            Point.Zero(8),
                        }), 
                        cutStart: 2, 
                        cutEnd: new Rational(20, 7), 
                        isStartIncluded: true, 
                        isEndIncluded: false
                    ),
                    (
                        sequence: new Sequence(new Element[]
                        {
                            Segment.Zero(-100, -10),
                            Point.Zero(-10),
                            Segment.Zero(-10, 190),
                            Point.Zero(190),
                            Segment.Zero(190, 200), 
                        }), 
                        cutStart: 0, 
                        cutEnd: 100, 
                        isStartIncluded: true, 
                        isEndIncluded: false
                    )

                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartIncluded,
                testCase.isEndIncluded
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void EquivalenceToExpected(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        Sequence cut = sequence.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);

        Assert.Equal(cutStart, cut.DefinedFrom);
        Assert.Equal(cutEnd, cut.DefinedUntil);
        Assert.Equal(isStartIncluded, cut.IsLeftClosed);
        Assert.Equal(isEndIncluded, cut.IsRightClosed);
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void CutElementsAreUninterrupted(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        Sequence cut = sequence.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        Assert.True(cut.Elements.AreUninterruptedSequence());
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void CutAgreesWithTheSequenceInsideTheInterval(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        // At the breakpoints of the sequence and the endpoints of the cut, and halfway between them: values inside, and limits from inside.
        Sequence cut = sequence.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var times = sequence.EnumerateBreakpoints()
            .Select(breakpoint => breakpoint.center.Time)
            .Concat([sequence.DefinedFrom, sequence.DefinedUntil, cutStart, cutEnd])
            .Where(t => t >= cutStart && t <= cutEnd)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        times = times.Concat(times.Zip(times.Skip(1), (x, y) => (x + y) / 2)).ToList();
        foreach (var t in times)
        {
            var inside = (t > cutStart || isStartIncluded) && (t < cutEnd || isEndIncluded);
            if (inside)
                Assert.Equal(sequence.ValueAt(t), cut.ValueAt(t));
            if (t < cutEnd)
                Assert.Equal(sequence.RightLimitAt(t), cut.RightLimitAt(t));
            if (t > cutStart)
                Assert.Equal(sequence.LeftLimitAt(t), cut.LeftLimitAt(t));
        }
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void CutIsTheStreamedCut(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        // Both keep the stored elements, splitting only the two at the ends of the cut.
        var streamed = sequence.Elements
            .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded)
            .ToSequence();
        Assert.Equal(streamed, sequence.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded));
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void EquivalenceToExpectedInterval(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        var interval = new Interval(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var cut = sequence.Cut(interval);

        Assert.Equal(cutStart, cut.DefinedFrom);
        Assert.Equal(cutEnd, cut.DefinedUntil);
        Assert.Equal(isStartIncluded, cut.IsLeftClosed);
        Assert.Equal(isEndIncluded, cut.IsRightClosed);
    }
    
    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void EquivalenceToExpected_AsEnumerable(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        var expected = sequence.Elements
            .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded)
            .ToArray();
        Sequence cut = sequence
            .CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded)
            .ToSequence();

        Assert.Equal(expected, cut.Elements);
        Assert.Equal(cutStart, cut.DefinedFrom);
        Assert.Equal(cutEnd, cut.DefinedUntil);
        Assert.Equal(isStartIncluded, cut.IsLeftClosed);
        Assert.Equal(isEndIncluded, cut.IsRightClosed);
    }

    [Theory]
    [MemberData(nameof(GetSuccessTestCases))]
    [MemberData(nameof(FromOtherTests))]
    public void EquivalenceToExpected_AsEnumerableInterval(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        var interval = new Interval(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        var cut = sequence
            .CutAsEnumerable(interval)
            .ToSequence();

        Assert.Equal(cutStart, cut.DefinedFrom);
        Assert.Equal(cutEnd, cut.DefinedUntil);
        Assert.Equal(isStartIncluded, cut.IsLeftClosed);
        Assert.Equal(isEndIncluded, cut.IsRightClosed);
    }

    public static IEnumerable<object[]> FullBoundsEndpointCases()
        => new (bool isStartIncluded, bool isEndIncluded)[]
        {
            (false, true),
            (true, false),
            (false, false)
        }.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FullBoundsEndpointCases))]
    public void StreamedCutOverFullBoundsExcludesRequestedEndpoints(bool isStartIncluded, bool isEndIncluded)
    {
        var expected = d.Elements
            .Cut(d.DefinedFrom, d.DefinedUntil, isStartIncluded, isEndIncluded)
            .ToArray();
        var cut = d.CutAsEnumerable(d.DefinedFrom, d.DefinedUntil, isStartIncluded, isEndIncluded);
        Assert.Equal(expected, cut);
    }

    public static IEnumerable<object[]> GetThrowingTestCases()
    {
        var testCases =
            new (Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
                []
                {
                    // Out of endpoints
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartIncluded: true, isEndIncluded: true),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartIncluded: false, isEndIncluded: false),
                    (sequence: a, cutStart: 5, cutEnd: 35, isStartIncluded: true, isEndIncluded: false),

                    // At endpoints
                    (sequence: a, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: true),
                    (sequence: b, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: true),
                    (sequence: c, cutStart: 0, cutEnd: 30, isStartIncluded: true, isEndIncluded: true),

                    // Matching, either non-inclusive
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartIncluded: false, isEndIncluded: true),
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartIncluded: true, isEndIncluded: false),
                    (sequence: a, cutStart: 20, cutEnd: 20, isStartIncluded: false, isEndIncluded: false)
                };

        foreach (var testCase in testCases)
        {
            yield return new object[]
            {
                testCase.sequence,
                testCase.cutStart,
                testCase.cutEnd,
                testCase.isStartIncluded,
                testCase.isEndIncluded
            };
        }
    }

    [Theory]
    [MemberData(nameof(GetThrowingTestCases))]
    public void Checks(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        Assert.Throws<ArgumentException>(() => sequence.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded));
    }
    
    [Theory]
    [MemberData(nameof(GetThrowingTestCases))]
    public void Checks_AsEnumerable(Sequence sequence, Rational cutStart, Rational cutEnd, bool isStartIncluded, bool isEndIncluded)
    {
        Assert.Throws<ArgumentException>(() => sequence.CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded));
    }

    /// <summary>
    /// A sequence closed at both ends, with a jump at 3.
    /// </summary>
    private static Sequence closedWithJump = new Sequence([
        Point.Origin(), new Segment(0, 3, 0, 1), new Point(3, 3), new Segment(3, 6, 5, 1), new Point(6, 8)
    ]);

    /// <summary>
    /// The same, right-open, so that nothing is defined at 6.
    /// </summary>
    private static Sequence rightOpenWithJump = new Sequence([
        Point.Origin(), new Segment(0, 3, 0, 1), new Point(3, 3), new Segment(3, 6, 5, 1)
    ]);

    private static Sequence continuousClosed = new Sequence([
        Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)
    ]);

    private static Sequence leftOpen = new Sequence([
        new Segment(1, 5, 2, 1), new Point(5, 6)
    ]);

    public static List<(Sequence sequence, Rational cutStart, Rational cutEnd)> NeighbourhoodTuples =
    [
        (closedWithJump, 0, 3),
        (closedWithJump, 3, 6),
        (closedWithJump, 0, 6),
        (closedWithJump, 1, 5),
        (rightOpenWithJump, 0, 3),
        (rightOpenWithJump, 1, 5),
        (continuousClosed, 0, 3),
        (continuousClosed, 0, 6),
        (continuousClosed, 3, 6),
        (leftOpen, 2, 5),
        (leftOpen, 2, 4),
    ];

    public static IEnumerable<object[]> NeighbourhoodTestCases()
        => NeighbourhoodTuples.ToXUnitTestCases();

    /// <summary>
    /// The interval is contained in the cut, and the cut agrees with the sequence over it.
    /// </summary>
    [Theory]
    [MemberData(nameof(NeighbourhoodTestCases))]
    public void NeighbourhoodCutContainsTheInterval(Sequence sequence, Rational cutStart, Rational cutEnd)
    {
        var cut = sequence.CutToNeighbourhood(cutStart, cutEnd);

        Assert.True(cut.IsDefinedAt(cutStart));
        Assert.True(cut.IsDefinedAt(cutEnd));
        foreach (var breakpoint in cut.EnumerateBreakpoints())
            Assert.Equal(sequence.ValueAt(breakpoint.center.Time), breakpoint.center.Value);
    }

    /// <summary>
    /// The one-sided limits at the endpoints are answerable on the result wherever the sequence itself can answer for them.
    /// Where it cannot, because the endpoint is at the edge of its own support, the cut is not extended and neither can.
    /// </summary>
    [Theory]
    [MemberData(nameof(NeighbourhoodTestCases))]
    public void NeighbourhoodCutAnswersTheOneSidedLimits(Sequence sequence, Rational cutStart, Rational cutEnd)
    {
        var cut = sequence.CutToNeighbourhood(cutStart, cutEnd);

        if (sequence.IsDefinedBefore(cutStart))
            Assert.Equal(sequence.LeftLimitAt(cutStart), cut.LeftLimitAt(cutStart));
        else
            Assert.Equal(sequence.DefinedFrom, cut.DefinedFrom);

        if (sequence.IsDefinedAfter(cutEnd))
            Assert.Equal(sequence.RightLimitAt(cutEnd), cut.RightLimitAt(cutEnd));
        else
            Assert.Equal(sequence.DefinedUntil, cut.DefinedUntil);
    }

    /// <summary>
    /// With neither side asked for, it is the plain cut over the closed interval.
    /// </summary>
    [Theory]
    [MemberData(nameof(NeighbourhoodTestCases))]
    public void NeighbourhoodCutWithoutEitherSideIsThePlainCut(Sequence sequence, Rational cutStart, Rational cutEnd)
    {
        var cut = sequence.CutToNeighbourhood(cutStart, cutEnd, leftNeighbourhood: false, rightNeighbourhood: false);

        Assert.Equal(sequence.Cut(cutStart, cutEnd, true, true), cut);
    }

    /// <summary>
    /// A sequence cut over the whole of its own support has nothing to either side, and is returned as it is.
    /// </summary>
    [Fact]
    public void NeighbourhoodCutOverTheWholeSupportIsNotExtended()
    {
        var cut = continuousClosed.CutToNeighbourhood(0, 6);

        Assert.Equal(0, cut.DefinedFrom);
        Assert.Equal(6, cut.DefinedUntil);
        Assert.Equal(continuousClosed, cut);
    }

    /// <summary>
    /// Where the element beyond the endpoint reaches the edge of a support that does not include it, the cut reaches it too and stays open there.
    /// </summary>
    [Fact]
    public void NeighbourhoodCutStaysOpenWhereTheSupportIs()
    {
        var cut = rightOpenWithJump.CutToNeighbourhood(0, 3);

        Assert.Equal(6, cut.DefinedUntil);
        Assert.False(cut.IsRightClosed);
        Assert.Equal(5, cut.RightLimitAt(3));
    }

    /// <summary>
    /// The right limit at a jump, which the plain cut ending there cannot answer for.
    /// </summary>
    [Fact]
    public void NeighbourhoodCutOfASequenceWithAJumpIsKnown()
    {
        Assert.Equal(3, closedWithJump.ValueAt(3));
        Assert.Equal(5, closedWithJump.RightLimitAt(3));

        var plain = closedWithJump.Cut(0, 3, true, true);
        Assert.Equal(3, plain.DefinedUntil);
        Assert.Throws<ArgumentException>(() => plain.RightLimitAt(3));

        var neighbourhood = closedWithJump.CutToNeighbourhood(0, 3);
        Assert.Equal(6, neighbourhood.DefinedUntil);
        Assert.Equal(5, neighbourhood.RightLimitAt(3));
    }

    public static IEnumerable<object[]> WholeSupports()
        => new[] { a, b, c, d }.Select(sequence => new object[] { sequence });

    [Theory]
    [MemberData(nameof(WholeSupports))]
    public void CutOfTheWholeSupportIsTheSequence(Sequence sequence)
    {
        var cut = sequence.Cut(sequence.DefinedFrom, sequence.DefinedUntil, sequence.IsLeftClosed, sequence.IsRightClosed);
        Assert.Same(sequence, cut);
    }

    /// <summary>
    /// Without propagation, a cut computes its properties anew, so it cannot be the sequence with its cache.
    /// </summary>
    [Theory]
    [MemberData(nameof(WholeSupports))]
    public void CutOfTheWholeSupportWithoutPropagationIsBuilt(Sequence sequence)
    {
        var cut = sequence.Cut(sequence.DefinedFrom, sequence.DefinedUntil, sequence.IsLeftClosed, sequence.IsRightClosed,
            new ComputationSettings { UseKnownPropertyPropagation = false });
        Assert.NotSame(sequence, cut);
        Assert.Equal(sequence.Elements, cut.Elements);
    }

}
