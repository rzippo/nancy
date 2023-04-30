using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveToNonDecreasing
{
    public static IEnumerable<object[]> GetDecreasingTestCases()
    {
        var testcases = new List<(Curve operand, Curve expected)>
        {
            (
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 3, 2, -1),
                        new Point(3, 1),
                        new Segment(3, 6, 1, 1),
                        new Point(6, 4),
                        new Segment(6, 7, 4, 1)
                    }),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 4, 2),
                        new Point(4, 2),
                        new Segment(4, 6, 2, 1),
                        new Point(6, 4),
                        new Segment(6, 7, 4, 1)
                    }),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                // this is essentially the same test, but the baseSequence ends before the intersection 
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 3, 2, -1),
                        new Point(3, 1),
                        new Segment(3, 3.1m, 1, 1),
                        new Point(3.1m, 1.1m),
                        new Segment(3.1m, 3.2m, 1.1m, 1)
                    }),
                    pseudoPeriodStart: 3.1m,
                    pseudoPeriodLength: 0.1m,
                    pseudoPeriodHeight: 0.1m
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 4, 2),
                        new Point(4, 2),
                        new Segment(4, 6, 2, 1),
                        new Point(6, 4),
                        new Segment(6, 7, 4, 1)
                    }),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 3, 2, -1),
                        new Point(3, 1),
                        new Segment(3, 5, 1, 1),
                        new Point(5, 3),
                        new Segment(5, 6, 3, -1),
                        new Point(6, 2),
                        new Segment(6, 8, 2, 1)
                    }),
                    pseudoPeriodStart: 5,
                    pseudoPeriodLength: 3,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 4, 2),
                        new Point(4, 2),
                        new Segment(4, 5, 2, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 3,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 4, 1, 1),
                        new Point(4, 3),
                        new Segment(4, 5, 3, 1)
                    }),
                    pseudoPeriodStart: 4,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        new Segment(3, 4, 2, 1),

                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 2.1m, 1, 1),
                        new Point(2.1m, 1.1m),
                        new Segment(2.1m, 2.2m, 1.1m, 1)
                    }),
                    pseudoPeriodStart: 2.1m,
                    pseudoPeriodLength: 0.1m,
                    pseudoPeriodHeight: 0.1m
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        new Segment(3, 4, 2, 1)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1
                )
            ),
            (
                operand: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        new Segment(2, 4, 1, 1)
                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                ),
                expected: new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        new Segment(0, 2, 0, 1),
                        new Point(2, 2),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        new Segment(3, 4, 2, 1),

                    }),
                    pseudoPeriodStart: 2,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 1
                )
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetDecreasingTestCases))]
    public void ToNonDecreasingTest(Curve operand, Curve expected)
    {
        Assert.False(operand.IsNonDecreasing);
        var result = operand.ToNonDecreasing();
        Assert.True(result.IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }

    [Theory]
    [MemberData(nameof(GetDecreasingTestCases))]
    public void VsMaxConvolution(Curve operand, Curve expected)
    {
        Assert.False(operand.IsNonDecreasing);
        var result = Curve.MaxPlusConvolution(operand, Curve.Zero());
        Assert.True(result.IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }
}