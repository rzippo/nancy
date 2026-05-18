using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Plots.Terminal;
using Xunit;

namespace Nancy.Plots.Terminal.Tests;

public class InstanceMethods
{
    private readonly ITestOutputHelper _testOutputHelper;

    public InstanceMethods(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RendererPlotsCurve()
    {
        var rl = new RateLatencyServiceCurve(1, 1);
        var terminalPlotter = new TerminalNancyPlotRenderer()
        {
            PlotSettings =
            {
                Title = "test plot",
                XLabel = "time",
                YLabel = "data",
                Width = 30,
                Height = 10,
                AnsiMode = TerminalPlotAnsiMode.PlainText
            }
        };

        var terminalPlot = terminalPlotter.Plot(rl);

        Assert.Contains("test plot", terminalPlot);
        Assert.Contains("x: time", terminalPlot);
        Assert.Contains("y: data", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }

    [Fact]
    public void RendererPlotsMultipleCurves()
    {
        var sc = new RateLatencyServiceCurve(2, 1);
        var ac = new SigmaRhoArrivalCurve(2, 1);
        var terminalPlotter = new TerminalNancyPlotRenderer()
        {
            PlotSettings =
            {
                XLimit = new Interval(-1, 8),
                Width = 40,
                Height = 12,
                AnsiMode = TerminalPlotAnsiMode.PlainText
            }
        };

        var terminalPlot = terminalPlotter.Plot([sc, ac]);

        Assert.Contains("Legend:", terminalPlot);
        Assert.Contains("-1", terminalPlot);
        _testOutputHelper.WriteLine(terminalPlot);
    }
}
