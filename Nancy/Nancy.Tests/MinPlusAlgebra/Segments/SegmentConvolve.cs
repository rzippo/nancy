using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentConvolution
{
    [Fact]
    public void SegmentSegment()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 10,
            slope: 5,
            startTime: 2,
            endTime: 5
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 8
        );

        Element[] convolution = first.Convolution(second).ToArray();
        Assert.Equal(3, convolution.Length);

        Segment leftSegment = (Segment) convolution[0];
        Point middlePoint = (Point) convolution[1];
        Segment rightSegment = (Segment) convolution[2];

        Assert.Equal(leftSegment.Slope, second.Slope);
        Assert.Equal(rightSegment.Slope, first.Slope);

        Assert.Equal(second.LeftLimitAtEndTime + first.RightLimitAtStartTime, middlePoint.Value);
        Assert.Equal(second.ValueAt(6) + first.RightLimitAtStartTime, leftSegment.ValueAt(6 + first.StartTime));
        Assert.Equal(first.ValueAt(4) + second.LeftLimitAtEndTime, rightSegment.ValueAt(4 + second.EndTime));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 1)]
    [InlineData(2, 4)]
    [InlineData(20, 40)]
    public void SelfConvolutionZeroSegments(int start, int end)
    {
        var segment = Segment.Zero(start, end);
        var convolution = new Sequence(segment.Convolution(segment));

        Assert.True(convolution.IsContinuous);
        Assert.True(convolution.IsRightContinuous);
        Assert.True(convolution.IsZero);
        Assert.Equal(2 * start, convolution.DefinedFrom);
        Assert.Equal(2 * end, convolution.DefinedUntil);
    }

    public static IEnumerable<object[]> CommutativityTestCases()
    {
        var testCases = new (Segment a, Segment b)[]
        {
            (
                new Segment( startTime: new Rational(0, 1), endTime: new Rational(3, 1), slope: new Rational(0, 1), rightLimitAtStartTime: new Rational(3, 1) ),
                new Segment( startTime: new Rational(34, 11), endTime: new Rational(4, 1), slope: new Rational(0, 1), rightLimitAtStartTime: new Rational(4, 1) )
            ),
            (
                new Segment( startTime: new Rational(0, 1), endTime: new Rational(3, 1), slope: new Rational(2, 1), rightLimitAtStartTime: new Rational(3, 1) ),
                new Segment( startTime: new Rational(34, 11), endTime: new Rational(4, 1), slope: new Rational(2, 1), rightLimitAtStartTime: new Rational(4, 1) )
            )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] { testCase.a, testCase.b };
        }
    }

    [Theory]
    [MemberData(nameof(CommutativityTestCases))]
    public void Commutativity(Segment a, Segment b)
    {
        Assert.Equal(
            a.Convolution(b),
            b.Convolution(a)
        );
    }
}