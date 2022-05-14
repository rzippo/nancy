using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <exclude />
/// <summary>
/// A continuous two-rates service curve.
/// </summary>
public class TwoRatesServiceCurve : Curve
{
    /// <summary>
    /// Maximum delay of service.
    /// </summary>
    public Rational Delay { get; }

    /// <summary>
    /// Minimum rate of service during transient.
    /// </summary>
    public Rational TransientRate { get; }

    /// <summary>
    /// Time of ending of transient.
    /// </summary>
    public Rational TransientEnd { get; }

    /// <summary>
    /// Minimum rate of service after transient.
    /// </summary>
    public Rational SteadyRate { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="transientRate"></param>
    /// <param name="transientEnd"></param>
    /// <param name="steadyRate"></param>
    public TwoRatesServiceCurve(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)
        : base(
            baseSequence: BuildSequence(delay, transientRate, transientEnd, steadyRate),
            pseudoPeriodStart: transientEnd,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: steadyRate * DefaultPeriodLength
        )
    {
        Delay = delay;
        TransientRate = transientRate;
        TransientEnd = transientEnd;
        SteadyRate = steadyRate;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor.
    /// </summary>
    private static Sequence BuildSequence(Rational delay, Rational transientRate, Rational transientEnd, Rational steadyRate)
    {
        if (delay > transientEnd)
            throw new ArgumentException("Delay time must precede transient end time");

        var endTransientValue = transientRate * (transientEnd - delay);

        Element[] elements;
        if (delay == 0)
        {
            elements = new Element[]
            {
                Point.Origin(),
                new Segment
                (
                    startTime : 0,
                    rightLimitAtStartTime : 0,
                    slope : transientRate,
                    endTime : transientEnd
                ),
                new Point(
                    time: transientEnd,
                    value: endTransientValue
                ),
                new Segment
                (
                    startTime : transientEnd,
                    rightLimitAtStartTime : endTransientValue,
                    slope : steadyRate,
                    endTime : transientEnd + DefaultPeriodLength
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
                    endTime : delay
                ),
                new Point(time: delay, value: 0),
                new Segment
                (
                    startTime : delay,
                    rightLimitAtStartTime : 0,
                    slope : transientRate,
                    endTime : transientEnd
                ),
                new Point(
                    time: transientEnd,
                    value: endTransientValue
                ),
                new Segment
                (
                    startTime : transientEnd,
                    rightLimitAtStartTime : endTransientValue,
                    slope : steadyRate,
                    endTime : transientEnd + DefaultPeriodLength
                )
            };
        }

        return new Sequence(elements);
    }

    private static readonly Rational DefaultPeriodLength = 1;
}