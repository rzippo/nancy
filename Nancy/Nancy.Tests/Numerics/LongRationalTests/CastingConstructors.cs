using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class CastingConstructors
{
    public static List<(double value, LongRational expected)> KnownDoubleValues =
    [
        (0.0, LongRational.Zero),
        (-0.0, LongRational.Zero),
        (0.5, new LongRational(1, 2)),
        (-0.5, new LongRational(-1, 2)),
        (2.5, new LongRational(5, 2)),
        (-2.5, new LongRational(-5, 2)),
        (0.1, new LongRational(1, 10)),
        (1.0 / 3.0, new LongRational(3_333_333_333_333_333, 10_000_000_000_000_000)),
        (1e-6, new LongRational(1, 1_000_000)),
        (1e16, new LongRational(10_000_000_000_000_000))
    ];

    public static IEnumerable<object[]> GetKnownDoubleTestCases()
        => KnownDoubleValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownDoubleTestCases))]
    public void DoubleConstructor(double value, LongRational expected)
    {
        var rational = new LongRational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (double) rational);
    }

    public static List<(float value, LongRational expected)> KnownFloatValues =
    [
        (0.0f, LongRational.Zero),
        (-0.0f, LongRational.Zero),
        (0.5f, new LongRational(1, 2)),
        (-0.5f, new LongRational(-1, 2)),
        (2.5f, new LongRational(5, 2)),
        (-2.5f, new LongRational(-5, 2)),
        (0.1f, new LongRational(1, 10)),
        (1.0f / 3.0f, new LongRational(16_666_667, 50_000_000)),
        (1e-6f, new LongRational(1, 1_000_000)),
        (1e10f, new LongRational(10_000_000_000))
    ];

    public static IEnumerable<object[]> GetKnownFloatTestCases()
        => KnownFloatValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownFloatTestCases))]
    public void FloatConstructor(float value, LongRational expected)
    {
        var rational = new LongRational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (float) rational);
    }

    [Fact]
    public void DoubleConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new LongRational(double.NaN));
    }

    [Fact]
    public void FloatConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new LongRational(float.NaN));
    }

    [Fact]
    public void DoubleConstructorInfinity()
    {
        Assert.Equal(LongRational.PlusInfinity, new LongRational(double.PositiveInfinity));
        Assert.Equal(LongRational.MinusInfinity, new LongRational(double.NegativeInfinity));
    }

    [Fact]
    public void FloatConstructorInfinity()
    {
        Assert.Equal(LongRational.PlusInfinity, new LongRational(float.PositiveInfinity));
        Assert.Equal(LongRational.MinusInfinity, new LongRational(float.NegativeInfinity));
    }
}
