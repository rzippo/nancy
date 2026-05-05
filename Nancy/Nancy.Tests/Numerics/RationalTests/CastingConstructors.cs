#if BIG_RATIONAL || LONG_RATIONAL
using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class CastingConstructors
{
    public static List<(double value, Rational expected)> KnownDoubleValues =
    [
        (0.0, Rational.Zero),
        (-0.0, Rational.Zero),
        (0.5, new Rational(1, 2)),
        (-0.5, new Rational(-1, 2)),
        (2.5, new Rational(5, 2)),
        (-2.5, new Rational(-5, 2)),
        (0.1, new Rational(1, 10)),
        (1.0 / 3.0, new Rational(3_333_333_333_333_333, 10_000_000_000_000_000)),
        (1e-6, new Rational(1, 1_000_000)),
        (1e16, new Rational(10_000_000_000_000_000)),
        // here is a good reason not to use floating point:
        (0.1 + 0.2, new Rational(7500000000000001, 25000000000000000)),
    ];

    public static IEnumerable<object[]> GetKnownDoubleTestCases()
        => KnownDoubleValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownDoubleTestCases))]
    public void DoubleConstructor(double value, Rational expected)
    {
        var rational = new Rational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (double) rational);
    }

    public static List<(float value, Rational expected)> KnownFloatValues =
    [
        (0.0f, Rational.Zero),
        (-0.0f, Rational.Zero),
        (0.5f, new Rational(1, 2)),
        (-0.5f, new Rational(-1, 2)),
        (2.5f, new Rational(5, 2)),
        (-2.5f, new Rational(-5, 2)),
        (0.1f, new Rational(1, 10)),
        (1.0f / 3.0f, new Rational(16_666_667, 50_000_000)),
        (1e-6f, new Rational(1, 1_000_000)),
        (1e10f, new Rational(10_000_000_000)),
        (0.1f + 0.2f, new Rational(3, 10)),
    ];

    public static IEnumerable<object[]> GetKnownFloatTestCases()
        => KnownFloatValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownFloatTestCases))]
    public void FloatConstructor(float value, Rational expected)
    {
        var rational = new Rational(value);

        Assert.Equal(expected, rational);
        Assert.Equal(value, (float) rational);
    }

    [Fact]
    public void DoubleConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new Rational(double.NaN));
    }

    [Fact]
    public void FloatConstructorNaN()
    {
        Assert.Throws<ArgumentException>(() => new Rational(float.NaN));
    }

    [Fact]
    public void DoubleConstructorInfinity()
    {
        Assert.Equal(Rational.PlusInfinity, new Rational(double.PositiveInfinity));
        Assert.Equal(Rational.MinusInfinity, new Rational(double.NegativeInfinity));
    }

    [Fact]
    public void FloatConstructorInfinity()
    {
        Assert.Equal(Rational.PlusInfinity, new Rational(float.PositiveInfinity));
        Assert.Equal(Rational.MinusInfinity, new Rational(float.NegativeInfinity));
    }
}
#endif
