using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.NetworkCalculus;

/// <summary>
/// A pure delay service model.
/// Given delay $\tau$, $f(t) = 0$ if $t \le \tau$, $f(t) = +\infty$ otherwise.
/// </summary>
[JsonConverter(typeof(DelayServiceCurveSystemJsonConverter))]
public class DelayServiceCurve : SuperAdditiveCurve
{
    /// <summary>
    /// Type identification constant for JSON (de)serialization. 
    /// </summary>
    /// <exclude />
    public new const string TypeCode = "delayServiceCurve";

    /// <summary>
    /// Models service of a delay-only server.
    /// </summary>
    public Rational Delay { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DelayServiceCurve(Rational delay)
        : base(
            baseSequence: BuildSequence(delay),
            pseudoPeriodStart: delay > 0 ? 2 * delay : DefaultPeriodLength,
            pseudoPeriodLength:delay > 0 ? delay : DefaultPeriodLength,
            pseudoPeriodHeight: Rational.PlusInfinity,
            doTest: false
        )
    {
        Delay = delay;
    }

    private static Sequence BuildSequence(Rational delay)
    {
        if(delay.IsNegative)
            throw new ArgumentException($"Delay curve parameter must be a finite positive number: {delay} is not valid.", nameof(delay));
        if(delay.IsPlusInfinite)
            throw new ArgumentException($"Delay curve parameter must be a finite positive number: {delay} is not valid.", nameof(delay));
        
        if (delay == 0)
        {
            return new Sequence(new Element[]
            {
                Point.Origin(),
                Segment.PlusInfinite
                (
                    startTime: delay,
                    endTime: delay + DefaultPeriodLength
                ),
                Point.PlusInfinite(time: delay + DefaultPeriodLength),
                Segment.PlusInfinite
                (
                    startTime: delay + DefaultPeriodLength,
                    endTime: delay + 2 * DefaultPeriodLength
                )
            });
        }
        else
        {
            var periodLength = delay;
            return new Sequence(new Element[]
            {
                Point.Origin(),
                new Segment
                (
                    startTime: 0,
                    rightLimitAtStartTime: 0,
                    slope: 0,
                    endTime: delay
                ),
                new Point(time: delay, value: 0),
                Segment.PlusInfinite
                (
                    startTime: delay,
                    endTime: delay + periodLength
                ),
                new Point(time: delay + periodLength, value: Rational.PlusInfinity),
                Segment.PlusInfinite
                (
                    startTime: delay + periodLength,
                    endTime: delay + 2 * periodLength
                )
            });
        }
    }

    private static readonly Rational DefaultPeriodLength = 1;

    /// <inheritdoc cref="Curve.ToCodeString"/>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new DelayServiceCurve({newline}");
        sb.Append($"{tabs(1)}{Delay.ToCodeString()}{newline}");
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
        return $"delay({Delay.ToMppgString()})";
    }

    /// <inheritdoc cref="Curve.Convolution(Curve, ComputationSettings?)"/>
    public override Curve Convolution(Curve curve, ComputationSettings? settings = null)
    {
        if (Delay == 0)
            return curve;
        else if (curve is DelayServiceCurve d)
            return d.Delay == 0 ? this : new DelayServiceCurve(Delay + d.Delay);
        else
            return curve.DelayBy(Delay);
    }

    /// <summary>
    /// Computes the convolution of the two curves.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <remarks>
    /// Optimized as another <see cref="DelayServiceCurve"/> with the sum of the delays.
    /// </remarks>
    public DelayServiceCurve Convolution(DelayServiceCurve curve, ComputationSettings? settings = null)
    {
        if (Delay == 0)
            return curve;
        else if (curve.Delay == 0)
            return this; 
        else 
            return new DelayServiceCurve(Delay + curve.Delay);
    }

    /// <inheritdoc cref="DelayServiceCurve.Convolution(DelayServiceCurve, ComputationSettings?)"/>
    public static DelayServiceCurve operator *(DelayServiceCurve dr1, DelayServiceCurve dr2)
    {
        return dr1.Convolution(dr2);
    }

    /// <summary>
    /// Computes the convolution of the set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <remarks>
    /// Optimized as a <see cref="DelayServiceCurve"/> with the sum of the delays.
    /// </remarks>
    public static DelayServiceCurve Convolution(IEnumerable<DelayServiceCurve> curves, ComputationSettings? settings = null)
        => new DelayServiceCurve(curves.Sum(c => c.Delay));
}

/// <summary>
/// Provides LINQ extension methods for <see cref="DelayServiceCurve"/>
/// </summary>
public static class DelayServiceCurveExtensions
{    
    /// <inheritdoc cref="DelayServiceCurve.Convolution(DelayServiceCurve, ComputationSettings?)"/>
    public static DelayServiceCurve Convolution(this IEnumerable<DelayServiceCurve> curves, ComputationSettings? settings = null)
        => DelayServiceCurve.Convolution(curves);
}