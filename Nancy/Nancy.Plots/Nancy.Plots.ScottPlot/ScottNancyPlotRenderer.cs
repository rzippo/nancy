using ScottPlot;

namespace Unipi.Nancy.Plots.ScottPlot;

public class ScottNancyPlotRenderer : NancyPlotRenderer<ScottPlotSettings, Plot, byte[]>
{
    public override NancyPlotModeler<ScottPlotSettings, Plot> GetDefaultModeler()
    {
        return new ScottNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    public override byte[] PlotToOutput(Plot plot)
    {
        // todo: make format configurable?
        plot.ScaleFactor = PlotSettings.ScaleFactor;
        return plot.GetImageBytes(PlotSettings.Width, PlotSettings.Height, ImageFormat.Png);
    }
}