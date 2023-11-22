using System.Collections.Generic;
using System.Text.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class SystemJsonTests
{
    public static List<(int numerator, int denominator, string serialization)> SerializeFinitePairs = new()
    {
        (1, 2, "{\"num\":1,\"den\":2}"),
        (2, 1, "2"),
        (-1, 2, "{\"num\":-1,\"den\":2}"),
        (-2, 1, "-2"),
    };

    public static IEnumerable<object[]> CoherencyTestCases()
    {
        foreach (var (numerator, denominator, _) in SerializeFinitePairs)
        {
            yield return new object[] { new Rational(numerator, denominator) };
        }
    }

    [Theory]
    [MemberData(nameof(CoherencyTestCases))]
    public void Coherency(Rational rational)
    {
        string serialization = JsonSerializer.Serialize(rational);
        var deserialized = JsonSerializer.Deserialize<Rational>(serialization);

        Assert.Equal(rational, deserialized);
    }
    
    public static IEnumerable<object[]> SerializeFiniteTestCases()
    {
        foreach (var (numerator, denominator, serialization) in SerializeFinitePairs)
        {
            yield return new object[] { numerator, denominator, serialization };
        }
    }
    
    [Theory]
    [MemberData(nameof(SerializeFiniteTestCases))]
    public void SerializeFinite(int numerator, int denominator, string expected)
    {
        Rational value = new Rational(numerator, denominator);
        string serialization = JsonSerializer.Serialize(value);

        Assert.Equal(expected, serialization);
    }

    public static List<(int numerator, int denominator, string serialization)> DeserializeFinitePairs = new()
    {
        (1, 2, "{\"num\":1,\"den\":2}"),
        (2, 1, "{\"num\":2,\"den\":1}"),
        (2, 1, "2"),
        (-1, 2, "{\"num\":-1,\"den\":2}"),
        (-2, 1, "{\"num\":-2,\"den\":1}"),
        (-2, 1, "-2"),
    };

    public static IEnumerable<object[]> DeserializeFiniteTestCases()
    {
        foreach (var (numerator, denominator, serialization) in DeserializeFinitePairs)
        {
            yield return new object[] { numerator, denominator, serialization };
        }
    }
    
    [Theory]
    [MemberData(nameof(DeserializeFiniteTestCases))]
    public void DeserializeFinite(int expectedNum, int expectedDen, string serialization)
    {
        Rational deserialized = JsonSerializer.Deserialize<Rational>(serialization);

        Assert.True(deserialized.IsFinite);
        Assert.Equal(expectedNum, deserialized.Numerator);
        Assert.Equal(expectedDen, deserialized.Denominator);
    }

    [Fact]
    public void SerializePlusInfinite()
    {
        Rational value = Rational.PlusInfinity;
        string expected = "{\"num\":1,\"den\":0}";

        string serialization = JsonSerializer.Serialize(value);

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializePlusInfinite()
    {
        string serialization = "{\"num\":1,\"den\":0}";

        Rational deserialized = JsonSerializer.Deserialize<Rational>(serialization);

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(Rational.PlusInfinity, deserialized);
    }

    [Fact]
    public void SerializeMinusInfinite()
    {
        Rational value = Rational.MinusInfinity;
        string expected = "{\"num\":-1,\"den\":0}";

        string serialization = JsonSerializer.Serialize(value);

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializeMinusInfinite()
    {
        string serialization = "{\"num\":-1,\"den\":0}";

        Rational deserialized = JsonSerializer.Deserialize<Rational>(serialization);

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
        var serialization = JsonSerializer.Serialize(value);
        Assert.Equal(expected, serialization);
    }
}