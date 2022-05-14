using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class AdditionTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FinitePlusFinite()
    {
        Assert.Equal(new LongRational(31, 20), LongRational.Add(a, b));
    }

    [Fact]
    public void FinitePlusInfinite()
    {
        Assert.Equal(LongRational.PlusInfinity, a + LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, b + LongRational.PlusInfinity);
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity + b);
        Assert.Equal(LongRational.MinusInfinity, a + LongRational.MinusInfinity);
        Assert.Equal(LongRational.MinusInfinity, b + LongRational.MinusInfinity);
        Assert.Equal(LongRational.MinusInfinity, LongRational.MinusInfinity + b);
    }

    [Fact]
    public void InfinitePlusInfinite()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity + LongRational.PlusInfinity);
        Assert.Equal(LongRational.MinusInfinity, LongRational.MinusInfinity + LongRational.MinusInfinity);

        Assert.Throws<UndeterminedResultException>(() => LongRational.MinusInfinity + LongRational.PlusInfinity);
    }

    [Fact]
    public void FinitePlusZero()
    {
        Assert.Equal(a, a + LongRational.Zero);
        Assert.Equal(b, LongRational.Zero + b);
    }

    [Fact]
    public void InfinitePlusZero()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.PlusInfinity + LongRational.Zero);
        Assert.Equal(LongRational.MinusInfinity, LongRational.Zero + LongRational.MinusInfinity);
    }

    [Fact]
    public void ZeroPlusZero()
    {
        Assert.Equal(LongRational.Zero, LongRational.Zero + LongRational.Zero);
    }
}