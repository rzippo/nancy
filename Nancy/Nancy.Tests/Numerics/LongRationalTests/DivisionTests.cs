using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class DivisionTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FiniteOverFinite()
    {
        Assert.Equal(new LongRational(16, 15), a / b);
    }

    [Fact]
    public void FiniteOverInfinite()
    {
        Assert.Equal(LongRational.Zero, -a / LongRational.PlusInfinity);
        Assert.Equal(LongRational.Zero, b / LongRational.PlusInfinity);
        Assert.Equal(LongRational.Zero, a / LongRational.MinusInfinity);
        Assert.Equal(LongRational.Zero, -b / LongRational.MinusInfinity);
    }

    [Fact]
    public void InfiniteOverInfinite()
    {
        Assert.Throws<UndeterminedResultException>(() => LongRational.PlusInfinity / LongRational.PlusInfinity);
        Assert.Throws<UndeterminedResultException>(() => LongRational.PlusInfinity / LongRational.MinusInfinity);
        Assert.Throws<UndeterminedResultException>(() => LongRational.MinusInfinity / LongRational.MinusInfinity);
    }

    [Fact]
    public void FiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => a / LongRational.Zero);
    }

    [Fact]
    public void InfiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => LongRational.PlusInfinity / LongRational.Zero);
        Assert.Throws<DivideByZeroException>(() => LongRational.MinusInfinity / LongRational.Zero);
    }

    [Fact]
    public void ZeroOverFinite()
    {
        Assert.Equal(LongRational.Zero, LongRational.Zero / a);
        Assert.Equal(LongRational.Zero, LongRational.Zero / b);
    }

    [Fact]
    public void ZeroOverInfinite()
    {
        Assert.Equal(LongRational.Zero, LongRational.Zero / LongRational.PlusInfinity);
        Assert.Equal(LongRational.Zero, LongRational.Zero / LongRational.MinusInfinity);
    }

    [Fact]
    public void ZeroOverZero()
    {
        Assert.Throws<UndeterminedResultException>(() => LongRational.Zero / LongRational.Zero);
    }
}