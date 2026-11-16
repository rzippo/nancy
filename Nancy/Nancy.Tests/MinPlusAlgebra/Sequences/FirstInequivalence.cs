using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// <see cref="Sequence.FindFirstInequivalence"/> returns the earliest time at or right after which the two sequences differ, being defined at one and not the other included, and null if they are the same function.
/// </summary>
public class FirstInequivalence
{
    /// <summary>$t$ over $[0, end]$, as one segment.</summary>
    private static Sequence Rate(Rational end)
        => new([Point.Origin(), new Segment(0, end, 0, 1), new Point(end, end)]);

    /// <summary>$t$ over $[0, end]$, split at 1 with a point there.</summary>
    private static Sequence RateSplitAtOne(Rational end)
        => new([Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, end, 1, 1), new Point(end, end)]);

    public static IEnumerable<object[]> Cases()
    {
        // the same function, split differently: nancy#181
        yield return ["t, as one segment and split at 1", Rate(2), RateSplitAtOne(2), null!];
        // split differently, and defined further: they differ only past 2
        yield return ["t, split differently, on [0, 2] and [0, 3]", Rate(2), RateSplitAtOne(3), (Rational?) 2];
        // t, then 2t - 1 from 1
        yield return ["t, and a steeper rate from 1", Rate(2),
            new Sequence([Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 2), new Point(2, 3)]), (Rational?) 1];
        yield return ["t, on [0, 2] and [0, 3]", Rate(2), Rate(3), (Rational?) 2];
        yield return ["t, on [1, 2] and [0, 2]",
            new Sequence([new Point(1, 1), new Segment(1, 2, 1, 1), new Point(2, 2)]), Rate(2), (Rational?) 0];
        yield return ["t, on [0, 2] and [0, 2[", Rate(2), new Sequence([Point.Origin(), new Segment(0, 2, 0, 1)]), (Rational?) 2];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void FindFirstInequivalenceIsTheEarliestDifference(string id, Sequence a, Sequence b, Rational? expected)
    {
        var ab = Sequence.FindFirstInequivalence(a, b);
        var ba = Sequence.FindFirstInequivalence(b, a);
        Assert.True(ab == expected, $"{id}: {ab}, not {expected}");
        Assert.True(ba == expected, $"{id}, swapped: {ba}, not {expected}");
    }
}
