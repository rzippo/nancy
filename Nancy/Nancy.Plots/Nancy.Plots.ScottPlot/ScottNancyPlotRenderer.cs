using ScottPlot;

namespace Unipi.Nancy.Plots.ScottPlot;

/// <summary>
/// Renders Nancy plots as ScottPlot PNG images.
/// </summary>
public class ScottNancyPlotRenderer : NancyPlotRenderer<ScottPlotSettings, Plot, byte[]>
{
    /// <inheritdoc />
    public override NancyPlotModeler<ScottPlotSettings, Plot> GetDefaultModeler()
    {
        return new ScottNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    /// <inheritdoc />
    public override byte[] PlotToOutput(Plot plot)
    {
        // todo: make format configurable?
        plot.ScaleFactor = PlotSettings.ScaleFactor;
        return plot.GetImageBytes(PlotSettings.Width, PlotSettings.Height, ImageFormat.Png);
    }
}
