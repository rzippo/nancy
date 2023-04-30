using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveDeconvolution
{
    [Fact]
    public void SigmaRho_RateLatency_0()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 20, rho: 10);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 30, latency: 10);

        Curve deconvolution = arrival.Deconvolution(service);

        Assert.False(deconvolution.IsZero);
        Assert.True(deconvolution.IsContinuous);
        Assert.True(deconvolution.IsRightContinuous);
        Assert.True(deconvolution.IsContinuousExceptOrigin);
        Assert.True(deconvolution.IsLeftContinuous);
        Assert.True(deconvolution.IsUltimatelyPlain);
        Assert.True(deconvolution.IsUltimatelyAffine);

        Assert.Equal(120, deconvolution.ValueAt(0));
        Assert.Equal(120, deconvolution.RightLimitAt(0));
        Assert.Equal(arrival.PseudoPeriodSlope, deconvolution.PseudoPeriodSlope);
    }

    [Fact]
    public void SigmaRho_RateLatency_1()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 15, rho: 30);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 40, latency: 25);

        Curve deconvolution = arrival.Deconvolution(service);

        Assert.False(deconvolution.IsZero);
        Assert.True(deconvolution.IsContinuous);
        Assert.True(deconvolution.IsRightContinuous);
        Assert.True(deconvolution.IsContinuousExceptOrigin);
        Assert.True(deconvolution.IsLeftContinuous);
        Assert.True(deconvolution.IsUltimatelyPlain);
        Assert.True(deconvolution.IsUltimatelyAffine);

        Assert.Equal(765, deconvolution.ValueAt(0));
        Assert.Equal(765, deconvolution.RightLimitAt(0));
        Assert.Equal(arrival.PseudoPeriodSlope, deconvolution.PseudoPeriodSlope);
    }

    [Fact]
    public void SigmaRho_RateLatency_2()
    {
        SigmaRhoArrivalCurve arrival = new SigmaRhoArrivalCurve(sigma: 15, rho: 30);
        RateLatencyServiceCurve service = new RateLatencyServiceCurve(rate: 20, latency: 10);

        Curve deconvolution = arrival.Deconvolution(service);

        Assert.False(deconvolution.IsZero);
        Assert.True(deconvolution.IsContinuous);
        Assert.True(deconvolution.IsUltimatelyPlain);
        Assert.True(deconvolution.IsUltimatelyInfinite);

        Assert.Equal(Rational.PlusInfinity, deconvolution.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, deconvolution.ValueAt(70));
        Assert.Equal(Rational.PlusInfinity, deconvolution.ValueAt(100));
    }

    [Fact]
    public void Generic()
    {
        // values for this test are form RTaW playground
        var arrivalParts = new[]
        {
            new SigmaRhoArrivalCurve(sigma: 2, rho: new Rational(7, 4)),
            new SigmaRhoArrivalCurve(sigma: 3, rho: new Rational(4, 5)),
            new SigmaRhoArrivalCurve(sigma: 5, rho: new Rational(2, 5))
        };
        var arrival = Curve.Minimum(arrivalParts);
        var service = new RateLatencyServiceCurve(2, 2);
        var expected = arrival.AnticipateBy(2);

        var deconv = Curve.Deconvolution(arrival, service);
        Assert.True(Curve.Equivalent(expected, deconv));
    }

    public static IEnumerable<object[]> GetConvolutionInverseTestCases()
    {
        var testcases = new (Curve arrival, Curve service)[]
        {
            (
                arrival: new SigmaRhoArrivalCurve(3, 4),
                service: new RateLatencyServiceCurve(5, 2)
            ),
            (
                arrival: new SigmaRhoArrivalCurve(3, 0.8m),
                service: new FlowControlCurve(5, 2, 5)
            )
        };

        foreach (var (arrival, service) in testcases)
        {
            yield return new object[] { arrival, service };
        }
    }

    [Theory]
    [MemberData(nameof(GetConvolutionInverseTestCases))]
    public void ConvolutionInverse(Curve arrival, Curve service)
    {
        var departure = Curve.Convolution(arrival, service);
        // this is not the same service curve, but it will yield the same departure 
        var service2 = Curve.Deconvolution(departure, arrival);
        var departure2 = Curve.Convolution(arrival, service2);
        Assert.True(Curve.Equivalent(departure, departure2));
    }

    [Theory]
    [MemberData(nameof(GetConvolutionInverseTestCases))]
    public void ConvolutionInverse_Generic(Curve arrival, Curve service)
    {
        var arrival_generic = new Curve(arrival);
        var service_generic = new Curve(service);

        var departure = Curve.Convolution(arrival_generic, service_generic);
        // this is not the same service curve, but it will yield the same departure 
        var service2 = Curve.Deconvolution(departure, arrival_generic);
        var departure2 = Curve.Convolution(arrival_generic, service2);
        Assert.True(Curve.Equivalent(departure, departure2));
    }

    public static IEnumerable<object[]> GetSubAdditiveSelfDeconvolutionTestCases()
    {
        var testcases = new SubAdditiveCurve[]
        {
            new SigmaRhoArrivalCurve(1, 0),
            new SigmaRhoArrivalCurve(5, 0),
            new SigmaRhoArrivalCurve(5, 3),
            new FlowControlCurve(3, 4, 2),
        };

        foreach (var curve in testcases)
        {
            yield return new object[] { curve };
        }
    }

    /// <summary>
    /// Tests the equivalence stated in [DNC18] Proposition 2.9 point 1)
    /// </summary>
    [Theory]
    [MemberData(nameof(GetSubAdditiveSelfDeconvolutionTestCases))]
    public void SubAdditiveSelfDeconvolution(SubAdditiveCurve curve)
    {
        var self_deconv = curve.Deconvolution(curve);
        Assert.Equal(curve, self_deconv);
    }
}