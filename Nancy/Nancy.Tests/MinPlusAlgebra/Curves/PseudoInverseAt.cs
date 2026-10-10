using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the pseudo-inverses of a curve at a height, $f^{-1}_\downarrow(y) = \inf \{ t \ge 0 \mid f(t) \ge y \}$ and $f^{-1}_\uparrow(y) = \sup \{ t \ge 0 \mid f(t) \le y \}$,
/// against values computed by hand, and that a curve that is not non-decreasing is rejected.
/// </summary>
public class PseudoInverseAt
{
    private static readonly Rational Inf = Rational.PlusInfinity;
    private static readonly Rational MinusInf = Rational.MinusInfinity;

    /// <summary>$t$ up to 1, 1 up to 2, then rising from 3 with slope 1 and jumping by 2 every period of length 1.</summary>
    private static readonly Curve PlateauAndJumps = new Curve(
        new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), Segment.Constant(1, 2, 1),
            new Point(2, 1), new Segment(2, 3, 3, 1)
        ]),
        pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 3);

    /// <summary>$t$ up to 2 excluded, 3 at 2, and $+\infty$ after.</summary>
    private static readonly Curve SlopeIntoPlusInfinity = new Curve(
        new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 3), Segment.PlusInfinite(2, 3),
            Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)
        ]),
        pseudoPeriodStart: 3, pseudoPeriodLength: 1, pseudoPeriodHeight: 0);

    public static List<(string name, Curve f, Rational y, Rational lower, Rational upper)> Cases =
    [
        // 2 (t - 3) from 3 on
        ("rate-latency, below f(0)", new RateLatencyServiceCurve(2, 3), -1, 0, MinusInf),
        ("rate-latency, at f(0), the end of the latency", new RateLatencyServiceCurve(2, 3), 0, 0, 3),
        ("rate-latency, on the slope", new RateLatencyServiceCurve(2, 3), new Rational(1, 2), new Rational(13, 4), new Rational(13, 4)),
        ("rate-latency, far on the slope", new RateLatencyServiceCurve(2, 3), 4, 5, 5),
        // 0 at 0, then k on ]2(k - 1), 2k]
        ("staircase, at f(0)", new StairCurve(1, 2), 0, 0, 0),
        ("staircase, at the first step, reached right after 0", new StairCurve(1, 2), 1, 0, 2),
        ("staircase, between steps", new StairCurve(1, 2), new Rational(3, 2), 2, 2),
        ("staircase, at the second step", new StairCurve(1, 2), 2, 2, 4),
        // t up to 2, then 2
        ("ultimately constant, on the slope", Curve.Minimum(new RateLatencyServiceCurve(1, 0), new ConstantCurve(2)), 1, 1, 1),
        ("ultimately constant, at the constant", Curve.Minimum(new RateLatencyServiceCurve(1, 0), new ConstantCurve(2)), 2, 2, Inf),
        ("ultimately constant, above it", Curve.Minimum(new RateLatencyServiceCurve(1, 0), new ConstantCurve(2)), 3, Inf, Inf),
        // the plateau at 1 ends at 2, where the curve jumps to 3; it is 4 at 3, where it jumps to 6
        ("plateau and jumps, at the plateau", PlateauAndJumps, 1, 1, 2),
        ("plateau and jumps, inside the first jump", PlateauAndJumps, 2, 2, 2),
        ("plateau and jumps, at the top of the first jump", PlateauAndJumps, 3, 2, 2),
        ("plateau and jumps, in the first period", PlateauAndJumps, new Rational(7, 2), new Rational(5, 2), new Rational(5, 2)),
        ("plateau and jumps, at the end of the first period", PlateauAndJumps, 4, 3, 3),
        ("plateau and jumps, inside the second jump", PlateauAndJumps, 5, 3, 3),
        // the fourth period: 10 at 5, then rising from 12 to 13
        ("plateau and jumps, inside a later jump", PlateauAndJumps, new Rational(23, 2), 5, 5),
        ("plateau and jumps, on a later slope", PlateauAndJumps, new Rational(25, 2), new Rational(11, 2), new Rational(11, 2)),
        // t up to 2 excluded, 3 at 2, +inf after
        ("into +inf, on the slope", SlopeIntoPlusInfinity, 1, 1, 1),
        ("into +inf, at its left limit", SlopeIntoPlusInfinity, 2, 2, 2),
        ("into +inf, at its value at 2", SlopeIntoPlusInfinity, 3, 2, 2),
        ("into +inf, above its value at 2", SlopeIntoPlusInfinity, 4, 2, 2),
        ("into +inf, at +inf", SlopeIntoPlusInfinity, Inf, 2, Inf),
        // 0 everywhere
        ("zero, below it", Curve.Zero(), -1, 0, MinusInf),
        ("zero, at it", Curve.Zero(), 0, 0, Inf),
        ("zero, above it", Curve.Zero(), 1, Inf, Inf),
        // t - 1, negative at first
        ("negative at first, below f(0)", new RateLatencyServiceCurve(1, 0).VerticalShift(-1), -2, 0, MinusInf),
        ("negative at first, at 0", new RateLatencyServiceCurve(1, 0).VerticalShift(-1), 0, 1, 1),
    ];

    public static IEnumerable<object[]> CaseData() => Cases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CaseData))]
    public void LowerPseudoInverseAtKnownValues(string name, Curve f, Rational y, Rational lower, Rational upper)
    {
        _ = upper;

        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(lower, f.LowerPseudoInverseAt(y));
    }

    [Theory]
    [MemberData(nameof(CaseData))]
    public void UpperPseudoInverseAtKnownValues(string name, Curve f, Rational y, Rational lower, Rational upper)
    {
        _ = lower;

        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(upper, f.UpperPseudoInverseAt(y));
    }

    /// <summary>$t$ up to 1 excluded, then 0: decreasing at 1.</summary>
    private static readonly Curve Decreasing = new Curve(
        new Sequence([Point.Origin(), new Segment(0, 1, 0, 1), Point.Origin().Delay(1), Segment.Zero(1, 2)]),
        pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 0);

    [Fact]
    public void ThrowsForACurveThatIsNotNonDecreasing()
    {
        Assert.Throws<ArgumentException>(() => Decreasing.LowerPseudoInverseAt(1));
        Assert.Throws<ArgumentException>(() => Decreasing.UpperPseudoInverseAt(1));
    }
}
