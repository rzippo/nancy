using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A constant curve, with 0 at the origin.
/// It is equivalent to a step function with stepTime = 0.
/// Sub-additive.
/// </summary>
public class ConstantCurve : SubAdditiveCurve
{
    /// <summary>
    /// Value of the curve for any t > 0
    /// </summary>
    public Rational Value { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public ConstantCurve(Rational value)
        : base(
            baseSequence: BuildSequence(value),
            pseudoPeriodStart: DefaultPeriodLength,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: 0
        )
    {
        Value = value;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor
    /// </summary>
    internal static Sequence BuildSequence(Rational value)
    {
        if (value.IsFinite)
        {
            return new Sequence(
                new Element[]
                {
                    Point.Origin(),
                    Segment.Constant(0,
                        DefaultPeriodLength, value),
                    new Point(DefaultPeriodLength, value),
                    Segment.Constant(DefaultPeriodLength,
                        2 * DefaultPeriodLength, value)
                });
        }
        else
        {
            return new Sequence(
                new Element[]
                {
                    Point.Origin(),
                    Segment.PlusInfinite(
                        0,
                        DefaultPeriodLength),
                    Point.PlusInfinite(DefaultPeriodLength),
                    Segment.PlusInfinite(
                        DefaultPeriodLength,
                        2 * DefaultPeriodLength)
                });
        }
    }

    internal static readonly Rational DefaultPeriodLength = 1;

    /// <inheritdoc cref="Curve.VerticalShift(Rational, bool)"/>
    public override Curve VerticalShift(Rational shift, bool exceptOrigin = true)
    {
        if (shift == 0)
            return this;
        if (exceptOrigin)
            return new ConstantCurve(Value + shift);
        else
            return base.VerticalShift(shift, exceptOrigin);
    }

    /// <inheritdoc cref="Curve.Addition(Curve)"/>
    public override Curve Addition(Curve curve)
    {
        if (curve is RateLatencyServiceCurve serviceCurve)
            return new RaisedRateLatencyServiceCurve(serviceCurve.Rate, serviceCurve.Latency, Value);
        else
            return base.Addition(curve);
    }
}