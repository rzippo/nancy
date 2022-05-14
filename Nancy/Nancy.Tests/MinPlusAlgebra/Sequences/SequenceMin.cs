using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceMin
{
    [Fact]
    public void Min()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;
            
        Sequence min = f1.Minimum(f2);

        Assert.Equal(0, min.DefinedFrom);
        Assert.Equal(6, min.DefinedUntil);

        //Assert.Equal(3, min.Optimize().Count);
        Assert.Equal(5, min.GetActiveSegmentAfter(2).Slope);
        Assert.True(min.GetActiveSegmentAfter(3).Slope < 0 && min.GetActiveSegmentAfter(3).Slope > -5);
        Assert.Equal(-5, min.GetActiveSegmentAfter(5).Slope);
        Assert.Equal(5, min.GetActiveSegmentBefore(6).LeftLimitAtEndTime);
    }

    [Fact]
    public void ChainingMin()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB.Delay(3);

        Sequence min = Sequence.Minimum(f1, f2, false);

        Assert.Equal(f1.DefinedFrom, min.DefinedFrom);
        Assert.Equal(f2.DefinedUntil, min.DefinedUntil);
        Assert.True(min.IsFinite);
    }
}