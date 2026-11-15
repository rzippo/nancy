using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// An infinite pseudo-period height added to the opposite infinity has no value, so a curve whose pseudo-period takes the infinity opposite to its height has no meaning, and its constructors reject it.
/// </summary>
public class InfiniteHeightAgainstThePeriod
{
    /// <summary>+inf throughout [0, 1[.</summary>
    private static readonly Sequence PlusInfinite = new([Point.PlusInfinite(0), Segment.PlusInfinite(0, 1)]);

    /// <summary>-inf throughout [0, 1[.</summary>
    private static readonly Sequence MinusInfinite = new([Point.MinusInfinite(0), Segment.MinusInfinite(0, 1)]);

    /// <summary>0 on [0, 1/2[ and -inf on [1/2, 1[.</summary>
    private static readonly Sequence ZeroThenMinusInfinite = new([
        Point.Origin(), Segment.Zero(0, new Rational(1, 2)), Point.MinusInfinite(new Rational(1, 2)), Segment.MinusInfinite(new Rational(1, 2), 1)
    ]);

    [Fact]
    public void ConstructorsRejectAnInfiniteHeightOppositeToThePeriod()
    {
        Assert.Throws<ArgumentException>(() => new Curve(PlusInfinite, 0, 1, Rational.MinusInfinity));
        Assert.Throws<ArgumentException>(() => new Curve(PlusInfinite, 0, 1, Rational.MinusInfinity, isPartialCurve: true));
        Assert.Throws<ArgumentException>(() => new Curve(MinusInfinite, 0, 1, Rational.PlusInfinity));
        Assert.Throws<ArgumentException>(() => new Curve(MinusInfinite, 0, 1, Rational.PlusInfinity, isPartialCurve: true));
    }

    /// <summary>
    /// The same after a finite transient: 0 at the origin, then +inf, or -inf, on [0, 2[, with the opposite height from 1.
    /// </summary>
    [Fact]
    public void ConstructorsRejectAnInfiniteHeightOppositeToThePeriodAfterATransient()
    {
        foreach (var value in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        {
            var sequence = new Sequence([
                Point.Origin(), Segment.Constant(0, 1, value),
                new Point(1, value), Segment.Constant(1, 2, value)
            ]);
            Assert.Throws<ArgumentException>(() => new Curve(sequence, 1, 1, -value));
        }
    }

    /// <summary>
    /// A finite value in the period does not help: the period still takes the opposite infinity elsewhere.
    /// </summary>
    [Fact]
    public void ConstructorsRejectAnInfiniteHeightOppositeToPartOfThePeriod()
    {
        Assert.Throws<ArgumentException>(() => new Curve(ZeroThenMinusInfinite, 0, 1, Rational.PlusInfinity));
        Assert.Throws<ArgumentException>(() => new Curve(ZeroThenMinusInfinite, 0, 1, Rational.PlusInfinity, isPartialCurve: true));
    }

    /// <summary>
    /// The filling of a partial curve with +inf counts as part of the period.
    /// </summary>
    [Fact]
    public void ConstructorsRejectAnInfiniteHeightOppositeToTheFilling()
    {
        var zeroOnHalf = new Sequence([Point.Origin(), Segment.Zero(0, new Rational(1, 2))]);
        Assert.Throws<ArgumentException>(() => new Curve(zeroOnHalf, 0, 1, Rational.MinusInfinity, isPartialCurve: true));
    }

    [Fact]
    public void ConstructorsAcceptAnInfiniteHeightOfTheSameSignOrAFiniteOne()
    {
        var plusAndPlus = new Curve(PlusInfinite, 0, 1, Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, plusAndPlus.ValueAt(new Rational(5, 2)));

        var plusAndFinite = new Curve(PlusInfinite, 0, 1, -3);
        Assert.Equal(Rational.PlusInfinity, plusAndFinite.ValueAt(new Rational(5, 2)));

        // 0 then +inf, with height +inf: +inf from the end of the first period on
        var zeroThenPlus = new Curve(new Sequence([
            Point.Origin(), Segment.Zero(0, new Rational(1, 2)), Point.PlusInfinite(new Rational(1, 2)), Segment.PlusInfinite(new Rational(1, 2), 1)
        ]), 0, 1, Rational.PlusInfinity);
        Assert.Equal(Rational.Zero, zeroThenPlus.ValueAt(new Rational(1, 4)));
        Assert.Equal(Rational.PlusInfinity, zeroThenPlus.ValueAt(new Rational(5, 4)));

        // the opposite infinity in the transient only
        var minusThenPlus = new Curve(new Sequence([
            Point.MinusInfinite(0), Segment.MinusInfinite(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
        ]), 1, 1, Rational.PlusInfinity);
        Assert.Equal(Rational.MinusInfinity, minusThenPlus.ValueAt(new Rational(1, 2)));
        Assert.Equal(Rational.PlusInfinity, minusThenPlus.ValueAt(new Rational(5, 2)));
    }
}
