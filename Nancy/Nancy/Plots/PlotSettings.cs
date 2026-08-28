using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// Settings for a plot of Nancy curves.
/// </summary>
/// <remarks>
/// The base class contains settings that are meaningful in all contexts,
/// But some may not be supported by all plot implementations.
/// </remarks>
public record PlotSettings
{
    /// <summary>
    /// The plot title.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Range the plotted items occupy along the x-axis.
    /// </summary>
    /// <remarks>
    /// When null, the extent of the plotted values is used.
    /// The frame drawn around the plot adds <see cref="RelativeXAxisMargin"/> on top of this range.
    /// </remarks>
    public Interval? XLimit { get; set; } = null;

    /// <summary>
    /// Controls how far to the right a plot of curves reaches, when <see cref="XLimit"/> does not say.
    /// </summary>
    public PlotEndStrategy PlotEndStrategy { get; set; } = PlotEndStrategy.TwoPeriodsEach;

    /// <summary>
    /// Range the plotted items occupy along the y-axis.
    /// </summary>
    /// <inheritdoc cref="XLimit" path="/remarks"/>
    public Interval? YLimit { get; set; } = null;

    /// <summary>
    /// If non-zero, adds margins left and right to the x-axis limits, however they were obtained.
    /// To be read as a ratio over the x-axis interval length.
    /// </summary>
    /// <remarks>
    /// Kept small, so that the mark at the origin is drawn whole without the axis appearing to open before time starts.
    /// </remarks>
    public double RelativeXAxisMargin { get; set; } = 0.03;

    /// <summary>
    /// If non-zero, adds margins top and bottom to the y-axis limits, however they were obtained.
    /// To be read as a ratio over the y-axis interval length.
    /// </summary>
    /// <remarks>
    /// Kept equal to <see cref="RelativeXAxisMargin"/>, so that the plot is framed evenly on both axes.
    /// </remarks>
    public double RelativeYAxisMargin { get; set; } = 0.03;
    
    /// <summary>
    /// Label for the x-axis.
    /// </summary>
    public string XLabel { get; set; } = "time";

    /// <summary>
    /// Label for the y-axis.
    /// </summary>
    public string YLabel { get; set; } = "data";
    
    /// <summary>
    /// If true, the two axes are drawn at the same scale, so that a slope of 1 appears at 45 degrees.
    /// </summary>
    /// <remarks>
    /// False by default: time and data are not commensurate, so equal scales size the plot by the ratio of the values rather than by what reads well.
    /// Not supported by all plot implementations, see their documentation.
    /// </remarks>
    public bool SameScaleAxes { get; set; } = false;

    /// Controls whether the legend is included or not in the plot.
    public LegendStrategy LegendStrategy { get; set; } = LegendStrategy.Auto;

    /// If the legend is included in the plot, controls where it should be placed.
    public LegendPosition LegendPosition { get; set; } = LegendPosition.SouthEast;

    /// <summary>
    /// If the legend is included in the plot, controls whether it is placed inside the plot area or beside it.
    /// </summary>
    /// <remarks>
    /// Inside by default, which keeps the figure as wide as the plot.
    /// A legend inside the plot area may cover the curves: how much room it needs is decided by the renderer, and is not known when the plot is built, so <see cref="Plots.LegendPlacement.Outside"/> is the only placement that rules the overlap out rather than making it less likely.
    /// </remarks>
    public LegendPlacement LegendPlacement { get; set; } = LegendPlacement.Inside;

    /// <summary>
    /// If true, a line style is cycled alongside the color, so that curves stay distinguishable without color.
    /// </summary>
    /// <remarks>
    /// Set to false to plot every curve with an uninterrupted line.
    /// Not supported by all plot implementations, see their documentation.
    /// </remarks>
    public bool UseLineStyles { get; set; } = true;

    /// <summary>
    /// The line styles to cycle, when <see cref="UseLineStyles"/> is true.
    /// </summary>
    /// <remarks>
    /// If null, <see cref="PlotStyleCycles.DefaultLineStyles"/> is used.
    /// </remarks>
    public IReadOnlyList<PlotLineStyle>? LineStyles { get; set; } = null;

    /// <summary>
    /// Controls how the infinite parts of a curve or sequence are plotted.
    /// </summary>
    public InfinityStrategy InfinityStrategy { get; set; } = InfinityStrategy.Areas;

    /// <summary>
    /// The fill patterns to cycle for the areas marking infinite values.
    /// </summary>
    /// <remarks>
    /// If null, <see cref="PlotStyleCycles.DefaultFillPatterns"/> is used.
    /// Not supported by all plot implementations, see their documentation.
    /// </remarks>
    public IReadOnlyList<PlotFillPattern>? FillPatterns { get; set; } = null;

    /// <summary>
    /// The y-axis range reserved for the areas marking infinite values, as a ratio over the finite y-axis range.
    /// </summary>
    /// <remarks>
    /// Used only when <see cref="InfinityStrategy"/> is <see cref="Plots.InfinityStrategy.Areas"/> and the plotted values leave a non-degenerate y-axis range.
    /// Otherwise <see cref="RelativeInfinityBandHeightFromXAxis"/> applies.
    /// </remarks>
    public double RelativeInfinityBandHeight { get; set; } = 0.35;

    /// <summary>
    /// The y-axis range reserved for the areas marking infinite values, as a ratio over the x-axis range.
    /// </summary>
    /// <remarks>
    /// Used when the plotted values leave a degenerate y-axis range, as happens for a <c>DelayServiceCurve</c>, whose only finite value is 0.
    /// There is then no finite range to scale the area against, and scaling it against the x-axis is what keeps the plot from collapsing to a strip.
    /// </remarks>
    public double RelativeInfinityBandHeightFromXAxis { get; set; } = 0.6;
}
