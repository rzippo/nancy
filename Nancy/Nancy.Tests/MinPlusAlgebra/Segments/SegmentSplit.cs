using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentSplit
{
    private readonly Segment testSegment = new Segment
    (
        rightLimitAtStartTime: 10,
        slope: 5,
        startTime: 2,
        endTime: 5
    );

    [Fact]
    public void ValidSplit()
    {
        var (leftSegment, point, rightSegment) = testSegment.Split(4);

        Assert.Equal(2, leftSegment.StartTime);
        Assert.Equal(4, leftSegment.EndTime);

        Assert.Equal(4, point.Time);

        Assert.Equal(4, rightSegment.StartTime);
        Assert.Equal(5, rightSegment.EndTime);

        Assert.Equal(20, rightSegment.RightLimitAtStartTime);
    }

    [Fact]
    public void OutsideDefinitionBounds()
    {
        Assert.Throws<ArgumentException>(() => testSegment.Split(testSegment.EndTime + 1));
    }

    [Fact]
    public void SplitAtStartTime()
    {
        Assert.Throws<ArgumentException>(() => testSegment.Split(testSegment.StartTime));
    }

    [Fact]
    public void SplitAtEndTime()
    {
        Assert.Throws<ArgumentException>(() => testSegment.Split(testSegment.EndTime));
    }
}