using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class PrimitiveConversions
{
    #if BIG_RATIONAL
    public static List<(BigInteger numerator, BigInteger denominator, Rational expected)> BigIntegerConstructorCases =
    [
        (0, -5, Rational.Zero),
        (6, -8, new Rational(-3, 4)),
        (-6, -8, new Rational(3, 4)),
        (5, 0, Rational.PlusInfinity),
        (-5, 0, Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> GetBigIntegerConstructorCases()
        => BigIntegerConstructorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBigIntegerConstructorCases))]
    public void BigIntegerConstructor_NormalizesSpecialValues(
        BigInteger numerator,
        BigInteger denominator,
        Rational expected)
    {
        Assert.Equal(expected, new Rational(numerator, denominator));
    }

    public static List<(BigInteger whole, BigInteger numerator, BigInteger denominator, Rational expected)> MixedNumberConstructorCases =
    [
        (1, 1, 2, new Rational(3, 2)),
        (-1, 1, 2, new Rational(-1, 2)),
        (1, 1, -2, new Rational(-3, 2)),
        (0, 0, 7, Rational.Zero)
    ];

    public static IEnumerable<object[]> GetMixedNumberConstructorCases()
        => MixedNumberConstructorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetMixedNumberConstructorCases))]
    public void MixedNumberConstructor_NormalizesParts(
        BigInteger whole,
        BigInteger numerator,
        BigInteger denominator,
        Rational expected)
    {
        Assert.Equal(expected, new Rational(whole, numerator, denominator));
    }

    public static List<(BigInteger whole, BigInteger numerator, BigInteger denominator)> InvalidMixedNumberConstructorCases =
    [
        (1, 1, 0)
    ];

    public static IEnumerable<object[]> GetInvalidMixedNumberConstructorCases()
        => InvalidMixedNumberConstructorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInvalidMixedNumberConstructorCases))]
    public void MixedNumberConstructor_RejectsZeroDenominator(
        BigInteger whole,
        BigInteger numerator,
        BigInteger denominator)
    {
        Assert.Throws<DivideByZeroException>(() => new Rational(whole, numerator, denominator));
    }
    #endif

    public static List<(Rational value, long integralPart, float singleValue, double doubleValue, decimal decimalValue)> ExplicitConversionCases =
    [
        (new Rational(7, 2), 3, 3.5f, 3.5, 3.5m),
        (new Rational(-7, 2), -3, -3.5f, -3.5, -3.5m)
    ];

    public static IEnumerable<object[]> GetExplicitConversionCases()
        => ExplicitConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitConversionCases))]
    public void ExplicitConversions_TruncateIntegerTargets(
        Rational value,
        long integralPart,
        float singleValue,
        double doubleValue,
        decimal decimalValue)
    {
        Assert.Equal((sbyte)integralPart, (sbyte)value);
        Assert.Equal((short)integralPart, (short)value);
        Assert.Equal((int)integralPart, (int)value);
        Assert.Equal(integralPart, (long)value);
        Assert.Equal(new BigInteger(integralPart), (BigInteger)value);
        Assert.Equal(singleValue, (float)value);
        Assert.Equal(doubleValue, (double)value);
        Assert.Equal(decimalValue, (decimal)value);
    }

    public static List<(Rational value, ulong integralPart)> ExplicitUnsignedConversionCases =
    [
        (new Rational(7, 2), 3)
    ];

    public static IEnumerable<object[]> GetExplicitUnsignedConversionCases()
        => ExplicitUnsignedConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitUnsignedConversionCases))]
    public void ExplicitUnsignedConversions_TruncateIntegerTargets(Rational value, ulong integralPart)
    {
        Assert.Equal((byte)integralPart, (byte)value);
        Assert.Equal((ushort)integralPart, (ushort)value);
        Assert.Equal((uint)integralPart, (uint)value);
        Assert.Equal(integralPart, (ulong)value);
    }

    public static List<(sbyte sbyteValue, ushort ushortValue, uint uintValue, ulong ulongValue, byte byteValue, short shortValue, int intValue, long longValue)> ImplicitConversionCases =
    [
        (-3, 4, 5, 6, 7, -8, -9, 10)
    ];

    public static IEnumerable<object[]> GetImplicitConversionCases()
        => ImplicitConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitConversionCases))]
    public void ImplicitIntegerConversions_CreateRationalValues(
        sbyte sbyteValue,
        ushort ushortValue,
        uint uintValue,
        ulong ulongValue,
        byte byteValue,
        short shortValue,
        int intValue,
        long longValue)
    {
        Assert.Equal(new Rational(sbyteValue), (Rational)sbyteValue);
        Assert.Equal(new Rational(ushortValue), (Rational)ushortValue);
        Assert.Equal(new Rational(uintValue), (Rational)uintValue);
        Assert.Equal(new Rational((long)ulongValue), (Rational)ulongValue);
        Assert.Equal(new Rational(byteValue), (Rational)byteValue);
        Assert.Equal(new Rational(shortValue), (Rational)shortValue);
        Assert.Equal(new Rational(intValue), (Rational)intValue);
        Assert.Equal(new Rational(longValue), (Rational)longValue);
    }

    #if BIG_RATIONAL
    public static List<BigInteger> ImplicitBigIntegerCases =
    [
        new BigInteger(11)
    ];

    public static IEnumerable<object[]> GetImplicitBigIntegerCases()
        => ImplicitBigIntegerCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitBigIntegerCases))]
    public void ImplicitBigIntegerConversion_CreatesRationalValue(BigInteger value)
    {
        Assert.Equal(new Rational(value), (Rational)value);
    }
    #endif

    public static List<(float singleValue, Rational singleExpected, double doubleValue, Rational doubleExpected, decimal decimalValue, Rational decimalExpected)> ImplicitFloatingConversionCases =
    [
        (1.25f, new Rational(5, 4), 2.5, new Rational(5, 2), 3.75m, new Rational(15, 4))
    ];

    public static IEnumerable<object[]> GetImplicitFloatingConversionCases()
        => ImplicitFloatingConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitFloatingConversionCases))]
    public void ImplicitFloatingConversions_CreateRationalValues(
        float singleValue,
        Rational singleExpected,
        double doubleValue,
        Rational doubleExpected,
        decimal decimalValue,
        Rational decimalExpected)
    {
        Assert.Equal(singleExpected, (Rational)singleValue);
        Assert.Equal(doubleExpected, (Rational)doubleValue);
        Assert.Equal(decimalExpected, (Rational)decimalValue);
    }

    public static List<Rational> InfiniteDecimalConversionCases =
    [
        Rational.PlusInfinity,
        Rational.MinusInfinity
    ];

    public static IEnumerable<object[]> GetInfiniteDecimalConversionCases()
        => InfiniteDecimalConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInfiniteDecimalConversionCases))]
    public void DecimalConversion_RejectsInfinities(Rational value)
    {
        Assert.Throws<InvalidConversionException>(() => (decimal)value);
    }
}
