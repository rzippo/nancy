using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class FromDecimalEdgeCases
{
    #if BIG_RATIONAL
    public static IEnumerable<object[]> GetDecimalsWithExpectedValues()
    {
        var decimalTuples = new[]
        {
            (128.3m, 1283, 10),
            (0.5m, 1, 2),
            (27m/150, 9, 50),
            (144m/400, 9, 25),
            (0.733333333333333m, 733_333_333_333_333, 1_000_000_000_000_000),
            (0.7333333333333333m, 7_333_333_333_333_333, 10_000_000_000_000_000),
        };

        foreach (var tuple in decimalTuples)
            yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
    }

    [Theory]
    [MemberData(nameof(GetDecimalsWithExpectedValues))]
    public void DecimalCtor_KnownDecimals(decimal d, long num, long den)
    {
        var r = new Rational(d);

        Assert.Equal(num, r.Numerator);
        Assert.Equal(den, r.Denominator);
    }
    #endif

    [Fact]
    public void DecimalCtor_Zero()
    {
        var r = new Rational(0m);
        Assert.Equal(Rational.Zero, r);
    }

    [Fact]
    public void DecimalCtor_Integer()
    {
        var r = new Rational(42m);
        Assert.Equal(42, r.Numerator);
        Assert.Equal(1, r.Denominator);
    }

    #if BIG_RATIONAL
    [Fact]
    public void DecimalCtor_Negative()
    {
        var r = new Rational(-5.5m);
        Assert.True(r < 0);
        Assert.Equal(new Rational(11, 2), Rational.Abs(r));
    }

    [Fact]
    public void DecimalCtor_Precision()
    {
        var original = 3.141592653m;
        var r = new Rational(original);
        var backToDecimal = (decimal)r;

        Assert.Equal(original, backToDecimal);
    }

    [Fact]
    public void DecimalCtor_SmallFraction()
    {
        var r = new Rational(0.000000001m);
        Assert.Equal(new Rational(1, 1000000000), r);
    }

    [Fact]
    public void DecimalCtor_MaxPrecision()
    {
        var original = 0.1234567890123456789012345678m;
        var r = new Rational(original);
        var backToDecimal = (decimal)r;

        Assert.Equal(original, backToDecimal);
    }

    [Fact]
    public void DecimalCtor_Roundtrip()
    {
        var decimals = new[]
        {
            0.1m, 0.2m, 0.3m, 0.4m, 0.5m, 0.6m, 0.7m, 0.8m, 0.9m,
            1.0m, 1.1m, 1.5m, 2.0m, 10.5m, 100.25m,
            0.01m, 0.001m, 0.0001m,
            -0.5m, -1.5m, -100.25m
        };

        foreach (var d in decimals)
        {
            var r = new Rational(d);
            var backToDecimal = (decimal)r;
            Assert.Equal(d, backToDecimal);
        }
    }

    [Fact]
    public void DecimalCtor_LargeInteger()
    {
        var r = new Rational(12345678901234567890m);
        Assert.True(r.IsInteger);
    }
    #endif

    [Fact]
    public void DecimalCtor_Simplification()
    {
        var r = new Rational(0.500m);
        Assert.Equal(new Rational(1, 2), r);
    }
}
