using Spectre.Console;

namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Renders Nancy plots to terminal text.
/// </summary>
public class TerminalNancyPlotRenderer : NancyPlotRenderer<TerminalPlotSettings, TerminalPlot, string>
{
    /// <inheritdoc />
    public override NancyPlotModeler<TerminalPlotSettings, TerminalPlot> GetDefaultModeler()
    {
        return new TerminalNancyPlotModeler()
        {
            PlotSettings = PlotSettings
        };
    }

    /// <inheritdoc />
    public override string PlotToOutput(TerminalPlot plot)
    {
        return plot.ToTerminalString(PlotSettings.AnsiMode);
    }

    /// <summary>
    /// Writes a terminal plot directly to a Spectre.Console console.
    /// </summary>
    /// <param name="plot">The plot to write.</param>
    /// <param name="console">Optional target console. Defaults to <see cref="AnsiConsole.Console"/>.</param>
    public void WriteToConsole(TerminalPlot plot, IAnsiConsole? console = null)
    {
        plot.WriteTo(console ?? AnsiConsole.Console);
    }
}
