using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class WithOriginAt
{
    public static List<Curve> Curves = ToLowerNonDecreasing.KnownPairs
        .SelectMany(p => new List<Curve>{p.operand, p.expected})
        .ToList();

    public static List<Rational> Values = Enumerable.Range(-3, 6)
        .Select(v => new Rational(v))
        .ToList();

    public static IEnumerable<object[]> WithZeroOriginTestCases()
        => Curves.ToXUnitTestCases();
    
    public static IEnumerable<object[]> WithOriginAtTestCases()
        => Curves
            .SelectMany(c => Values.Select(v => (c, v)))
            .ToXUnitTestCases();

    public static IEnumerable<object[]> WithOriginRightContinuousTestCases()
        => Curves.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(WithZeroOriginTestCases))]
    public void WithZeroOriginTest(Curve curve)
    {
        var result = curve.WithZeroOrigin();
        Assert.True(result.ValueAt(0) <= 0);
    }
    
    [Theory]
    [MemberData(nameof(WithOriginAtTestCases))]
    public void WithOriginAtTest(Curve curve, Rational value)
    {
        var result = curve.WithOriginAt(value);
        Assert.Equal(value, result.ValueAt(0));
    }
    
    [Theory]
    [MemberData(nameof(WithOriginRightContinuousTestCases))]
    public void WithOriginRightContinuousTest(Curve curve)
    {
        var result = curve.WithOriginRightContinuous();
        Assert.Equal(result.RightLimitAt(0), result.ValueAt(0));
    }
}