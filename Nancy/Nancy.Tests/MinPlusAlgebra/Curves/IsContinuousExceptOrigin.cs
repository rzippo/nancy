using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class IsContinuousExceptOrigin
{
    public static List<(Curve curve, bool expected)> Cases = new()
    {
        // the pseudo-period starts at 0, and its jump right after 0 repeats at 3, 6, ...
        (new StairCurve(2, 3), false),
        // the same jumps, with the pseudo-period starting after 0
        (new StairCurve(2, 3).DelayBy(1), false),
        // the pseudo-period starts at 0, and its jump right before its end repeats at 1, 2, ...
        (
            new Curve(
                new Sequence(new List<Element> { Point.Origin(), new Segment(0, 1, 0, 1) }),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 2
            ),
            false
        ),
        // the only jump is at the origin
        (new SigmaRhoArrivalCurve(3, 1), true),
        (new RateLatencyServiceCurve(2, 1), true),
        (new ConstantCurve(3), true),
    };

    public static IEnumerable<object[]> TestCases()
        => Cases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(TestCases))]
    public void IsContinuousExceptOriginTest(Curve curve, bool expected)
    {
        Assert.Equal(expected, curve.IsContinuousExceptOrigin);
    }
}