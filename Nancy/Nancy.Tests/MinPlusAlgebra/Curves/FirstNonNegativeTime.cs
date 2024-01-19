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
        )
    };

    public static IEnumerable<object[]> KnownResultsTestCases = 
        KnownPairs.ToXUnitTestCases();

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