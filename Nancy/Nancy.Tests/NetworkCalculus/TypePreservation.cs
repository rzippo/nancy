using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus;

/// <summary>
/// The specialized types must survive the operations that preserve their property,
/// and each result must equal the same computation with the types erased.
/// </summary>
public class TypePreservation
{
    private static Curve Erased(Curve curve) => new Curve(curve);

    [Fact]
    public void SumOfSubAdditiveCurvesIsSubAdditive()
    {
        var a = new FlowControlCurve(latency: 3, rate: 2, height: 5);
        var b = new FlowControlCurve(latency: 4, rate: 3, height: 2);

        var sum = a + b;

        Assert.IsAssignableFrom<SubAdditiveCurve>(sum);
        Assert.True(Curve.Equivalent(Erased(a) + Erased(b), sum));
        Assert.True(new Curve(sum).IsRegularSubAdditive);
    }

    [Fact]
    public void SumOfSuperAdditiveCurvesIsSuperAdditive()
    {
        var a = new SuperAdditiveCurve(new RateLatencyServiceCurve(2, 3), doTest: false);
        var b = new SuperAdditiveCurve(new RateLatencyServiceCurve(3, 1), doTest: false);

        var sum = a + b;

        Assert.IsAssignableFrom<SuperAdditiveCurve>(sum);
        Assert.True(Curve.Equivalent(Erased(a) + Erased(b), sum));
        Assert.True(new Curve(sum).IsRegularSuperAdditive);
    }

    [Fact]
    public void SumOfConstantCurvesIsConstant()
    {
        var a = new ConstantCurve(5);
        var b = new ConstantCurve(3);

        var sum = a + b;

        Assert.IsType<ConstantCurve>(sum);
        Assert.Equal(8, ((ConstantCurve)sum).Value);
        Assert.True(Curve.Equivalent(Erased(a) + Erased(b), sum));
    }

    public static List<(Rational sigma, Rational rho, Rational shift)> BurstShifts =
    [
        (3, 2, 5),
        (3, 2, -1),
        (3, 2, -3),
        (3, 2, -5)
    ];

