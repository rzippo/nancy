using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Count
{
    private static Curve JoinedPeriods() => new(
        new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 1),
            new Point(1, 1), new Segment(1, 2, 1, -1),
            new Point(2, 0), new Segment(2, 3, 0, 1)
        ]), 0, 3, 1);

    private static Curve SplitAffine() => new(
        new Sequence([Point.Origin(), new Segment(0, 5, 0, 1)]), 2, 3, 3);

    private static Curve TransientAndJoinedPeriods() => new(
        new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 1),
            new Point(1, 1), new Segment(1, 4, 1, 0),
            new Point(4, 1), new Segment(4, 5, 1, 1),
            new Point(5, 2), new Segment(5, 6, 2, -1),
            new Point(6, 1), new Segment(6, 7, 1, 1)
        ]), 4, 3, 1);

    public static IEnumerable<(Curve Curve, Rational Start, Rational End, bool IncludeStart, bool IncludeEnd, int Expected)> NormalizedCountCases()
    {
        yield return (TransientAndJoinedPeriods(), (Rational)0, (Rational)4, true, false, 4);
        yield return (TransientAndJoinedPeriods(), new Rational(1, 2), new Rational(7, 2), true, true, 5);
        yield return (TransientAndJoinedPeriods(), (Rational)4, (Rational)13, true, false, 14);
        yield return (TransientAndJoinedPeriods(), (Rational)8, (Rational)12, false, false, 5);
        yield return (TransientAndJoinedPeriods(), new Rational(17, 2), new Rational(23, 2), true, true, 7);
        yield return (JoinedPeriods(), (Rational)0, (Rational)9, true, false, 14);
        yield return (JoinedPeriods(), (Rational)2, (Rational)7, false, false, 5);
        yield return (JoinedPeriods(), (Rational)2, (Rational)7, true, true, 7);
        yield return (JoinedPeriods(), new Rational(5, 2), new Rational(13, 2), true, true, 7);
        yield return (SplitAffine(), (Rational)0, (Rational)30, true, false, 2);
        yield return (SplitAffine(), (Rational)1, (Rational)29, false, false, 1);
        yield return (new StairCurve(2, 3), (Rational)0, (Rational)12, true, true, 9);
        yield return (new StairCurve(2, 3), (Rational)3, (Rational)9, false, false, 3);
        yield return (new Curve(new Sequence([
            Point.Origin(), Segment.PlusInfinite(0, 1),
            new Point(1, 0), Segment.Zero(1, 2)
        ]), 0, 2, 0), (Rational)0, (Rational)4, true, false, 8);
        yield return (new Curve(new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 1)
        ]), 0, 2, 3), (Rational)0, (Rational)6, true, false, 6);
        yield return (new Curve(new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 1),
            new Point(1, 1), new Segment(1, 2, 1, -1)
        ]), 0, 2, 0), (Rational)0, (Rational)6, true, false, 12);
    }

    public static IEnumerable<object[]> NormalizedCounts()
        => NormalizedCountCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NormalizedCounts))]
    public void CountsRetainedBreakpointsAndIncludedEndpoints(
        Curve curve, Rational start, Rational end, bool includeStart, bool includeEnd, int expected)
    {
        Assert.Equal(expected, curve.Count(start, end, includeStart, includeEnd));
    }
}
