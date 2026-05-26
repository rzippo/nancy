using System.Collections.Generic;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class OperatorsTests
{
    [Fact]
    public void Negation()
    {
        Assert.Equal(Rational.MinusInfinity, -Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, -Rational.MinusInfinity);
    }

    [Fact]
    public void Increment()
    {
        Rational plusInfinity = Rational.PlusInfinity;
        Rational minusInfinity = Rational.MinusInfinity;

        Assert.Equal(Rational.PlusInfinity, ++plusInfinity);
        Assert.Equal(Rational.MinusInfinity, ++minusInfinity);
    }

    [Fact]
    public void Decrement()
    {
        Rational plusInfinity = Rational.PlusInfinity;
        Rational minusInfinity = Rational.MinusInfinity;

        Assert.Equal(Rational.PlusInfinity, --plusInfinity);
        Assert.Equal(Rational.MinusInfinity, --minusInfinity);
    }

    public static List<(Rational a, Rational b, bool areEqual)> EqualityCases =
    [
        (JsonConvert.DeserializeObject<Rational>("{\"num\":2,\"den\":4}"), new Rational(1, 2), true),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":3,\"den\":9}"), new Rational(1, 3), true),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":6,\"den\":4}"), new Rational(3, 2), true),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":-2,\"den\":4}"), new Rational(-1, 2), true),
        (JsonConvert.DeserializeObject<Rational>("{\"num\":0,\"den\":5}"), Rational.Zero, true),
        (new Rational(1, 2), new Rational(1, 2), true),
        (new Rational(1, 2), new Rational(3, 4), false),
        (new Rational(2, 3), new Rational(5, 7), false),
        (new Rational(-3, 4), new Rational(3, 4), false)
    ];

    public static IEnumerable<object[]> GetEqualityCases()
        => EqualityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetEqualityCases))]
    public void Equality(Rational a, Rational b, bool areEqual)
    {
        Assert.Equal(areEqual, a == b);
        Assert.NotEqual(areEqual, a != b);
    }
}