using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Json;

public class ElementSystemJson
{
    private readonly ITestOutputHelper output;

    public ElementSystemJson(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static IEnumerable<(Element e, string serialization)> ElementSerializationPairs =
        PointNewtonsoftJson.PointSerializationPairs.Select(pair => ((Element)pair.p, pair.serialization))
            .Concat(SegmentNewtonsoftJson.SegmentSerializationPairs.Select(
                pair => ((Element)pair.s, pair.serialization)));
    
    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (element, serialization) in ElementSerializationPairs)
            yield return new object[] { element };
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Element element)
    {
        string serialization = JsonSerializer.Serialize(element);
        output.WriteLine(serialization);
        var deserialized = JsonSerializer.Deserialize<Element>(serialization)!;

        Assert.Equal(element, deserialized);
    }
    
    public static IEnumerable<object[]> DeserializationTestCases()
    {
        foreach (var (element, serialization) in ElementSerializationPairs)
            yield return new object[] { element, serialization };    
    }

    [Theory]
    [MemberData(nameof(DeserializationTestCases))]
    public void ElementDeserialization(Element expected, string serialization)
    {
        var deserialized = JsonSerializer.Deserialize<Element>(serialization)!;
        Assert.Equal(expected, deserialized);
    }
    
    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void MethodsCoherency(Element element)
    {
        var serialization = element.ToString();
        var deserialized = Element.FromJson(serialization);
        Assert.Equal(element, deserialized);
    }
}