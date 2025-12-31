namespace Unipi.Nancy.Plots.Tikz;

public class TikzNancyPlotRenderer : NancyPlotRenderer<TikzPlotSettings, TikzPlot, string>
{
    public override NancyPlotModeler<TikzPlotSettings, TikzPlot> GetDefaultModeler()
    {
        return new TikzNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    public override string PlotToOutput(TikzPlot plot)
    {
        return plot.ToTikzCode();
    }
}