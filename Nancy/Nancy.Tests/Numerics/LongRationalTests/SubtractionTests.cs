using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class SubtractionTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FiniteMinusFinite()
    {
        Assert.Equal(new LongRational(1, 20), a - b);
    }

    [Fact]
    public void FiniteMinusInfinite()
    {
        Assert.Equal(LongRational.MinusInfinity, a - LongRational.PlusInfinity);
        Assert.Equal(LongRational.MinusInfinity, b - LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, a - LongRational.MinusInfinity);
        Assert.Equal(LongRational.PlusInfinity, b - LongRational.MinusInfinity);
    }

    [Fact]
    public void InfiniteMinusInfinite()
    {
        Assert.Equal(LongRational.MinusInfinity, LongRational.MinusInfinity - LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity - LongRational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => LongRational.MinusInfinity - LongRational.MinusInfinity);
    }

    [Fact]
    public void FiniteMinusZero()
    {
        Assert.Equal(a, a - LongRational.Zero);
        Assert.Equal(-b, LongRational.Zero - b);
    }

    [Fact]
    public void InfiniteMinusZero()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity - LongRational.Zero);
        Assert.Equal(LongRational.PlusInfinity, LongRational.Zero - LongRational.MinusInfinity);
        Assert.Equal(LongRational.MinusInfinity, LongRational.Zero - LongRational.PlusInfinity);
    }

    [Fact]
    public void ZeroMinusZero()
    {
        Assert.Equal(LongRational.Zero, LongRational.Zero - LongRational.Zero);
    }
}