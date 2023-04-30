using System;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointMin
{
    [Fact]
    public void PointsOverlap()
    {
        Point first = new Point(time: 2, value: 5);

        Point second = new Point(time: 2, value: 7);

        Point min = first.Minimum(second);

        Assert.Equal(2, min.StartTime);
        Assert.Equal(2, min.EndTime);
        Assert.Equal(Rational.Min(first.Value, second.Value), min.Value);
    }

    [Fact]
    public void PointsOverlap_asElement()
    {
        Point first = new Point(time: 2, value: 5);

        Point second = new Point(time: 2, value: 7);

        var min = first.Minimum(second as Element);
        Assert.Single(min);
        var minPoint = (Point) min.Single();

        Assert.Equal(2, minPoint.StartTime);
        Assert.Equal(2, minPoint.EndTime);
        Assert.Equal(Rational.Min(first.Value, second.Value), minPoint.Value);
    }

    [Fact]
    public void PointsOverlap_asList()
    {
        Point first = new Point(time: 2, value: 5);

        Point second = new Point(time: 2, value: 7);

        Point min = Point.Minimum(new []{first, second});

        Assert.Equal(2, min.StartTime);
        Assert.Equal(2, min.EndTime);
        Assert.Equal(Rational.Min(first.Value, second.Value), min.Value);
    }

    [Fact]
    public void PointsNonOverlap()
    {
        Point first = new Point(time: 3, value: 5);

        Point second = new Point(time: 2, value: 7);

        Assert.Throws<ArgumentException>(() => first.Minimum(second));
    }

    [Fact]
    public void PointsNonOverlap_asList()
    {
        Point first = new Point(time: 3, value: 5);

        Point second = new Point(time: 2, value: 7);

        Assert.Throws<ArgumentException>(() => Point.Minimum(new []{first, second}));
    }

    [Fact]
    public void Points_SingleList()
    {
        var point = new Point(time: 2, value: 5);

        var min = Point.Minimum(new[] {point});
        Assert.Equal(point, min);
    }

    [Fact]
    public void Points_EmptyList()
    {
        Assert.Throws<InvalidOperationException>(() => Point.Minimum(new Point[] { }));
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

        Point min = point.Minimum(segment);

        Assert.Equal(2, min.Time);
        Assert.Equal(
            Rational.Min(segment.ValueAt(2), point.Value),
            min.Value);
    }

    [Fact]
    public void PointAndSegment_asElement()
    {
        Point point = new Point(time: 2, value: 5);

        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 1,
            endTime: 3
        );

        var min = point.Minimum(segment as Element);
        Assert.Single(min);
        var minPoint = (Point) min.Single();

        Assert.Equal(2, minPoint.Time);
        Assert.Equal(
            Rational.Min(segment.ValueAt(2), point.Value),
            minPoint.Value);
    }

    [Fact]
    public void PlusInfinityPoint()
    {
        Point infinite = Point.PlusInfinite(5);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Point min = infinite.Minimum(finite);

        Assert.Equal(5, min.StartTime);
        Assert.Equal(finite.ValueAt(5), min.ValueAt(5));
    }

    [Fact]
    public void MinusInfinityPoint()
    {
        Point infinite = Point.MinusInfinite(5);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Point min = infinite.Minimum(finite);

        Assert.Equal(5, min.StartTime);
        Assert.Equal(Rational.MinusInfinity, min.ValueAt(5));
    }
}