using System.Text;
using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A curve that is 0 for any t less or equal to T,
/// and of constant value for t > T.
/// It is left-continuous.
/// </summary>
[JsonConverter(typeof(StepCurveSystemJsonConverter))]
public class StepCurve : Curve
{
    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "stepCurve";

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
            baseSequence: BuildSequence(value, stepTime),
            pseudoPeriodStart: stepTime + DefaultPeriodLength,
            pseudoPeriodLength: DefaultPeriodLength,
            pseudoPeriodHeight: 0
        )
    {
        Value = value;
        StepTime = stepTime;
    }

    /// <summary>
    /// Builds the sequence for the base class constructor
    /// </summary>
    private static Sequence BuildSequence(Rational value, Rational stepTime)
    {
        if (stepTime == 0)
        {
            return new Sequence([
                Point.Origin(),
                Segment.Constant(0, DefaultPeriodLength, value),
                new Point(DefaultPeriodLength, value),
                Segment.Constant(DefaultPeriodLength, 2 * DefaultPeriodLength, value)
            ]);
        }
        else
        {
            return new Sequence([
                Point.Origin(),
                Segment.Zero(0, stepTime),
                Point.Zero(stepTime),
                Segment.Constant(stepTime, stepTime + DefaultPeriodLength, value),
                new Point(stepTime + DefaultPeriodLength, value),
                Segment.Constant(stepTime + DefaultPeriodLength, stepTime + 2 * DefaultPeriodLength, value)
            ]);
        }
    }
    
    private static readonly Rational DefaultPeriodLength = 1;

    /// <inheritdoc cref="Curve.ToCodeString"/>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new StepCurve({newline}");
        sb.Append($"{tabs(1)}{Value.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}{StepTime.ToCodeString()}{newline}");
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

    /// <inheritdoc cref="Curve.ToMppgString"/>
    public override string ToMppgString()
    {
        return $"step({StepTime.ToMppgString()}, {Value.ToMppgString()})";
    }
}