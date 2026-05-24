using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceExtensionsEdgeCases
{
    public static List<(List<Element> elements, Rational time, Rational expected)> PointCutCases =
    [
        (
            [
                Point.Origin(),
                Segment.Constant(0, 2, 5),
                new Point(2, 5)
            ],
            1,
            5
        )
    ];

    public static IEnumerable<object[]> GetPointCutCases()
        => PointCutCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetPointCutCases))]
    public void Cut_EqualInclusiveEndpoints_ReturnsSampledPoint(
        List<Element> elements,
        Rational time,
        Rational expected)
    {
        var cut = elements.Cut(time, time, isStartIncluded: true, isEndIncluded: true).ToList();

        var point = Assert.IsType<Point>(Assert.Single(cut));
        Assert.Equal(time, point.Time);
        Assert.Equal(expected, point.Value);
    }

    public static List<(List<Element> elements, Rational cutStart, Rational cutEnd)> InvalidCutCases =
    [
        ([], 0, 1),
        ([Point.Origin()], 2, 1),
        ([Segment.Constant(0, 1, 3)], 0, 1),
        ([Point.Origin()], 0, 0)
    ];

    public static IEnumerable<object[]> GetInvalidCutCases()
        => InvalidCutCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidCutCases))]
    public void Cut_InvalidArguments_Throw(List<Element> elements, Rational cutStart, Rational cutEnd)
    {
        Assert.Throws<ArgumentException>(() =>
            elements.Cut(cutStart, cutEnd, isStartIncluded: true, isEndIncluded: false).ToList()
        );
    }

    public static List<(List<Element> elements, Rational fillFrom, Rational fillTo, Rational fillWith)> FillCases =
    [
        (
            [
                new Point(1, 2),
                Segment.Constant(1, 2, 2)
            ],
            0,
            3,
            9
        )
    ];

    public static IEnumerable<object[]> GetFillCases()
        => FillCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFillCases))]
    public void ToSequence_WithFill_CompletesGaps(
        List<Element> elements,
        Rational fillFrom,
        Rational fillTo,
        Rational fillWith)
    {
        var sequence = elements.ToSequence(fillFrom, fillTo, fillWith);

        Assert.Equal(fillFrom, sequence.DefinedFrom);
        Assert.Equal(fillTo, sequence.DefinedUntil);
        Assert.Equal(fillWith, sequence.ValueAt(0));
        Assert.Equal(2, sequence.ValueAt(1));
        Assert.Equal(fillWith, sequence.RightLimitAt(2));
    }

    public static List<List<Element>> MergeAsEnumerableCases =
    [
        [
            Segment.Constant(0, 1, 2),
            new Point(1, 2),
            Segment.Constant(1, 2, 2)
        ]
    ];

    public static IEnumerable<object[]> GetMergeAsEnumerableCases()
        => MergeAsEnumerableCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetMergeAsEnumerableCases))]
    public void MergeAsEnumerable_MergesAlignedTriplets(List<Element> elements)
    {
        var merged = elements.MergeAsEnumerable().ToList();

        var segment = Assert.IsType<Segment>(Assert.Single(merged));
        Assert.Equal(0, segment.StartTime);
        Assert.Equal(2, segment.EndTime);
        Assert.Equal(2, segment.RightLimitAtStartTime);
        Assert.Equal(0, segment.Slope);
    }

    public static List<List<Element>> InvalidMergeAsEnumerableCases =
    [
        [],
        [Point.Origin(), new Point(1, 1)],
        [Segment.Constant(0, 1, 0), Segment.Constant(1, 2, 0)],
        [Segment.Constant(0, 1, 0), new Point(1, 0), new Point(1, 0)]
    ];

    public static IEnumerable<object[]> GetInvalidMergeAsEnumerableCases()
        => InvalidMergeAsEnumerableCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidMergeAsEnumerableCases))]
    public void MergeAsEnumerable_InvalidSequences_Throw(List<Element> elements)
    {
        Assert.Throws<ArgumentException>(() => elements.MergeAsEnumerable().ToList());
    }

    public static List<(List<Element> elements, bool inTimeOrder, bool inTimeSequence, bool uninterrupted)> SequenceShapeCases =
    [
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 0)
            ],
            true,
            true,
            true
        ),
        (
            [
                new Point(1, 1),
                Point.Origin()
            ],
            false,
            false,
            false
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(1, 2, 0)
            ],
            true,
            true,
            false
        )
    ];

    public static IEnumerable<object[]> GetSequenceShapeCases()
        => SequenceShapeCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSequenceShapeCases))]
    public void SequenceShapePredicates_ReturnExpectedValues(
        List<Element> elements,
        bool inTimeOrder,
        bool inTimeSequence,
        bool uninterrupted)
    {
        Assert.Equal(inTimeOrder, elements.AreInTimeOrder());
        Assert.Equal(inTimeSequence, elements.AreInTimeSequence());
        Assert.Equal(uninterrupted, elements.AreUninterruptedSequence());
    }

    public static List<List<Element>> EmptyElementCases =
    [
        []
    ];

    public static IEnumerable<object[]> GetEmptyElementCases()
        => EmptyElementCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetEmptyElementCases))]
    public void EmptyElementOperations_Throw(List<Element> elements)
    {
        Assert.Throws<InvalidOperationException>(() => elements.AreUninterruptedSequence());
        Assert.Throws<ArgumentException>(() => elements.EnumerateBreakpoints().ToList());
        Assert.Throws<ArgumentException>(() => elements.IsContinuous());
    }

    public static List<(List<Element> elements, Rational time)> MissingElementCases =
    [
        ([Point.Origin(), Segment.Constant(0, 1, 0)], 3)
    ];

    public static IEnumerable<object[]> GetMissingElementCases()
        => MissingElementCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetMissingElementCases))]
    public void ElementLookup_OutOfSupport_Throws(List<Element> elements, Rational time)
    {
        Assert.Throws<ArgumentException>(() => elements.GetElementAt(time));
        Assert.Throws<ArgumentException>(() => elements.GetSegmentAfter(time));
    }

    public static List<(List<Element> elements, Rational time)> MissingPreviousSegmentCases =
    [
        ([Point.Origin(), Segment.Constant(0, 1, 0)], 0)
    ];

    public static IEnumerable<object[]> GetMissingPreviousSegmentCases()
        => MissingPreviousSegmentCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetMissingPreviousSegmentCases))]
    public void GetSegmentBefore_WhenNoSegmentEndsBeforeTime_Throws(List<Element> elements, Rational time)
    {
        Assert.Throws<ArgumentException>(() => elements.GetSegmentBefore(time));
    }

    public static List<List<Element>> SortCases =
    [
        [
            new Point(2, 2),
            Point.Origin(),
            new Point(1, 1)
        ]
    ];

    public static IEnumerable<object[]> GetSortCases()
        => SortCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSortCases))]
    public void SortElements_ReordersByTime(List<Element> elements)
    {
        var sorted = elements.SortElements();

        Assert.Equal(new List<Rational> { 0, 1, 2 }, sorted.Select(e => e.StartTime).ToList());
    }

    public static List<List<Element>> BreakpointCases =
    [
        [
            Point.Origin(),
            Segment.Constant(0, 1, 0),
            new Point(1, 2),
            Segment.Constant(1, 2, 3)
        ],
        [
            Segment.Constant(0, 1, 0)
        ]
    ];

    public static IEnumerable<object[]> GetBreakpointCases()
        => BreakpointCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBreakpointCases))]
    public void BreakpointEnumeration_HandlesEndpointsAndSegmentOnlySequences(List<Element> elements)
    {
        var breakpoints = elements.EnumerateBreakpoints().ToList();

        if (elements.Count == 1)
        {
            Assert.Empty(breakpoints);
        }
        else
        {
            Assert.Equal(2, breakpoints.Count);
            Assert.Null(breakpoints[0].left);
            Assert.NotNull(breakpoints[0].right);
            Assert.NotNull(breakpoints[1].left);
            Assert.NotNull(breakpoints[1].right);
        }
    }

    public static List<(Segment? left, Point center, Segment? right, List<Rational> expected)> BoundaryValueCases =
    [
        (
            Segment.Constant(0, 1, 2),
            new Point(1, 3),
            Segment.Constant(1, 2, 4),
            [2, 3, 4]
        ),
        (
            null,
            Point.Origin(),
            Segment.Constant(0, 1, 5),
            [0, 5]
        )
    ];

    public static IEnumerable<object[]> GetBoundaryValueCases()
        => BoundaryValueCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBoundaryValueCases))]
    public void BreakpointBoundaryValues_ReturnLeftPointAndRightLimits(
        Segment? left,
        Point center,
        Segment? right,
        List<Rational> expected)
    {
        Assert.Equal(expected, (left, center, right).GetBreakpointBoundaryValues().ToList());
    }

    public static List<(List<Element> elements, List<Rational> values, List<(Rational time, Rational value)> pairs)> ElementBoundaryCases =
    [
        (
            [
                Point.Origin(),
                new Segment(0, 2, 1, 3),
                new Point(2, 7)
            ],
            [0, 1, 7, 7],
            [(0, 0), (0, 1), (2, 7), (2, 7)]
        )
    ];

    public static IEnumerable<object[]> GetElementBoundaryCases()
        => ElementBoundaryCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetElementBoundaryCases))]
    public void ElementBoundaryEnumerators_ReturnValuesAndPairs(
        List<Element> elements,
        List<Rational> values,
        List<(Rational time, Rational value)> pairs)
    {
        Assert.Equal(values, elements.GetElementsBoundaryValues().ToList());
        Assert.Equal(pairs, elements.GetElementsBoundaryPairs().ToList());
        Assert.Equal(values, elements.EnumerateBreakpoints().GetBreakpointsBoundaryValues().ToList());
    }

    public static List<(List<Element> elements, bool continuous, bool leftContinuous, bool rightContinuous)> ContinuityCases =
    [
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 0)
            ],
            true,
            true,
            true
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 1),
                new Point(1, 1)
            ],
            false,
            true,
            false
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 2)
            ],
            false,
            false,
            true
        )
    ];

    public static IEnumerable<object[]> GetContinuityCases()
        => ContinuityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetContinuityCases))]
    public void ContinuityPredicates_ReturnExpectedValues(
        List<Element> elements,
        bool continuous,
        bool leftContinuous,
        bool rightContinuous)
    {
        Assert.Equal(continuous, elements.IsContinuous());
        Assert.Equal(leftContinuous, elements.IsLeftContinuous());
        Assert.Equal(rightContinuous, elements.IsRightContinuous());
    }

    public static List<(List<Element> elements, bool isIncreasing)> IncreasingCases =
    [
        (
            [
                Point.Origin(),
                new Segment(0, 1, 0, 1),
                new Point(1, 1)
            ],
            true
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 0)
            ],
            false
        ),
        (
            [
                Point.Origin(),
                new Segment(0, 1, 2, -1),
                new Point(1, 1)
            ],
            false
        )
    ];

    public static IEnumerable<object[]> GetIncreasingCases()
        => IncreasingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetIncreasingCases))]
    public void IsIncreasing_ReturnsExpectedValues(List<Element> elements, bool isIncreasing)
    {
        Assert.Equal(isIncreasing, elements.IsIncreasing());
    }

    public static List<(List<Element> elements, Rational supArg, Rational? maxArg, Rational infArg, Rational? minArg)> ExtremumArgumentCases =
    [
        (
            [
                Point.Origin(),
                new Segment(0, 1, 0, 2),
                new Point(1, 3),
                Segment.Constant(1, 2, 1)
            ],
            1,
            1,
            0,
            0
        ),
        (
            [
                new Segment(0, 1, 0, 1)
            ],
            1,
            null,
            0,
            null
        )
    ];

    public static IEnumerable<object[]> GetExtremumArgumentCases()
        => ExtremumArgumentCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExtremumArgumentCases))]
    public void ExtremumArgumentMethods_ReturnFirstBoundaryThatDeterminesValue(
        List<Element> elements,
        Rational supArg,
        Rational? maxArg,
        Rational infArg,
        Rational? minArg)
    {
        IReadOnlyCollection<Element> collection = elements;

        Assert.Equal(supArg, elements.SupArg());
        Assert.Equal(maxArg, elements.MaxArg());
        Assert.Equal(maxArg, collection.MaxArg());
        Assert.Equal(infArg, elements.InfArg());
        Assert.Equal(minArg, elements.MinArg());
        Assert.Equal(minArg, collection.MinArg());
    }

    public static List<(List<Element> elements, Rational? ceiling, bool isCeilingIncluded, int expectedCount, Rational expectedLastValue)> CutWithCeilingCases =
    [
        (
            [
                Point.Origin(),
                new Segment(0, 3, 0, 2),
                new Point(3, 6)
            ],
            3,
            true,
            3,
            3
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 5)
            ],
            3,
            false,
            3,
            3
        ),
        (
            [
                Point.Origin(),
                Segment.Constant(0, 1, 0)
            ],
            null,
            true,
            2,
            0
        )
    ];

    public static IEnumerable<object[]> GetCutWithCeilingCases()
        => CutWithCeilingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetCutWithCeilingCases))]
    public void CutWithCeiling_TruncatesAtFirstCeilingCrossing(
        List<Element> elements,
        Rational? ceiling,
        bool isCeilingIncluded,
        int expectedCount,
        Rational expectedLastValue)
    {
        var cut = elements.CutWithCeiling(ceiling, isCeilingIncluded).ToList();
        var lastValue = cut.Last() switch
        {
            Point p => p.Value,
            Segment s => s.LeftLimitAtEndTime,
            _ => throw new InvalidOperationException()
        };

        Assert.Equal(expectedCount, cut.Count);
        Assert.Equal(expectedLastValue, lastValue);
    }

    public static List<(List<Element> elements, Rational? ceiling)> InvalidCutWithCeilingCases =
    [
        (
            [
                Point.Origin(),
                new Segment(0, 1, 2, -1)
            ],
            1
        )
    ];

    public static IEnumerable<object[]> GetInvalidCutWithCeilingCases()
        => InvalidCutWithCeilingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidCutWithCeilingCases))]
    public void CutWithCeiling_RejectsDecreasingSegments(List<Element> elements, Rational? ceiling)
    {
        Assert.Throws<ArgumentException>(() => elements.CutWithCeiling(ceiling).ToList());
    }

    public static List<(List<Sequence> sequences, Rational sampleTime, Rational lowerValue, Rational upperValue)> SequenceEnvelopeCases =
    [
        (
            [
                ConstantSequence(3),
                ConstantSequence(1),
                ConstantSequence(2)
            ],
            0,
            1,
            3
        ),
        (
            [
                ConstantSequence(3),
                ConstantSequence(4),
                ConstantSequence(5),
                ConstantSequence(6),
                ConstantSequence(7),
                ConstantSequence(8),
                ConstantSequence(9),
                ConstantSequence(10),
                ConstantSequence(11)
            ],
            0,
            3,
            11
        )
    ];

    public static IEnumerable<object[]> GetSequenceEnvelopeCases()
        => SequenceEnvelopeCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSequenceEnvelopeCases))]
    public void SequenceListEnvelopes_ReturnExpectedBoundaryValues(
        List<Sequence> sequences,
        Rational sampleTime,
        Rational lowerValue,
        Rational upperValue)
    {
        var settings = new ComputationSettings
        {
            UseParallelListLowerEnvelope = true,
            UseParallelListUpperEnvelope = true
        };

        var lower = sequences.LowerEnvelope(settings).ToSequence();
        var upper = sequences.UpperEnvelope(settings).ToSequence();

        Assert.Equal(lowerValue, lower.ValueAt(sampleTime));
        Assert.Equal(upperValue, upper.ValueAt(sampleTime));
    }

    public static List<List<Element>> EmptyEnvelopeElementCases =
    [
        []
    ];

    public static IEnumerable<object[]> GetEmptyEnvelopeElementCases()
        => EmptyEnvelopeElementCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetEmptyEnvelopeElementCases))]
    public void ElementEnvelopes_RejectEmptyInput(List<Element> elements)
    {
        Assert.Throws<ArgumentException>(() => elements.LowerEnvelope());
        Assert.Throws<ArgumentException>(() => elements.UpperEnvelope());
    }

    private static Sequence ConstantSequence(Rational value)
    {
        return new Sequence(
            [
                new Point(0, value),
                Segment.Constant(0, 1, value)
            ]
        );
    }
}
