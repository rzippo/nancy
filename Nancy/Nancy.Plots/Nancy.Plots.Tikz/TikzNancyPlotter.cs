using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Plots.Tikz;

public class TikzNancyPlotter : NancyPlotter<TikzPlotSettings, TikzPlot, string>
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

    public override string PlotToOutput(TikzPlot plot)
    {
        return plot.ToTikzCode();
    }
}