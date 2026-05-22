using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class PrimitiveEdgeCases
{
    public static List<(long numerator, long denominator, BigRational expected)> NormalizationCases =
    [
        (0, -5, BigRational.Zero),
        (6, -8, new BigRational(-3, 4)),
        (-6, -8, new BigRational(3, 4)),
        (5, 0, BigRational.PlusInfinity),
        (-5, 0, BigRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> NormalizationTestCases()
        => NormalizationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NormalizationTestCases))]
    public void ConstructorNormalizesSpecialValues(long numerator, long denominator, BigRational expected)
    {
        var value = new BigRational(numerator, denominator);

        Assert.Equal(expected, value);
        Assert.Equal(expected.Numerator, value.Numerator);
        Assert.Equal(expected.Denominator, value.Denominator);
    }

    public static List<(BigInteger whole, BigInteger numerator, BigInteger denominator, BigRational expected)> MixedNumberCases =
    [
        (1, 1, 2, new BigRational(3, 2)),
        (-1, 1, 2, new BigRational(-1, 2)),
        (1, 1, -2, new BigRational(-3, 2)),
        (0, 0, 7, BigRational.Zero)
    ];

    public static IEnumerable<object[]> MixedNumberTestCases()
        => MixedNumberCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MixedNumberTestCases))]
    public void MixedNumberConstructorNormalizesParts(
        BigInteger whole,
        BigInteger numerator,
        BigInteger denominator,
        BigRational expected)
    {
        var value = new BigRational(whole, numerator, denominator);

        Assert.Equal(expected, value);
        Assert.Equal(expected.Numerator, value.Numerator);
        Assert.Equal(expected.Denominator, value.Denominator);
    }

    public static List<(BigInteger whole, BigInteger numerator, BigInteger denominator)> InvalidMixedNumberCases =
    [
        (1, 1, 0)
    ];

    public static IEnumerable<object[]> InvalidMixedNumberTestCases()
        => InvalidMixedNumberCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidMixedNumberTestCases))]
    public void MixedNumberConstructorRejectsZeroDenominator(
        BigInteger whole,
        BigInteger numerator,
        BigInteger denominator)
    {
        Assert.Throws<DivideByZeroException>(() => new BigRational(whole, numerator, denominator));
    }

    public static List<(long numerator, long denominator)> UndeterminedConstructorCases =
    [
        (0, 0)
    ];

    public static IEnumerable<object[]> UndeterminedConstructorTestCases()
        => UndeterminedConstructorCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UndeterminedConstructorTestCases))]
    public void ConstructorRejectsZeroOverZero(long numerator, long denominator)
    {
        Assert.Throws<UndeterminedResultException>(() => new BigRational(numerator, denominator));
    }

    public static List<(BigRational value, BigRational whole, BigRational fraction)> DecompositionCases =
    [
        (new BigRational(7, 3), 2, new BigRational(1, 3)),
        (new BigRational(-7, 3), -2, new BigRational(-1, 3)),
        (new BigRational(1, 2), 0, new BigRational(1, 2)),
        (new BigRational(-1, 2), 0, new BigRational(-1, 2))
    ];

    public static IEnumerable<object[]> DecompositionTestCases()
        => DecompositionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecompositionTestCases))]
    public void DecomposesIntoWholeAndFractionParts(BigRational value, BigRational whole, BigRational fraction)
    {
        Assert.Equal(whole, new BigRational(value.GetWholePart()));
        Assert.Equal(fraction, value.GetFractionPart());
    }

    public static List<(BigRational value, BigRational abs, BigRational negated, BigRational inverted)> UnaryOperationCases =
    [
        (new BigRational(-3, 4), new BigRational(3, 4), new BigRational(3, 4), new BigRational(-4, 3)),
        (new BigRational(3, 4), new BigRational(3, 4), new BigRational(-3, 4), new BigRational(4, 3)),
        (BigRational.PlusInfinity, BigRational.PlusInfinity, BigRational.MinusInfinity, BigRational.Zero),
        (BigRational.MinusInfinity, BigRational.PlusInfinity, BigRational.PlusInfinity, BigRational.Zero)
    ];

    public static IEnumerable<object[]> UnaryOperationTestCases()
        => UnaryOperationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnaryOperationTestCases))]
    public void UnaryOperationsHandleFiniteAndInfiniteValues(
        BigRational value,
        BigRational abs,
        BigRational negated,
        BigRational inverted)
    {
        Assert.Equal(abs, BigRational.Abs(value));
        Assert.Equal(negated, BigRational.Negate(value));
        Assert.Equal(inverted, BigRational.Invert(value));
    }

    public static List<(BigRational dividend, BigRational divisor, BigRational quotient, BigRational remainder)> DivRemCases =
    [
        (new BigRational(7, 3), new BigRational(1, 2), new BigRational(14, 3), new BigRational(1, 3)),
        (new BigRational(-7, 3), new BigRational(1, 2), new BigRational(-14, 3), new BigRational(-1, 3)),
        (new BigRational(7, 3), new BigRational(2, 3), new BigRational(7, 2), new BigRational(1, 3))
    ];

    public static IEnumerable<object[]> DivRemTestCases()
        => DivRemCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DivRemTestCases))]
    public void DivRemMatchesDivisionAndRemainder(
        BigRational dividend,
        BigRational divisor,
        BigRational quotient,
        BigRational remainder)
    {
        var result = BigRational.DivRem(dividend, divisor, out var actualRemainder);

        Assert.Equal(quotient, result);
        Assert.Equal(remainder, actualRemainder);
        Assert.Equal(remainder, BigRational.Remainder(dividend, divisor));
    }

    public static List<(BigRational left, BigRational right)> InfiniteRemainderCases =
    [
        (BigRational.PlusInfinity, BigRational.One),
        (BigRational.One, BigRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> InfiniteRemainderTestCases()
        => InfiniteRemainderCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteRemainderTestCases))]
    public void RemainderRejectsInfiniteOperands(BigRational left, BigRational right)
    {
        Assert.Throws<UndeterminedResultException>(() => left % right);
        Assert.Throws<UndeterminedResultException>(() => BigRational.Remainder(left, right));
    }

    public static List<(BigRational value, BigInteger exponent, BigRational expected)> PowerCases =
    [
        (new BigRational(2, 3), 3, new BigRational(8, 27)),
        (new BigRational(2, 3), -2, new BigRational(9, 4)),
        (new BigRational(-2), 3, new BigRational(-8)),
        (BigRational.Zero, 0, BigRational.One)
    ];

    public static IEnumerable<object[]> PowerTestCases()
        => PowerCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PowerTestCases))]
    public void PowHandlesPositiveNegativeAndZeroExponents(
        BigRational value,
        BigInteger exponent,
        BigRational expected)
    {
        Assert.Equal(expected, BigRational.Pow(value, exponent));
    }

    public static List<BigInteger> InvalidZeroPowerExponents =
    [
        -1
    ];

    public static IEnumerable<object[]> InvalidZeroPowerTestCases()
        => InvalidZeroPowerExponents.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidZeroPowerTestCases))]
    public void PowRejectsZeroWithNegativeExponent(BigInteger exponent)
    {
        Assert.Throws<ArgumentException>(() => BigRational.Pow(BigRational.Zero, exponent));
    }

    public static List<(BigRational left, BigRational right, BigRational gcd, BigRational lcm, BigInteger lcd)> CommonMeasureCases =
    [
        (new BigRational(3, 2), BigRational.One, new BigRational(1, 2), new BigRational(3), 2),
        (new BigRational(5, 6), new BigRational(7, 10), new BigRational(1, 30), new BigRational(35, 2), 30),
        (new BigRational(12), new BigRational(8), new BigRational(4), new BigRational(24), 1)
    ];

    public static IEnumerable<object[]> CommonMeasureTestCases()
        => CommonMeasureCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CommonMeasureTestCases))]
    public void GcdLcmAndLcdHandleFractionalValues(
        BigRational left,
        BigRational right,
        BigRational gcd,
        BigRational lcm,
        BigInteger lcd)
    {
        Assert.Equal(gcd, BigRational.GreatestCommonDivisor(left, right));
        Assert.Equal(lcm, BigRational.LeastCommonMultiple(left, right));
        Assert.Equal(lcd, BigRational.LeastCommonDenominator(left, right));
    }

    public static List<(BigRational value, string toString, string code)> FormattingCases =
    [
        (BigRational.Zero, "0/1", "0"),
        (new BigRational(3, 4), "3/4", "new Rational(3, 4)"),
        (new BigRational(-2), "-2/1", "-2"),
        (BigRational.PlusInfinity, "1/0", "new Rational(1, 0)"),
        (BigRational.MinusInfinity, "-1/0", "new Rational(-1, 0)")
    ];

    public static IEnumerable<object[]> FormattingTestCases()
        => FormattingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FormattingTestCases))]
    public void FormattingMethodsReturnStableRepresentations(
        BigRational value,
        string toString,
        string code)
    {
        Assert.Equal(toString, value.ToString());
        Assert.Equal(code, value.ToCodeString());
    }

    public static List<(BigRational value, object? other, int expectedSign)> ObjectComparisonCases =
    [
        (new BigRational(2, 3), null, 1),
        (new BigRational(2, 3), new BigRational(2, 3), 0),
        (new BigRational(2, 3), new BigRational(1, 3), 1),
        (new BigRational(2, 3), new BigRational(3, 4), -1)
    ];

    public static IEnumerable<object[]> ObjectComparisonTestCases()
        => ObjectComparisonCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ObjectComparisonTestCases))]
    public void ComparableObjectImplementationUsesRationalOrdering(
        BigRational value,
        object? other,
        int expectedSign)
    {
        var comparison = ((IComparable)value).CompareTo(other);

        Assert.Equal(Math.Sign(expectedSign), Math.Sign(comparison));
    }

    public static List<object> InvalidComparisonObjects =
    [
        "not a rational"
    ];

    public static IEnumerable<object[]> InvalidComparisonObjectTestCases()
        => InvalidComparisonObjects.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidComparisonObjectTestCases))]
    public void ComparableObjectImplementationRejectsNonRationalObjects(object other)
    {
        Assert.Throws<ArgumentException>(() => ((IComparable)BigRational.One).CompareTo(other));
    }
}
