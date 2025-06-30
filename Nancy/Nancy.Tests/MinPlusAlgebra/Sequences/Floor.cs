using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Floor
{
    public static List<(Sequence Sequence, Sequence Floor)> KnownPairs =
    [
        (
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 1),
                Segment.Constant(1, 2, 1),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 1),
                Segment.Constant(1, 2, 1),
            ])
        ),
        (
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0.25m),
                new Point(1, 1),
                Segment.Constant(1, 2, 1.25m),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 1),
                Segment.Constant(1, 2, 1),
            ])
        ),
        (
            new Sequence([
                Point.Origin(),
                new Segment(0, 1, 0, 0.5m),
                new Point(1, 1),
                new Segment(1, 2, 1, 0.5m),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 1),
                Segment.Constant(1, 2, 1),
            ])
        ),
        (
            new Sequence([
                Point.Origin(),
                new Segment(0, 0.5m, 0, 0.5m),
                new Point(0.5m, 0.25m),
                new Segment(0.5m, 1, 0.25m, 0.25m),
                new Point(1, 1),
                new Segment(1, 2, 1, 0.5m),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 1, 0),
                new Point(1, 1),
                Segment.Constant(1, 2, 1),
            ])
        ),
        (
            new Sequence([
                Point.Origin(),
                new Segment(0, 1, 0, 4),
                new Point(1, 4),
                new Segment(1, 2, 4, 4),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 0.25m, 0),
                new Point(0.25m, 1),
                Segment.Constant(0.25m, 0.5m, 1),
                new Point(0.5m, 2),
                Segment.Constant(0.5m, 0.75m, 2),
                new Point(0.75m, 3),
                Segment.Constant(0.75m, 1, 3),
                new Point(1, 4),
                Segment.Constant(1, 1.25m, 4),
                new Point(1.25m, 5),
                Segment.Constant(1.25m, 1.5m, 5),
                new Point(1.5m, 6),
                Segment.Constant(1.5m, 1.75m, 6),
                new Point(1.75m, 7),
                Segment.Constant(1.75m, 2, 7),
            ])
        ),
        (
            new Sequence([
                Point.Origin(),
                new Segment(0, 1, 0, -4),
                new Point(1, -4),
                new Segment(1, 2, -4, -4),
            ]),
            new Sequence([
                Point.Origin(),
                Segment.Constant(0, 0.25m, -1),
                new Point(0.25m, -1),
                Segment.Constant(0.25m, 0.5m, -2),
                new Point(0.5m, -2),
                Segment.Constant(0.5m, 0.75m, -3),
                new Point(0.75m, -3),
                Segment.Constant(0.75m, 1, -4),
                new Point(1, -4),
                Segment.Constant(1, 1.25m, -5),
                new Point(1.25m, -5),
                Segment.Constant(1.25m, 1.5m, -6),
                new Point(1.5m, -6),
                Segment.Constant(1.5m, 1.75m, -7),
                new Point(1.75m, -7),
                Segment.Constant(1.75m, 2, -8),
            ])
        ),
    ];

    public static IEnumerable<object[]> FloorTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FloorTestCases))]
    public void FloorTest(Sequence sequence, Sequence expected)
    {
        var floor = sequence.Floor();
        Assert.True(Sequence.Equivalent(expected, floor));
    }
}