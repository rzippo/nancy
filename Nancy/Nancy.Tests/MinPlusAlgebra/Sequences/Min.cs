using System;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Min
{
    [Fact]
    public void EquivalenceToExpected()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;

        Sequence min = f1.Minimum(f2);

        Assert.Equal(0, min.DefinedFrom);
        Assert.Equal(6, min.DefinedUntil);

        //Assert.Equal(3, min.Optimize().Count);
        Assert.Equal(5, min.GetSegmentAfter(2).Slope);
        Assert.True(min.GetSegmentAfter(3).Slope < 0 && min.GetSegmentAfter(3).Slope > -5);
        Assert.Equal(-5, min.GetSegmentAfter(5).Slope);
        Assert.Equal(5, min.GetSegmentBefore(6).LeftLimitAtEndTime);
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

    /// <summary>
    /// $t$ over $[0, 1]$, and $t + 5$ over $[1, 2]$, $]1, 2]$, and $t$ over $[0, 1[$.
    /// </summary>
    private static readonly Sequence TOn01 = new([ Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1) ]);
    private static readonly Sequence TPlus5On12 = new([ new Point(1, 6), new Segment(1, 2, 6, 1), new Point(2, 7) ]);
    private static readonly Sequence TPlus5OnOpen12 = new([ new Segment(1, 2, 6, 1), new Point(2, 7) ]);
    private static readonly Sequence TOn01Open = new([ Point.Origin(), new Segment(0, 1, 0, 1) ]);

    /// <summary>
    /// Supports that share a single time, which both include, give the minimum at that time alone.
    /// </summary>
    [Fact]
    public void SupportsSharingASingleTimeGiveTheMinimumThere()
    {
        var min = Sequence.Minimum(TOn01, TPlus5On12);
        Assert.True(Sequence.Equivalent(new Sequence([ new Point(1, 1) ]), min), $"{min}");
    }

    /// <summary>
    /// Supports that touch at a time one of them excludes share no time, and the minimum throws.
    /// </summary>
    [Fact]
    public void SupportsTouchingAtAnExcludedTimeAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Sequence.Minimum(TOn01, TPlus5OnOpen12));
        Assert.Throws<ArgumentException>(() => Sequence.Minimum(TOn01Open, TPlus5On12));
    }
}
