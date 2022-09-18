using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class JsonLowerEnvelope
{
    private readonly ITestOutputHelper output;

    public JsonLowerEnvelope(ITestOutputHelper output)
    {
        this.output = output;
    }

    /// <summary>
    /// This is the set of lower envelope tests that that complete even when using Rational as numeric type
    /// </summary>
    public static string[] NonOverflowingElementsNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonLowerEnvelopeTests/1.json"
    };

    /// <summary>
    /// This is the set of lower envelope tests that result in an overflow, unless BigRational is used as numeric type
    /// Note: After application of greedy filtering, this is not resulting in overflows anymore
    /// </summary>
    public static string[] OverflowingElementsNames =
    {
        "Unipi.Nancy.Tests/MinPlusAlgebra/Sequences/JsonLowerEnvelopeTests/8.json"
    };

    public static IEnumerable<object[]> GetNonOverflowingTestCases()
    {
        foreach (var elementsName in NonOverflowingElementsNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(elementsName);
            var elements = JsonConvert.DeserializeObject<Element[]>(json, new RationalConverter())!;

            yield return new object[] { elements };
        }
    }

    public static IEnumerable<object[]> GetOverflowingTestCases()
    {
        foreach (var elementsName in OverflowingElementsNames)
        {
            string json = EmbeddedResourceDataAttribute.ReadManifestData(elementsName);
            var elements = JsonConvert.DeserializeObject<Element[]>(json, new RationalConverter())!;

            yield return new object[] { elements };
        }
    }

    [Theory]
    [MemberData(nameof(GetNonOverflowingTestCases))]
    [MemberData(nameof(GetOverflowingTestCases))]
    public void NonOverflowing(Element[] elements)
    {
        var lowerEnvelope = elements.LowerEnvelope();

        //Used to get the relative JsonMerge test case
        //output.WriteLine(JsonConvert.SerializeObject(lowerEnvelope));

        //Check that the elements are positive, i.e. there was no overflow
        foreach (var element in lowerEnvelope)
        {
            Assert.True(element.StartTime.Sign > 0);
            Assert.True(element.EndTime.Sign > 0);

            Assert.True(element.StartTime <= element.EndTime, $"Invalid element: ${element}");
        }

        Assert.True(lowerEnvelope.AreInTimeSequence());
    }

    //[Theory]
    //[MemberData(nameof(GetOverflowingTestCases))]
    //public void Overflowing(Element[] elements)
    //{
    //After application of greedy filtering, this is not resulting in overflow anymore
    //Assert.Throws<AggregateException>(() => Sequence.LowerEnvelope(elements));
    //}
}