using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.XPlot.Plotly;
using Xunit;

namespace Nancy.Plots.XPlot.Plotly.Tests;

/// <summary>
/// What is drawn between the requested x-limit and the frame.
/// </summary>
/// <remarks>
/// This backend never drew the strip past the data limit at all,
/// leaving the 3% blank while ScottPlot and Tikz filled it with a projection.
/// Sampling to the frame is what puts the curve there.
/// It marks every breakpoint, the one at the frame included, since a mark does not stand for an end in this backend.
/// </remarks>
public class MarginDrawing
{
    private static Curve RateChangeAtOne() => new Curve(
        baseSequence: new Sequence([
            Point.Origin(),
            new Segment(0, 1, 0, 1),
            new Point(1, 1),
            new Segment(1, 2, 1, 20)
        ]),
        pseudoPeriodStart: 1,
        pseudoPeriodLength: 1,
        pseudoPeriodHeight: 20
    );

    private static XPlotPlotSettings Until(Rational upper)
        => new() { XLimit = new Interval(0, upper) };

    [Fact]
    public void ACurveIsDrawnToTheFrameFollowingTheCurve()
    {
        var html = XPlotPlots.ToXPlotHtml(RateChangeAtOne(), "h", Until(1));

        // the frame is at 1.03, where the curve is 1.6
        Assert.Contains("\"x\":[0.0,1.0,1.03],\"y\":[0.0,1.0,1.6]", html);
    }

    /// <summary>
    /// Nothing is known past a sequence's end, so the strip past it stays empty.
    /// </summary>
    [Fact]
    public void ASequenceIsDrawnOnlyToItsOwnEnd()
    {
        var html = XPlotPlots.ToXPlotHtml(RateChangeAtOne().Cut(0, 1), "s", Until(1));

        Assert.Contains("\"x\":[0.0,1.0],\"y\":[0.0,1.0]", html);
        Assert.Contains("\"range\":[-0.015,1.03]", html);
    }
}
