namespace Unipi.Nancy.Plots.XPlot.Plotly;

/// <summary>
/// Settings controlling XPlot.Plotly rendering.
/// </summary>
/// <remarks>
/// Plotly has no fill pattern for a filled area, so <see cref="PlotSettings.FillPatterns"/> is accepted and ignored: overlapping areas are told apart by color, at <see cref="InfinityAreaOpacity"/>.
/// <see cref="PlotSettings.SameScaleAxes"/> is ignored too: the axes are adapted to the render size.
/// </remarks>
public record XPlotPlotSettings : PlotSettings
{
    /// <summary>
    /// Output width in pixels.
    /// </summary>
    public int Width { get; set; } = 1200;

    /// <summary>
    /// Output height in pixels.
    /// </summary>
    public int Height { get; set; } = 800;

    /// <summary>
    /// Scale factor used by compatible renderers.
    /// </summary>
    public double ScaleFactor { get; set; } = 1.5;

    /// <summary>
    /// How opaque the areas marking infinite values are drawn.
    /// </summary>
    /// <remarks>
    /// Kept low: plotly has no fill pattern for these, so the area is solid, and two overlapping ones compound.
    /// At 0.2 a pair still reads as two, and a curve drawn over them stays legible.
    /// </remarks>
    public double InfinityAreaOpacity { get; set; } = 0.2;
}
