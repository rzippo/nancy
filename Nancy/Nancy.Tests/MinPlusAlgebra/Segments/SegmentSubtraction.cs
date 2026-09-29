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

    public static IEnumerable<object[]> GetSegmentPointTests()
    {
        var testcases = new List<(Segment segment, Point point, Point expected)>
        {
            // s(1) = 4
            (
                segment: new Segment(0, 2, 3, 1),
                point: new Point(1, 1),
                expected: new Point(1, 3)
            ),
            // s(3) = 11, a negative result
            (
                segment: new Segment(2, 5, 10, 1),
                point: new Point(3, 20),
                expected: new Point(3, -9)
            ),
            // equal values
            (
                segment: new Segment(2, 5, 10, 0),
                point: new Point(4, 10),
                expected: new Point(4, 0)
            )
        };

        foreach (var (segment, point, expected) in testcases)
        {
            yield return new object[] { segment, point, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetSegmentPointTests))]
    public void SegmentPoint(Segment segment, Point point, Point expected)
    {
        Assert.Equal(expected, segment - point);
        Assert.Equal(expected, segment.Subtraction(point));
        Assert.Equal(expected, Segment.Subtraction(segment, point));
        Assert.Equal(expected, ((Element) segment).Subtraction(point));
        Assert.Equal(expected, Element.Subtraction(segment, point));
    }

    [Theory]
    [MemberData(nameof(GetSegmentPointTests))]
    public void PointSegment(Segment segment, Point point, Point expected)
    {
        var reversed = new Point(expected.Time, -expected.Value);
        Assert.Equal(reversed, point - segment);
        Assert.Equal(reversed, point.Subtraction(segment));
        Assert.Equal(reversed, Point.Subtraction(point, segment));
        Assert.Equal(reversed, ((Element) point).Subtraction(segment));
        Assert.Equal(reversed, Element.Subtraction(point, segment));
    }

    [Fact]
    public void SegmentPointNoOverlap()
    {
        var segment = new Segment(0, 2, 3, 1);
        Assert.Throws<ArgumentException>(() => segment - new Point(2, 1));
        Assert.Throws<ArgumentException>(() => segment - new Point(5, 1));
    }
}