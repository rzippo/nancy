using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.MinPlusAlgebra.Json;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class CurveNewtonsoftJson
{
    private readonly ITestOutputHelper output;

    public CurveNewtonsoftJson(ITestOutputHelper output)
    {
        this.output = output;
    }
    
    public static List<(Curve curve, string serialization)> CurveSerializationPairs = new()
    {
        (
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
            "{\"type\":\"curve\",\"baseSequence\":{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":0},{\"type\":\"segment\",\"startTime\":0,\"endTime\":3,\"rightLimitAtStartTime\":0,\"slope\":0},{\"type\":\"point\",\"time\":3,\"value\":5},{\"type\":\"segment\",\"startTime\":3,\"endTime\":5,\"rightLimitAtStartTime\":5,\"slope\":0}]},\"pseudoPeriodStart\":3,\"pseudoPeriodLength\":2,\"pseudoPeriodHeight\":3}"
        )
    };

    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (curve, serialization) in CurveSerializationPairs)
        {
            yield return new object[] { curve };
        }
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Curve curve)
    {
        var settings = new JsonSerializerSettings {
            Converters = new JsonConverter[] { new CurveNewtonsoftJsonConverter() }
        };

        string serialization = JsonConvert.SerializeObject(curve, settings);
        output.WriteLine(serialization);
        Curve deserialized = JsonConvert.DeserializeObject<Curve>(serialization, settings)!;

        Assert.Equal(curve, deserialized);
    }

    public static IEnumerable<object[]> DeserializeTestCases()
    {
        foreach (var (curve, serialization) in CurveSerializationPairs)
        {
            yield return new object[] { curve, serialization };
        }
    }
    
    [Theory]
    [MemberData(nameof(DeserializeTestCases))]
    public void Deserialization(Curve expected, string serialization)
    {
        var settings = new JsonSerializerSettings {
            Converters = new JsonConverter[] { new CurveNewtonsoftJsonConverter() }
        };

        Curve deserialized = JsonConvert.DeserializeObject<Curve>(serialization, settings)!;
        output.WriteLine($"var deserialized = {deserialized.ToCodeString()};");
        
        Assert.Equal(expected, deserialized);
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void MethodsCoherency(Curve curve)
    {
        var serialization = curve.ToString();
        var deserialized = Curve.FromJson(serialization);
        Assert.Equal(curve, deserialized);
    }
}