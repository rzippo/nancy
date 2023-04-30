using System.Collections.Generic;
using System.Linq;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A rate-latency service curve
/// </summary>
public class RateLatencyServiceCurve : ConvexCurve
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Minimum rate of service.
    /// </summary>
    public Rational Rate { get; }

    /// <summary>
    /// Maximum latency of service.
    /// </summary>
    public Rational Latency { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public RateLatencyServiceCurve(Rational rate, Rational latency)
        : base(
            baseSequence: BuildSequence(rate, latency),
            pseudoPeriodStart: latency,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: rate * DefaultPeriodLength
        )
    {
        Latency = latency;
        Rate = rate;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor
    /// </summary>
    private static Sequence BuildSequence(Rational rate, Rational latency)
    {
        Element[] elements;
        if(latency == 0)
        {
            elements = new Element[]
            {
                Point.Origin(),
                new Segment
                (
                    startTime : 0,
                    rightLimitAtStartTime : 0,
                    slope : rate,
                    endTime : DefaultPeriodLength
                )
            };
        }
        else
        {
            elements = new Element[]
            {
                Point.Origin(),
                new Segment
                (
                    startTime : 0,
                    rightLimitAtStartTime : 0,
                    slope : 0,
                    endTime : latency
                ),
                new Point(time: latency, value: 0),
                new Segment
                (
                    startTime : latency,
                    rightLimitAtStartTime : 0,
                    slope : rate,
                    endTime : latency + DefaultPeriodLength
                )
            };
        }

        return new Sequence(elements);
    }

    private static readonly Rational DefaultPeriodLength = 1;

    #region Optimized Overrides

    /// <inheritdoc cref="Curve.Scale(Rational)"/>
    public override Curve Scale(Rational scaling)
    {
        logger.Trace("Optimized RL Scale");
        return new RateLatencyServiceCurve(rate: scaling * Rate, latency: Latency);
    }

    /// <inheritdoc cref="Curve.DelayBy(Rational)"/>
    public override Curve DelayBy(Rational delay)
    {
        logger.Trace("Optimized RL DelayBy");
        return new RateLatencyServiceCurve(rate: Rate, latency: delay + Latency);
    }


    /// <inheritdoc cref="Curve.Addition(Curve)"/>
    public override Curve Addition(Curve curve)
    {
        logger.Trace("Optimized RL Addition");
        if (curve is ConstantCurve bufferCurve)
            return new RaisedRateLatencyServiceCurve(Rate, Latency, bufferCurve.Value);
        else
            return base.Addition(curve);
    }

    /// <inheritdoc cref="Curve.VerticalShift(Rational, bool)"/>
    public override Curve VerticalShift(Rational shift, bool exceptOrigin = true)
    {
        if (exceptOrigin)
        {
            logger.Trace("Optimized RL VerticalShift");
            return new RaisedRateLatencyServiceCurve(Rate, Latency, shift);
        }
        else
            return base.VerticalShift(shift, exceptOrigin);
    }

    /// <inheritdoc cref="Curve.Convolution(Curve, ComputationSettings?)"/>
    public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
    {
        if (curve is RateLatencyServiceCurve dr)
        {
            return Convolution(dr, settings);
        }
        else
            return base.Convolution(curve, settings);
    }

    /// <summary>
    /// Computes the convolution of the two curves.
    /// </summary>
    /// <param name="dr"></param>
    /// <param name="settings"></param>
    /// <remarks>
    /// Optimized as another <see cref="RateLatencyServiceCurve"/> with the sum of the delays and the minimum of the rates.
    /// </remarks>
    public RateLatencyServiceCurve Convolution(RateLatencyServiceCurve dr, ComputationSettings? settings = null)
    {
        logger.Trace("Optimized RL Convolution");
        return new RateLatencyServiceCurve(rate: Rational.Min(Rate, dr.Rate), latency: Latency + dr.Latency);
    }

    /// <inheritdoc cref="RateLatencyServiceCurve.Convolution(RateLatencyServiceCurve, ComputationSettings?)"/>
    public static RateLatencyServiceCurve operator *(RateLatencyServiceCurve dr1, RateLatencyServiceCurve dr2)
    {
        return dr1.Convolution(dr2);
    }

    /// <summary>
    /// Computes the convolution of the set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <remarks>
    /// Optimized as a <see cref="RateLatencyServiceCurve"/> with the sum of the delays and the minimum of the rates.
    /// </remarks>
    public static RateLatencyServiceCurve Convolution(IReadOnlyCollection<RateLatencyServiceCurve> curves,
        ComputationSettings? settings = null)
    {
        return new RateLatencyServiceCurve(rate: curves.Min(c => c.Rate), latency: curves.Sum(c => c.Latency));
    }

    #endregion
}

/// <summary>
/// Provides LINQ extension methods for <see cref="RateLatencyServiceCurve"/>
/// </summary>
public static class RateLatencyServiceCurveExtensions
{
    /// <inheritdoc cref="RateLatencyServiceCurve.Convolution(RateLatencyServiceCurve, ComputationSettings?)"/>
    public static RateLatencyServiceCurve Convolution(this IReadOnlyCollection<RateLatencyServiceCurve> curves, ComputationSettings? settings = null)
        => RateLatencyServiceCurve.Convolution(curves);
}