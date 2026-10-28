using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveMonotonicity
{
    private static Curve PeriodicRamp(int slope, int height) => new(
        new Sequence([new Point(0, 0), new Segment(0, 1, 0, slope)]), 0, 1, height);

    public static IEnumerable<(Curve Curve, bool NonIncreasing, bool Decreasing)> Cases =>
    [
        (PeriodicRamp(-1, -1), true, true),
        (PeriodicRamp(-1, -2), true, true),
        (PeriodicRamp(-1, 0), false, false),
        (PeriodicRamp(0, -1), true, false),
        (PeriodicRamp(0, 0), true, false),
        (PeriodicRamp(0, 1), false, false),
        (PeriodicRamp(1, -1), false, false),
        (Curve.PlusInfinite(), true, false),
        (Curve.MinusInfinite(), true, false),
        (new Curve(new Sequence([
            new Point(0, 0), new Segment(0, 1, 0, 1),
            new Point(1, 1), new Segment(1, 2, 1, -1)
        ]), 1, 1, -1), false, false),
        (new Curve(new Sequence([
            new Point(0, 0), new Segment(0, 1, 0, -1),
            new Point(1, -1), new Segment(1, 2, -1, -1)
        ]), 1, 1, -1), true, true)
    ];

    public static IEnumerable<object[]> NonIncreasingCases()
        => Cases.Select(c => (c.Curve, c.NonIncreasing))
            .ToXUnitTestCases();

    public static IEnumerable<object[]> DecreasingCases()
        => Cases.Select(c => (c.Curve, c.Decreasing))
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NonIncreasingCases))]
    public void NonIncreasingCoversTransientAndPeriodSeams(Curve curve, bool expected)
    {
        Assert.Equal(expected, curve.IsNonIncreasing);
        Assert.True(curve._isNonIncreasing!.IsValueCreated);
        curve.ResetCachedProperties();
        Assert.Null(curve._isNonIncreasing);
        Assert.Equal(expected, curve.IsNonIncreasing);
    }

    [Theory]
    [MemberData(nameof(DecreasingCases))]
    public void DecreasingCoversTransientAndPeriodSeams(Curve curve, bool expected)
    {
        Assert.Equal(expected, curve.IsDecreasing);
        Assert.True(curve._isDecreasing!.IsValueCreated);
        curve.ResetCachedProperties();
        Assert.Null(curve._isDecreasing);
        Assert.Equal(expected, curve.IsDecreasing);
    }
}
