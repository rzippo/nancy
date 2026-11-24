#if BIG_RATIONAL
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class SharedFactorAddition
{
    public static IEnumerable<object[]> Cases() => SharedFactorAdditionCases.Values();

    [Theory]
    [MemberData(nameof(Cases))]
    public void AdditionReturnsCanonicalParts(int a, BigInteger b, int c, BigInteger d, int remainingDivisor)
    {
        var left = new Rational(a, b);
        var right = new Rational(c, d);
        Assert.Equal(b, left.Denominator);
        Assert.Equal(d, right.Denominator);
        Assert.NotEqual(b, d);
        Assert.True(b.GetBitLength() + d.GetBitLength() > 63);
        var denominatorGcd = BigInteger.GreatestCommonDivisor(b, d);
        Assert.True(denominatorGcd > 1);

        // Reduce the direct cross-product sum independently of the LCD implementation.
        var numerator = a * d + c * b;
        var denominator = b * d;
        var divisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
        Assert.Equal(denominatorGcd * remainingDivisor, divisor);
        var expectedNumerator = numerator / divisor;
        var expectedDenominator = denominator / divisor;

        foreach (var result in new[] { left + right, right + left, Rational.Add(left, right) })
        {
            Assert.Equal(expectedNumerator, result.Numerator);
            Assert.Equal(expectedDenominator, result.Denominator);
        }
    }
}
#endif
