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
        .Concat([
            Curve.Zero(),
            Curve.PlusInfinite(),
            Curve.MinusInfinite()
        ])
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
        Assert.True(Curve.EquivalentExceptOrigin(result, curve));
    }
    
    /// <summary>
    /// $f^\circ = \min(f, \delta_0)$ only lowers the origin, so a curve already at or below 0 there is
    /// returned as it is.
    /// <see cref="WithZeroOriginTest"/> cannot see this, since a forced origin would satisfy $f(0) \le 0$ too.
    /// </summary>
    [Theory]
    [MemberData(nameof(WithZeroOriginTestCases))]
    public void WithZeroOriginLeavesANonPositiveOriginAlone(Curve curve)
    {
        var result = curve.WithZeroOrigin();

        if (curve.ValueAt(0) <= 0)
            Assert.Same(curve, result);
        else
            Assert.Equal(0, result.ValueAt(0));
    }

    [Theory]
    [MemberData(nameof(WithOriginAtTestCases))]
    public void WithOriginAtTest(Curve curve, Rational value)
    {
        var result = curve.WithOriginAt(value);
        Assert.Equal(value, result.ValueAt(0));
        Assert.True(Curve.EquivalentExceptOrigin(result, curve));
    }
    
    [Theory]
    [MemberData(nameof(WithOriginRightContinuousTestCases))]
    public void WithOriginRightContinuousTest(Curve curve)
    {
        var result = curve.WithOriginRightContinuous();
        Assert.Equal(result.RightLimitAt(0), result.ValueAt(0));
        Assert.True(Curve.EquivalentExceptOrigin(result, curve));
    }
}