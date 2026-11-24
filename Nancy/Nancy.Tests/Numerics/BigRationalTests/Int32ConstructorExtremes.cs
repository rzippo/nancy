using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

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
        var expected = new BigRational(new BigInteger(numerator), new BigInteger(denominator));
        var actual = new BigRational(numerator, denominator);

        Assert.Equal(expected.Numerator, actual.Numerator);
        Assert.Equal(expected.Denominator, actual.Denominator);
    }
}
