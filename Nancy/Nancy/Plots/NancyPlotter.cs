using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Abstract class that can plot Nancy curves.
/// </summary>
/// <typeparam name="TSettings">Settings for this plotter, extends <see cref="PlotSettings"/>.</typeparam>
/// <typeparam name="TPlot">
/// Modeling object for the plot.
/// This is usually a plotting library's own plot class.
/// This may be used as intermediate result to customize the plot further.
/// </typeparam>
/// <typeparam name="TOutput">
/// The plot raw output. E.g., an HTML string, or the bytes of an image.  
/// </typeparam>
public abstract class NancyPlotter<TSettings, TPlot, TOutput>
    where TSettings : PlotSettings, new()
{
    /// <summary>
    /// Plot settings.
    /// </summary>
    public TSettings PlotSettings { get; init; } = new();

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    /// <returns>A <see cref="TOutput"/> object.</returns>
    public abstract TPlot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    );

    /// <summary>
    /// Renders the plot model <see cref="TPlot"/> to the output type <see cref="TOutput"/>.
    /// </summary>
    /// <param name="plot"></param>
    public abstract TOutput PlotToOutput(TPlot plot);

    #region GetPlot()

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
    public TPlot GetPlot(
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        Rational? upTo = null
    )
    {
        Rational t;
        if (upTo is not null)
            t = (Rational)upTo;
        else
            t = curves.Max(c => c.SecondPseudoPeriodEnd);
        t = t == 0 ? 10 : t;

        var cuts = curves
            .Select(c => c.Cut(0, t, isEndIncluded: true))
            .ToList();

        return GetPlot(cuts, names);
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
    /// <returns>A <see cref="TOutput"/> object.</returns>
    public TPlot GetPlot(
        Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        Rational? upTo = null
    )
    {
        return GetPlot([curve], [name], upTo);
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
    /// The names of the curves can be automatically captured if one uses a syntax like <c>Plots.Plot([b1, b2, b3], upTo: 10);</c>
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public TPlot GetPlot(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = "",
        Rational? upTo = null
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var plot = GetPlot(curves, parsedNames, upTo);
        return plot;

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
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TPlot GetPlot(
        Curve f,
        Curve g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var plot = GetPlot([f, g], [fName, gName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TPlot GetPlot(
        Curve f,
        Curve g,
        Curve h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var plot = GetPlot([f, g, h], [fName, gName, hName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TPlot GetPlot(
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
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TPlot GetPlot(
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
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TPlot GetPlot(
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
        return plot;
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    public TPlot GetPlot(
        Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f"
    )
    {
        return GetPlot([sequence], [name]);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// It attempts to parse the names for the sequences from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="sequences"/> parameter.
    /// Otherwise, the sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names for the sequences to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <remarks>
    /// The names of the sequences can be automatically captured if one uses a syntax like <c>Plots.Plot([b1, b2, b3]);</c>
    /// Note that this works if each sequence has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public TPlot GetPlot(
        IReadOnlyCollection<Sequence> sequences,
        [CallerArgumentExpression(nameof(sequences))]
        string names = ""
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var plot = GetPlot(sequences, parsedNames);
        return plot;

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
    public TPlot GetPlot(
        Sequence f,
        Sequence g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var plot = GetPlot([f, g], [fName, gName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TPlot GetPlot(
        Sequence f,
        Sequence g,
        Sequence h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var plot = GetPlot([f, g, h], [fName, gName, hName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TPlot GetPlot(
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
        var plot = GetPlot([f, g, h, i], [fName, gName, hName, iName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TPlot GetPlot(
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
        var plot = GetPlot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TPlot GetPlot(
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
        var plot = GetPlot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
        return plot;
    }

    #endregion

    #endregion

    #region Plot()

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
    public TOutput Plot(
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names,
        Rational? upTo = null
    )
    {
        var plot = GetPlot(curves, names, upTo);
        return PlotToOutput(plot);
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
    public TOutput Plot(
        Curve curve,
        [CallerArgumentExpression("curve")] string name = "f",
        Rational? upTo = null
    )
    {
        var plot = GetPlot([curve], [name], upTo);
        return PlotToOutput(plot);
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
    /// The names of the curves can be automatically captured if one uses a syntax like <c>Plots.Plot([b1, b2, b3], upTo: 10);</c>
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public TOutput Plot(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = "",
        Rational? upTo = null
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var plot = Plot(curves, parsedNames, upTo);
        return plot;

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
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TOutput Plot(
        Curve f,
        Curve g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var plot = Plot([f, g], [fName, gName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TOutput Plot(
        Curve f,
        Curve g,
        Curve h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var plot = Plot([f, g, h], [fName, gName, hName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i], [fName, gName, hName, iName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of curves.
    /// The curves will be given default names f, g, h and so on.
    /// The x-axis right edge of the plot will be set to $max_{i}(T_i + 2 * d_i)$.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
        return plot;
    }

    #endregion

    #region Sequences

    /// <summary>
    /// Plots a set of sequences.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names of the sequences.</param>
    public TOutput Plot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    )
    {
        var plot = GetPlot(sequences, names);
        return PlotToOutput(plot);
    }

    /// <summary>
    /// Plots a sequence.
    /// </summary>
    /// <param name="sequence">The sequence to plot.</param>
    /// <param name="name">
    /// The name of the sequence.
    /// By default, it captures the expression used for <paramref name="sequence"/>.
    /// </param>
    public TOutput Plot(
        Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "f"
    )
    {
        var plot = GetPlot([sequence], [name]);
        return PlotToOutput(plot);
    }

    /// <summary>
    /// Plots a set of sequences.
    /// It attempts to parse the names for the sequences from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="sequences"/> parameter.
    /// Otherwise, the sequences will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="names">The names for the sequences to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <remarks>
    /// The names of the sequences can be automatically captured if one uses a syntax like <c>Plots.Plot([b1, b2, b3]);</c>
    /// Note that this works if each sequence has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public TOutput Plot(
        IReadOnlyCollection<Sequence> sequences,
        [CallerArgumentExpression(nameof(sequences))]
        string names = ""
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParseNames(names);

        var plot = Plot(sequences, parsedNames);
        return plot;

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
    public TOutput Plot(
        Sequence f,
        Sequence g,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g"
    )
    {
        var plot = Plot([f, g], [fName, gName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TOutput Plot(
        Sequence f,
        Sequence g,
        Sequence h,
        [CallerArgumentExpression(nameof(f))] string fName = "f",
        [CallerArgumentExpression(nameof(g))] string gName = "g",
        [CallerArgumentExpression(nameof(h))] string hName = "h"
    )
    {
        var plot = Plot([f, g, h], [fName, gName, hName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i], [fName, gName, hName, iName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i, j], [fName, gName, hName, iName, jName]);
        return plot;
    }

    /// <summary>
    /// Plots a set of sequences.
    /// The sequences will be given default names f, g, h and so on.
    /// </summary>
    public TOutput Plot(
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
        var plot = Plot([f, g, h, i, j, k], [fName, gName, hName, iName, jName, kName]);
        return plot;
    }

    #endregion

    #endregion
}