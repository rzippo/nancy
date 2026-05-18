using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Builds terminal plot models from Nancy sequences.
/// </summary>
public class TerminalNancyPlotModeler : NancyPlotModeler<TerminalPlotSettings, TerminalPlot>
{
    /// <inheritdoc />
    public override TerminalPlot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names)
    {
        return new TerminalPlot(
            sequences.ToList(),
            names.ToList(),
            PlotSettings
        );
    }
}
