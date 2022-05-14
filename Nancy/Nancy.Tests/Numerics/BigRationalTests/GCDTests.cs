using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class GCDTests
{
    public static IEnumerable<object[]> GetTestCases()
    {
        yield return new object[] { new BigRational(84) , new BigRational(18), new BigRational(6) };
        yield return new object[] { new BigRational(18) , new BigRational(84), new BigRational(6) };

        yield return new object[] { new BigRational(12) , new BigRational(8), new BigRational(4) };
        yield return new object[] { new BigRational(8) , new BigRational(12), new BigRational(4) };

    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void ValueTest(BigRational a, BigRational b, BigRational expected)
    {
        var gcd = BigRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expected, gcd);
    }
}