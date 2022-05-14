using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceMethods
{
    [Fact]
    public void Optimize()
    {
        Sequence function = new Sequence(new Element[]
        {
            Segment.Zero(0, 10),
            Point.Zero(10), 
            Segment.Zero(10, 20),
            Point.Zero(20),
            Segment.Zero(20, 30),
            Point.Zero(30),
            Segment.Zero(30, 40),
            Point.Zero(40),
            Segment.Zero(40, 50),
        });

        Sequence optimized = function.Optimize();

        Assert.Single(optimized.Elements);
        Assert.Equal(0, optimized.DefinedFrom);
        Assert.Equal(50, optimized.DefinedUntil);
    }

    [Fact]
    public void GetOverlap()
    {
        var overlap = TestFunctions.SequenceA.GetOverlap(TestFunctions.SequenceB);

        Assert.NotNull(overlap);
        var (start, end, isLeftClosed, isRightClosed) = overlap ?? default;

        Assert.Equal(0, start);
        Assert.Equal(6, end);
        Assert.True(isLeftClosed);
        Assert.False(isRightClosed);
    }
}