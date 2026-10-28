using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class PropertyContracts
{
    [Fact]
    public void IsZeroRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsZero());

    [Fact]
    public void IsZeroValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 0), new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsZero());
    }

    [Fact]
    public void IsInfiniteRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsInfinite());

    [Fact]
    public void IsInfiniteValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, Rational.PlusInfinity), new Point(0, Rational.PlusInfinity)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsInfinite());
    }

    [Fact]
    public void HasPlusInfinityRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasPlusInfinity());

    [Fact]
    public void HasPlusInfinityValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 0), new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).HasPlusInfinity());
    }

    [Fact]
    public void HasMinusInfinityRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().HasMinusInfinity());

    [Fact]
    public void HasMinusInfinityValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 0), new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).HasMinusInfinity());
    }

    [Fact]
    public void IsFiniteRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsFinite());

    [Fact]
    public void IsFiniteValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 0), new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsFinite());
    }

    [Fact]
    public void IsMinusInfiniteRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsMinusInfinite());

    [Fact]
    public void IsMinusInfiniteValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, Rational.MinusInfinity), new Point(0, Rational.MinusInfinity)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsMinusInfinite());
    }

    [Fact]
    public void IsPlusInfiniteRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsPlusInfinite());

    [Fact]
    public void IsPlusInfiniteValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, Rational.PlusInfinity), new Point(0, Rational.PlusInfinity)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsPlusInfinite());
    }

    [Fact]
    public void IsNonPositiveRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonPositive());

    [Fact]
    public void IsNonPositiveValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, -1), new Point(0, -1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsNonPositive());
    }

    [Fact]
    public void IsPositiveRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsPositive());

    [Fact]
    public void IsPositiveValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsPositive());
    }

    [Fact]
    public void IsNegativeRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNegative());

    [Fact]
    public void IsNegativeValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, -1), new Point(0, -1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsNegative());
    }

    [Fact]
    public void IsNonNegativeRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonNegative());

    [Fact]
    public void IsNonNegativeValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsNonNegative());
    }

    [Fact]
    public void IsConstantRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsConstant());

    [Fact]
    public void IsConstantValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsConstant());
    }

    [Fact]
    public void IsAffineRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsAffine());

    [Fact]
    public void IsAffineValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsAffine());
    }

    [Fact]
    public void IsNonDecreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonDecreasing());

    [Fact]
    public void IsNonDecreasingValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsNonDecreasing());
    }

    [Fact]
    public void IsIncreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsIncreasing());

    [Fact]
    public void IsIncreasingValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsIncreasing());
    }

    [Fact]
    public void IsNonIncreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsNonIncreasing());

    [Fact]
    public void IsNonIncreasingValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsNonIncreasing());
    }

    [Fact]
    public void IsDecreasingRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsDecreasing());

    [Fact]
    public void IsDecreasingValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsDecreasing());
    }

    [Fact]
    public void IsContinuousRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsContinuous());

    [Fact]
    public void IsContinuousValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsContinuous());
    }

    [Fact]
    public void IsLeftContinuousRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsLeftContinuous());

    [Fact]
    public void IsLeftContinuousValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsLeftContinuous());
    }

    [Fact]
    public void IsRightContinuousRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().IsRightContinuous());

    [Fact]
    public void IsRightContinuousValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).IsRightContinuous());
    }

    [Fact]
    public void SupValueRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValue());

    [Fact]
    public void SupValueValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).SupValue());
    }

    [Fact]
    public void InfValueRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfValue());

    [Fact]
    public void InfValueValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).InfValue());
    }

    [Fact]
    public void SupArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupArg());

    [Fact]
    public void SupArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).SupArg());
    }

    [Fact]
    public void InfArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfArg());

    [Fact]
    public void InfArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).InfArg());
    }

    [Fact]
    public void MaxValueRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MaxValue());

    [Fact]
    public void MaxValueValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).MaxValue());
    }

    [Fact]
    public void MinValueRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MinValue());

    [Fact]
    public void MinValueValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).MinValue());
    }

    [Fact]
    public void MaxArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MaxArg());

    [Fact]
    public void MaxArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).MaxArg());
    }

    [Fact]
    public void MinArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().MinArg());

    [Fact]
    public void MinArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).MinArg());
    }

    [Fact]
    public void SupValueAndArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().SupValueAndArg());

    [Fact]
    public void SupValueAndArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).SupValueAndArg());
    }

    [Fact]
    public void InfValueAndArgRejectsEmptyInput()
        => Assert.Throws<ArgumentException>(() => Array.Empty<Element>().InfValueAndArg());

    [Fact]
    public void InfValueAndArgValidatesTheElementItConsumes()
    {
        Element[] input = [new Point(0, 1), new Point(0, 1)];
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(input).InfValueAndArg());
    }

}
