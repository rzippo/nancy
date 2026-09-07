using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Plots;

/// <summary>
/// The three x-ranges a plot works with, and where the samples it was given came from.
/// </summary>
/// <param name="Data">
/// The range the reader asked to see.
/// The frame and the tick marks are derived from this one, and from no other.
/// It may reach below 0 when asked for explicitly, which is not a range a curve can be sampled over.
/// </param>
/// <param name="Frame">
/// The range the frame is drawn at, <paramref name="Data"/> opened up if degenerate and given its margin.
/// </param>
/// <param name="Sampling">
/// The range that was actually evaluated.
/// For curves this is <paramref name="Frame"/> within $[0, +\infty[$,
/// so that the strip between the data and the frame holds real values rather than a line projected into it.
/// For sequences given directly it is the extent of the sequences, which is all there is.
/// </param>
/// <param name="DataContinuesPastSamples">
/// True when the samples are a cut of something that goes on past them, which is what a curve gives and a sequence does not.
/// It decides whether an infinite run reaching the last sample is drawn as reaching the frame, or as stopping where the samples stop.
/// </param>
/// <remarks>
/// Sampling and framing are separate questions,
/// and answering them with one range is what let a plot draw a curve going the wrong way across its own margin.
/// The two facts this carries are also separate:
/// whether the samples reach the frame decides how a line is ended,
/// while <paramref name="DataContinuesPastSamples"/> decides how far an infinite area reaches.
/// </remarks>
public readonly record struct PlotXWindow(
    Interval Data,
    Interval Frame,
    Interval Sampling,
    bool DataContinuesPastSamples)
{
    /// <summary>
    /// True when the samples run all the way to the frame, so the last point drawn is the frame rather than an end of the data.
    /// </summary>
    /// <remarks>
    /// A line ending here is ending because the plot does, so it carries none of the marks that say a function ended:
    /// no dot, no bracket, no shortening.
    /// A sequence framed past its own end keeps all of them.
    /// </remarks>
    public bool SamplesReachTheFrame => Sampling.Upper == Frame.Upper;

    /// <summary>
    /// The window for plotting curves: framed around what was asked for, and sampled over as much of the frame as a curve has.
    /// </summary>
    public static PlotXWindow ForCurves(
        IReadOnlyCollection<Curve> curves,
        PlotSettings settings)
    {
        var data = PlotAxisLimitAlgorithms.GetCurveDataXLimit(curves, settings);
        var frame = PlotAxisLimitAlgorithms.GetFramingXLimit(data, settings);

        // curves are not defined below 0, so the frame may reach where nothing can be sampled
        var sampling = new Interval(
            Rational.Max(0, frame.Lower),
            frame.Upper,
            isLowerIncluded: true,
            isUpperIncluded: true);

        return new PlotXWindow(data, frame, sampling, DataContinuesPastSamples: true);
    }

    /// <summary>
    /// The window for plotting sequences as given: nothing is known past them, so nothing is sampled past them either.
    /// </summary>
    /// <param name="sequences">The sequences to plot.</param>
    /// <param name="settings">The settings of the plot.</param>
    /// <param name="dataContinuesPastSamples">
    /// Set when the caller knows the sequences are cuts of something that goes on, which the sequences themselves cannot say.
    /// </param>
    public static PlotXWindow ForSequences(
        IReadOnlyCollection<Sequence> sequences,
        PlotSettings settings,
        bool dataContinuesPastSamples = false)
    {
        var data = PlotAxisLimitAlgorithms.GetSequenceDataXLimit(sequences, settings);
        var extent = PlotAxisLimitAlgorithms.GetSequenceExtent(sequences);

        return new PlotXWindow(
            data,
            PlotAxisLimitAlgorithms.GetFramingXLimit(data, settings),
            extent,
            dataContinuesPastSamples);
    }

}
