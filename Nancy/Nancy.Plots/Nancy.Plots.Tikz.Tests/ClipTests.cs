using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// Covers the clipping of curves to the axis limits.
/// </summary>
public class ClipTests
{
    [Fact]
    public void ClippingToLimitsIsTheDefault()
    {
        var code = TikzPlots.ToTikzPlotCode(
            new SigmaRhoArrivalCurve(sigma: 1, rho: 2), "g",
            new TikzPlotSettings
            {
                XLimit = new Interval(0, 10, true, true),
                YLimit = new Interval(0, 10, true, true)
            });

        Assert.Contains("clip = true", code);
        // the requested window is the data limit, and the default margin frames it
        Assert.Contains("ymax = 10.3", code);
    }

    [Fact]
    public void ClipStrategyOffEmitsNoClipping()
    {
        var code = TikzPlots.ToTikzPlotCode(
            new SigmaRhoArrivalCurve(sigma: 1, rho: 2), "g",
            new TikzPlotSettings
            {
                ClipStrategy = ClipStrategy.Off,
                XLimit = new Interval(0, 10, true, true),
                YLimit = new Interval(0, 10, true, true)
            });

        Assert.Contains("clip = false", code);
    }
}
