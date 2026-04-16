using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class FromDecimalEdgeCases
{
    [Fact]
    public void DecimalCtor_SimpleDecimals()
    {
        Assert.Equal(new LongRational(1, 2), new LongRational(0.5m));
        Assert.Equal(new LongRational(1283, 10), new LongRational(128.3m));
    }

    [Fact]
    public void DecimalCtor_WithTrailingZeros()
    {
        var r = new LongRational(100.0m);
        Assert.Equal(new LongRational(100), r);
    }

    [Fact]
    public void DecimalCtor_NegativeNumbers()
    {
        var r = new LongRational(-5.5m);
        Assert.Equal(new LongRational(-11, 2), r);
    }

    [Fact]
    public void DecimalCtor_IntegerPart()
    {
        var r = new LongRational(10.0m);
        Assert.Equal(new LongRational(10), r);
        Assert.True(r.IsInteger);
    }

    [Fact]
    public void DecimalCtor_Zero()
    {
        var r = new LongRational(0.0m);
        Assert.Equal(LongRational.Zero, r);
    }

    [Fact]
    public void DecimalCtor_LargeIntegerPart()
    {
        var r = new LongRational(1234567.0m);
        Assert.Equal(new LongRational(1234567), r);
    }

    [Fact]
    public void DecimalCtor_MultipleDecimalPlaces()
    {
        var r = new LongRational(1.25m);
        Assert.Equal(new LongRational(5, 4), r);
    }

    [Fact]
    public void DecimalCtor_NineDigits()
    {
        var r = new LongRational(0.123456789m);
        Assert.Equal(new LongRational(123456789, 1000000000), r);
    }

    [Fact]
    public void DecimalCtor_ThreeDigits()
    {
        var r = new LongRational(0.123m);
        Assert.Equal(new LongRational(123, 1000), r);
    }

    [Fact]
    public void DecimalCtor_OneDecimal()
    {
        var r = new LongRational(5.5m);
        Assert.Equal(new LongRational(11, 2), r);
    }

    [Fact]
    public void DecimalCtor_MoreThanNineDigits_TruncatesToNine()
    {
        var r = new LongRational(0.123456789m);
        Assert.Equal(new LongRational(123456789, 1000000000), r);
    }

    [Fact]
    public void DecimalCtor_TooManySignificantDigits_ThrowsOverflow()
    {
        Assert.Throws<OverflowException>(() => new LongRational(decimal.MaxValue));
    }

    [Fact]
    public void DecimalCtor_RoundtripPrecision()
    {
        var original = 3.141592653m;
        var r = new LongRational(original);
        var backToDecimal = (decimal)r;
        
        Assert.Equal(3.141592653m, backToDecimal);
    }

    [Fact]
    public void DecimalCtor_SmallFraction()
    {
        var r = new LongRational(0.000000001m);
        Assert.Equal(new LongRational(1, 1000000000), r);
    }

    [Fact]
    public void DecimalCtor_ComplexNumber()
    {
        var r = new LongRational(123.456m);
        Assert.Equal(new LongRational(123456, 1000), r);
    }
}
