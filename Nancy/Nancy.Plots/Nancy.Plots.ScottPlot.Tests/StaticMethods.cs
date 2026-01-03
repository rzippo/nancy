using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.ScottPlot;
using Unipi.Nancy.Utility;
using Xunit.Abstractions;

namespace Nancy.Plots.ScottPlot.Tests;

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
        var bytes = ScottPlots.ToScottPlotImage(rl);
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test1_Settings()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var bytes = ScottPlots.ToScottPlotImage(rl, settings: new ScottPlotSettings
        {
            Title = "test static plotting"
        });
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var bytes = ScottPlots.ToScottPlotImage([sc, ac]);
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test2_Settings()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var bytes = ScottPlots.ToScottPlotImage([sc, ac], settings: new ScottPlotSettings
        {
            Title = "test static plotting"
        });
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
    
    [Fact]
    public void Test3()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var bytes = ScottPlots.ToScottPlotImage([sc, ac], settings: new ScottPlotSettings
        {
            Title = "static negative xlim",
            XLimit = new Interval(-1, 10)
        });
        
        var hash = bytes.GetStableHashCode();
        var hashHex = hash.ToString("X");
        var path = Path.GetFullPath($"{hashHex}.png");
        File.WriteAllBytes(path, bytes);
        _testOutputHelper.WriteLine(path);
    }
}