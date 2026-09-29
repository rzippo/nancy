using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class FromDecimal
{
    public static IEnumerable<object[]> GetDecimals()
    {
        var decimalTuples = new []
        {
            (128.3m, 1283, 10),
            (0.5m, 1, 2)
        };

        foreach (var tuple in decimalTuples)
            yield return new object[] { tuple.Item1, tuple.Item2, tuple.Item3 };
    }

    [Theory]
    [MemberData(nameof(GetDecimals))]
    public void DecimalCtorEquivalence(decimal d, int num, int den)
    {
        var r = new BigRational(d);

        Assert.Equal(num, r.Numerator);
        Assert.Equal(den, r.Denominator);
    }

    public static IEnumerable<object[]> GetHighDecimals()
    {
        var values = new List<decimal>
        {
            0.733333333333333m,
            0.7333333333333333m
        };

        foreach (var value in values)
        {
            yield return new object[] { value };
        }
    }
    
    [Theory]
    [MemberData(nameof(GetHighDecimals))]
    public void DecimalCtorNoException(decimal d)
    {
        var r = new BigRational(d);
    }

    /// <summary>
    /// Decimals whose significand words have the sign bit set, each with its exact value.
    /// </summary>
    public static List<(decimal d, BigRational expected)> DecimalsPastTheRangeOfALong =
    [
        // 2^63
        (9223372036854775808m, new BigRational(BigInteger.Pow(2, 63))),
        // -2^63
        (-9223372036854775808m, new BigRational(-BigInteger.Pow(2, 63))),
        // -2^63 - 1
        (-9223372036854775809m, new BigRational(-BigInteger.Pow(2, 63) - 1)),
        // (2^64 + 1) / 2
        (9223372036854775808.5m, new BigRational(BigInteger.Pow(2, 64) + 1, 2)),
        // 2^96 - 1
        (decimal.MaxValue, new BigRational(BigInteger.Pow(2, 96) - 1)),
        // 1 - 2^96
        (decimal.MinValue, new BigRational(1 - BigInteger.Pow(2, 96))),
    ];

    public static IEnumerable<object[]> GetDecimalsPastTheRangeOfALong()
        => DecimalsPastTheRangeOfALong.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetDecimalsPastTheRangeOfALong))]
    public void DecimalCtor_PastTheRangeOfALong(decimal d, BigRational expected)
    {
        Assert.Equal(expected, new BigRational(d));
    }
}