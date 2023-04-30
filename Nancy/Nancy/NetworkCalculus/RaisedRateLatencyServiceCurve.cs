using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Result of the sum of a <see cref="RateLatencyServiceCurve"/> and a <see cref="ConstantCurve"/>.
/// Used to optimize its sub-additive closure.
/// </summary>
public class RaisedRateLatencyServiceCurve : Curve
{
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
    /// Constructor.
    /// </summary>
    public RaisedRateLatencyServiceCurve(Rational rate, Rational latency, Rational bufferShift,
        bool withZeroOrigin = false)
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
    }

    /// <summary>
    /// Builds the sequence for the base class constructor.
    /// </summary>
    internal static Sequence BuildSequence(Rational rate, Rational latency, Rational bufferShift, bool withZeroOrigin)
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
    /// <param name="settings"></param>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Optimized via known closed-form expression.</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        //Actual shape may not be staircase, but is guaranteed to be sub-additive
        return new FlowControlCurve(latency: Latency, rate: Rate, height: BufferShift);
    }
}