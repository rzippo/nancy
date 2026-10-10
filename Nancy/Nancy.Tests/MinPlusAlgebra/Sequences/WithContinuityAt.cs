using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks that <see cref="Sequence.WithRightContinuousAt"/> and <see cref="Sequence.WithLeftContinuousAt"/> set the value at the instant to the right or left limit and change nothing else,
/// at values computed by hand and over the grid of <see cref="WithValueAt"/>, and that they throw where the instant is outside the support or the limit is missing.
/// </summary>
public class WithContinuityAt
{
    private static readonly Rational Inf = Rational.PlusInfinity;

    public static List<(string name, Sequence f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)> KnownCases =
    [
        ("staircase, at a step", WithValueAt.Staircase, 2, 2, 1, [(1, 1), (3, 2), (4, 2)]),
        ("staircase, at another step", WithValueAt.Staircase, 4, 3, 2, [(3, 2), (5, 3), (6, 3)]),
        ("open staircase, at a step", WithValueAt.OpenStaircase, 4, 3, 2, [(2, 1), (3, 2), (new Rational(9, 2), 3)]),
        ("into +inf, at the jump to +inf", WithValueAt.SlopeIntoPlusInfinity, 2, Inf, 2, [(1, 1), (new Rational(5, 2), Inf)]),
    ];

    public static IEnumerable<object[]> KnownCaseData()
        => KnownCases.Select(c => new object[] { c.name, c.f, c.t, c.right, c.left, c.others });

    [Theory]
    [MemberData(nameof(KnownCaseData))]
    public void WithRightContinuousAtKnownValues(string name, Sequence f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)
    {
        _ = left;

        Assert.False(string.IsNullOrEmpty(name));
        var result = f.WithRightContinuousAt(t);
        Assert.Equal(right, result.ValueAt(t));
        foreach (var (time, value) in others)
            Assert.Equal(value, result.ValueAt(time));
    }

    [Theory]
    [MemberData(nameof(KnownCaseData))]
    public void WithLeftContinuousAtKnownValues(string name, Sequence f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)
    {
        _ = right;

        Assert.False(string.IsNullOrEmpty(name));
        var result = f.WithLeftContinuousAt(t);
        Assert.Equal(left, result.ValueAt(t));
        foreach (var (time, value) in others)
            Assert.Equal(value, result.ValueAt(time));
    }

    [Fact]
    public void ThrowsOutsideTheSupportAndWhereALimitIsMissing()
    {
        // at an excluded end
        Assert.Throws<ArgumentException>(() => WithValueAt.OpenStaircase.WithRightContinuousAt(1));
        Assert.Throws<ArgumentException>(() => WithValueAt.OpenStaircase.WithLeftContinuousAt(5));
        // no left limit at the start, and no right limit at the end
        Assert.Throws<ArgumentException>(() => WithValueAt.Staircase.WithLeftContinuousAt(0));
        Assert.Throws<ArgumentException>(() => WithValueAt.Staircase.WithRightContinuousAt(6));
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void WithRightContinuousAtOnlyChangesTheValueAtTheInstant(Sequence f, Rational t)
    {
        if (t == f.DefinedUntil)
        {
            Assert.Throws<ArgumentException>(() => f.WithRightContinuousAt(t));
            return;
        }
        var result = f.WithRightContinuousAt(t);
        WithValueAt.AssertOnlyTheValueAtTheInstantChanges(f, result, t, f.RightLimitAt(t));
        Assert.True(result.IsRightContinuousAt(t));
        if (f.IsRightContinuousAt(t))
            Assert.Same(f, result);
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void WithLeftContinuousAtOnlyChangesTheValueAtTheInstant(Sequence f, Rational t)
    {
        if (t == f.DefinedFrom)
        {
            Assert.Throws<ArgumentException>(() => f.WithLeftContinuousAt(t));
            return;
        }
        var result = f.WithLeftContinuousAt(t);
        WithValueAt.AssertOnlyTheValueAtTheInstantChanges(f, result, t, f.LeftLimitAt(t));
        Assert.True(result.IsLeftContinuousAt(t));
        if (f.IsLeftContinuousAt(t))
            Assert.Same(f, result);
    }
}
