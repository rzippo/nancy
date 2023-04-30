using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class PeriodicSegmentClosure
{
    [Fact]
    public void TypeA_1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));

        Assert.Equal(3, closure.LeftLimitAt(2));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2));
        Assert.Equal(2, closure.RightLimitAt(2));

        Assert.Equal(5, closure.LeftLimitAt(4));
        Assert.Equal(5, closure.ValueAt(4));
        Assert.Equal(4, closure.RightLimitAt(4));

        Assert.Equal(9, closure.LeftLimitAt(8));
        Assert.Equal(9, closure.ValueAt(8));
        Assert.Equal(8, closure.RightLimitAt(8));
    }

    [Fact]
    public void TypeA_2()
    {
        Segment segment = new Segment(
            startTime: 5,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 2
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(5, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(5));
        Assert.Equal(5, closure.RightLimitAt(5));

        Assert.Equal(7, closure.LeftLimitAt(6));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(6));
        Assert.Equal(Rational.PlusInfinity, closure.RightLimitAt(6));

        Assert.Equal(13, closure.LeftLimitAt(10));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(10));
        Assert.Equal(10, closure.RightLimitAt(10));

        Assert.Equal(18, closure.LeftLimitAt(15));
        Assert.Equal(18, closure.ValueAt(15));
        Assert.Equal(15, closure.RightLimitAt(15));

        Assert.Equal(38, closure.LeftLimitAt(34));
        Assert.Equal(38, closure.ValueAt(34));
        Assert.Equal(36, closure.RightLimitAt(34));
    }

    [Fact]
    public void TypeB_1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 2,
            slope: 1
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 3
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(2, closure.RightLimitAt(1));

        Assert.Equal(3, closure.LeftLimitAt(2));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2));
        Assert.Equal(4, closure.RightLimitAt(2));

        Assert.Equal(6, closure.LeftLimitAt(4));
        Assert.Equal(7, closure.ValueAt(4));
        Assert.Equal(7, closure.RightLimitAt(4));

        Assert.Equal(9, closure.LeftLimitAt(8));
        Assert.Equal(10, closure.ValueAt(8));
        Assert.Equal(10, closure.RightLimitAt(8));

        //reference period
        Assert.Equal(14, closure.LeftLimitAt(13));
        Assert.Equal(14, closure.ValueAt(13));
        Assert.Equal(11, closure.RightLimitAt(13));

        Assert.Equal(12, closure.LeftLimitAt(14));
        Assert.Equal(16, closure.ValueAt(14));
        Assert.Equal(13, closure.RightLimitAt(14));

        Assert.Equal(14, closure.LeftLimitAt(15));
        Assert.Equal(14, closure.ValueAt(15));
        Assert.Equal(14, closure.RightLimitAt(15));

        Assert.Equal(15, closure.LeftLimitAt(16));
        Assert.Equal(16, closure.ValueAt(16));
        Assert.Equal(16, closure.RightLimitAt(16));

        Assert.Equal(17, closure.LeftLimitAt(17));
        Assert.Equal(17, closure.ValueAt(17));
        Assert.Equal(14, closure.RightLimitAt(17));
    }

    [Fact]
    public void TypeB_2()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 4
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));

        Assert.Equal(3, closure.LeftLimitAt(2));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2));
        Assert.Equal(2, closure.RightLimitAt(2));

        //reference period
        Assert.Equal(14, closure.LeftLimitAt(13));
        Assert.Equal(14, closure.ValueAt(13));
        Assert.Equal(13, closure.RightLimitAt(13));

        Assert.Equal(15, closure.LeftLimitAt(14));
        Assert.Equal(15, closure.ValueAt(14));
        Assert.Equal(14, closure.RightLimitAt(14));

        Assert.Equal(16, closure.LeftLimitAt(15));
        Assert.Equal(16, closure.ValueAt(15));
        Assert.Equal(15, closure.RightLimitAt(15));

        Assert.Equal(17, closure.LeftLimitAt(16));
        Assert.Equal(17, closure.ValueAt(16));
        Assert.Equal(16, closure.RightLimitAt(16));

        Assert.Equal(18, closure.LeftLimitAt(17));
        Assert.Equal(18, closure.ValueAt(17));
        Assert.Equal(17, closure.RightLimitAt(17));
    }

    [Fact]
    public void TypeC_1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 1
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodHeight: 6,
            pseudoPeriodLength: 4
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);
        Assert.True(closure.Cut(3, 20).IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));

        Assert.Equal(2, closure.LeftLimitAt(2));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(2));
        Assert.Equal(2, closure.RightLimitAt(2));

        Assert.Equal(4, closure.LeftLimitAt(4));
        Assert.Equal(4, closure.ValueAt(4));
        Assert.Equal(4, closure.RightLimitAt(4));

        //reference period
        Assert.Equal(13, closure.LeftLimitAt(13));
        Assert.Equal(13, closure.ValueAt(13));
        Assert.Equal(13, closure.RightLimitAt(13));

        Assert.Equal(14, closure.LeftLimitAt(14));
        Assert.Equal(14, closure.ValueAt(14));
        Assert.Equal(14, closure.RightLimitAt(14));

        Assert.Equal(15, closure.LeftLimitAt(15));
        Assert.Equal(15, closure.ValueAt(15));
        Assert.Equal(15, closure.RightLimitAt(15));

        Assert.Equal(16, closure.LeftLimitAt(16));
        Assert.Equal(16, closure.ValueAt(16));
        Assert.Equal(16, closure.RightLimitAt(16));

        Assert.Equal(17, closure.LeftLimitAt(17));
        Assert.Equal(17, closure.ValueAt(17));
        Assert.Equal(17, closure.RightLimitAt(17));
    }

    [Fact]
    public void TypeC_2()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 0.5m
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodHeight: 4,
            pseudoPeriodLength: 4
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));

        Assert.Equal(2, closure.LeftLimitAt(3));
        Assert.Equal(2.5m, closure.ValueAt(3));
        Assert.Equal(2.5m, closure.RightLimitAt(3));

        Assert.Equal(3, closure.LeftLimitAt(4));
        Assert.Equal(3, closure.ValueAt(4));
        Assert.Equal(3, closure.RightLimitAt(4));

        //reference period
        Assert.Equal(9, closure.LeftLimitAt(13));
        Assert.Equal(9, closure.ValueAt(13));
        Assert.Equal(9, closure.RightLimitAt(13));

        Assert.Equal(9.5m, closure.LeftLimitAt(14));
        Assert.Equal(9.5m, closure.ValueAt(14));
        Assert.Equal(9.5m, closure.RightLimitAt(14));

        Assert.Equal(10, closure.LeftLimitAt(15));
        Assert.Equal(10.5m, closure.ValueAt(15));
        Assert.Equal(10.5m, closure.RightLimitAt(15));

        Assert.Equal(11, closure.LeftLimitAt(16));
        Assert.Equal(11, closure.ValueAt(16));
        Assert.Equal(11, closure.RightLimitAt(16));

        Assert.Equal(11.5m, closure.LeftLimitAt(17));
        Assert.Equal(11.5m, closure.ValueAt(17));
        Assert.Equal(11.5m, closure.RightLimitAt(17));
    }

    [Fact]
    public void TypeD_1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 3,
            rightLimitAtStartTime: 2,
            slope: 0.5m
        );

        Curve closure = segment.SubAdditiveClosure(
            pseudoPeriodHeight: 4,
            pseudoPeriodLength: 6
        );

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(1, closure.FirstFiniteTimeExceptOrigin);
        Assert.False(closure.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(2, closure.RightLimitAt(1));

        Assert.Equal(3, closure.LeftLimitAt(3));
        Assert.Equal(4.5m, closure.ValueAt(3));
        Assert.Equal(4.5m, closure.RightLimitAt(3));

        Assert.Equal(5, closure.LeftLimitAt(4));
        Assert.Equal(5, closure.ValueAt(4));
        Assert.Equal(5, closure.RightLimitAt(4));

        //reference period
        Assert.Equal(12, closure.LeftLimitAt(13));
        Assert.Equal(12, closure.ValueAt(13));
        Assert.Equal(10, closure.RightLimitAt(13));

        Assert.Equal(10.5m, closure.LeftLimitAt(14));
        Assert.Equal(10.5m, closure.ValueAt(14));
        Assert.Equal(10.5m, closure.RightLimitAt(14));

        Assert.Equal(11, closure.LeftLimitAt(15));
        Assert.Equal(12.5m, closure.ValueAt(15));
        Assert.Equal(12.5m, closure.RightLimitAt(15));

        Assert.Equal(13, closure.LeftLimitAt(16));
        Assert.Equal(13, closure.ValueAt(16));
        Assert.Equal(13, closure.RightLimitAt(16));

        Assert.Equal(13.5m, closure.LeftLimitAt(17));
        Assert.Equal(13.5m, closure.ValueAt(17));
        Assert.Equal(13.5m, closure.RightLimitAt(17));

        Assert.Equal(14, closure.LeftLimitAt(18));
        Assert.Equal(15.5m, closure.ValueAt(18));
        Assert.Equal(15.5m, closure.RightLimitAt(18));

        Assert.Equal(16, closure.LeftLimitAt(19));
        Assert.Equal(16, closure.ValueAt(19));
        Assert.Equal(14, closure.RightLimitAt(19));
    }

    //todo: add test for infinite segment
}