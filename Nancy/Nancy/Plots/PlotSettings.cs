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
    /// Range for the x-axis.
    /// </summary>
    public Interval? XLimit { get; set; } = null;

    /// <summary>
    /// Range for the y-axis.
    /// </summary>
    public Interval? YLimit { get; set; } = null;
    
    /// <summary>
    /// Label for the x-axis.
    /// </summary>
    public string XLabel { get; set; } = string.Empty;

    /// <summary>
    /// Label for the y-axis.
    /// </summary>
    public string YLabel { get; set; } = string.Empty;
    
    /// Controls whether the legend is included or not in the plot.
    public LegendStrategy LegendStrategy { get; set; } = LegendStrategy.Auto;

    /// If the legend is included in the plot, controls where it should be placed.
    public LegendPosition LegendPosition { get; set; } = LegendPosition.SouthEast;
}