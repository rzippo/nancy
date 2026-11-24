using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class Inversion
{
    public static IEnumerable<(BigRational Value, BigInteger Numerator, BigInteger Denominator)> Cases()
    {
        yield return (new BigRational(3, 4), 4, 3);
        yield return (new BigRational(-3, 4), -4, 3);
        var power = BigInteger.One << 256;
        yield return (new BigRational(power + 1, power), power, power + 1);
        yield return (new BigRational(-power - 1, power), -power, power + 1);
        yield return (BigRational.Zero, 1, 0);
        yield return (BigRational.PlusInfinity, 0, 1);
        yield return (BigRational.MinusInfinity, 0, 1);
    }

    public static IEnumerable<object[]> TestCases() => Cases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(TestCases))]
    public void InvertReturnsCanonicalParts(BigRational value, BigInteger numerator, BigInteger denominator)
    {
        var result = BigRational.Invert(value);
        Assert.Equal(numerator, result.Numerator);
        Assert.Equal(denominator, result.Denominator);
    }

    [Fact]
    public void InvertRejectsDefaultValue()
    {
        var exception = Assert.Throws<UndeterminedResultException>(() => BigRational.Invert(default));
        Assert.Equal("Zero over zero", exception.Message);
    }
}
