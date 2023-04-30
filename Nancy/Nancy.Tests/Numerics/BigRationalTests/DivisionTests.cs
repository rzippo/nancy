using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class DivisionTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FiniteOverFinite()
    {
        Assert.Equal(new BigRational(16, 15), a / b);
    }

    [Fact]
    public void FiniteOverInfinite()
    {
        Assert.Equal(BigRational.Zero, -a / BigRational.PlusInfinity);
        Assert.Equal(BigRational.Zero, b / BigRational.PlusInfinity);
        Assert.Equal(BigRational.Zero, a / BigRational.MinusInfinity);
        Assert.Equal(BigRational.Zero, -b / BigRational.MinusInfinity);
    }

    [Fact]
    public void InfiniteOverInfinite()
    {
        Assert.Throws<UndeterminedResultException>(() => BigRational.PlusInfinity / BigRational.PlusInfinity);
        Assert.Throws<UndeterminedResultException>(() => BigRational.PlusInfinity / BigRational.MinusInfinity);
        Assert.Throws<UndeterminedResultException>(() => BigRational.MinusInfinity / BigRational.MinusInfinity);
    }

    [Fact]
    public void FiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => a / BigRational.Zero);
    }

    [Fact]
    public void InfiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => BigRational.PlusInfinity / BigRational.Zero);
        Assert.Throws<DivideByZeroException>(() => BigRational.MinusInfinity / BigRational.Zero);
    }

    [Fact]
    public void ZeroOverFinite()
    {
        Assert.Equal(BigRational.Zero, BigRational.Zero / a);
        Assert.Equal(BigRational.Zero, BigRational.Zero / b);
    }

    [Fact]
    public void ZeroOverInfinite()
    {
        Assert.Equal(BigRational.Zero, BigRational.Zero / BigRational.PlusInfinity);
        Assert.Equal(BigRational.Zero, BigRational.Zero / BigRational.MinusInfinity);
    }

    [Fact]
    public void ZeroOverZero()
    {
        Assert.Throws<UndeterminedResultException>(() => BigRational.Zero / BigRational.Zero);
    }
}