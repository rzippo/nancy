using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Result of the sum of a <see cref="RateLatencyServiceCurve"/> and a <see cref="ConstantCurve"/>.
/// Used to optimize its sub-additive closure.
/// </summary>
[JsonConverter(typeof(RaisedRateLatencyServiceCurveSystemJsonConverter))]
public class RaisedRateLatencyServiceCurve : Curve
{
    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "raisedRateLatencyServiceCurve";

    /// <summary>
    /// Maximum latency of service.
    /// </summary>
    public Rational Latency { get; }

    /// <summary>
    /// Minimum rate of service.
    /// </summary>
    public Rational Rate { get; }

    /// <summary>
    /// Upwards shifting due to sum with buffer.
    /// </summary>
    public Rational BufferShift { get; }

    /// <summary>
    /// True if the value at the origin is left at 0, rather than raised by <see cref="BufferShift"/>.
    /// </summary>
    /// <remarks>
    /// It tells the sum with a <see cref="ConstantCurve"/>, which is 0 at the origin, from the sum with a constant, which is not.
    /// The two differ only at $t = 0$, and have the same <see cref="SubAdditiveClosure"/>.
    /// </remarks>
    public bool HasZeroOrigin { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="rate">Minimum rate of service.</param>
    /// <param name="latency">Maximum latency of service.</param>
    /// <param name="bufferShift">Upwards shifting due to sum with buffer.</param>
    /// <param name="withZeroOrigin">
    /// If false, which is the default, the value at the origin is raised by <paramref name="bufferShift"/> as well.
    /// If true, it is left at 0.
    /// </param>
    public RaisedRateLatencyServiceCurve(Rational rate, Rational latency, Rational bufferShift, bool withZeroOrigin = false)
        : base(
            baseSequence: BuildSequence(rate, latency, bufferShift, withZeroOrigin),
            pseudoPeriodStart: PeriodStart(latency, bufferShift),
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: PeriodHeight(latency, rate)
        )
    {
        Latency = latency;
        Rate = rate;
        BufferShift = bufferShift;
        HasZeroOrigin = withZeroOrigin;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor.
    /// </summary>
    internal static Sequence BuildSequence(Rational rate, Rational latency, Rational bufferShift, bool withZeroOrigin = false)
    {
        Element[] elements;
        if (latency == 0)
        {
            if (bufferShift == 0)
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
                    withZeroOrigin ? Point.Origin() : new Point(time: 0, value: bufferShift),
                    new Segment
                    (
                        startTime : 0,
                        rightLimitAtStartTime : bufferShift,
                        slope : rate,
                        endTime : PeriodStart(latency, bufferShift)
                    ),
                    new Point
                    (
                        time: PeriodStart(latency, bufferShift),
                        value: bufferShift + rate * PeriodStart(latency, bufferShift)
                    ),
                    new Segment
                    (
                        startTime : PeriodStart(latency, bufferShift),
                        rightLimitAtStartTime : bufferShift + rate * PeriodStart(latency, bufferShift),
                        slope : rate,
                        endTime : PeriodStart(latency, bufferShift) + DefaultPeriodLength
                    )
                };
            }
        }
        else
        {
            elements = new Element[]
            {
                withZeroOrigin ? Point.Origin() : new Point(time: 0, value: bufferShift),
                new Segment
                (
                    startTime : 0,
                    rightLimitAtStartTime : bufferShift,
                    slope : 0,
                    endTime : latency
                ),
                new Point(time: latency, value: bufferShift),
                new Segment
                (
                    startTime : latency,
                    rightLimitAtStartTime : bufferShift,
                    slope : rate,
                    endTime : latency + DefaultPeriodLength
                )
            };
        }

        return new Sequence(elements);
    }

    internal static Rational PeriodStart(Rational delay, Rational bufferShift) =>
        (delay == 0 && bufferShift > 0) ? DefaultPeriodLength : delay;

    internal static Rational PeriodHeight(Rational delay, Rational rate) =>
        rate * DefaultPeriodLength;

    internal static readonly Rational DefaultPeriodLength = 1;

    /// <summary>
    /// Computes the sub-additive closure of the curve.
    /// </summary>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Optimized via known closed-form expression.</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        // a negative shift is not described by a FlowControlCurve, whose steps would have a negative height
        if (BufferShift.IsNegative)
            return base.SubAdditiveClosure(settings);

        //Actual shape may not be staircase, but is guaranteed to be sub-additive
        return new FlowControlCurve(latency: Latency, rate: Rate, height: BufferShift);
    }
}