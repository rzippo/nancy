using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.XPlot.Plotly;
using Unipi.Nancy.Utility;
using Xunit.Abstractions;

namespace Nancy.Plots.XPlot.Plotly.Tests;

public class StaticMethods
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StaticMethods(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var html = XPlotPlots.ToXPlotHtml(rl);
        
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test1_Settings()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var html = XPlotPlots.ToXPlotHtml(rl, settings: new XPlotPlotSettings
        {
            Title = "test static plotting"
        });
        
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var html = XPlotPlots.ToXPlotHtml([sc, ac]);
        
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test2_Settings()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var html = XPlotPlots.ToXPlotHtml([sc, ac], settings: new XPlotPlotSettings
        {
            Title = "test static plotting"
        });
        
        var hash = html.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.html");
        File.WriteAllText(path, html);
        _testOutputHelper.WriteLine(path);
    }
}