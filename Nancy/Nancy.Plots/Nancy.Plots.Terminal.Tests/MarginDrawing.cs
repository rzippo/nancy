using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots;
using Unipi.Nancy.Plots.Terminal;
using Xunit;

namespace Nancy.Plots.Terminal.Tests;

/// <summary>
/// What is drawn between the requested x-limit and the frame.
/// </summary>
/// <remarks>
/// Like the Plotly backend, this one left the strip past the data limit blank rather than projecting into it,
/// so sampling to the frame is what puts a curve there at all.
/// The axis has to hold what is drawn, which is the visible consequence here.
///
/// The tick filter this backend shares with Tikz is not pinned here, and cannot be:
/// a breakpoint in the margin sits within 3% of the right edge, where the axis already writes its own end label,
/// and <c>TryPlaceLabel</c> declines to overwrite it. Tikz covers that rule.
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

    private static TerminalPlotSettings Until(Rational upper)
        => new() { XLimit = new Interval(0, upper) };

    /// <summary>
    /// The curve reaches 1.6 at the frame, so the axis has to reach past it.
    /// Drawn from a projection of the old rate it would have reached only 1.03.
    /// </summary>
    [Fact]
    public void TheAxisHoldsWhatTheMarginDraws()
    {
        var markup = TerminalPlots.ToTerminalPlot(RateChangeAtOne(), "h", Until(1));

        Assert.Contains("1.6", markup);
        Assert.DoesNotContain("1.061", markup);
    }

    /// <summary>
    /// A sequence is not sampled past its end, so the axis has only its own values to hold.
    /// </summary>
    [Fact]
    public void ASequenceIsNotDrawnPastItsOwnEnd()
    {
        var markup = TerminalPlots.ToTerminalPlot(RateChangeAtOne().Cut(0, 1), "s", Until(1));

        Assert.DoesNotContain("1.6", markup);
    }
}
