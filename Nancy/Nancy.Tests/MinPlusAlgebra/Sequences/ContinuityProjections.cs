using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ContinuityProjections
{
    public static List<(Element[] elements, Element[] expected)> ToLeftContinuousCases =
    [
        (
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 9),
                Segment.Constant(1, 2, 4)
            ],
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 2),
                Segment.Constant(1, 2, 4)
            ]
        ),
        (
            // a lone segment has no point to project
            [new Segment(0, 2, 2, -1)],
            [new Segment(0, 2, 2, -1)]
        )
    ];

    public static IEnumerable<object[]> ToLeftContinuousTestCases()
        => ToLeftContinuousCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ToLeftContinuousTestCases))]
    public void ToLeftContinuousProjectsPointsFromLeft(Element[] elements, Element[] expected)
    {
        var result = elements.ToLeftContinuous().ToList();

        Assert.Equal(expected, result);
    }

    public static List<(Element[] elements, Element[] expected)> ToRightContinuousCases =
    [
        (
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 9),
                Segment.Constant(1, 2, 4)
            ],
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 4),
                Segment.Constant(1, 2, 4)
            ]
        ),
        (
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 9)
            ],
            [
                Segment.Constant(0, 1, 2),
                new Point(1, 9)
            ]
        ),
        (
            // a lone segment has no point to project
            [new Segment(0, 2, 2, -1)],
            [new Segment(0, 2, 2, -1)]
        )
    ];

    public static IEnumerable<object[]> ToRightContinuousTestCases()
        => ToRightContinuousCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ToRightContinuousTestCases))]
    public void ToRightContinuousProjectsPointsFromRight(Element[] elements, Element[] expected)
    {
        var result = elements.ToRightContinuous().ToList();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void LoneSegmentSequenceIsLeftAndRightContinuous()
    {
        var sequence = new Sequence([new Segment(0, 2, 2, -1)]);
        Assert.Equal(sequence, sequence.ToLeftContinuous());
        Assert.Equal(sequence, sequence.ToRightContinuous());
    }
}
