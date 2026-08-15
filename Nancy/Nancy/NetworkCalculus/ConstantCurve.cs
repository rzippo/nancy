using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A constant curve, with 0 at the origin.
/// It is equivalent to a step function with stepTime = 0.
/// Sub-additive unless the value is finite and negative, which is why this type does not derive from <see cref="SubAdditiveCurve"/>.
/// </summary>
/// <remarks>
/// The curve is ultimately affine, so the algorithms of [ZS23] that <see cref="SubAdditiveCurve"/> provides
/// were measured to cost as much as the general ones on this shape.
/// The one they did save, the sub-additive closure, is provided here instead.
/// </remarks>
[JsonConverter(typeof(ConstantCurveSystemJsonConverter))]
public class ConstantCurve : Curve
{
    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "constantCurve";

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
        // sub-additivity fails only for a finite negative value: at $-\infty$, $f(t+s)$ and $f(t) + f(s)$ are both $-\infty$
        if (!(value.IsFinite && value.IsNegative))
            _IsSubAdditive = true;
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
        else if (value.IsPlusInfinite)
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
        else
        {
            return new Sequence(
                new Element[]
                {
                    Point.Origin(),
                    Segment.MinusInfinite(
                        0,
                        DefaultPeriodLength),
                    Point.MinusInfinite(DefaultPeriodLength),
                    Segment.MinusInfinite(
                        DefaultPeriodLength,
                        2 * DefaultPeriodLength)
                });
        }
    }

    internal static readonly Rational DefaultPeriodLength = 1;

    /// <inheritdoc cref="Curve.SubAdditiveClosure(ComputationSettings?)"/>
    /// <remarks>
    /// A constant curve that is sub-additive is its own sub-additive closure, so the general algorithm is skipped.
    /// </remarks>
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        if (Value.IsFinite && Value.IsNegative)
            return base.SubAdditiveClosure(settings);
        return new SubAdditiveCurve(this, false);
    }

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

    /// <inheritdoc cref="Curve.Addition(Curve, ComputationSettings)"/>
    public override Curve Addition(Curve curve, ComputationSettings? settings = null)
    {
        if (curve is RateLatencyServiceCurve serviceCurve)
            return new RaisedRateLatencyServiceCurve(serviceCurve.Rate, serviceCurve.Latency, Value);
        else
            return base.Addition(curve, settings);
    }
}