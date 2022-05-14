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

        string serialization = JsonConvert.SerializeObject(value);

        Assert.Equal(expected, serialization);
    }

    [Theory]
    [InlineData("{\"num\":1,\"den\":2}", 1, 2)]
    [InlineData("{\"num\":2,\"den\":1}", 2, 1)]
    [InlineData("2", 2, 1)]
    public void DeserializeFinite(string serialization, int expectedNum, int expectedDen)
    {
        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization);

        Assert.True(deserialized.IsFinite);
        Assert.Equal(expectedNum, deserialized.Numerator);
        Assert.Equal(expectedDen, deserialized.Denominator);
    }

    [Fact]
    public void SerializePlusInfinite()
    {
        Rational value = Rational.PlusInfinity;
        string expected = "{\"num\":1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value);

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializePlusInfinite()
    {
        string serialization = "{\"num\":1,\"den\":0}";

        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization);

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(Rational.PlusInfinity, deserialized);
    }

    [Fact]
    public void SerializeMinusInfinite()
    {
        Rational value = Rational.MinusInfinity;
        string expected = "{\"num\":-1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value);

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializeMinusInfinite()
    {
        string serialization = "{\"num\":-1,\"den\":0}";

        Rational deserialized = JsonConvert.DeserializeObject<Rational>(serialization);

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(Rational.MinusInfinity, deserialized);
    }
}