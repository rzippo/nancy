using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class GCDTests
{
    public static IEnumerable<object[]> GetRationalTestCases()
    {
        yield return new object[] { new Rational(84), new Rational(18), new Rational(6) };
        yield return new object[] { new Rational(18) , new Rational(84), new Rational(6) };

        yield return new object[] { new Rational(12) , new Rational(8), new Rational(4) };
        yield return new object[] { new Rational(8) , new Rational(12), new Rational(4) };

    }

    [Theory]
    [MemberData(nameof(GetRationalTestCases))]
    public void Rational(Rational a, Rational b, Rational expected)
    {
        var gcd = Nancy.Numerics.Rational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }

    public static IEnumerable<object[]> GetLongTestCases()
    {
        yield return new object[] { 84, 18, 6 };
        yield return new object[] { 18, 84, 6 };

        yield return new object[] { 12, 8, 4 };
        yield return new object[] { 8, 12, 4 };

    }

    [Theory]
    [MemberData(nameof(GetLongTestCases))]
    public void Long(long a, long b, long expected)
    {
        long gcd = Nancy.Numerics.LongRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }
}