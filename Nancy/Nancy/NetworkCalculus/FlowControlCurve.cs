using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Result of the sub-additive closure of a rate-latency service curve and a constant, as used in static window flow control: $\overline{\beta_{R, \theta} + W}$.
/// For completeness, also degenerate cases are defined.
/// </summary>
public class FlowControlCurve : SubAdditiveCurve
{
    /// <summary>
    /// Time in-between a step and the next one.
    /// </summary>
    public Rational Latency { get; }

    /// <summary>
    /// Rate while stepping up.
    /// </summary>
    public Rational Rate { get; }

    /// <summary>
    /// Height of a step.
    /// </summary>
    public Rational Height { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public FlowControlCurve(Rational latency, Rational rate, Rational height)
        : base(
            baseSequence: BuildSequence(latency, rate, height),
            pseudoPeriodStart: PeriodStart(latency, rate, height),
            pseudoPeriodLength: PeriodLength(latency, rate, height),
            pseudoPeriodHeight: PeriodHeight(latency, rate, height)
        )
    {
        Latency = latency;
        Rate = rate;
        Height = height;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor
    /// </summary>
    private static Sequence BuildSequence(Rational latency, Rational rate, Rational height)
    {
        if (height >= rate * latency)
        {
            return RaisedRateLatencyServiceCurve.BuildSequence(rate, latency, bufferShift: height, withZeroOrigin: true);
        }
        else
        {
            if (height == 0)
            {
                return ConstantCurve.BuildSequence(0);
            }
            else
            {
                var secondBufferFillTime = latency + height / rate;

                return new Sequence(
                    new Element[]
                    {
                        Point.Origin(),
                        new Segment
                        (
                            startTime : 0,
                            rightLimitAtStartTime : height,
                            slope : 0,
                            endTime : latency
                        ),
                        new Point(time: latency, value: height),
                        new Segment
                        (
                            startTime : latency,
                            rightLimitAtStartTime : height,
                            slope : rate,
                            endTime : secondBufferFillTime
                        ),
                        new Point(time: secondBufferFillTime, value: 2 * height),
                        new Segment(
                            startTime : secondBufferFillTime,
                            rightLimitAtStartTime : 2 * height,
                            slope : 0,
                            endTime : 2 * latency
                        )
                    }
                );
            }
        }
    }

    private static Rational PeriodStart(Rational latency, Rational rate, Rational height)
    {
        if (height >= rate * latency)
        {
            return RaisedRateLatencyServiceCurve.PeriodStart(latency, bufferShift: height);
        }
        else
        {
            if (height == 0)
            {
                return ConstantCurve.DefaultPeriodLength;
            }
            else
            {
                return latency;
            }
        }
    }

    private static Rational PeriodLength(Rational latency, Rational rate, Rational height)
    {
        if (height >= rate * latency)
        {
            return RaisedRateLatencyServiceCurve.DefaultPeriodLength;
        }
        else
        {
            if (height == 0)
            {
                return ConstantCurve.DefaultPeriodLength;
            }
            else
            {
                return latency;
            }
        }
    }

    private static Rational PeriodHeight(Rational latency, Rational rate, Rational height)
    {
        if (height >= rate * latency)
        {
            return RaisedRateLatencyServiceCurve.PeriodHeight(latency, rate);
        }
        else
        {
            return height;
        }
    }

    private static readonly Rational DefaultPeriodLength = 1;

    /// <summary>
    /// Computes the convolution of a set of staircase curves.
    /// </summary>
    /// <param name="curves">The set of staircase curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall convolution.</returns>
    public static SubAdditiveCurve Convolution(IEnumerable<FlowControlCurve> curves, ComputationSettings? settings = null)
        => SubAdditiveCurve.Convolution(curves, settings);
}