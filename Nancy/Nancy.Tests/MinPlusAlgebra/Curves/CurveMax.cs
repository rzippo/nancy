using System.Collections.Generic;
using System.Linq;

using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveMax
{
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
        var max = Curve.Maximum(a, b);

        var breakPoints = 
                a.Extend(max.FirstPseudoPeriodEnd).EnumerateBreakpoints()
            .Concat(
                b.Extend(max.FirstPseudoPeriodEnd).EnumerateBreakpoints())
            .Concat(
                max.Extend(max.FirstPseudoPeriodEnd).EnumerateBreakpoints())
            .Select(x => x.center.Time)
            .OrderBy(t => t)
            .Distinct();

        foreach(var t in breakPoints)
        {
            if (t != 0)
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
        var max = Curve.Maximum(c, Curve.PlusInfinite());
        Assert.True(Curve.Equivalent(max, Curve.PlusInfinite()));
    }

    [Theory]
    [MemberData(nameof(GetSingleTestCases))]
    public void MinusInfinite(Curve c)
    {
        var max = Curve.Maximum(c, Curve.MinusInfinite());
        Assert.True(Curve.Equivalent(max, c));
    }

    //[Fact]
    //public void UltimatelyInfinite()
    //{
    //    Curve f1 = new DelayServiceCurve(6);
    //    Curve f2 = new RateLatencyServiceCurve(4, 4);

    //    Assert.False(f1.IsFinite);
    //    Assert.False(f1.IsContinuous);
    //    Assert.True(f1.IsUltimatelyPlain);

    //    Assert.True(f2.IsFinite);
    //    Assert.True(f2.IsContinuous);
    //    Assert.True(f2.IsRightContinuous);
    //    Assert.True(f2.IsUltimatelyPlain);

    //    Curve min = Curve.Minimum(f1, f2);
    //    Assert.True(min.IsFinite);
    //    Assert.False(min.IsContinuous);
    //    Assert.True(min.IsUltimatelyPlain);
    //}

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
            }.Select(t => -t).ToList(),
            // no tt term
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":9,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":9,\"value\":-2,\"type\":\"point\"},{\"startTime\":9,\"endTime\":11,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":9,\"periodLength\":2,\"periodHeight\":-3}")
            }.Select(t => -t).ToList(),
            // no pp term
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
            }.Select(t => -t).ToList(),
            // tt, tp terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":6,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":6,\"value\":0,\"type\":\"point\"},{\"startTime\":6,\"endTime\":8,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":6,\"periodLength\":2,\"periodHeight\":-3}"),
            }.Select(t => -t).ToList(),
            // tt, pt terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":3,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":3,\"value\":-2,\"type\":\"point\"},{\"startTime\":3,\"endTime\":5,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":3,\"periodLength\":2,\"periodHeight\":-1}"),
            }.Select(t => -t).ToList(),
            // only tt and pp terms
            new List<Curve>
            {
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":0,\"type\":\"point\"},{\"startTime\":0,\"endTime\":2,\"rightLimit\":0,\"slope\":0,\"type\":\"segment\"},{\"time\":2,\"value\":0,\"type\":\"point\"},{\"startTime\":2,\"endTime\":3,\"rightLimit\":0,\"slope\":-1,\"type\":\"segment\"},{\"time\":3,\"value\":-1,\"type\":\"point\"},{\"startTime\":3,\"endTime\":7,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"},{\"time\":7,\"value\":-1,\"type\":\"point\"},{\"startTime\":7,\"endTime\":9,\"rightLimit\":-1,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":7,\"periodLength\":2,\"periodHeight\":{\"num\":1,\"den\":0}}"),
                Curve.FromJson("{\"type\":\"curve\",\"sequence\":{\"elements\":[{\"time\":0,\"value\":{\"num\":1,\"den\":0},\"type\":\"point\"},{\"startTime\":0,\"endTime\":9,\"rightLimit\":{\"num\":1,\"den\":0},\"slope\":{\"num\":1,\"den\":0},\"type\":\"segment\"},{\"time\":9,\"value\":-2,\"type\":\"point\"},{\"startTime\":9,\"endTime\":11,\"rightLimit\":-2,\"slope\":0,\"type\":\"segment\"}]},\"periodStart\":9,\"periodLength\":2,\"periodHeight\":-3}")
            }.Select(t => -t).ToList(),
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
        var max = Curve.Maximum(curves);
        foreach (var (_, point, _) in 
                 max
                     .CutAsEnumerable(0, max.SecondPseudoPeriodEnd, true, true)
                     .EnumerateBreakpoints())
        {
            Assert.Contains(curves, c => c.ValueAt(point.Time) == point.Value);
        }
    }
}