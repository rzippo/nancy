using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointMax
{
    [Fact]
    public void PointsOverlap()
    {
        Point first = new Point(time: 2, value: 5);

        Point second = new Point(time: 2, value: 7);

        Point max = first.Maximum(second);

        Assert.Equal(2, max.StartTime);
        Assert.Equal(2, max.EndTime);
        Assert.Equal(Rational.Max(first.Value, second.Value), max.Value);
    }

    [Fact]
    public void PointsNonOverlap()
    {
        Point first = new Point(time: 3, value: 5);

        Point second = new Point(time: 2, value: 7);

        Assert.Throws<ArgumentException>(() => first.Maximum(second));
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

        Point max = point.Maximum(segment);

        Assert.Equal(2, max.StartTime);
        Assert.Equal(2, max.EndTime);
        Assert.Equal(
            Rational.Max(segment.ValueAt(2), point.Value),
            max.Value);
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

        Point max = infinite.Maximum(finite);

        Assert.Equal(5, max.StartTime);
        Assert.Equal(finite.ValueAt(5), max.ValueAt(5));
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

        Point max = infinite.Maximum(finite);

        Assert.Equal(5, max.StartTime);
        Assert.Equal(Rational.PlusInfinity, max.ValueAt(5));
    }
}