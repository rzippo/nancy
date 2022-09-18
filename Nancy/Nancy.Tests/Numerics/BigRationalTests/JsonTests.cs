using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class JsonTests
{
    [Fact]
    public void SerializeFinite()
    {
        BigRational value = new BigRational(1, 2);
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
        BigRational deserialized = JsonConvert.DeserializeObject<BigRational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsFinite);
        Assert.Equal(expectedNum, deserialized.Numerator);
        Assert.Equal(expectedDen, deserialized.Denominator);
    }

    [Fact]
    public void SerializePlusInfinite()
    {
        BigRational value = BigRational.PlusInfinity;
        string expected = "{\"num\":1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value, new RationalConverter());

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializePlusInfinite()
    {
        string serialization = "{\"num\":1,\"den\":0}";

        BigRational deserialized = JsonConvert.DeserializeObject<BigRational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(BigRational.PlusInfinity, deserialized);
    }

    [Fact]
    public void SerializeMinusInfinite()
    {
        BigRational value = BigRational.MinusInfinity;
        string expected = "{\"num\":-1,\"den\":0}";

        string serialization = JsonConvert.SerializeObject(value, new RationalConverter());

        Assert.Equal(expected, serialization);
    }

    [Fact]
    public void DeserializeMinusInfinite()
    {
        string serialization = "{\"num\":-1,\"den\":0}";

        BigRational deserialized = JsonConvert.DeserializeObject<BigRational>(serialization, new RationalConverter());

        Assert.True(deserialized.IsInfinite);
        Assert.Equal(BigRational.MinusInfinity, deserialized);
    }
}