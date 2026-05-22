using System;
using System.Collections.Generic;
using System.Numerics;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class PrimitiveEdgeCases
{
    public static List<(long numerator, long denominator, Rational expected)> NormalizationCases =
    [
        (0, -5, Rational.Zero),
        (6, -8, new Rational(-3, 4)),
        (-6, -8, new Rational(3, 4)),
        (5, 0, Rational.PlusInfinity),
        (-5, 0, Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> NormalizationTestCases()
        => NormalizationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NormalizationTestCases))]
    public void ConstructorNormalizesSpecialValues(long numerator, long denominator, Rational expected)
    {
        var value = new Rational(numerator, denominator);

        Assert.Equal(expected, value);
        Assert.Equal(expected.Numerator, value.Numerator);
        Assert.Equal(expected.Denominator, value.Denominator);
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
        Assert.Throws<UndeterminedResultException>(() => new Rational(numerator, denominator));
    }

    public static List<(Rational value, Rational whole, Rational fraction)> DecompositionCases =
    [
        (new Rational(7, 3), 2, new Rational(1, 3)),
        (new Rational(-7, 3), -2, new Rational(-1, 3)),
        (new Rational(1, 2), 0, new Rational(1, 2)),
        (new Rational(-1, 2), 0, new Rational(-1, 2))
    ];

    public static IEnumerable<object[]> DecompositionTestCases()
        => DecompositionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecompositionTestCases))]
    public void DecomposesIntoWholeAndFractionParts(Rational value, Rational whole, Rational fraction)
    {
        Assert.Equal(whole, new Rational(value.GetWholePart()));
        Assert.Equal(fraction, value.GetFractionPart());
    }

    public static List<(Rational value, Rational abs, Rational negated, Rational inverted)> UnaryOperationCases =
    [
        (new Rational(-3, 4), new Rational(3, 4), new Rational(3, 4), new Rational(-4, 3)),
        (new Rational(3, 4), new Rational(3, 4), new Rational(-3, 4), new Rational(4, 3)),
        (Rational.PlusInfinity, Rational.PlusInfinity, Rational.MinusInfinity, Rational.Zero),
        (Rational.MinusInfinity, Rational.PlusInfinity, Rational.PlusInfinity, Rational.Zero)
    ];

    public static IEnumerable<object[]> UnaryOperationTestCases()
        => UnaryOperationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnaryOperationTestCases))]
    public void UnaryOperationsHandleFiniteAndInfiniteValues(
        Rational value,
        Rational abs,
        Rational negated,
        Rational inverted)
    {
        Assert.Equal(abs, Rational.Abs(value));
        Assert.Equal(negated, Rational.Negate(value));
        Assert.Equal(inverted, Rational.Invert(value));
    }

    public static List<(Rational dividend, Rational divisor, Rational quotient, Rational remainder)> DivRemCases =
    [
        (new Rational(7, 3), new Rational(1, 2), new Rational(14, 3), new Rational(1, 3)),
        (new Rational(-7, 3), new Rational(1, 2), new Rational(-14, 3), new Rational(-1, 3)),
        (new Rational(7, 3), new Rational(2, 3), new Rational(7, 2), new Rational(1, 3))
    ];

    public static IEnumerable<object[]> DivRemTestCases()
        => DivRemCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DivRemTestCases))]
    public void DivRemMatchesDivisionAndRemainder(
        Rational dividend,
        Rational divisor,
        Rational quotient,
        Rational remainder)
    {
        var result = Rational.DivRem(dividend, divisor, out var actualRemainder);

        Assert.Equal(quotient, result);
        Assert.Equal(remainder, actualRemainder);
        Assert.Equal(remainder, Rational.Remainder(dividend, divisor));
    }

    public static List<(Rational left, Rational right)> InfiniteRemainderCases =
    [
        (Rational.PlusInfinity, Rational.One),
        (Rational.One, Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> InfiniteRemainderTestCases()
        => InfiniteRemainderCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteRemainderTestCases))]
    public void RemainderRejectsInfiniteOperands(Rational left, Rational right)
    {
        Assert.Throws<UndeterminedResultException>(() => left % right);
        Assert.Throws<UndeterminedResultException>(() => Rational.Remainder(left, right));
    }

    public static List<(Rational value, BigInteger exponent, Rational expected)> PowerCases =
    [
        (new Rational(2, 3), 3, new Rational(8, 27)),
        (new Rational(2, 3), -2, new Rational(9, 4)),
        (new Rational(-2), 3, new Rational(-8)),
        (Rational.Zero, 0, Rational.One)
    ];

    public static IEnumerable<object[]> PowerTestCases()
        => PowerCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PowerTestCases))]
    public void PowHandlesPositiveNegativeAndZeroExponents(
        Rational value,
        BigInteger exponent,
        Rational expected)
    {
        Assert.Equal(expected, Rational.Pow(value, exponent));
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
        Assert.Throws<ArgumentException>(() => Rational.Pow(Rational.Zero, exponent));
    }

    public static List<(Rational left, Rational right, Rational gcd, Rational lcm, BigInteger lcd)> CommonMeasureCases =
    [
        (new Rational(3, 2), Rational.One, new Rational(1, 2), new Rational(3), 2),
        (new Rational(5, 6), new Rational(7, 10), new Rational(1, 30), new Rational(35, 2), 30),
        (new Rational(12), new Rational(8), new Rational(4), new Rational(24), 1)
    ];

    public static IEnumerable<object[]> CommonMeasureTestCases()
        => CommonMeasureCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CommonMeasureTestCases))]
    public void GcdLcmAndLcdHandleFractionalValues(
        Rational left,
        Rational right,
        Rational gcd,
        Rational lcm,
        BigInteger lcd)
    {
        Assert.Equal(gcd, Rational.GreatestCommonDivisor(left, right));
        Assert.Equal(lcm, Rational.LeastCommonMultiple(left, right));
        Assert.Equal(lcd, Rational.LeastCommonDenominator(left, right));
    }

    public static List<(Rational value, string toString, string code, string mppg)> FormattingCases =
    [
        (Rational.Zero, "0", "0", "0"),
        (new Rational(3, 4), "3/4", "new Rational(3, 4)", "3/4"),
        (new Rational(-2), "-2", "-2", "-2"),
        (Rational.PlusInfinity, "1/0", "new Rational(1, 0)", "+inf"),
        (Rational.MinusInfinity, "-1/0", "new Rational(-1, 0)", "-inf")
    ];

    public static IEnumerable<object[]> FormattingTestCases()
        => FormattingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FormattingTestCases))]
    public void FormattingMethodsReturnStableRepresentations(
        Rational value,
        string toString,
        string code,
        string mppg)
    {
        Assert.Equal(toString, value.ToString());
        Assert.Equal(code, value.ToCodeString());
        Assert.Equal(mppg, value.ToMppgString());
    }

    public static List<(Rational value, object? other, int expectedSign)> ObjectComparisonCases =
    [
        (new Rational(2, 3), null, 1),
        (new Rational(2, 3), new Rational(2, 3), 0),
        (new Rational(2, 3), new Rational(1, 3), 1),
        (new Rational(2, 3), new Rational(3, 4), -1)
    ];

    public static IEnumerable<object[]> ObjectComparisonTestCases()
        => ObjectComparisonCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ObjectComparisonTestCases))]
    public void ComparableObjectImplementationUsesRationalOrdering(
        Rational value,
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
        Assert.Throws<ArgumentException>(() => ((IComparable)Rational.One).CompareTo(other));
    }
}
