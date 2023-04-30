using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.NetworkCalculus;

public class CurveLowerBounds
{
    private readonly ITestOutputHelper output;

    public CurveLowerBounds(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static string[] CurveNames =
    {
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/1.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/2.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/3.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/4.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/5.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/6.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/7.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/8.json",
        "Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/9.json"
    };

    public static IEnumerable<string> GetCurvesJson()
    {
        yield return "{\"type\":\"rateLatencyServiceCurve\",\"latency\":10,\"rate\":10 }";

        foreach (var curveName in CurveNames)
            yield return EmbeddedResourceDataAttribute.ReadManifestData(curveName);
    }

    public static IEnumerable<object[]> GetTwoRatesTestCases()
    {
        foreach (string json in GetCurvesJson())
            yield return new[] { json };
    }

    [Theory]
    [MemberData(nameof(GetTwoRatesTestCases))]
    public void TwoRatesTest(string curveJson)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(curveJson, new GenericCurveConverter(), new RationalConverter())!;
        var delay = curve.FirstNonZeroTime;

        var twoRates = curve.TwoRatesLowerBound();

        output.WriteLine(JsonConvert.SerializeObject(twoRates, new GenericCurveConverter(), new RationalConverter()));

        Assert.True(twoRates.IsContinuous);
        Assert.True(twoRates.IsRightContinuous);
        Assert.True(twoRates.IsFinite);
        Assert.True(twoRates.IsUltimatelyPlain);

        switch (twoRates)
        {
            case RateLatencyServiceCurve dr:
                Assert.True(dr.Latency >= delay);
                break;

            case TwoRatesServiceCurve tr:
                Assert.True(tr.Delay >= delay);
                break;
        }

        Assert.True(twoRates <= curve);
    }

    public static IEnumerable<object[]> GetRateLatencyTestCases()
    {
        var alphas = new[]
        {
            0.0m, 0.5m, 1.0m
        };

        foreach (string json in GetCurvesJson())
        foreach (decimal alpha in alphas)
            yield return new object[] { json, alpha };
    }

    [Theory]
    [MemberData(nameof(GetRateLatencyTestCases))]
    public void RateLatencyTest(string curveJson, decimal alpha)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(curveJson, new GenericCurveConverter())!;
        var delay = curve.FirstNonZeroTime;

        var rateLatency = curve.RateLatencyLowerBound(alpha);

        output.WriteLine(JsonConvert.SerializeObject(rateLatency, new GenericCurveConverter()));

        Assert.True(rateLatency.IsContinuous);
        Assert.True(rateLatency.IsRightContinuous);
        Assert.True(rateLatency.IsFinite);
        Assert.True(rateLatency.IsUltimatelyPlain);
        Assert.True(rateLatency.Latency >= delay);

        Assert.True(rateLatency <= curve);
    }

    public static IEnumerable<object[]> GetRateLatencyAlphaTestCases()
    {
        yield return new object[]
        {
            EmbeddedResourceDataAttribute.ReadManifestData("Unipi.Nancy.Tests/NetworkCalculus/CurveExamples/9.json"),
            new Rational(3), new Rational(1, 2),
            new Rational(6), new Rational(2)
        };
    }

    [Theory]
    [MemberData(nameof(GetRateLatencyAlphaTestCases))]
    public void RateLatencyAlphaTest(string curveJson, Rational minDelay, Rational minRate, Rational maxDelay, Rational maxRate)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(curveJson, new GenericCurveConverter(), new RationalConverter())!;

        var minRateLatency = curve.RateLatencyLowerBound(alpha: 0);
        Assert.True(minRateLatency <= curve);
        Assert.Equal(minRateLatency.Latency, minDelay);
        Assert.Equal(minRateLatency.Rate, minRate);

        var maxRateLatency = curve.RateLatencyLowerBound(alpha: 1);
        Assert.True(maxRateLatency <= curve);
        Assert.Equal(maxRateLatency.Latency, maxDelay);
        Assert.Equal(maxRateLatency.Rate, maxRate);
    }

    public static IEnumerable<object[]> GetRateLatencyFromPointsTestCases()
    {
        var testCases = new[]
        {
            new {
                points = new[]
                {
                    new Point(time: 1, value: 0),
                    new Point(time: 2, value: 2),
                    new Point(time: 3, value: 2),
                    new Point(time: 4, value: 4),
                },
                alpha = 0.0m,
                expected = (delay: new Rational(1), rate: new Rational(1))
            },
            new {
                points = new[]
                {
                    new Point(time: 1, value: 0),
                    new Point(time: 2, value: 2),
                    new Point(time: 3, value: 2),
                    new Point(time: 4, value: 4),
                },
                alpha = 1.0m,
                expected = (delay: new Rational(2), rate: new Rational(2))
            },
            new {
                points = new[]
                {
                    new Point(time: 1, value: 0),
                    new Point(time: 2, value: 2),
                    new Point(time: 2, value: 3),
                    new Point(time: 4, value: 4),
                },
                alpha = 0.0m,
                expected = (delay: new Rational(1), rate: new Rational(4,3))
            },
            new {
                points = new[]
                {
                    new Point(time: 1, value: 0),
                    new Point(time: 2, value: 2),
                    new Point(time: 2, value: 3),
                    new Point(time: 4, value: 4),
                },
                alpha = 1.0m,
                expected = (delay: new Rational(1), rate: new Rational(4,3))
            }
        };

        foreach (var testCase in testCases)
        {
            yield return new object[] {testCase.points, testCase.alpha, testCase.expected};
        }

    }

    [Theory]
    [MemberData(nameof(GetRateLatencyFromPointsTestCases))]
    public void RateLatencyFromPointsTest(Point[] points, decimal alpha, (Rational delay, Rational rate) expected)
    {
        var rateLatency = Nancy.NetworkCalculus.CurveLowerBounds.RateLatencyLowerBound(points, alpha);

        // output.WriteLine(JsonConvert.SerializeObject(rateLatency, new GenericCurveConverter()));

        Assert.True(rateLatency.IsContinuous);
        Assert.True(rateLatency.IsRightContinuous);
        Assert.True(rateLatency.IsFinite);
        Assert.True(rateLatency.IsUltimatelyPlain);
        Assert.True(rateLatency.Latency == expected.delay);
        Assert.True(rateLatency.Rate == expected.rate);

        foreach (var point in points)
        {
            Assert.True(rateLatency <= point);    
        }
    }
}