using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class PrimitiveConversions
{
    public static List<(BigRational value, long integralPart, float singleValue, double doubleValue, decimal decimalValue)> ExplicitConversionCases =
    [
        (new BigRational(7, 2), 3, 3.5f, 3.5, 3.5m),
        (new BigRational(-7, 2), -3, -3.5f, -3.5, -3.5m)
    ];

    public static IEnumerable<object[]> GetExplicitConversionCases()
        => ExplicitConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitConversionCases))]
    public void ExplicitConversions_TruncateIntegerTargets(
        BigRational value,
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

    public static List<(BigRational value, ulong integralPart)> ExplicitUnsignedConversionCases =
    [
        (new BigRational(7, 2), 3)
    ];

    public static IEnumerable<object[]> GetExplicitUnsignedConversionCases()
        => ExplicitUnsignedConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetExplicitUnsignedConversionCases))]
    public void ExplicitUnsignedConversions_TruncateIntegerTargets(BigRational value, ulong integralPart)
    {
        Assert.Equal((byte)integralPart, (byte)value);
        Assert.Equal((ushort)integralPart, (ushort)value);
        Assert.Equal((uint)integralPart, (uint)value);
        Assert.Equal(integralPart, (ulong)value);
    }

    public static List<(sbyte sbyteValue, ushort ushortValue, uint uintValue, ulong ulongValue, byte byteValue, short shortValue, int intValue, long longValue, BigInteger bigIntegerValue)> ImplicitConversionCases =
    [
        (-3, 4, 5, 6, 7, -8, -9, 10, new BigInteger(11))
    ];

    public static IEnumerable<object[]> GetImplicitConversionCases()
        => ImplicitConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitConversionCases))]
    public void ImplicitIntegerConversions_CreateBigRationalValues(
        sbyte sbyteValue,
        ushort ushortValue,
        uint uintValue,
        ulong ulongValue,
        byte byteValue,
        short shortValue,
        int intValue,
        long longValue,
        BigInteger bigIntegerValue)
    {
        Assert.Equal(new BigRational(sbyteValue), (BigRational)sbyteValue);
        Assert.Equal(new BigRational(ushortValue), (BigRational)ushortValue);
        Assert.Equal(new BigRational(uintValue), (BigRational)uintValue);
        Assert.Equal(new BigRational(new BigInteger(ulongValue)), (BigRational)ulongValue);
        Assert.Equal(new BigRational(byteValue), (BigRational)byteValue);
        Assert.Equal(new BigRational(shortValue), (BigRational)shortValue);
        Assert.Equal(new BigRational(intValue), (BigRational)intValue);
        Assert.Equal(new BigRational(longValue), (BigRational)longValue);
        Assert.Equal(new BigRational(bigIntegerValue), (BigRational)bigIntegerValue);
    }

    public static List<(float singleValue, BigRational singleExpected, double doubleValue, BigRational doubleExpected, decimal decimalValue, BigRational decimalExpected)> ImplicitFloatingConversionCases =
    [
        (1.25f, new BigRational(5, 4), 2.5, new BigRational(5, 2), 3.75m, new BigRational(15, 4))
    ];

    public static IEnumerable<object[]> GetImplicitFloatingConversionCases()
        => ImplicitFloatingConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetImplicitFloatingConversionCases))]
    public void ImplicitFloatingConversions_CreateBigRationalValues(
        float singleValue,
        BigRational singleExpected,
        double doubleValue,
        BigRational doubleExpected,
        decimal decimalValue,
        BigRational decimalExpected)
    {
        Assert.Equal(singleExpected, (BigRational)singleValue);
        Assert.Equal(doubleExpected, (BigRational)doubleValue);
        Assert.Equal(decimalExpected, (BigRational)decimalValue);
    }

    public static List<BigRational> InfiniteDecimalConversionCases =
    [
        BigRational.PlusInfinity,
        BigRational.MinusInfinity
    ];

    public static IEnumerable<object[]> GetInfiniteDecimalConversionCases()
        => InfiniteDecimalConversionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetInfiniteDecimalConversionCases))]
    public void DecimalConversion_RejectsInfinities(BigRational value)
    {
        Assert.Throws<InvalidConversionException>(() => (decimal)value);
    }
}
