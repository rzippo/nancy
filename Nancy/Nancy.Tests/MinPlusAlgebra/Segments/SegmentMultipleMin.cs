using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentMultipleMin
{
    [Fact]
    public void MultipleMin1()
    {
        Segment f = new Segment(
            startTime: 0,
            endTime: 40,
            slope: new Rational(7, 4),
            rightLimitAtStartTime: 2
        );

        Segment g = new Segment(
            startTime: 0,
            endTime: 20,
            slope: new Rational(4, 5),
            rightLimitAtStartTime: 3
        );

        Segment h = new Segment(
            startTime: 0,
            endTime: 10,
            slope: new Rational(2, 5),
            rightLimitAtStartTime: 5
        );

        Sequence minFunction = new Sequence(Element.Minimum(new[] { f, g, h }));

        Assert.Equal(0, minFunction.DefinedFrom);
        Assert.Equal(10, minFunction.DefinedUntil);
        Assert.Equal(2, minFunction.RightLimitAt(0));
        Assert.Equal(
            new Rational(73, 19), 
            minFunction.ValueAt(new Rational(20, 19)));
        Assert.Equal(7, minFunction.ValueAt(5));
    }

    [Fact]
    public void MultipleMin2()
    {
        Point f = new Point(time: 5, value: 2);

        Segment g = new Segment(
            startTime: 0,
            endTime: 20,
            slope: new Rational(4, 5),
            rightLimitAtStartTime: 3
        );

        Segment h = new Segment(
            startTime: 0,
            endTime: 10,
            slope: new Rational(2, 5),
            rightLimitAtStartTime: 5
        );

        Element[] minSegments = Element.Minimum(new Element[] { f, g, h }).ToArray();

        Assert.Single(minSegments);
        Point minPoint = (Point)minSegments.Single();
        Assert.Equal(2, minPoint.Value);
        Assert.Equal(5, minPoint.Time);
    }

    [Fact]
    public void MultipleMin3()
    {
        Segment f = new Segment(
            startTime: 0,
            endTime: 20,
            slope: 12,
            rightLimitAtStartTime: 0
        );

        Segment g = new Segment(
            startTime: 0,
            endTime: 30,
            slope: 7,
            rightLimitAtStartTime: 10
        );

        Segment h = new Segment(
            startTime: 0,
            endTime: 40,
            slope: 4,
            rightLimitAtStartTime: 20
        );

        Segment k = new Segment(
            startTime: 0,
            endTime: 50,
            slope: 2,
            rightLimitAtStartTime: 30
        );

        Element[] minSegments = Element.Minimum(new Element[] { g, k, h, f }).ToArray();

        Sequence minFunction = new Sequence(minSegments);
        Assert.Equal(0, minFunction.DefinedFrom);
        Assert.Equal(20, minFunction.DefinedUntil);

        Assert.Equal(0, minFunction.RightLimitAt(0));
        Assert.Equal(2, minFunction.GetSegmentAfter(2).StartTime);
        Assert.Equal(5, minFunction.GetSegmentAfter(5).StartTime);
        Assert.Equal(40, minFunction.ValueAt(5));
    }
}