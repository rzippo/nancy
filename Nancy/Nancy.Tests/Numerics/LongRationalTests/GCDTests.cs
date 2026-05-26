using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class GCDTests
{
    public static List<(LongRational a, LongRational b, LongRational expected)> GcdCases =
    [
        (new LongRational(84), new LongRational(18), new LongRational(6)),
        (new LongRational(18), new LongRational(84), new LongRational(6)),
        (new LongRational(12), new LongRational(8), new LongRational(4)),
        (new LongRational(8), new LongRational(12), new LongRational(4)),
    ];

    public static IEnumerable<object[]> GetGcdCases()
        => GcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetGcdCases))]
    public void Rational(LongRational a, LongRational b, LongRational expected)
    {
        var gcd = Nancy.Numerics.LongRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }

    public static List<(long a, long b, long expected)> GcdLongCases =
    [
        (84, 18, 6),
        (18, 84, 6),
        (12, 8, 4),
        (8, 12, 4),
    ];

    public static IEnumerable<object[]> GetGcdLongCases()
        => GcdLongCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetGcdLongCases))]
    public void Long(long a, long b, long expected)
    {
        long gcd = Nancy.Numerics.LongRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }

    public static List<(LongRational a, LongRational b, LongRational expected)> NegativeGcdCases =
    [
        (new LongRational(-84), new LongRational(18), new LongRational(6)),
        (new LongRational(84), new LongRational(-18), new LongRational(6)),
        (new LongRational(-84), new LongRational(-18), new LongRational(6)),
        (new LongRational(-12), new LongRational(8), new LongRational(4)),
        (new LongRational(12), new LongRational(-8), new LongRational(4)),
        (new LongRational(-12), new LongRational(-8), new LongRational(4)),
        (new LongRational(-4), new LongRational(6), new LongRational(2)),
        (new LongRational(4), new LongRational(-6), new LongRational(2)),
        (new LongRational(-4), new LongRational(-6), new LongRational(2)),
        (new LongRational(-2, 3), new LongRational(4, 5), new LongRational(2, 15)),
        (new LongRational(2, 3), new LongRational(-4, 5), new LongRational(2, 15)),
        (new LongRational(-2, 3), new LongRational(-4, 5), new LongRational(2, 15)),
        (new LongRational(-1, 2), new LongRational(3, 4), new LongRational(1, 4)),
        (new LongRational(7, 10), new LongRational(-3, 5), new LongRational(1, 10)),
        (new LongRational(-7, 10), new LongRational(-3, 5), new LongRational(1, 10)),
    ];

    public static IEnumerable<object[]> GetNegativeGcdCases()
        => NegativeGcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeGcdCases))]
    public void Gcd_WithNegativeInputs_ReturnsPositive(LongRational a, LongRational b, LongRational expected)
    {
        var gcd = Nancy.Numerics.LongRational.GreatestCommonDivisor(a, b);
        Assert.True(gcd >= 0);
        Assert.Equal(expected, gcd);
    }

    public static List<(LongRational a, LongRational b)> NegativeLcmCases =
    [
        (new LongRational(-4), new LongRational(6)),
        (new LongRational(4), new LongRational(-6)),
        (new LongRational(-4), new LongRational(-6)),
        (new LongRational(-2, 3), new LongRational(4, 5)),
        (new LongRational(2, 3), new LongRational(-4, 5)),
        (new LongRational(-2, 3), new LongRational(-4, 5)),
    ];

    public static IEnumerable<object[]> GetNegativeLcmCases()
        => NegativeLcmCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeLcmCases))]
    public void Lcm_WithNegativeInputs_ReturnsNonNegative(LongRational a, LongRational b)
    {
        var lcm = Nancy.Numerics.LongRational.LeastCommonMultiple(a, b);
        Assert.True(lcm >= 0);
    }
}
