using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class Compare
{
    public static List<(Rational a, Rational b, int expected)> KnownComparisons =
    [
        (new Rational(1, 2), new Rational(1, 3), 1),
        (new Rational(271616, 11), new Rational(30734848, 1243), -1),
        (new Rational(1388768, 113), 12292, -1),
    ];

    public static IEnumerable<object[]> CompareTestCases()
        => KnownComparisons.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CompareTestCases))]
    public void CompareTest(Rational a, Rational b, int expected)
    {
        var result = Rational.Compare(a, b);
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void Compare_Infinities()
    {
        Assert.True(Rational.PlusInfinity >= Rational.PlusInfinity);
        Assert.False(Rational.PlusInfinity > Rational.PlusInfinity);
        Assert.True(Rational.PlusInfinity > Rational.MinusInfinity);

        Assert.True(Rational.MinusInfinity <= Rational.MinusInfinity);
        Assert.False(Rational.MinusInfinity < Rational.MinusInfinity);
        Assert.True(Rational.MinusInfinity < Rational.PlusInfinity);
    }

    [Fact]
    public void Compare_FiniteWithInfinite()
    {
        Assert.True(Rational.PlusInfinity > 2);
        Assert.True(Rational.PlusInfinity > 0);
        Assert.True(Rational.PlusInfinity > -2);

        Assert.True(Rational.MinusInfinity < 2);
        Assert.True(Rational.MinusInfinity < 0);
        Assert.True(Rational.MinusInfinity < -2);
    }

    [Fact]
    public void Compare_Infinity_One()
    {
        Assert.True(Rational.PlusInfinity > Rational.One);
        Assert.True(Rational.MinusInfinity < Rational.One);
    }

    [Fact]
    public void Compare_Infinity_Zero()
    {
        Assert.True(Rational.PlusInfinity > Rational.Zero);
        Assert.True(Rational.Zero < Rational.PlusInfinity);
        Assert.True(Rational.MinusInfinity < Rational.Zero);
        Assert.True(Rational.Zero > Rational.MinusInfinity);
    }
}