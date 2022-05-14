using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveExtensionAt
{
    [Fact]
    public void StaircaseMidPoints()
    {
        var function = new Curve(
            baseSequence: new Sequence(new Element[] 
            {
                Point.Origin(),
                new Segment
                (
                    rightLimitAtStartTime : 0,
                    slope : 0,
                    startTime : 0,
                    endTime : 3
                ),
                new Point(time: 3, value: 5), 
                new Segment
                (
                    rightLimitAtStartTime : 5,
                    slope : 0,
                    startTime : 3,
                    endTime : 5
                )
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 3
        );

        Assert.Equal(0, function.ValueAt(2));
        Assert.Equal(5, function.ValueAt(4));
        Assert.Equal(8, function.ValueAt(6));
        Assert.Equal(11, function.ValueAt(8));
    }

    [Fact]
    public void StaircaseStartPoints()
    {
        var curve = new Curve(
            baseSequence: new Sequence(new Element[] 
            {
                Point.Origin(), 
                new Segment
                (
                    rightLimitAtStartTime : 0,
                    slope : 0,
                    startTime : 0,
                    endTime : 3
                ),
                new Point(time: 3, value: 5), 
                new Segment
                (
                    rightLimitAtStartTime : 5,
                    slope : 0,
                    startTime : 3,
                    endTime : 5
                )
            }),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 3
        );


        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(5, curve.ValueAt(3));
        Assert.Equal(8, curve.ValueAt(5));
        Assert.Equal(11, curve.ValueAt(7));
    }

    [Fact]
    public void InputBuffer()
    {
        var curve = new Curve(
            baseSequence: new Sequence(new Element[] 
            {
                Point.Origin(), 
                new Segment
                (
                    rightLimitAtStartTime : 5,
                    slope : 0,
                    startTime : 0,
                    endTime : 5
                ),
                new Point(time: 5, value: 5), 
                new Segment
                (
                    rightLimitAtStartTime: 5,
                    slope: 1,
                    startTime : 5,
                    endTime : 10
                ),
                new Point(time: 10, value: 10), 
                new Segment
                (
                    rightLimitAtStartTime : 10,
                    slope : 0,
                    startTime : 10,
                    endTime : 15
                )
            }),
            pseudoPeriodStart: 5,
            pseudoPeriodLength: 10,
            pseudoPeriodHeight: 5
        );

        Assert.Equal(0, curve.ValueAt(0));
        Assert.Equal(5, curve.RightLimitAt(0));
        Assert.Equal(5, curve.LeftLimitAt(5));

        Assert.Equal(5, curve.ValueAt(5));
        Assert.Equal(7, curve.ValueAt(7));
        Assert.Equal(10, curve.ValueAt(12));

        Assert.Equal(12, curve.ValueAt(17));
        Assert.Equal(15, curve.ValueAt(22));

        Assert.Equal(22, curve.ValueAt(37));
        Assert.Equal(25, curve.ValueAt(42));
    }
}