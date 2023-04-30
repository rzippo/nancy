using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentAddition
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

    [Fact]
    public void FullOverlap()
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
            startTime: 2,
            endTime: 5
        );

        Segment sum = first + second;

        Assert.Equal(2, sum.StartTime);
        Assert.Equal(5, sum.EndTime);

        Assert.Equal(17, sum.RightLimitAtStartTime);
        Assert.Equal(6, sum.Slope);

        Assert.Equal(
            first.ValueAt(4) + second.ValueAt(4),
            sum.ValueAt(4));
    }

    [Fact]
    public void FullOverlap_Generic()
    {
        Element first = new Segment
        (
            rightLimitAtStartTime: 10,
            slope: 5,
            startTime: 2,
            endTime: 5
        );

        Element second = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 2,
            endTime: 5
        );

        Element sumElement = first + second;

        Assert.IsType<Segment>(sumElement);
        var sum = (Segment) sumElement;

        Assert.Equal(2, sum.StartTime);
        Assert.Equal(5, sum.EndTime);

        Assert.Equal(17, sum.RightLimitAtStartTime);
        Assert.Equal(6, sum.Slope);

        Assert.Equal(
            first.ValueAt(4) + second.ValueAt(4),
            sum.ValueAt(4));
    }

    [Fact]
    public void PartialOverlap()
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
            startTime: 3,
            endTime: 6
        );

        Segment sum = first + second;

        Assert.Equal(3, sum.StartTime);
        Assert.Equal(5, sum.EndTime);

        Assert.Equal(22, sum.RightLimitAtStartTime);
        Assert.Equal(6, sum.Slope);

        Assert.Equal(
            first.ValueAt(4) + second.ValueAt(4),
            sum.ValueAt(4));
    }

    [Fact]
    public void PartialOverlapCommutativity()
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
            startTime: 3,
            endTime: 6
        );

        Segment sum = second + first;

        Assert.Equal(3, sum.StartTime);
        Assert.Equal(5, sum.EndTime);

        Assert.Equal(22, sum.RightLimitAtStartTime);
        Assert.Equal(6, sum.Slope);

        Assert.Equal(
            first.ValueAt(4) + second.ValueAt(4),
            sum.ValueAt(4));
    }

    [Fact]
    public void SegmentAndPoint()
    {
        Point point = new Point(time: 2, value: 5);

        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 1,
            endTime: 3
        );

        Point sum = segment + point;

        Assert.Equal(2, sum.Time);
        Assert.Equal(segment.ValueAt(2) + point.Value, sum.Value);
    }

    [Fact]
    public void SegmentAndPoint_Generic()
    {
        Element point = new Point(time: 2, value: 5);

        Element segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 1,
            endTime: 3
        );

        Element sumElement = segment + point;

        Assert.IsType<Point>(sumElement);
        var sum = (Point) sumElement;

        Assert.Equal(2, sum.Time);
        Assert.Equal(segment.ValueAt(2) + ((Point) point).Value, sum.Value);
    }

    [Fact]
    public void InfiniteAdd_Segments()
    {
        Segment infinite = Segment.PlusInfinite(5, 10);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Segment sum = infinite.Addition(finite);

        Assert.Equal(5, sum.StartTime);
        Assert.Equal(10, sum.EndTime);
        Assert.Equal(Rational.PlusInfinity, sum.ValueAt(5));
        Assert.True(sum.IsPlusInfinite);
    }

    [Fact]
    public void WithZero()
    {
        Segment segment = new Segment
        (
            rightLimitAtStartTime: 10,
            slope: 5,
            startTime: 2,
            endTime: 5
        );

        Segment zero = Segment.Zero(2, 5);

        var sum1 = segment + zero;
        Assert.Equal(segment, sum1);

        var sum2 = zero + segment;
        Assert.Equal(segment, sum2);
    }
}