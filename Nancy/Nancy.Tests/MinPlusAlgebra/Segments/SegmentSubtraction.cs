using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentSubtraction
{
    [Fact]
    public void NoOverlap()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 10,
            slope: 5,
            startTime: 2,
            endTime: 5
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 8
        );

        Assert.Throws<ArgumentException>(() => first + second);
    }

    public static IEnumerable<object[]> GetSegmentSegmentTests()
    {
        var testcases = new List<(Segment a, Segment b, Segment expected)>
        {
            (
                a: new Segment(2, 5, 10, 5),
                b: new Segment(2, 5, 7, 1),
                expected: new Segment(2, 5, 3, 4)
            ),
            (
                a: new Segment(2, 5, 7, 1),
                b: new Segment(2, 5, 10, 5),
                expected: new Segment(2, 5, -3, -4)
            ),
            (
                a: new Segment(2, 5, 10, 5),
                b: new Segment(3, 6, 7, 1),
                expected: new Segment(3, 5, 8, 4)
            ),
            (
                a: new Segment(3, 6, 7, 1),
                b: new Segment(2, 5, 10, 5),
                expected: new Segment(3, 5, -8, -4)
            ),
            (
                a: new Segment(2, 5, 7, 1),
                b: new Segment(2, 5, 0, 0),
                expected: new Segment(2, 5, 7, 1)
            ),
            (
                a: new Segment(2, 5, 0, 0),
                b: new Segment(2, 5, 7, 1),
                expected: new Segment(2, 5, -7, -1)
            )
        };

        foreach (var (a, b, expected) in testcases)
        {
            yield return new object[] { a, b, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetSegmentSegmentTests))]
    public void SegmentSegment(Segment a, Segment b, Segment expected)
    {
        var diff = a - b;
        Assert.Equal(expected, diff);
    }
}