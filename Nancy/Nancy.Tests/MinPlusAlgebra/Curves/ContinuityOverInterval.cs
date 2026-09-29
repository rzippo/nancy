using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ContinuityOverInterval
{
    /// <summary>0 over $[0, 4]$, 1 over $]4, 5[$, 2 from 5: right-discontinuous at 4, left-discontinuous at 5.</summary>
    private static readonly Curve JumpsInTransient = new Curve(
        new Sequence(new List<Element>
        {
            Point.Origin(), Segment.Zero(0, 4), Point.Zero(4), Segment.Constant(4, 5, 1), new Point(5, 2), Segment.Constant(5, 6, 2)
        }),
        pseudoPeriodStart: 5,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 0
    );

    public static List<(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)> RightContinuityCases = new()
    {
        // the only right-discontinuity is at the origin
        (new SigmaRhoArrivalCurve(3, 1), 0, null, false, false, true),
        (new SigmaRhoArrivalCurve(3, 1), 0, null, true, false, false),
        // a right-discontinuity at 2, after one pseudo-period from the start of the interval
        (new StepCurve(4, 2), 1, null, true, false, false),
        (new StepCurve(4, 2), 1, 3, true, false, false),
        (new StepCurve(4, 2), 2, null, false, false, true),
        // right-discontinuities at 3, 6, 9, ...: excluding one leaves the next
        (new StairCurve(2, 3), 3, null, false, false, false),
        (new StairCurve(2, 3), 3, null, true, false, false),
        // a right-discontinuity at 4, in the transient
        (JumpsInTransient, 1, null, true, false, false),
        (JumpsInTransient, 4, null, true, false, false),
        (JumpsInTransient, 4, null, false, false, true),
        (new RateLatencyServiceCurve(2, 1), 1, null, true, false, true),
    };

    public static IEnumerable<object[]> RightContinuityTestCases()
        => RightContinuityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RightContinuityTestCases))]
    public void RightContinuity(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)
    {
        Assert.Equal(expected, curve.IsRightContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
    }

    public static List<(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)> LeftContinuityCases = new()
    {
        // a left-discontinuity at 5, in the transient
        (JumpsInTransient, 1, null, true, false, false),
        (JumpsInTransient, 1, 6, true, false, false),
        // 5 is not interior from the left to an interval starting at 5
        (JumpsInTransient, 5, null, true, false, true),
        (JumpsInTransient, 5, null, false, false, true),
        (new StepCurve(4, 2), 1, null, true, false, true),
        (new StairCurve(2, 3), 1, null, true, false, true),
    };

    public static IEnumerable<object[]> LeftContinuityTestCases()
        => LeftContinuityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(LeftContinuityTestCases))]
    public void LeftContinuity(Curve curve, Rational start, Rational? end, bool isStartIncluded, bool isEndIncluded, bool expected)
    {
        Assert.Equal(expected, curve.IsLeftContinuousOverInterval(start, end, isStartIncluded, isEndIncluded));
    }
}