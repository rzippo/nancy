using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class BaseOperations
{
    [Fact]
    public void Classification()
    {
        Interval interval = new Interval(1, 10);

        Assert.Equal(
            Interval.OverlapTypes.NoOverlap,
            interval.Classify(Segment.Zero(11, 20)));

        Assert.Equal(
            Interval.OverlapTypes.SegmentSupportContainsInterval,
            interval.Classify(Segment.Zero(1, 10)));

        Assert.Equal(
            Interval.OverlapTypes.SegmentSupportContainsInterval,
            interval.Classify(Segment.Zero(0, 12)));

        Assert.Equal(
            Interval.OverlapTypes.SegmentFullyContained,
            interval.Classify(Segment.Zero(2, 8)));

        Assert.Equal(
            Interval.OverlapTypes.SegmentStartContained,
            interval.Classify(Segment.Zero(4, 12)));

        Assert.Equal(
            Interval.OverlapTypes.SegmentEndContained,
            interval.Classify(Segment.Zero(0, 9)));

        Assert.Equal(
            Interval.OverlapTypes.PointInside,
            interval.Classify(Point.Zero(2)));

        Assert.Equal(
            Interval.OverlapTypes.NoOverlap,
            interval.Classify(Point.Zero(1)));
    }

    [Fact]
    public void PointIntervalClassification()
    {
        Interval interval = new Interval(10);

        Assert.Equal(
            Interval.OverlapTypes.NoOverlap,
            interval.Classify(Segment.Zero(11, 20)));

        Assert.Equal(
            Interval.OverlapTypes.NoOverlap,
            interval.Classify(Segment.Zero(10, 20)));

        Assert.Equal(
            Interval.OverlapTypes.NoOverlap,
            interval.Classify(Segment.Zero(0, 10)));

        Assert.Equal(
            Interval.OverlapTypes.PointInside,
            interval.Classify(Point.Zero(10)));
    }

    [Fact]
    public void SplitMiddle()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Interval[] split = interval.SplitOver(Point.Zero(5)).ToArray();

        Assert.Equal(3, split.Length);
        Assert.Equal(2, split[0].Count);
        Assert.Equal(3, split[1].Count);
        Assert.Equal(2, split[2].Count);
    }

    [Fact]
    public void SplitLeftHalf()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Interval[] split = interval.SplitOver(Segment.Zero(0, 5)).ToArray();

        Assert.Equal(3, split.Length);

        Assert.True(split[0].IsSegmentInterval);
        Assert.Equal(3, split[0].Count);

        Assert.True(split[1].IsPointInterval);
        Assert.Equal(2, split[1].Count);

        Assert.True(split[2].IsSegmentInterval);
        Assert.Equal(2, split[2].Count);
    }

    [Fact]
    public void SplitRightHalf()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Interval[] split = interval.SplitOver(Segment.Zero(5, 13)).ToArray();

        Assert.Equal(3, split.Length);

        Assert.True(split[0].IsSegmentInterval);
        Assert.Equal(2, split[0].Count);

        Assert.True(split[1].IsPointInterval);
        Assert.Equal(2, split[1].Count);

        Assert.True(split[2].IsSegmentInterval);
        Assert.Equal(3, split[2].Count);
    }

    [Fact]
    public void SplitStartToMiddle()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Interval[] split = interval.SplitOver(Segment.Zero(1, 5)).ToArray();

        Assert.Equal(3, split.Length);

        Assert.True(split[0].IsSegmentInterval);
        Assert.Equal(3, split[0].Count);

        Assert.True(split[1].IsPointInterval);
        Assert.Equal(2, split[1].Count);

        Assert.True(split[2].IsSegmentInterval);
        Assert.Equal(2, split[2].Count);
    }

    [Fact]
    public void SplitMiddleToEnd()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Interval[] split = interval.SplitOver(Segment.Zero(5, 10)).ToArray();

        Assert.Equal(3, split.Length);

        Assert.True(split[0].IsSegmentInterval);
        Assert.Equal(2, split[0].Count);

        Assert.True(split[1].IsPointInterval);
        Assert.Equal(2, split[1].Count);

        Assert.True(split[2].IsSegmentInterval);
        Assert.Equal(3, split[2].Count);
    }

    [Fact]
    public void SplitPointAtStart()
    {
        Interval interval = new Interval(1, 10);
        interval.Add(Segment.Zero(1, 10));
        interval.Add(Segment.Zero(0, 12));

        Assert.ThrowsAny<Exception>(() => interval.SplitOver(Point.Zero(1)));
    }
}