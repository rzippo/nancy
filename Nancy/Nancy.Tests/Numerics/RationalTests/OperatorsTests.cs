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

    [Fact]
    public void Compare_Infinite()
    {
        Assert.True(Rational.PlusInfinity >= Rational.PlusInfinity);
        Assert.False(Rational.PlusInfinity > Rational.PlusInfinity);
        Assert.True(Rational.PlusInfinity > Rational.MinusInfinity);

        Assert.True(Rational.MinusInfinity <= Rational.MinusInfinity);
        Assert.False(Rational.MinusInfinity < Rational.MinusInfinity);
        Assert.True(Rational.MinusInfinity < Rational.PlusInfinity);
    }

    [Fact]
    public void Compare_FiniteWithInfinite()
    {
        Assert.True(Rational.PlusInfinity > 2);
        Assert.True(Rational.PlusInfinity > 0);
        Assert.True(Rational.PlusInfinity > -2);

        Assert.True(Rational.MinusInfinity < 2);
        Assert.True(Rational.MinusInfinity < 0);
        Assert.True(Rational.MinusInfinity < -2);
    }

    [Fact]
    public void Compare_One()
    {
        Assert.True(Rational.PlusInfinity > Rational.One);
        Assert.True(Rational.MinusInfinity < Rational.One);
    }

    [Fact]
    public void Compare_Zero()
    {
        Assert.True(Rational.PlusInfinity > Rational.Zero);
        Assert.True(Rational.Zero < Rational.PlusInfinity);
        Assert.True(Rational.MinusInfinity < Rational.Zero);
        Assert.True(Rational.Zero > Rational.MinusInfinity);
    }
}