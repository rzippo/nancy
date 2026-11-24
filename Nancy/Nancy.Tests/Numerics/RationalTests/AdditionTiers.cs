#if BIG_RATIONAL
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class AdditionTiers
{
    public static IEnumerable<object[]> Cases() => AdditionTierCases.Values();

    [Theory]
    [MemberData(nameof(Cases))]
    public void SumDifferenceAndComparisonAreExact(BigInteger a, BigInteger b, BigInteger c, BigInteger d,
        BigInteger sumNumerator, BigInteger sumDenominator, BigInteger differenceNumerator, BigInteger differenceDenominator, int comparison)
    {
        var left = new Rational(a, b);
        var right = new Rational(c, d);
        // The operands are reduced as given, so each case reaches the tier its components select.
        Assert.Equal(a, left.Numerator);
        Assert.Equal(b, left.Denominator);
        Assert.Equal(c, right.Numerator);
        Assert.Equal(d, right.Denominator);

        foreach (var sum in new[] { left + right, Rational.Add(left, right) })
        {
            Assert.Equal(sumNumerator, sum.Numerator);
            Assert.Equal(sumDenominator, sum.Denominator);
        }
        foreach (var difference in new[] { left - right, Rational.Subtract(left, right) })
        {
            Assert.Equal(differenceNumerator, difference.Numerator);
            Assert.Equal(differenceDenominator, difference.Denominator);
        }
        Assert.Equal(comparison, Rational.Compare(left, right));
        Assert.Equal(comparison == 0, left == right);
        Assert.Equal(comparison != 0, left != right);
        Assert.Equal(comparison == 0, left.Equals(right));
    }
}
#endif
