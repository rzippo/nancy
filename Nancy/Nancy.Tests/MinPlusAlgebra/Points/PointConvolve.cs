using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointConvolution
{
    [Fact]
    public void PointPoint()
    {
        Point first = new Point
        (time: 3, value: 5);

        Point second = new Point
        (time: 2, value: 7);

        Point convolution = first.Convolution(second);

        Assert.Equal(first.Time + second.Time, convolution.Time);
        Assert.Equal(first.Value + second.Value, convolution.Value);
    }

    [Fact]
    public void PointSegment()
    {
        Point point = new Point
        (time: 2, value: 5);

        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 7
        );

        Segment convolution = point.Convolution(segment);

        Assert.Equal(point.Time + segment.StartTime, convolution.StartTime);
        Assert.Equal(point.EndTime + segment.EndTime, convolution.EndTime);

        Assert.Equal(point.Value + segment.ValueAt(6), convolution.ValueAt(6 + point.Time));
    }
}