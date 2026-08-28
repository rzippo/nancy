using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.Tikz;
using Xunit;

namespace Nancy.Plots.Tikz.Tests;

/// <summary>
/// Smoke tests for the TikZ instance plotting entry points.
/// </summary>
/// <remarks>
/// These check that the call completes and produces output, not what the output contains.
/// They are marked with the Smoke category so they can be told apart from the tests that assert.
/// </remarks>
public class InstanceMethods
{
    private readonly ITestOutputHelper _testOutputHelper;

    public InstanceMethods(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    [Trait("Category", "Smoke")]
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
    [Trait("Category", "Smoke")]
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

    [Fact]
    [Trait("Category", "Smoke")]
    public void Test3()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var tikzPlotter = new TikzNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "negative xlim",
                XLimit = new Interval(-1, 10)
            }
        };
        var tikzCode = tikzPlotter.Plot([sc, ac]);
        _testOutputHelper.WriteLine(tikzCode);
    }
}