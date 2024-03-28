using System.Runtime.CompilerServices;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using XPlot.Plotly;

namespace Unipi.Nancy.Interactive;

// ReSharper disable MemberCanBePrivate.Global

public static partial class Plots
{
    #region Curves

    /// <summary>
    /// Plots a set of curves.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $max_{i}(T_i + 2 * d_i)$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        Rational? upTo = null
    )
    {
        var plot = GetPlot(curves, names, upTo);
        return plot.GetNotebookHtml();
    }

    /// <summary>
    /// Plots a curve.
    /// </summary>
    /// <param name="curve">The curve to plot.</param>
    /// <param name="name">
    /// The name of the curve.
    /// By default, it captures the expression used for <paramref name="curve"/>.
    /// </param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $T + 2 * d$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        Rational? upTo = null
    )
    {
        var plot = GetPlot([curve], [name], upTo);
        return plot.GetNotebookHtml();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $max_{i}(T_i + 2 * d_i)$.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this IReadOnlyCollection<Curve> curves,
        Rational? upTo = null
    )
    {
        var names = curves.Select((_, i) => $"{(char)('f' + i)}");
        var plot = GetPlot(curves, names, upTo);
        return plot.GetNotebookHtml();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        params Curve[] curves
    )
    {
        var plot = GetPlot(curves, null);
        return plot.GetNotebookHtml();
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    )
    {
        var plot = GetPlot(sequences, names);
        return plot.GetNotebookHtml();
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this IReadOnlyCollection<Sequence> sequences
    )
    {
        var names = sequences.Select((_, i) => $"{(char)('f' + i)}");
        var plot = GetPlot(sequences, names);
        return plot.GetNotebookHtml();
    }

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    /// <returns>A <see cref="PlotlyChart"/> object.</returns>
    public static string GetPlotAsHtml(
        this Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f"
    )
    {
        var plot = GetPlot([sequence], [name]);
        return plot.GetNotebookHtml();
    }

    #endregion
}