    public static IEnumerable<object[]> GetBurstShifts()
        => BurstShifts.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBurstShifts))]
    public void ShiftingAnArrivalCurveRaisesItsBurst(Rational sigma, Rational rho, Rational shift)
    {
        var arrival = new SigmaRhoArrivalCurve(sigma, rho);

        var shifted = arrival.VerticalShift(shift, exceptOrigin: true);

        Assert.True(Curve.Equivalent(Erased(arrival).VerticalShift(shift, exceptOrigin: true), shifted));
        // the burst must stay non-negative for the result to be an arrival curve of this kind
        if ((sigma + shift).IsNegative)
            Assert.IsType<Curve>(shifted);
        else
            Assert.Equal(sigma + shift, Assert.IsType<SigmaRhoArrivalCurve>(shifted).Sigma);
    }

    /// <summary>
    /// Covers both operands with a zero or positive latency.
    /// </summary>
    public static List<Curve> InfiniteShiftOperands =
    [
        new SigmaRhoArrivalCurve(sigma: 3, rho: 2),
        new ConstantCurve(5),
        new RaisedRateLatencyServiceCurve(rate: 2, latency: 3, bufferShift: 5),
        new RaisedRateLatencyServiceCurve(rate: 2, latency: 3, bufferShift: 5, withZeroOrigin: true),
        new RaisedRateLatencyServiceCurve(rate: 2, latency: 0, bufferShift: 5),
        new RaisedRateLatencyServiceCurve(rate: 2, latency: 0, bufferShift: 5, withZeroOrigin: true),
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new RateLatencyServiceCurve(rate: 2, latency: 0)
    ];

    public static IEnumerable<object[]> GetInfiniteShiftOperands()
        => InfiniteShiftOperands.ToXUnitTestCases();

    // the burst of an arrival curve cannot hold an infinite value, so the shortcut must not be taken for one
    [Theory]
    [MemberData(nameof(GetInfiniteShiftOperands))]
    public void ShiftingByAnInfiniteFactorLeavesTheSpecializedTypes(Curve curve)
    {
        foreach (var shift in new[] { Rational.PlusInfinity, Rational.MinusInfinity })
        foreach (var exceptOrigin in new[] { true, false })
            Assert.True(
                Curve.Equivalent(Erased(curve).VerticalShift(shift, exceptOrigin), curve.VerticalShift(shift, exceptOrigin)),
                $"{curve.GetType().Name}.VerticalShift({shift}, {exceptOrigin})"
            );
    }

    /// <summary>
    /// The buffer a shift lands on, covering both signs and a zero latency,
    /// since the shape of the curve at the origin is where the latency and the buffer interact.
    /// </summary>
    public static List<(Rational latency, Rational bufferShift, Rational shift)> BufferShifts =
    [
        (3, 5, 4),
        (3, 5, -9),
        (0, 5, 4),
        (0, 5, -5),
        (0, 5, -9)
    ];

    public static IEnumerable<object[]> GetBufferShifts()
        => BufferShifts.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBufferShifts))]
    public void ShiftingARaisedRateLatencyRaisesItsBuffer(Rational latency, Rational bufferShift, Rational shift)
    {
        foreach (var hasZeroOrigin in new[] { true, false })
        {
            var curve = new RaisedRateLatencyServiceCurve(rate: 2, latency: latency, bufferShift: bufferShift, withZeroOrigin: hasZeroOrigin);

            foreach (var exceptOrigin in new[] { true, false })
            {
                var shifted = curve.VerticalShift(shift, exceptOrigin);
                var because = $"withZeroOrigin: {hasZeroOrigin}, exceptOrigin: {exceptOrigin}";

                Assert.True(Curve.Equivalent(Erased(curve).VerticalShift(shift, exceptOrigin), shifted), because);
                // the origin lands where this type puts it only when the two agree
                if (exceptOrigin == hasZeroOrigin)
                    Assert.Equal(bufferShift + shift, Assert.IsType<RaisedRateLatencyServiceCurve>(shifted).BufferShift);
                else
                    Assert.IsType<Curve>(shifted);
            }
        }
    }

    /// <summary>
    /// Scaling factors, including the ones the specialized types cannot hold:
    /// a negative factor turns an arrival curve concave-side-down and a service curve convex-side-down,
    /// and an infinite one is neither a burst nor a rate.
    /// </summary>
    public static List<Rational> ScalingFactors =
    [
        2, 1, new Rational(1, 2), 0, -1, -4, Rational.PlusInfinity, Rational.MinusInfinity
    ];

    public static IEnumerable<object[]> GetScalingFactors()
        => ScalingFactors.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetScalingFactors))]
    public void ScalingAnArrivalCurveScalesItsBurstAndRate(Rational scaling)
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 3, rho: 2);

        var scaled = arrival.Scale(scaling);

        Assert.True(Curve.Equivalent(Erased(arrival).Scale(scaling), scaled));
        // the burst and the rate must stay finite and non-negative for the result to be an arrival curve of this kind
        if (scaling.IsFinite && !scaling.IsNegative)
        {
            var typed = Assert.IsType<SigmaRhoArrivalCurve>(scaled);
            Assert.Equal(3 * scaling, typed.Sigma);
            Assert.Equal(2 * scaling, typed.Rho);
        }
        else
            Assert.IsType<Curve>(scaled);
    }

    [Theory]
    [MemberData(nameof(GetScalingFactors))]
    public void ScalingARateLatencyScalesItsRate(Rational scaling)
    {
        var service = new RateLatencyServiceCurve(rate: 2, latency: 3);

        var scaled = service.Scale(scaling);

        Assert.True(Curve.Equivalent(Erased(service).Scale(scaling), scaled));
        // the rate must stay finite and non-negative for the result to be a service curve of this kind
        if (scaling.IsFinite && !scaling.IsNegative)
        {
            var typed = Assert.IsType<RateLatencyServiceCurve>(scaled);
            Assert.Equal(2 * scaling, typed.Rate);
            Assert.Equal(3, typed.Latency);
        }
        else
            Assert.IsType<Curve>(scaled);
    }

    /// <summary>
    /// Delays, including the ones <see cref="Curve.DelayBy"/> documents as invalid.
    /// </summary>
    public static List<Rational> Delays =
    [
        0, 1, 5, -1, -5, Rational.PlusInfinity, Rational.MinusInfinity
    ];

    public static IEnumerable<object[]> GetDelays()
        => Delays.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDelays))]
    public void DelayingARateLatencyRaisesItsLatency(Rational delay)
    {
        var service = new RateLatencyServiceCurve(rate: 2, latency: 3);

        // the delay must be non-negative and finite, which the optimized path must enforce as the base one does
        if (!delay.IsFinite || delay.IsNegative)
        {
            // the message must be the one the base method gives, not one from further down the shortcut
            var fromBase = Assert.Throws<ArgumentException>(() => Erased(service).DelayBy(delay));
            var fromOptimized = Assert.Throws<ArgumentException>(() => service.DelayBy(delay));
            Assert.Equal(fromBase.Message, fromOptimized.Message);
            return;
        }

        var delayed = service.DelayBy(delay);

        Assert.True(Curve.Equivalent(Erased(service).DelayBy(delay), delayed));
        var typed = Assert.IsType<RateLatencyServiceCurve>(delayed);
        Assert.Equal(2, typed.Rate);
        Assert.Equal(3 + delay, typed.Latency);
    }

    [Fact]
    public void StaticWindowFlowControlKeepsItsTypes()
    {
        // the equivalent service curve of a window flow controlled server, as in [ZS23]:
        // the service curve raised by the window size, then closed, then convolved along the tandem
        var beta1 = new RateLatencyServiceCurve(rate: 20, latency: 10);
        var beta2 = new RateLatencyServiceCurve(rate: 15, latency: 5);
        var window = new ConstantCurve(30);

        var raised = beta1 + window;
        Assert.IsType<RaisedRateLatencyServiceCurve>(raised);
        Assert.Equal(0, raised.ValueAt(0));

        var flowController = raised.SubAdditiveClosure();
        Assert.IsType<FlowControlCurve>(flowController);

        var equivalent = flowController.Convolution(new SubAdditiveCurve(beta2.SubAdditiveClosure(), doTest: false));
        Assert.IsAssignableFrom<SubAdditiveCurve>(equivalent);

        // the whole pipeline must agree with the same computation over plain curves
        var erasedEquivalent = Curve.Convolution(
            Erased(Erased(beta1 + window).SubAdditiveClosure()),
            Erased(Erased(beta2).SubAdditiveClosure())
        );
        Assert.True(Curve.Equivalent(erasedEquivalent, equivalent));
    }

    [Fact]
    public void TokenBucketPipelineKeepsItsTypes()
    {
        // a token bucket policed flow through a rate-latency server, the shape of the delay bound example in [DNC18]
        var arrival = new SigmaRhoArrivalCurve(sigma: 100, rho: 10);
        var service = new RateLatencyServiceCurve(rate: 20, latency: 4);

        // an extra burst allowance keeps the arrival curve an arrival curve
        var raisedArrival = arrival.VerticalShift(20, exceptOrigin: true);
        Assert.IsType<SigmaRhoArrivalCurve>(raisedArrival);
        Assert.Equal(120, ((SigmaRhoArrivalCurve)raisedArrival).Sigma);

        // the aggregate of two flows is still concave
        var aggregate = arrival + new SigmaRhoArrivalCurve(sigma: 50, rho: 5);
        Assert.IsAssignableFrom<ConcaveCurve>(aggregate);
        Assert.True(Curve.Equivalent(Erased(arrival) + Erased(new SigmaRhoArrivalCurve(50, 5)), aggregate));

        // the tandem of two service curves is still a rate-latency one
        var tandem = service.Convolution(new RateLatencyServiceCurve(rate: 30, latency: 2));
        Assert.IsType<RateLatencyServiceCurve>(tandem);
        Assert.Equal(20, tandem.Rate);
        Assert.Equal(6, tandem.Latency);

        var delay = Curve.HorizontalDeviation(raisedArrival, service);
        Assert.True(delay.IsFinite);
    }
}
