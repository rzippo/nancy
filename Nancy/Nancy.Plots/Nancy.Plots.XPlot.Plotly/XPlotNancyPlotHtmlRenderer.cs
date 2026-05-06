using XPlot.Plotly;

namespace Unipi.Nancy.Plots.XPlot.Plotly;

/// <summary>
/// Renders Nancy plots as XPlot.Plotly HTML.
/// </summary>
public class XPlotNancyPlotHtmlRenderer : NancyPlotRenderer<XPlotPlotSettings, PlotlyChart, string>
{
    /// <inheritdoc />
    public override NancyPlotModeler<XPlotPlotSettings, PlotlyChart> GetDefaultModeler()
    {
        return new XPlotNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    /// <inheritdoc />
    public override string PlotToOutput(PlotlyChart plot)
    {
        return plot.GetHtml();
    }
}
