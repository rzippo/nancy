using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class SubtractionTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FiniteMinusFinite()
    {
        Assert.Equal(new Rational(1, 20), a - b);
    }

    [Fact]
    public void FiniteMinusInfinite()
    {
        Assert.Equal(Rational.MinusInfinity, a - Rational.PlusInfinity);
        Assert.Equal(Rational.MinusInfinity, b - Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, a - Rational.MinusInfinity);
        Assert.Equal(Rational.PlusInfinity, b - Rational.MinusInfinity);
    }

    [Fact]
    public void InfiniteMinusInfinite()
    {
        Assert.Equal(Rational.MinusInfinity, Rational.MinusInfinity - Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity - Rational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => Rational.MinusInfinity - Rational.MinusInfinity);
    }

    [Fact]
    public void FiniteMinusZero()
    {
        Assert.Equal(a, a - Rational.Zero);
        Assert.Equal(-b, Rational.Zero - b);
    }

    [Fact]
    public void InfiniteMinusZero()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity - Rational.Zero);
        Assert.Equal(Rational.PlusInfinity, Rational.Zero - Rational.MinusInfinity);
        Assert.Equal(Rational.MinusInfinity, Rational.Zero - Rational.PlusInfinity);
    }

    [Fact]
    public void ZeroMinusZero()
    {
        Assert.Equal(Rational.Zero, Rational.Zero - Rational.Zero);
    }
}