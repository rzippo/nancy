using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class SequenceNewtonsoftJson
{
    private readonly ITestOutputHelper output;

    public SequenceNewtonsoftJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<(Sequence sequence, string serialization)> SequenceSerializationPairs = new()
    {
        (
            TestFunctions.SequenceA,
            "{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":20},{\"type\":\"segment\",\"startTime\":0,\"endTime\":4,\"rightLimitAtStartTime\":20,\"slope\":{\"num\":-5,\"den\":4}},{\"type\":\"point\",\"time\":4,\"value\":15},{\"type\":\"segment\",\"startTime\":4,\"endTime\":6,\"rightLimitAtStartTime\":15,\"slope\":3}]}"
        ),
        (
            TestFunctions.SequenceB,
            "{\"elements\":[{\"type\":\"point\",\"time\":0,\"value\":5},{\"type\":\"segment\",\"startTime\":0,\"endTime\":3,\"rightLimitAtStartTime\":5,\"slope\":5},{\"type\":\"point\",\"time\":3,\"value\":20},{\"type\":\"segment\",\"startTime\":3,\"endTime\":7,\"rightLimitAtStartTime\":20,\"slope\":-5},{\"type\":\"point\",\"time\":7,\"value\":0},{\"type\":\"segment\",\"startTime\":7,\"endTime\":8,\"rightLimitAtStartTime\":0,\"slope\":0}]}"
        )
    };

    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (sequence, serialization) in SequenceSerializationPairs)
        {
            yield return new object[] { sequence };
        }
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Sequence sequence)
    {
        string serialization = JsonConvert.SerializeObject(sequence, new RationalNewtonsoftJsonConverter());
        output.WriteLine(serialization);
        Sequence deserialized = JsonConvert.DeserializeObject<Sequence>(serialization, new RationalNewtonsoftJsonConverter())!;

        Assert.Equal(sequence, deserialized);
    }
    
    public static IEnumerable<object[]> DeserializationTestCases()
    {
        foreach (var (sequence, serialization) in SequenceSerializationPairs)
            yield return new object[] { sequence, serialization };    
    }
    
    [Theory]
    [MemberData(nameof(DeserializationTestCases))]
    public void SequenceDeserialization(Sequence expected, string serialization)
    {
        var deserialized = JsonConvert.DeserializeObject<Sequence>(serialization, new RationalNewtonsoftJsonConverter())!;
        Assert.Equal(expected, deserialized);
    }
}