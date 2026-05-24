using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.NetworkCalculus.CurveCtorTests;

public class SuperAdditiveCurve
{
    public static List<(Rational rate, Rational delay)> SuperAdditiveCurves =
    [
        (5, 3),
        (10, 7),
    ];

    public static IEnumerable<object[]> GetSuperAdditiveCurves()
        => SuperAdditiveCurves.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetSuperAdditiveCurves))]
    public void Constructor_FromSuperAdditiveCurve(Rational rate, Rational delay)
    {
        var rateLatency = new RateLatencyServiceCurve(rate, delay);
        var curve = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(rateLatency);

        Assert.True(curve.IsFinite);
        Assert.True(curve.IsSuperAdditive);
        Assert.True(curve.IsSuperAdditiveCheck());

        var closure = curve.SuperAdditiveClosure();
        Assert.Same(curve, closure);

        Assert.Equal(rateLatency.ValueAt(0), curve.ValueAt(0));
        Assert.Equal(rateLatency.ValueAt(delay), curve.ValueAt(delay));
        Assert.Equal(rateLatency.ValueAt(delay + 10), curve.ValueAt(delay + 10));
    }

    [Fact]
    public void Constructor_NoTest()
    {
        var rateLatency = new RateLatencyServiceCurve(5, 3);
        var curve = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(rateLatency, doTest: false);

        Assert.True(curve.IsSuperAdditive);
        Assert.True(curve.IsSuperAdditiveCheck());
    }

    [Fact]
    public void CopyConstructor()
    {
        var original = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(
            new RateLatencyServiceCurve(5, 3)
        );
        var copy = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(original, doTest: false);

        Assert.True(copy.IsFinite);
        Assert.True(copy.IsSuperAdditive);
        Assert.Equal(original.ValueAt(0), copy.ValueAt(0));
        Assert.Equal(original.ValueAt(10), copy.ValueAt(10));
    }

    [Fact]
    public void IsSuperAdditive_AlwaysTrue()
    {
        var curve = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(
            new RateLatencyServiceCurve(5, 3),
            doTest: false
        );

        Assert.True(curve.IsSuperAdditive);
    }

    [Fact]
    public void IsSuperAdditiveCheck_PassesForRateLatency()
    {
        var curve = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(
            new RateLatencyServiceCurve(5, 3)
        );

        Assert.True(curve.IsSuperAdditiveCheck());
    }

    [Fact]
    public void NonSuperAdditiveCurve_Throws()
    {
        var stepCurve = new StepCurve(value: 5, stepTime: 3);

        Assert.Throws<InvalidOperationException>(() =>
            new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(stepCurve, doTest: true)
        );
    }

    [Fact]
    public void NonSuperAdditiveCurve_SkipTest()
    {
        var stepCurve = new StepCurve(value: 5, stepTime: 3);
        var curve = new Unipi.Nancy.NetworkCalculus.SuperAdditiveCurve(stepCurve, doTest: false);

        Assert.True(curve.IsSuperAdditive);
        Assert.False(curve.IsSuperAdditiveCheck());
    }
}
