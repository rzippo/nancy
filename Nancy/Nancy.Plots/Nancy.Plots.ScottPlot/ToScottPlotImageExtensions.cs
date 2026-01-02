using System.Runtime.CompilerServices;
using Unipi.Nancy.MinPlusAlgebra;
using System.Text.RegularExpressions;

namespace Unipi.Nancy.Plots.ScottPlot;

/// <summary>
/// Extensions class that adds <c>ToScottPlotImage</c> methods.
/// </summary>
public static class ScottPlots
{
    #region Curves

    /// <summary>
    /// Plots a set of curves.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    /// <param name="settings"></param>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        this IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        ScottPlotSettings? settings = null
    )
    {
        settings ??= new ScottPlotSettings();
        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
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
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        this Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        ScottPlotSettings? settings = null
    )
    {
        settings ??= new ScottPlotSettings();
        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
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
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// The names of the curves can be automatically captured if one uses a syntax like <c>ScottPlots.ToScottPlotImage([b1, b2, b3]);</c>
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = "",
        ScottPlotSettings? settings = null
    )
    {
        settings ??= new ScottPlotSettings();
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
        return renderer.Plot(curves, parsedNames);
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
    // The downside is that it works only up to a certain number of arguments, the upside is that it keeps the argument name recognition working, which seems to be much more important.

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        Curve f,
        Curve g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g], [fName, gName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        Curve f,
        Curve g,
        Curve h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h], [fName, gName, hName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h, i], [fName, gName, hName, iName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
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
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names,
        ScottPlotSettings? settings = null
    )
    {
        settings ??= new ScottPlotSettings();
        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
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
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f",
        ScottPlotSettings? settings = null
    )
    {
        settings ??= new ScottPlotSettings();
        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
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
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// The names of the sequences can be automatically captured if one uses a syntax like <c>ScottPlots.ToScottPlotImage([b1, b2, b3]);</c>
    /// Note that this works if each sequence has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        IReadOnlyCollection<Sequence> sequences,
        [CallerArgumentExpression(nameof(sequences))]
        string names = "",
        ScottPlotSettings? settings = null
    )
    {        
        settings ??= new ScottPlotSettings();
        var renderer = new ScottNancyPlotRenderer() { PlotSettings = settings };
        
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        return renderer.Plot(sequences, parsedNames);
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
                => sequences.Select((_, i) => $"{(char)('f' + i)}");
        }
    }

    // Instead of a params method, we use a set of overloads with 2, 3, ... curves.
    // The downside is that it works only up to a certain number of arguments, the upside is that it keeps the argument name recognition working, which seems to be much more important.

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        Sequence f,
        Sequence g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g], [fName, gName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
        Sequence f,
        Sequence g,
        Sequence h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h], [fName, gName, hName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h, i], [fName, gName, hName, iName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <returns>The bytes of a png image of the plot.</returns>
    /// <remarks>
    /// This is a shortcut method, that uses <see cref="ScottNancyPlotRenderer"/>.
    /// </remarks>
    public static byte[] ToScottPlotImage(
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
        var renderer = new ScottNancyPlotRenderer();
        return renderer.Plot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
    }

    #endregion
}