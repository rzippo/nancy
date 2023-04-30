using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class MultiplicationTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FiniteByFinite()
    {
        Assert.Equal(new LongRational(3, 5), a * b);
    }

    [Fact]
    public void FiniteByInfinite()
    {
        Assert.Equal(LongRational.MinusInfinity, -a * LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, b * LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity * b);

        Assert.Equal(LongRational.MinusInfinity, a * LongRational.MinusInfinity);
        Assert.Equal(LongRational.PlusInfinity, -b * LongRational.MinusInfinity);
        Assert.Equal(LongRational.PlusInfinity, LongRational.MinusInfinity * -b);
    }

    [Fact]
    public void InfiniteByInfinite()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity * LongRational.PlusInfinity);
        Assert.Equal(LongRational.MinusInfinity, LongRational.PlusInfinity * LongRational.MinusInfinity);
        Assert.Equal(LongRational.PlusInfinity, LongRational.MinusInfinity * LongRational.MinusInfinity);
    }

    [Fact]
    public void FiniteByZero()
    {
        Assert.Equal(LongRational.Zero, a * LongRational.Zero);
    }

    [Fact]
    public void InfiniteByZero()
    {
        Assert.Equal(LongRational.Zero, LongRational.PlusInfinity * LongRational.Zero);
        Assert.Equal(LongRational.Zero, LongRational.MinusInfinity * LongRational.Zero);
    }

    [Fact]
    public void ZeroByZero()
    {
        Assert.Equal(LongRational.Zero, LongRational.Zero * LongRational.Zero);
    }
}