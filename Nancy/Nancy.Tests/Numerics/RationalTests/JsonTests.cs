using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class JsonTests
{
    [Fact]
    public void SerializeFinite()
    {
        Rational value = new Rational(1, 2);
        string expected = "{\"num\":1,\"den\":2}";

        string serialization = JsonConvert.SerializeObject(value, new RationalConverter());

        Assert.Equal(expected, serialization);
    }

    [Theory]
    [InlineData("{\"num\":1,\"den\":2}", 1, 2)]
    [InlineData("{\"num\":2,\"den\":1}", 2, 1)]
    [InlineData("2", 2, 1)]
    public void DeserializeFinite(string serialization, int expectedNum, int expectedDen)
    {
        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsFinite);
        Assert.Equal(expectedNum, deserialized.Numerator);
        Assert.Equal(expectedDen, deserialized.Denominator);
    }

    [Fact]
    public void SerializePlusInfinite()
    {
        Rational value = Rational.PlusInfinity;
        string expected = "{\"num\":1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value, new RationalConverter());

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializePlusInfinite()
    {
        string serialization = "{\"num\":1,\"den\":0}";

        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(Rational.PlusInfinity, deserialized);
    }

    [Fact]
    public void SerializeMinusInfinite()
    {
        Rational value = Rational.MinusInfinity;
        string expected = "{\"num\":-1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value, new RationalConverter());

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializeMinusInfinite()
    {
        string serialization = "{\"num\":-1,\"den\":0}";

        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(Rational.MinusInfinity, deserialized);
    }

    public static IEnumerable<object[]> SimplifiedSerializationCases()
    {
        var testCases = new (object value, string expected)[]
        {
            (new Rational(4, 1), "4"),
            (
                new []{new Rational(4, 1), new Rational(5, 1)},
                "[4, 5]"
            ),
            (
                new []{new Rational(4, 1), new Rational(5, 2)},
                "[4, {\"num\":5, \"den\":2}]"
            ),
            (
                new { a = new Rational(4, 1), b = new Rational(5, 2)},
                "{\"a\": 4, \"b\": {\"num\":5, \"den\":2} }"
            )
        };

        foreach (var (value, expected) in testCases)
        {
            yield return new object[] {value, expected.Replace(" ","")};
        }
    }

    [Theory]
    [MemberData(nameof(SimplifiedSerializationCases))]
    public void SimplifiedSerialization(object value, string expected)
    {
        var serialization = JsonConvert.SerializeObject(value, new RationalConverter());
        Assert.Equal(expected, serialization);
    }
}