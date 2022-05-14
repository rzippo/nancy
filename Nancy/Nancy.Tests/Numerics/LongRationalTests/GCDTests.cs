using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class GCDTests
{
    public static IEnumerable<object[]> GetRationalTestCases()
    {
        yield return new object[] { new LongRational(84), new LongRational(18), new LongRational(6) };
        yield return new object[] { new LongRational(18) , new LongRational(84), new LongRational(6) };

        yield return new object[] { new LongRational(12) , new LongRational(8), new LongRational(4) };
        yield return new object[] { new LongRational(8) , new LongRational(12), new LongRational(4) };

    }

    [Theory]
    [MemberData(nameof(GetRationalTestCases))]
    public void Rational(LongRational a, LongRational b, LongRational expected)
    {
        var gcd = Nancy.Numerics.LongRational.GreatestCommonDivisor(a, b);
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