using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

#if BIG_RATIONAL
public class Int64Construction
{
    public static IEnumerable<object[]> SignedPairs()
        => Int64ConstructorCases.SignedPairs().ToXUnitTestCases();

    public static IEnumerable<object[]> CanonicalPairs()
        => Int64ConstructorCases.CanonicalPairs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(SignedPairs))]
    public void PublicConstructorNormalizesSignedInputs(long numerator, long denominator)
    {
        var expected = new BigRational(new BigInteger(numerator), new BigInteger(denominator));
        var actual = new Rational(numerator, denominator);

        Assert.Equal(expected.Numerator, actual.Numerator);
        Assert.Equal(expected.Denominator, actual.Denominator);
    }

    [Theory]
    [MemberData(nameof(SignedPairs))]
    public void NormalizingOverloadNormalizesSignedInputs(long numerator, long denominator)
    {
        var expected = new BigRational(new BigInteger(numerator), new BigInteger(denominator));
        var actual = new Rational(numerator, denominator, skipNormalize: false);

        Assert.Equal(expected.Numerator, actual.Numerator);
        Assert.Equal(expected.Denominator, actual.Denominator);
    }

    [Theory]
    [MemberData(nameof(CanonicalPairs))]
    public void SkippingNormalizationPreservesCanonicalInputs(long numerator, long denominator)
    {
        var actual = new Rational(numerator, denominator, skipNormalize: true);

        Assert.Equal(new BigInteger(numerator), actual.Numerator);
        Assert.Equal(new BigInteger(denominator), actual.Denominator);
    }

    [Fact]
    public void PublicConstructorRejectsZeroOverZero()
    {
        Assert.Throws<UndeterminedResultException>(() => new Rational(0L, 0L));
    }

    [Fact]
    public void NormalizingOverloadRejectsZeroOverZero()
    {
        Assert.Throws<UndeterminedResultException>(() => new Rational(0L, 0L, skipNormalize: false));
    }
}
#endif
