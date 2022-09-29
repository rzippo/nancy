using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurvePeriodStartInfimum
{
    public static IEnumerable<object[]> PeriodStartInfimumTestCases()
    {
        var testCases = new List<(Curve curve, Rational expected)>
        {
            (
                new RateLatencyServiceCurve(4, 3),
                3
            ),
            (
                new SigmaRhoArrivalCurve(4, 3),
                0
            ),
            (
                new SigmaRhoArrivalCurve(4, 0),
                0
            ),
            (
                new DelayServiceCurve(5),
                5
            ),
            (
                Curve.Minimum(new ConstantCurve(3), new DelayServiceCurve(5)),
                5
            ),
            (
                new Curve(
                    baseSequence: new Sequence(new Element[]
                    {
                        Point.Origin(),
                        Segment.Zero(0, 1),
                        new Point(1, 0),
                        new Segment(1, 2, 0, 1),
                        new Point(2, 1),
                        Segment.Constant(2, 3, 2),
                        new Point(3, 2),
                        Segment.Constant(3, 4, 2)
                    }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                ),
                2
            )
        };

        foreach (var (curve, expected) in testCases)
        {
            yield return new object[] { curve, expected };
        }
    }

    [Theory]
    [MemberData(nameof(PeriodStartInfimumTestCases))]
    public void PeriodStartInfimum(Curve curve, Rational expected)
    {
        var t_l = curve.PseudoPeriodStartInfimum;
        Assert.Equal(t_l, expected);
    }
}