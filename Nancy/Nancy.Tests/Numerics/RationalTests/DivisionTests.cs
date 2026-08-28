using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class DivisionTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FiniteOverFinite()
    {
        Assert.Equal(new Rational(16, 15), a / b);
    }

    [Fact]
    public void FiniteOverInfinite()
    {
        Assert.Equal(Rational.Zero, -a / Rational.PlusInfinity);
        Assert.Equal(Rational.Zero, b / Rational.PlusInfinity);
        Assert.Equal(Rational.Zero, a / Rational.MinusInfinity);
        Assert.Equal(Rational.Zero, -b / Rational.MinusInfinity);
    }

    [Fact]
    public void InfiniteOverInfinite()
    {
        Assert.Throws<UndeterminedResultException>(() => Rational.PlusInfinity / Rational.PlusInfinity);
        Assert.Throws<UndeterminedResultException>(() => Rational.PlusInfinity / Rational.MinusInfinity);
        Assert.Throws<UndeterminedResultException>(() => Rational.MinusInfinity / Rational.MinusInfinity);
    }

    [Fact]
    public void FiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => a / Rational.Zero);
    }

    [Fact]
    public void InfiniteOverZero()
    {
        Assert.Throws<DivideByZeroException>(() => Rational.PlusInfinity / Rational.Zero);
        Assert.Throws<DivideByZeroException>(() => Rational.MinusInfinity / Rational.Zero);
    }

    [Fact]
    public void ZeroOverFinite()
    {
        Assert.Equal(Rational.Zero, Rational.Zero / a);
        Assert.Equal(Rational.Zero, Rational.Zero / b);
    }

    [Fact]
    public void ZeroOverInfinite()
    {
        Assert.Equal(Rational.Zero, Rational.Zero / Rational.PlusInfinity);
        Assert.Equal(Rational.Zero, Rational.Zero / Rational.MinusInfinity);
    }

    [Fact]
    public void ZeroOverZero()
    {
        Assert.Throws<UndeterminedResultException>(() => Rational.Zero / Rational.Zero);
    }

    [Fact]
    public void DivideByOne()
    {
        Assert.Equal(new Rational(2, 3), new Rational(2, 3) / Rational.One);
        Assert.Equal(new Rational(-2, 3), new Rational(-2, 3) / Rational.One);
    }

    [Fact]
    public void ReciprocalOfPositive()
    {
        Assert.Equal(new Rational(3, 2), Rational.One / new Rational(2, 3));
    }

    [Fact]
    public void ReciprocalOfNegative()
    {
        Assert.Equal(new Rational(-3, 2), Rational.One / new Rational(-2, 3));
    }
}