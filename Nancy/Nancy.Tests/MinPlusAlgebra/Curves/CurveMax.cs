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
                a: new StaircaseCurve(new Rational(2*5*11), 4000, new Rational(2*5*11)),
                b: new StaircaseCurve(new Rational(3*7*13), 5000, new Rational(3*7*13))
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
}