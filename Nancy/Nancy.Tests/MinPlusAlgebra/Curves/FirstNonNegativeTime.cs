using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class FirstNonNegativeTime
{


    public static List<Curve> NonDecreasingCurves = new()
    {
        new Curve(
            baseSequence: new Sequence(new List<Element>
            {
                new Point(0, new Rational(-152, 15)), new Segment(0, 4, new Rational(-152, 15), new Rational(1, 30)),
                new Point(4, -10), new Segment(4, new Rational(19, 4), -10, new Rational(4, 3)),
                new Point(new Rational(19, 4), -9),
                new Segment(new Rational(19, 4), new Rational(11, 2), -9, new Rational(4, 3))
            }), pseudoPeriodStart: new Rational(19, 4), pseudoPeriodLength: new Rational(3, 4), pseudoPeriodHeight: 1),
    };
    
    public static List<(Curve curve, Rational expected)> KnownPairs = new()
    {
        (
            curve: new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0, new Rational(-17, 6)), new Segment(0, 1, new Rational(-17, 6), 1) }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            expected: new Rational(17, 6)
        ),
        (
            // the second period is the first with a non-negative value, right after its start, where the curve is -1
            curve: new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0, -2), new Segment(0, new Rational(5, 2), -1, 0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: 1),
            expected: new Rational(5, 2)
        ),
        (
            // the second period is the first with a non-negative value, and its start is already one
            curve: new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0, new Rational(-3, 2)), new Segment(0, 2, new Rational(-1, 2), 0) }), pseudoPeriodStart: 0, pseudoPeriodLength: 2, pseudoPeriodHeight: 1),
            expected: 2
        )
    };

    /// <summary>
    /// Curves whose pseudo-periods rise above their end value and drop back, so that a period reaches 0 before the one that ends above it.
    /// </summary>
    public static List<(Curve curve, Rational expected)> NonMonotonePeriodPairs = new()
    {
        (
            // in [k, k + 1[: -10 + k at k, rising with slope 15 to -5/2 + k, then -19/2 + k;
            // the period starting at 3 is the first to reach 0, at 3 + 7/15, while the values at the period ends are non-negative only from 9
            curve: new Curve(baseSequence: new Sequence(new List<Element>{
                new Point(0, -10), new Segment(0, new Rational(1, 2), -10, 15),
                new Point(new Rational(1, 2), new Rational(-19, 2)), Segment.Constant(new Rational(1, 2), 1, new Rational(-19, 2))
            }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            expected: new Rational(52, 15)
        ),
        (
            // the same periods after a transient of -20 over [0, 2[
            curve: new Curve(baseSequence: new Sequence(new List<Element>{
                new Point(0, -20), Segment.Constant(0, 2, -20),
                new Point(2, -10), new Segment(2, new Rational(5, 2), -10, 15),
                new Point(new Rational(5, 2), new Rational(-19, 2)), Segment.Constant(new Rational(5, 2), 3, new Rational(-19, 2))
            }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            expected: new Rational(82, 15)
        ),
        (
            // the period rises to -1 only as a limit, so the period starting at 1 approaches 0 without reaching it, and the next one reaches it right after its start
            curve: new Curve(baseSequence: new Sequence(new List<Element>{
                new Point(0, -3), new Segment(0, new Rational(1, 2), -3, 4),
                new Point(new Rational(1, 2), -3), Segment.Constant(new Rational(1, 2), 1, -3)
            }), pseudoPeriodStart: 0, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            expected: new Rational(9, 4)
        ),
        (
            // the transient rises to 0 only as a limit at 1, where the curve is -5, then the period is constant from -5, rising 1 per period
            curve: new Curve(baseSequence: new Sequence(new List<Element>{
                new Point(0, -2), new Segment(0, 1, -2, 2),
                new Point(1, -5), Segment.Constant(1, 2, -5)
            }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
            expected: 6
        ),
    };

    public static IEnumerable<object[]> KnownResultsTestCases = 
        KnownPairs.Concat(NonMonotonePeriodPairs).ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownResultsTestCases))]
    public void KnownResultsTest(Curve curve, Rational expected)
    {
        var firstNonNegativeTime = curve.FirstNonNegativeTime;
        Assert.Equal(expected, firstNonNegativeTime);
    }

    public static IEnumerable<object[]> NonDecreasingTestCases =
        NonDecreasingCurves
            .Concat(KnownPairs.Select(p => p.curve))
            .ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(NonDecreasingTestCases))]
    public void NonDecreasingTest(Curve curve)
    {
        var firstNonNegativeTime = curve.FirstNonNegativeTime;
        Assert.True(firstNonNegativeTime.IsFinite);
        Assert.True(firstNonNegativeTime >= 0);
        Assert.True(firstNonNegativeTime < Rational.PlusInfinity);

        if (firstNonNegativeTime > 0)
        {
            Assert.True(curve.GetSegmentBefore(firstNonNegativeTime).RightLimitAtStartTime < 0);    
        }
        
        Assert.True(
            curve.ValueAt(firstNonNegativeTime) >= 0 ||
            curve.RightLimitAt(firstNonNegativeTime) >= 0
        );
    }
}