using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class AdditionTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FinitePlusFinite()
    {
        Assert.Equal(new BigRational(31, 20), BigRational.Add(a, b));
    }

    [Fact]
    public void FinitePlusInfinite()
    {
        Assert.Equal(BigRational.PlusInfinity, a + BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, b + BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity + b);
        Assert.Equal(BigRational.MinusInfinity, a + BigRational.MinusInfinity);
        Assert.Equal(BigRational.MinusInfinity, b + BigRational.MinusInfinity);
        Assert.Equal(BigRational.MinusInfinity, BigRational.MinusInfinity + b);
    }

    [Fact]
    public void InfinitePlusInfinite()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity + BigRational.PlusInfinity);
        Assert.Equal(BigRational.MinusInfinity, BigRational.MinusInfinity + BigRational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => BigRational.MinusInfinity + BigRational.PlusInfinity);
    }

    [Fact]
    public void FinitePlusZero()
    {
        Assert.Equal(a, a + BigRational.Zero);
        Assert.Equal(b, BigRational.Zero + b);
    }

    [Fact]
    public void InfinitePlusZero()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity + BigRational.Zero);
        Assert.Equal(BigRational.MinusInfinity, BigRational.Zero + BigRational.MinusInfinity);
    }

    [Fact]
    public void ZeroPlusZero()
    {
        Assert.Equal(BigRational.Zero, BigRational.Zero + BigRational.Zero);
    }

    [Fact]
    public void SameDenominator()
    {
        Assert.Equal(new BigRational(1, 1), new BigRational(1, 2) + new BigRational(1, 2));
    }

    [Fact]
    public void LcdSharedFactors()
    {
        Assert.Equal(new BigRational(5, 12), new BigRational(1, 4) + new BigRational(1, 6));
    }

    [Fact]
    public void LcdNegativeNumerator()
    {
        Assert.Equal(new BigRational(-1, 6), new BigRational(-1, 3) + new BigRational(1, 6));
    }

    [Fact]
    public void LcdDenominatorMultiple()
    {
        Assert.Equal(new BigRational(1, 2), new BigRational(1, 3) + new BigRational(1, 6));
    }

    [Fact]
    public void LcdSecondLevelGcd()
    {
        Assert.Equal(new BigRational(1, 2), new BigRational(2, 8) + new BigRational(3, 12));
    }

    [Fact]
    public void LcdLargeSharedFactor()
    {
        Assert.Equal(new BigRational(11, 24), new BigRational(3, 8) + new BigRational(1, 12));
    }

    [Fact]
    public void CoPrimeDenominators()
    {
        Assert.Equal(new BigRational(8, 15), new BigRational(1, 3) + new BigRational(1, 5));
    }
}