using System;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// The same function stored with a breakpoint inside a line, and without it: 1 + t over [0, 4].
/// </summary>
public class RepresentationIndependence
{
    private static readonly Element[] Split =
        [new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3), new Segment(2, 4, 3, 1), new Point(4, 5)];

    private static readonly Element[] Merged =
        [new Point(0, 1), new Segment(0, 4, 1, 1), new Point(4, 5)];

    [Fact]
    public void DifferentlyMergedStreamsOfTheSameFunctionAreEquivalent()
    {
        Assert.True(Sequence.Equivalent(Split, Merged));
        Assert.True(Sequence.Equivalent(Merged, Split));
    }

    [Fact]
    public void StreamsOfDifferentFunctionsAreNotEquivalent()
    {
        Element[] other = [new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3), new Segment(2, 4, 3, 2), new Point(4, 7)];
        Assert.False(Sequence.Equivalent(Split, other));
        Assert.False(Sequence.Equivalent(Merged, other));
    }

    [Fact]
    public void AMergingCutJoinsTheStoredBreakpoints()
    {
        Element[] expected = [new Point(1, 2), new Segment(1, 3, 2, 1)];
        Assert.Equal(expected, Split.CutAndMerge(1, 3));
    }

    [Fact]
    public void AMergingCutRejectsElementsThatDoNotAdjoin()
    {
        // on the line 1 + t, but with nothing defined between 1 and 2
        Element[] gap = [new Point(0, 1), new Segment(0, 1, 1, 1), new Point(2, 3), new Segment(2, 3, 3, 1), new Point(3, 4)];
        Assert.Throws<ArgumentException>(() => gap.CutAndMerge(0, 3).ToList());
    }

    [Fact]
    public void ACutKeepsTheStoredBreakpoints()
    {
        Element[] expected = [new Point(1, 2), new Segment(1, 2, 2, 1), new Point(2, 3), new Segment(2, 3, 3, 1)];
        Assert.Equal(expected, Split.Cut(1, 3));
        Assert.Equal(expected, new Sequence(Split).Cut(1, 3).Elements);
    }
}
