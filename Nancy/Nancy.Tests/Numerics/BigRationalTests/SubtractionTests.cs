using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class SubtractionTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FiniteMinusFinite()
    {
        Assert.Equal(new BigRational(1, 20), a - b);
    }

    [Fact]
    public void FiniteMinusInfinite()
    {
        Assert.Equal(BigRational.MinusInfinity, a - BigRational.PlusInfinity);
        Assert.Equal(BigRational.MinusInfinity, b - BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, a - BigRational.MinusInfinity);
        Assert.Equal(BigRational.PlusInfinity, b - BigRational.MinusInfinity);
    }

    [Fact]
    public void InfiniteMinusInfinite()
    {
        Assert.Equal(BigRational.MinusInfinity, BigRational.MinusInfinity - BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity - BigRational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => BigRational.MinusInfinity - BigRational.MinusInfinity);
    }

    [Fact]
    public void FiniteMinusZero()
    {
        Assert.Equal(a, a - BigRational.Zero);
        Assert.Equal(-b, BigRational.Zero - b);
    }

    [Fact]
    public void InfiniteMinusZero()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity - BigRational.Zero);
        Assert.Equal(BigRational.PlusInfinity, BigRational.Zero - BigRational.MinusInfinity);
        Assert.Equal(BigRational.MinusInfinity, BigRational.Zero - BigRational.PlusInfinity);
    }

    [Fact]
    public void ZeroMinusZero()
    {
        Assert.Equal(BigRational.Zero, BigRational.Zero - BigRational.Zero);
    }
}