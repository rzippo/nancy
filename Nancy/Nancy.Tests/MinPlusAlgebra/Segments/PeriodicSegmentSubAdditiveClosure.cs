using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class PeriodicSegmentSubAdditiveClosure
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PeriodicSegmentSubAdditiveClosure(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

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

    // todo: fill in more cases from notes
    
    public static
        List<(Segment segment, Rational periodLength, Rational periodHeight, Curve closure)>
        KnownPeriodicSegmentSubAdditiveClosures =
            [
                // Type A1 (say more...)
                (
                    segment: new Segment(1, 2, 1, 2),
                    periodLength: 4,
                    periodHeight: 6,
                    closure: new Curve(
                        new Sequence([
                            Point.Origin(),
                            Segment.PlusInfinite(0, 1),
                            Point.PlusInfinite(1),
                            new Segment(1, 2, 1, 2),
                            Point.PlusInfinite(2),
                            new Segment(2, 3, 2, 2),
                            new Point(3, 4),
                            new Segment(3, 4, 3, 2),
                        ]),
                        3,
                        1,
                        1
                    )
                ),
                // Type A2 (say more...)
                (
                    segment: new Segment(5, 6, 5, 2),
                    periodLength: 4,
                    periodHeight: 6,
                    closure: new Curve(
                        new Sequence([
                            Point.Origin(),
                            Segment.PlusInfinite(0, 5),
                            Point.PlusInfinite(5),
                            new Segment(5, 6, 5, 2),
                            Point.PlusInfinite(6),
                            Segment.PlusInfinite(6, 9),
                            Point.PlusInfinite(9),
                            new Segment(9, 10, 11, 2),
                            Point.PlusInfinite(10),
                            new Segment(10, 12, 10, 2),
                            Point.PlusInfinite(12),
                            Segment.PlusInfinite(12, 13),
                            Point.PlusInfinite(13),
                            new Segment(13, 14, 17, 2),
                            Point.PlusInfinite(14),
                            new Segment(14, 15, 16, 2),
                            new Point(15, 18),
                            new Segment(15, 18, 15, 2),
                            Point.PlusInfinite(18),
                            new Segment(18, 19, 22, 2),
                            new Point(19, 24),
                            new Segment(19, 20, 21, 2),
                            new Point(20, 23),
                            new Segment(20, 24, 20, 2),
                            new Point(24, 29),
                            new Segment(24, 25, 26, 2),
                            new Point(25, 28),
                            new Segment(25, 29, 25, 2),
                            new Point(29, 33),
                            new Segment(29, 30, 31, 2),
                            new Point(30, 33),
                            new Segment(30, 34, 30, 2)
                        ]),
                        29,
                        5,
                        5
                    )
                ),
            ];
    
    public static IEnumerable<object[]> KnownPeriodicSegmentSubAdditiveClosuresTestCases
        => KnownPeriodicSegmentSubAdditiveClosures.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownPeriodicSegmentSubAdditiveClosuresTestCases))]
    public void KnownSegmentSubAdditiveClosuresEquivalenceTest(
        Segment segment, Rational periodLength, Rational periodHeight, Curve closure
    )
    {
        _testOutputHelper.WriteLine($"var segment = {segment.ToCodeString()};");
        _testOutputHelper.WriteLine($"var periodLength = {periodLength.ToCodeString()};");
        _testOutputHelper.WriteLine($"var periodHeight = {periodHeight.ToCodeString()};");
        _testOutputHelper.WriteLine($"var closure = {closure.ToCodeString()};");
        var result = segment.SubAdditiveClosure(periodLength, periodHeight);
        _testOutputHelper.WriteLine($"var result = {result.ToCodeString()};");
        Assert.True(Curve.Equivalent(closure, result));
    }
}