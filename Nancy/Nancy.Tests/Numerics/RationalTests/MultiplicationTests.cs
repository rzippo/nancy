using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class MultiplicationTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FiniteByFinite()
    {
        Assert.Equal(new Rational(3, 5), a * b);
    }

    [Fact]
    public void FiniteByInfinite()
    {
        Assert.Equal(Rational.MinusInfinity, -a * Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, b * Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity * b);

        Assert.Equal(Rational.MinusInfinity, a * Rational.MinusInfinity);
        Assert.Equal(Rational.PlusInfinity, -b * Rational.MinusInfinity);
        Assert.Equal(Rational.PlusInfinity, Rational.MinusInfinity * -b);
    }

    [Fact]
    public void InfiniteByInfinite()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity * Rational.PlusInfinity);
        Assert.Equal(Rational.MinusInfinity, Rational.PlusInfinity * Rational.MinusInfinity);
        Assert.Equal(Rational.PlusInfinity, Rational.MinusInfinity * Rational.MinusInfinity);
    }

    [Fact]
    public void FiniteByZero()
    {
        Assert.Equal(Rational.Zero, a * Rational.Zero);
    }

    [Fact]
    public void InfiniteByZero()
    {
        Assert.Equal(Rational.Zero, Rational.PlusInfinity * Rational.Zero);
        Assert.Equal(Rational.Zero, Rational.MinusInfinity * Rational.Zero);
    }

    [Fact]
    public void ZeroByZero()
    {
        Assert.Equal(Rational.Zero, Rational.Zero * Rational.Zero);
    }
}