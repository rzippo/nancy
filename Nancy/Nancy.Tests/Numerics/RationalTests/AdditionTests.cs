using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class AdditionTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FinitePlusFinite()
    {
        Assert.Equal(new Rational(31, 20), Rational.Add(a, b));
    }

    [Fact]
    public void FinitePlusInfinite()
    {
        Assert.Equal(Rational.PlusInfinity, a + Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, b + Rational.PlusInfinity);
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity + b);
        Assert.Equal(Rational.MinusInfinity, a + Rational.MinusInfinity);
        Assert.Equal(Rational.MinusInfinity, b + Rational.MinusInfinity);
        Assert.Equal(Rational.MinusInfinity, Rational.MinusInfinity + b);
    }

    [Fact]
    public void InfinitePlusInfinite()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity + Rational.PlusInfinity);
        Assert.Equal(Rational.MinusInfinity, Rational.MinusInfinity + Rational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => Rational.MinusInfinity + Rational.PlusInfinity);
    }

    [Fact]
    public void FinitePlusZero()
    {
        Assert.Equal(a, a + Rational.Zero);
        Assert.Equal(b, Rational.Zero + b);
    }

    [Fact]
    public void InfinitePlusZero()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.PlusInfinity + Rational.Zero);
        Assert.Equal(Rational.MinusInfinity, Rational.Zero + Rational.MinusInfinity);
    }

    [Fact]
    public void ZeroPlusZero()
    {
        Assert.Equal(Rational.Zero, Rational.Zero + Rational.Zero);
    }
}