using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.ScottPlot;
using Unipi.Nancy.Utility;
using Xunit.Abstractions;

namespace Nancy.Plots.ScottPlot.Tests;

public class InstanceMethods
{
    private readonly ITestOutputHelper _testOutputHelper;

    public InstanceMethods(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        var rl = new RateLatencyServiceCurve(1, 1);
        var scottPlotter = new ScottNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var bytes = scottPlotter.Plot(rl);
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var scottPlotter = new ScottNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var bytes = scottPlotter.Plot([sc, ac]);
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }

    [Fact]
    public void Test3()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var scottPlotter = new ScottNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "negative xlim",
                XLimit = new Interval(-1, 10)
            }
        };
        var bytes = scottPlotter.Plot([sc, ac]);
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
}