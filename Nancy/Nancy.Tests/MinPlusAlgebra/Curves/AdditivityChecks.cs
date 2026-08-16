using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class AdditivityChecks
{
    /// <summary>
    /// A curve that answers the min-plus convolution from its own claim, as <see cref="SubAdditiveCurve"/> does.
    /// </summary>
    private class ShortcuttingConvolution : Curve
    {
        public ShortcuttingConvolution(Curve other) : base(other)
        {
        }

        public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
            => this;
    }

    /// <summary>
    /// A curve that answers the max-plus convolution from its own claim.
    /// </summary>
    private class ShortcuttingMaxPlusConvolution : Curve
    {
        public ShortcuttingMaxPlusConvolution(Curve other) : base(other)
        {
        }

        public override Curve MaxPlusConvolution(Curve curve, ComputationSettings? settings = null)
            => this;
    }

    [Fact]
    public void IsSubAdditive_IsNotAnsweredByAShortcut()
    {
        var notSubAdditive = new StepCurve(value: 5, stepTime: 3);
        Assert.False(new Curve(notSubAdditive).IsSubAdditive);

        var shortcutting = new ShortcuttingConvolution(notSubAdditive);

        Assert.False(shortcutting.IsSubAdditive);
    }

    /// <summary>
    /// A rate-latency curve lowered at the origin only: super-additive, since $f(t+s) \ge f(t) + f(s)$ needs $f(0) \le 0$,
    /// but not with $f(0) = 0$.
    /// </summary>
    private static Curve SuperAdditiveNotAtOrigin
        => new Curve(new RateLatencyServiceCurve(2, 3)).WithOriginAt(-1);

    /// <summary>
    /// A constant curve raised at the origin only: sub-additive, which needs $f(0) \ge 0$, but not with $f(0) = 0$.
    /// </summary>
    private static Curve SubAdditiveNotAtOrigin
        => new Curve(new ConstantCurve(5)).WithOriginAt(5);

    [Fact]
    public void IsSuperAdditive_HoldsWhenTheOriginIsBelowZero()
    {
        var curve = SuperAdditiveNotAtOrigin;

        // the property only requires f(0) <= 0, which the definition confirms on a grid
        foreach (var t in new Rational[] { 0, 1, 2, 3, 4, 6 })
        foreach (var s in new Rational[] { 0, 1, 2, 3, 4, 6 })
            Assert.True(curve.ValueAt(t + s) >= curve.ValueAt(t) + curve.ValueAt(s));

        Assert.True(curve.IsSuperAdditive);
        Assert.False(curve.IsRegularSuperAdditive);
    }

    [Fact]
    public void IsSubAdditive_HoldsWhenTheOriginIsAboveZero()
    {
        var curve = SubAdditiveNotAtOrigin;

        Assert.True(curve.IsSubAdditive);
        Assert.False(curve.IsRegularSubAdditive);
    }

    [Fact]
    public void AdditivityTypes_RequireTheOrigin()
    {
        // both types are documented as regular, and the optimizations written against them rely on it
        Assert.Throws<InvalidOperationException>(() => new SuperAdditiveCurve(SuperAdditiveNotAtOrigin, doTest: true));
        Assert.Throws<InvalidOperationException>(() => new SubAdditiveCurve(SubAdditiveNotAtOrigin, doTest: true));

        Assert.False(new SuperAdditiveCurve(SuperAdditiveNotAtOrigin, doTest: false).IsRegularSuperAdditiveCheck());
        Assert.False(new SubAdditiveCurve(SubAdditiveNotAtOrigin, doTest: false).IsRegularSubAdditiveCheck());
    }

    [Fact]
    public void IsSuperAdditive_IsNotAnsweredByAShortcut()
    {
        var notSuperAdditive = new StepCurve(value: 5, stepTime: 3);
        Assert.False(new Curve(notSuperAdditive).IsSuperAdditive);

        var shortcutting = new ShortcuttingMaxPlusConvolution(notSuperAdditive);

        Assert.False(shortcutting.IsSuperAdditive);
    }
}
