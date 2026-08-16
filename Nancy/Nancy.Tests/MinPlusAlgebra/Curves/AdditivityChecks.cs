using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
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

    [Fact]
    public void IsSuperAdditive_IsNotAnsweredByAShortcut()
    {
        var notSuperAdditive = new StepCurve(value: 5, stepTime: 3);
        Assert.False(new Curve(notSuperAdditive).IsSuperAdditive);

        var shortcutting = new ShortcuttingMaxPlusConvolution(notSuperAdditive);

        Assert.False(shortcutting.IsSuperAdditive);
    }
}
