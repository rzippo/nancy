using System.Runtime.CompilerServices;
using Unipi.Nancy.MinPlusAlgebra;
using System.Text.RegularExpressions;

namespace Unipi.Nancy.Plots.XPlot.Plotly;

/// <summary>
/// Extensions class that adds <c>ToXPlotHtml</c> methods.
/// </summary>
public static class XPlotPlots
{
    #region Curves

    /// <summary>
    /// Plots a set of curves.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        XPlotPlotSettings? settings = null
    )
    {
        settings ??= new XPlotPlotSettings();
        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        return renderer.Plot(curves, names);
    }

    /// <summary>
    /// Plots a curve.
    /// </summary>
    /// <param name="curve">The curve to plot.</param>
    /// <param name="name">
    /// The name of the curve.
    /// By default, it captures the expression used for <paramref name="curve"/>.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        XPlotPlotSettings? settings = null
    )
    {
        settings ??= new XPlotPlotSettings();
        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        return renderer.Plot(curve, name);
    }

    /// <summary>
    /// Plots a set of curves.
    /// It attempts to parse the names for the curves from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="curves"/> parameter.
    /// Otherwise, the curves will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names for the curves to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// The names of the curves can be automatically captured if one uses a syntax like <c>TikzPlots.ToXPlotHtml([b1, b2, b3]);</c>
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public static string ToXPlotHtml(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = "",
        XPlotPlotSettings? settings = null
    )
    {
        settings ??= new XPlotPlotSettings();
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParametersNameParsing.ParseNames(names, curves.Count);

        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        return renderer.Plot(curves, parsedNames);
    }

    // Instead of a params method, we use a set of overloads with 2, 3, ... curves.
    // The downside is that it works only up to a certain number of arguments, the upside is that it keeps the argument name recognition working, which seems to be much more important.

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Curve f,
        Curve g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g], [fName, gName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Curve f,
        Curve g,
        Curve h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h], [fName, gName, hName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Curve f,
        Curve g,
        Curve h,
        Curve i,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i], [fName, gName, hName, iName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Curve f,
        Curve g,
        Curve h,
        Curve i,
        Curve j,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i",
        [CallerArgumentExpression(nameof(j))] string jName = "j"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Curve f,
        Curve g,
        Curve h,
        Curve i,
        Curve j,
        Curve k,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i",
        [CallerArgumentExpression(nameof(j))] string jName = "j",
        [CallerArgumentExpression(nameof(k))] string kName = "k"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names,
        XPlotPlotSettings? settings = null
    )
    {
        settings ??= new XPlotPlotSettings();
        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        return renderer.Plot(sequences, names);
    }

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f",
        XPlotPlotSettings? settings = null
    )
    {
        settings ??= new XPlotPlotSettings();
        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        return renderer.Plot([sequence], [name]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// It attempts to parse the names for the sequences from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="sequences"/> parameter.
    /// Otherwise, the sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names for the sequences to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <param name="settings"></param>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// The names of the sequences can be automatically captured if one uses a syntax like <c>TikzPlots.ToXPlotHtml([b1, b2, b3]);</c>
    /// Note that this works if each sequence has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public static string ToXPlotHtml(
        IReadOnlyCollection<Sequence> sequences,
        [CallerArgumentExpression(nameof(sequences))]
        string names = "",
        XPlotPlotSettings? settings = null
    )
    {        
        settings ??= new XPlotPlotSettings();
        var renderer = new XPlotNancyPlotHtmlRenderer() { PlotSettings = settings };
        
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParametersNameParsing.ParseNames(names, sequences.Count);

        return renderer.Plot(sequences, parsedNames);
    }

    // Instead of a params method, we use a set of overloads with 2, 3, ... curves.
    // The downside is that it works only up to a certain number of arguments, the upside is that it keeps the argument name recognition working, which seems to be much more important.

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence f,
        Sequence g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g], [fName, gName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence f,
        Sequence g,
        Sequence h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h], [fName, gName, hName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence f,
        Sequence g,
        Sequence h,
        Sequence i,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i], [fName, gName, hName, iName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence f,
        Sequence g,
        Sequence h,
        Sequence i,
        Sequence j,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i",
        [CallerArgumentExpression(nameof(j))] string jName = "j"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The HTML code for the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="XPlotNancyPlotHtmlRenderer"/>.
    /// </remarks>
    public static string ToXPlotHtml(
        Sequence f,
        Sequence g,
        Sequence h,
        Sequence i,
        Sequence j,
        Sequence k,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h",
        [CallerArgumentExpression(nameof(i))] string iName = "i",
        [CallerArgumentExpression(nameof(j))] string jName = "j",
        [CallerArgumentExpression(nameof(k))] string kName = "k"
    )
    {
        var renderer = new XPlotNancyPlotHtmlRenderer();
        return renderer.Plot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
    }

    #endregion
}