using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentMax
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

        Assert.Throws<ArgumentException>(() => first.Maximum(second));
    }

    [Fact]
    public void SingleMaxFullOverlap()
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

        IEnumerable<Element> MaxEnumerable = first.Maximum(second);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(2, max.StartTime);
        Assert.Equal(5, max.EndTime);

        Assert.Equal(Rational.Max(first.RightLimitAtStartTime, second.RightLimitAtStartTime), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), max.Slope);
    }

    [Fact]
    public void SingleMaxPartialOverlap()
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

        IEnumerable<Element> MaxEnumerable = first.Maximum(second);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(3, max.StartTime);
        Assert.Equal(5, max.EndTime);

        Assert.Equal(Rational.Max(first.ValueAt(3), second.RightLimitAtStartTime), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), max.Slope);
    }

    [Fact]
    public void SingleMaxPartialOverlapCommutativity()
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

        IEnumerable<Element> MaxEnumerable = second.Maximum(first);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(3, max.StartTime);
        Assert.Equal(5, max.EndTime);

        Assert.Equal(Rational.Max(first.ValueAt(3), second.RightLimitAtStartTime), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), max.Slope);
    }

    [Fact]
    public void SingleMaxIntersectionAtMaxStart()
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

        IEnumerable<Element> MaxEnumerable = first.Maximum(second);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(10, max.StartTime);
        Assert.Equal(15, max.EndTime);

        Assert.Equal(Rational.Max(first.ValueAt(10), second.RightLimitAtStartTime), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), max.Slope);
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

        Element[] maxArray = first.Maximum(second).ToArray();
        Assert.Equal(3, maxArray.Length);

        Segment leftSegment = (Segment) maxArray[0];
        Point splitPoint = (Point) maxArray[1];
        Segment rightSegment = (Segment) maxArray[2];

        Assert.Equal(5, leftSegment.StartTime);
        Assert.Equal(10, leftSegment.EndTime);

        Assert.Equal(Rational.Max(first.RightLimitAtStartTime, second.RightLimitAtStartTime), leftSegment.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), leftSegment.Slope);

        Assert.Equal(10, splitPoint.Time);
        Assert.Equal(first.ValueAt(10), splitPoint.Value);

        Assert.Equal(10, rightSegment.StartTime);
        Assert.Equal(15, rightSegment.EndTime);

        Assert.Equal(second.ValueAt(10), rightSegment.RightLimitAtStartTime);
        Assert.Equal(Rational.Max(first.Slope, second.Slope), rightSegment.Slope);
        Assert.Equal(Rational.Max(first.ValueAt(14), second.ValueAt(14)), rightSegment.ValueAt(14));
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

        IEnumerable<Element> MaxEnumerable = first.Maximum(second);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(6, max.StartTime);
        Assert.Equal(9, max.EndTime);

        Assert.Equal(Rational.Max(first.RightLimitAtStartTime, second.ValueAt(6)), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), max.Slope);
        Assert.Equal(Rational.Max(first.ValueAt(9), second.LeftLimitAtEndTime), max.LeftLimitAtEndTime);
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

        IEnumerable<Element> MaxEnumerable = second.Maximum(first);
        Segment max = (Segment) MaxEnumerable.Single();

        Assert.Equal(6, max.StartTime);
        Assert.Equal(9, max.EndTime);

        Assert.Equal(Rational.Max(first.RightLimitAtStartTime, second.ValueAt(6)), max.RightLimitAtStartTime);
        Assert.Equal(Rational.Min(first.Slope, second.Slope), max.Slope);
        Assert.Equal(Rational.Max(first.ValueAt(9), second.LeftLimitAtEndTime), max.LeftLimitAtEndTime);
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

        Segment max = (Segment) infinite.Maximum(finite).Single();

        Assert.Equal(5, max.StartTime);
        Assert.Equal(10, max.EndTime);
        Assert.Equal(finite.ValueAt(6), max.ValueAt(6));
        Assert.Equal(finite.Slope, max.Slope);
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

        Segment max = (Segment) infinite.Maximum(finite).Single();

        Assert.Equal(5, max.StartTime);
        Assert.Equal(10, max.EndTime);
        Assert.True(max.IsPlusInfinite);
        Assert.Equal(Rational.PlusInfinity, max.ValueAt(5));
        Assert.Equal(Rational.PlusInfinity, max.RightLimitAtStartTime);
        Assert.Equal(Rational.PlusInfinity, max.LeftLimitAtEndTime);
    }

    [Fact]
    public void PlusInfinitySegments_NonRegular()
    {
        Segment infinite = new Segment(
            startTime: 5, 
            endTime: 10,
            rightLimitAtStartTime: -1,
            slope: Rational.PlusInfinity
        );
        Segment finite = new Segment(
            startTime: 0,
            endTime: 15,
            rightLimitAtStartTime: 0,
            slope: 5
        );

        Segment max = (Segment)infinite.Maximum(finite).Single();

        Assert.Equal(5, max.StartTime);
        Assert.Equal(10, max.EndTime);
        Assert.True(max.IsPlusInfinite);
        Assert.Equal(Rational.PlusInfinity, max.ValueAt(5));
        Assert.Equal(Rational.PlusInfinity, max.RightLimitAtStartTime);
        Assert.Equal(Rational.PlusInfinity, max.LeftLimitAtEndTime);
    }
}