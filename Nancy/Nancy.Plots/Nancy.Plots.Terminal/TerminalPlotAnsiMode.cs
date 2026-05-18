namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Controls how terminal plot colors are rendered.
/// </summary>
public enum TerminalPlotAnsiMode
{
    /// <summary>
    /// Let Spectre.Console detect terminal support.
    /// </summary>
    Auto,

    /// <summary>
    /// Force ANSI escape sequences in the rendered output.
    /// </summary>
    Ansi,

    /// <summary>
    /// Render plain text without ANSI escape sequences.
    /// </summary>
    PlainText
}
