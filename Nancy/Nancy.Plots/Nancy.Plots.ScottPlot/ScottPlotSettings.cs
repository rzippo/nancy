namespace Unipi.Nancy.Plots.ScottPlot;

/// <summary>
/// Settings controlling ScottPlot rendering.
/// </summary>
public record ScottPlotSettings : PlotSettings
{
    /// <summary>
    /// Output image width in pixels.
    /// </summary>
    public int Width { get; set; } = 1200;

    /// <summary>
    /// Output image height in pixels.
    /// </summary>
    public int Height { get; set; } = 800;

    /// <summary>
    /// Scale factor used when rendering the output image.
    /// </summary>
    public double ScaleFactor { get; set; } = 1.5;

}
