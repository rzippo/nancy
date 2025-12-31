using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Plots.Tikz;

public class TikzNancyPlotModeler : NancyPlotModeler<TikzPlotSettings, TikzPlot>
{
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