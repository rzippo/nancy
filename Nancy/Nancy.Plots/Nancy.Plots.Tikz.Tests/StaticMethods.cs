using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.Tikz;
using Unipi.Nancy.Utility;
using Xunit.Abstractions;

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
}