using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class OperatorsTests
{
    [Fact]
    public void Negation()
    {
        Assert.Equal(BigRational.MinusInfinity, -BigRational.PlusInfinity);
        Assert.Equal(BigRational.PlusInfinity, -BigRational.MinusInfinity);
    }

    [Fact]
    public void Increment()
    {
        BigRational plusInfinity = BigRational.PlusInfinity;
        BigRational minusInfinity = BigRational.MinusInfinity;

        Assert.Equal(BigRational.PlusInfinity, ++plusInfinity);
        Assert.Equal(BigRational.MinusInfinity, ++minusInfinity);
    }

    [Fact]
    public void Decrement()
    {
        BigRational plusInfinity = BigRational.PlusInfinity;
        BigRational minusInfinity = BigRational.MinusInfinity;

        Assert.Equal(BigRational.PlusInfinity, --plusInfinity);
        Assert.Equal(BigRational.MinusInfinity, --minusInfinity);
    }

    [Fact]
    public void Equal()
    {
        Assert.True(BigRational.PlusInfinity == BigRational.PlusInfinity);
        Assert.True(BigRational.MinusInfinity == BigRational.MinusInfinity);

        Assert.False(BigRational.MinusInfinity == BigRational.PlusInfinity);

        Assert.False(BigRational.One == BigRational.MinusInfinity);
        Assert.False(BigRational.One == BigRational.PlusInfinity);
    }

    [Fact]
    public void CompareFinite()
    {
        BigRational a = new BigRational(12, 5);
        BigRational b_double = new BigRational(2.5);
        BigRational b_decimal = new BigRational(2.5M);

        Assert.True(b_decimal > a);
        Assert.True(b_double > a);
        Assert.Equal(b_decimal, b_double);
    }

    [Fact]
    public void Compare_Infinite()
    {
        Assert.True(BigRational.PlusInfinity >= BigRational.PlusInfinity);
        Assert.False(BigRational.PlusInfinity > BigRational.PlusInfinity);
        Assert.True(BigRational.PlusInfinity > BigRational.MinusInfinity);

        Assert.True(BigRational.MinusInfinity <= BigRational.MinusInfinity);
        Assert.False(BigRational.MinusInfinity < BigRational.MinusInfinity);
        Assert.True(BigRational.MinusInfinity < BigRational.PlusInfinity);
    }

    [Fact]
    public void Compare_FiniteWithInfinite()
    {
        Assert.True(BigRational.PlusInfinity > 2);
        Assert.True(BigRational.PlusInfinity > 0);
        Assert.True(BigRational.PlusInfinity > -2);

        Assert.True(BigRational.MinusInfinity < 2);
        Assert.True(BigRational.MinusInfinity < 0);
        Assert.True(BigRational.MinusInfinity < -2);
    }

    [Fact]
    public void Compare_One()
    {
        Assert.True(BigRational.PlusInfinity > BigRational.One);
        Assert.True(BigRational.MinusInfinity < BigRational.One);
    }

    [Fact]
    public void Compare_Zero()
    {
        Assert.True(BigRational.PlusInfinity > BigRational.Zero);
        Assert.True(BigRational.Zero < BigRational.PlusInfinity);
        Assert.True(BigRational.MinusInfinity < BigRational.Zero);
        Assert.True(BigRational.Zero > BigRational.MinusInfinity);
    }

    /// <summary>
    /// Pairs of values near the range of a long, whose cross products overflow it, and whether they are equal.
    /// </summary>
    public static List<(BigRational a, BigRational b, bool areEqual)> LargeEqualsCases =
    [
        (new BigRational(3, 4), new BigRational(long.MaxValue), false),
        (new BigRational(3, 4), new BigRational(long.MaxValue - 1, long.MaxValue), false),
        (new BigRational(long.MaxValue - 1, long.MaxValue), new BigRational(long.MaxValue - 1, long.MaxValue), true),
        (new BigRational(1, long.MaxValue), new BigRational(1, long.MaxValue - 1), false),
    ];

    public static IEnumerable<object[]> LargeEqualsTestCases()
        => LargeEqualsCases.ToXUnitTestCases();

    /// <summary>
    /// Equals of values near the range of a long agrees with the equality operator.
    /// </summary>
    [Theory]
    [MemberData(nameof(LargeEqualsTestCases))]
    public void EqualsOfLargeValues(BigRational a, BigRational b, bool areEqual)
    {
        Assert.Equal(areEqual, a.Equals(b));
        Assert.Equal(areEqual, a.Equals((object)b));
        Assert.Equal(areEqual, a == b);
    }
}
