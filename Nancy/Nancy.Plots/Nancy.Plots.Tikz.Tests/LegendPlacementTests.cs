using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// Covers the legend placement, and the line styles carried into the emitted code.
/// </summary>
/// <remarks>
/// Placing the legend outside the axis is what rules the overlap out rather than making it unlikely,
/// and the point of ruling it out is that it can be checked here instead of by looking at a rendering.
/// </remarks>
public class LegendPlacementTests
{
    private static readonly IReadOnlyList<Curve> TwoCurves =
        [new RateLatencyServiceCurve(1, 3), new SigmaRhoArrivalCurve(2, 1)];

    private static readonly IEnumerable<string> TwoNames = new List<string> { "f", "g" };

    [Fact]
    public void InsideIsTheDefaultAndUsesLegendPos()
    {
        var code = TikzPlots.ToTikzPlotCode(TwoCurves, TwoNames);

        Assert.Contains("legend pos =", code);
        Assert.DoesNotContain("legend style =", code);
    }

    [Fact]
    public void OutsideAnchorsAgainstTheAxisBox()
    {
        var code = TikzPlots.ToTikzPlotCode(
            TwoCurves, TwoNames,
            new TikzPlotSettings { LegendPlacement = LegendPlacement.Outside });

        // anchored past the right edge of the axis, so no legend size can bring it back inside
        Assert.Contains("legend style = { at = {(1.05,0)}, anchor = south west }", code);
        Assert.DoesNotContain("legend pos =", code);
    }

    [Fact]
    public void LineStylesCycleAcrossCurves()
    {
        var code = TikzPlots.ToTikzPlotCode(TwoCurves, TwoNames);

        Assert.Contains("solid", code);
        Assert.Contains("dashed", code);
    }

    [Fact]
    public void UseLineStylesFalseDrawsEverythingSolid()
    {
        var code = TikzPlots.ToTikzPlotCode(
            TwoCurves, TwoNames,
            new TikzPlotSettings { UseLineStyles = false });

        Assert.Contains("solid", code);
        Assert.DoesNotContain("dashed", code);
        Assert.DoesNotContain("dotted", code);
    }

    [Fact]
    public void RawLineStylesWinOverTheSharedCycle()
    {
        var code = TikzPlots.ToTikzPlotCode(
            TwoCurves, TwoNames,
            new TikzPlotSettings { RawLineStyles = ["densely dash dot dot"] });

        Assert.Contains("densely dash dot dot", code);
    }

    [Fact]
    public void EqualScaleAxesIsOptIn()
    {
        Assert.DoesNotContain("axis equal image", TikzPlots.ToTikzPlotCode(TwoCurves, TwoNames));
        Assert.Contains(
            "axis equal image",
            TikzPlots.ToTikzPlotCode(
                TwoCurves, TwoNames, new TikzPlotSettings { SameScaleAxes = true }));
    }

    [Fact]
    public void InfinityAreasAreEmittedForADelayCurve()
    {
        var code = TikzPlots.ToTikzPlotCode(new DelayServiceCurve(10), "delta");

        Assert.Contains("\\fill [ pattern =", code);
        Assert.Contains("+\\infty", code);
    }

    [Fact]
    public void IgnoreEmitsNoAreas()
    {
        var code = TikzPlots.ToTikzPlotCode(
            new DelayServiceCurve(10), "delta",
            new TikzPlotSettings { InfinityStrategy = InfinityStrategy.Ignore });

        Assert.DoesNotContain("\\fill [ pattern =", code);
        Assert.DoesNotContain("\\infty", code);
    }
}
