using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// A curve whose pseudo-period takes both $+\infty$ and $-\infty$ is rejected by its constructors.
/// </summary>
public class PeriodTakingBothInfinities
{
    /// <summary>+inf on [0, 1/2], -inf on (1/2, 1), repeated.</summary>
    private static readonly Sequence BothInfinities = new([
        Point.PlusInfinite(0), Segment.PlusInfinite(0, new Rational(1, 2)), Point.PlusInfinite(new Rational(1, 2)), Segment.MinusInfinite(new Rational(1, 2), 1)
    ]);

    /// <summary>0 at the origin, then +inf on (0, 1), 1 at 1, and -inf on (1, 2), repeated.</summary>
    private static readonly Sequence BothInfinitiesAndAFinitePoint = new([
        Point.Origin(), Segment.PlusInfinite(0, 1), new Point(1, 1), Segment.MinusInfinite(1, 2)
    ]);

    [Fact]
    public void ConstructorsRejectAPeriodTakingBothInfinities()
    {
        Assert.Throws<ArgumentException>(() => new Curve(BothInfinities, 0, 1, 0));
        Assert.Throws<ArgumentException>(() => new Curve(BothInfinities, 0, 1, Rational.PlusInfinity));
        Assert.Throws<ArgumentException>(() => new Curve(BothInfinitiesAndAFinitePoint, 0, 2, 0));
        Assert.Throws<ArgumentException>(() => new Curve(BothInfinitiesAndAFinitePoint, 0, 2, 0, isPartialCurve: true));
    }

    /// <summary>
    /// The period found after the transient reduction of nancy#173 took both infinities, from a curve given with a later start.
    /// </summary>
    [Fact]
    public void ConstructorsRejectAPeriodTakingBothInfinitiesAfterATransient()
    {
        var sequence = new Sequence([
            Point.Origin(), Segment.Zero(0, 1), new Point(1, 0), Segment.PlusInfinite(1, 2),
            Point.MinusInfinite(2), Segment.MinusInfinite(2, new Rational(5, 2)), Point.MinusInfinite(new Rational(5, 2)), Segment.PlusInfinite(new Rational(5, 2), 3),
            Point.MinusInfinite(3), Segment.MinusInfinite(3, new Rational(7, 2)), Point.MinusInfinite(new Rational(7, 2)), Segment.PlusInfinite(new Rational(7, 2), 4)
        ]);
        Assert.Throws<ArgumentException>(() => new Curve(sequence, 3, 1, 0));
    }

    [Fact]
    public void ConstructorsAcceptBothInfinitiesInTheTransientOrOneInThePeriod()
    {
        var transient = new Curve(new Sequence([
            Point.Origin(), Segment.PlusInfinite(0, 1), Point.PlusInfinite(1), Segment.MinusInfinite(1, 2), Point.Zero(2), Segment.Zero(2, 3)
        ]), 2, 1, 0);
        Assert.True(transient.HasPlusInfinity && transient.HasMinusInfinity);

        var oneInfinity = new Curve(new Sequence([
            Point.Origin(), Segment.PlusInfinite(0, 1), new Point(1, 1), Segment.PlusInfinite(1, 2)
        ]), 0, 2, 0);
        Assert.True(oneInfinity.HasPlusInfinity && !oneInfinity.HasMinusInfinity);
    }

    /// <summary>+inf on [0, 1[ and 1 on [1, 2[, repeated with height 2.</summary>
    private static readonly Curve PlusInfiniteThenOne = new(new Sequence([
        Point.PlusInfinite(0), Segment.PlusInfinite(0, 1), new Point(1, 1), Segment.Constant(1, 2, 1)
    ]), 0, 2, 2);

    /// <summary>0 on [0, 1[ and +inf, or -inf, on [1, 2[, repeated.</summary>
    private static Curve ZeroThenInfinite(Rational infinity) => new(new Sequence([
        Point.Origin(), Segment.Zero(0, 1), new Point(1, infinity), Segment.Constant(1, 2, infinity)
    ]), 0, 2, 0);

    /// <summary>
    /// Two valid curves whose sum, or difference, is defined at every time, +inf on [0, 1[ and -inf on [1, 2[ of each period, but has a period taking both infinities, which no curve can represent.
    /// </summary>
    [Fact]
    public void AdditionAndSubtractionRejectAResultWhosePeriodTakesBothInfinities()
    {
        Assert.Throws<ArgumentException>(() => Curve.Addition(PlusInfiniteThenOne, ZeroThenInfinite(Rational.MinusInfinity)));
        Assert.Throws<ArgumentException>(() => Curve.Subtraction(PlusInfiniteThenOne, ZeroThenInfinite(Rational.PlusInfinity)));
    }
}
