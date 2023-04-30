using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveConvolution
{
    private readonly ITestOutputHelper output;

    public CurveConvolution(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void TwoRateLatencyEquivalent()
    {
        RateLatencyServiceCurve firstRouter = new RateLatencyServiceCurve(rate: 3, latency: 4);
        RateLatencyServiceCurve secondRouter = new RateLatencyServiceCurve(rate: 4, latency: 6);

        Curve equivalentService = firstRouter.Convolution(secondRouter);

        Assert.False(equivalentService.IsZero);
        Assert.True(equivalentService.IsContinuous);
        Assert.True(equivalentService.IsRightContinuous);
        Assert.True(equivalentService.IsContinuousExceptOrigin);
        Assert.True(equivalentService.IsLeftContinuous);
        Assert.True(equivalentService.IsUltimatelyPlain);
        Assert.True(equivalentService.IsUltimatelyAffine);

        Assert.Equal(0, equivalentService.ValueAt(0));
        Assert.Equal(0, equivalentService.ValueAt(10));
        Assert.Equal(6, equivalentService.ValueAt(12));
        Assert.Equal(3, equivalentService.PseudoPeriodSlope);
    }

    [Fact]
    public void ThreeRateLatencyEquivalent()
    {
        RateLatencyServiceCurve firstRouter = new RateLatencyServiceCurve(rate: 3, latency: 4);
        RateLatencyServiceCurve secondRouter = new RateLatencyServiceCurve(rate: 4, latency: 6);
        RateLatencyServiceCurve thirdRouter = new RateLatencyServiceCurve(rate: 6, latency: 2);

        Curve equivalentService = firstRouter.Convolution(secondRouter).Convolution(thirdRouter);

        Assert.False(equivalentService.IsZero);
        Assert.True(equivalentService.IsContinuous);
        Assert.True(equivalentService.IsRightContinuous);
        Assert.True(equivalentService.IsContinuousExceptOrigin);
        Assert.True(equivalentService.IsLeftContinuous);
        Assert.True(equivalentService.IsUltimatelyPlain);
        Assert.True(equivalentService.IsUltimatelyAffine);

        Assert.Equal(0, equivalentService.ValueAt(0));
        Assert.Equal(0, equivalentService.ValueAt(12));
        Assert.Equal(6, equivalentService.ValueAt(14));
        Assert.Equal(3, equivalentService.PseudoPeriodSlope);
    }

    public static IEnumerable<object[]> GetFiniteCurves()
    {
        var curves = new Curve[] {
            new ConstantCurve(4),
            new RateLatencyServiceCurve(11, 10),
            new RateLatencyServiceCurve(rate: 3, latency: 4),
            new SigmaRhoArrivalCurve(4, 2),
            new ConstantCurve(0),
            new Curve(baseSequence: new Sequence(new List<Element>{new Point(0,0),new Segment(0,1,0,0),new Point(1,0),new Segment(1,3,0,new Rational(1, 2)),new Point(3,1),new Segment(3,4,1,1),new Point(4,2),new Segment(4,5,2,0),}),pseudoPeriodStart: 3,pseudoPeriodLength: 2,pseudoPeriodHeight: 1),
            new Curve(baseSequence: new Sequence(new List<Element>{new Point(0,4),new Segment(0,2,4,new Rational(1, 2)),}),pseudoPeriodStart: 0,pseudoPeriodLength: 2,pseudoPeriodHeight: 1)
        };

        foreach (var curve in curves)
            yield return new object[] { curve };
    }

    [Theory]
    [MemberData(nameof(GetFiniteCurves))]
    public void ConvolutionWithZeroDelay(Curve curve)
    {
        Curve zeroDelay = new ConstantCurve(Rational.PlusInfinity);

        Curve convolution = curve.Convolution(zeroDelay);
        Assert.Equal(curve, convolution);
    }

    [Theory]
    [MemberData(nameof(GetFiniteCurves))]
    public void ConvolutionWithInfinity(Curve curve)
    {
        Curve infinite = Curve.PlusInfinite();

        Curve convolution = curve.Convolution(infinite);
        Assert.Equal(Rational.PlusInfinity, convolution.ValueAt(0));
        Assert.True(Curve.Equivalent(Curve.PlusInfinite(), convolution));
    }

    public static IEnumerable<object[]> GetFlowControlFiniteCurves()
    {
        //From flow control studies
        var delay = 4;
        var rate = 3;
        var buffer = 5;
        var router_2 = new RateLatencyServiceCurve(rate, delay);
        var flowController_2 = (router_2 + new ConstantCurve(buffer)).SubAdditiveClosure();
        var router_1 = (new RateLatencyServiceCurve(rate, delay)).Convolution(flowController_2);
        var proto_flowController_1 = router_1 + new ConstantCurve(buffer);

        var curves = new Curve[]{
            router_2,
            flowController_2,
            router_1,
            proto_flowController_1
        };

        foreach (var curve in curves)
            yield return new object[] { curve };
    }

    [Theory]
    [MemberData(nameof(GetFiniteCurves))]
    [MemberData(nameof(GetFlowControlFiniteCurves))]
    public void SelfConvolutions(Curve curve)
    {
        Assert.True(curve.IsFinite);

        var curve_2 = curve.Convolution(curve);
        output.WriteLine(curve_2.ToString());
        Assert.True(curve_2.IsFinite);

        var curve_4 = curve_2.Convolution(curve_2);
        output.WriteLine(curve_4.ToString());            
        Assert.True(curve_4.IsFinite);

        var curve_8 = curve_4.Convolution(curve_4);
        output.WriteLine(curve_8.ToString());            
        Assert.True(curve_8.IsFinite);

        var curve_16 = curve_8.Convolution(curve_8);
        output.WriteLine(curve_16.ToString());            
        Assert.True(curve_16.IsFinite);
    }

    [Theory]
    [InlineData(0, 5, 3)]
    [InlineData(1, 5, 3)]
    [InlineData(4, 2, 1)]
    public void OriginPointSelfConvolution(decimal value, decimal length, decimal step)
    {
        var sequence = new Sequence(
            elements: new[] { new Point(time: 0, value: value) },
            fillFrom: 0,
            fillTo: length
        );
        var curve = new Curve(
            baseSequence: sequence,
            pseudoPeriodStart: 0,
            pseudoPeriodLength: length,
            pseudoPeriodHeight: step
        );

        Assert.False(curve.HasTransient);
        Assert.Equal(length, curve.FirstFiniteTimeExceptOrigin);

        var curve_2 = curve.Convolution(curve);

        //todo: optimize for transient removal
        //Assert.False(curve_2.HasTransient);
        Assert.Equal(length, curve_2.FirstFiniteTimeExceptOrigin);
        Assert.False(curve_2.IsFinite);
        Assert.False(curve_2.IsZero);
        Assert.False(curve_2.IsContinuous);
        Assert.False(curve_2.IsContinuousExceptOrigin);
        Assert.False(curve_2.IsUltimatelyPlain);
        Assert.False(curve_2.IsUltimatelyAffine);

        if (value > 0)
            Assert.False(curve_2.Equivalent(curve));
        else
            Assert.True(curve_2.Equivalent(curve));
    }

    [Theory]
    [InlineData(0, 4, 2, 3)]
    [InlineData(2, 4, 3, 3)]
    [InlineData(1, 2, 3, 4)]
    [InlineData(8, 6, 4, 2)]
    [InlineData(8, 6, 8, 6)]
    [InlineData(1, 2, 1, 2)]
    public void RateLatencyConvolution(int delay_a, int rate_a, int delay_b, int rate_b)
    {
        var a = new RateLatencyServiceCurve(rate: rate_a, latency: delay_a);
        var b = new RateLatencyServiceCurve(rate: rate_b, latency: delay_b);

        var convolution = Curve.Convolution(a, b);

        Assert.True(convolution.IsFinite);
        Assert.False(convolution.IsZero);
        Assert.True(convolution.IsContinuous);
        Assert.True(convolution.IsRightContinuous);
        Assert.True(convolution.IsContinuousExceptOrigin);
        Assert.True(convolution.IsLeftContinuous);
        Assert.True(convolution.IsUltimatelyPlain);
        Assert.True(convolution.IsUltimatelyAffine);

        Assert.Equal(delay_a + delay_b, convolution.FirstNonZeroTime);
        Assert.Equal(Rational.Min(rate_a, rate_b), convolution.PseudoPeriodSlope);
    }

    [Theory]
    [InlineData(0, 4, 2, 3)]
    [InlineData(2, 4, 3, 3)]
    [InlineData(1, 2, 3, 4)]
    [InlineData(8, 6, 4, 2)]
    [InlineData(8, 6, 8, 6)]
    [InlineData(1, 2, 1, 2)]
    public void RateLatencyConvolution_Equivalence(int delay_a, int rate_a, int delay_b, int rate_b)
    {
        var a = new RateLatencyServiceCurve(rate: rate_a, latency: delay_a);
        var b = new RateLatencyServiceCurve(rate: rate_b, latency: delay_b);

        var optimized = Curve.Convolution(a, b);
        var unoptimized = Curve.Convolution(new Curve(a), new Curve(b));

        Assert.True(Curve.Equivalent(optimized, unoptimized));
    }

    public static IEnumerable<object[]> GetSameSlopeTestCases()
    {
        var testCases = new (Curve a, Curve b)[]
        {
            (
                a: new Curve(
                    baseSequence: new Sequence(
                        new Element[]{
                            Point.Origin(), 
                            new Segment(0, 5, 3, 0), 
                            new Point(5, 3), 
                            new Segment(5, 6, 3, 2), 
                            new Point(6, 5), 
                            new Segment(6, 7, 5, 0)
                        }),
                    pseudoPeriodStart: 5,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 2
                ),
                b: new Curve(
                    baseSequence: new Sequence(
                        new Element[]{
                            Point.Origin(), 
                            new Segment(0, 5, 1, new Rational(2, 5)), 
                            new Point(5, 3), 
                            new Segment(5, 6, 3, 2), 
                            new Point(6, 5), 
                            new Segment(6, 7, 5, 0)
                        }),
                    pseudoPeriodStart: 5,
                    pseudoPeriodLength: 2,
                    pseudoPeriodHeight: 2
                )
            ),
            (
                a: new Curve(
                    baseSequence: new Sequence(
                        new Element[]{
                            Point.Origin(), 
                            new Segment(0, 3, 1, 3), 
                            new Point(3, 10), 
                            new Segment(3, 6, 10, 2)
                        }),
                    pseudoPeriodStart: 3,
                    pseudoPeriodLength: 3,
                    pseudoPeriodHeight: 6
                ),
                b: new Curve(
                    new FlowControlCurve(5, 4, 10)
                )
            ),
            #if !SKIP_LONG_TESTS
            (
                a: new Curve(
                    new FlowControlCurve(162, 311, 7938)
                ),
                b: new Curve(
                    new FlowControlCurve(279, 64, 13671)
                )
            )
            #endif
        };

        foreach (var testCase in testCases)
        {
            yield return new []{testCase.a, testCase.b};
        }
    }

    [Theory]
    [MemberData(nameof(GetSameSlopeTestCases))]
    public void SameSlopeEquivalence(Curve a, Curve b)
    {
        var fourTerms = Curve.Convolution(a, b, new ComputationSettings { SinglePassConvolution = false });
        var singlePass = Curve.Convolution(a, b, new ComputationSettings { SinglePassConvolution = true });

        Assert.True(Curve.Equivalent(fourTerms, singlePass));
    }
}