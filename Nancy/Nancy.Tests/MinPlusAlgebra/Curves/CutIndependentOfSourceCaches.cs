using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CutIndependentOfSourceCaches
{
    public static IEnumerable<(Rational Start, Rational End, bool Expected)> Restrictions =>
    [
        (0, 2, false),
        (new Rational(3, 2), 2, true),
        (2, 4, true)
    ];

    public static IEnumerable<object[]> RestrictionCases() => Restrictions.ToXUnitTestCases();

    private static Curve LeftJump() => new(new Sequence([
        new Point(0, 1), new Segment(0, 1, 1, 0),
        new Point(1, 0), new Segment(1, 2, 0, 0),
        new Point(2, 0), new Segment(2, 3, 0, 0)
    ]), 2, 1, 0);

    private static Curve RightJump() => new(new Sequence([
        new Point(0, 1), new Segment(0, 1, 1, 0),
        new Point(1, 1), new Segment(1, 2, 0, 0),
        new Point(2, 0), new Segment(2, 3, 0, 0)
    ]), 2, 1, 0);

    [Theory]
    [MemberData(nameof(RestrictionCases))]
    public void NonDecreasingResultIsIndependentOfSourceCache(Rational start, Rational end, bool expected)
    {
        var curve = LeftJump();
        Assert.Equal(expected, curve.Cut(start, end).IsNonDecreasing);
        Assert.False(curve.IsNonDecreasing);
        Assert.Equal(expected, curve.Cut(start, end).IsNonDecreasing);
    }

    [Theory]
    [MemberData(nameof(RestrictionCases))]
    public void LeftContinuityResultIsIndependentOfSourceCache(Rational start, Rational end, bool expected)
    {
        var curve = LeftJump();
        Assert.Equal(expected, curve.Cut(start, end).IsLeftContinuous);
        Assert.False(curve.IsLeftContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsLeftContinuous);
    }

    [Theory]
    [MemberData(nameof(RestrictionCases))]
    public void RightContinuityResultIsIndependentOfSourceCache(Rational start, Rational end, bool expected)
    {
        var curve = RightJump();
        Assert.Equal(expected, curve.Cut(start, end).IsRightContinuous);
        Assert.False(curve.IsRightContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsRightContinuous);
    }

    [Theory]
    [MemberData(nameof(RestrictionCases))]
    public void LeftContinuityResultIsIndependentOfRightContinuityCache(Rational start, Rational end, bool expected)
    {
        var curve = LeftJump();
        Assert.True(curve.IsRightContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsLeftContinuous);

        Assert.False(curve.IsLeftContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsLeftContinuous);
    }

    [Theory]
    [MemberData(nameof(RestrictionCases))]
    public void RightContinuityResultIsIndependentOfLeftContinuityCache(Rational start, Rational end, bool expected)
    {
        var curve = RightJump();
        Assert.True(curve.IsLeftContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsRightContinuous);

        Assert.False(curve.IsRightContinuous);
        Assert.Equal(expected, curve.Cut(start, end).IsRightContinuous);
    }

}
