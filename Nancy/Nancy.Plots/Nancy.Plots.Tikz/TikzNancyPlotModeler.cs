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
        );
        return tikzPlot;
    }
}
