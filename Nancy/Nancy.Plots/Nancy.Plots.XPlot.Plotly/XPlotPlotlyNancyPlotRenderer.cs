using XPlot.Plotly;

namespace Unipi.Nancy.Plots.XPlot.Plotly;

public class XPlotPlotlyNancyPlotHtmlRenderer : NancyPlotRenderer<XPlotPlotlyPlotSettings, PlotlyChart, string>
{
    public override NancyPlotModeler<XPlotPlotlyPlotSettings, PlotlyChart> GetDefaultModeler()
    {
        return new XPlotPlotlyNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    public override string PlotToOutput(PlotlyChart plot)
    {
        return plot.GetHtml();
    }
}