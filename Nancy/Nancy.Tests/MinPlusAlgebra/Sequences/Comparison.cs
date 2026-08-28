using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Tests of <see cref="Sequence.LessOrEqual"/> and <see cref="Sequence.GreaterOrEqual"/>, which
/// compare two sequences over the part of the time axis on which both are defined.
/// </summary>
public class Comparison
{
    /// <summary>
    /// Constant <paramref name="value"/> over $[$<paramref name="start"/>, <paramref name="end"/>$[$.
    /// </summary>
    private static Sequence Constant(Rational value, Rational start, Rational end)
        => new Sequence([
            new Point(start, value),
            Segment.Constant(start, end, value)
        ]);

    [Fact]
    public void OneBelowTheOther()
    {
        var low = Constant(1, 0, 5);
        var high = Constant(3, 0, 5);

        Assert.True(Sequence.LessOrEqual(low, high));
        Assert.False(Sequence.GreaterOrEqual(low, high));
        Assert.True(Sequence.GreaterOrEqual(high, low));
        Assert.False(Sequence.LessOrEqual(high, low));
    }

    [Fact]
    public void EqualSequences_AreBothLessAndGreaterOrEqual()
    {
        var a = Constant(2, 0, 5);
        var b = Constant(2, 0, 5);

        Assert.True(Sequence.LessOrEqual(a, b));
        Assert.True(Sequence.GreaterOrEqual(a, b));
    }

    [Fact]
    public void CrossingSequences_AreNeither()
    {
        // a rises from 0 to 4 while b falls from 4 to 0, so neither bounds the other
        var a = new Sequence([
            new Point(0, 0),
            new Segment(0, 4, 0, 1)
        ]);
        var b = new Sequence([
            new Point(0, 4),
            new Segment(0, 4, 4, -1)
        ]);

        Assert.False(Sequence.LessOrEqual(a, b));
        Assert.False(Sequence.GreaterOrEqual(a, b));
    }

    [Fact]
    public void ADifferenceAtASinglePoint_IsEnough()
    {
        // the two agree everywhere except at t = 2, where a is above b
        var a = new Sequence([
            new Point(0, 0),
            Segment.Constant(0, 2, 0),
            new Point(2, 1),
            Segment.Constant(2, 4, 0)
        ]);
        var b = Constant(0, 0, 4);

        Assert.True(Sequence.GreaterOrEqual(a, b));
        Assert.False(Sequence.LessOrEqual(a, b));
    }

    [Fact]
    public void OperandsOfDifferentSupports_AreComparedOverTheOverlap()
    {
        // 5 is above 1 wherever both are defined, whichever of the two is the first operand,
        // and what either does outside the overlap is not part of the comparison
        var longer = Constant(5, 0, 10);
        var shorter = Constant(1, 0, 4);

        Assert.True(Sequence.GreaterOrEqual(longer, shorter));
        Assert.False(Sequence.LessOrEqual(longer, shorter));

        Assert.True(Sequence.LessOrEqual(shorter, longer));
        Assert.False(Sequence.GreaterOrEqual(shorter, longer));
    }

    [Fact]
    public void AnOverlapAwayFromTheOrigin_IsComparedAllTheSame()
    {
        var a = Constant(5, 0, 10);
        var late = Constant(1, 6, 12);

        Assert.True(Sequence.GreaterOrEqual(a, late));
        Assert.False(Sequence.LessOrEqual(a, late));
    }

    [Fact]
    public void SequencesThatDoNotOverlap_Throw()
    {
        // as for the minimum and the maximum, which the comparison is built on
        var early = Constant(0, 0, 4);
        var late = Constant(0, 6, 10);

        Assert.Throws<ArgumentException>(() => Sequence.LessOrEqual(early, late));
        Assert.Throws<ArgumentException>(() => Sequence.GreaterOrEqual(early, late));
    }

    [Fact]
    public void InfiniteValues_AreOrderedAsExpected()
    {
        var finite = Constant(0, 0, 4);
        var plusInfinite = new Sequence([
            Point.PlusInfinite(0),
            Segment.PlusInfinite(0, 4)
        ]);
        var minusInfinite = new Sequence([
            Point.MinusInfinite(0),
            Segment.MinusInfinite(0, 4)
        ]);

        Assert.True(Sequence.GreaterOrEqual(plusInfinite, finite));
        Assert.False(Sequence.LessOrEqual(plusInfinite, finite));
        Assert.True(Sequence.LessOrEqual(minusInfinite, finite));
        Assert.False(Sequence.GreaterOrEqual(minusInfinite, finite));
    }

    [Fact]
    public void TheOperatorsAgreeWithTheMethods()
    {
        var low = Constant(1, 0, 5);
        var high = Constant(3, 0, 5);

        Assert.True(low <= high);
        Assert.True(high >= low);
        Assert.False(low >= high);
        Assert.False(high <= low);
    }
}
