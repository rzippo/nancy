#if BIG_RATIONAL
using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class Inversion
{
    public static IEnumerable<(Rational Value, BigInteger Numerator, BigInteger Denominator)> Cases()
    {
        yield return (new Rational(3, 4), 4, 3);
        yield return (new Rational(-3, 4), -4, 3);
        var power = BigInteger.One << 256;
        yield return (new Rational(power + 1, power), power, power + 1);
        yield return (new Rational(-power - 1, power), -power, power + 1);
        yield return (Rational.Zero, 1, 0);
        yield return (Rational.PlusInfinity, 0, 1);
        yield return (Rational.MinusInfinity, 0, 1);
    }

    public static IEnumerable<object[]> TestCases() => Cases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(TestCases))]
    public void InvertReturnsCanonicalParts(Rational value, BigInteger numerator, BigInteger denominator)
    {
        var result = Rational.Invert(value);
        Assert.Equal(numerator, result.Numerator);
        Assert.Equal(denominator, result.Denominator);
    }

    [Fact]
    public void InvertRejectsDefaultValue()
    {
        var exception = Assert.Throws<UndeterminedResultException>(() => Rational.Invert(default));
        Assert.Equal("Zero over zero", exception.Message);
    }
}
#endif
