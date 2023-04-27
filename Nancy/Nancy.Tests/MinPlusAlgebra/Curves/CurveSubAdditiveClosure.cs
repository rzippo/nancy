using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class CurveSubAdditiveClosure
{
    private readonly ITestOutputHelper output;

    public CurveSubAdditiveClosure(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> FlowControllerTestCases()
    {
        var testCases = new (Rational BufferSize, Rational Delay, Rational Rate)[]
        {
            (10, 10, 2),
            (14, 10, 2),
            (5, 15, 3),
            (54, 115, 23),
            (10, 5, 2)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.BufferSize, testCase.Delay, testCase.Rate };
    }

    [Theory]
    [MemberData(nameof(FlowControllerTestCases))]
    public void FlowControllerClosure_Generic(Rational bufferSize, Rational delay, Rational rate)
    {
        ConstantCurve buffer = new ConstantCurve(value: bufferSize);
        RateLatencyServiceCurve router = new RateLatencyServiceCurve(rate: rate, latency: delay);

        RaisedRateLatencyServiceCurve sum = (RaisedRateLatencyServiceCurve) (buffer + router);
        Curve genericSum = new Curve(sum);
        Curve closure = genericSum.SubAdditiveClosure();

        Assert.True(closure.IsFinite);
        Assert.False(closure.IsContinuous);
        Assert.True(closure.IsContinuousExceptOrigin);
        Assert.True(closure.IsLeftContinuous);
        Assert.True(closure.Cut(1, 10 * delay).IsContinuous);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(bufferSize, closure.RightLimitAt(0));
        Assert.Equal(bufferSize, closure.ValueAt(delay));

        Rational fillTime = bufferSize / rate;
        if (fillTime < delay)
        {
            Assert.False(closure.IsUltimatelyAffine);
            Assert.Equal(2 * bufferSize, closure.ValueAt(delay + fillTime));
            Assert.Equal(2 * bufferSize, closure.ValueAt(2 * delay));
            Assert.Equal(3 * bufferSize, closure.ValueAt(2 * delay + fillTime));
            Assert.Equal(3 * bufferSize, closure.ValueAt(3 * delay));

            Assert.Equal(bufferSize / delay, closure.PseudoPeriodSlope);
        }
        else
        {
            Assert.Equal(rate, closure.PseudoPeriodSlope);
            Assert.True(closure.IsUltimatelyAffine);
        }
    }

    [Theory]
    [MemberData(nameof(FlowControllerTestCases))]
    public void FlowControllerClosure_ClosedForm(Rational bufferSize, Rational delay, Rational rate)
    {
        ConstantCurve buffer = new ConstantCurve(value: bufferSize);
        RateLatencyServiceCurve router = new RateLatencyServiceCurve(rate: rate, latency: delay);

        Curve sum = buffer + router;
        Curve closure = sum.SubAdditiveClosure();

        Assert.True(closure.IsFinite);
        Assert.False(closure.IsContinuous);
        Assert.True(closure.IsContinuousExceptOrigin);
        Assert.True(closure.IsLeftContinuous);
        Assert.True(closure.Cut(1, 10 * delay).IsContinuous);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);

        Assert.Equal(0, closure.ValueAt(0));
        Assert.Equal(bufferSize, closure.RightLimitAt(0));
        Assert.Equal(bufferSize, closure.ValueAt(delay));

        Rational fillTime = bufferSize / rate;
        if (fillTime < delay)
        {
            Assert.False(closure.IsUltimatelyAffine);
            Assert.Equal(2 * bufferSize, closure.ValueAt(delay + fillTime));
            Assert.Equal(2 * bufferSize, closure.ValueAt(2 * delay));
            Assert.Equal(3 * bufferSize, closure.ValueAt(2 * delay + fillTime));
            Assert.Equal(3 * bufferSize, closure.ValueAt(3 * delay));

            Assert.Equal(bufferSize / delay, closure.PseudoPeriodSlope);
        }
        else
        {
            Assert.Equal(rate, closure.PseudoPeriodSlope);
            Assert.True(closure.IsUltimatelyAffine);
        }
    }

    public static IEnumerable<object[]> RateLatencyTestCases()
    {
        var testCases = new (Rational Latency, Rational Rate)[]
        {
            (10, 2),
            (5, 3),
            (115, 23),
            (132, 21),
            (0, 3),
            (0, 5),
            (0, 82)
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.Rate, testCase.Latency };
    }

    [Theory]
    [MemberData(nameof(RateLatencyTestCases))]
    public void RateLatencyClosure(Rational rate, Rational latency)
    {
        RateLatencyServiceCurve drsc = new RateLatencyServiceCurve(rate, latency);

        Curve closure = drsc.SubAdditiveClosure();

        Assert.Equal(0, closure.ValueAt(0));
        Assert.True(closure.IsFinite);
        Assert.True(closure.IsContinuous);
        Assert.True(closure.IsRightContinuous);
        Assert.True(closure.IsContinuousExceptOrigin);
        Assert.True(closure.IsLeftContinuous);
        Assert.True(closure.IsUltimatelyPlain);
        Assert.True(closure.IsUltimatelyAffine);

        if (latency > 0)
        {
            Assert.True(closure.IsZero);
        }
        else
        {
            Assert.False(closure.IsZero);
            Assert.Equal(0, closure.FirstNonZeroTime);
            Assert.Equal(rate, closure.PseudoPeriodSlope);
        }
    }

}