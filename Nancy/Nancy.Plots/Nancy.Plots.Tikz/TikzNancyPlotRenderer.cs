namespace Unipi.Nancy.Plots.Tikz;

/// <summary>
/// Renders Nancy plots as TikZ code.
/// </summary>
public class TikzNancyPlotRenderer : NancyPlotRenderer<TikzPlotSettings, TikzPlot, string>
{
    /// <inheritdoc />
    public override NancyPlotModeler<TikzPlotSettings, TikzPlot> GetDefaultModeler()
    {
        return new TikzNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    /// <inheritdoc />
    public override string PlotToOutput(TikzPlot plot)
    {
        return plot.ToTikzCode();
    }
}
