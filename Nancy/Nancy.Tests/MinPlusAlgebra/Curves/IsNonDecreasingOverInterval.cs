using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class IsNonDecreasingOverInterval
{
    /// <summary>2 over $[0, 5[$, 1 from 5: decreasing at 5, before the pseudo-period.</summary>
    private static readonly Curve DropInTransient = new Curve(
        new Sequence(new List<Element>
        {
            new Point(0, 2), Segment.Constant(0, 5, 2), new Point(5, 1), Segment.Constant(5, 6, 1)
        }),
        pseudoPeriodStart: 5,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 0
    );

    /// <summary>5 at 0, $t$ after it: decreasing only at the origin.</summary>
    private static readonly Curve DropAtOrigin = new Curve(
        new Sequence(new List<Element> { new Point(0, 5), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 1, 1) }),
        pseudoPeriodStart: 1,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1
    );

    /// <summary>
    /// 0 over $[0, 1[$, then in each pseudo-period $[k, k + 1[$, $k \ge 1$, $k + 4$ at $k$ and rising from $k - 1$ after it:
    /// decreasing right after each $k$.
    /// </summary>
    private static readonly Curve DropAfterEachPeriodStart = new Curve(
        new Sequence(new List<Element>
        {
            Point.Origin(), Segment.Zero(0, 1), new Point(1, 5), new Segment(1, 2, 0, 1)
        }),
        pseudoPeriodStart: 1,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 1
    );

    public static List<(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)> Cases = new()
    {
        (DropInTransient, 1, null, true, false, false),
        (DropInTransient, 1, 6, true, false, false),
        (DropInTransient, 5, null, true, false, true),
        (DropAtOrigin, 0, null, false, false, true),
        (DropAtOrigin, 0, null, true, false, false),
        // excluding 1 leaves the drops after 2, 3, ...
        (DropAfterEachPeriodStart, 1, null, false, false, false),
        (DropAfterEachPeriodStart, 1, null, true, false, false),
        (new RateLatencyServiceCurve(2, 1), 1, null, true, false, true),
    };

    public static IEnumerable<object[]> TestCases()
        => Cases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(TestCases))]
    public void IsNonDecreasingOverIntervalTest(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)
    {
        Assert.Equal(expected, curve.IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded));
    }
}