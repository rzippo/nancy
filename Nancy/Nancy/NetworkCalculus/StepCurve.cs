using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A curve that is 0 for any t less or equal to T,
/// and of constant value for t > T.
/// </summary>
public class StepCurve : Curve
{
    /// <summary>
    /// Value of the curve for any t > <see cref="StepTime"/>
    /// </summary>
    public Rational Value { get; }

    /// <summary>
    /// Time of the step.
    /// </summary>
    public Rational StepTime { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public StepCurve(Rational value, Rational stepTime)
        : base(
            baseSequence: new Sequence(new Element[]
            {
                Point.Origin(),
                Segment.Zero(0, stepTime),
                Point.Zero(stepTime),
                Segment.Constant(stepTime,
                    stepTime + DefaultPeriodLength, value),
                new Point(stepTime + DefaultPeriodLength, value),
                Segment.Constant(stepTime + DefaultPeriodLength,
                    stepTime + 2 * DefaultPeriodLength, value),
            }),
            pseudoPeriodStart: stepTime + DefaultPeriodLength,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: 0
        )
    {
        Value = value;
        StepTime = stepTime;
    }

    private static readonly Rational DefaultPeriodLength = 1;
}