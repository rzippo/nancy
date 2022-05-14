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
}