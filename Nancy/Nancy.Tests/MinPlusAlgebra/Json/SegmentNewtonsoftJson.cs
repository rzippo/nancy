using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class SegmentNewtonsoftJson
{
    private readonly ITestOutputHelper output;

    public SegmentNewtonsoftJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Segment s, string serialization)> SegmentSerializationPairs = new()
    {
        (Segment.Zero(0, 5),"{\"type\":\"segment\",\"startTime\":0,\"endTime\":5,\"rightLimitAtStartTime\":0,\"slope\":0}"),
        (Segment.Zero(3, 15),"{\"type\":\"segment\",\"startTime\":3,\"endTime\":15,\"rightLimitAtStartTime\":0,\"slope\":0}"),
        (Segment.Constant(3.3m, 8.2m, 4.17m),"{\"type\":\"segment\",\"startTime\":{\"num\":33,\"den\":10},\"endTime\":{\"num\":82,\"den\":10},\"rightLimitAtStartTime\":{\"num\":417,\"den\":100},\"slope\":0}"),
        (new Segment(3, 5, 3, 7),"{\"type\":\"segment\",\"startTime\":3,\"endTime\":5,\"rightLimitAtStartTime\":3,\"slope\":7}"),
        (new Segment(2, 7, 1, 8),"{\"type\":\"segment\",\"startTime\":2,\"endTime\":7,\"rightLimitAtStartTime\":1,\"slope\":8}"),
    };
    
    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (segment, serialization) in SegmentSerializationPairs)
            yield return new object[] { segment };
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Segment segment)
    {
        string serialization = JsonConvert.SerializeObject(segment, new RationalNewtonsoftJsonConverter());
        output.WriteLine(serialization);
        Segment deserialized = JsonConvert.DeserializeObject<Segment>(serialization, new RationalNewtonsoftJsonConverter())!;

        Assert.Equal(segment, deserialized);
    }
    
    public static IEnumerable<object[]> DeserializationTestCases()
    {
        foreach (var (point, serialization) in SegmentSerializationPairs)
            yield return new object[] { point, serialization };    
    }

    [Theory]
    [MemberData(nameof(DeserializationTestCases))]
    public void SegmentDeserialization(Segment expected, string serialization)
    {
        var deserialized = JsonConvert.DeserializeObject<Segment>(serialization, new RationalNewtonsoftJsonConverter())!;
        Assert.Equal(expected, deserialized);
    }
    
    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void MethodsCoherency(Segment segment)
    {
        string serialization = segment.ToString();
        output.WriteLine(serialization);
        var deserialized = Segment.FromJson(serialization);

        Assert.Equal(segment, deserialized);
    }
}