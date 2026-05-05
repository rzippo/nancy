using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class CastingConstructors
{
    public static List<(double value, BigRational expected)> KnownDoubleValues =
    [
        (0.0, BigRational.Zero),
        (-0.0, BigRational.Zero),
        (0.5, new BigRational(1, 2)),
        (-0.5, new BigRational(-1, 2)),
        (2.5, new BigRational(5, 2)),
        (-2.5, new BigRational(-5, 2)),
        (0.1, new BigRational(1, 10)),
        (1.0 / 3.0, new BigRational(3_333_333_333_333_333, 10_000_000_000_000_000)),
        (1e-6, new BigRational(1, 1_000_000)),
        (1e16, new BigRational(10_000_000_000_000_000))
    ];

    public static IEnumerable<object[]> GetKnownDoubleTestCases()
        => KnownDoubleValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownDoubleTestCases))]
    public void DoubleConstructor(double value, BigRational expected)
    {
        var rational = new BigRational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (double) rational);
    }

    public static List<(float value, BigRational expected)> KnownFloatValues =
    [
        (0.0f, BigRational.Zero),
        (-0.0f, BigRational.Zero),
        (0.5f, new BigRational(1, 2)),
        (-0.5f, new BigRational(-1, 2)),
        (2.5f, new BigRational(5, 2)),
        (-2.5f, new BigRational(-5, 2)),
        (0.1f, new BigRational(1, 10)),
        (1.0f / 3.0f, new BigRational(16_666_667, 50_000_000)),
        (1e-6f, new BigRational(1, 1_000_000)),
        (1e10f, new BigRational(10_000_000_000))
    ];

    public static IEnumerable<object[]> GetKnownFloatTestCases()
        => KnownFloatValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownFloatTestCases))]
    public void FloatConstructor(float value, BigRational expected)
    {
        var rational = new BigRational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (float) rational);
    }

    [Fact]
    public void DoubleConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new BigRational(double.NaN));
    }

    [Fact]
    public void FloatConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new BigRational(float.NaN));
    }

    [Fact]
    public void DoubleConstructorInfinity()
    {
        Assert.Equal(BigRational.PlusInfinity, new BigRational(double.PositiveInfinity));
        Assert.Equal(BigRational.MinusInfinity, new BigRational(double.NegativeInfinity));
    }

    [Fact]
    public void FloatConstructorInfinity()
    {
        Assert.Equal(BigRational.PlusInfinity, new BigRational(float.PositiveInfinity));
        Assert.Equal(BigRational.MinusInfinity, new BigRational(float.NegativeInfinity));
    }
}
