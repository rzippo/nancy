using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentMin
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

        Assert.Throws<ArgumentException>(() => first.Minimum(second));
    }

    [Fact]
    public void SingleMinFullOverlap()
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

        var minEnumerable = first.Minimum(second);
        Assert.Single(minEnumerable);

        var minElement = minEnumerable.Single();
        Assert.IsType<Segment>(minElement);
        var min = (Segment) minElement;

        Assert.Equal(2, min.StartTime);
        Assert.Equal(5, min.EndTime);

        Assert.Equal(Rational.Min(first.RightLimitAtStartTime, second.RightLimitAtStartTime), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), min.Slope);
    }

    [Fact]
    public void SingleMinPartialOverlap()
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

        var minEnumerable = first.Minimum(second);
        Assert.Single(minEnumerable);

        var minElement = minEnumerable.Single();
        Assert.IsType<Segment>(minElement);
        var min = (Segment) minElement;


        Assert.Equal(3, min.StartTime);
        Assert.Equal(5, min.EndTime);

        Assert.Equal(Rational.Min(first.ValueAt(3), second.RightLimitAtStartTime), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), min.Slope);
    }

    [Fact]
    public void SingleMinPartialOverlapCommutativity()
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

        var minEnumerable = first.Minimum(second);
        Assert.Single(minEnumerable);

        var minElement = minEnumerable.Single();
        Assert.IsType<Segment>(minElement);
        var min = (Segment) minElement;


        Assert.Equal(3, min.StartTime);
        Assert.Equal(5, min.EndTime);

        Assert.Equal(Rational.Min(first.ValueAt(3), second.RightLimitAtStartTime), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), min.Slope);
    }

    [Fact]
    public void SingleMinIntersectionAtMinStart()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 25,
            slope: 5,
            startTime: 5,
            endTime: 15
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 50,
            slope: 3,
            startTime: 10,
            endTime: 15
        );

        var minEnumerable = first.Minimum(second);
        Assert.Single(minEnumerable);

        var minElement = minEnumerable.Single();
        Assert.IsType<Segment>(minElement);
        var min = (Segment) minElement;

        Assert.Equal(10, min.StartTime);
        Assert.Equal(15, min.EndTime);

        Assert.Equal(Rational.Min(first.ValueAt(10), second.RightLimitAtStartTime), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), min.Slope);
    }

    [Fact]
    public void IntersectionInsideBoundsFullOverlap()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 25,
            slope: 5,
            startTime: 5,
            endTime: 15
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 0,
            slope: 10,
            startTime: 5,
            endTime: 15
        );

        Element[] minArray = first.Minimum(second).ToArray();
        Assert.Equal(3, minArray.Length);

        Segment firstMinSegment = (Segment) minArray[0];
        Point intersectionPoint = (Point)minArray[1];
        Segment secondSegment = (Segment) minArray[2];

        Assert.Equal(5, firstMinSegment.StartTime);
        Assert.Equal(10, firstMinSegment.EndTime);

        Assert.Equal(Rational.Min(first.RightLimitAtStartTime, second.RightLimitAtStartTime), firstMinSegment.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), firstMinSegment.Slope);

        Assert.Equal(10, intersectionPoint.Time);
        Assert.Equal(first.ValueAt(10), intersectionPoint.Value);

        Assert.Equal(10, secondSegment.StartTime);
        Assert.Equal(15, secondSegment.EndTime);

        Assert.Equal(second.ValueAt(10), secondSegment.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), secondSegment.Slope);
        Assert.Equal(Rational.Min(first.ValueAt(14), second.ValueAt(14)), secondSegment.ValueAt(14));
    }

    [Fact]
    public void IntersectionOutsideBoundsPartialOverlap()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 30,
            slope: 5,
            startTime: 6,
            endTime: 15
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 0,
            slope: 10,
            startTime: 5,
            endTime: 9
        );

        var minEnumerable = first.Minimum(second);
        Assert.Single(minEnumerable);

        var minElement = minEnumerable.Single();
        Assert.IsType<Segment>(minElement);
        var min = (Segment) minElement;

        Assert.Equal(6, min.StartTime);
        Assert.Equal(9, min.EndTime);

        Assert.Equal(Rational.Min(first.RightLimitAtStartTime, second.ValueAt(6)), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), min.Slope);
        Assert.Equal(Rational.Min(first.ValueAt(9), second.LeftLimitAtEndTime), min.LeftLimitAtEndTime);
    }

    [Fact]
    public void IntersectionOutsideBoundsPartialOverlapCommutativity()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 30,
            slope: 5,
            startTime: 6,
            endTime: 15
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 0,
            slope: 10,
            startTime: 5,
            endTime: 9
        );

        IEnumerable<Element> minEnumerable = first.Minimum(second);
        Segment min = (Segment)minEnumerable.Single();

        Assert.Equal(6, min.StartTime);
        Assert.Equal(9, min.EndTime);

        Assert.Equal(Rational.Min(first.RightLimitAtStartTime, second.ValueAt(6)), min.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), min.Slope);
        Assert.Equal(Rational.Min(first.ValueAt(9), second.LeftLimitAtEndTime), min.LeftLimitAtEndTime);
    }

    [Fact]
    public void PlusInfinitySegments()
    {
        Segment infinite = Segment.PlusInfinite(5, 10);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Segment min = (Segment) infinite.Minimum(finite).Single();

        Assert.Equal(5, min.StartTime);
        Assert.Equal(10, min.EndTime);
        Assert.Equal(finite.ValueAt(6), min.ValueAt(6));
        Assert.Equal(finite.Slope, min.Slope);
    }

    [Fact]
    public void MinusInfinitySegments()
    {
        Segment infinite = Segment.MinusInfinite(5, 10);
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Segment min = (Segment)infinite.Minimum(finite).Single();

        Assert.Equal(5, min.StartTime);
        Assert.Equal(10, min.EndTime);
        Assert.True(min.IsMinusInfinite);
        Assert.Equal(Rational.MinusInfinity, min.ValueAt(6));
        Assert.Equal(Rational.MinusInfinity, min.RightLimitAtStartTime);
        Assert.Equal(Rational.MinusInfinity, min.LeftLimitAtEndTime);
    }
}