using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using NLog;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A rate-latency service curve
/// </summary>
[JsonConverter(typeof(RateLatencyServiceCurveSystemJsonConverter))]
public class RateLatencyServiceCurve : ConvexCurve
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "rateLatencyServiceCurve";

    /// <summary>
    /// Minimum rate of service.
    /// </summary>
    [JsonPropertyName("rate")]
    public Rational Rate { get; }

    /// <summary>
    /// Maximum latency of service.
    /// </summary>
    [JsonPropertyName("latency")]
    public Rational Latency { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    [JsonConstructor]
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

    /// <inheritdoc cref="Curve.ToCodeString"/>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new RateLatencyServiceCurve({newline}");
        sb.Append($"{tabs(1)}{Rate.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}{Latency.ToCodeString()}{newline}");
        sb.Append($"{tabs(0)})");

        return sb.ToString();
        
        string tabs(int n)
        {
            if (!formatted)
                return "";
            var sbt = new StringBuilder();
            for (int i = 0; i < indentation + n; i++)
                sbt.Append("\t");
            return sbt.ToString();
        }
    }

    #region Optimized Overrides

    /// <inheritdoc cref="Curve.Scale(Rational)"/>
    public override Curve Scale(Rational scaling)
    {
        #if DO_LOG
        logger.Trace("Optimized RL Scale");
        #endif
        return new RateLatencyServiceCurve(rate: scaling * Rate, latency: Latency);
    }

    /// <inheritdoc cref="Curve.DelayBy(Rational)"/>
    public override Curve DelayBy(Rational delay)
    {
        #if DO_LOG
        logger.Trace("Optimized RL DelayBy");
        #endif
        return new RateLatencyServiceCurve(rate: Rate, latency: delay + Latency);
    }


    /// <inheritdoc cref="Curve.Addition(Curve, ComputationSettings)"/>
    public override Curve Addition(Curve curve, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Optimized RL Addition");
        #endif
        if (curve is ConstantCurve bufferCurve)
            return new RaisedRateLatencyServiceCurve(Rate, Latency, bufferCurve.Value);
        else
            return base.Addition(curve, settings);
    }

    /// <inheritdoc cref="Curve.VerticalShift(Rational, bool)"/>
    public override Curve VerticalShift(Rational shift, bool exceptOrigin = true)
    {
        if (exceptOrigin)
        {
            #if DO_LOG
            logger.Trace("Optimized RL VerticalShift");
            #endif
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
        #if DO_LOG
        logger.Trace("Optimized RL Convolution");
        #endif
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