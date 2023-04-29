using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceSkipUntilValue
{
    public static IEnumerable<object[]> TestCases()
    {
        var testcases = new List<(Sequence operand, Rational value, Sequence expected)>
        {
            (
                operand: new Sequence(new List<Element>
                {
                    new Point(0, 0),
                    new Segment(0, 4, 0, 0),
                    new Point(4, 0),
                    new Segment(4, 5, 0, 2),
                    new Point(5, 2),
                }),
                value: 0,
                expected: new Sequence(new List<Element>
                {
                    new Point(0, 0),
                    new Segment(0, 4, 0, 0),
                    new Point(4, 0),
                    new Segment(4, 5, 0, 2),
                    new Point(5, 2),
                })
            )
        };

        foreach (var (operand, value, expected) in testcases)
        {
            yield return new object[] {operand, value, expected};
        }
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void SkipUntilValueEquivalence(Sequence operand, Rational value, Sequence expected)
    {
        var resultElements = operand.Elements.SkipUntilValue(value).ToList();
        var result = resultElements.ToSequence();
        Assert.True(Sequence.Equivalent(expected, result));
    }
}