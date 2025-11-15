using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class ToDouble
{
    public static List<(double d, long numerator, long denominator)> KnownDoubles =
    [
        (128.3, 1283, 10),
        (0.5, 1, 2),
        (27.0/150, 9, 50),
        (144.0/400, 9, 25),
        (0.733333333333333, 733_333_333_333_333, 1_000_000_000_000_000),
        (0.7333333333333333, 7_333_333_333_333_333, 10_000_000_000_000_000),
    ];

    public static IEnumerable<object[]> GetKnownDoublesTestCases()
        => KnownDoubles.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownDoublesTestCases))]
    public void DoubleCastEquivalence(double d, long num, long den)
    {
        var r = new Rational(num, den);
        var dCast = (double)r;
        
        Assert.Equal(d, dCast);
    }

    [Fact]
    public void PlusInfiniteCastTest()
    {
        var r = Rational.PlusInfinity;
        var dCast = (double)r;
        Assert.Equal(double.PositiveInfinity, dCast);
    }
    
    [Fact]
    public void MinusInfiniteCastTest()
    {
        var r = Rational.MinusInfinity;
        var dCast = (double)r;
        Assert.Equal(double.NegativeInfinity, dCast);
    }
}