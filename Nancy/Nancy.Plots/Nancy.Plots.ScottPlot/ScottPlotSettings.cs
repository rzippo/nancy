namespace Unipi.Nancy.Plots.ScottPlot;

/// <summary>
/// Settings controlling ScottPlot rendering.
/// </summary>
public record ScottPlotSettings : PlotSettings
{
    /// If true, the plot is forced to have the x and y axes to have the same scale.
    /// If false, they are adapted to the render size.
    public bool SameScaleAxes { get; set; } = false;

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

    /// <summary>
    /// If non-zero, adds margins left and right.
    /// To be read as a ratio over the initial x-axis interval length.
    /// </summary>
    public double RelativeXAxisMargin { get; set; } = 0.05;
    
    /// <summary>
    /// If non-zero, adds margins top and bottom.
    /// To be read as a ratio over the initial y-axis interval length.
    /// </summary>
    public double RelativeYAxisMargin { get; set; } = 0.05;
}
