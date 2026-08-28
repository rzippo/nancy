using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.Tikz;
using Unipi.Nancy.Utility;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// Tests for the TikZ static plotting entry points.
/// </summary>
/// <remarks>
/// The cases in the Smoke category check only that the call completes and produces output; the rest assert on what is emitted.
/// </remarks>
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
        
        // the curve is ultimately affine, so it is cut over twice its transient, [0, 6], and
        // spans [0, 3]; it is then carried on to the right edge, and the framing adds its margin
        // on the side the values occupy and half of it on the other
        Assert.Contains("xmin = -0.09,", tikzCode);
        Assert.Contains("ymin = -0.0477,", tikzCode);
        Assert.Contains("xmax = 6.18,", tikzCode);
        Assert.Contains("ymax = 3.2754,", tikzCode);
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    [Trait("Category", "Smoke")]
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
    [Trait("Category", "Smoke")]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(3, 1);
        var ac = new SigmaRhoArrivalCurve(2, 2);
        var tikzCode = TikzPlots.ToTikzPlotCode([sc, ac]);
        
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    [Trait("Category", "Smoke")]
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

        // the explicit limits are the data window, and the default margin frames them
        Assert.Contains("xmin = -1.33,", tikzCode);
        Assert.Contains("xmax = 10.33,", tikzCode);
        Assert.Contains("ymin = -2.96,", tikzCode);
        Assert.Contains("ymax = 30.96,", tikzCode);
        _testOutputHelper.WriteLine(tikzCode);
    }
}
