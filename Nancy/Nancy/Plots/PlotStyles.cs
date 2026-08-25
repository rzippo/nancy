using System.Collections.Generic;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Line styles that can be cycled to tell plotted curves apart.
/// </summary>
/// <remarks>
/// These are backend-neutral: each plot implementation maps them onto its own representation.
/// Cycling a style alongside the color is what keeps curves distinguishable in black-and-white print.
/// </remarks>
public enum PlotLineStyle
{
    /// An uninterrupted line.
    Solid,

    /// A dashed line.
    Dashed,

    /// A dotted line.
    Dotted,

    /// A line alternating dashes and dots.
    /// Not all backends can render this distinctly, see their documentation.
    DashDotted,

    /// A dashed line with shorter gaps than <see cref="Dashed"/>.
    DenselyDashed,

    /// A dotted line with shorter gaps than <see cref="Dotted"/>.
    DenselyDotted
}

/// <summary>
/// Fill patterns that can be cycled for the areas marking infinite values.
/// </summary>
/// <remarks>
/// These are backend-neutral: each plot implementation maps them onto its own representation.
/// Cycling a pattern is what keeps overlapping infinite areas readable, since they do not align.
/// </remarks>
public enum PlotFillPattern
{
    /// A sparse pattern of dots.
    Dots,

    /// A denser pattern of dots.
    DenseDots,

    /// Lines running from bottom-left to top-right.
    DiagonalLines,

    /// Lines running from top-left to bottom-right.
    ReverseDiagonalLines,

    /// Horizontal and vertical lines.
    Grid,

    /// Diagonal lines in both directions.
    Crosshatch
}

/// <summary>
/// Controls how the infinite parts of a curve or sequence are plotted.
/// </summary>
public enum InfinityStrategy
{
    /// Infinite elements are skipped, leaving a gap in the plot.
    Ignore,

    /// Infinite elements are marked by an area reaching the vertical extreme of the plot.
    /// The area is above the x-axis for $+\infty$ and below it for $-\infty$.
    Areas
}

/// <summary>
/// The default cycles used when <see cref="PlotSettings.LineStyles"/> or <see cref="PlotSettings.FillPatterns"/> are not set.
/// </summary>
public static class PlotStyleCycles
{
    /// <summary>
    /// The line styles cycled by default.
    /// </summary>
    /// <remarks>
    /// Limited to styles that every backend able to dash can render distinctly.
    /// </remarks>
    public static readonly IReadOnlyList<PlotLineStyle> DefaultLineStyles =
    [
        PlotLineStyle.Solid,
        PlotLineStyle.Dashed,
        PlotLineStyle.Dotted,
        PlotLineStyle.DenselyDashed
    ];

    /// <summary>
    /// The fill patterns cycled by default.
    /// </summary>
    public static readonly IReadOnlyList<PlotFillPattern> DefaultFillPatterns =
    [
        PlotFillPattern.Dots,
        PlotFillPattern.DiagonalLines,
        PlotFillPattern.ReverseDiagonalLines,
        PlotFillPattern.Grid
    ];

    /// <summary>
    /// Picks the line style for the sequence at index <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index of the sequence being plotted.</param>
    /// <param name="styles">The styles being cycled.</param>
    /// <remarks>
    /// The style advances at every sequence, so that two curves plotted together never share one.
    /// Colors and styles then cycle independently, and a pen repeats only after the least common multiple of the two lengths: 12 for 3 colors and 4 styles.
    /// Lengths that share a factor repeat sooner, so a palette meant to be cycled is best given a length coprime with the number of styles.
    /// </remarks>
    public static TStyle Pick<TStyle>(int index, IReadOnlyList<TStyle> styles)
    {
        return styles[index % styles.Count];
    }
}
