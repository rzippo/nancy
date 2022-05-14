using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceInfinite
{
    [Fact]
    public void MinWithInfiniteParts()
    {
        Sequence f1 = new Sequence(new Element[]
        {
            Point.PlusInfinite(0),
            Segment.PlusInfinite(0, 10),
            new Point(
                time: 10,
                value: 10
            ), 
            new Segment(
                startTime: 10,
                endTime: 15,
                rightLimitAtStartTime: 10,
                slope: 5
            ),
        });

        Sequence f2 = new Sequence(new Element[]
        {
            new Point(
                time: 0,
                value: 0
            ),
            new Segment(
                startTime: 0,
                endTime: 10,
                rightLimitAtStartTime: 0,
                slope: 1
            ),
            Point.PlusInfinite(10),
            Segment.PlusInfinite(10, 15)
        });

        Sequence min = f1.Minimum(f2);

        Assert.All(min.Elements, segment =>
        {
            Assert.False(segment.IsInfinite);
        });

        Assert.Equal(0, min.ValueAt(0));
        Assert.Equal(10, min.ValueAt(10));
    }
}