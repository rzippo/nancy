using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentClosure
{
    //todo: add test for negative segment

    [Fact]
    public void SegmentClosureTypeA_Test1()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 1,
            slope: 3
        );

        Curve closure = segment.SubAdditiveClosure();

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsContinuousExceptOrigin);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));
        Assert.Equal(4, closure.LeftLimitAt(2));
        Assert.Equal(5, closure.LeftLimitAt(3));
        Assert.Equal(3, closure.RightLimitAt(3));
        Assert.Equal(7, closure.LeftLimitAt(5));
        Assert.Equal(5, closure.RightLimitAt(5));

        Assert.True(closure.PseudoPeriodStart <= 2); //could be optimized
        Assert.Equal(1, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    [Fact]
    public void SegmentClosureTypeA_Test2()
    {
        Segment segment = new Segment(
            startTime: 3,
            endTime: 4,
            rightLimitAtStartTime: 1,
            slope: 3
        );

        Curve closure = segment.SubAdditiveClosure();

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsContinuousExceptOrigin);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(3));
        Assert.Equal(1, closure.RightLimitAt(3));
        Assert.Equal(4, closure.LeftLimitAt(4));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(5));
        Assert.Equal(2, closure.RightLimitAt(6));
        Assert.Equal(8, closure.LeftLimitAt(8));

        Assert.Equal(13, closure.LeftLimitAt(15));
        Assert.Equal(5, closure.RightLimitAt(15));

        Assert.Equal(14, closure.LeftLimitAt(18));
        Assert.Equal(6, closure.RightLimitAt(18));

        Assert.True(closure.PseudoPeriodStart <= 12); //could be optimized
        Assert.Equal(3, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    [Fact]
    public void SegmentClosureTypeB_Decreasing()
    {
        Segment segment = new Segment(
            startTime: 1,
            endTime: 2,
            rightLimitAtStartTime: 4,
            slope: -3
        );

        Curve closure = segment.SubAdditiveClosure();

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsContinuousExceptOrigin);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(0.5m));

        Assert.Equal(4, closure.RightLimitAt(1));
        Assert.Equal(1, closure.LeftLimitAt(2));

        Assert.Equal(2, closure.LeftLimitAt(4));
        Assert.Equal(9, closure.RightLimitAt(4));

        Assert.Equal(5, closure.LeftLimitAt(10));
        Assert.Equal(12, closure.RightLimitAt(10));

        Assert.True(closure.PseudoPeriodStart <= 4); //could be optimized
        Assert.Equal(2, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    [Fact]
    public void SegmentClosureTypeBConstant()
    {
        Segment segment = Segment.Constant(startTime: 1,
            endTime: 2, value: 1);

        Curve closure = segment.SubAdditiveClosure();

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsContinuousExceptOrigin);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(0.5m));
        Assert.Equal(1, closure.RightLimitAt(1));
        Assert.Equal(2, closure.RightLimitAt(2));
        Assert.Equal(3, closure.RightLimitAt(4));
        Assert.Equal(4, closure.RightLimitAt(6));

        Assert.True(closure.PseudoPeriodStart <= 4); //could be optimized
        Assert.Equal(2, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    //todo: should add an increasing type b case

    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 10)]
    [InlineData(1, 2)]
    [InlineData(10, 20)]
    public void ZeroSegmentClosure(int start, int end)
    {
        var segment = Segment.Zero(start, end);
        var closure = segment.SubAdditiveClosure();

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(0, closure.RightLimitAt(start));
        Assert.True(closure.IsUltimatelyPlain);
        Assert.Equal(start, closure.FirstFiniteTimeExceptOrigin);

        var sequence = closure.Cut(start, closure.SecondPseudoPeriodEnd);

        if (start == 0)
        {
            Assert.Equal(0, closure.ValueAt(start));
            Assert.True(sequence.IsContinuous);
            Assert.True(sequence.IsRightContinuous);
            Assert.True(sequence.IsZero);
        }
        else
        {
            var length = end - start;
            Assert.Equal(length > start, sequence.IsContinuous);
            var periodicSequence = closure.Cut(closure.PseudoPeriodStart, closure.SecondPseudoPeriodEnd);
            Assert.True(periodicSequence.IsContinuous);
            Assert.True(periodicSequence.IsRightContinuous);
            Assert.True(periodicSequence.IsZero);
        }
    }

    //todo: add test for infinite segment
}