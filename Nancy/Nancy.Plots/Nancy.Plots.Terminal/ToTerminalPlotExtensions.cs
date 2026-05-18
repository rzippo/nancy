using System.Runtime.CompilerServices;
using Spectre.Console;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Plots.Terminal;

/// <summary>
/// Extensions class that adds terminal plot methods.
/// </summary>
public static class TerminalPlots
{
    #region Curves

    /// <summary>
    /// Plots a set of curves as terminal text.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>A terminal-ready string. ANSI output depends on <see cref="TerminalPlotSettings.AnsiMode"/>.</returns>
    public static string ToTerminalPlot(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(curves, names);
    }

    /// <summary>
    /// Plots a curve as terminal text.
    /// </summary>
    /// <param name="curve">The curve to plot.</param>
    /// <param name="name">
    /// The name of the curve.
    /// By default, it captures the expression used for <paramref name="curve"/>.
    /// </param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>A terminal-ready string. ANSI output depends on <see cref="TerminalPlotSettings.AnsiMode"/>.</returns>
    public static string ToTerminalPlot(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(curve, name);
    }

    /// <summary>
    /// Plots a set of curves as terminal text.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names for the curves to plot.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public static string ToTerminalPlot(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = "",
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var parsedNames = ParametersNameParsing.ParseNames(names, curves.Count);

        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(curves, parsedNames);
    }

    /// <summary>
    /// Writes a curve plot to the terminal.
    /// </summary>
    /// <param name="curve">The curve to plot.</param>
    /// <param name="name">
    /// The name of the curve.
    /// By default, it captures the expression used for <paramref name="curve"/>.
    /// </param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="console">Optional target console. Defaults to <see cref="AnsiConsole.Console"/>.</param>
    public static void PlotToTerminal(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        TerminalPlotSettings? settings = null,
        IAnsiConsole? console = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        var plot = renderer.GetDefaultModeler().GetPlot(curve, name);
        renderer.WriteToConsole(plot, console);
    }

    /// <summary>
    /// Writes a set of curves to the terminal.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="console">Optional target console. Defaults to <see cref="AnsiConsole.Console"/>.</param>
    public static void PlotToTerminal(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        TerminalPlotSettings? settings = null,
        IAnsiConsole? console = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        var plot = renderer.GetDefaultModeler().GetPlot(curves, names);
        renderer.WriteToConsole(plot, console);
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences as terminal text.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>A terminal-ready string. ANSI output depends on <see cref="TerminalPlotSettings.AnsiMode"/>.</returns>
    public static string ToTerminalPlot(
        this IEnumerable<Sequence> sequences,
        IEnumerable<string> names,
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(sequences, names);
    }

    /// <summary>
    /// Plots a sequence as terminal text.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>A terminal-ready string. ANSI output depends on <see cref="TerminalPlotSettings.AnsiMode"/>.</returns>
    public static string ToTerminalPlot(
        this Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f",
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(sequence, name);
    }

    /// <summary>
    /// Plots a set of sequences as terminal text.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names for the sequences to plot.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    public static string ToTerminalPlot(
        IReadOnlyCollection<Sequence> sequences,
        [CallerArgumentExpression(nameof(sequences))]
        string names = "",
        TerminalPlotSettings? settings = null)
    {
        settings ??= new TerminalPlotSettings();
        var parsedNames = ParametersNameParsing.ParseNames(names, sequences.Count);

        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(sequences, parsedNames);
    }

    /// <summary>
    /// Writes a sequence plot to the terminal.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="console">Optional target console. Defaults to <see cref="AnsiConsole.Console"/>.</param>
    public static void PlotToTerminal(
        this Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f",
        TerminalPlotSettings? settings = null,
        IAnsiConsole? console = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        var plot = renderer.GetDefaultModeler().GetPlot(sequence, name);
        renderer.WriteToConsole(plot, console);
    }

    /// <summary>
    /// Writes a set of sequences to the terminal.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <param name="console">Optional target console. Defaults to <see cref="AnsiConsole.Console"/>.</param>
    public static void PlotToTerminal(
        this IEnumerable<Sequence> sequences,
        IEnumerable<string> names,
        TerminalPlotSettings? settings = null,
        IAnsiConsole? console = null)
    {
        settings ??= new TerminalPlotSettings();
        var renderer = new TerminalNancyPlotRenderer() { PlotSettings = settings };
        var plot = renderer.GetDefaultModeler().GetPlot(sequences, names);
        renderer.WriteToConsole(plot, console);
    }

    #endregion
}
