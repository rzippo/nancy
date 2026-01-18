using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Abstract class to plot Nancy curves via another library that produces <typeparamref name="TPlot"/> objects.
/// Used to abstract over different methods for plotting, e.g. TikZ, Plotly, ScottPlot.
/// </summary>
/// <typeparam name="TSettings">Settings for this plotter, extends <see cref="PlotSettings"/>.</typeparam>
/// <typeparam name="TPlot">
/// Modeling object for the plot.
/// This is usually a plotting library's own plot class.
/// This may be used as intermediate result to customize the plot further.
/// </typeparam>
public abstract class NancyPlotModeler<TSettings, TPlot>
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
    /// <returns>A <typeparamref name="TPlot"/> object.</returns>
    public abstract TPlot GetPlot(
        IEnumerable<Sequence> sequences,
        IEnumerable<string> names
    );

    #region Curves

    /// <summary>
    /// Plots a set of curves.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names of the curves.</param>
    public TPlot GetPlot(
        IReadOnlyCollection<Curve> curves,
        IEnumerable<string> names
    )
    {
        Interval xi;
        if (PlotSettings.XLimit is { } xLimit && xLimit.Upper >= 0 && xLimit.Upper.IsFinite)
        {
            xi = xLimit.Lower >= 0 ? xLimit : xLimit.WithLower(0);
        }
        else
        {
            // todo: implement different strategies here
            var rightEdge = curves.Max(c => c.SecondPseudoPeriodEnd);
            xi = new Interval(0, rightEdge, true, true);
        }

        var cuts = curves
            .Select(c => c.Cut(xi))
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
    /// <returns>A <typeparamref name="TPlot"/> object.</returns>
    public TPlot GetPlot(
        Curve curve,
        [CallerArgumentExpression("curve")] string name = "f"
    )
    {
        return GetPlot([curve], [name]);
    }

    /// <summary>
    /// Plots a set of curves.
    /// It attempts to parse the names for the curves from the optional parameter <paramref name="names"/> or
    /// from the expression used for the <paramref name="curves"/> parameter.
    /// Otherwise, the curves will be given default names f, g, h and so on.
    /// </summary>
    /// <param name="curves">The curves to plot.</param>
    /// <param name="names">The names for the curves to plot. If manually specified, the recommended format is "[name1, name2, ...]".</param>
    /// <remarks>
    /// The names of the curves can be automatically captured if one uses a syntax like <c>Plots.Plot([b1, b2, b3]: 10);</c>
    /// Note that this works if each curve has its own variable name, rather than being the result of an expression.
    /// </remarks>
    public TPlot GetPlot(
        IReadOnlyCollection<Curve> curves,
        [CallerArgumentExpression(nameof(curves))]
        string names = ""
    )
    {
        // this code tries to recognize patterns for variable names
        // if it fails, the default [f, g, h, ...] names will be used
        var parsedNames = ParametersNameParsing.ParseNames(names, curves.Count);

        var plot = GetPlot(curves, parsedNames);
        return plot;
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
        var parsedNames = ParametersNameParsing.ParseNames(names, sequences.Count);

        var plot = GetPlot(sequences, parsedNames);
        return plot;
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
}