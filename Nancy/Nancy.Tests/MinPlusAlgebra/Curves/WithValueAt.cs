using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks that <see cref="Curve.WithValueAt"/> sets the value at the instant and changes nothing else,
/// at values computed by hand and over a grid of curves and instants.
/// </summary>
public class WithValueAt
{
    private static readonly Rational Inf = Rational.PlusInfinity;

    /// <summary>$k + 1$ at each integer $k$, and $t$ elsewhere: a point above the line at every instant of the period.</summary>
    internal static readonly Curve PointsAbove = new Curve(
        new Sequence([new Point(0, 1), new Segment(0, 1, 0, 1)]),
        pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

    /// <summary>$t$, except 5 at 1, and from 2 on rising by 1 every period of length 1.</summary>
    internal static readonly Curve PointAboveInTransient = new Curve(
        new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 5), new Segment(1, 2, 1, 1),
            new Point(2, 2), new Segment(2, 3, 2, 1)
        ]),
        pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);

    /// <summary>$t$ up to 2, and $+\infty$ after.</summary>
    internal static readonly Curve SlopeIntoPlusInfinity = new Curve(
        new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.PlusInfinite(2, 3),
            Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)
        ]),
        pseudoPeriodStart: 3, pseudoPeriodLength: 1, pseudoPeriodHeight: 0);

    public static List<Curve> GridCurves =
    [
        ..WithOriginAt.Curves,
        new StairCurve(1, 2),
        new SigmaRhoArrivalCurve(2, 1),
        new RateLatencyServiceCurve(2, 3),
        PointsAbove,
        PointAboveInTransient,
        SlopeIntoPlusInfinity,
    ];

    public static List<Rational> GridTimes = [0, new(1, 2), 1, 2, new(5, 2), 3, new(7, 2), 6, 10];

    public static IEnumerable<object[]> GridData()
        => GridCurves.SelectMany(f => GridTimes.Select(t => new object[] { f, t }));

    /// <summary>Returns the instants where the result must match the curve: a grid over two pseudo-periods past <paramref name="t"/>, and instants just before and after it.</summary>
    internal static IEnumerable<Rational> Samples(Curve f, Rational t)
    {
        var horizon = f.SecondPseudoPeriodEnd + t + 2;
        var times = Enumerable.Range(0, (int)(horizon * 4).Ceil() + 1).Select(i => new Rational(i, 4));
        var near = new[] { t - new Rational(1, 1000), t + new Rational(1, 1000) }.Where(x => x >= 0);
        return times.Concat(near).Where(x => x != t);
    }

    /// <summary>
    /// Lists values to set, with the results computed by hand, including values in the middle of a jump.
    /// </summary>
    public static List<(string name, Curve f, Rational t, Rational value, bool isLeftContinuous, bool isRightContinuous, (Rational time, Rational value)[] others)> ValueCases =
    [
        // 0 at 0, then k on ]2(k - 1), 2k]
        ("staircase, in the middle of a step in the transient", new StairCurve(1, 2), 2, new Rational(3, 2), false, false, [(1, 1), (3, 2), (4, 2)]),
        ("staircase, in the middle of a step in a later period", new StairCurve(1, 2), 6, new Rational(7, 2), false, false, [(4, 2), (5, 3), (7, 4), (8, 4)]),
        ("staircase, at its left limit", new StairCurve(1, 2), 4, 2, true, false, [(3, 2), (5, 3)]),
        // 2 + t, 0 at 0
        ("sigma-rho, in the middle of the jump at 0", new SigmaRhoArrivalCurve(2, 1), 0, 1, false, false, [(1, 3)]),
        ("sigma-rho, a spike where continuous", new SigmaRhoArrivalCurve(2, 1), 3, 10, false, false, [(2, 4), (4, 6)]),
        ("points above, at the line in a later period", PointsAbove, 3, 3, true, true, [(2, 3), (4, 5)]),
        ("into +inf, between its last finite value and +inf", SlopeIntoPlusInfinity, 2, 5, false, false, [(1, 1), (3, Inf)]),
        ("into +inf, at +inf", SlopeIntoPlusInfinity, 2, Inf, false, true, [(1, 1), (3, Inf)]),
    ];

    public static IEnumerable<object[]> ValueCaseData()
        => ValueCases.Select(c => new object[] { c.name, c.f, c.t, c.value, c.isLeftContinuous, c.isRightContinuous, c.others });

    [Theory]
    [MemberData(nameof(ValueCaseData))]
    public void WithValueAtKnownValues(
        string name, Curve f, Rational t, Rational value, bool isLeftContinuous, bool isRightContinuous, (Rational time, Rational value)[] others
    )
    {
        Assert.False(string.IsNullOrEmpty(name));
        var result = f.WithValueAt(t, value);
        Assert.Equal(value, result.ValueAt(t));
        if (t > 0)
            Assert.Equal(isLeftContinuous, result.IsLeftContinuousAt(t));
        Assert.Equal(isRightContinuous, result.IsRightContinuousAt(t));
        foreach (var (time, v) in others)
            Assert.Equal(v, result.ValueAt(time));
    }

    /// <summary>Returns the values to set at an instant: one above the value there, and, where both limits are finite, the middle of the jump between them.</summary>
    private static IEnumerable<Rational> ValuesToSet(Curve f, Rational t)
    {
        var value = f.ValueAt(t);
        if (value.IsFinite)
            yield return value + 1;
        if (t > 0 && f.LeftLimitAt(t).IsFinite && f.RightLimitAt(t).IsFinite)
            yield return (f.LeftLimitAt(t) + f.RightLimitAt(t)) / 2;
    }

    [Theory]
    [MemberData(nameof(GridData))]
    public void WithValueAtOnlyChangesTheValueAtTheInstant(Curve f, Rational t)
    {
        Assert.Same(f, f.WithValueAt(t, f.ValueAt(t)));
        foreach (var value in ValuesToSet(f, t))
        {
            var result = f.WithValueAt(t, value);
            Assert.Equal(value, result.ValueAt(t));
            Assert.Equal(f.RightLimitAt(t), result.RightLimitAt(t));
            if (t > 0)
                Assert.Equal(f.LeftLimitAt(t), result.LeftLimitAt(t));
            foreach (var x in Samples(f, t))
                Assert.Equal(f.ValueAt(x), result.ValueAt(x));
        }
    }
}
