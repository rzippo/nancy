using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class OperatorsTests
{
    [Fact]
    public void Negation()
    {
        Assert.Equal(BigRational.MinusInfinity, -BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, -BigRational.MinusInfinity);
    }

    [Fact]
    public void Increment()
    {
        BigRational plusInfinity = BigRational.PlusInfinity;
        BigRational minusInfinity = BigRational.MinusInfinity;

        Assert.Equal(BigRational.PlusInfinity, ++plusInfinity);
        Assert.Equal(BigRational.MinusInfinity, ++minusInfinity);
    }

    [Fact]
    public void Decrement()
    {
        BigRational plusInfinity = BigRational.PlusInfinity;
        BigRational minusInfinity = BigRational.MinusInfinity;

        Assert.Equal(BigRational.PlusInfinity, --plusInfinity);
        Assert.Equal(BigRational.MinusInfinity, --minusInfinity);
    }

    [Fact]
    public void Equal()
    {
        Assert.True(BigRational.PlusInfinity == BigRational.PlusInfinity);
        Assert.True(BigRational.MinusInfinity == BigRational.MinusInfinity);

        Assert.False(BigRational.MinusInfinity == BigRational.PlusInfinity);

        Assert.False(BigRational.One == BigRational.MinusInfinity);
        Assert.False(BigRational.One == BigRational.PlusInfinity);
    }

    [Fact]
    public void CompareFinite()
    {
        //this test proves that BigRational(double) is unreliable

        BigRational a = new BigRational(12, 5);
        BigRational b_double = new BigRational(2.5);
        BigRational b_decimal = new BigRational(2.5M);

        Assert.True(b_decimal > a);
        Assert.False(b_double > a);
    }

    [Fact]
    public void Compare_Infinite()
    {
        Assert.True(BigRational.PlusInfinity >= BigRational.PlusInfinity);
        Assert.False(BigRational.PlusInfinity > BigRational.PlusInfinity);
        Assert.True(BigRational.PlusInfinity > BigRational.MinusInfinity);

        Assert.True(BigRational.MinusInfinity <= BigRational.MinusInfinity);
        Assert.False(BigRational.MinusInfinity < BigRational.MinusInfinity);
        Assert.True(BigRational.MinusInfinity < BigRational.PlusInfinity);
    }

    [Fact]
    public void Compare_FiniteWithInfinite()
    {
        Assert.True(BigRational.PlusInfinity > 2);
        Assert.True(BigRational.PlusInfinity > 0);
        Assert.True(BigRational.PlusInfinity > -2);

        Assert.True(BigRational.MinusInfinity < 2);
        Assert.True(BigRational.MinusInfinity < 0);
        Assert.True(BigRational.MinusInfinity < -2);
    }

    [Fact]
    public void Compare_One()
    {
        Assert.True(BigRational.PlusInfinity > BigRational.One);
        Assert.True(BigRational.MinusInfinity < BigRational.One);
    }

    [Fact]
    public void Compare_Zero()
    {
        Assert.True(BigRational.PlusInfinity > BigRational.Zero);
        Assert.True(BigRational.Zero < BigRational.PlusInfinity);
        Assert.True(BigRational.MinusInfinity < BigRational.Zero);
        Assert.True(BigRational.Zero > BigRational.MinusInfinity);
    }
}