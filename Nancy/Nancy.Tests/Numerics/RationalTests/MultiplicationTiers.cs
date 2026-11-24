#if BIG_RATIONAL
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class MultiplicationTiers
{
    public static IEnumerable<object[]> Cases() => MultiplicationTierCases.Values();

    [Theory]
    [MemberData(nameof(Cases))]
    public void ProductReturnsCanonicalParts(BigInteger a, BigInteger b, BigInteger c, BigInteger d, BigInteger numerator, BigInteger denominator)
    {
        var left = new Rational(a, b);
        var right = new Rational(c, d);
        // The operands are reduced as given, so each case reaches the tier its components select.
        Assert.Equal(a, left.Numerator);
        Assert.Equal(b, left.Denominator);
        Assert.Equal(c, right.Numerator);
        Assert.Equal(d, right.Denominator);

        foreach (var result in new[] { left * right, Rational.Multiply(left, right) })
        {
            Assert.Equal(numerator, result.Numerator);
            Assert.Equal(denominator, result.Denominator);
        }
    }
}
#endif
