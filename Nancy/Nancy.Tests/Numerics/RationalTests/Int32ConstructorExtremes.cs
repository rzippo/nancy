using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

#if BIG_RATIONAL
public class Int32ConstructorExtremes
{
    [Theory]
    [InlineData(int.MinValue, -1)]
    [InlineData(int.MinValue, -8)]
    [InlineData(1, int.MinValue)]
    [InlineData(-6, int.MinValue)]
    [InlineData(int.MinValue, int.MinValue)]
    public void NegativeDenominatorNormalizesWithoutInt32Overflow(int numerator, int denominator)
    {
        var expected = new Rational(new BigInteger(numerator), new BigInteger(denominator));
        var actual = new Rational(numerator, denominator);

        Assert.Equal(expected.Numerator, actual.Numerator);
        Assert.Equal(expected.Denominator, actual.Denominator);
    }
}
#endif
