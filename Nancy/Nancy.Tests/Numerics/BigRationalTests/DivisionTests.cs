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

    [Fact]
    public void DivideByOne()
    {
        Assert.Equal(new BigRational(2, 3), new BigRational(2, 3) / BigRational.One);
        Assert.Equal(new BigRational(-2, 3), new BigRational(-2, 3) / BigRational.One);
    }

    [Fact]
    public void ReciprocalOfPositive()
    {
        Assert.Equal(new BigRational(3, 2), BigRational.One / new BigRational(2, 3));
    }

    [Fact]
    public void ReciprocalOfNegative()
    {
        Assert.Equal(new BigRational(-3, 2), BigRational.One / new BigRational(-2, 3));
    }

    /// <summary>
    /// The undetermined cases must say which one they are.
    /// A parameterless <see cref="UndeterminedResultException"/> reaches the caller as .NET's placeholder,
    /// which is a bare type name, and every consumer that reports a message reports that instead.
    /// </summary>
    [Fact]
    public void ZeroOverZero_SaysWhy()
    {
        var thrown = Assert.Throws<UndeterminedResultException>(() => BigRational.Zero / BigRational.Zero);
        Assert.Equal("Zero over zero", thrown.Message);
    }

    [Fact]
    public void InfiniteOverInfinite_SaysWhy()
    {
        BigRational[] infinities = [BigRational.PlusInfinity, BigRational.MinusInfinity];
        foreach (var x in infinities)
        foreach (var y in infinities)
        {
            var thrown = Assert.Throws<UndeterminedResultException>(() => x / y);
            Assert.Equal("Infinity over infinity", thrown.Message);
        }
    }

    /// <summary>
    /// <see cref="BigRational.Divide"/> forwards to the operator, so it carries whatever the operator throws.
    /// </summary>
    [Fact]
    public void NamedDivideSaysWhyToo()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => BigRational.Divide(BigRational.Zero, BigRational.Zero)).Message);
        Assert.Equal(
            "Infinity over infinity",
            Assert.Throws<UndeterminedResultException>(() => BigRational.Divide(BigRational.PlusInfinity, BigRational.PlusInfinity)).Message);
    }

    /// <summary>
    /// The constructor already worded its own zero-over-zero, and is the wording the division follows.
    /// </summary>
    [Fact]
    public void ConstructorSaysWhy()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => new BigRational(0, 0)).Message);
    }
}
