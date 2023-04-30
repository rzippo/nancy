using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class MultiplicationTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FiniteByFinite()
    {
        Assert.Equal(new BigRational(3, 5), a * b);
    }

    [Fact]
    public void FiniteByInfinite()
    {
        Assert.Equal(BigRational.MinusInfinity, -a * BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, b * BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity * b);

        Assert.Equal(BigRational.MinusInfinity, a * BigRational.MinusInfinity);
        Assert.Equal(BigRational.PlusInfinity, -b * BigRational.MinusInfinity);
        Assert.Equal(BigRational.PlusInfinity, BigRational.MinusInfinity * -b);
    }

    [Fact]
    public void InfiniteByInfinite()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.PlusInfinity * BigRational.PlusInfinity);
        Assert.Equal(BigRational.MinusInfinity, BigRational.PlusInfinity * BigRational.MinusInfinity);
        Assert.Equal(BigRational.PlusInfinity, BigRational.MinusInfinity * BigRational.MinusInfinity);
    }

    [Fact]
    public void FiniteByZero()
    {
        Assert.Equal(BigRational.Zero, a * BigRational.Zero);
    }

    [Fact]
    public void InfiniteByZero()
    {
        Assert.Equal(BigRational.Zero, BigRational.PlusInfinity * BigRational.Zero);
        Assert.Equal(BigRational.Zero, BigRational.MinusInfinity * BigRational.Zero);
    }

    [Fact]
    public void ZeroByZero()
    {
        Assert.Equal(BigRational.Zero, BigRational.Zero * BigRational.Zero);
    }
}