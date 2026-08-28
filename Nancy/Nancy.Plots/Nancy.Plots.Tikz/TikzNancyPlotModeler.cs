using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Plots.Tikz;

/// <summary>
/// Builds TikZ plot models from Nancy sequences.
/// </summary>
public class TikzNancyPlotModeler : NancyPlotModeler<TikzPlotSettings, TikzPlot>
{
    /// <inheritdoc />
    public override TikzPlot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names)
    {
        var tikzPlot = new TikzPlot(
            sequences.ToList(),
            names.ToList(),
            PlotSettings
        )
        {
            SequencesContinuePastEnd = SequencesContinuePastCut
        };
        return tikzPlot;
    }

    /// <inheritdoc />
    public override TikzPlot GetPlot(
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names)
    {
        var plot = base.GetPlot(curves, names);

        if (curves.Count == 1 && curves.Single() is { IsUltimatelyInfinite: false } f)
        {
            plot.AddUppMarks(f, names.First());
        }

        return plot;
    }
}
