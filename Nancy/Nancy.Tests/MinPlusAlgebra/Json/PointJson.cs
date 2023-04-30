using System.Collections.Generic;
using System.IO.Pipes;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class PointJson
{
    private readonly ITestOutputHelper output;

    public PointJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> GetPoints()
    {
        Point[] testPoints = new[]
        {
            Point.Origin(),
            Point.Zero(0),
            Point.Zero(1),
            Point.Zero(5),
            Point.PlusInfinite(0),
            Point.PlusInfinite(15),
            new Point(0, 0),
            new Point(3, 5),
            new Point(5, 2),
            new Point(12, 18),
            new Point(33, 52),
            new Point(3.18m, 4.5m),
        };

        foreach (var point in testPoints)
            yield return new object[] { point };
    }

    [Theory]
    [MemberData(nameof(GetPoints))]
    public void PointSerialization(Point point)
    {
        string serialization = JsonConvert.SerializeObject(point, new RationalConverter());
        output.WriteLine(serialization);
        Point deserialized = JsonConvert.DeserializeObject<Point>(serialization, new RationalConverter())!;

        Assert.Equal(point, deserialized);
    }

    public static IEnumerable<object[]> GetSerializedPoints()
    {
        var testCases = new[]
        {
            (
                serialization: "{\"time\":{\"num\":0,\"den\":1},\"value\":{\"num\":0,\"den\":1},\"type\":\"point\"}",
                time: 0,
                value: 0
            ),
            (
                serialization: "{\"time\":0,\"value\":0,\"type\":\"point\"}",
                time: 0,
                value: 0
            ),
            (
                serialization: "{\"time\":1,\"value\":2,\"type\":\"point\"}",
                time: 1,
                value: 2
            ),
            (
                serialization: "{\"time\":{\"num\":113,\"den\":10},\"value\":{\"num\":2414,\"den\":100},\"type\":\"point\"}",
                time: 11.3m,
                value: 24.14m
            )
        };

        foreach (var testCase in testCases)
            yield return new object[] { testCase.serialization, testCase.time, testCase.value };
    }

    [Theory]
    [MemberData(nameof(GetSerializedPoints))]
    public void PointDeserialization(string serialization, decimal expectedTime, decimal expectedValue)
    {
        Point deserialized = JsonConvert.DeserializeObject<Point>(serialization, new RationalConverter())!;
        Assert.Equal(expectedTime, deserialized.Time);
        Assert.Equal(expectedValue, deserialized.Value);
    }

    [Theory]
    [MemberData(nameof(GetSerializedPoints))]
    public void PointDeserializationMethods(string serialization, decimal expectedTime, decimal expectedValue)
    {
        Point deserialized = Point.FromJson(serialization);
        Assert.Equal(expectedTime, deserialized.Time);
        Assert.Equal(expectedValue, deserialized.Value);

        var ser2 = deserialized.ToString();
        var p2 = Point.FromJson(ser2);
        Assert.Equal(deserialized, p2);
    }
}