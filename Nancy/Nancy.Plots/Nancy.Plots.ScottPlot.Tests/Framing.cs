using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.ScottPlot;
using Xunit;

namespace Nancy.Plots.ScottPlot.Tests;

/// <summary>
/// Covers the shared framing semantics in the ScottPlot backend.
/// </summary>
public class Framing
{
    [Fact]
    public void ExplicitLimitsFrameWithTheDefaultMargins()
    {
        var modeler = new ScottNancyPlotModeler
        {
            PlotSettings = new ScottPlotSettings
            {
                XLimit = new Interval(0, 10),
                YLimit = new Interval(0, 10)
            }
        };
        var plot = modeler.GetPlot([RisingSequence()], ["f"]);

        // the requested window is the data limit, and the default signed margin frames it
        var limits = plot.Axes.GetLimits();
        Assert.Equal(-0.15, limits.Left, 1e-9);
        Assert.Equal(10.3, limits.Right, 1e-9);
        Assert.Equal(-0.15, limits.Bottom, 1e-9);
        Assert.Equal(10.3, limits.Top, 1e-9);
    }

    private static Sequence RisingSequence() => new(new Element[]
    {
        new Point(0, 0),
        new Segment(0, 5, 0, 1),
        new Point(5, 5)
    });
}
