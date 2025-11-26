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

    [Fact]
    public void Equal()
    {
        Assert.True(Rational.PlusInfinity == Rational.PlusInfinity);
        Assert.True(Rational.MinusInfinity == Rational.MinusInfinity);

        Assert.False(Rational.MinusInfinity == Rational.PlusInfinity);

        Assert.False(Rational.One == Rational.MinusInfinity);
        Assert.False(Rational.One == Rational.PlusInfinity);
    }
}