using System.Runtime.CompilerServices;
using Microsoft.DotNet.Interactive.Formatting;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using XPlot.Plotly;

// ReSharper disable MemberCanBePrivate.Global

namespace Unipi.Nancy.Interactive;

/// <summary>
/// Static class with extension methods that provide plotting functionality
/// to <see cref="Curve"/> and <see cref="Sequence"/>,
/// using <a href="https://github.com/fslaborg/XPlot">XPlot.Plotly</a>
/// and with support for <a href="https://github.com/dotnet/interactive">.NET Interactive</a>.
/// </summary>
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
    public static void Plot(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        Rational? upTo = null
    )
    {
        var plot = GetPlot(curves, names, upTo);
        plot.DisplayOnNotebook();
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
    public static void Plot(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        Rational? upTo = null
    )
    {
        var plot = GetPlot([curve], [name], upTo);
        plot.DisplayOnNotebook();
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
    public static void Plot(
        this IReadOnlyCollection<Curve> curves,
        Rational? upTo = null
    )
    {
        var names = curves.Select((_, i) => $"{(char)('f' + i)}");
        var plot = GetPlot(curves, names, upTo);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    public static void Plot(
        params Curve[] curves
    )
    {
        var plot = GetPlot(curves, null);
        plot.DisplayOnNotebook();
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    public static void Plot(
        this IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    )
    {
        var plot = GetPlot(sequences, names);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    public static void Plot(
        this IReadOnlyCollection<Sequence> sequences
    )
    {
        var names = sequences.Select((_, i) => $"{(char)('f' + i)}");
        var plot = GetPlot(sequences, names);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    public static void Plot(
        this Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f"
    )
    {
        var plot = GetPlot([sequence], [name]);
        plot.DisplayOnNotebook();
    }

    #endregion
}