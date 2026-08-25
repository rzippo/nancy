using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.Tikz;
using Unipi.Nancy.Utility;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

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
        var tikzCode = TikzPlots.ToTikzPlotCode(rl);
        
        // the curve is cut over [0, 5] and spans [0, 2], is then carried on to the right edge,
        // reaching 2.15 there, and the framing adds its margin on the side the values occupy
        // and half of it on the other
        Assert.Contains("xmin = -0.075,", tikzCode);
        Assert.Contains("ymin = -0.03225,", tikzCode);
        Assert.Contains("xmax = 5.15,", tikzCode);
        Assert.Contains("ymax = 2.2145,", tikzCode);
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    public void Test1_Settings()
    {
        var rl = new RateLatencyServiceCurve(1, 3);
        var tikzCode = TikzPlots.ToTikzPlotCode(rl, settings: new TikzPlotSettings
        {
            Title = "test static plotting"
        });
        
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var tikzCode = TikzPlots.ToTikzPlotCode([sc, ac]);
        
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    public void Test2_Settings()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var tikzCode = TikzPlots.ToTikzPlotCode([sc, ac], settings: new TikzPlotSettings
        {
            Title = "test static plotting"
        });
        
        _testOutputHelper.WriteLine(tikzCode);
    }

    [Fact]
    public void Test3()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var tikzCode = TikzPlots.ToTikzPlotCode([sc, ac], settings: new TikzPlotSettings
        {
            Title = "static negative xlim",
            XLimit = new Interval(-1, 10),
            YLimit = new Interval(-2, 30)
        });
        
        Assert.Contains("xmin = -1,", tikzCode);
        Assert.Contains("xmax = 10,", tikzCode);
        Assert.Contains("ymin = -2,", tikzCode);
        Assert.Contains("ymax = 30,", tikzCode);
        _testOutputHelper.WriteLine(tikzCode);
    }
}
