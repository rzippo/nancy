using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class ToFloat
{
    public static List<(float d, long numerator, long denominator)> KnownFloats =
    [
        (128.3f, 1283, 10),
        (0.5f, 1, 2),
        (27.0f/150, 9, 50),
        (144.0f/400, 9, 25),
        (0.733333333333333f, 733_333_333_333_333, 1_000_000_000_000_000),
        (0.7333333333333333f, 7_333_333_333_333_333, 10_000_000_000_000_000),
    ];

    public static IEnumerable<object[]> GetKnownFloatsTestCases()
        => KnownFloats.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownFloatsTestCases))]
    public void FloatCastEquivalence(float d, long num, long den)
    {
        var r = new Rational(num, den);
        var dCast = (float)r;
        
        Assert.Equal(d, dCast);
    }

    [Fact]
    public void PlusInfiniteCastTest()
    {
        var r = Rational.PlusInfinity;
        var dCast = (float)r;
        Assert.Equal(float.PositiveInfinity, dCast);
    }
    
    [Fact]
    public void MinusInfiniteCastTest()
    {
        var r = Rational.MinusInfinity;
        var dCast = (float)r;
        Assert.Equal(float.NegativeInfinity, dCast);
    }
}