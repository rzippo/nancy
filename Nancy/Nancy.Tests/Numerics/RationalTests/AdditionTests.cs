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

    [Fact]
    public void SameDenominator()
    {
        Assert.Equal(new Rational(1, 1), new Rational(1, 2) + new Rational(1, 2));
    }

    [Fact]
    public void LcdSharedFactors()
    {
        Assert.Equal(new Rational(5, 12), new Rational(1, 4) + new Rational(1, 6));
    }

    [Fact]
    public void LcdNegativeNumerator()
    {
        Assert.Equal(new Rational(-1, 6), new Rational(-1, 3) + new Rational(1, 6));
    }

    [Fact]
    public void LcdDenominatorMultiple()
    {
        Assert.Equal(new Rational(1, 2), new Rational(1, 3) + new Rational(1, 6));
    }

    [Fact]
    public void LcdSecondLevelGcd()
    {
        Assert.Equal(new Rational(1, 2), new Rational(2, 8) + new Rational(3, 12));
    }

    [Fact]
    public void LcdLargeSharedFactor()
    {
        Assert.Equal(new Rational(11, 24), new Rational(3, 8) + new Rational(1, 12));
    }

    [Fact]
    public void CoPrimeDenominators()
    {
        Assert.Equal(new Rational(8, 15), new Rational(1, 3) + new Rational(1, 5));
    }
}