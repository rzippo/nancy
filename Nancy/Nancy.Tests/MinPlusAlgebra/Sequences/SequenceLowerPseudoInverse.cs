using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceLowerPseudoInverse
{
    public static IEnumerable<object[]> ContinuousRevertibleTestCases()
    {
        var testcases = new (Sequence operand, Sequence expected)[]
        {
            (
                operand: new Sequence( new Element[]
                {
                    Point.Origin(),
                    Segment.Zero(0, 3),
                    Point.Zero(3),
                    new Segment(3, 5, 0, 2)
                }),
                expected: new Sequence( new Element[]
                {
                    Point.Origin(),
                    new Segment(0, 4, 3, new Rational(1, 2))
                })
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    public static IEnumerable<object[]> DiscontinuousRevertibleTestCases()
    {
        var testcases = new (Sequence operand, Sequence expected)[]
        {
            (
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 2, 1)
                }),
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 1, 0),
                    new Point(2, 1),
                    new Segment(2, 3, 1, 1)
                })
            )
        };

        foreach (var (operand, expected) in testcases)
        {
            yield return new object[] { operand, expected };
        }
    }

    public static IEnumerable<object[]> DiscontinuousNonRevertibleTestCases()
    {
        var testcases = new (Sequence operand, Sequence expected1, Sequence expected2)[]
        {
            (
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 2),
                    new Segment(1, 2, 2, 1)
                }),
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 1, 0),
                    new Point(2, 1),
                    new Segment(2, 3, 1, 1)
                }),
                new Sequence( new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 1, 0, 1),
                    new Point(1, 1),
                    new Segment(1, 2, 2, 1)
                })
            )
        };

        foreach (var (operand, expected1, expected2) in testcases)
        {
            yield return new object[] { operand, expected1, expected2 };
        }
    }

    [Theory]
    [MemberData(nameof(ContinuousRevertibleTestCases))]
    [MemberData(nameof(DiscontinuousRevertibleTestCases))]
    public void RevertibleInverseTest(Sequence operand, Sequence expected)
    {
        var result = operand.LowerPseudoInverse();
        Assert.True(Sequence.Equivalent(expected, result));

        var result2 = result.LowerPseudoInverse();
        Assert.True(Sequence.Equivalent(operand, result2));
    }

    [Theory]
    [MemberData(nameof(DiscontinuousNonRevertibleTestCases))]
    public void NonRevertibleInverseTest(Sequence operand, Sequence expected1, Sequence expected2)
    {
        var result = operand.LowerPseudoInverse();
        Assert.True(Sequence.Equivalent(expected1, result));

        var result2 = result.LowerPseudoInverse();
        Assert.True(Sequence.Equivalent(expected2, result2));
    }
}