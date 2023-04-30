using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointDeconvolution
{
    [Fact]
    public void PointPoint()
    {
        Point first = new Point
        (time: 3, value: 5);

        Point second = new Point
        (time: 2, value: 7);

        Point deconvolution = first.Deconvolution(second);

        Assert.Equal(first.Time - second.Time, deconvolution.Time);
        Assert.Equal(first.Value - second.Value, deconvolution.Value);
    }

    [Fact]
    public void PointSegment()
    {
        Point point = new Point
        (time: 2, value: 5);

        Segment segment = new Segment
        (
            rightLimitAtStartTime: 7,
            slope: 1,
            startTime: 5,
            endTime: 7
        );

        Segment deconvolution = point.Deconvolution(segment);

        Assert.Equal(point.Time - segment.EndTime, deconvolution.StartTime);
        Assert.Equal(point.Time - segment.StartTime, deconvolution.EndTime);
        Assert.Equal(point.Value - segment.RightLimitAtStartTime, deconvolution.LeftLimitAtEndTime);
    }

    public static IEnumerable<object[]> GetAsElementsTestCases()
    {
        var testCases = new (Element a, Element b)[]
        {
            (
                a: new Point
                (time: 3, value: 5),
                b: new Point
                (time: 2, value: 7)
            ),
            (
                a: new Point
                (time: 2, value: 5),
                b: new Segment
                (
                    rightLimitAtStartTime: 7,
                    slope: 1,
                    startTime: 5,
                    endTime: 7
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