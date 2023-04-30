using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class SegmentJson
{
    private readonly ITestOutputHelper output;

    public SegmentJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<object[]> GetSegments()
    {
        Segment[] testSegments = new[]
        {
            Segment.Zero(0, 5),
            Segment.Zero(3, 15),
            Segment.Constant(3.3m, 8.2m, 4.17m),
            new Segment(3, 5, 3, 7),
            new Segment(2, 7, 1, 8)
        };

        foreach (var segment in testSegments)
            yield return new object[] { segment };
    }

    [Theory]
    [MemberData(nameof(GetSegments))]
    public void SegmentSerialization(Segment segment)
    {
        string serialization = JsonConvert.SerializeObject(segment, new RationalConverter());
        output.WriteLine(serialization);
        Segment deserialized = JsonConvert.DeserializeObject<Segment>(serialization, new RationalConverter())!;

        Assert.Equal(segment, deserialized);
    }

    [Theory]
    [MemberData(nameof(GetSegments))]
    public void SegmentSerializationMethod(Segment segment)
    {
        string serialization = segment.ToString();
        output.WriteLine(serialization);
        Segment deserialized = Segment.FromJson(serialization);

        Assert.Equal(segment, deserialized);
    }
}