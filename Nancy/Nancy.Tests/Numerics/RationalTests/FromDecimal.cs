using System.Collections.Generic;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class FromDecimal
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
    
    public static List<(decimal d, long numerator, long denominator)> ImpreciseDecimals =
    [
        (28m/150, 14, 75),
        (98m/600, 49, 300),
    ];
    
    public static IEnumerable<object[]> GetImpreciseDecimalsTestCases()
        => ImpreciseDecimals.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(GetKnownDecimalsTestCases))]
    public void DecimalCtorEquivalence(decimal d, long num, long den)
    {
        var r = new Rational(d);

        Assert.Equal(num, r.Numerator);
        Assert.Equal(den, r.Denominator);
    }
    
    [Theory]
    [MemberData(nameof(GetKnownDecimalsTestCases))]
    [MemberData(nameof(GetImpreciseDecimalsTestCases))]
    public void DecimalCtorApproximateEquivalence(decimal d, long num, long den)
    {
        var r = new Rational(d);
        var r_cast = (decimal)r;

        Assert.Equal(d, r_cast);
    }

}