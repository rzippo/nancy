using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
        [CallerArgumentExpression(nameof(curve))] string name = "f",
        Rational? upTo = null
    )
    {
        var plot = GetPlot([curve], [name], upTo);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// It attempts to parse the names for the curves from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="curves"/> parameter.
    /// Otherwise, the curves will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names for the curves to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <param name="upTo">
    /// The x-axis right edge of the plot.
    /// If null, it is set by default as $max_{i}(T_i + 2 * d_i)$.
    /// </param>
    /// <remarks>
    /// The names of the curves can be automatically captured if one uses a syntax like "Plots.Plot([b1, b2, b3], upTo: 10);"
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public static void Plot(
        this IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))] string names = "",
        Rational? upTo = null
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var plot = GetPlot(curves, parsedNames, upTo);
        plot.DisplayOnNotebook();

        IEnumerable<string> ParseNames(string expr)
        {
            if (string.IsNullOrEmpty(expr))
                return DefaultNames();

            // matches collection expressions, like "[f, g, h]"
            var bracketsNotationRegex = new Regex(@"\[(?:([\w\d_\s+*-]+)(?:,\s*)?)+\]");
            // matches array expressions, like "new []{f, g, h}"
            var arrayNotationRegex = new Regex(@"new \w*?\[\]\{(?:([\w\d_\s+*-]+)(?:,\s*)?)+\}");
            // matches list expressions, like "new List<>{f, g, h}"
            var listNotationRegex = new Regex(@"new List<.*>(?:\(\))?\{(?:([\w\d_\s+*-]+)(?:,\s*)?)+\}");

            if (bracketsNotationRegex.IsMatch(expr))
            {
                var match = bracketsNotationRegex.Match(expr);
                var pNames = match.Groups[1].Captures
                    .Select(c => c.Value);
                return pNames;
            }
            else if (arrayNotationRegex.IsMatch(expr))
            {
                var match = arrayNotationRegex.Match(expr);
                var pNames = match.Groups[1].Captures
                    .Select(c => c.Value);
                return pNames;
            }
            else if (listNotationRegex.IsMatch(expr))
            {
                var match = listNotationRegex.Match(expr);
                var pNames = match.Groups[1].Captures
                    .Select(c => c.Value);
                return pNames;
            }

            // if all else failed
            return DefaultNames();

            IEnumerable<string> DefaultNames()
                => curves.Select((_, i) => $"{(char)('f' + i)}");
        }
    }

    // Instead of a params method, we use a set of overloads with 2, 3, ... curves.
    // This allows to keep the argument name recognition, which seems to be much more important.

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public static void Plot(
        Curve f,
        Curve g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var plot = GetPlot([f, g], [fName, gName]);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public static void Plot(
        Curve f,
        Curve g,
        Curve h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var plot = GetPlot([f, g, h], [fName, gName, hName]);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public static void Plot(
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
        var plot = GetPlot([f, g, h, i], [fName, gName, hName, iName]);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public static void Plot(
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
        var plot = GetPlot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
        plot.DisplayOnNotebook();
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public static void Plot(
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
        var plot = GetPlot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
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
        [CallerArgumentExpression(nameof(sequence))] string name = "f"
    )
    {
        var plot = GetPlot([sequence], [name]);
        plot.DisplayOnNotebook();
    }

    #endregion
}