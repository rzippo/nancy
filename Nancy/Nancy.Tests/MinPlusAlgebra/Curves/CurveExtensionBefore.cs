using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveExtensionBefore
{
    [Fact]
    public void StaircaseMidPoints()
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

        Assert.Equal(0, curve.LeftLimitAt(2));
        Assert.Equal(11, curve.LeftLimitAt(8));
        Assert.Equal(5, curve.LeftLimitAt(4));
        Assert.Equal(8, curve.LeftLimitAt(6));           
    }

    [Fact]
    public void StaircaseEndPoints()
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

        Assert.Throws<ArgumentException>(() => curve.LeftLimitAt(0));
        Assert.Throws<ArgumentException>(() => curve.GetSegmentBefore(0));

        Assert.Equal(0, curve.LeftLimitAt(3));
        Assert.Equal(11, curve.LeftLimitAt(9));
        Assert.Equal(5, curve.LeftLimitAt(5));
        Assert.Equal(8, curve.LeftLimitAt(7));
    }

    //todo: add also InputBuffer equivalent
}