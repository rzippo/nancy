using System.Collections.Generic;
using System.Linq;

using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceEnumerateBreakpoints
{
    public static IEnumerable<object[]> GetTestCases()
    {
        var testcases = new List<(Sequence s, List<(Segment? left, Point center, Segment? right)> expected)>
        {
            (
                s: new SigmaRhoArrivalCurve(sigma: 100, rho: 5).Extend(20),
                expected: new List<(Segment? left, Point center, Segment? right)>
                {
                    (
                        left: null,
                        center: Point.Origin(),
                        right: new Segment(0, 20, 100, 5)
                    )
                }
            ),
            (
                s: new RateLatencyServiceCurve(rate: 20, latency: 10).Extend(20),
                expected: new List<(Segment? left, Point center, Segment? right)>
                {
                    (
                        left: null,
                        center: Point.Origin(),
                        right: new Segment(0, 10, 0, 0)
                    ),
                    (
                        left: new Segment(0, 10, 0, 0),
                        center: new Point(10, 0),
                        right: new Segment(10, 20, 0, 20)
                    )
                }
            ),
            (
                s: new RateLatencyServiceCurve(rate: 20, latency: 10).Cut(0, 20, isEndIncluded: true),
                expected: new List<(Segment? left, Point center, Segment? right)>
                {
                    (
                        left: null,
                        center: Point.Origin(),
                        right: new Segment(0, 10, 0, 0)
                    ),
                    (
                        left: new Segment(0, 10, 0, 0),
                        center: new Point(10, 0),
                        right: new Segment(10, 20, 0, 20)
                    ),
                    (
                        left: new Segment(10, 20, 0, 20),
                        center: new Point(20, 200),
                        right: null
                    )
                }
            )
        };

        foreach (var (s, expected) in testcases)
        {
            yield return new object[] { s, expected };
        }
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void SigmaRhoRateLatency(Sequence s, List<(Segment? left, Point center, Segment? right)> expected)
    {
        var breakpoints = s.EnumerateBreakpoints();
        Assert.True(expected.SequenceEqual(breakpoints));
    }
}