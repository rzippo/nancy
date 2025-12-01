using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Intervals;

public class BaseOperations
{
    [Fact]
    public void Classification()
    {
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);

        Assert.Equal(
            IntervalBucket.OverlapTypes.NoOverlap,
            intervalBucket.Classify(Segment.Zero(11, 20)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.SegmentSupportContainsInterval,
            intervalBucket.Classify(Segment.Zero(1, 10)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.SegmentSupportContainsInterval,
            intervalBucket.Classify(Segment.Zero(0, 12)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.SegmentFullyContained,
            intervalBucket.Classify(Segment.Zero(2, 8)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.SegmentStartContained,
            intervalBucket.Classify(Segment.Zero(4, 12)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.SegmentEndContained,
            intervalBucket.Classify(Segment.Zero(0, 9)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.PointInside,
            intervalBucket.Classify(Point.Zero(2)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.NoOverlap,
            intervalBucket.Classify(Point.Zero(1)));
    }

    [Fact]
    public void PointIntervalClassification()
    {
        IntervalBucket intervalBucket = new IntervalBucket(10);

        Assert.Equal(
            IntervalBucket.OverlapTypes.NoOverlap,
            intervalBucket.Classify(Segment.Zero(11, 20)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.NoOverlap,
            intervalBucket.Classify(Segment.Zero(10, 20)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.NoOverlap,
            intervalBucket.Classify(Segment.Zero(0, 10)));

        Assert.Equal(
            IntervalBucket.OverlapTypes.PointInside,
            intervalBucket.Classify(Point.Zero(10)));
    }

    [Fact]
    public void SplitMiddle()
    {
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        IntervalBucket[] split = intervalBucket.SplitOver(Point.Zero(5)).ToArray();

        Assert.Equal(3, split.Length);
        Assert.Equal(2, split[0].Count);
        Assert.Equal(3, split[1].Count);
        Assert.Equal(2, split[2].Count);
    }

    [Fact]
    public void SplitLeftHalf()
    {
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        IntervalBucket[] split = intervalBucket.SplitOver(Segment.Zero(0, 5)).ToArray();

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
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        IntervalBucket[] split = intervalBucket.SplitOver(Segment.Zero(5, 13)).ToArray();

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
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        IntervalBucket[] split = intervalBucket.SplitOver(Segment.Zero(1, 5)).ToArray();

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
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        IntervalBucket[] split = intervalBucket.SplitOver(Segment.Zero(5, 10)).ToArray();

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
        IntervalBucket intervalBucket = new IntervalBucket(1, 10);
        intervalBucket.Add(Segment.Zero(1, 10));
        intervalBucket.Add(Segment.Zero(0, 12));

        Assert.ThrowsAny<Exception>(() => intervalBucket.SplitOver(Point.Zero(1)));
    }
}