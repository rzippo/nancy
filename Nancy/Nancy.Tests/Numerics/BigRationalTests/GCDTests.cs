using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class GCDTests
{
    public static List<(BigRational a, BigRational b, BigRational expected)> GcdCases =
    [
        (new BigRational(84), new BigRational(18), new BigRational(6)),
        (new BigRational(18), new BigRational(84), new BigRational(6)),
        (new BigRational(12), new BigRational(8), new BigRational(4)),
        (new BigRational(8), new BigRational(12), new BigRational(4)),
    ];

    public static IEnumerable<object[]> GetGcdCases()
        => GcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetGcdCases))]
    public void ValueTest(BigRational a, BigRational b, BigRational expected)
    {
        var gcd = BigRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }

    public static List<(BigRational a, BigRational b, BigRational expected)> NegativeGcdCases =
    [
        (new BigRational(-84), new BigRational(18), new BigRational(6)),
        (new BigRational(84), new BigRational(-18), new BigRational(6)),
        (new BigRational(-84), new BigRational(-18), new BigRational(6)),
        (new BigRational(-12), new BigRational(8), new BigRational(4)),
        (new BigRational(12), new BigRational(-8), new BigRational(4)),
        (new BigRational(-12), new BigRational(-8), new BigRational(4)),
        (new BigRational(-4), new BigRational(6), new BigRational(2)),
        (new BigRational(4), new BigRational(-6), new BigRational(2)),
        (new BigRational(-4), new BigRational(-6), new BigRational(2)),
        (new BigRational(-2, 3), new BigRational(4, 5), new BigRational(2, 15)),
        (new BigRational(2, 3), new BigRational(-4, 5), new BigRational(2, 15)),
        (new BigRational(-2, 3), new BigRational(-4, 5), new BigRational(2, 15)),
        (new BigRational(-1, 2), new BigRational(3, 4), new BigRational(1, 4)),
        (new BigRational(7, 10), new BigRational(-3, 5), new BigRational(1, 10)),
        (new BigRational(-7, 10), new BigRational(-3, 5), new BigRational(1, 10)),
    ];

    public static IEnumerable<object[]> GetNegativeGcdCases()
        => NegativeGcdCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeGcdCases))]
    public void Gcd_WithNegativeInputs_ReturnsPositive(BigRational a, BigRational b, BigRational expected)
    {
        var gcd = BigRational.GreatestCommonDivisor(a, b);
        Assert.True(gcd >= 0);
        Assert.Equal(expected, gcd);
    }

    public static List<(BigRational a, BigRational b)> NegativeLcmCases =
    [
        (new BigRational(-4), new BigRational(6)),
        (new BigRational(4), new BigRational(-6)),
        (new BigRational(-4), new BigRational(-6)),
        (new BigRational(-2, 3), new BigRational(4, 5)),
        (new BigRational(2, 3), new BigRational(-4, 5)),
        (new BigRational(-2, 3), new BigRational(-4, 5)),
    ];

    public static IEnumerable<object[]> GetNegativeLcmCases()
        => NegativeLcmCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetNegativeLcmCases))]
    public void Lcm_WithNegativeInputs_ReturnsNonNegative(BigRational a, BigRational b)
    {
        var lcm = BigRational.LeastCommonMultiple(a, b);
        Assert.True(lcm >= 0);
    }
}
