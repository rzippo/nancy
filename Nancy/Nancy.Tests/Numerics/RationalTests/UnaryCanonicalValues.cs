#if BIG_RATIONAL
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class UnaryCanonicalValues
{
    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { BigInteger.Zero, BigInteger.One };
        yield return new object[] { BigInteger.One, BigInteger.One };
        yield return new object[] { BigInteger.MinusOne, BigInteger.One };
        yield return new object[] { new BigInteger(7), new BigInteger(3) };
        yield return new object[] { new BigInteger(-7), new BigInteger(3) };
        yield return new object[] { new BigInteger(long.MinValue), BigInteger.One };
        yield return new object[] { new BigInteger(long.MaxValue), BigInteger.One };
        yield return new object[] { (BigInteger.One << 512) + 1, (BigInteger.One << 256) + 3 };
        yield return new object[] { -((BigInteger.One << 512) + 1), (BigInteger.One << 256) + 3 };
        yield return new object[] { BigInteger.One, BigInteger.Zero };
        yield return new object[] { BigInteger.MinusOne, BigInteger.Zero };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void UnaryOperationsPreserveCanonicalParts(BigInteger numerator, BigInteger denominator)
    {
        var value = new Rational(numerator, denominator);
        var absolute = Rational.Abs(value);
        var opposite = -value;
        var negated = Rational.Negate(value);

        Assert.Equal(BigInteger.Abs(value.Numerator), absolute.Numerator);
        Assert.Equal(value.Denominator, absolute.Denominator);
        Assert.Equal(-value.Numerator, opposite.Numerator);
        Assert.Equal(value.Denominator, opposite.Denominator);
        Assert.Equal(-value.Numerator, negated.Numerator);
        Assert.Equal(value.Denominator, negated.Denominator);
    }

    [Fact]
    public void AbsPreservesDefaultValue()
    {
        var result = Rational.Abs(default);
        Assert.Equal(BigInteger.Zero, result.Numerator);
        Assert.Equal(BigInteger.Zero, result.Denominator);
    }

    [Fact]
    public void NegatingDefaultValueRemainsUndetermined()
    {
        var operatorException = Assert.Throws<UndeterminedResultException>(() => -default(Rational));
        var methodException = Assert.Throws<UndeterminedResultException>(() => Rational.Negate(default));
        Assert.Equal("Zero over zero", operatorException.Message);
        Assert.Equal("Zero over zero", methodException.Message);
    }
}
#endif
