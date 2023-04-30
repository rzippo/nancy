using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveMin
{
    [Fact]
    public void SigmaRho_RateLatency()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma:100, rho: 5);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve fun = Curve.Minimum(arrival, service);

        Assert.Equal(0, fun.ValueAt(0));
        Assert.Equal(0, fun.ValueAt(10));
        Assert.Equal(200, fun.ValueAt(20));
        Assert.Equal(250, fun.ValueAt(30));

        Assert.Equal(0, fun.GetSegmentAfter(3).Slope);
        Assert.Equal(20, fun.GetSegmentAfter(13).Slope);
        Assert.Equal(5, fun.GetSegmentAfter(23).Slope);
    }

    [Fact]
    public void PeriodSubsampling()
    {
        Curve f1 = new Curve(
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
        );

        Curve f2 = new Curve(
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
        );

        Curve min = Curve.Minimum(f1, f2);

        Assert.Equal(4, min.FirstFiniteTime);
        Assert.True(min.PseudoPeriodStart > 4 && min.PseudoPeriodStart < 8);
        Assert.Equal(1, min.PseudoPeriodLength);
        Assert.Equal(1, min.PseudoPeriodHeight);

        Assert.Equal(6, min.LeftLimitAt(5));
        Assert.Equal(6, min.ValueAt(5));
        Assert.Equal(5, min.RightLimitAt(5));

        Assert.Equal(8, min.LeftLimitAt(7));
        Assert.Equal(8, min.ValueAt(7));
        Assert.Equal(7, min.RightLimitAt(7));

        Assert.Equal(21, min.LeftLimitAt(20));
        Assert.Equal(21, min.ValueAt(20));
        Assert.Equal(20, min.RightLimitAt(20));
    }

    [Fact]
    public void InfinitePeriodStart()
    {
        //This tested a b_u_g that is now fixed.
        //The resulting min had an infinite pseudo period start time.

        Curve f1 = new Curve(
            baseSequence: new Sequence(
                elements: new Element[]{
                    Point.Origin(),
                    new Segment(
                        startTime: 10,
                        endTime: 20,
                        rightLimitAtStartTime: 10,
                        slope: 2
                    )
                },
                fillFrom: 0,
                fillTo: 20
            ),
            pseudoPeriodStart: 10,
            pseudoPeriodLength: 10,
            pseudoPeriodHeight: 10                
        );

        Curve f2 = new Curve(
            baseSequence: new Sequence(
                elements: new Element[]{
                    Point.Origin(),
                    Segment.PlusInfinite(0, 50),
                    new Point(
                        time: 50,
                        value: 90
                    ),
                    new Segment(
                        startTime: 50,
                        endTime: 100,
                        rightLimitAtStartTime: 90,
                        slope: 2
                    ),
                    new Point(
                        time: 100,
                        value: 180
                    ),
                    new Segment(
                        startTime: 100,
                        endTime: 150,
                        rightLimitAtStartTime: 180,
                        slope: 2
                    ),
                    new Point(
                        time: 150,
                        value: 270
                    ),
                    new Segment(
                        startTime: 150,
                        endTime: 200,
                        rightLimitAtStartTime: 270,
                        slope: 2
                    )
                }
            ),
            pseudoPeriodStart: 150,
            pseudoPeriodLength: 50,
            pseudoPeriodHeight: 90
        );

        Assert.False(f1.IsFinite);
        Assert.False(f1.IsContinuous);
        Assert.False(f1.IsUltimatelyPlain);

        Assert.False(f2.IsFinite);
        Assert.False(f2.IsContinuous);
        Assert.True(f2.IsUltimatelyPlain);

        Curve min = Curve.Minimum(f1, f2);
        Assert.NotEqual(Rational.PlusInfinity, min.PseudoPeriodStart);
    }

    [Fact]
    public void UltimatelyInfinite()
    {
        Curve f1 = new DelayServiceCurve(6);
        Curve f2 = new RateLatencyServiceCurve(4, 4);

        Assert.False(f1.IsFinite);
        Assert.False(f1.IsContinuous);
        Assert.True(f1.IsUltimatelyPlain);

        Assert.True(f2.IsFinite);
        Assert.True(f2.IsContinuous);
        Assert.True(f2.IsRightContinuous);
        Assert.True(f2.IsUltimatelyPlain);

        Curve min = Curve.Minimum(f1, f2);
        Assert.True(min.IsFinite);
        Assert.False(min.IsContinuous);
        Assert.True(min.IsUltimatelyPlain);
    }

    public static IEnumerable<object[]> MinimumSamplingTestCases()
    {
        var testCases = new (Curve a, Curve b, Rational[] times)[]
        {
            (
                new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13)),
                new Rational[]{ 
                    383,
                    new (174265019/455000),
                    493,
                    new (224315019/455000)
                }
            )
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.a, testCase.b, testCase.times};
        }
    }

    [Theory]
    [MemberData(nameof(MinimumSamplingTestCases))]
    public void MinimumSampling(Curve a, Curve b, Rational[] times)
    {
        var min = Curve.Minimum(a, b, new (){UseRepresentationMinimization = false});
        foreach (var time in times)
        {
            Assert.Equal(min.ValueAt(time), Rational.Min(a.ValueAt(time), b.ValueAt(time)));
        }
    }

    public static IEnumerable<object[]> GetPairTestCases()
    {
        var testCases = new List<(Curve a, Curve b)>
        {
            (
                a: new SigmaRhoArrivalCurve(sigma:100, rho: 5),
                b: new RateLatencyServiceCurve(rate: 20, latency: 10)
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
                ),
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
            ),
            (
                a: new FlowControlCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                b: new FlowControlCurve(new Rational(3*7*13), 5000, new Rational(3*7*13))
            )
        };

        foreach(var (a, b) in testCases)
            yield return new object[] { a, b };
    }

    [Theory]
    [MemberData(nameof(GetPairTestCases))]
    public void BreakpointSampling(Curve a, Curve b)
    {
        var min = Curve.Minimum(a, b);

        var breakPoints = 
                a.Extend(min.FirstPseudoPeriodEnd).EnumerateBreakpoints()
            .Concat(
                b.Extend(min.FirstPseudoPeriodEnd).EnumerateBreakpoints())
            .Concat(
                min.Extend(min.FirstPseudoPeriodEnd).EnumerateBreakpoints())
            .Select(x => x.center.Time)
            .OrderBy(t => t)
            .Distinct();

        foreach(var t in breakPoints)
        {
            if (t != 0)
                Assert.Equal(
                    Rational.Min(a.LeftLimitAt(t), b.LeftLimitAt(t)),
                    min.LeftLimitAt(t)
                );

            Assert.Equal(
                Rational.Min(a.ValueAt(t), b.ValueAt(t)),
                min.ValueAt(t)
            );
            Assert.Equal(
                Rational.Min(a.RightLimitAt(t), b.RightLimitAt(t)),
                min.RightLimitAt(t)
            );
        }
    }

    public static IEnumerable<object[]> GetSingleTestCases()
    {

        foreach (var pair in GetPairTestCases())
        {
            yield return new object[] { pair[0] }; // Curve a
            yield return new object[] { pair[1] }; // Curve b
        }

        yield return new object[] { Curve.PlusInfinite() };
        yield return new object[] { Curve.MinusInfinite() };
    }

    [Theory]
    [MemberData(nameof(GetSingleTestCases))]
    public void PlusInfinite(Curve c)
    {
        var min = Curve.Minimum(c, Curve.PlusInfinite());
        Assert.True(Curve.Equivalent(min, c));
    }

    [Theory]
    [MemberData(nameof(GetSingleTestCases))]
    public void MinusInfinite(Curve c)
    {
        var min = Curve.Minimum(c, Curve.MinusInfinite(), ComputationSettings.Default() with {UseParallelism = false} );
        Assert.True(Curve.Equivalent(min, Curve.MinusInfinite()));
    }

    public static IEnumerable<object[]> GetSetTestCases()
    {
        var testcases = new List<List<Curve>>
        {
            // These tests are gathered from a max-plus convolution test.
            // It broke due to a wierd issue with tt terms being UI curves with a non-conventional shape

            // all 4 terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":9,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":9,\"value\":-2,\"type\":\"point\"},{\"startTime\":9,\"endTime\":11,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":9,\"periodLength\":2,\"periodHeight\":-3}")
            },
            // no tt term
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":9,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":9,\"value\":-2,\"type\":\"point\"},{\"startTime\":9,\"endTime\":11,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":9,\"periodLength\":2,\"periodHeight\":-3}")
            },
            // no pp term
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
            },
            // tt, tp terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
            },
            // tt, pt terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
            },
            // only tt and pp terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":9,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":9,\"value\":-2,\"type\":\"point\"},{\"startTime\":9,\"endTime\":11,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":9,\"periodLength\":2,\"periodHeight\":-3}")
            },
        };

        foreach (var set in testcases)
        {
            yield return new object[] {set};
        }
    }

    [Theory]
    [MemberData(nameof(GetSetTestCases))]
    public void BreakpointMatchTest(List<Curve> curves)
    {
        var min = Curve.Minimum(curves);
        foreach (var (_, point, _) in 
                 min
                     .CutAsEnumerable(0, min.SecondPseudoPeriodEnd, true, true)
                     .EnumerateBreakpoints())
        {
            Assert.Contains(curves, c => c.ValueAt(point.Time) == point.Value);
        }
    }
}