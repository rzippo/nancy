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

    /// <summary>
    /// The undetermined cases must say which one they are.
    /// A parameterless <see cref="UndeterminedResultException"/> reaches the caller as .NET's placeholder,
    /// which is a bare type name, and every consumer that reports a message reports that instead.
    /// </summary>
    [Fact]
    public void ZeroOverZero_SaysWhy()
    {
        var thrown = Assert.Throws<UndeterminedResultException>(() => LongRational.Zero / LongRational.Zero);
        Assert.Equal("Zero over zero", thrown.Message);
    }

    [Fact]
    public void InfiniteOverInfinite_SaysWhy()
    {
        LongRational[] infinities = [LongRational.PlusInfinity, LongRational.MinusInfinity];
        foreach (var x in infinities)
        foreach (var y in infinities)
        {
            var thrown = Assert.Throws<UndeterminedResultException>(() => x / y);
            Assert.Equal("Infinity over infinity", thrown.Message);
        }
    }

    /// <summary>
    /// <see cref="LongRational.Divide"/> forwards to the operator, so it carries whatever the operator throws.
    /// </summary>
    [Fact]
    public void NamedDivideSaysWhyToo()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => LongRational.Divide(LongRational.Zero, LongRational.Zero)).Message);
        Assert.Equal(
            "Infinity over infinity",
            Assert.Throws<UndeterminedResultException>(() => LongRational.Divide(LongRational.PlusInfinity, LongRational.PlusInfinity)).Message);
    }

    /// <summary>
    /// The constructor already worded its own zero-over-zero, and is the wording the division follows.
    /// </summary>
    [Fact]
    public void ConstructorSaysWhy()
    {
        Assert.Equal(
            "Zero over zero",
            Assert.Throws<UndeterminedResultException>(() => new LongRational(0, 0)).Message);
    }
}
