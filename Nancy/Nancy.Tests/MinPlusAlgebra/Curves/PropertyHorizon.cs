using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class PropertyHorizon
{
    public static IEnumerable<(Curve Curve, Rational Start)> PeriodicRightDrops()
    {
        yield return (new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 0, 3)]), 0, 1, 1), 0);
        yield return (new Curve(new Sequence([new Point(0, 2), new Segment(0, 1, 0, 3)]), 0, 1, 1), 2);
        yield return (new Curve(new Sequence([Point.Origin(), Segment.Zero(0, 1), new Point(1, 2), new Segment(1, 2, 0, 3)]), 1, 1, 1), 1);
    }

    public static IEnumerable<object[]> RightDropCases() => PeriodicRightDrops().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void NondecreasingChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        Assert.False(curve.IsNonDecreasingOverInterval(start, isStartIncluded: false));
        Assert.False(curve.IsNonDecreasing);
        Assert.True(curve.IsNonDecreasingOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void IncreasingChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        Assert.False(curve.IsIncreasingOverInterval(start, isStartIncluded: false));
        Assert.False(curve.IsIncreasing);
        Assert.True(curve.IsIncreasingOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void NonincreasingChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        var decreasing = -curve;
        Assert.False(decreasing.IsNonIncreasingOverInterval(start, isStartIncluded: false));
        Assert.False(decreasing.IsNonIncreasing);
        Assert.True(decreasing.IsNonIncreasingOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void DecreasingChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        var decreasing = -curve;
        Assert.False(decreasing.IsDecreasingOverInterval(start, isStartIncluded: false));
        Assert.False(decreasing.IsDecreasing);
        Assert.True(decreasing.IsDecreasingOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void RightContinuityChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        Assert.False(curve.IsRightContinuousOverInterval(start, isStartIncluded: false));
        Assert.False(curve.IsRightContinuous);
        Assert.True(curve.IsRightContinuousOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Theory]
    [MemberData(nameof(RightDropCases))]
    public void ContinuityChecksTheRepeatedExcludedStart(Curve curve, Rational start)
    {
        Assert.False(curve.IsContinuousOverInterval(start, isStartIncluded: false));
        Assert.False(curve.IsContinuous);
        Assert.True(curve.IsContinuousOverInterval(start, start + 1, isStartIncluded: false));
    }

    [Fact]
    public void AnExcludedTransientJumpDoesNotRecur()
    {
        var curve = new Curve(new Sequence([new Point(0, 5), new Segment(0, 2, 0, 1)]), 1, 1, 1);
        Assert.True(curve.IsIncreasingOverInterval(0, isStartIncluded: false));
        Assert.True(curve.IsNonDecreasingOverInterval(0, isStartIncluded: false));
        Assert.True(curve.IsRightContinuousOverInterval(0, isStartIncluded: false));
        Assert.True(curve.IsContinuousOverInterval(0, isStartIncluded: false));
        Assert.True(curve.IsLeftContinuousOverInterval(0, isStartIncluded: false));
        var opposite = -curve;
        Assert.True(opposite.IsDecreasingOverInterval(0, isStartIncluded: false));
        Assert.True(opposite.IsNonIncreasingOverInterval(0, isStartIncluded: false));
    }

    [Fact]
    public void StrictMonotonicityAllowsEqualBoundaryValues()
    {
        var curve = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 1);
        Assert.True(curve.IsIncreasingOverInterval(0, isStartIncluded: false));
        Assert.True((-curve).IsDecreasingOverInterval(0, isStartIncluded: false));
    }

    [Fact]
    public void LeftContinuityChecksTheClosingPoint()
    {
        var curve = new Curve(new Sequence([new Point(0, 1), Segment.Zero(0, 1)]), 0, 1, 0);
        Assert.False(curve.IsLeftContinuous);
        Assert.False(curve.IsLeftContinuousOverInterval(0, isStartIncluded: false));
        Assert.True(curve.IsLeftContinuousOverInterval(0, 1, isStartIncluded: false));
    }
    public static IEnumerable<Curve> LongerHorizonCurves()
    {
        foreach (var (curve, _) in PeriodicRightDrops())
        {
            yield return curve;
            yield return -curve;
        }
        yield return new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, Rational.PlusInfinity);
        yield return new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, Rational.MinusInfinity);
        yield return Curve.PlusInfinite();
        yield return Curve.MinusInfinite();
        yield return Curve.Zero();
        yield return new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]), 0, 1, 1);
        yield return new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]), 0, 1, -1);
        yield return new Curve(new Sequence([new Point(0, 5), new Segment(0, 2, 0, 1)]), 1, 1, 1);
    }

    public static IEnumerable<object[]> LongerHorizonCases() => LongerHorizonCurves().ToXUnitTestCases();

    private static Sequence LongerCut(Curve curve) => curve.Cut(0,
        curve.PseudoPeriodStart + 5 * curve.PseudoPeriodLength, isEndIncluded: true);

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void NonDecreasingAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsNonDecreasing;
        Assert.Equal(expected, curve.IsNonDecreasing);
        Assert.Equal(expected, curve.IsNonDecreasingOverInterval(0));
    }

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void IncreasingAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsIncreasing;
        Assert.Equal(expected, curve.IsIncreasing);
        Assert.Equal(expected, curve.IsIncreasingOverInterval(0));
    }

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void NonIncreasingAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsNonIncreasing;
        Assert.Equal(expected, curve.IsNonIncreasing);
        Assert.Equal(expected, curve.IsNonIncreasingOverInterval(0));
    }

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void DecreasingAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsDecreasing;
        Assert.Equal(expected, curve.IsDecreasing);
        Assert.Equal(expected, curve.IsDecreasingOverInterval(0));
    }

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void LeftContinuousAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsLeftContinuous;
        Assert.Equal(expected, curve.IsLeftContinuous);
        Assert.Equal(expected, curve.IsLeftContinuousOverInterval(0));
    }

    [Theory]
    [MemberData(nameof(LongerHorizonCases))]
    public void RightContinuousAgreesWithFivePeriods(Curve curve)
    {
        var expected = LongerCut(curve).IsRightContinuous;
        Assert.Equal(expected, curve.IsRightContinuous);
        Assert.Equal(expected, curve.IsRightContinuousOverInterval(0));
    }

    [Fact]
    public void AnInfinitePeriodHeightCreatesAConstantTail()
    {
        var increasingPrefix = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 1)]),
            0, 1, Rational.PlusInfinity);
        Assert.Equal(1, increasingPrefix.PseudoPeriodStart);
        Assert.Equal(1, increasingPrefix.PseudoPeriodLength);
        Assert.Equal(Rational.PlusInfinity, increasingPrefix.PseudoPeriodHeight);
        Assert.Equal(Rational.PlusInfinity, increasingPrefix.ValueAt(2));
        Assert.Equal(Rational.PlusInfinity, increasingPrefix.ValueAt(3));
        Assert.False(increasingPrefix.IsIncreasing);
        Assert.False(increasingPrefix.IsIncreasingOverInterval(0));
        Assert.False(increasingPrefix.IsIncreasingOverInterval(0, isStartIncluded: false));
        Assert.True(increasingPrefix.IsIncreasingOverInterval(0, 1));

        var decreasingPrefix = new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, -1)]),
            0, 1, Rational.MinusInfinity);
        Assert.Equal(1, decreasingPrefix.PseudoPeriodStart);
        Assert.Equal(1, decreasingPrefix.PseudoPeriodLength);
        Assert.Equal(Rational.MinusInfinity, decreasingPrefix.PseudoPeriodHeight);
        Assert.Equal(Rational.MinusInfinity, decreasingPrefix.ValueAt(2));
        Assert.Equal(Rational.MinusInfinity, decreasingPrefix.ValueAt(3));
        Assert.False(decreasingPrefix.IsDecreasing);
        Assert.False(decreasingPrefix.IsDecreasingOverInterval(0));
        Assert.False(decreasingPrefix.IsDecreasingOverInterval(0, isStartIncluded: false));
        Assert.True(decreasingPrefix.IsDecreasingOverInterval(0, 1));
    }

}
