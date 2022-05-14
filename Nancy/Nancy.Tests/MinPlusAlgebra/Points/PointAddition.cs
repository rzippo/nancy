using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointAddition
{
    [Fact]
    public void PointsOverlap()
    {
        Point first = new Point(time: 2, value: 5);

        Point second = new Point(time: 2, value: 7);

        Point sum = first + second;

        Assert.Equal(2, sum.StartTime);
        Assert.Equal(2, sum.EndTime);
        Assert.Equal(12, sum.Value);
    }

    [Fact]
    public void PointsOverlap_Generic()
    {
        Element first = new Point(time: 2, value: 5);

        Element second = new Point(time: 2, value: 7);

        Element sumElement = first + second;

        Assert.IsType<Point>(sumElement);
        var sum = (Point) sumElement;

        Assert.Equal(2, sum.StartTime);
        Assert.Equal(2, sum.EndTime);
        Assert.Equal(12, sum.Value);
    }


    [Fact]
    public void PointsNonOverlap()
    {
        Point first = new Point(time: 3, value: 5);

        Point second = new Point(time: 2, value: 7);

        Assert.Throws<ArgumentException>(() => first + second);
    }

    [Fact]
    public void PointAndSegment()
    {
        Point point = new Point(time: 2, value: 5);

        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 1,
            endTime: 3
        );

        Point sum = point + segment;

        Assert.Equal(2, sum.Time);
        Assert.Equal(segment.ValueAt(2) + point.Value, sum.Value);
    }

    [Fact]
    public void PointAndSegment_Generic()
    {
        Element point = new Point(time: 2, value: 5);

        Element segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 1,
            endTime: 3
        );

        Element sumElement = point + segment;

        Assert.IsType<Point>(sumElement);
        var sum = (Point) sumElement;

        Assert.Equal(2, sum.Time);
        Assert.Equal(segment.ValueAt(2) + ((Point) point).Value, sum.Value);
    }

    [Fact]
    public void InfiniteAdd_Point()
    {
        Point infinite = Point.PlusInfinite(5);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Point sum = infinite.Addition(finite);

        Assert.Equal(5, sum.StartTime);
        Assert.Equal(Rational.PlusInfinity, sum.ValueAt(5));
    }
}