using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

#if BIG_RATIONAL
public class Int64ConstructorExtremes
{
    [Theory]
    [InlineData(long.MinValue, -1L)]
    [InlineData(1L, long.MinValue)]
    [InlineData(long.MinValue, long.MinValue)]
    [InlineData(long.MinValue + 1, long.MinValue)]
    public void NegativeDenominatorNormalizesWithoutInt64Overflow(long numerator, long denominator)
    {
        var expected = new Rational(new BigInteger(numerator), new BigInteger(denominator));
        var actual = new Rational(numerator, denominator);

        Assert.Equal(expected.Numerator, actual.Numerator);
        Assert.Equal(expected.Denominator, actual.Denominator);
    }
}
#endif
