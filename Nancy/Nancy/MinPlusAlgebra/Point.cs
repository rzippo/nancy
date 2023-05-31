using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// A point of a function. Defined only in <see cref="Time"/>.
/// </summary>
/// <remarks>
/// From unit structure defined in [BT07] Section 4.1
/// </remarks>
/// <docs position="3"/>
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public sealed class Point : Element, IEquatable<Point>
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    #region Properties

    /// <summary>
    /// Time for which the point is defined.
    /// </summary>
    [JsonProperty(PropertyName = "time")]
    public Rational Time { get; }

    /// <summary>
    /// Value of the point.
    /// </summary>
    [JsonProperty(PropertyName = "value")]
    public Rational Value { get; }

    /// <inheritdoc />
    public override Rational StartTime => Time;

    /// <inheritdoc />
    public override Rational EndTime => Time;

    /// <summary>
    /// Slope, w.r.t. origin, of the point.
    /// </summary>
    public Rational PointSlope => Value / Time;

    /// <summary>
    /// True if the value of the point is $\pm\infty$.
    /// </summary>
    public override bool IsInfinite => Value.IsInfinite;

    /// <summary>
    /// True if the value of the point is $+\infty$.
    /// </summary>
    public override bool IsPlusInfinite => Value.IsPlusInfinite;

    /// <summary>
    /// True if the value of the point is $-\infty$.
    /// </summary>
    public override bool IsMinusInfinite => Value.IsMinusInfinite;

    /// <summary>
    /// True if the value of the point is 0.
    /// </summary>
    public override bool IsZero
        => Value.IsZero;

    /// <summary>
    /// True if the point is $(0, 0)$
    /// </summary>
    public bool IsOrigin
        => Time.IsZero && Value.IsZero;

    #endregion Properties

    #region Serialization

    /// <summary>
    /// Type identification constant for JSON (de)serialization.
    /// </summary>
    /// <exclude />
    public const string TypeCode = "point";

    /// <summary>
    /// Type identification property for JSON (de)serialization.
    /// </summary>
    [JsonProperty(PropertyName = "type")]
    public override string Type { get; } = TypeCode;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="time">Time of the point.</param>
    /// <param name="value">Value of the point.</param>
    public Point(Rational time, Rational value)
    {
        Time = time;
        Value = value;
    }

    /// <summary>
    /// Constructs a point with 0 as value.
    /// </summary>
    /// <param name="time">Time of the point.</param>
    public static Point Zero(Rational time)
    {
        return new Point(
            time: time,
            value: 0
        );
    }

    /// <summary>
    /// Constructs a point in (0, 0).
    /// </summary>
    public static Point Origin()
    {
        return new Point(time: 0, value: 0);
    }

    /// <summary>
    /// Constructs a point with $+\infty$ as value.
    /// </summary>
    /// <param name="time">Time of the point.</param>
    public static Point PlusInfinite(Rational time)
    {
        return new Point(
            time: time,
            value: Rational.PlusInfinity
        );
    }

    /// <summary>
    /// Constructs a point with $-\infty$ as value.
    /// </summary>
    /// <param name="time">Time of the point.</param>
    public static Point MinusInfinite(Rational time)
    {
        return new Point(
            time: time,
            value: Rational.MinusInfinity
        );
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override Rational ValueAt(Rational time)
    {
        return time == Time ? Value : Rational.PlusInfinity;
    }

    /// <inheritdoc />
    public override bool IsDefinedFor(Rational time)
    {
        return time == Time;
    }

    /// <summary>
    /// Deserializes a <see cref="Point"/>.
    /// </summary>
    public new static Point FromJson(string json)
    {
        var point = JsonConvert.DeserializeObject<Point>(json, new RationalConverter());
        if (point == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return point;
    }

    /// <summary>
    /// Returns a string containing C# code to create this Point.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var sb = new StringBuilder();
        sb.Append("new Point(");
        sb.Append($"{Time.ToCodeString()},");
        sb.Append($"{Value.ToCodeString()}");
        sb.Append(")");

        return sb.ToString();
    }

    #endregion Methods

    #region Basic manipulations

    /// <summary>
    /// Scales the point by a multiplicative factor.
    /// </summary>
    public override Element Scale(Rational scaling)
    {
        return new Point(
            time: Time,
            value: Value * scaling
        );
    }

    /// <inheritdoc />
    public override Element Delay(Rational delay)
    {
        return new Point(
            time: Time + delay,
            value: Value
        );
    }

    /// <inheritdoc />
    public override Element Anticipate(Rational time)
    {
        return new Point(
            time: Time - time,
            value: Value
        );
    }

    /// <summary>
    /// Shifts the point vertically by an additive factor.
    /// </summary>
    public override Element VerticalShift(Rational shift)
    {
        return new Point(
            time: Time,
            value: Value + shift
        );
    }

    /// <inheritdoc />
    public override Element Negate()
    {
        if (IsZero)
            return this;

        return new Point(
            time: Time,
            value: -Value
        );
    }

    /// <inheritdoc />
    public override Element Inverse()
    {
        return new Point(
            time: Value,
            value: Time
        );
    }

    #endregion Basic manipulations

    #region Equality methods

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => (obj is Point point) && Equals(point);

    /// <inheritdoc />
    public override int GetHashCode()
        => (Value, Time).GetHashCode();

    /// <inheritdoc />
    public bool Equals(Point? other)
        => other is not null && (Value, Time) == (other.Value, other.Time);

    /// <summary>
    /// Returns <code>true</code> if its operands are equal, <code>false</code> otherwise
    /// </summary>
    public static bool operator ==(Point? a, Point? b) =>
        Equals(a, b);

    /// <summary>
    /// Returns <code>false</code> if its operands are equal, <code>true</code> otherwise
    /// </summary>
    public static bool operator !=(Point? a, Point? b) =>
        !Equals(a, b);

    #endregion Equality methods

    #region Basic comparisons

    /// <summary>
    /// Returns true if the first point has always value higher than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool IsCertainlyAbove(Point a, Point b)
        => a.Value > b.Value;

    /// <summary>
    /// Returns true if the first point has always value higher than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public bool IsCertainlyAbove(Point point)
        => IsCertainlyAbove(a: this, b: point);

    /// <summary>
    /// Returns true if the first point has always value higher than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool operator >(Point a, Point b)
        => IsCertainlyAbove(a, b);

    /// <summary>
    /// Returns true if the first point has always value lower than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool IsCertainlyBelow(Point a, Point b)
        => a.Value < b.Value;

    /// <summary>
    /// Returns true if the first point has always value lower than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public bool IsCertainlyBelow(Point point)
        => IsCertainlyBelow(a: this, b: point);

    /// <summary>
    /// Returns true if the first point has always value lower than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool operator <(Point a, Point b)
        => IsCertainlyBelow(a, b);

    #endregion Basic comparisons

    #region Addition and Subtraction operators

    /// <summary>
    /// Sums the <see cref="Point"/> with an overlapping <see cref="Element"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the point and the element do not overlap.</exception>
    /// <returns>The element resulting from the sum.</returns>
    public override Element Addition(Element element)
    {
        switch (element)
        {
            case Point p:
                return Addition(this, p);
            case Segment s:
                return Addition(this, s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Sums two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point Addition(Point a, Point b)
    {
        if (a.Time != b.Time)
            throw new ArgumentException("The two points do not overlap.");

        return new Point(
            time: a.Time,
            value: a.Value + b.Value
        );
    }

    /// <summary>
    /// Sums two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public Point Addition(Point point)
        => Addition(a: this, b: point);

    /// <summary>
    /// Sums two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point operator +(Point a, Point b)
        => Addition(a, b);

    /// <summary>
    /// Sums a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point Addition(Point point, Segment segment)
    {
        if (!segment.IsDefinedFor(point.Time))
            throw new ArgumentException("Point and segment do not overlap.");

        return new Point(
            time: point.Time,
            value: point.Value + segment.ValueAt(point.Time)
        );
    }

    /// <summary>
    /// Sums a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public Point Addition(Segment segment)
        => Addition(point: this, segment);

    /// <summary>
    /// Sums a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point operator +(Point point, Segment segment)
        => Addition(point, segment);

    /// <summary>
    /// Subtracts the <see cref="Point"/> with an overlapping <see cref="Element"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the point and the element do not overlap.</exception>
    /// <returns>The element resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public override Element Subtraction(Element element)
    {
        switch (element)
        {
            case Point p:
                return Subtraction(this, p);
            case Segment s:
                return Subtraction(this, s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Subtracts two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point Subtraction(Point a, Point b)
    {
        if (a.Time != b.Time)
            throw new ArgumentException("The two points do not overlap.");

        return new Point(
            time: a.Time,
            value: a.Value - b.Value
        );
    }

    /// <summary>
    /// Subtracts two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public Point Subtraction(Point point)
        => Subtraction(a: this, b: point);

    /// <summary>
    /// Subtracts two <see cref="Point"/>s that are defined for the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point operator -(Point a, Point b)
        => Subtraction(a, b);

    /// <summary>
    /// Subtracts a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point Subtraction(Point point, Segment segment)
    {
        if (!segment.IsDefinedFor(point.Time))
            throw new ArgumentException("Point and segment do not overlap.");

        return new Point(
            time: point.Time,
            value: point.Value - segment.ValueAt(point.Time)
        );
    }

    /// <summary>
    /// Subtracts a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public Point Subtraction(Segment segment)
        => Subtraction(point: this, segment);

    /// <summary>
    /// Subtracts a <see cref="Point"/> to an overlapping <see cref="Segment"/>
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point operator -(Point point, Segment segment)
        => Subtraction(point, segment);

    #endregion Addition and Subtraction operators

    #region Minimum operator

    /// <summary>
    /// Computes the minimum of the <see cref="Point"/> and the given <see cref="Element"/> over their overlapping part.
    /// </summary>
    /// <returns>The <see cref="Point"/>s resulting from the minimum, wrapped in a list due to inheritance.</returns>
    public override List<Element> Minimum(Element element)
    {
        switch(element)
        {
            case Point p:
                return new List<Element> { Minimum(this, p) };
            case Segment s:
                return new List<Element> { Minimum(this, s) };

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the minimum of two <see cref="Point"/>s that are defined at the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the minimum.</returns>
    public static Point Minimum(Point a, Point b)
    {
        if (a.Time != b.Time)
            throw new ArgumentException("The two points do not overlap.");

        return new Point(
            time: a.Time,
            value: Rational.Min(a.Value, b.Value)
        );
    }

    /// <summary>
    /// Computes the minimum of two <see cref="Point"/>s that are defined at the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the minimum.</returns>
    public Point Minimum(Point b)
        => Minimum(this, b);

    /// <summary>
    /// Computes the minimum of a <see cref="Point"/> to an overlapping <see cref="Segment"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the minimum.</returns>
    public static Point Minimum(Point point, Segment segment)
    {
        if (!segment.IsDefinedFor(point.Time))
            throw new ArgumentException("Point and segment do not overlap.");

        return new Point(
            time: point.Time,
            value: Rational.Min(point.Value, segment.ValueAt(point.Time))
        );
    }

    /// <summary>
    /// Computes the minimum of a <see cref="Point"/> to an overlapping <see cref="Segment"/>.
    /// </summary>
    /// <param name="segment">Second operand</param>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the minimum.</returns>
    public Point Minimum(Segment segment)
        => Minimum(point: this, segment);

    /// <summary>
    /// Computes the minimum of a set of points.
    /// </summary>
    /// <param name="points">Points of which the minimum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the points do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of points is empty.</exception>
    /// <returns>The point resulting from the overall minimum.</returns>
    public static Point Minimum(IReadOnlyList<Point> points)
    {
        if(!points.Any())
            throw new InvalidOperationException("The enumerable is empty.");
        if (points.Count() == 1)
            return points.Single();

        var time = points.First().Time;
        if (points.Any(p => p.Time != time))
            throw new ArgumentException("The points do not overlap.");

        var minValue = points.Min(p => p.Value);
        return new Point(time: time, value: minValue);
    }

    #endregion Minimum operator

    #region Maximum operator

    /// <summary>
    /// Computes the maximum of the <see cref="Point"/> and the given <see cref="Element"/> over their overlapping part.
    /// </summary>
    /// <returns>The <see cref="Point"/>s resulting from the maximum, wrapped in a list due to inheritance.</returns>
    public override List<Element> Maximum(Element element)
    {
        switch (element)
        {
            case Point p:
                return new List<Element> { Maximum(this, p) };
            case Segment s:
                return new List<Element> { Maximum(this, s) };

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the maximum of two <see cref="Point"/>s that are defined at the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the maximum.</returns>
    public static Point Maximum(Point a, Point b)
    {
        if (a.Time != b.Time)
            throw new ArgumentException("The two points do not overlap.");

        return new Point(
            time: a.Time,
            value: Rational.Max(a.Value, b.Value)
        );
    }

    /// <summary>
    /// Computes the maximum of two <see cref="Point"/>s that are defined at the same time.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the two points do not overlap.</exception>
    /// <returns>The point resulting from the maximum.</returns>
    public Point Maximum(Point b)
        => Maximum(this, b);

    /// <summary>
    /// Computes the maximum of a <see cref="Point"/> to an overlapping <see cref="Segment"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the maximum.</returns>
    public static Point Maximum(Point point, Segment segment)
    {
        if (!segment.IsDefinedFor(point.Time))
            throw new ArgumentException("Point and segment do not overlap.");

        return new Point(
            time: point.Time,
            value: Rational.Max(point.Value, segment.ValueAt(point.Time))
        );
    }

    /// <summary>
    /// Computes the maximum of a <see cref="Point"/> to an overlapping <see cref="Segment"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the maximum.</returns>
    public Point Maximum(Segment segment)
        => Maximum(this, segment);

    /// <summary>
    /// Computes the maximum of a set of points.
    /// </summary>
    /// <param name="points">Points of which the maximum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the points do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of points is empty.</exception>
    /// <returns>The point resulting from the overall maximum.</returns>
    public static Point Maximum(IReadOnlyList<Point> points)
    {
        if(!points.Any())
            throw new InvalidOperationException("The enumerable is empty.");
        if (points.Count() == 1)
            return points.Single();

        var time = points.First().Time;
        if (points.Any(p => p.Time != time))
            throw new ArgumentException("The points do not overlap.");

        var minValue = points.Max(p => p.Value);
        return new Point(time: time, value: minValue);
    }

    #endregion Maximum operator

    #region Convolution operator 

    /// <summary>
    /// Computes the convolution between the <see cref="Point"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The <see cref="Element"/>s resulting from the convolution, wrapped in a list due to inheritance.</returns>
    public override IEnumerable<Element> Convolution(
        Element element, Rational? cutEnd = null, Rational? cutCeiling = null
    )
    {
        cutEnd ??= Rational.PlusInfinity;
        cutCeiling ??= Rational.PlusInfinity;
        if (this.IsOrigin)
        {
            if (element is Segment s && (cutEnd != Rational.PlusInfinity || cutCeiling != Rational.PlusInfinity))
            {
                var tCE = cutEnd ?? Rational.PlusInfinity;
                var tCC = (cutCeiling != Rational.PlusInfinity && s is { IsFinite: true, Slope.IsPositive: true }) 
                    ? (((Rational) cutCeiling - s.RightLimitAtStartTime) / s.Slope) + s.StartTime 
                    : Rational.PlusInfinity;
                var t = Rational.Min(tCE, tCC);
                if (t <= s.StartTime)
                {
                    // yield nothing
                }
                else if (t < s.EndTime)
                {
                    var (l, c, _) = s.Split(t);
                    yield return l;
                    yield return c;
                }
                else
                    yield return s;
            }
            else
                yield return element;
            yield break;
        }

        switch (element)
        {
            case Point p:
            {
                yield return Convolution(a: this, b: p);
                yield break;
            }
            case Segment s:
            {
                var cs = Segment.Convolution(segment: s, point: this);
                if (cutEnd != Rational.PlusInfinity || cutCeiling != Rational.PlusInfinity)
                {
                    var tCE = cutEnd ?? Rational.PlusInfinity;
                    var tCC = (cutCeiling != Rational.PlusInfinity && cs is { IsFinite: true, Slope.IsPositive: true }) 
                        ? (((Rational) cutCeiling - cs.RightLimitAtStartTime) / cs.Slope) + cs.StartTime
                        : Rational.PlusInfinity;
                    var t = Rational.Min(tCE, tCC);
                    if (t <= cs.StartTime)
                    {
                        // yield nothing
                    }
                    else if (t < cs.EndTime)
                    {
                        var (l, c, _) = cs.Split(t);
                        yield return l;
                        yield return c;
                    }
                    else
                        yield return cs;
                }
                else
                    yield return cs;
                yield break;
            }
            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the convolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 2</remarks>
    public static Point Convolution(Point a, Point b)
    {
        return new Point(time: a.StartTime + b.StartTime, value: a.Value + b.Value);
    }

    /// <summary>
    /// Computes the convolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 2</remarks>
    public Point Convolution(Point point)
        => Convolution(a: this, b: point);

    /// <summary>
    /// Computes the convolution between a <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 3</remarks>
    public static Segment Convolution(Point point, Segment segment)
        => Segment.Convolution(segment: segment, point: point);

    /// <summary>
    /// Computes the convolution between the <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 3</remarks>
    public Segment Convolution(Segment segment)
        => Segment.Convolution(segment: segment, point: this);

    #endregion Convolution operator

    #region Deconvolution operator

    /// <summary>
    /// Computes the deconvolution between the <see cref="Point"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The <see cref="Element"/>s resulting from the deconvolution, wrapped in a list due to inheritance.</returns>
    public override IEnumerable<Element> Deconvolution(Element element)
    {
        switch (element)
        {
            case Point p:
                yield return Deconvolution(a: this, b: p);
                break;
            case Segment s:
                yield return Deconvolution(point: this, segment: s);
                break;

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the deconvolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 5</remarks>
    public static Point Deconvolution(Point a, Point b)
    {
        return new Point(time: a.Time - b.Time, value: a.Value - b.Value);
    }

    /// <summary>
    /// Computes the deconvolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 5</remarks>
    public Point Deconvolution(Point point)
        => Deconvolution(a: this, b: point);

    /// <summary>
    /// Computes the deconvolution between a <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 7</remarks>
    public static Segment Deconvolution(Point point, Segment segment)
    {
        if (point.IsInfinite || segment.IsInfinite)
            throw new ArgumentException("The arguments must be finite.");

        return new Segment(
            startTime: point.Time - segment.EndTime,
            endTime: point.Time - segment.StartTime,
            rightLimitAtStartTime: point.Value - segment.RightLimitAtStartTime - segment.Slope * segment.Length,
            slope: segment.Slope
        );
    }

    /// <summary>
    /// Computes the deconvolution between the <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 7</remarks>
    public Segment Deconvolution(Segment segment)
        => Deconvolution(point: this, segment);

    #endregion Deconvolution operator

    #region Max-Plus Convolution operator 

    /// <summary>
    /// Computes the max-plus convolution between the <see cref="Point"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The <see cref="Element"/>s resulting from the max-plus convolution, wrapped in a list due to inheritance.</returns>
    public override IEnumerable<Element> MaxPlusConvolution(Element element, Rational? cutEnd = null)
    {
        cutEnd ??= Rational.PlusInfinity;
        if (this.IsOrigin)
        {
            if (element is Segment s && cutEnd != Rational.PlusInfinity)
            {
                var tCE = cutEnd ?? Rational.PlusInfinity;
                if (tCE <= s.StartTime)
                {
                    // yield nothing
                }
                else if (tCE < s.EndTime)
                {
                    var (l, c, _) = s.Split(tCE);
                    yield return l;
                    yield return c;
                }
                else
                    yield return s;
            }
            else
                yield return element;
            yield break;
        }

        switch (element)
        {
            case Point p:
            {
                yield return MaxPlusConvolution(a: this, b: p);
                yield break;
            }
            case Segment s:
            {
                var cs = Segment.MaxPlusConvolution(segment: s, point: this);
                if (cutEnd != Rational.PlusInfinity)
                {
                    var tCE = cutEnd ?? Rational.PlusInfinity;
                    if (tCE <= cs.StartTime)
                    {
                        // yield nothing
                    }
                    else if (tCE < cs.EndTime)
                    {
                        var (l, c, _) = cs.Split(tCE);
                        yield return l;
                        yield return c;
                    }
                    else
                        yield return cs;
                }
                else
                    yield return cs;
                yield break;
            }
            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the max-plus convolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 2</remarks>
    public static Point MaxPlusConvolution(Point a, Point b)
    {
        return new Point(time: a.StartTime + b.StartTime, value: a.Value + b.Value);
    }

    /// <summary>
    /// Computes the max-plus convolution between two <see cref="Point"/>s.
    /// </summary>
    /// <returns>The <see cref="Point"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 2</remarks>
    public Point MaxPlusConvolution(Point point)
        => MaxPlusConvolution(a: this, b: point);

    /// <summary>
    /// Computes the max-plus convolution between a <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 3</remarks>
    public static Segment MaxPlusConvolution(Point point, Segment segment)
        => Segment.MaxPlusConvolution(segment: segment, point: point);

    /// <summary>
    /// Computes the max-plus convolution between the <see cref="Point"/> and a <see cref="Segment"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 3</remarks>
    public Segment MaxPlusConvolution(Segment segment)
        => Segment.MaxPlusConvolution(segment: segment, point: this);

    #endregion Max-Plus Convolution operator

    #region Sub-additive closure

    /// <summary>
    /// Computes the sub-additive closure of the point.
    /// </summary>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <param name="settings"></param>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 8.</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        if (Time == Rational.Zero)
        {
            if (Value < 0)
            {
                throw new NotImplementedException("Sub-additive closure of negative point is not implemented.");
            }

            return new SubAdditiveCurve(new DelayServiceCurve(0));
        }
        else
        {
            Sequence baseSequence = new Sequence(
                elements: new[] { Origin() },
                fillFrom: 0,
                fillTo: Time);

            return new SubAdditiveCurve(
                baseSequence: baseSequence,
                pseudoPeriodStart: 0,
                pseudoPeriodLength: Time,
                pseudoPeriodHeight: Value
            );
        }
    }

    /// <summary>
    /// Computes the sub-additive closure of the pseudo-periodic point.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <param name="settings"></param>
    /// <returns>The result of the sub-additive closure</returns>
    /// <exception cref="ArgumentException">Thrown if the period is not greater than 0</exception>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 9</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        ComputationSettings? settings = null
    )
    {
        if (pseudoPeriodLength <= 0)
            throw new ArgumentException("Period must be > 0");

        if (Time == 0)
        {
            return PeriodicPointClosureType0(pseudoPeriodLength, pseudoPeriodHeight);
        }
        else
        {
            var iterationSlope = pseudoPeriodHeight / pseudoPeriodLength;

            SubAdditiveCurve closure;
            if (PointSlope > iterationSlope)
                closure = PeriodicPointClosureTypeA(pseudoPeriodLength, pseudoPeriodHeight, settings);
            else if (PointSlope < iterationSlope)
                closure = PeriodicPointClosureTypeB(pseudoPeriodLength, pseudoPeriodHeight, settings);
            else // pointSlope == iterationSlope
                closure = PeriodicPointClosureTypeC(pseudoPeriodLength, pseudoPeriodHeight);

            settings ??= ComputationSettings.Default();
            if (settings.UseRepresentationMinimization)
                return new SubAdditiveCurve(closure.Optimize(), false);
            else
                return new SubAdditiveCurve(closure, false);
        }
    }

    /// <summary>
    /// Branch of <see cref="SubAdditiveClosure(Rational, Rational, ComputationSettings?)"/> with point in 0.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    private SubAdditiveCurve PeriodicPointClosureType0(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight)
    {
        var sequence = new Sequence(
            elements: new[] {
                Point.Origin(),
                new Point(time: pseudoPeriodLength, value: Value + pseudoPeriodHeight)
            },
            fillFrom: 0,
            fillTo: 2 * pseudoPeriodLength
        );

        return new SubAdditiveCurve(
            baseSequence: sequence,
            pseudoPeriodStart: pseudoPeriodLength,
            pseudoPeriodLength: pseudoPeriodLength,
            pseudoPeriodHeight: pseudoPeriodHeight
        );
    }

    /// <summary>
    /// Branch of <see cref="SubAdditiveClosure(Rational, Rational, ComputationSettings?)"/> with point slope greater than slope of iteration.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <param name="settings"></param>
    /// <remarks>From [BT07], first case of algorithm 9</remarks>
    private SubAdditiveCurve PeriodicPointClosureTypeA(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        ComputationSettings? settings = null    
    )
    {
        //beta is off by one wrt [BT07] for simpler implementation
        //note that in [BT07], in this section, /\ means gcd
        Rational beta = pseudoPeriodLength / Rational.GreatestCommonDivisor(pseudoPeriodLength, Time);

        #if DO_LOG
        logger.Trace($"Periodic point closure type A. beta: {beta}");
        #endif

        List<Curve> iteratedPoints = new List<Curve>();

        for (int k = 1; k <= beta; k++)
        {
            Point pointK = new Point(time: k * Time, value: k * Value);

            Curve iteratedPointK = new Curve(
                baseSequence: new Sequence(
                    new[] { Origin(), pointK },  //Addition of origin handles f^0
                    fillFrom: 0,
                    fillTo: pointK.Time + pseudoPeriodLength),
                pseudoPeriodStart: pointK.Time,
                pseudoPeriodLength: pseudoPeriodLength,
                pseudoPeriodHeight: pseudoPeriodHeight
            );

            iteratedPoints.Add(iteratedPointK);
        }

        return new SubAdditiveCurve(
            Curve.Minimum(iteratedPoints, settings),
            false
        );
    }

    /// <summary>
    /// Branch of <see cref="SubAdditiveClosure(Rational, Rational, ComputationSettings?)"/> with point slope less than slope of iteration.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <param name="settings"></param>
    /// <remarks>From [BT07], second case of algorithm 9</remarks>
    private SubAdditiveCurve PeriodicPointClosureTypeB(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        ComputationSettings? settings = null    
    )
    {
        //alpha is off by one wrt [BT07] for simpler implementation
        //note that in [BT07], in this section, /\ means gcd
        Rational alpha = (Time / Rational.GreatestCommonDivisor(pseudoPeriodLength, Time)) - 1;

        #if DO_LOG
        logger.Trace($"Periodic point closure type B. alpha: {alpha}");
        #endif

        List<Curve> curves = new List<Curve>();

        for (int i = 0; i <= alpha; i++)
        {
            Point pointK = new Point(time: Time + i * pseudoPeriodLength, value: Value + i * pseudoPeriodHeight);

            Curve iteratedPointK = new Curve(
                baseSequence: new Sequence(
                    new[] { Origin(), pointK },  //Addition of origin handles f^0
                    fillFrom: 0,
                    fillTo: pointK.Time + Time),
                pseudoPeriodStart: pointK.Time,
                pseudoPeriodLength: Time,
                pseudoPeriodHeight: Value
            );

            curves.Add(iteratedPointK);
        }

        return new SubAdditiveCurve( 
            Curve.Minimum(curves, settings),
            false
        );
    }

    /// <summary>
    /// Branch of <see cref="SubAdditiveClosure(Rational, Rational, ComputationSettings?)"/> with point slope equal to slope of iteration.
    /// </summary>
    /// <param name="pseudoPeriodLength">Lenght of the pseudo-period</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period</param>
    /// <remarks>From [BT07], third case of algorithm 9</remarks>
    private SubAdditiveCurve PeriodicPointClosureTypeC(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight    
    )
    {
        Rational slope = Value / Time;

        Rational closurePeriodStart = Frobenius(Time, pseudoPeriodLength) + Time; //Corrected with offset of Time
        Rational closurePeriodLenght = Rational.GreatestCommonDivisor(Time, pseudoPeriodLength);
        Rational closurePeriodHeight = slope * closurePeriodLenght;

        Rational rightClosureEndpoint = closurePeriodStart + closurePeriodLenght;
        int maxK = (int)Math.Floor((decimal)(rightClosureEndpoint / Time)) - 1;
        int maxI = (int)Math.Floor((decimal)((rightClosureEndpoint - Time) / pseudoPeriodLength));

        #if DO_LOG
        logger.Trace($"Periodic point closure type C. maxK: {maxK} ; maxI: {maxI} ; complexity: {maxK * maxI}");
        #endif

        List<Point> closurePoints = new List<Point> { Origin() };
        var ikPairs = Enumerable.Range(0, maxI + 1)
            .SelectMany(i => Enumerable.Range(0, maxK + 1)
                .Select(k => (i, k)))
            .ToList();

        var pastOriginClosurePoints = ikPairs
            .Select(pair => Time + pair.k * Time + pair.i * pseudoPeriodLength)
            .Where(time => time < rightClosureEndpoint)
            .Select(time => new Point(
                time: time,
                value: slope * time
            ))
            .OrderBy(point => point.Time);

        closurePoints.AddRange(pastOriginClosurePoints);

        return new SubAdditiveCurve(
            baseSequence: new Sequence(
                elements: closurePoints,
                fillFrom: 0,
                fillTo: rightClosureEndpoint
            ),
            pseudoPeriodStart: closurePeriodStart,
            pseudoPeriodLength: closurePeriodLenght,
            pseudoPeriodHeight: closurePeriodHeight
        );

        Rational Frobenius(Rational a, Rational b)
        {
            return Rational.LeastCommonMultiple(a, b) - a - b + Rational.GreatestCommonDivisor(a, b);
        }
    }

    #endregion Sub-additive closure
}