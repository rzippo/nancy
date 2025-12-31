using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.XPlot.Plotly;
using Unipi.Nancy.Utility;
using Xunit.Abstractions;

namespace Nancy.Plots.XPlot.Plotly.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var rl = new RateLatencyServiceCurve(1, 1);
        var xplotPlotter = new XPlotPlotlyNancyPlotHtmlRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var html = xplotPlotter.Plot(rl);
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);

    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var xplotPlotter = new XPlotPlotlyNancyPlotHtmlRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var html = xplotPlotter.Plot([sc, ac]);
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }
}