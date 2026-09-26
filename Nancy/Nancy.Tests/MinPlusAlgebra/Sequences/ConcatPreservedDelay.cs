using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// With the delay preserved, the two operands of a concatenation do not meet:
/// for $a$ over $[x, y]$ and $b$ over $[z, w]$ the result is $+\infty$ over $]y, y + z[$, and the endpoints belong to the operands rather than to the gap.
/// </summary>
public class ConcatPreservedDelay
{
    // Right-open, so the sequence that follows can be left-closed at the join.
    private static readonly Sequence A =
        new([Point.Origin(), new Segment(0, 2, 0, 1)]);

    // Starts at 1, so there is a delay to preserve, and at 10, so there is a shift to remove.
    private static readonly Sequence B =
        new([new Point(1, 10), new Segment(1, 3, 10, 2)]);

    [Fact]
    public void ThePreservedDelayIsInfiniteOverTheOpenInterval()
    {
        var result = Sequence.Concat(A, B, preserveDelay: true);

        Assert.Equal(0, result.DefinedFrom);
        Assert.Equal(5, result.DefinedUntil);

        // y belongs to a, which ends there with value 2.
        Assert.Equal(2, result.ValueAt(2));

        // ]y, y + z[ is the gap.
        Assert.True(result.ValueAt(new Rational(9, 4)).IsPlusInfinite);
        Assert.True(result.ValueAt(new Rational(5, 2)).IsPlusInfinite);
        Assert.True(result.ValueAt(new Rational(11, 4)).IsPlusInfinite);

        // y + z belongs to b, whose own shift was removed, so it continues from where a stopped.
        Assert.Equal(2, result.ValueAt(3));
        Assert.Equal(4, result.ValueAt(4));
    }

    [Fact]
    public void WithoutThePreservedDelayThereIsNoGap()
    {
        var result = Sequence.Concat(A, B, preserveDelay: false);

        Assert.Equal(0, result.DefinedFrom);
        Assert.Equal(4, result.DefinedUntil);
        Assert.Equal(2, result.ValueAt(2));
        Assert.Equal(3, result.ValueAt(new Rational(5, 2)));
    }

    /// <summary>
    /// Where the following sequence does not define its own left endpoint, there is no value of it to take at the end of the gap, and the point there is infinite as well.
    /// </summary>
    [Fact]
    public void AFollowingSequenceOpenOnTheLeftLeavesTheJoinInfinite()
    {
        var leftOpen = new Sequence([new Segment(1, 3, 10, 2)]);

        var result = Sequence.Concat(A, leftOpen, preserveDelay: true);

        Assert.Equal(2, result.ValueAt(2));
        Assert.True(result.ValueAt(new Rational(5, 2)).IsPlusInfinite);
        Assert.True(result.ValueAt(3).IsPlusInfinite);
        Assert.Equal(4, result.ValueAt(4));
    }

    /// <summary>
    /// The n-ary form folds the binary one, so it carries the same gap.
    /// </summary>
    [Fact]
    public void TheNAryFormCarriesThePreservedDelayToo()
    {
        var result = Sequence.Concat([A, B], preserveDelay: true);

        Assert.True(result.ValueAt(new Rational(5, 2)).IsPlusInfinite);
        Assert.Equal(2, result.ValueAt(3));
    }
}
