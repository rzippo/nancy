using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.UncheckedInternals;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class FloatingPointExtensions
{
    [Fact]
    public void DoubleInfinity()
    {
        var (num, den) = double.PositiveInfinity.GetRationalParts();
        Assert.Equal(BigInteger.One, num);
        Assert.Equal(BigInteger.Zero, den);

        var (num2, den2) = double.NegativeInfinity.GetRationalParts();
        Assert.Equal(BigInteger.MinusOne, num2);
        Assert.Equal(BigInteger.Zero, den2);
    }

    [Fact]
    public void FloatInfinity()
    {
        var (num, den) = float.PositiveInfinity.GetRationalParts();
        Assert.Equal(BigInteger.One, num);
        Assert.Equal(BigInteger.Zero, den);

        var (num2, den2) = float.NegativeInfinity.GetRationalParts();
        Assert.Equal(BigInteger.MinusOne, num2);
        Assert.Equal(BigInteger.Zero, den2);
    }

    [Fact]
    public void DoubleNaN()
    {
        Assert.Throws<ArgumentException>(() => double.NaN.GetRationalParts());
    }

    [Fact]
    public void FloatNaN()
    {
        Assert.Throws<ArgumentException>(() => float.NaN.GetRationalParts());
    }

    public static List<(double value, BigInteger expectedNum, BigInteger expectedDen)> DoubleDirectValues =
    [
        (0.0, 0, 1),
        (128.3, 1283, 10),
        (1e-6, 1, 1_000_000),
        (1e16, 10_000_000_000_000_000, 1),
    ];

    public static IEnumerable<object[]> GetDoubleDirectValues()
        => DoubleDirectValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDoubleDirectValues))]
    public void DoubleDirectValue(double value, BigInteger expectedNum, BigInteger expectedDen)
    {
        var (num, den) = value.GetRationalParts();
        Assert.Equal(expectedNum, num);
        Assert.Equal(expectedDen, den);
    }

    public static List<(float value, BigInteger expectedNum, BigInteger expectedDen)> FloatDirectValues =
    [
        (0.0f, 0, 1),
        (128.3f, 1283, 10),
        (1e-6f, 1, 1_000_000),
    ];

    public static IEnumerable<object[]> GetFloatDirectValues()
        => FloatDirectValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFloatDirectValues))]
    public void FloatDirectValue(float value, BigInteger expectedNum, BigInteger expectedDen)
    {
        var (num, den) = value.GetRationalParts();
        Assert.Equal(expectedNum, num);
        Assert.Equal(expectedDen, den);
    }

    #if BIG_RATIONAL
    public static List<double> DoubleRoundtripValues =
    [
        0.1, 0.2, 0.5, 1.5, 3.141592653589793, 100.25, -0.5, 1e-10, 1e10
    ];

    public static IEnumerable<object[]> GetDoubleRoundtripValues()
        => DoubleRoundtripValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDoubleRoundtripValues))]
    public void DoubleRoundtrip(double d)
    {
        var (num, den) = d.GetRationalParts();
        var r = new Rational(num, den);
        var back = (double)r;
        Assert.Equal(d, back);
    }

    public static List<float> FloatRoundtripValues =
    [
        0.1f, 0.2f, 0.5f, 1.5f, 3.14159f, 100.25f, -0.5f, 1e-6f, 1e5f
    ];

    public static IEnumerable<object[]> GetFloatRoundtripValues()
        => FloatRoundtripValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFloatRoundtripValues))]
    public void FloatRoundtrip(float f)
    {
        var (num, den) = f.GetRationalParts();
        var r = new Rational(num, den);
        var back = (float)r;
        Assert.Equal(f, back);
    }

    public static List<(double value, Rational expected)> DoubleKnownValues =
    [
        (0.0, Rational.Zero),
        (0.5, new Rational(1, 2)),
        (-0.5, new Rational(-1, 2)),
        (2.5, new Rational(5, 2)),
        (0.1, new Rational(1, 10)),
        (128.3, new Rational(1283, 10)),
        (1e-6, new Rational(1, 1_000_000)),
        (1e16, new Rational(10_000_000_000_000_000)),
    ];

    public static IEnumerable<object[]> GetDoubleKnownValues()
        => DoubleKnownValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDoubleKnownValues))]
    public void DoubleGetRationalParts_ProducesCorrectRational(double value, Rational expected)
    {
        var (num, den) = value.GetRationalParts();
        var rational = new Rational(num, den);
        Assert.Equal(expected, rational);
    }

    public static List<(float value, Rational expected)> FloatKnownValues =
    [
        (0.0f, Rational.Zero),
        (0.5f, new Rational(1, 2)),
        (-0.5f, new Rational(-1, 2)),
        (2.5f, new Rational(5, 2)),
        (0.1f, new Rational(1, 10)),
        (128.3f, new Rational(1283, 10)),
        (1e-6f, new Rational(1, 1_000_000)),
    ];

    public static IEnumerable<object[]> GetFloatKnownValues()
        => FloatKnownValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetFloatKnownValues))]
    public void FloatGetRationalParts_ProducesCorrectRational(float value, Rational expected)
    {
        var (num, den) = value.GetRationalParts();
        var rational = new Rational(num, den);
        Assert.Equal(expected, rational);
    }
    #endif
}

