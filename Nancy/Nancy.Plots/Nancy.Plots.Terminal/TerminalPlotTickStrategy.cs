namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Strategy used to choose terminal plot axis tick labels.
/// </summary>
public enum TerminalPlotTickStrategy
{
    /// <summary>
    /// Place tick labels at evenly spaced axis values.
    /// </summary>
    EvenlySpaced,

    /// <summary>
    /// Prefer tick labels at plotted sequence breakpoints, then fill remaining labels evenly.
    /// </summary>
    PreferBreakpoints
}
