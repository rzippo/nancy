using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Plots.Tikz;
using Xunit.Abstractions;

namespace Nancy.Plots.Tikz.Tests;

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
        var tikzPlotter = new TikzNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var tikzCode = tikzPlotter.Plot(rl);
        _testOutputHelper.WriteLine(tikzCode);
    }
    
    [Fact]
    public void Test2()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var tikzPlotter = new TikzNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
            }
        };
        var tikzCode = tikzPlotter.Plot([sc, ac]);
        _testOutputHelper.WriteLine(tikzCode);
    }
}