#if BIG_RATIONAL
public class DecimalExtensionsTests
{
    public static List<(decimal value, Rational expected)> DecimalKnownValues =
    [
        (0m, Rational.Zero),
        (1m, Rational.One),
        (-1m, new Rational(-1)),
        (0.5m, new Rational(1, 2)),
        (-0.5m, new Rational(-1, 2)),
        (128.3m, new Rational(1283, 10)),
        (0.1m, new Rational(1, 10)),
        (42m, new Rational(42)),
        (0.000000001m, new Rational(1, 1_000_000_000)),
    ];

    public static IEnumerable<object[]> GetDecimalKnownValues()
        => DecimalKnownValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDecimalKnownValues))]
    public void DecimalGetRationalParts_ProducesCorrectRational(decimal value, Rational expected)
    {
        var (num, den) = value.GetRationalParts();
        var rational = new Rational(num, den);
        Assert.Equal(expected, rational);
    }

    [Fact]
    public void DecimalZero()
    {
        var (num, den) = 0m.GetRationalParts();
        Assert.Equal(BigInteger.Zero, num);
        Assert.Equal(BigInteger.One, den);
    }

    public static List<decimal> DecimalRoundtripValues =
    [
        0.1m, 0.2m, 0.3m, 0.5m, 1.5m, 100.25m,
        0.01m, 0.001m, 0.0001m,
        -0.5m, -1.5m, -100.25m,
        3.141592653589793m,
    ];

    public static IEnumerable<object[]> GetDecimalRoundtripValues()
        => DecimalRoundtripValues.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDecimalRoundtripValues))]
    public void DecimalRoundtrip(decimal d)
    {
        var (num, den) = d.GetRationalParts();
        var r = new Rational(num, den);
        var back = (decimal)r;
        Assert.Equal(d, back);
    }

    [Fact]
    public void DecimalLargeInteger()
    {
        var d = 12345678901234567890m;
        var (num, den) = d.GetRationalParts();
        var r = new Rational(num, den);
        Assert.True(r.IsInteger);
        var back = (decimal)r;
        Assert.Equal(d, back);
    }

    [Fact]
    public void DecimalMaxPrecision()
    {
        var original = 0.1234567890123456789012345678m;
        var (num, den) = original.GetRationalParts();
        var r = new Rational(num, den);
        var back = (decimal)r;
        Assert.Equal(original, back);
    }

    [Fact]
    public void DecimalNegative()
    {
        var (num, den) = (-5.5m).GetRationalParts();
        Assert.True(num < 0);
        Assert.Equal(new BigInteger(55), BigInteger.Abs(num));
        Assert.Equal(new BigInteger(10), den);
    }
}
#endif
