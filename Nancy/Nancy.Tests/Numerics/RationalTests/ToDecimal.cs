using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class ToDecimal
{
    public static List<(decimal d, long numerator, long denominator)> KnownDecimals =
    [
        (128.3m, 1283, 10),
        (0.5m, 1, 2),
        (27m/150, 9, 50),
        (144m/400, 9, 25),
        (0.733333333333333m, 733_333_333_333_333, 1_000_000_000_000_000),
        (0.7333333333333333m, 7_333_333_333_333_333, 10_000_000_000_000_000),
    ];

    public static IEnumerable<object[]> GetKnownDecimalsTestCases()
        => KnownDecimals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownDecimalsTestCases))]
    public void DecimalCastEquivalence(decimal d, long num, long den)
    {
        var r = new Rational(num, den);
        var dCast = (decimal)r;
        
        Assert.Equal(d, dCast);
    }

    [Fact]
    public void PlusInfiniteCastTest()
    {
        var r = Rational.PlusInfinity;
        Assert.Throws<InvalidConversionException>(() => (decimal)r);
    }
    
    [Fact]
    public void MinusInfiniteCastTest()
    {
        var r = Rational.MinusInfinity;
        Assert.Throws<InvalidConversionException>(() => (decimal)r);
    }
}