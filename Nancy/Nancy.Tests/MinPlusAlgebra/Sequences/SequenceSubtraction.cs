using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceSubtraction
{
    public static IEnumerable<object[]> GetSigmaRhoRateLatencyTests()
    {
        var testcases = new List<(Sequence a, Sequence b, Sequence expected)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                expected: new Sequence(new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 10, 100, 5),
                    new Point(10, 150),
                    new Segment(10, 20, 150, -15)
                })
            ),
            (
                a: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                b: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                expected: new Sequence(new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 10, -100, -5),
                    new Point(10, -150),
                    new Segment(10, 20, -150, 15)
                })
            )
        };

        foreach (var (a, b, expected) in testcases)
        {
            yield return new object[] { a, b, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyTests))]
    public void SigmaRhoRateLatency(Sequence a, Sequence b, Sequence expected)
    {
        var diff = Sequence.Subtraction(a, b, false);
        Assert.True(Sequence.Equivalent(expected, diff));
    }
}