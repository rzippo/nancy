using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class PrimitiveConversions
{
    public static List<(LongRational value, long integralPart, float singleValue, double doubleValue, decimal decimalValue)> ExplicitConversionCases =
    [
        (new LongRational(7, 2), 3, 3.5f, 3.5, 3.5m),
        (new LongRational(-7, 2), -3, -3.5f, -3.5, -3.5m)
    ];

    public static IEnumerable<object[]> GetExplicitConversionCases()
        => ExplicitConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitConversionCases))]
    public void ExplicitConversions_TruncateIntegerTargets(
        LongRational value,
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

    public static List<(LongRational value, ulong integralPart)> ExplicitUnsignedConversionCases =
    [
        (new LongRational(7, 2), 3)
    ];

    public static IEnumerable<object[]> GetExplicitUnsignedConversionCases()
        => ExplicitUnsignedConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitUnsignedConversionCases))]
    public void ExplicitUnsignedConversions_TruncateIntegerTargets(LongRational value, ulong integralPart)
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
    public void ImplicitIntegerConversions_CreateLongRationalValues(
        sbyte sbyteValue,
        ushort ushortValue,
        uint uintValue,
        ulong ulongValue,
        byte byteValue,
        short shortValue,
        int intValue,
        long longValue)
    {
        Assert.Equal(new LongRational(sbyteValue), (LongRational)sbyteValue);
        Assert.Equal(new LongRational(ushortValue), (LongRational)ushortValue);
        Assert.Equal(new LongRational(uintValue), (LongRational)uintValue);
        Assert.Equal(new LongRational((long)ulongValue), (LongRational)ulongValue);
        Assert.Equal(new LongRational(byteValue), (LongRational)byteValue);
        Assert.Equal(new LongRational(shortValue), (LongRational)shortValue);
        Assert.Equal(new LongRational(intValue), (LongRational)intValue);
        Assert.Equal(new LongRational(longValue), (LongRational)longValue);
    }

    public static List<BigInteger> ImplicitBigIntegerCases =
    [
        new BigInteger(11)
    ];

    public static IEnumerable<object[]> GetImplicitBigIntegerCases()
        => ImplicitBigIntegerCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitBigIntegerCases))]
    public void ImplicitBigIntegerConversion_CreatesLongRationalValue(BigInteger value)
    {
        Assert.Equal(new LongRational((long)value), (LongRational)value);
    }

    public static List<(float singleValue, LongRational singleExpected, double doubleValue, LongRational doubleExpected, decimal decimalValue, LongRational decimalExpected)> ImplicitFloatingConversionCases =
    [
        (1.25f, new LongRational(5, 4), 2.5, new LongRational(5, 2), 3.75m, new LongRational(15, 4))
    ];

    public static IEnumerable<object[]> GetImplicitFloatingConversionCases()
        => ImplicitFloatingConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitFloatingConversionCases))]
    public void ImplicitFloatingConversions_CreateLongRationalValues(
        float singleValue,
        LongRational singleExpected,
        double doubleValue,
        LongRational doubleExpected,
        decimal decimalValue,
        LongRational decimalExpected)
    {
        Assert.Equal(singleExpected, (LongRational)singleValue);
        Assert.Equal(doubleExpected, (LongRational)doubleValue);
        Assert.Equal(decimalExpected, (LongRational)decimalValue);
    }

    public static List<LongRational> InfiniteDecimalConversionCases =
    [
        LongRational.PlusInfinity,
        LongRational.MinusInfinity
    ];

    public static IEnumerable<object[]> GetInfiniteDecimalConversionCases()
        => InfiniteDecimalConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInfiniteDecimalConversionCases))]
    public void DecimalConversion_RejectsInfinities(LongRational value)
    {
        Assert.Throws<InvalidConversionException>(() => (decimal)value);
    }
}
