using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.NetworkCalculus.JsonTests;

public class CurvePolimorphicSystemJson
{
    private readonly ITestOutputHelper output;

    public CurvePolimorphicSystemJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Curve curve, string serialization)> CurveSerializationPairs = new()
    {
        (
            new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,3,0,0), new Point(3,5), new Segment(3,5,5,0) }),pseudoPeriodStart: 3,pseudoPeriodLength: 2,pseudoPeriodHeight: 3),
            "{\"type\":\"curve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":3,\"rightLimitAtStartTime\":0,\"slope\":0},{\"type\":\"point\",\"time\":3,\"value\":5},{\"type\":\"segment\",\"startTime\":3,\"endTime\":5,\"rightLimitAtStartTime\":5,\"slope\":0}]},\"pseudoPeriodStart\":3,\"pseudoPeriodLength\":2,\"pseudoPeriodHeight\":3}"
        ),
        (
            new DelayServiceCurve(10),
            "{\"type\":\"delayServiceCurve\",\"delay\":10}"
        ),
        (
            new RateLatencyServiceCurve(rate: 20, latency: 10),
            "{\"type\":\"rateLatencyServiceCurve\",\"rate\":20,\"latency\":10}"     
        ),
        (
            new ConvexCurve(new RateLatencyServiceCurve(rate: 20, latency: 10)),
            "{\"type\":\"convexCurve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":10,\"rightLimitAtStartTime\":0,\"slope\":0},{\"type\":\"point\",\"time\":10,\"value\":0},{\"type\":\"segment\",\"startTime\":10,\"endTime\":11,\"rightLimitAtStartTime\":0,\"slope\":20}]},\"pseudoPeriodStart\":10,\"pseudoPeriodLength\":1,\"pseudoPeriodHeight\":20}"     
        ),
        (
            new SuperAdditiveCurve(new RateLatencyServiceCurve(rate: 20, latency: 10)),
            "{\"type\":\"superAdditiveCurve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":10,\"rightLimitAtStartTime\":0,\"slope\":0},{\"type\":\"point\",\"time\":10,\"value\":0},{\"type\":\"segment\",\"startTime\":10,\"endTime\":11,\"rightLimitAtStartTime\":0,\"slope\":20}]},\"pseudoPeriodStart\":10,\"pseudoPeriodLength\":1,\"pseudoPeriodHeight\":20}"     
        ),
        (
            new ConstantCurve(value: 23),
            "{\"type\":\"constantCurve\",\"value\":23}"
        ),
        (
            new SigmaRhoArrivalCurve(sigma: 100, rho: 5),
            "{\"type\":\"sigmaRhoArrivalCurve\",\"sigma\":100,\"rho\":5}"
        ),
        (
            new ConcaveCurve(new SigmaRhoArrivalCurve(sigma: 100, rho: 5)),
            "{\"type\":\"concaveCurve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":1,\"rightLimitAtStartTime\":100,\"slope\":5},{\"type\":\"point\",\"time\":1,\"value\":105},{\"type\":\"segment\",\"startTime\":1,\"endTime\":2,\"rightLimitAtStartTime\":105,\"slope\":5}]},\"pseudoPeriodStart\":1,\"pseudoPeriodLength\":1,\"pseudoPeriodHeight\":5}"
        ),
        (
            new SubAdditiveCurve(new SigmaRhoArrivalCurve(sigma: 100, rho: 5)),
            "{\"type\":\"subAdditiveCurve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":1,\"rightLimitAtStartTime\":100,\"slope\":5},{\"type\":\"point\",\"time\":1,\"value\":105},{\"type\":\"segment\",\"startTime\":1,\"endTime\":2,\"rightLimitAtStartTime\":105,\"slope\":5}]},\"pseudoPeriodStart\":1,\"pseudoPeriodLength\":1,\"pseudoPeriodHeight\":5}"
        ),
        (
            new FlowControlCurve(10, 1, 5),
            "{\"type\":\"flowControlCurve\",\"latency\":10,\"rate\":1,\"height\":5}"
        ),
        (
            new StepCurve(value: 13, stepTime: 2),
            "{\"type\":\"stepCurve\",\"value\":13,\"stepTime\":2}"
        ),
        (
            new StairCurve(13, 2),
            "{\"type\":\"stairCurve\",\"a\":13,\"b\":2}"
        ),
        (
            new RaisedRateLatencyServiceCurve(rate: 20, latency: 10, bufferShift: 5),
            "{\"type\":\"raisedRateLatencyServiceCurve\",\"rate\":20,\"latency\":10,\"bufferShift\":5}"
        ),
        (
            new Point(5, 4).SubAdditiveClosure(),
            "{\"type\":\"subAdditiveCurve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":5,\"rightLimitAtStartTime\":{\"num\":1,\"den\":0},\"slope\":0}]},\"pseudoPeriodStart\":0,\"pseudoPeriodLength\":5,\"pseudoPeriodHeight\":4}"
        ),
    };

    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (curve, serialization) in CurveSerializationPairs)
            yield return new object[] { curve };
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Curve curve)
    {
        string serialization = JsonSerializer.Serialize(curve);
        output.WriteLine(serialization);
        var deserialized = JsonSerializer.Deserialize<Curve>(serialization)!;

        Assert.Equal(curve, deserialized);
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void MethodsCoherency(Curve curve)
    {
        var serialization = curve.ToString();
        output.WriteLine(serialization);
        var deserialized = Curve.FromJson(serialization);
        
        Assert.Equal(curve, deserialized);
    }

    public static IEnumerable<object[]> DeserializationTestCases()
    {
        foreach (var (curve, serialization) in CurveSerializationPairs)
            yield return new object[] { curve, serialization };    
    }

    [Theory]
    [MemberData(nameof(DeserializationTestCases))]
    public void CurveDeserialization(Curve expected, string serialization)
    {
        var deserialized = JsonSerializer.Deserialize<Curve>(serialization);
        Assert.Equal(expected, deserialized);
    }

    public static IEnumerable<object[]> SimplifiedRationalNotationCases()
    {
        var testCases = new (object value, string expected)[]
        {
            (
                new RateLatencyServiceCurve(5, 3),
                "{\"type\":\"rateLatencyServiceCurve\",\"rate\":5,\"latency\":3}"
            )
        };

        foreach (var (curve, expected) in testCases)
        {
            yield return new object[] {curve, expected.Replace(" ","")};
        }
    }

    [Theory]
    [MemberData(nameof(SimplifiedRationalNotationCases))]
    public void SimplifiedRationalNotation(Curve curve, string expected)
    {
        var serialization = JsonSerializer.Serialize(curve);
        Assert.Equal(expected, serialization);
    }
}