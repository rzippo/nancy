using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.NetworkCalculus.JsonTests;

public class CurveJson
{
    private readonly ITestOutputHelper output;

    public CurveJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> Curves()
    {
        Curve[] testCurves = new Curve[]{
            new Curve(
                baseSequence: new Sequence(new Element[]
                {
                    Point.Origin(),
                    new Segment
                    (
                        rightLimitAtStartTime : 0,
                        slope : 0,
                        startTime : 0,
                        endTime : 3
                    ),
                    new Point(time: 3, value: 5),
                    new Segment
                    (
                        rightLimitAtStartTime : 5,
                        slope : 0,
                        startTime : 3,
                        endTime : 5
                    )
                }),
                pseudoPeriodStart: 3,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 3
            ),
            new SigmaRhoArrivalCurve(sigma: 100, rho: 5),
            new RateLatencyServiceCurve(rate: 20, latency: 10),
            new RaisedRateLatencyServiceCurve(rate: 20, latency: 10, bufferShift: 5),
            new DelayServiceCurve(delay: 10),
            new ConstantCurve(value: 23),
            new StepCurve(value: 13, stepTime: 2),
            new Point(5, 4).SubAdditiveClosure()
        };

        foreach (var curve in testCurves)
        {
            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(Curves))]
    public void CurveSerialization(Curve curve)
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new JsonConverter[] { new GenericCurveConverter() }
        };

        string serialization = JsonConvert.SerializeObject(curve, settings);
        output.WriteLine(serialization);
        Curve deserialized = JsonConvert.DeserializeObject<Curve>(serialization, settings)!;

        Assert.Equal(curve, deserialized);
    }
        
    [Theory]
    [MemberData(nameof(Curves))]
    public void CurveSerializationMethods(Curve curve)
    {
        string serialization = curve.ToString();
        output.WriteLine(serialization);
        Curve deserialized = Curve.FromJson(serialization);

        Assert.Equal(curve, deserialized);
    }
}