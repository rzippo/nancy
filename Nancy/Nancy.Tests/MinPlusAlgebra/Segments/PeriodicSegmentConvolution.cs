using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class PeriodicSegmentConvolution
{
    [Fact]
    public void DisjointSegments_AbovePeriod()
    {
        //slope >= period slope

        Segment segment = new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 5
        );

        Curve curve = segment.PeriodicSegmentConvolution(
            pseudoPeriodHeight: 6,
            pseudoPeriodLength: 12,
            k: 4
        );

        Assert.False(curve.IsFinite);
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(4));

        //First period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(16));
        Assert.Equal(20, curve.RightLimitAt(16));
        Assert.Equal(60 ,curve.LeftLimitAt(24));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(24));

        //Second period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(28));
        Assert.Equal(26, curve.RightLimitAt(28));
        Assert.Equal(66 ,curve.LeftLimitAt(36));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(36));

        //Fourth period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(64));
        Assert.Equal(44, curve.RightLimitAt(64));
        Assert.Equal(84 ,curve.LeftLimitAt(72));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(72));
    }

    [Fact]
    public void DisjointSegments_BelowPeriod()
    {
        //slope < period slope

        Segment segment = new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 1
        );

        Curve curve = segment.PeriodicSegmentConvolution(
            pseudoPeriodHeight: 6,
            pseudoPeriodLength: 12,
            k: 4
        );

        Assert.False(curve.IsFinite);
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(4));

        //First period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(16));
        Assert.Equal(20, curve.RightLimitAt(16));
        Assert.Equal(28 ,curve.LeftLimitAt(24));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(24));

        //Fourth period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(28));
        Assert.Equal(26, curve.RightLimitAt(28));
        Assert.Equal(34 ,curve.LeftLimitAt(36));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(36));

        //Fourth period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(64));
        Assert.Equal(44, curve.RightLimitAt(64));
        Assert.Equal(52 ,curve.LeftLimitAt(72));
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(72));
    }

    [Fact]
    public void OverlappingSegments_AbovePeriod()
    {
        //slope >= period slope

        Segment segment = new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 5
        );

        Curve curve = segment.PeriodicSegmentConvolution(
            pseudoPeriodHeight: 6,
            pseudoPeriodLength: 12,
            k: 8
        );

        Assert.False(curve.IsFinite);
        Assert.True(curve.Cut(33, 158).IsFinite);
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(4));
        Assert.Equal(Rational.PlusInfinity, curve.RightLimitAt(20));

        //First period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(32));
        Assert.Equal(40, curve.RightLimitAt(32));
        Assert.Equal(100, curve.LeftLimitAt(44));
        Assert.Equal(100, curve.ValueAt(44));

        //Second period
        Assert.Equal(46, curve.RightLimitAt(44));
        Assert.Equal(106, curve.LeftLimitAt(56));
        Assert.Equal(106, curve.ValueAt(56));

        //Fourth period
        Assert.Equal(58, curve.RightLimitAt(68));
        Assert.Equal(118, curve.LeftLimitAt(80));
        Assert.Equal(118, curve.ValueAt(80));
    }

    [Fact]
    public void OverlappingSegments_BelowPeriod()
    {
        //slope < period slope

        Segment segment = new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 1
        );

        Curve curve = segment.PeriodicSegmentConvolution(
            pseudoPeriodHeight: 16,
            pseudoPeriodLength: 12,
            k: 8
        );

        Assert.False(curve.IsFinite);
        Assert.True(curve.Cut(33, 158).IsFinite);
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(4));
        Assert.Equal(Rational.PlusInfinity, curve.RightLimitAt(20));

        //First period
        Assert.Equal(Rational.PlusInfinity, curve.ValueAt(32));
        Assert.Equal(40, curve.RightLimitAt(32));
        Assert.Equal(56, curve.LeftLimitAt(48));

        //Second period
        Assert.Equal(60, curve.ValueAt(48));
        Assert.Equal(60, curve.RightLimitAt(48));
        Assert.Equal(72, curve.LeftLimitAt(60));

        //Third period
        Assert.Equal(76, curve.ValueAt(60));
        Assert.Equal(76, curve.RightLimitAt(60));
        Assert.Equal(88, curve.LeftLimitAt(72));

        //Fourth period
        Assert.Equal(92, curve.ValueAt(72));
        Assert.Equal(92, curve.RightLimitAt(72));
        Assert.Equal(104, curve.LeftLimitAt(84));
    }
}