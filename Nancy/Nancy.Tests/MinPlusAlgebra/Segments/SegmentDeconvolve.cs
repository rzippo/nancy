using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentDeconvolution
{
    [Fact]
    public void SegmentPoint()
    {
        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 7
        );

        Point point = new Point 
        (
            time: 2,
            value: 5
        );

        Segment deconvolution = segment.Deconvolution(point);

        Assert.Equal(segment.StartTime - point.StartTime, deconvolution.StartTime);
        Assert.Equal(segment.EndTime - point.StartTime, deconvolution.EndTime);
        Assert.Equal(segment.Slope, deconvolution.Slope);
    }

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

        Element[] deconvolution = first.Deconvolution(second).ToArray();
        Assert.Equal(3, deconvolution.Length);

        Segment leftSegment = (Segment) deconvolution[0];
        Point middlePoint = (Point) deconvolution[1];
        Segment rightSegment = (Segment) deconvolution[2];

        Assert.Equal(first.StartTime - second.EndTime, leftSegment.StartTime);
        Assert.Equal(first.EndTime - second.StartTime, rightSegment.EndTime);

        Assert.Equal(leftSegment.Slope, first.Slope);
        Assert.Equal(rightSegment.Slope, second.Slope);

        Assert.Equal(middlePoint.Value, leftSegment.LeftLimitAtEndTime);
        Assert.Equal(middlePoint.Value, rightSegment.RightLimitAtStartTime);
        Assert.Equal(first.ValueAt(3) - second.LeftLimitAtEndTime, leftSegment.ValueAt(3 - second.EndTime));
    }

    [Fact]
    public void SegmentSegment_2()
    {
        Segment first = new Segment
        (
            rightLimitAtStartTime: 10,
            slope: 5,
            startTime: -2,
            endTime: 1
        );

        Segment second = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 8
        );

        Element[] deconvolution = first.Deconvolution(second).ToArray();
        Assert.Equal(3, deconvolution.Length);

        Segment leftSegment = (Segment) deconvolution[0];
        Point middlePoint = (Point) deconvolution[1];
        Segment rightSegment = (Segment) deconvolution[2];

        Assert.Equal(first.StartTime - second.EndTime, leftSegment.StartTime);
        Assert.Equal(first.EndTime - second.StartTime, rightSegment.EndTime);

        Assert.Equal(leftSegment.Slope, first.Slope);
        Assert.Equal(rightSegment.Slope, second.Slope);

        Assert.Equal(middlePoint.Value, leftSegment.LeftLimitAtEndTime);
        Assert.Equal(middlePoint.Value, rightSegment.RightLimitAtStartTime);
        Assert.Equal(first.ValueAt(1) - second.LeftLimitAtEndTime, leftSegment.ValueAt(1 - second.EndTime));
    }

    public static IEnumerable<object[]> GetAsElementsTestCases()
    {
        var testCases = new (Element a, Element b)[]
        {
            (
                a: new Segment
                (
                    rightLimitAtStartTime: 7,
                    slope: 1,
                    startTime: 5,
                    endTime: 7
                ),
                b: new Point
                (
                    time: 2,
                    value: 5
                )
            ),
            (
                a: new Segment
                (
                    rightLimitAtStartTime: 10,
                    slope: 5,
                    startTime: 2,
                    endTime: 5
                ),
                b: new Segment
                (
                    rightLimitAtStartTime: 7,
                    slope: 1,
                    startTime: 5,
                    endTime: 8
                )
            )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[]{ testCase.a, testCase.b };
        }
    }

    [Theory]
    [MemberData(nameof(GetAsElementsTestCases))]
    public void AsElements(Element a, Element b)
    {
        //Does not throw
        var deconvolution = Element.Deconvolution(a, b);
    }
}