using System.Collections.Generic;
using System.Linq;

using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

using Unipi.Nancy.Numerics;

using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceMax
{
    [Fact]
    public void Max()
    {
        Sequence f1 = TestFunctions.SequenceA;
        Sequence f2 = TestFunctions.SequenceB;

        Sequence max = f1.Maximum(f2);

        Assert.Equal(0, max.DefinedFrom);
        Assert.Equal(6, max.DefinedUntil);

        //Assert.Equal(4, max.Optimize().Count);
        Assert.True(max.GetSegmentAfter(0).Slope < 0);
        Assert.Equal(5, max.GetSegmentBefore(3).Slope);
        Assert.Equal(-5, max.GetSegmentAfter(3).Slope);
        Assert.Equal(3, max.GetSegmentAfter(4).Slope);
        Assert.Equal(21, max.LeftLimitAt(6));
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        var testCases = new List<(Sequence a, Sequence b)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma:100, rho: 5).Cut(0, 20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Cut(0, 20)
            ),
            (
                a: new SigmaRhoArrivalCurve(sigma:100, rho: 5).Cut(0, 20),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10).Cut(5, 30)
            ),
            (
                a: new Curve(
                    baseSequence: new Sequence(
                        elements: new Element[]
                        {
                            new Segment(
                                startTime: 4,
                                endTime: 8,
                                rightLimitAtStartTime: 4,
                                slope: 2
                            )
                        }
                    ),
                    pseudoPeriodStart: 4,
                    pseudoPeriodLength: 4,
                    pseudoPeriodHeight: 6,
                    isPartialCurve: true
                )
                .Cut(0, 20),
                b: new Curve(
                    baseSequence: new Sequence(
                        elements: new Element[]
                        {
                            new Segment(
                                startTime: 5,
                                endTime: 6,
                                rightLimitAtStartTime: 5,
                                slope: 2
                            ),
                            new Point(
                                time: 6,
                                value: 7
                            ),
                            new Segment(
                                startTime: 6,
                                endTime: 7,
                                rightLimitAtStartTime: 6,
                                slope: 2
                            )
                        }
                    ),
                    pseudoPeriodStart: 6,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 1,
                    isPartialCurve: true
                )
                .Cut(0, 20)
            ),
            (
                a: new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)).Cut(0, 300),
                b: new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13)).Cut(0, 300)
            )
        };

        foreach (var (a, b) in testCases)
            yield return new object[] { a, b };
    }


    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void BreakpointSampling(Sequence a, Sequence b)
    {
        var max = Sequence.Maximum(a, b);

        var breakPoints =
                a.EnumerateBreakpoints()
            .Concat(
                b.EnumerateBreakpoints())
            .Concat(
                max.EnumerateBreakpoints())
            .Select(x => x.center.Time)
            .OrderBy(t => t)
            .Distinct()
            .Where(t => max.IsDefinedAt(t));

        foreach (var t in breakPoints)
        {
            if (t != max.DefinedFrom)
                Assert.Equal(
                    Rational.Max(a.LeftLimitAt(t), b.LeftLimitAt(t)),
                    max.LeftLimitAt(t)
                );

            Assert.Equal(
                Rational.Max(a.ValueAt(t), b.ValueAt(t)),
                max.ValueAt(t)
            );
            Assert.Equal(
                Rational.Max(a.RightLimitAt(t), b.RightLimitAt(t)),
                max.RightLimitAt(t)
            );
        }
    }

}