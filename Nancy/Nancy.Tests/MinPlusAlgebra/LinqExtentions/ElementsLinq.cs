using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.LinqExtentions;

public class ElementsLinq
{
    public static IEnumerable<object[]> GetAreInTimeOrderTestCases()
    {
        var testCases = new (List<Element> elements, bool expected)[]
        {
            (
                elements: new List<Element>
                {
                    Segment.Zero(0, 1),
                    Point.Zero(2),
                    Segment.Zero(2, 3),
                    Point.Zero(3),
                    Segment.Zero(11, 15),
                },
                expected: true
            ),
            (
                elements: new List<Element>
                {
                    Segment.Zero(0, 1),
                    Segment.Zero(5, 9),
                    Segment.Zero(11, 15),
                    Segment.Zero(3, 11),
                    Segment.Zero(13, 15),
                    Segment.Zero(0, 5),
                    Point.Zero(7),
                    Segment.Zero(9, 15)
                },
                expected: false
            ),
        };

        foreach(var testCase in testCases)
        {
            yield return new object[] { testCase.elements, testCase.expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetAreInTimeOrderTestCases))]
    public void AreInTimeOrderTest(List<Element> elements, bool expected)
    {
        Assert.Equal(expected, elements.AreInTimeOrder());
    }

    public static IEnumerable<object[]> GetSortTestCases()
    {
        var testCases = new List<Element>[]
        {
            new List<Element>
            {
                Segment.Zero(0, 1),
                Point.Zero(2),
                Segment.Zero(2, 3),
                Point.Zero(3),
                Segment.Zero(11, 15),
            },
            new List<Element>
            {
                Segment.Zero(0, 1),
                Segment.Zero(5, 9),
                Segment.Zero(11, 15),
                Segment.Zero(3, 11),
                Segment.Zero(13, 15),
                Segment.Zero(0, 5),
                Point.Zero(7),
                Segment.Zero(9, 15)
            }
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] { testCase };
        }
    }

    [Theory]
    [MemberData(nameof(GetSortTestCases))]
    public void SortTest(List<Element> elements)
    {
        //note: to test parallel sorting, we need more than 10_000 elements
        var sorted = elements.SortElements();
        Assert.True(sorted.AreInTimeOrder());
    }
}