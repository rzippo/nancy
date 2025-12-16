using System;
using System.Text;
using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// Stair curve defined in [TLBB21] as $v(t) = a \cdot \left\lceil \frac{t}{b} \right\rceil$.
/// It is left-continuous.
/// </summary>
[JsonConverter(typeof(StairCurveSystemJsonConverter))]
public class StairCurve : Curve
{
    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "stairCurve";

    /// <summary>
    /// Vertical height of each step.
    /// </summary>
    public Rational A { get; }

    /// <summary>
    /// Horizontal length of each step.
    /// </summary>
    public Rational B { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public StairCurve(Rational a, Rational b)
        : base(
            baseSequence: BuildSequence(a, b),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: b,
            pseudoPeriodHeight: a
        )
    {
        A = a;
        B = b;
    }

    private static Sequence BuildSequence(Rational a, Rational b)
    {
        if (a < 0)
            throw new ArgumentException("a must be >= 0");
        if (b <= 0)
            throw new ArgumentException("b must be > 0");

        if (a == 0)
            return Sequence.Zero(0, b);
        else
        {
            return new Sequence(new Element[]
            {
                Point.Origin(),
                Segment.Constant(0, b, a)
            });
        }
    }

    /// <inheritdoc cref="Curve.ToCodeString"/>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new StairCurve({newline}");
        sb.Append($"{tabs(1)}{A.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}{B.ToCodeString()}{newline}");
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
        return $"stair(0, {B.ToMppgString()}, {A.ToMppgString()})";
    }
}