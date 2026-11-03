using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceMonotonicity
{
    public static IEnumerable<(Sequence Sequence, bool NonIncreasing, bool Decreasing)> Cases =>
    [
        (new Sequence([new Point(0, 1), new Segment(0, 1, 1, -1), new Point(1, 0)]), true, true),
        (new Sequence([new Point(0, 2), new Segment(0, 1, 1, -1), new Point(1, -1)]), true, true),
        (new Sequence([new Point(0, 0), new Segment(0, 1, 1, -1)]), false, false),
        (new Sequence([new Segment(0, 1, 1, -1), new Point(1, 1)]), false, false),
        (new Sequence([new Segment(0, 1, 1, 0)]), true, false),
        (new Sequence([new Segment(0, 1, 0, 1)]), false, false),
        (new Sequence([new Point(0, 1)]), true, true),
        (new Sequence([Segment.PlusInfinite(0, 1)]), true, false),
        (new Sequence([Segment.MinusInfinite(0, 1)]), true, false)
    ];

    public static IEnumerable<object[]> NonIncreasingCases()
        => Cases.Select(c => (c.Sequence, c.NonIncreasing))
            .ToXUnitTestCases();

    public static IEnumerable<object[]> DecreasingCases()
        => Cases.Select(c => (c.Sequence, c.Decreasing))
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NonIncreasingCases))]
    public void NonIncreasingMatchesExpectedOrder(Sequence sequence, bool expected)
    {
        Assert.Equal(expected, sequence.IsNonIncreasing);
        Assert.True(sequence._isNonIncreasing!.IsValueCreated);
        Assert.Equal(expected, sequence.IsNonIncreasing);
        sequence.ResetCachedProperties();
        Assert.Null(sequence._isNonIncreasing);
        Assert.Equal(expected, sequence.IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(DecreasingCases))]
    public void DecreasingMatchesExpectedStrictOrder(Sequence sequence, bool expected)
    {
        Assert.Equal(expected, sequence.IsDecreasing);
        Assert.True(sequence._isDecreasing!.IsValueCreated);
        Assert.Equal(expected, sequence.IsDecreasing);
        sequence.ResetCachedProperties();
        Assert.Null(sequence._isDecreasing);
        Assert.Equal(expected, sequence.IsDecreasing);
    }
}
