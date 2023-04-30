using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class PeriodicSegmentTransversalView
{
    [Fact]
    public void TransversalViewTypeA_0()
    {
        Segment segment = new Segment(
            startTime: 5,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6,
            i: 0
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(25, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(25));
        Assert.Equal(25, transversalView.RightLimitAt(25));

        Assert.Equal(33, transversalView.LeftLimitAt(29));
        Assert.Equal(33, transversalView.ValueAt(29));
        Assert.Equal(Rational.PlusInfinity, transversalView.RightLimitAt(29));

        Assert.Equal(5, transversalView.PseudoPeriodLength);
        Assert.Equal(5, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeA_1()
    {
        Segment segment = new Segment(
            startTime: 5,
            endTime: 6,
            rightLimitAtStartTime: 5,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6,
            i: 1
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(29, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(29));
        Assert.Equal(31, transversalView.RightLimitAt(29));

        Assert.Equal(39, transversalView.LeftLimitAt(33));
        Assert.Equal(39, transversalView.ValueAt(33));
        Assert.Equal(Rational.PlusInfinity, transversalView.RightLimitAt(33));

        Assert.Equal(5, transversalView.PseudoPeriodLength);
        Assert.Equal(5, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeB_0()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6,
            i: 0
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(5, transversalView.FirstFiniteTimeExceptOrigin);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(5));
        Assert.Equal(5, transversalView.RightLimitAt(5));
        Assert.Equal(9, transversalView.ValueAt(8));
        Assert.Equal(8, transversalView.RightLimitAt(8));

        Assert.Equal(1, transversalView.PseudoPeriodLength);
        Assert.Equal(1, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeB_1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 6,
            i: 1
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(9, transversalView.FirstFiniteTimeExceptOrigin);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(9));
        Assert.Equal(11, transversalView.RightLimitAt(9));
        Assert.Equal(15, transversalView.ValueAt(12));
        Assert.Equal(14, transversalView.RightLimitAt(12));

        Assert.Equal(1, transversalView.PseudoPeriodLength);
        Assert.Equal(1, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeC()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 0.5m
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 4,
            i: 0
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(3, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(3));
        Assert.Equal(3, transversalView.RightLimitAt(3));

        Assert.Equal(6, transversalView.LeftLimitAt(9));
        Assert.Equal(6.5m, transversalView.ValueAt(9));
        Assert.Equal(6.5m, transversalView.RightLimitAt(9));

        Assert.Equal(10, transversalView.LeftLimitAt(15));
        Assert.Equal(10.5m, transversalView.ValueAt(15));
        Assert.Equal(10.5m, transversalView.RightLimitAt(15));

        Assert.Equal(3, transversalView.PseudoPeriodLength);
        Assert.Equal(2, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeD()
    {
        Segment segment = new Segment(
            startTime: 2,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodLength: 4,
            pseudoPeriodHeight: 12,
            i: 0
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(10, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(10));
        Assert.Equal(5, transversalView.RightLimitAt(10));

        Assert.Equal(9, transversalView.LeftLimitAt(12));
        Assert.Equal(9, transversalView.ValueAt(12));
        Assert.Equal(6, transversalView.RightLimitAt(12));

        Assert.Equal(10, transversalView.LeftLimitAt(14));
        Assert.Equal(10, transversalView.ValueAt(14));
        Assert.Equal(7, transversalView.RightLimitAt(14));

        Assert.Equal(11, transversalView.LeftLimitAt(16));
        Assert.Equal(11, transversalView.ValueAt(16));
        Assert.Equal(8, transversalView.RightLimitAt(16));

        Assert.Equal(2, transversalView.PseudoPeriodLength);
        Assert.Equal(1, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeE()
    {
        Segment segment = new Segment(
            startTime: 2,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodHeight: 6,
            pseudoPeriodLength: 2,
            i: 1
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(9, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        //k = 3
        Assert.Equal(11, transversalView.ValueAt(9));
        Assert.Equal(11, transversalView.RightLimitAt(9));
        Assert.Equal(15, transversalView.LeftLimitAt(11));
        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(11));
        Assert.Equal(Rational.PlusInfinity, transversalView.RightLimitAt(11));

        //k = 4
        Assert.Equal(14, transversalView.ValueAt(12));
        Assert.Equal(14, transversalView.RightLimitAt(12));
        Assert.Equal(18, transversalView.LeftLimitAt(14));
        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(14));
        Assert.Equal(Rational.PlusInfinity, transversalView.RightLimitAt(14));

        //k = 5
        Assert.Equal(17, transversalView.ValueAt(15));
        Assert.Equal(17, transversalView.RightLimitAt(15));
        Assert.Equal(21, transversalView.LeftLimitAt(17));
        Assert.Equal(Rational.PlusInfinity, transversalView.ValueAt(17));
        Assert.Equal(Rational.PlusInfinity, transversalView.RightLimitAt(17));

        Assert.Equal(3, transversalView.PseudoPeriodLength);
        Assert.Equal(3, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeF()
    {
        Segment segment = new Segment(
            startTime: 2,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 2
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodHeight: 12,
            pseudoPeriodLength: 4,
            i: 1
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(15, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        //k = 5
        Assert.Equal(19, transversalView.ValueAt(15));
        Assert.Equal(19, transversalView.RightLimitAt(15));
        Assert.Equal(25, transversalView.LeftLimitAt(18));

        //k = 6
        Assert.Equal(22, transversalView.ValueAt(18));
        Assert.Equal(22, transversalView.RightLimitAt(18));
        Assert.Equal(28, transversalView.LeftLimitAt(21));

        Assert.Equal(3, transversalView.PseudoPeriodLength);
        Assert.Equal(3, transversalView.PseudoPeriodHeight);
    }

    [Fact]
    public void TransversalViewTypeG()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 3,
            rightLimitAtStartTime: 1,
            slope: 0.5m
        );

        Curve transversalView = segment.TransversalViewOfIteratedConvolutions(
            pseudoPeriodHeight: 4,
            pseudoPeriodLength: 4,
            i: 1
        );

        Assert.Equal(0, transversalView.ValueAt(0));
        Assert.Equal(9, transversalView.FirstFiniteTimeExceptOrigin);
        Assert.False(transversalView.IsContinuous);

        //k = 3
        Assert.Equal(8, transversalView.ValueAt(9));
        Assert.Equal(8, transversalView.RightLimitAt(9));
        Assert.Equal(10, transversalView.LeftLimitAt(13));

        //k = 4
        Assert.Equal(10.5m, transversalView.ValueAt(13));
        Assert.Equal(10.5m, transversalView.RightLimitAt(13));
        Assert.Equal(12, transversalView.LeftLimitAt(16));

        //k = 5
        Assert.Equal(12.5m, transversalView.ValueAt(16));
        Assert.Equal(12.5m, transversalView.RightLimitAt(16));
        Assert.Equal(14, transversalView.LeftLimitAt(19));

        Assert.Equal(3, transversalView.PseudoPeriodLength);
        Assert.Equal(2, transversalView.PseudoPeriodHeight);
    }
}