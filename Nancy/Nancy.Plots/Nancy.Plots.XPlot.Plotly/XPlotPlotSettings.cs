namespace Unipi.Nancy.Plots.XPlot.Plotly;

/// <summary>
/// Settings controlling XPlot.Plotly rendering.
/// </summary>
public record XPlotPlotSettings : PlotSettings
{
    /// If true, the plot is forced to have the x and y axes to have the same scale.
    /// If false, they are adapted to the render size.
    public bool SameScaleAxes { get; set; } = false;

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
}
