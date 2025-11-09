using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class Subtraction
{
    public static IEnumerable<object[]> GetSigmaRhoRateLatencyNegativeTests()
    {
        var testcases = new List<(Curve a, Curve b, Curve expected)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma: 100, rho: 5),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 10, 100, 5),
                        new Point(10, 150),
                        new Segment(10, 20, 150, -15)
                    }),
                    pseudoPeriodStart: 10,
                    pseudoPeriodLength: 10,
                    pseudoPeriodHeight: -150
                )
            ),
            (
                a: new RateLatencyServiceCurve(rate: 20, latency: 10),
                b: new SigmaRhoArrivalCurve(sigma: 100, rho: 5),
                expected: new Curve(
                    baseSequence: new Sequence(new List<Element>
                    {
                        Point.Origin(),
                        new Segment(0, 10, -100, -5),
                        new Point(10, -150),
                        new Segment(10, 20, -150, 15)
                    }),
                    pseudoPeriodStart: 10,
                    pseudoPeriodLength: 10,
                    pseudoPeriodHeight: 150
                )
            )
        };

        foreach (var (a, b, expected) in testcases)
        {
            yield return new object[] { a, b, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyNegativeTests))]
    public void SigmaRhoRateLatency(Curve a, Curve b, Curve expected)
    {
        var diff = Curve.Subtraction(a,b);
        Assert.True(Curve.Equivalent(expected, diff));
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyNegativeTests))]
    public void SigmaRhoRateLatency_Obsolete_Negative(Curve a, Curve b, Curve expected)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var diff = Curve.Subtraction(a,b, false);
        Assert.True(Curve.Equivalent(expected, diff));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyNegativeTests))]
    public void SigmaRhoRateLatency_Obsolete_NonNegative(Curve a, Curve b, Curve expected)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var diff = Curve.Subtraction(a,b, true);
        var actualExpected = expected.ToNonNegative();
        Assert.True(Curve.Equivalent(actualExpected, diff));
#pragma warning restore CS0618 // Type or member is obsolete
    }
}