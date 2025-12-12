using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentSubAdditiveClosure
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SegmentSubAdditiveClosure(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SegmentSubAdditiveClosureTypeA_Test1()
    {
        // Type A implies s(a+)/a <= s(b-)/b
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
        Assert.False(closure.IsPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));
        Assert.Equal(1, closure.RightLimitAt(1));
        Assert.Equal(4, closure.LeftLimitAt(2));
        Assert.Equal(5, closure.LeftLimitAt(3));
        Assert.Equal(3, closure.RightLimitAt(3));
        Assert.Equal(7, closure.LeftLimitAt(5));
        Assert.Equal(5, closure.RightLimitAt(5));

        Assert.Equal(2, closure.PseudoPeriodStartInfimum);
        Assert.Equal(3, closure.PseudoPeriodStart);
        Assert.Equal(1, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    [Fact]
    public void SegmentSubAdditiveClosureTypeA_Test2()
    {
        // Type A implies s(a+)/a <= s(b-)/b
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
        Assert.False(closure.IsPlain);

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

        Assert.Equal(12, closure.PseudoPeriodStartInfimum);
        Assert.Equal(15, closure.PseudoPeriodStart);
        Assert.Equal(3, closure.PseudoPeriodLength);
        Assert.Equal(1, closure.PseudoPeriodHeight);
    }

    [Fact]
    public void SegmentSubAdditiveClosureTypeB_Decreasing()
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
        Assert.False(closure.IsPlain);

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
    public void SegmentSubAdditiveClosureTypeBConstant()
    {
        Segment segment = Segment.Constant(startTime: 1,
            endTime: 2, value: 1);

        Curve closure = segment.SubAdditiveClosure();

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsContinuousExceptOrigin);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);
        Assert.False(closure.IsPlain);

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
    public void ZeroSegmentSubAdditiveClosure(int start, int end)
    {
        var segment = Segment.Zero(start, end);
        var closure = segment.SubAdditiveClosure();

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(0, closure.RightLimitAt(start));
        Assert.True(closure.IsUltimatelyPlain);
        Assert.Equal(start == 0, closure.IsPlain);
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

    public static List<(Segment segment, Curve closure)> KnownSegmentSubAdditiveClosures =
    [
        // edge case: zero segment with a = 0
        (
            segment: Segment.Zero(0, 1),
            closure: Curve.Zero()
        ),
        (
            segment: Segment.Zero(0, 10),
            closure: Curve.Zero()
        ),
        // edge case: segment with a = 0, s(a+) < 0 => minus infinite closure
        (
            segment: new Segment(0, 1, -1, 1),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.MinusInfinite(0, 1),
                    Point.MinusInfinite(1),
                    Segment.MinusInfinite(1, 2)
                ]),
                1,
                1,
                0
            )
        ),
        // edge case: segment with a = 0, s(a+) > 0 => normal closure, but s(a+)/a undefined
        // rho > 0
        (
            segment: new Segment(0, 1, 1, 1),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 1, 1, 1),
                    new Point(1, 3),
                    new Segment(1, 2, 3, 1)
                ]),
                1,
                1,
                2
            )
        ),
        (
            segment: new Segment(0, 2, 1, new Rational(1, 2)),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 2, 1, new Rational(1, 2)),
                    new Point(2, 3),
                    new Segment(2, 4, 3, new Rational(1, 2))
                ]),
                2,
                2,
                2
            )
        ),
        // edge case: segment with a = 0, s(a+) > 0 => normal closure, but s(a+)/a undefined
        // rho < 0
        (
            segment: new Segment(0, 2, 2, new Rational(-1, 2)),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 2, 2, new Rational(-1, 2)),
                    new Point(2, 3),
                    new Segment(2, 4, 3, new Rational(-1, 2))
                ]),
                2,
                2,
                1
            )
        ),
        // edge case: segment with a = 0, s(a+) > 0 => normal closure, but s(a+)/a undefined
        // rho = 0
        (
            segment: new Segment(0, 2, 1, 0),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    new Segment(0, 2, 1, 0),
                    new Point(2, 2),
                    new Segment(2, 4, 2, 0)
                ]),
                2,
                2,
                1
            )
        ),
        // Type A, i.e. s(a+)/a < s(b-)/b
        // rho > 0
        (
            segment: new Segment(1, 2, 1, 3),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 2, 1, 3),
                    Point.PlusInfinite(2),
                    new Segment(2, 3, 2, 3),
                    new Point(3, 5),
                    new Segment(3, 4, 3, 3),
                ]),
                3,
                1,
                1
            )
        ),
        (
            segment: new Segment(3, 4, 1, 3),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 3),
                    Point.PlusInfinite(3),
                    new Segment(3, 4, 1, 3),
                    Point.PlusInfinite(4),
                    Segment.PlusInfinite(4, 6),
                    Point.PlusInfinite(6),
                    new Segment(6, 8, 2, 3),
                    Point.PlusInfinite(8),
                    Segment.PlusInfinite(8, 9),
                    Point.PlusInfinite(9),
                    new Segment(9, 12, 3, 3),
                    Point.PlusInfinite(12),
                    new Segment(12, 15, 4, 3),
                    new Point(15, 13),
                    new Segment(15, 18, 5, 3),
                ]),
                15,
                3,
                1
            )
        ),
        // Type A, i.e. s(a+)/a < s(b-)/b
        // rho < 0, negative values
        (
            segment: new Segment(1, 4, -1, new Rational(-1, 3)),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 2, -1, new Rational(-1, 3)),
                    new Point(2, new Rational(-4, 3)),
                    new Segment(2, 3, -2, new Rational(-1, 3)),
                ]),
                2,
                1,
                -1
            )
        ),
        // Type B, i.e. s(a+)/a > s(b-)/b
        // rho < 0
        (
            segment: new Segment(1, 2, 3, -2),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 2, 3, -2),
                    Point.PlusInfinite(2),
                    new Segment(2, 4, 6, -2),
                    new Point(4, 7),
                    new Segment(4, 6, 7, -2),
                ]),
                4,
                2,
                1
            )
        ),
        // Type B, i.e. s(a+)/a > s(b-)/b
        // rho > 0
        (
            segment: new Segment(1, 4, 1, new Rational(1, 3)),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 4, 1, new Rational(1, 3)),
                    new Point(4, 2 + new Rational(2, 3)),
                    new Segment(4, 8, 2 + new Rational(2, 3), new Rational(1, 3)),
                ]),
                4,
                4,
                2
            )
        ),
        // Type C, i.e. s(a+)/a = s(b-)/b
        // rho > 0
        (
            segment: new Segment(1, 2, 1, 1),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 2, 1, 1),
                    Point.PlusInfinite(2),
                    new Segment(2, 4, 2, 1)
                ]),
                3,
                1,
                1
            )
        ),
        // Type C, i.e. s(a+)/a = s(b-)/b
        // rho = 0
        (
            segment: Segment.Zero(1, 2),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    Segment.Zero(1, 2),
                    Point.PlusInfinite(2),
                    Segment.Zero(2, 4)
                ]),
                3,
                1,
                0
            )
        ),
        // Type C, i.e. s(a+)/a = s(b-)/b
        // rho < 0
        (
            segment: new Segment(1, 2, -1, -1),
            closure: new Curve(
                new Sequence([
                    Point.Origin(),
                    Segment.PlusInfinite(0, 1),
                    Point.PlusInfinite(1),
                    new Segment(1, 2, -1, -1),
                    Point.PlusInfinite(2),
                    new Segment(2, 4, -2, -1)
                ]),
                3,
                1,
                -1
            )
        ),
    ];

    public static IEnumerable<object[]> KnownSegmentSubAdditiveClosuresTestCases
        => KnownSegmentSubAdditiveClosures.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownSegmentSubAdditiveClosuresTestCases))]
    public void KnownSegmentSubAdditiveClosuresEquivalenceTest(Segment segment, Curve closure)
    {
        _testOutputHelper.WriteLine($"var segment = {segment.ToCodeString()};");
        _testOutputHelper.WriteLine($"var closure = {closure.ToCodeString()};");
        var result = segment.SubAdditiveClosure();
        _testOutputHelper.WriteLine($"var result = {result.ToCodeString()};");
        Assert.True(Curve.Equivalent(closure, result));
    }
}