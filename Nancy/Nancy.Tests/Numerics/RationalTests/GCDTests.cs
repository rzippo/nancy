using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class GCDTests
{
    public static List<(Rational a, Rational b, Rational expected)> GcdCases =
    [
        (new Rational(84), new Rational(18), new Rational(6)),
        (new Rational(18), new Rational(84), new Rational(6)),
        (new Rational(12), new Rational(8), new Rational(4)),
        (new Rational(8), new Rational(12), new Rational(4)),
    ];

    public static IEnumerable<object[]> GetGcdCases()
        => GcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetGcdCases))]
    public void Rational(Rational a, Rational b, Rational expected)
    {
        var gcd = Nancy.Numerics.Rational.GreatestCommonDivisor(a, b);
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
        long gcd = Nancy.Numerics.Rational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }

    public static List<(Rational a, Rational b, Rational expected)> NegativeGcdCases =
    [
        (new Rational(-84), new Rational(18), new Rational(6)),
        (new Rational(84), new Rational(-18), new Rational(6)),
        (new Rational(-84), new Rational(-18), new Rational(6)),
        (new Rational(-12), new Rational(8), new Rational(4)),
        (new Rational(12), new Rational(-8), new Rational(4)),
        (new Rational(-12), new Rational(-8), new Rational(4)),
        (new Rational(-4), new Rational(6), new Rational(2)),
        (new Rational(4), new Rational(-6), new Rational(2)),
        (new Rational(-4), new Rational(-6), new Rational(2)),
        (new Rational(-2, 3), new Rational(4, 5), new Rational(2, 15)),
        (new Rational(2, 3), new Rational(-4, 5), new Rational(2, 15)),
        (new Rational(-2, 3), new Rational(-4, 5), new Rational(2, 15)),
        (new Rational(-1, 2), new Rational(3, 4), new Rational(1, 4)),
        (new Rational(7, 10), new Rational(-3, 5), new Rational(1, 10)),
        (new Rational(-7, 10), new Rational(-3, 5), new Rational(1, 10)),
    ];

    public static IEnumerable<object[]> GetNegativeGcdCases()
        => NegativeGcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeGcdCases))]
    public void Gcd_WithNegativeInputs_ReturnsPositive(Rational a, Rational b, Rational expected)
    {
        var gcd = Nancy.Numerics.Rational.GreatestCommonDivisor(a, b);
        Assert.True(gcd >= 0);
        Assert.Equal(expected, gcd);
    }

    public static List<(long a, long b, long expected)> GcdLongMinValueCases =
    [
        (long.MinValue, 6, 2),
        (6, long.MinValue, 2),
        (long.MinValue, -12, 4),
        (long.MinValue, 1L << 40, 1L << 40),
        (long.MinValue, long.MaxValue, 1),
    ];

    public static IEnumerable<object[]> GetGcdLongMinValueCases()
        => GcdLongMinValueCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetGcdLongMinValueCases))]
    public void Long_WithInt64MinValue(long a, long b, long expected)
    {
        Assert.Equal(expected, Nancy.Numerics.Rational.GreatestCommonDivisor(a, b));
    }

    [Theory]
    [InlineData(long.MinValue, 0)]
    [InlineData(0, long.MinValue)]
    [InlineData(long.MinValue, long.MinValue)]
    public void Long_GcdOfTwoToThe63_Overflows(long a, long b)
    {
        Assert.Throws<System.OverflowException>(() => Nancy.Numerics.Rational.GreatestCommonDivisor(a, b));
    }

    public static List<(Rational a, Rational b)> NegativeLcmCases =
    [
        (new Rational(-4), new Rational(6)),
        (new Rational(4), new Rational(-6)),
        (new Rational(-4), new Rational(-6)),
        (new Rational(-2, 3), new Rational(4, 5)),
        (new Rational(2, 3), new Rational(-4, 5)),
        (new Rational(-2, 3), new Rational(-4, 5)),
    ];

    public static IEnumerable<object[]> GetNegativeLcmCases()
        => NegativeLcmCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeLcmCases))]
    public void Lcm_WithNegativeInputs_ReturnsNonNegative(Rational a, Rational b)
    {
        var lcm = Nancy.Numerics.Rational.LeastCommonMultiple(a, b);
        Assert.True(lcm >= 0);
    }
}
