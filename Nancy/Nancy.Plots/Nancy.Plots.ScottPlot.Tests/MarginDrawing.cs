using System.Collections.Generic;
using System.Linq;
using ScottPlot.Plottables;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.ScottPlot;
using Xunit;

namespace Nancy.Plots.ScottPlot.Tests;

/// <summary>
/// What is drawn between the requested x-limit and the frame.
/// </summary>
/// <remarks>
/// The same rules the Tikz backend is held to, since both draw from the shared layer:
/// the strip past the data limit shows the curve rather than a projection of it,
/// the frame is not marked as an end, and a sequence given directly stops where it stops.
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

    private static ScottNancyPlotModeler Modeler(Rational upper) => new()
    {
        PlotSettings = new ScottPlotSettings { XLimit = new Interval(0, upper) }
    };

    [Fact]
    public void ACurveIsDrawnFromTheCurveWhereTheLimitLandsOnABreakpoint()
    {
        var curve = RateChangeAtOne();

        var plot = Modeler(1).GetPlot(curve, "h");

        var drawn = Lines(plot);
        Assert.NotEmpty(drawn);
        foreach (var (x, y) in drawn)
            Assert.Equal((double)curve.ValueAt((Rational)(decimal)x), y, 1e-9);
        // the frame is at 1.03, where the curve is 1.6
        Assert.Equal(1.6, drawn[^1].y, 1e-9);
    }

    /// <summary>
    /// The dot at the end of a line says the function ends there, and at the frame it does not.
    /// </summary>
    [Fact]
    public void ACurveIsNotMarkedWhereThePlotEnds()
    {
        var plot = Modeler(1).GetPlot(RateChangeAtOne(), "h");

        Assert.DoesNotContain(1.03, Marks(plot).Select(p => p.x));
    }

    /// <summary>
    /// Nothing is known past a sequence's end, so it is neither drawn nor carried to the frame.
    /// </summary>
    [Fact]
    public void ASequenceIsDrawnOnlyToItsOwnEnd()
    {
        var sequence = RateChangeAtOne().Cut(0, 1);

        var plot = Modeler(1).GetPlot(sequence, "s");

        var drawn = Lines(plot);
        Assert.Equal(1.0, drawn[^1].x, 1e-9);
        Assert.DoesNotContain(drawn, p => p.x > 1.0);
    }

    /// <summary>
    /// A finite run that stops before the frame stops because the curve does, here because it goes infinite,
    /// so it keeps the dot that says so.
    /// Only the run reaching the frame loses one.
    /// </summary>
    [Fact]
    public void AFiniteRunEndingBeforeTheFrameKeepsItsMark()
    {
        var plot = Modeler(8).GetPlot(new DelayServiceCurve(3), "d");

        Assert.Contains((3.0, 0.0), Marks(plot));
    }

    /// <summary>
    /// The coordinates are doubles, and a frame at 103/300 is not one a decimal holds exactly,
    /// so recognising the frame by converting a coordinate back to a rational works only for tidy limits.
    /// </summary>
    [Fact]
    public void ACurveIsNotMarkedAtAFrameThatIsNotATerminatingDecimal()
    {
        var plot = Modeler(new Rational(1, 3)).GetPlot(RateChangeAtOne(), "h");

        var frameUpper = (double)PlotXWindow
            .ForCurves([RateChangeAtOne()], new PlotSettings { XLimit = new Interval(0, new Rational(1, 3)) })
            .Frame.Upper;
        Assert.DoesNotContain(frameUpper, Marks(plot).Select(p => p.x));
    }

    #region Reading the plottables

    /// <summary>
    /// ScottPlot draws the lines and the dots as separate scatters, told apart by whether the line has width.
    /// </summary>
    private static List<(double x, double y)> Lines(global::ScottPlot.Plot plot)
        => Scatters(plot, markersOnly: false);

    private static List<(double x, double y)> Marks(global::ScottPlot.Plot plot)
        => Scatters(plot, markersOnly: true);

    private static List<(double x, double y)> Scatters(global::ScottPlot.Plot plot, bool markersOnly)
        => plot.GetPlottables()
            .OfType<Scatter>()
            .Where(s => (s.LineWidth == 0) == markersOnly)
            .SelectMany(s => s.Data.GetScatterPoints().Select(p => (p.X, p.Y)))
            .ToList();

    #endregion
}
