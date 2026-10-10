using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks that <see cref="Curve.WithRightContinuousAt"/> and <see cref="Curve.WithLeftContinuousAt"/> set the value at the instant to the right or left limit and change nothing else,
/// at values computed by hand and over the grid of <see cref="WithValueAt"/>.
/// </summary>
public class WithContinuityAt
{
    private static readonly Rational Inf = Rational.PlusInfinity;

    /// <summary>
    /// Lists, for each curve and instant, the values after each operation, computed by hand.
    /// </summary>
    public static List<(string name, Curve f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)> KnownCases =
    [
        // 0 at 0, then k on ]2(k - 1), 2k]: left-continuous everywhere
        ("staircase, at a step in the transient", new StairCurve(1, 2), 2, 2, 1, [(1, 1), (3, 2), (4, 2)]),
        ("staircase, at a step in a later period", new StairCurve(1, 2), 6, 4, 3, [(4, 2), (5, 3), (8, 4)]),
        ("staircase, at 0", new StairCurve(1, 2), 0, 1, 0, [(1, 1), (2, 1)]),
        // 2 + t, 0 at 0
        ("sigma-rho, at 0", new SigmaRhoArrivalCurve(2, 1), 0, 2, 0, [(1, 3)]),
        ("sigma-rho, where continuous", new SigmaRhoArrivalCurve(2, 1), 3, 5, 5, [(2, 4), (4, 6)]),
        ("rate-latency, at the latency", new RateLatencyServiceCurve(2, 3), 3, 0, 0, [(4, 2)]),
        ("points above, at 0", WithValueAt.PointsAbove, 0, 0, 1, [(1, 2), (2, 3)]),
        ("points above, in a later period", WithValueAt.PointsAbove, 3, 3, 3, [(2, 3), (4, 5), (7, 8), (new Rational(7, 2), new Rational(7, 2))]),
        ("point above in the transient", WithValueAt.PointAboveInTransient, 1, 1, 1, [(new Rational(1, 2), new Rational(1, 2)), (2, 2), (5, 5)]),
        // f(2) = 2 is the left limit, and +inf the right one
        ("into +inf, at its last finite instant", WithValueAt.SlopeIntoPlusInfinity, 2, Inf, 2, [(1, 1), (3, Inf)]),
    ];

    public static IEnumerable<object[]> KnownCaseData()
        => KnownCases.Select(c => new object[] { c.name, c.f, c.t, c.right, c.left, c.others });

    [Theory]
    [MemberData(nameof(KnownCaseData))]
    public void WithRightContinuousAtKnownValues(string name, Curve f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)
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
    public void WithLeftContinuousAtKnownValues(string name, Curve f, Rational t, Rational right, Rational left, (Rational time, Rational value)[] others)
    {
        _ = right;

        Assert.False(string.IsNullOrEmpty(name));
        if (t == 0)
        {
            Assert.Throws<ArgumentException>(() => f.WithLeftContinuousAt(t));
            return;
        }
        var result = f.WithLeftContinuousAt(t);
        Assert.Equal(left, result.ValueAt(t));
        foreach (var (time, value) in others)
            Assert.Equal(value, result.ValueAt(time));
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void WithRightContinuousAtOnlyChangesTheValueAtTheInstant(Curve f, Rational t)
    {
        var result = f.WithRightContinuousAt(t);
        Assert.Equal(f.RightLimitAt(t), result.ValueAt(t));
        Assert.Equal(f.RightLimitAt(t), result.RightLimitAt(t));
        if (t > 0)
            Assert.Equal(f.LeftLimitAt(t), result.LeftLimitAt(t));
        foreach (var x in WithValueAt.Samples(f, t))
            Assert.Equal(f.ValueAt(x), result.ValueAt(x));
        Assert.True(result.IsRightContinuousAt(t));
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void WithLeftContinuousAtOnlyChangesTheValueAtTheInstant(Curve f, Rational t)
    {
        if (t == 0)
        {
            Assert.Throws<ArgumentException>(() => f.WithLeftContinuousAt(t));
            return;
        }
        var result = f.WithLeftContinuousAt(t);
        Assert.Equal(f.LeftLimitAt(t), result.ValueAt(t));
        Assert.Equal(f.LeftLimitAt(t), result.LeftLimitAt(t));
        Assert.Equal(f.RightLimitAt(t), result.RightLimitAt(t));
        foreach (var x in WithValueAt.Samples(f, t))
            Assert.Equal(f.ValueAt(x), result.ValueAt(x));
        Assert.True(result.IsLeftContinuousAt(t));
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void ACurveAlreadyContinuousAtTheInstantIsReturnedAsItIs(Curve f, Rational t)
    {
        var right = f.WithRightContinuousAt(t);
        if (f.IsRightContinuousAt(t))
            Assert.Same(f, right);
        else
            Assert.Same(right, right.WithRightContinuousAt(t));
        if (t > 0 && f.IsLeftContinuousAt(t))
            Assert.Same(f, f.WithLeftContinuousAt(t));
    }

    [Theory]
    [MemberData(nameof(WithValueAt.GridData), MemberType = typeof(WithValueAt))]
    public void AtTheOriginItIsWithOriginRightContinuous(Curve f, Rational t)
    {
        if (t != 0)
            return;

        Assert.True(Curve.Equivalent(f.WithOriginRightContinuous(), f.WithRightContinuousAt(0)));
    }

}
