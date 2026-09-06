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

    /// <summary>
    /// The undetermined cases must say which one they are.
    /// A parameterless <see cref="UndeterminedResultException"/> reaches the caller as .NET's placeholder,
    /// which is a bare type name, and every consumer that reports a message reports that instead.
    /// </summary>
    [Fact]
    public void ZeroOverZero_SaysWhy()
    {
        var thrown = Assert.Throws<UndeterminedResultException>(() => Rational.Zero / Rational.Zero);
        Assert.Equal("Zero over zero", thrown.Message);
    }

    [Fact]
    public void InfiniteOverInfinite_SaysWhy()
    {
        Rational[] infinities = [Rational.PlusInfinity, Rational.MinusInfinity];
        foreach (var x in infinities)
        foreach (var y in infinities)
        {
            var thrown = Assert.Throws<UndeterminedResultException>(() => x / y);
            Assert.Equal("Infinity over infinity", thrown.Message);
        }
    }

    /// <summary>
    /// <see cref="Rational.Divide"/> forwards to the operator, so it carries whatever the operator throws.
    /// </summary>
    [Fact]
    public void NamedDivideSaysWhyToo()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => Rational.Divide(Rational.Zero, Rational.Zero)).Message);
        Assert.Equal(
            "Infinity over infinity",
            Assert.Throws<UndeterminedResultException>(() => Rational.Divide(Rational.PlusInfinity, Rational.PlusInfinity)).Message);
    }

    /// <summary>
    /// The constructor already worded its own zero-over-zero, and is the wording the division follows.
    /// </summary>
    [Fact]
    public void ConstructorSaysWhy()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => new Rational(0, 0)).Message);
    }
}
