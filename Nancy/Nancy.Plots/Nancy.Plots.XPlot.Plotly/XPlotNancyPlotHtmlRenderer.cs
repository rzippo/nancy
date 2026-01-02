using XPlot.Plotly;

namespace Unipi.Nancy.Plots.XPlot.Plotly;

public class XPlotNancyPlotHtmlRenderer : NancyPlotRenderer<XPlotPlotSettings, PlotlyChart, string>
{
    public override NancyPlotModeler<XPlotPlotSettings, PlotlyChart> GetDefaultModeler()
    {
        return new XPlotNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    public override string PlotToOutput(PlotlyChart plot)
    {
        return plot.GetHtml();
    }
}