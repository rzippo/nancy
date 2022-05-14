using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceComposition
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testcases = new List<(Sequence f, Sequence g, Sequence expected)>
        {
            (
                f: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 2, 0, 2)
                }),
                g: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 2, 0, 1),
                    new Point(2, 1),
                    new Segment(2, 3, 1, 0),
                    new Point(3, 1),
                    new Segment(3, 4, 1, 1)
                }),
                expected: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 3),
                    Point.Zero(3),
                    new Segment(3, 4, 0, 2)
                })
            ),
            (
                f: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 1),
                    Point.Zero(1),
                    new Segment(1, 4, 0, 3)
                }),
                g: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 2),
                    new Point(1, 2),
                    new Segment(1, 2, 2, 0),
                    new Point(2, 2),
                    new Segment(2, 3, 2, 2)
                }),
                expected: new Sequence(new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 0.5m),
                    Point.Zero(0.5m),
                    new Segment(0.5m, 1, 0, 6),
                    new Point(1, 3),
                    new Segment(1, 2, 3, 0),
                    new Point(2, 3),
                    new Segment(2, 3, 3, 6)
                })
            )
        };

        foreach (var (f, g, expected) in testcases)
        {
            yield return new object[] { f, g, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Composition(Sequence f, Sequence g, Sequence expected)
    {
        var result = Sequence.Composition(f, g);
        Assert.True(Sequence.Equivalent(result, expected));
    }
}