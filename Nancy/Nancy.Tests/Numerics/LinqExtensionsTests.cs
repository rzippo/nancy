using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics;

public class LinqExtensionsTests
{
    public static List<(List<Rational> values, Rational expected)> RationalSumCases =
    [
        ([new Rational(1, 2), new Rational(2, 3), new Rational(5, 6)], 2),
        ([Rational.PlusInfinity, 3, 5], Rational.PlusInfinity),
        ([Rational.MinusInfinity, -3, -5], Rational.MinusInfinity)
    ];

    public static IEnumerable<object[]> GetRationalSumCases()
        => RationalSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRationalSumCases))]
    public void RationalSum_ReturnsAggregate(List<Rational> values, Rational expected)
    {
        Assert.Equal(expected, values.Sum());
    }

    public static List<(List<int> values, Rational expected)> RationalSelectorSumCases =
    [
        ([1, 2, 3], 3),
        ([-2, 4, 8], 5)
    ];

    public static IEnumerable<object[]> GetRationalSelectorSumCases()
        => RationalSelectorSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetRationalSelectorSumCases))]
    public void RationalSelectorSum_ReturnsAggregate(List<int> values, Rational expected)
    {
        Assert.Equal(expected, values.Sum(value => new Rational(value, 2)));
    }

    [Fact]
    public void RationalSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<Rational>().Sum());
    }

    [Fact]
    public void RationalSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(Rational.Zero, new List<Rational>().Sum(defaultZero: true));
    }

    [Fact]
    public void RationalSelectorSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<int>().Sum(v => new Rational(v)));
    }

    [Fact]
    public void RationalSelectorSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(Rational.Zero, new List<int>().Sum(v => new Rational(v), defaultZero: true));
    }

    public static List<(List<LongRational> values, LongRational expected)> LongRationalSumCases =
    [
        ([new LongRational(1, 2), new LongRational(2, 3), new LongRational(5, 6)], 2),
        ([LongRational.PlusInfinity, 3, 5], LongRational.PlusInfinity),
        ([LongRational.MinusInfinity, -3, -5], LongRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> GetLongRationalSumCases()
        => LongRationalSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetLongRationalSumCases))]
    public void LongRationalSum_ReturnsAggregate(List<LongRational> values, LongRational expected)
    {
        Assert.Equal(expected, values.Sum());
    }

    public static List<(List<int> values, LongRational expected)> LongRationalSelectorSumCases =
    [
        ([1, 2, 3], 3),
        ([-2, 4, 8], 5)
    ];

    public static IEnumerable<object[]> GetLongRationalSelectorSumCases()
        => LongRationalSelectorSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetLongRationalSelectorSumCases))]
    public void LongRationalSelectorSum_ReturnsAggregate(List<int> values, LongRational expected)
    {
        Assert.Equal(expected, values.Sum(value => new LongRational(value, 2)));
    }

    [Fact]
    public void LongRationalSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<LongRational>().Sum());
    }

    [Fact]
    public void LongRationalSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(LongRational.Zero, new List<LongRational>().Sum(defaultZero: true));
    }

    [Fact]
    public void LongRationalSelectorSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<int>().Sum(v => new LongRational(v)));
    }

    [Fact]
    public void LongRationalSelectorSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(LongRational.Zero, new List<int>().Sum(v => new LongRational(v), defaultZero: true));
    }

    public static List<(List<BigRational> values, BigRational expected)> BigRationalSumCases =
    [
        ([new BigRational(1, 2), new BigRational(2, 3), new BigRational(5, 6)], 2),
        ([BigRational.PlusInfinity, 3, 5], BigRational.PlusInfinity),
        ([BigRational.MinusInfinity, -3, -5], BigRational.MinusInfinity)
    ];

    public static IEnumerable<object[]> GetBigRationalSumCases()
        => BigRationalSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBigRationalSumCases))]
    public void BigRationalSum_ReturnsAggregate(List<BigRational> values, BigRational expected)
    {
        Assert.Equal(expected, values.Sum());
    }

    public static List<(List<int> values, BigRational expected)> BigRationalSelectorSumCases =
    [
        ([1, 2, 3], 3),
        ([-2, 4, 8], 5)
    ];

    public static IEnumerable<object[]> GetBigRationalSelectorSumCases()
        => BigRationalSelectorSumCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetBigRationalSelectorSumCases))]
    public void BigRationalSelectorSum_ReturnsAggregate(List<int> values, BigRational expected)
    {
        Assert.Equal(expected, values.Sum(value => new BigRational(value, 2)));
    }

    [Fact]
    public void BigRationalSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<BigRational>().Sum());
    }

    [Fact]
    public void BigRationalSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(BigRational.Zero, new List<BigRational>().Sum(defaultZero: true));
    }

    [Fact]
    public void BigRationalSelectorSum_EmptyEnumerable_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new List<int>().Sum(v => new BigRational(v)));
    }

    [Fact]
    public void BigRationalSelectorSum_EmptyEnumerable_DefaultZero_ReturnsZero()
    {
        Assert.Equal(BigRational.Zero, new List<int>().Sum(v => new BigRational(v), defaultZero: true));
    }
}
