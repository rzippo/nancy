using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceAddition
{
    [Fact]
    public void Add()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;

        Sequence sum = f1.Addition(f2);

        Assert.Equal(0, sum.DefinedFrom);
        Assert.Equal(6, sum.DefinedUntil);

        //Assert.Equal(3, sum.Optimize().Count);
        Assert.True(sum.GetSegmentBefore(3).Slope > 0);
        Assert.True(sum.GetSegmentAfter(3).Slope < 0);
        Assert.Equal(-2, sum.GetSegmentAfter(5).Slope);
        Assert.Equal(26, sum.LeftLimitAt(6));
    }

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
                    new Segment(10, 20, 150, 25)
                })
            ),
            (
                a: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                b: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                expected: new Sequence(new List<Element>
                {
                    Point.Origin(),
                    new Segment(0, 10, 100, 5),
                    new Point(10, 150),
                    new Segment(10, 20, 150, 25)
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
        var diff = a + b;
        Assert.True(Sequence.Equivalent(expected, diff));
    }
}