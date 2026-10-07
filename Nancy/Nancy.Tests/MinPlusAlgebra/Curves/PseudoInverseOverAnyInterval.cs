using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the pseudo-inverses over intervals bounded or not, open or closed at either end, on the examples of [TBP-PI-INT], under the names it gives them.
/// </summary>
public class PseudoInverseOverAnyInterval
{
    private static readonly Rational Inf = Rational.PlusInfinity;
    private static readonly Rational MinusInf = Rational.MinusInfinity;

    /// <summary>$\min(t, 3)$.</summary>
    private static readonly Curve RampToThree = Curve.Minimum(new RateLatencyServiceCurve(1, 0), new ConstantCurve(3));

    public static List<(string name, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, Rational y, Rational lower, Rational upper)> ValueCases =
    [
        ("PI-closed", 1, 2, true, true, new Rational(3, 2), new Rational(3, 2), new Rational(3, 2)),
        ("PI-closed", 1, 2, true, true, 2, 2, 2),
        ("PI-closed", 1, 2, true, true, new Rational(5, 2), Inf, 2),
        ("PI-closed", 1, 2, true, true, new Rational(1, 2), Inf, MinusInf),
        ("PI-open-end", 1, 2, true, false, 2, Inf, 2),
        ("PI-open-end", 1, 2, true, false, new Rational(5, 2), Inf, 2),
        ("PI-open-start", 1, 2, false, true, 1, 1, MinusInf),
        ("PI-open-start", 1, 2, false, true, new Rational(1, 2), Inf, MinusInf),
        ("PI-open-start-plateau", 3, 5, false, true, 3, 3, 5),
        ("PI-open-start-plateau", 3, 5, false, true, 4, Inf, 5),
        ("PI-unbounded", 1, null, true, false, 4, Inf, Inf),
    ];

    public static IEnumerable<object[]> ValueTestCases()
        => ValueCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ValueTestCases))]
    public void ValuesOfMinOfTAndThree(string name, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, Rational y, Rational lower, Rational upper)
    {
        Assert.False(string.IsNullOrEmpty(name));
        Assert.Equal(lower, RampToThree.LowerPseudoInverseOverInterval(start, end, isStartIncluded, isEndIncluded).ValueAt(y));
        Assert.Equal(upper, RampToThree.UpperPseudoInverseOverInterval(start, end, isStartIncluded, isEndIncluded).ValueAt(y));
    }

    /// <summary>
    /// PI-decomposition: $g$ is $t$ before 1 and $t + 1$ from 1 on, so it jumps at 1.
    /// At $3/2$, the upper pseudo-inverse of $g$ is the maximum of those over $[0, 1[$ and $[1, +\infty[$, as the jump does not break the decomposition.
    /// </summary>
    [Fact]
    public void DecompositionAcrossAJump()
    {
        var g = new Curve(
            new Sequence([Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 2), new Segment(1, 2, 2, 1)]),
            pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1);
        var y = new Rational(3, 2);

        Assert.Equal(1, g.UpperPseudoInverse().ValueAt(y));
        Assert.Equal(1, g.UpperPseudoInverseOverInterval(0, 1, true, false).ValueAt(y));
        Assert.Equal(MinusInf, g.UpperPseudoInverseOverInterval(1).ValueAt(y));

        Assert.Equal(1, g.LowerPseudoInverse().ValueAt(y));
        Assert.Equal(Inf, g.LowerPseudoInverseOverInterval(0, 1, true, false).ValueAt(y));
        Assert.Equal(Inf, g.LowerPseudoInverseOverInterval(1).ValueAt(y));
    }
}
