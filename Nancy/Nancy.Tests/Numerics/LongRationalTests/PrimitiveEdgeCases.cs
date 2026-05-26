using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class PrimitiveEdgeCases
{
    public static List<(long numerator, long denominator, LongRational expected)> NormalizationCases =
    [
        (0, -5, LongRational.Zero),
        (6, -8, new LongRational(-3, 4)),
        (-6, -8, new LongRational(3, 4)),
        (5, 0, LongRational.PlusInfinity),
        (-5, 0, LongRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> NormalizationTestCases()
        => NormalizationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NormalizationTestCases))]
    public void ConstructorNormalizesSpecialValues(long numerator, long denominator, LongRational expected)
    {
        var value = new LongRational(numerator, denominator);

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
        Assert.Throws<UndeterminedResultException>(() => new LongRational(numerator, denominator));
    }

    public static List<(LongRational value, LongRational whole, LongRational fraction)> DecompositionCases =
    [
        (new LongRational(7, 3), 2, new LongRational(1, 3)),
        (new LongRational(-7, 3), -2, new LongRational(-1, 3)),
        (new LongRational(1, 2), 0, new LongRational(1, 2)),
        (new LongRational(-1, 2), 0, new LongRational(-1, 2))
    ];

    public static IEnumerable<object[]> DecompositionTestCases()
        => DecompositionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecompositionTestCases))]
    public void DecomposesIntoWholeAndFractionParts(LongRational value, LongRational whole, LongRational fraction)
    {
        Assert.Equal(whole, new LongRational(value.GetWholePart()));
        Assert.Equal(fraction, value.GetFractionPart());
    }

    [Fact]
    public void Abs_OfLongMinValue()
    {
        Assert.Throws<OverflowException>(() => LongRational.Abs(new LongRational(long.MinValue, 1)));
    }

    [Fact]
    public void Negate_OfLongMinValue()
    {
        var min = new LongRational(long.MinValue, 1);
        Assert.Throws<OverflowException>(() => LongRational.Negate(min));
    }

    public static List<(LongRational value, LongRational abs, LongRational negated, LongRational inverted)> UnaryOperationCases =
    [
        (new LongRational(-3, 4), new LongRational(3, 4), new LongRational(3, 4), new LongRational(-4, 3)),
        (new LongRational(3, 4), new LongRational(3, 4), new LongRational(-3, 4), new LongRational(4, 3)),
        (LongRational.Zero, LongRational.Zero, LongRational.Zero, LongRational.PlusInfinity),
        (LongRational.One, LongRational.One, new LongRational(-1), LongRational.One),
        (new LongRational(-1), LongRational.One, LongRational.One, new LongRational(-1)),
        (LongRational.PlusInfinity, LongRational.PlusInfinity, LongRational.MinusInfinity, LongRational.Zero),
        (LongRational.MinusInfinity, LongRational.PlusInfinity, LongRational.PlusInfinity, LongRational.Zero)
    ];

    public static IEnumerable<object[]> UnaryOperationTestCases()
        => UnaryOperationCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnaryOperationTestCases))]
    public void UnaryOperationsHandleFiniteAndInfiniteValues(
        LongRational value,
        LongRational abs,
        LongRational negated,
        LongRational inverted)
    {
        Assert.Equal(abs, LongRational.Abs(value));
        Assert.Equal(negated, LongRational.Negate(value));
        Assert.Equal(inverted, LongRational.Invert(value));
    }

    public static List<(LongRational dividend, LongRational divisor, LongRational quotient, LongRational remainder)> DivRemCases =
    [
        (new LongRational(7, 3), new LongRational(1, 2), new LongRational(4), new LongRational(1, 3)),
        (new LongRational(-7, 3), new LongRational(1, 2), new LongRational(-4), new LongRational(-1, 3)),
        (new LongRational(7, 3), new LongRational(2, 3), new LongRational(3), new LongRational(1, 3))
    ];

    public static IEnumerable<object[]> DivRemTestCases()
        => DivRemCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DivRemTestCases))]
    public void DivRemMatchesDivisionAndRemainder(
        LongRational dividend,
        LongRational divisor,
        LongRational quotient,
        LongRational remainder)
    {
        var result = LongRational.DivRem(dividend, divisor, out var actualRemainder);

        Assert.Equal(quotient, result);
        Assert.Equal(remainder, actualRemainder);
        Assert.Equal(remainder, LongRational.Remainder(dividend, divisor));
    }

    public static List<(LongRational dividend, LongRational divisor)> DivRemIdentityCases =
    [
        (new LongRational(7, 3), new LongRational(1, 2)),
        (new LongRational(-7, 3), new LongRational(1, 2)),
        (new LongRational(7, 3), new LongRational(2, 3)),
        (new LongRational(5, 1), new LongRational(2, 1)),
        (new LongRational(0, 1), new LongRational(5, 1)),
        (new LongRational(1, 1), new LongRational(3, 1)),
        (new LongRational(-5, 1), new LongRational(2, 1)),
        (new LongRational(5, 1), new LongRational(-2, 1)),
        (new LongRational(-5, 1), new LongRational(-2, 1))
    ];

    public static IEnumerable<object[]> DivRemIdentityTestCases()
        => DivRemIdentityCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DivRemIdentityTestCases))]
    public void DivRemIdentity_Hold(LongRational dividend, LongRational divisor)
    {
        var quotient = LongRational.DivRem(dividend, divisor, out var remainder);
        Assert.True(quotient.IsInteger);
        Assert.Equal(dividend, quotient * divisor + remainder);
    }

    [Fact]
    public void DivRem_LargeDividend_ThrowsOverflow()
    {
        // this is a known limitation of the current implementation
        // the results would in fact fit in a long
        var dividend = new LongRational(long.MaxValue / 2, 1);
        var divisor = new LongRational(3, 5);
        Assert.Throws<OverflowException>(() => LongRational.DivRem(dividend, divisor, out _));
    }

    public static List<(long a, long b, long expectedGcd)> GcdLongCases =
    [
        (12, 8, 4),
        (7, 13, 1),
        (0, 5, 5),
        (-12, 8, 4)
    ];

    public static IEnumerable<object[]> GcdLongTestCases()
        => GcdLongCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GcdLongTestCases))]
    public void GreatestCommonDivisor_OfLongValues(long a, long b, long expectedGcd)
    {
        var result = LongRational.GreatestCommonDivisor(a, b);
        Assert.Equal(expectedGcd, result);
    }

    [Fact]
    public void GreatestCommonDivisor_WithMinValue_ThrowsOverflow()
    {
        // this is a known limitation of the current implementation
        // the result would be 1, but the algorithm relies on negating the value, which overflows
        Assert.Throws<OverflowException>(() => LongRational.GreatestCommonDivisor(long.MinValue, 1));
    }

    public static List<(LongRational left, LongRational right)> InfiniteRemainderCases =
    [
        (LongRational.PlusInfinity, LongRational.One),
        (LongRational.One, LongRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> InfiniteRemainderTestCases()
        => InfiniteRemainderCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteRemainderTestCases))]
    public void RemainderRejectsInfiniteOperands(LongRational left, LongRational right)
    {
        Assert.Throws<UndeterminedResultException>(() => left % right);
        Assert.Throws<UndeterminedResultException>(() => LongRational.Remainder(left, right));
    }

    public static List<(LongRational value, long exponent, LongRational expected)> PowerCases =
    [
        (new LongRational(2, 3), 3, new LongRational(8, 27)),
        (new LongRational(2, 3), -2, new LongRational(9, 4)),
        (new LongRational(-2), 3, new LongRational(-8)),
        (LongRational.Zero, 0, LongRational.One)
    ];

    public static IEnumerable<object[]> PowerTestCases()
        => PowerCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PowerTestCases))]
    public void PowHandlesPositiveNegativeAndZeroExponents(
        LongRational value,
        long exponent,
        LongRational expected)
    {
        Assert.Equal(expected, LongRational.Pow(value, exponent));
    }

    public static List<long> InvalidZeroPowerExponents =
    [
        -1
    ];

    public static IEnumerable<object[]> InvalidZeroPowerTestCases()
        => InvalidZeroPowerExponents.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidZeroPowerTestCases))]
    public void PowRejectsZeroWithNegativeExponent(long exponent)
    {
        Assert.Throws<ArgumentException>(() => LongRational.Pow(LongRational.Zero, exponent));
    }

    public static List<(LongRational left, LongRational right, LongRational gcd, LongRational lcm, long lcd)> CommonMeasureCases =
    [
        (new LongRational(3, 2), LongRational.One, new LongRational(1, 2), new LongRational(3), 2),
        (new LongRational(5, 6), new LongRational(7, 10), new LongRational(1, 30), new LongRational(35, 2), 30),
        (new LongRational(12), new LongRational(8), new LongRational(4), new LongRational(24), 1)
    ];

    public static IEnumerable<object[]> CommonMeasureTestCases()
        => CommonMeasureCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CommonMeasureTestCases))]
    public void GcdLcmAndLcdHandleFractionalValues(
        LongRational left,
        LongRational right,
        LongRational gcd,
        LongRational lcm,
        long lcd)
    {
        Assert.Equal(gcd, LongRational.GreatestCommonDivisor(left, right));
        Assert.Equal(lcm, LongRational.LeastCommonMultiple(left, right));
        Assert.Equal(lcd, LongRational.LeastCommonDenominator(left, right));
    }

    public static List<(LongRational value, string toString, string code)> FormattingCases =
    [
        (LongRational.Zero, "0/1", "0"),
        (new LongRational(3, 4), "3/4", "new Rational(3, 4)"),
        (new LongRational(-2), "-2/1", "-2"),
        (LongRational.PlusInfinity, "1/0", "new Rational(1, 0)"),
        (LongRational.MinusInfinity, "-1/0", "new Rational(-1, 0)")
    ];

    public static IEnumerable<object[]> FormattingTestCases()
        => FormattingCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(FormattingTestCases))]
    public void FormattingMethodsReturnStableRepresentations(
        LongRational value,
        string toString,
        string code)
    {
        Assert.Equal(toString, value.ToString());
        Assert.Equal(code, value.ToCodeString());
    }

    public static List<(LongRational value, object? other, int expectedSign)> ObjectComparisonCases =
    [
        (new LongRational(2, 3), null, 1),
        (new LongRational(2, 3), new LongRational(2, 3), 0),
        (new LongRational(2, 3), new LongRational(1, 3), 1),
        (new LongRational(2, 3), new LongRational(3, 4), -1)
    ];

    public static IEnumerable<object[]> ObjectComparisonTestCases()
        => ObjectComparisonCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ObjectComparisonTestCases))]
    public void ComparableObjectImplementationUsesRationalOrdering(
        LongRational value,
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
        Assert.Throws<ArgumentException>(() => ((IComparable)LongRational.One).CompareTo(other));
    }

    public static List<(LongRational value, bool isOne, bool isPositive, bool isNegative, bool isInteger)> PropertyCases =
    [
        (LongRational.One, true, true, false, true),
        (new LongRational(-3, 2), false, false, true, false),
        (LongRational.Zero, false, false, false, true)
    ];

    public static IEnumerable<object[]> PropertyTestCases()
        => PropertyCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(PropertyTestCases))]
    public void PropertiesReflectNormalizedState(
        LongRational value,
        bool isOne,
        bool isPositive,
        bool isNegative,
        bool isInteger)
    {
        Assert.Equal(isOne, value.IsOne);
        Assert.Equal(isPositive, value.IsPositive);
        Assert.Equal(isNegative, value.IsNegative);
        Assert.Equal(isInteger, value.IsInteger);
    }

    public static List<LongRational> InfiniteDecompositionCases =
    [
        LongRational.PlusInfinity,
        LongRational.MinusInfinity
    ];

    public static IEnumerable<object[]> InfiniteDecompositionTestCases()
        => InfiniteDecompositionCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InfiniteDecompositionTestCases))]
    public void DecompositionRejectsInfiniteValues(LongRational value)
    {
        Assert.Throws<UndeterminedResultException>(() => value.GetWholePart());
        Assert.Throws<UndeterminedResultException>(() => value.GetFractionPart());
        Assert.Throws<UndeterminedResultException>(() => value.Floor());
        Assert.Throws<UndeterminedResultException>(() => value.Ceil());
    }

    public static List<(LongRational value, long floor, long ceil)> RoundingBoundaryCases =
    [
        (new LongRational(7, 3), 2, 3),
        (new LongRational(-7, 3), -3, -2),
        (new LongRational(2), 2, 2)
    ];

    public static IEnumerable<object[]> RoundingBoundaryTestCases()
        => RoundingBoundaryCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RoundingBoundaryTestCases))]
    public void FloorAndCeilReturnExpectedBounds(LongRational value, long floor, long ceil)
    {
        Assert.Equal(floor, value.Floor());
        Assert.Equal(ceil, value.Ceil());
    }

    public static List<(LongRational left, LongRational right, LongRational sum, LongRational difference, LongRational product, LongRational quotient)> StaticArithmeticCases =
    [
        (
            new LongRational(3, 4),
            new LongRational(2, 5),
            new LongRational(23, 20),
            new LongRational(7, 20),
            new LongRational(3, 10),
            new LongRational(15, 8)
        )
    ];

    public static IEnumerable<object[]> StaticArithmeticTestCases()
        => StaticArithmeticCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(StaticArithmeticTestCases))]
    public void StaticArithmeticMethodsMatchOperators(
        LongRational left,
        LongRational right,
        LongRational sum,
        LongRational difference,
        LongRational product,
        LongRational quotient)
    {
        Assert.Equal(sum, LongRational.Add(left, right));
        Assert.Equal(difference, LongRational.Subtract(left, right));
        Assert.Equal(product, LongRational.Multiply(left, right));
        Assert.Equal(quotient, LongRational.Divide(left, right));
        Assert.Equal(+left, left);
    }

    public static List<(LongRational left, LongRational middle, LongRational right, LongRational minimum, LongRational maximum)> MinMaxOverloadCases =
    [
        (new LongRational(3, 4), LongRational.MinusOne, new LongRational(5), LongRational.MinusOne, new LongRational(5))
    ];

    public static IEnumerable<object[]> MinMaxOverloadTestCases()
        => MinMaxOverloadCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(MinMaxOverloadTestCases))]
    public void MinMaxOverloadsUseAllOperands(
        LongRational left,
        LongRational middle,
        LongRational right,
        LongRational minimum,
        LongRational maximum)
    {
        Assert.Equal(maximum, LongRational.Max(left, middle, right));
        Assert.Equal(maximum, LongRational.Max([left, middle, right]));
        Assert.Equal(minimum, LongRational.Min(left, middle, right));
        Assert.Equal(minimum, LongRational.Min([left, middle, right]));
    }

    public static List<(LongRational value, LongRational equalValue, object other)> EqualityAndHashCases =
    [
        (new LongRational(2, 3), new LongRational(4, 6), "not a rational")
    ];

    public static IEnumerable<object[]> EqualityAndHashTestCases()
        => EqualityAndHashCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(EqualityAndHashTestCases))]
    public void EqualityHashAndTypedCompareHandleEquivalentValues(
        LongRational value,
        LongRational equalValue,
        object other)
    {
        Assert.True(value.Equals(equalValue));
        Assert.False(value.Equals(null));
        Assert.False(value.Equals(other));
        Assert.Equal(0, value.CompareTo(equalValue));
        Assert.Equal(value.GetStableHashCode(), equalValue.GetStableHashCode());
        Assert.Equal(value.GetHashCode(), equalValue.GetHashCode());
    }
}
