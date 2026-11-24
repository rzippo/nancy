#if BIG_RATIONAL
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class DivisionTiers
{
    public static IEnumerable<object[]> Cases() => DivisionTierCases.Values();

    [Theory]
    [MemberData(nameof(Cases))]
    public void QuotientReturnsCanonicalParts(BigInteger p, BigInteger q, BigInteger r, BigInteger s, BigInteger numerator, BigInteger denominator)
    {
        var dividend = new Rational(p, q);
        var divisor = new Rational(r, s);
        // The operands are reduced as given, so each case reaches the tier its components select.
        Assert.Equal(p, dividend.Numerator);
        Assert.Equal(q, dividend.Denominator);
        Assert.Equal(r, divisor.Numerator);
        Assert.Equal(s, divisor.Denominator);

        foreach (var result in new[] { dividend / divisor, Rational.Divide(dividend, divisor) })
        {
            Assert.Equal(numerator, result.Numerator);
            Assert.Equal(denominator, result.Denominator);
        }
    }
}
#endif
