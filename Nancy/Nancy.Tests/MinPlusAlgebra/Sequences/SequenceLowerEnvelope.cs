using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceLowerEnvelope
{
    [Fact]
    public void LowerEnvelope1()
    {
        Element[] elements = new Element[]
        {
            new Point(
                time: 0,
                value: 0
            ), 
            new Segment(
                startTime: 0,
                endTime: 90,
                rightLimitAtStartTime: 0,
                slope: 30
            ),
            new Point(
                time: 30,
                value: 30
            ), 
            new Segment(
                startTime: 30,
                endTime: 60,
                rightLimitAtStartTime: 30,
                slope: 0),
            new Point(
                time: 60,
                value: 60
            ), 
            new Segment(
                startTime: 60,
                endTime: 90,
                rightLimitAtStartTime: 60,
                slope: 0),
        };

        Sequence fun =  elements.LowerEnvelope().ToSequence();

        //Assert.Equal(3, fun.Count);
        Assert.Equal(30, fun.GetSegmentAfter(0).Slope);
        Assert.Equal(0, fun.GetSegmentAfter(40).Slope);
        Assert.Equal(30, fun.ValueAt(50));
        Assert.Equal(0, fun.GetSegmentAfter(80).Slope);
        Assert.Equal(60, fun.ValueAt(70));
    }

}