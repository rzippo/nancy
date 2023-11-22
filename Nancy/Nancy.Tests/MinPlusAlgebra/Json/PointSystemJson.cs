using System.Collections.Generic;
using System.IO.Pipes;
using System.Text.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class PointSystemJson
{
    private readonly ITestOutputHelper output;

    public PointSystemJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Point p, string serialization)> PointSerializationPairs = new()
    {
        (Point.Origin(),"{\"type\":\"point\",\"time\":0,\"value\":0}"),
        (Point.Zero(0),"{\"type\":\"point\",\"time\":0,\"value\":0}"),
        (Point.Zero(1),"{\"type\":\"point\",\"time\":1,\"value\":0}"),
        (Point.Zero(5),"{\"type\":\"point\",\"time\":5,\"value\":0}"),
        (Point.PlusInfinite(0),"{\"type\":\"point\",\"time\":0,\"value\":{\"num\":1,\"den\":0}}"),
        (Point.PlusInfinite(15),"{\"type\":\"point\",\"time\":15,\"value\":{\"num\":1,\"den\":0}}"),
        (new Point(0, 0),"{\"type\":\"point\",\"time\":0,\"value\":0}"),
        (new Point(3, 5),"{\"type\":\"point\",\"time\":3,\"value\":5}"),
        (new Point(5, 2),"{\"type\":\"point\",\"time\":5,\"value\":2}"),
        (new Point(12, 18),"{\"type\":\"point\",\"time\":12,\"value\":18}"),
        (new Point(33, 52),"{\"type\":\"point\",\"time\":33,\"value\":52}"),
        (new Point(3.18m, 4.5m),"{\"type\":\"point\",\"time\":{\"num\":318,\"den\":100},\"value\":{\"num\":45,\"den\":10}}"),
    };
    
    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (point, serialization) in PointSerializationPairs)
            yield return new object[] { point };
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Point point)
    {
        string serialization = JsonSerializer.Serialize(point);
        output.WriteLine(serialization);
        Point deserialized = JsonSerializer.Deserialize<Point>(serialization)!;

        Assert.Equal(point, deserialized);
    }

    public static IEnumerable<object[]> DeserializationTestCases()
    {
        foreach (var (point, serialization) in PointSerializationPairs)
            yield return new object[] { point, serialization };
    }

    [Theory]
    [MemberData(nameof(DeserializationTestCases))]
    public void PointDeserialization(Point expected, string serialization)
    {
        Point deserialized = JsonSerializer.Deserialize<Point>(serialization)!;
        Assert.Equal(expected.Time, deserialized.Time);
        Assert.Equal(expected.Value, deserialized.Value);
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void MethodsCoherency(Point point)
    {
        var serialization = point.ToString();
        var p2 = Point.FromJson(serialization);
        Assert.Equal(point.Time, p2.Time);
        Assert.Equal(point.Value, p2.Value);
    }
}