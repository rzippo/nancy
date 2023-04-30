using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class OperatorsTests
{
    [Fact]
    public void Negation()
    {
        Assert.Equal(LongRational.MinusInfinity, -LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, -LongRational.MinusInfinity);
    }

    [Fact]
    public void Increment()
    {
        LongRational plusInfinity = LongRational.PlusInfinity;
        LongRational minusInfinity = LongRational.MinusInfinity;

        Assert.Equal(LongRational.PlusInfinity, ++plusInfinity);
        Assert.Equal(LongRational.MinusInfinity, ++minusInfinity);
    }

    [Fact]
    public void Decrement()
    {
        LongRational plusInfinity = LongRational.PlusInfinity;
        LongRational minusInfinity = LongRational.MinusInfinity;

        Assert.Equal(LongRational.PlusInfinity, --plusInfinity);
        Assert.Equal(LongRational.MinusInfinity, --minusInfinity);
    }

    [Fact]
    public void Equal()
    {
        Assert.True(LongRational.PlusInfinity == LongRational.PlusInfinity);
        Assert.True(LongRational.MinusInfinity == LongRational.MinusInfinity);

        Assert.False(LongRational.MinusInfinity == LongRational.PlusInfinity);

        Assert.False(LongRational.One == LongRational.MinusInfinity);
        Assert.False(LongRational.One == LongRational.PlusInfinity);
    }

    [Fact]
    public void Compare_Infinite()
    {
        Assert.True(LongRational.PlusInfinity >= LongRational.PlusInfinity);
        Assert.False(LongRational.PlusInfinity > LongRational.PlusInfinity);
        Assert.True(LongRational.PlusInfinity > LongRational.MinusInfinity);

        Assert.True(LongRational.MinusInfinity <= LongRational.MinusInfinity);
        Assert.False(LongRational.MinusInfinity < LongRational.MinusInfinity);
        Assert.True(LongRational.MinusInfinity < LongRational.PlusInfinity);
    }

    [Fact]
    public void Compare_FiniteWithInfinite()
    {
        Assert.True(LongRational.PlusInfinity > 2);
        Assert.True(LongRational.PlusInfinity > 0);
        Assert.True(LongRational.PlusInfinity > -2);

        Assert.True(LongRational.MinusInfinity < 2);
        Assert.True(LongRational.MinusInfinity < 0);
        Assert.True(LongRational.MinusInfinity < -2);
    }

    [Fact]
    public void Compare_One()
    {
        Assert.True(LongRational.PlusInfinity > LongRational.One);
        Assert.True(LongRational.MinusInfinity < LongRational.One);
    }

    [Fact]
    public void Compare_Zero()
    {
        Assert.True(LongRational.PlusInfinity > LongRational.Zero);
        Assert.True(LongRational.Zero < LongRational.PlusInfinity);
        Assert.True(LongRational.MinusInfinity < LongRational.Zero);
        Assert.True(LongRational.Zero > LongRational.MinusInfinity);
    }
}