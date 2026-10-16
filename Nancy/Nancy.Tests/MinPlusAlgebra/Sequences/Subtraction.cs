using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Subtraction
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
        var diff = Sequence.Subtraction(a, b);
        Assert.True(Sequence.Equivalent(expected, diff));
    }
    
    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyTests))]
    public void SigmaRhoRateLatency_Obsolete_Negative(Sequence a, Sequence b, Sequence expected)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var diff = Sequence.Subtraction(a,b, false);
        Assert.True(Sequence.Equivalent(expected, diff));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Theory]
    [MemberData(nameof(GetSigmaRhoRateLatencyTests))]
    public void SigmaRhoRateLatency_Obsolete_NonNegative(Sequence a, Sequence b, Sequence expected)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var diff = Sequence.Subtraction(a,b, true);
        var actualExpected = expected.ToNonNegative();
        Assert.True(Sequence.Equivalent(actualExpected, diff));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Supports that share a single time, which both include, give the difference at that time alone:
    /// $t$ over $[0, 2]$ minus $t + 3$ over $[2, 4]$ is $-3$ at 2.
    /// </summary>
    [Fact]
    public void SupportsSharingASingleTimeGiveTheDifferenceThere()
    {
        var a = new Sequence([ Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2) ]);
        var b = new Sequence([ new Point(2, 5), new Segment(2, 4, 5, 1), new Point(4, 7) ]);
        var difference = Sequence.Subtraction(a, b);
        Assert.True(Sequence.Equivalent(new Sequence([ new Point(2, -3) ]), difference), $"{difference}");
    }
}
