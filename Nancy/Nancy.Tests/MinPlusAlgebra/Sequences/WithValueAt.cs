using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks that <see cref="Sequence.WithValueAt"/> sets the value at the instant and changes nothing else,
/// at values computed by hand and over a grid of sequences and instants, and that it throws where the instant is outside the support.
/// </summary>
public class WithValueAt
{
    /// <summary>0 at 0, then k on ]2(k - 1), 2k], over [0, 6].</summary>
    internal static readonly Sequence Staircase = new StairCurve(1, 2).Cut(0, 6, true, true);

    /// <summary>The same staircase over ]1, 5[.</summary>
    internal static readonly Sequence OpenStaircase = new StairCurve(1, 2).Cut(1, 5, false, false);

    /// <summary>$t$ over $[0, 2[$, 3 at 2, and $+\infty$ over $]2, 3]$.</summary>
    internal static readonly Sequence SlopeIntoPlusInfinity = new Sequence([
        Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 3), Segment.PlusInfinite(2, 3), Point.PlusInfinite(3)
    ]);

    [Fact]
    public void WithValueAtInTheMiddleOfAJump()
    {
        var result = Staircase.WithValueAt(2, new Rational(3, 2));
        Assert.Equal(new Rational(3, 2), result.ValueAt(2));
        Assert.False(result.IsLeftContinuousAt(2));
        Assert.False(result.IsRightContinuousAt(2));
        Assert.Equal(1, result.ValueAt(1));
        Assert.Equal(2, result.ValueAt(3));
    }

    [Fact]
    public void WithValueAtTheEndsOfTheSupport()
    {
        var atStart = Staircase.WithValueAt(0, 5);
        Assert.Equal(5, atStart.ValueAt(0));
        Assert.Equal(1, atStart.ValueAt(1));
        var atEnd = Staircase.WithValueAt(6, 5);
        Assert.Equal(5, atEnd.ValueAt(6));
        Assert.Equal(3, atEnd.ValueAt(5));
    }

    [Fact]
    public void ThrowsOutsideTheSupport()
    {
        // past the end, and at an excluded end
        Assert.Throws<ArgumentException>(() => Staircase.WithValueAt(7, 1));
        Assert.Throws<ArgumentException>(() => OpenStaircase.WithValueAt(1, 1));
        Assert.Throws<ArgumentException>(() => OpenStaircase.WithValueAt(5, 1));
    }

    public static List<Sequence> GridSequences =
    [
        Staircase,
        OpenStaircase,
        new StairCurve(1, 2).Cut(2, 4, true, false),
        new SigmaRhoArrivalCurve(2, 1).Cut(0, 3, true, true),
        new RateLatencyServiceCurve(2, 3).Cut(1, 5, false, true),
        SlopeIntoPlusInfinity,
        new Sequence([new Point(2, 5)]),
    ];

    public static IEnumerable<object[]> GridData()
        => GridSequences.SelectMany(f => Enumerable.Range(0, 13)
            .Select(i => new Rational(i, 2))
            .Where(f.IsDefinedAt)
            .Select(t => new object[] { f, t }));

    /// <summary>Returns the instants of the support other than <paramref name="t"/>, where the result must match the sequence.</summary>
    internal static IEnumerable<Rational> Samples(Sequence f, Rational t)
        => Enumerable.Range(0, 49)
            .Select(i => new Rational(i, 8))
            .Concat([t - new Rational(1, 1000), t + new Rational(1, 1000)])
            .Where(x => x != t && f.IsDefinedAt(x));

    internal static void AssertOnlyTheValueAtTheInstantChanges(Sequence f, Sequence result, Rational t, Rational value)
    {
        Assert.Equal(value, result.ValueAt(t));
        Assert.Equal(f.DefinedFrom, result.DefinedFrom);
        Assert.Equal(f.DefinedUntil, result.DefinedUntil);
        Assert.Equal(f.IsLeftClosed, result.IsLeftClosed);
        Assert.Equal(f.IsRightClosed, result.IsRightClosed);
        if (t > f.DefinedFrom)
            Assert.Equal(f.LeftLimitAt(t), result.LeftLimitAt(t));
        if (t < f.DefinedUntil)
            Assert.Equal(f.RightLimitAt(t), result.RightLimitAt(t));
        foreach (var x in Samples(f, t))
            Assert.Equal(f.ValueAt(x), result.ValueAt(x));
    }

    [Theory]
    [MemberData(nameof(GridData))]
    public void WithValueAtOnlyChangesTheValueAtTheInstant(Sequence f, Rational t)
    {
        Assert.Same(f, f.WithValueAt(t, f.ValueAt(t)));
        var value = f.ValueAt(t);
        if (value.IsFinite)
            AssertOnlyTheValueAtTheInstantChanges(f, f.WithValueAt(t, value + 1), t, value + 1);
        if (t > f.DefinedFrom && t < f.DefinedUntil && f.LeftLimitAt(t).IsFinite && f.RightLimitAt(t).IsFinite)
        {
            var middle = (f.LeftLimitAt(t) + f.RightLimitAt(t)) / 2;
            AssertOnlyTheValueAtTheInstantChanges(f, f.WithValueAt(t, middle), t, middle);
        }
    }
}
