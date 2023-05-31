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
/// An open segment. Defined in ]<see cref="StartTime"/>, <see cref="EndTime"/>[. 
/// </summary>
/// <remarks> 
/// From unit structure defined in [BT07] Section 4.1 
/// </remarks>
/// <docs position="4"/>
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public sealed class Segment : Element, IEquatable<Segment>
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    #region Properties

    /// <summary>
    /// Exclusive left endpoint of the support of the segment.
    /// </summary>
    /// <remarks> 
    /// Referred to as $x_i$ in [BT07] Section 4.1 
    /// </remarks>
    [JsonProperty(PropertyName = "startTime")]
    public override Rational StartTime { get; }

    /// <summary>
    /// Exclusive rigth endpoint of the support of the segment.
    /// </summary>
    /// <remarks> 
    /// Referred to as $x_i$ in [BT07] Section 4.1 
    /// </remarks>
    [JsonProperty(PropertyName = "endTime")]
    public override Rational EndTime { get; }

    /// <summary>
    /// Right limit of the segment at <see cref="StartTime"/>, $f(a+)$.
    /// </summary>
    /// <remarks>
    /// Referred to as $\rho_i$ in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "rightLimit")]
    public Rational RightLimitAtStartTime { get; }

    /// <summary>
    /// Slope of the segment.
    /// </summary>
    /// <remarks>
    /// Referred to as $\rho_i$ in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "slope")]
    public Rational Slope { get; }

    /// <summary>
    /// Left limit of the segment at <see cref="EndTime"/>, $f(b-)$.
    /// </summary>
    public Rational LeftLimitAtEndTime => RightLimitAtStartTime + Slope * Length;

    /// <summary>
    /// True if the segment has value plus/minus infinite.
    /// </summary>
    public override bool IsInfinite => RightLimitAtStartTime.IsInfinite;

    /// <summary>
    /// True if the segment has value plus infinite.
    /// </summary>
    public override bool IsPlusInfinite => RightLimitAtStartTime.IsPlusInfinite;

    /// <summary>
    /// True if the segment has value minus infinite.
    /// </summary>
    public override bool IsMinusInfinite => RightLimitAtStartTime.IsMinusInfinite;

    /// <summary>
    /// True if the segment has value 0 over all of its support.
    /// </summary>
    public override bool IsZero => 
        RightLimitAtStartTime.IsZero && Slope.IsZero;

    /// <summary>
    /// True if the segment has a constant value
    /// </summary>
    public bool IsConstant => Slope == 0;

    /// <summary>
    /// Slope, w.r.t. origin, of the left endpoint of the segment.
    /// </summary>
    public Rational StartSlope => StartTime > 0 ?
        RightLimitAtStartTime / StartTime :
        Rational.PlusInfinity;

    /// <summary>
    /// Slope, w.r.t. origin, of the right endpoint of the segment.
    /// </summary>
    public Rational EndSlope => LeftLimitAtEndTime / EndTime;

    #endregion Properties

    #region Serialization

    /// <summary>
    /// Type identification constant for JSON (de)serialization.
    /// </summary>
    /// <exclude />
    public const string TypeCode = "segment";

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
    /// <param name="startTime">Left endpoint of the support of the segment.</param>
    /// <param name="endTime">Right endpoint of the support of the segment.</param>
    /// <param name="rightLimitAtStartTime">Right limit of the segment at startTime, f(a+).</param>
    /// <param name="slope">Slope of the segment.</param>
    /// <exception cref="ArgumentException"></exception>
    public Segment(
        Rational startTime,
        Rational endTime,
        Rational rightLimitAtStartTime,
        Rational slope
    )
    {
        if (!(startTime < endTime))
            throw new ArgumentException("Segment end time must be greater than start time");

        if( rightLimitAtStartTime.IsFinite && slope.IsFinite )
        {
            StartTime = startTime;
            EndTime = endTime;
            RightLimitAtStartTime = rightLimitAtStartTime;
            Slope = slope;
        }
        else
        {
            if (rightLimitAtStartTime.IsInfinite && slope.IsInfinite && rightLimitAtStartTime != slope)
                throw new ArgumentException("Cannot have opposite infinites as rightLimit and slope");

            if (rightLimitAtStartTime.IsPlusInfinite || slope.IsPlusInfinite)
            {
                StartTime = startTime;
                EndTime = endTime;
                RightLimitAtStartTime = Rational.PlusInfinity;
                Slope = 0;
            }
            else
            {
                StartTime = startTime;
                EndTime = endTime;
                RightLimitAtStartTime = Rational.MinusInfinity;
                Slope = 0;
            }
        }
    }

    /// <summary>
    /// Constructs a segment with a constant value.
    /// </summary>
    /// <param name="startTime">Left endpoint of the support of the segment.</param>
    /// <param name="endTime">Right endpoint of the support of the segment.</param>
    /// <param name="value">Value of the segment.</param>
    public static Segment Constant(Rational startTime,
        Rational endTime, Rational value)
    {
        return new Segment(
            startTime: startTime,
            endTime: endTime,
            rightLimitAtStartTime: value,
            slope: 0);
    }

    /// <summary>
    /// Constructs a segment with constant value 0.
    /// </summary>
    /// <param name="startTime">Left endpoint of the support of the segment.</param>
    /// <param name="endTime">Right endpoint of the support of the segment.</param>
    public static Segment Zero(Rational startTime, Rational endTime)
    {
        return Constant(startTime: startTime,
            endTime: endTime, value: 0);
    }

    /// <summary>
    /// Constructs a segment with constant value $+\infty$.
    /// </summary>
    /// <param name="startTime">Left endpoint of the support of the segment.</param>
    /// <param name="endTime">Right endpoint of the support of the segment.</param>
    public static Segment PlusInfinite(Rational startTime, Rational endTime)
    {
        return new Segment(
            startTime: startTime,
            endTime: endTime,
            rightLimitAtStartTime: Rational.PlusInfinity,
            slope: 0
        );
    }

    /// <summary>
    /// Constructs a segment with constant value $-\infty$.
    /// </summary>
    /// <param name="startTime">Left endpoint of the support of the segment.</param>
    /// <param name="endTime">Right endpoint of the support of the segment.</param>
    public static Segment MinusInfinite(Rational startTime, Rational endTime)
    {
        return new Segment(
            startTime: startTime,
            endTime: endTime,
            rightLimitAtStartTime: Rational.MinusInfinity,
            slope: 0
        );
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override Rational ValueAt(Rational time)
    {
        return IsDefinedFor(time)
            ? RightLimitAtStartTime + (time - StartTime) * Slope
            : Rational.PlusInfinity;
    }

    /// <summary>
    /// Computes the right limit of the segment at given <paramref name="time"/>
    /// </summary>
    /// <param name="time">The target time of the limit</param>
    /// <returns>The value of $f(t^+)$</returns>
    public Rational RightLimitAt(Rational time)
    {
        return time == StartTime ? RightLimitAtStartTime : ValueAt(time);
    }

    /// <summary>
    /// Computes the left limit of the segment at given <paramref name="time"/>
    /// </summary>
    /// <param name="time">The target time of the limit</param>
    /// <returns>The value of $f(t^-)$</returns>
    public Rational LeftLimitAt(Rational time)
    {
        return time == EndTime ? LeftLimitAtEndTime : ValueAt(time);
    }

    /// <inheritdoc /> 
    public override bool IsDefinedFor(Rational time)
    {
        return time > StartTime && time < EndTime;
    }

    /// <summary>
    /// Splits the segment in a segment-point-segment set.
    /// </summary>
    /// <param name="splitTime">The time of split.</param>
    /// <returns>A tuple containing the segment-point-segment set resulting from the split.</returns>
    /// <exception cref="ArgumentException">Thrown if the segment is not defined for the time of split.</exception>
    public (Segment leftSegment, Point point, Segment rightSegment) Split(Rational splitTime)
    {
        if (!IsDefinedFor(splitTime))
            throw new ArgumentException("The segment must be defined for the time of split");

        Rational valueAtSplitTime = ValueAt(splitTime);

        return (
            leftSegment: new Segment(
                startTime: StartTime,
                endTime: splitTime,
                rightLimitAtStartTime: RightLimitAtStartTime,
                slope: Slope
            ),
            point: new Point(
                time: splitTime,
                value: valueAtSplitTime
            ),
            rightSegment: new Segment(
                startTime: splitTime,
                endTime: EndTime,
                rightLimitAtStartTime: valueAtSplitTime,
                slope: Slope
            )
        );
    }

    /// <summary>
    /// Returns a <see cref="Point"/> sampling of the segment.
    /// </summary>
    /// <param name="time">Time of sampling.</param>
    /// <exception cref="ArgumentException">Thrown if the segment is not defined for the time of sampling.</exception>
    /// <returns>The sampled <see cref="Point"/>.</returns>
    public Point Sample(Rational time)
    {
        if (!IsDefinedFor(time))
            throw new ArgumentException("Cannot sample at endpoints");

        return new Point(
            time: time,
            value: ValueAt(time));
    }

    /// <summary>
    /// Returns a cut of the segment for a smaller ]start, end[ support. 
    /// </summary>
    /// <param name="cutStart">Left endpoint of the new support.</param>
    /// <param name="cutEnd">Right endpoint of the new support.</param>
    /// <exception cref="ArgumentException">
    /// Thrown if the new support is not a subset of the current one.
    /// </exception>
    /// <returns>
    /// The <see cref="Segment"/> resulting from the cut.
    /// </returns>
    public Segment Cut(Rational cutStart, Rational cutEnd)
    {
        if (cutStart == StartTime && cutEnd == EndTime)
            return this;

        if (cutStart < StartTime || cutEnd > EndTime)
            throw new ArgumentException("Cut bounds are over segment support.");

        if (cutStart == cutEnd)
            throw new ArgumentException("Cannot cut an open segment with coinciding endpoints.");

        return new Segment(
            startTime: cutStart,
            endTime: cutEnd,
            rightLimitAtStartTime: (cutStart == StartTime) ? RightLimitAtStartTime : ValueAt(cutStart),
            slope: Slope
        );
    }

    /// <summary>
    /// Computes the overlap between two segments.
    /// </summary>
    /// <returns>The endpoints of the overlap interval, or null if there is none.</returns>
    public static (Rational start, Rational end)? GetOverlap(Segment a, Segment b)
    {
        Rational start = Rational.Max(a.StartTime, b.StartTime);
        Rational end = Rational.Min(a.EndTime, b.EndTime);

        if (start > end || start == end)
        {
            return null;
        }
        else
        {
            return (start, end);
        }
    }

    /// <summary>
    /// Computes the overlap between two elements.
    /// </summary>
    /// <returns>The endpoints of the overlap interval, or null if there is none.</returns>
    public (Rational start, Rational end)? GetOverlap(Segment secondOperand)
        => GetOverlap(this, secondOperand);

    /// <summary>
    /// Computes the <see cref="Point"/> of intersection between two <see cref="Segment"/>s, if it exists.
    /// </summary>
    /// <returns>The point of intersection, or null if it does not exist.</returns>
    /// <remarks>The point is computed only if unique. If two segments coincide, even if in part, this method will return null.</remarks>
    public static Point? GetIntersection(Segment a, Segment b)
    {
        var overlap = GetOverlap(a, b);

        if (overlap == null)
            return null;

        var (start, end) = overlap ?? default;

        Segment aCut = a.Cut(start, end);
        Segment bCut = b.Cut(start, end);

        var intersectionTime = GetIntersectionTime(aCut, bCut);

        if (intersectionTime == null)
            return null;
        else
            return a.Sample((Rational)intersectionTime);
    }

    /// <summary>
    /// Computes the point in time where two segments intersect, if they do.
    /// The segments must be already cut to the same interval, and an intersection is considered as such only if it happens within said interval.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segments' supports do not coincide.</exception>
    /// <returns>The intersection time, or null if the two segments do not intersect within their support.</returns>
    private static Rational? GetIntersectionTime(Segment a, Segment b)
    {
        if (a.StartTime != b.StartTime || a.EndTime != b.EndTime)
            throw new ArgumentException("The two segments have not identical support.");

        if (a.IsInfinite || b.IsInfinite)
            return null;

        //Parallel and coincident segments are treated the same
        if (a.Slope == b.Slope)
            return null;

        //Obtained by analytically solving by t f1(t) = f2(t)
        //Approximate meaning: the target time is equal to the time that f = (rho_1 - rho_2)*t takes to cover the "vertical gap" between the two segments
        Rational t =
            ((a.Slope * a.StartTime - a.RightLimitAtStartTime) -
             (b.Slope * b.StartTime - b.RightLimitAtStartTime))
            / (a.Slope - b.Slope);

        //Intersections are invalid outside overlap interval
        if (a.StartTime < t && t < b.EndTime)
            return t;
        else
            return null;
    }

    /// <summary>
    /// Computes the first time the segment is at or above given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to reach.</param>
    /// <returns>The first time t at which f(t) = value, or + infinity if it is never reached.</returns>
    public Rational TimeAt(Rational value)
    {
        if (RightLimitAtStartTime >= value)
            return StartTime;
        else
        {
            if (LeftLimitAtEndTime == value)
                return EndTime;
            if (LeftLimitAtEndTime > value)
                return StartTime + (value - RightLimitAtStartTime) / Slope;
            else
                return Rational.PlusInfinity;
        }
    }

    /// <summary>
    /// Deserializes a <see cref="Segment"/>.
    /// </summary>
    public new static Segment FromJson(string json)
    {
        var segment = JsonConvert.DeserializeObject<Segment>(json, new RationalConverter());
        if (segment == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return segment;
    }

    /// <summary>
    /// Returns a string containing C# code to create this Segment.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public override string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var sb = new StringBuilder();
        sb.Append("new Segment(");
        sb.Append($"{StartTime.ToCodeString()},");
        sb.Append($"{EndTime.ToCodeString()},");
        sb.Append($"{RightLimitAtStartTime.ToCodeString()},");
        sb.Append($"{Slope.ToCodeString()}");
        sb.Append(")");

        return sb.ToString();
    }

    #endregion Methods

    #region Basic manipulations

    /// <summary>
    /// Scales the segment by a multiplicative factor.
    /// </summary>
    public override Element Scale(Rational scaling)
    {
        return new Segment(
            startTime: StartTime,
            endTime: EndTime,
            rightLimitAtStartTime: RightLimitAtStartTime * scaling,
            slope: Slope * scaling
        );
    }

    /// <inheritdoc />
    public override Element Delay(Rational delay)
    {
        return new Segment(
            startTime: StartTime + delay,
            endTime: EndTime + delay,
            rightLimitAtStartTime: RightLimitAtStartTime,
            slope: Slope
        );
    }

    /// <inheritdoc />
    public override Element Anticipate(Rational time)
    {
        return new Segment(
            startTime: StartTime - time,
            endTime: EndTime - time,
            rightLimitAtStartTime: RightLimitAtStartTime,
            slope: Slope
        );
    }

    /// <summary>
    /// Shifts the segment vertically by an additive factor.
    /// </summary>
    public override Element VerticalShift(Rational shift)
    {
        return new Segment(
            startTime: StartTime,
            endTime: EndTime,
            rightLimitAtStartTime: RightLimitAtStartTime + shift,
            slope: Slope
        );
    }

    /// <inheritdoc />
    public override Element Negate()
    {
        if (IsZero)
            return this;

        return new Segment(
            startTime: StartTime,
            endTime: EndTime,
            rightLimitAtStartTime: -RightLimitAtStartTime,
            slope: -Slope
        );
    }

    /// <inheritdoc />
    public override Element Inverse()
    {
        if (IsConstant)
            throw new InvalidOperationException("The inverse of a constant segment is not defined.");
        else
            return new Segment(
                startTime: RightLimitAtStartTime,
                endTime: LeftLimitAtEndTime,
                rightLimitAtStartTime: StartTime,
                slope: Rational.Invert(Slope)
            );
    }

    #endregion Basic manipulations

    #region Equality methods

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => (obj is Segment segment) && Equals(segment);

    /// <inheritdoc />
    public override int GetHashCode()
        => (StartTime, EndTime, Slope, RightLimitAtStartTime).GetHashCode();

    /// <inheritdoc />
    public bool Equals(Segment? s)
        => s is not null && (StartTime, EndTime, Slope, RightLimitAtStartTime) == (s.StartTime, s.EndTime, s.Slope, s.RightLimitAtStartTime);

    /// <summary>
    /// Returns <code>true</code> if its operands are equal, <code>false</code> otherwise
    /// </summary>
    public static bool operator ==(Segment? a, Segment? b) =>
        Equals(a, b);

    /// <summary>
    /// Returns <code>false</code> if its operands are equal, <code>true</code> otherwise
    /// </summary>
    public static bool operator !=(Segment? a, Segment? b) =>
        !Equals(a, b);

    #endregion Equality methods

    #region Basic comparisons

    /// <summary>
    /// Returns true if the first segment has always higher value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool IsCertainlyAbove(Segment a, Segment b)
    {
        return a.RightLimitAtStartTime > b.RightLimitAtStartTime &&
               a.LeftLimitAtEndTime > b.LeftLimitAtEndTime;
    }

    /// <summary>
    /// Returns true if the first segment has always higher value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public bool IsCertainlyAbove(Segment segment)
        => IsCertainlyAbove(a: this, b: segment);

    /// <summary>
    /// Returns true if the first segment has always higher value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool operator >(Segment a, Segment b)
        => IsCertainlyAbove(a, b);

    /// <summary>
    /// Returns true if the first segment has always lower value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool IsCertainlyBelow(Segment a, Segment b)
    {
        return a.RightLimitAtStartTime < b.RightLimitAtStartTime &&
               a.LeftLimitAtEndTime < b.LeftLimitAtEndTime;
    }

    /// <summary>
    /// Returns true if the first segment has always lower value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public bool IsCertainlyBelow(Segment segment)
        => IsCertainlyBelow(a: this, b: segment);

    /// <summary>
    /// Returns true if the first segment has always lower value than the second one.
    /// Does not consider time overlapping.
    /// </summary>
    public static bool operator <(Segment a, Segment b)
        => IsCertainlyBelow(a, b);

    #endregion Basic comparisons

    #region Addition and Subtraction operators

    /// <summary>
    /// Sums the <see cref="Segment"/> with an overlapping <see cref="Element"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segment and the element do not overlap.</exception>
    /// <returns>The element resulting from the sum.</returns>
    public override Element Addition(Element element)
    {
        switch (element)
        {
            case Point p:
                return Point.Addition(point: p, segment: this);
            case Segment s:
                return Addition(a: this, b: s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Sums a <see cref="Segment"/> with an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point Addition(Segment segment, Point point)
        =>  Point.Addition(point, segment);

    /// <summary>
    /// Sums the <see cref="Segment"/> with an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public Point Addition(Point point)
        => Point.Addition(point, segment: this);

    /// <summary>
    /// Sums a <see cref="Segment"/> to an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the sum.</returns>
    public static Point operator +(Segment segment, Point point)
        => Point.Addition(point, segment);

    /// <summary>
    /// Sums two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the sum.</returns>
    public static Segment Addition(Segment a, Segment b)
    {
        var overlap = a.GetOverlap(b);
        Rational overlapStart, overlapEnd;
        (overlapStart, overlapEnd) = overlap ?? throw new ArgumentException("Non-overlapping segments");

        Segment firstCut = a.Cut(overlapStart, overlapEnd);
        Segment secondCut = b.Cut(overlapStart, overlapEnd);

        Segment sumResult = new Segment
        (
            startTime: overlapStart,
            endTime: overlapEnd,
            rightLimitAtStartTime: firstCut.RightLimitAtStartTime + secondCut.RightLimitAtStartTime,
            slope: firstCut.Slope + secondCut.Slope
        );

        return sumResult;
    }

    /// <summary>
    /// Sums two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the sum.</returns>
    public Segment Addition(Segment segment)
        => Addition(a: this, b: segment);

    /// <summary>
    /// Sums two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the sum.</returns>
    public static Segment operator +(Segment a, Segment b)
        => Addition(a, b);

    /// <summary>
    /// Subtracts the <see cref="Segment"/> with an overlapping <see cref="Element"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segment and the element do not overlap.</exception>
    /// <returns>The element resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public override Element Subtraction(Element element)
    {
        switch (element)
        {
            case Point p:
                return Point.Subtraction(point: p, segment: this);
            case Segment s:
                return Subtraction(a: this, b: s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Subtracts a <see cref="Segment"/> with an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point Subtraction(Segment segment, Point point)
        =>  Point.Subtraction(point, segment);

    /// <summary>
    /// Subtracts the <see cref="Segment"/> with an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public Point Subtraction(Point point)
        => Point.Subtraction(point, segment: this);

    /// <summary>
    /// Subtracts a <see cref="Segment"/> to an overlapping <see cref="Point"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Point operator -(Segment segment, Point point)
        => Point.Subtraction(point, segment);

    /// <summary>
    /// Subtracts two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Segment Subtraction(Segment a, Segment b)
    {
        var overlap = a.GetOverlap(b);
        Rational overlapStart, overlapEnd;
        (overlapStart, overlapEnd) = overlap ?? throw new ArgumentException("Non-overlapping segments");

        Segment firstCut = a.Cut(overlapStart, overlapEnd);
        Segment secondCut = b.Cut(overlapStart, overlapEnd);

        Segment subResult = new Segment
        (
            startTime: overlapStart,
            endTime: overlapEnd,
            rightLimitAtStartTime: firstCut.RightLimitAtStartTime - secondCut.RightLimitAtStartTime,
            slope: firstCut.Slope - secondCut.Slope
        );

        return subResult;
    }

    /// <summary>
    /// Subtracts two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public Segment Subtraction(Segment segment)
        => Subtraction(a: this, b: segment);

    /// <summary>
    /// Subtracts two <see cref="Segment"/>s over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if there is no overlap between the segments.</exception>
    /// <returns>A segment representing the subtraction.</returns>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Segment operator -(Segment a, Segment b)
        => Subtraction(a, b);

    #endregion Addition and Subtraction operators

    #region Minimum operator

    /// <summary>
    /// Computes the minimum of the <see cref="Segment"/> and the given <see cref="Element"/> over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the minimum.</returns>
    public override List<Element> Minimum(Element element)
    {
        switch (element)
        {
            case Point p:
                return new List<Element> { Point.Minimum(point: p, segment: this) };
            case Segment s:
                return Minimum(a: this, b: s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the minimum of a <see cref="Segment"/> and a <see cref="Point"/> over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the minimum.</returns>
    public static Point Minimum(Segment segment, Point point)
        => Point.Minimum(point, segment);

    /// <summary>
    /// Computes the minimum of the <see cref="Segment"/> and a <see cref="Point"/> over their overlapping part.
    /// </summary>
    /// <param name="point">Second operand</param>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap</exception>
    /// <returns>The point resulting from the minimum</returns>
    public Point Minimum(Point point)
        => Point.Minimum(point, segment: this);

    /// <summary>
    /// Computes the minimum of two <see cref="Segment"/>s over their overlapping part.
    /// The result is either a segment or, if the segments intersect, a segment-point-segment sequence.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap.</exception>
    /// <returns>The set of <see cref="Element"/>s resulting from the minimum.</returns>
    public static List<Element> Minimum(Segment a, Segment b)
    {
        var overlap = GetOverlap(a, b);
        if (overlap == null)
            throw new ArgumentException("The two segments do not overlap.");

        var (start, end) = overlap ?? default;

        Segment aCut = a.Cut(start, end);
        Segment bCut = b.Cut(start, end);

        Rational? intersectionTime = GetIntersectionTime(aCut, bCut);

        Rational rightLimitAtMinStart = Rational.Min(aCut.RightLimitAtStartTime, bCut.RightLimitAtStartTime);

        if (intersectionTime == null)
        {
            //Single min segment, no intersection within bounds

            Rational minSlope;
            if (aCut.RightLimitAtStartTime != bCut.RightLimitAtStartTime)
            {
                //Clear distinction: the min segment at ts+ stays the min
                minSlope = bCut.RightLimitAtStartTime < aCut.RightLimitAtStartTime ? bCut.Slope : aCut.Slope;
            }
            else
            {
                //Limits at ts+ coincide, must order by slope
                minSlope = Rational.Min(bCut.Slope, aCut.Slope);
            }

            Segment onlyMinSegment = new Segment
            (
                startTime: start,
                endTime: end,
                rightLimitAtStartTime: rightLimitAtMinStart,
                slope: minSlope
            );
            return new List<Element> { onlyMinSegment };
        }
        else
        {
            //Forward intersection: at intersectionTime, the two segments change positions
            //This must mean that we have the max slope first and the min one then
            Rational intersectionValue = bCut.ValueAt((Rational)intersectionTime);

            Segment firstMinSegment = new Segment
            (
                startTime: start,
                endTime: (Rational)intersectionTime,
                rightLimitAtStartTime: rightLimitAtMinStart,
                slope: Rational.Max(aCut.Slope, bCut.Slope)
            );

            Point intersectionPoint = new Point(
                time: (Rational)intersectionTime,
                value: intersectionValue
            );

            Segment secondMinSegment = new Segment
            (
                startTime: (Rational)intersectionTime,
                endTime: end,
                rightLimitAtStartTime: intersectionValue,
                slope: Rational.Min(aCut.Slope, bCut.Slope)
            );

            return new List<Element> { firstMinSegment, intersectionPoint, secondMinSegment };
        }
    }

    /// <summary>
    /// Computes the minimum of two <see cref="Segment"/>s over their overlapping part.
    /// The result is either a segment or, if the segments intersect, a segment-point-segment sequence.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap</exception>
    /// <returns>The set of <see cref="Element"/>s resulting from the minimum</returns>
    public List<Element> Minimum(Segment segment)
        => Minimum(a: this, b: segment);

    /// <summary>
    /// Computes the minimum of a set of segments over their overlapping part.
    /// </summary>
    /// <param name="segments">Segments of which the minimum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of segments is empty.</exception>
    /// <returns>The result of the overall minimum.</returns>
    public static List<Element> Minimum(IReadOnlyList<Segment> segments)
    {
        if (!segments.Any())
            throw new InvalidOperationException("The enumerable is empty.");

        if (!SameSize())
        {
            var minLength = segments.Min(s => s.Length);
            var reference = segments.First(s => s.Length == minLength);

            segments = segments
                .Select(s => s.Cut(reference.StartTime, reference.EndTime))
                .ToList();
        }

        segments = GreedyFilter();

        if (segments.Count == 1)
            return segments.ToList<Element>();
        if (segments.Count == 2)
            return Segment.Minimum(segments.ElementAt(0), segments.ElementAt(1));

        return segments.LowerEnvelope();

        bool SameSize()
        {
            var firstSegment = segments.First();
            return segments.All(e => e.Length == firstSegment.Length);
        }

        List<Segment> GreedyFilter()
        {
            var minStartValue = segments
                .Min(s => s.RightLimitAtStartTime);
            var startReference = segments
                .First(s => s.RightLimitAtStartTime == minStartValue);

            var minEndValue = segments
                .Min(s => s.LeftLimitAtEndTime);
            var endReference = segments
                .First(s => s.LeftLimitAtEndTime == minEndValue);

            return segments
                .Where(segment => 
                    !segment.IsCertainlyAbove(startReference) &&
                    !segment.IsCertainlyAbove(endReference))
                .ToList();
        }
    }

    #endregion Minimum operator

    #region Maximum operator

    /// <summary>
    /// Computes the maximum of the <see cref="Segment"/> and the given <see cref="Element"/> over their overlapping part.
    /// The result is either a point, a segment or a segment-point-segment sequence.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the maximum.</returns>
    public override List<Element> Maximum(Element element)
    {
        switch (element)
        {
            case Point p:
                return new List<Element> { Point.Maximum(point:p, segment: this) };
            case Segment s:
                return Maximum(a: this, b: s);

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the maximum of a <see cref="Segment"/> and a <see cref="Point"/> over their overlapping part.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap.</exception>
    /// <returns>The point resulting from the maximum.</returns>
    public static Point Maximum(Segment segment, Point point)
        => Point.Maximum(point, segment);

    /// <summary>
    /// Computes the maximum of the <see cref="Segment"/> and a <see cref="Point"/> over their overlapping part.
    /// </summary>
    /// <param name="point">Second operand</param>
    /// <exception cref="ArgumentException">Thrown if point and segment do not overlap</exception>
    /// <returns>The point resulting from the maximum</returns>
    public Point Maximum(Point point)
        => Point.Maximum(point, segment: this);

    /// <summary>
    /// Computes the maximum of two <see cref="Segment"/>s over their overlapping part.
    /// The result is either a segment or, if the segments intersect, a segment-point-segment sequence.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap.</exception>
    /// <returns>The set of <see cref="Element"/>s resulting from the maximum.</returns>
    public static List<Element> Maximum(Segment a, Segment b)
    {
        var overlap = GetOverlap(a, b);
        if (overlap == null)
        {
            throw new ArgumentException("The two segments do not overlap.");
        }

        var (start, end) = overlap ?? default;

        Segment aCut = a.Cut(start, end);
        Segment bCut = b.Cut(start, end);

        Rational? intersectionTime = GetIntersectionTime(aCut, bCut);

        Rational rightLimitAtMaxStart = Rational.Max(aCut.RightLimitAtStartTime, bCut.RightLimitAtStartTime);

        if (intersectionTime == null || intersectionTime == bCut.StartTime)
        {
            //Single max segment, no intersection within bounds

            Rational maxSlope;
            if (aCut.RightLimitAtStartTime != bCut.RightLimitAtStartTime)
            {
                //Clear distinction: the max segment at ts+ stays the max
                maxSlope = bCut.RightLimitAtStartTime > aCut.RightLimitAtStartTime ? bCut.Slope : aCut.Slope;
            }
            else
            {
                //Limits at ts+ coincide, must order by slope
                maxSlope = Rational.Max(bCut.Slope, aCut.Slope);
            }

            Segment onlyMaxSegment = new Segment
            (
                startTime: start,
                endTime: end,
                rightLimitAtStartTime: rightLimitAtMaxStart,
                slope: maxSlope
            );
            return new List<Element> { onlyMaxSegment };
        }
        else
        {
            //Forward intersection: at intersectionTime, the two segments change positions
            //This must mean that we have the min slope first and the max one then
            Rational intersectionValue = bCut.ValueAt((Rational)intersectionTime);

            Segment firstMaxSegment = new Segment
            (
                startTime: start,
                endTime: (Rational)intersectionTime,
                rightLimitAtStartTime: rightLimitAtMaxStart,
                slope: Rational.Min(aCut.Slope, bCut.Slope)
            );

            Point intersectionPoint = new Point(
                time: (Rational)intersectionTime,
                value: intersectionValue
            );

            Segment secondMaxSegment = new Segment
            (
                startTime: (Rational)intersectionTime,
                endTime: end,
                rightLimitAtStartTime: intersectionValue,
                slope: Rational.Max(aCut.Slope, bCut.Slope)
            );

            return new List<Element> { firstMaxSegment, intersectionPoint, secondMaxSegment };
        }
    }

    /// <summary>
    /// Computes the maximum of two <see cref="Segment"/>s over their overlapping part.
    /// The result is either a segment or, if the segments intersect, a segment-point-segment sequence.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap</exception>
    /// <returns>The set of <see cref="Element"/>s resulting from the maximum</returns>
    public List<Element> Maximum(Segment segment)
        => Maximum(a: this, segment);

    /// <summary>
    /// Computes the maximum of a set of segments over their overlapping part.
    /// </summary>
    /// <param name="segments">Segments of which the maximum has to be computed.</param>
    /// <exception cref="ArgumentException">Thrown if the segments do not overlap.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the set of segments is empty.</exception>
    /// <returns>The result of the overall maximum.</returns>
    public static List<Element> Maximum(IReadOnlyList<Segment> segments)
    {
        if (!segments.Any())
            throw new InvalidOperationException("The enumerable is empty.");

        if (!SameSize())
        {
            var minLength = segments.Min(s => s.Length);
            var reference = segments.First(s => s.Length == minLength);

            segments = segments
                .Select(s => s.Cut(reference.StartTime, reference.EndTime))
                .ToList();
        }

        segments = GreedyFilter();

        if (segments.Count == 1)
            return segments.ToList<Element>();
        if (segments.Count == 2)
            return Segment.Maximum(segments.ElementAt(0), segments.ElementAt(1));

        return segments.UpperEnvelope();

        bool SameSize()
        {
            var firstSegment = segments.First();
            return segments.All(e => e.Length == firstSegment.Length);
        }

        List<Segment> GreedyFilter()
        {
            var startReference = segments
                .MaxBy(s => s.RightLimitAtStartTime)!;

            var endReference = segments
                .MaxBy(s => s.LeftLimitAtEndTime)!;

            return segments
                .Where(segment => 
                    !segment.IsCertainlyBelow(startReference) &&
                    !segment.IsCertainlyBelow(endReference))
                .ToList();
        }
    }

    #endregion Maximum operator

    #region Convolution operator

    /// <summary>
    /// Computes the convolution between the <see cref="Segment"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the convolution.</returns>
    public override IEnumerable<Element> Convolution(
        Element element, Rational? cutEnd = null, Rational? cutCeiling = null
    )
    {
        cutEnd ??= Rational.PlusInfinity;
        cutCeiling ??= Rational.PlusInfinity;

        switch (element)
        {
            case Point p:
            {
                var cs = p.IsOrigin ? this : Convolution(segment: this, point: p);
                if (cutEnd != Rational.PlusInfinity || cutCeiling != Rational.PlusInfinity)
                {
                    var tCE = cutEnd ?? Rational.PlusInfinity;
                    var tCC = (cutCeiling != Rational.PlusInfinity && cs is { IsFinite: true, Slope.IsPositive: true }) 
                        ? (((Rational) cutCeiling - cs.RightLimitAtStartTime) / cs.Slope) + cs.StartTime
                        : Rational.PlusInfinity;
                    var t = Rational.Min(tCE, tCC);
                    if (t <= cs.StartTime)
                    {
                        // yield nothing;
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

            case Segment s:
            {
                var conv = Convolution(a: this, b: s);
                if (cutEnd != Rational.PlusInfinity || cutCeiling != Rational.PlusInfinity)
                {
                    if (cutEnd != Rational.PlusInfinity && cutEnd < this.EndTime + s.EndTime)
                        conv = conv.Cut(this.StartTime + s.StartTime, (Rational)cutEnd, false, true);
                    if (cutCeiling != Rational.PlusInfinity)
                        conv = conv.CutWithCeiling(cutCeiling, true);
                }
                foreach (var e in conv)
                    yield return e;
                yield break;
            }

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the convolution between a <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 3</remarks>
    public static Segment Convolution(Segment segment, Point point)
    {
        return new Segment(
            startTime: segment.StartTime + point.Time,
            endTime: segment.EndTime + point.Time,
            rightLimitAtStartTime: segment.RightLimitAtStartTime + point.Value,
            slope: segment.Slope
        );
    }

    /// <summary>
    /// Computes the convolution between the <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 3</remarks>
    public Segment Convolution(Point point)
        => Convolution(segment: this, point);

    /// <summary>
    /// Computes the convolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 4</remarks>
    public static IEnumerable<Element> Convolution(Segment a, Segment b)
    {
        Segment minSlopeSegment, maxSlopeSegment;

        if (a.Slope == b.Slope)
        {
            yield return new Segment(
                startTime: a.StartTime + b.StartTime,
                endTime: a.EndTime + b.EndTime,
                slope: a.Slope,
                rightLimitAtStartTime: a.RightLimitAtStartTime + b.RightLimitAtStartTime
            );
            yield break;
        }
        else if (a.Slope < b.Slope)
        {
            minSlopeSegment = a;
            maxSlopeSegment = b;
        }
        else
        {
            minSlopeSegment = b;
            maxSlopeSegment = a;
        }

        Rational initTime = minSlopeSegment.StartTime + maxSlopeSegment.StartTime;
        Rational middleTime = initTime + minSlopeSegment.Length;
        Rational endTime = minSlopeSegment.EndTime + maxSlopeSegment.EndTime;

        Rational initValue = minSlopeSegment.RightLimitAtStartTime + maxSlopeSegment.RightLimitAtStartTime;
        Rational middleValue = minSlopeSegment.LeftLimitAtEndTime + maxSlopeSegment.RightLimitAtStartTime;

        yield return new Segment(
            startTime: initTime,
            endTime: middleTime,
            rightLimitAtStartTime: initValue,
            slope: minSlopeSegment.Slope
        );
        yield return new Point(
            time: middleTime,
            value: middleValue
        );
        yield return new Segment(
            startTime: middleTime,
            endTime: endTime,
            rightLimitAtStartTime: middleValue,
            slope: maxSlopeSegment.Slope
        );
    }

    /// <summary>
    /// Computes the convolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the convolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.1, Lemma 4</remarks>
    public IEnumerable<Element> Convolution(Segment segment)
        => Convolution(a: this, b: segment);

    #endregion Convolution operator

    #region Deconvolution operator

    /// <summary>
    /// Computes the deconvolution between the <see cref="Segment"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the deconvolution.</returns>
    public override IEnumerable<Element> Deconvolution(Element element)
    {
        switch (element)
        {
            case Point p:
            {
                if (p.IsOrigin)
                    yield return this;
                else
                    yield return Deconvolution(segment: this, point: p);
                break;
            }
            case Segment s:
            {
                foreach (var e in Deconvolution(a: this, b: s))
                    yield return e;
                break;
            }

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the deconvolution between a <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 6</remarks>
    public static Segment Deconvolution(Segment segment, Point point)
    {
        return new Segment(
            startTime: segment.StartTime - point.Time,
            endTime: segment.EndTime - point.Time,
            rightLimitAtStartTime: segment.RightLimitAtStartTime - point.Value,
            slope: segment.Slope
        );
    }

    /// <summary>
    /// Computes the deconvolution between the <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 6</remarks>
    public Segment Deconvolution(Point point)
        => Deconvolution(segment: this, point);

    /// <summary>
    /// Computes the deconvolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 8</remarks>
    public static IEnumerable<Element> Deconvolution(Segment a, Segment b)
    {
        if (a.IsInfinite || b.IsInfinite)
            throw new ArgumentException("The arguments must be finite.");

        Segment minSlopeSegment, maxSlopeSegment;
        if (a.Slope < b.Slope)
        {
            minSlopeSegment = a;
            maxSlopeSegment = b;
        }
        else
        {
            minSlopeSegment = b;
            maxSlopeSegment = a;
        }

        Rational startTime = a.StartTime - b.EndTime;
        Rational middleTime = startTime + maxSlopeSegment.Length;
        Rational endTime = a.EndTime - b.StartTime;

        Rational initValue = a.RightLimitAtStartTime - b.LeftLimitAtEndTime;
        Rational middleValue = initValue + maxSlopeSegment.Slope * maxSlopeSegment.Length;

        yield return new Segment(
            startTime: startTime,
            endTime: middleTime,
            rightLimitAtStartTime: initValue,
            slope: maxSlopeSegment.Slope
        );
        yield return new Point(
            time: middleTime,
            value: middleValue
        );
        yield return new Segment(
            startTime: middleTime,
            endTime: endTime,
            rightLimitAtStartTime: middleValue,
            slope: minSlopeSegment.Slope
        );
    }

    /// <summary>
    /// Computes the deconvolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the deconvolution.</returns>
    /// <remarks>Defined in [BT07] Section 3.2.2, Lemma 8</remarks>
    public IEnumerable<Element> Deconvolution(Segment segment)
        => Deconvolution(a: this, b: segment);

    #endregion

    #region Max-plus Convolution operator

    /// <summary>
    /// Computes the max-plus convolution between the <see cref="Segment"/> and the given <see cref="Element"/>.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the max-plus convolution.</returns>
    public override IEnumerable<Element> MaxPlusConvolution(Element element, Rational? cutEnd = null)
    {
        cutEnd ??= Rational.PlusInfinity;

        switch (element)
        {
            case Point p:
            {
                var cs = p.IsOrigin ? this : MaxPlusConvolution(segment: this, point: p);
                if (cutEnd != Rational.PlusInfinity)
                {
                    var tCE = cutEnd ?? Rational.PlusInfinity;
                    if (tCE <= cs.StartTime)
                    {
                        // yield nothing;
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

            case Segment s:
            {
                var conv = MaxPlusConvolution(a: this, b: s);
                if (cutEnd != Rational.PlusInfinity)
                {
                    if (cutEnd != Rational.PlusInfinity && cutEnd < this.EndTime + s.EndTime)
                        conv = conv.Cut(this.StartTime + s.StartTime, (Rational)cutEnd, false, true);
                }
                foreach (var e in conv)
                    yield return e;
                yield break;
            }

            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Computes the max-plus convolution between a <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 3</remarks>
    public static Segment MaxPlusConvolution(Segment segment, Point point)
    {
        return new Segment(
            startTime: segment.StartTime + point.Time,
            endTime: segment.EndTime + point.Time,
            rightLimitAtStartTime: segment.RightLimitAtStartTime + point.Value,
            slope: segment.Slope
        );
    }

    /// <summary>
    /// Computes the max-plus convolution between the <see cref="Segment"/> and a <see cref="Point"/>.
    /// </summary>
    /// <returns>The <see cref="Segment"/> resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 3</remarks>
    public Segment MaxPlusConvolution(Point point)
        => MaxPlusConvolution(segment: this, point);

    /// <summary>
    /// Computes the max-plus convolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 4</remarks>
    public static IEnumerable<Element> MaxPlusConvolution(Segment a, Segment b)
    {
        Segment minSlopeSegment, maxSlopeSegment;

        if (a.Slope == b.Slope)
        {
            yield return new Segment(
                startTime: a.StartTime + b.StartTime,
                endTime: a.EndTime + b.EndTime,
                slope: a.Slope,
                rightLimitAtStartTime: a.RightLimitAtStartTime + b.RightLimitAtStartTime
            );
            yield break;
        }
        else if (a.Slope < b.Slope)
        {
            minSlopeSegment = a;
            maxSlopeSegment = b;
        }
        else
        {
            minSlopeSegment = b;
            maxSlopeSegment = a;
        }

        Rational initTime = minSlopeSegment.StartTime + maxSlopeSegment.StartTime;
        Rational middleTime = initTime + maxSlopeSegment.Length;
        Rational endTime = minSlopeSegment.EndTime + maxSlopeSegment.EndTime;

        Rational initValue = minSlopeSegment.RightLimitAtStartTime + maxSlopeSegment.RightLimitAtStartTime;
        Rational middleValue = maxSlopeSegment.LeftLimitAtEndTime + minSlopeSegment.RightLimitAtStartTime;

        yield return new Segment(
            startTime: initTime,
            endTime: middleTime,
            rightLimitAtStartTime: initValue,
            slope: maxSlopeSegment.Slope
        );
        yield return new Point(
            time: middleTime,
            value: middleValue
        );
        yield return new Segment(
            startTime: middleTime,
            endTime: endTime,
            rightLimitAtStartTime: middleValue,
            slope: minSlopeSegment.Slope
        );
    }

    /// <summary>
    /// Computes the max-plus convolution between two <see cref="Segment"/>s.
    /// </summary>
    /// <returns>The set of <see cref="Element"/>s resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT07] Section 3.2.1, Lemma 4</remarks>
    public IEnumerable<Element> MaxPlusConvolution(Segment segment)
        => MaxPlusConvolution(a: this, b: segment);

    #endregion Max-plus Convolution operator

    #region Sub-additive closure

    /// <summary>
    /// Computes the sub-additive closure of the segment.
    /// </summary>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 10</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //Checked separately as infinite segments fail the normal check
        if(StartTime == 0 && RightLimitAtStartTime > 0)
            return SegmentClosureTypeB();

        SubAdditiveCurve closure;
        if (StartSlope <= EndSlope)
            closure = SegmentClosureTypeA();
        else
            closure = SegmentClosureTypeB();

        if (settings.UseRepresentationMinimization)
            return new SubAdditiveCurve(closure.Optimize(), false);
        else
            return closure;

        //Local functions

        // Branch with slope at start less or equal slope at end.
        // From [BT07], first case of algorithm 10.
        SubAdditiveCurve SegmentClosureTypeA()
        {
            List<Element> elements = new List<Element> { Point.Origin() };

            int k = (int)Math.Floor((decimal)(StartTime / (EndTime - StartTime)));

            for (int i = 1; i <= k; i++)
            {
                elements.Add(new Segment(
                    startTime: i * StartTime,
                    endTime: i * EndTime,
                    slope: Slope,
                    rightLimitAtStartTime: i * RightLimitAtStartTime
                ));
            }

            Rational periodStartTime = (k + 1) * StartTime;
            Rational periodEndTime = (k + 2) * StartTime;
            Rational periodLength = StartTime;
            Rational periodHeight = RightLimitAtStartTime;
            Rational periodStartValue = (k + 1) * RightLimitAtStartTime;

            elements.Add(new Point(
                time: periodStartTime,
                value: periodStartValue
            ));
            elements.Add(new Segment(
                startTime: periodStartTime,
                endTime: periodEndTime,
                slope: Slope,
                rightLimitAtStartTime: periodStartValue
            ));                

            IEnumerable<Element> lowerEnvelope = elements.LowerEnvelope(settings);
            Sequence baseSequence = new Sequence(
                    lowerEnvelope,
                    fillFrom: 0,
                    fillTo: periodEndTime)
                .Cut(cutStart: 0, cutEnd: periodEndTime);

            return new SubAdditiveCurve(
                baseSequence: baseSequence,
                pseudoPeriodStart: periodStartTime,
                pseudoPeriodLength: periodLength,
                pseudoPeriodHeight: periodHeight
            );
        }

        // Branch with slope at start greater than slope at end.
        // From [BT07], second case of algorithm 10.
        SubAdditiveCurve SegmentClosureTypeB()
        {
            List<Element> elements = new List<Element> { Point.Origin() };

            int k = (int)Math.Floor((decimal)(StartTime / (EndTime - StartTime)));

            for (int i = 1; i <= k + 1; i++)
            {
                elements.Add(new Segment(
                    startTime: i * StartTime,
                    endTime: i * EndTime,
                    slope: Slope,
                    rightLimitAtStartTime: i * RightLimitAtStartTime
                ));
            }

            Rational periodStartTime = (k + 1) * EndTime;
            Rational periodEndTime = (k + 2) * EndTime;
            Rational periodLength = EndTime;
            Rational periodHeight = LeftLimitAtEndTime;
            Rational periodStartValue = (k + 2) * LeftLimitAtEndTime - Slope * EndTime;

            elements.Add(new Point(
                time: periodStartTime,
                value: periodStartValue
            ));
            elements.Add(new Segment(
                startTime: periodStartTime,
                endTime: periodEndTime,
                slope: Slope,
                rightLimitAtStartTime: periodStartValue
            ));

            IEnumerable<Element> lowerEnvelope = elements.LowerEnvelope(settings);
            Sequence baseSequence = new Sequence(
                    lowerEnvelope,
                    fillFrom: 0,
                    fillTo: periodEndTime)
                .Cut(cutStart: 0, cutEnd: periodEndTime);

            return new SubAdditiveCurve(
                baseSequence: baseSequence,
                pseudoPeriodStart: periodStartTime,
                pseudoPeriodLength: periodLength,
                pseudoPeriodHeight: periodHeight
            );
        }
    }

    /// <summary>
    /// Computes the sub-additive closure of the pseudo-periodic segment.
    /// </summary>
    /// <param name="pseudoPeriodLength">Length of the pseudo-period.</param>
    /// <param name="pseudoPeriodHeight">Step gained after each pseudo-period.</param>
    /// <param name="settings"></param>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 11</remarks>
    public override SubAdditiveCurve SubAdditiveClosure(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        ComputationSettings? settings = null
    )
    {
        settings ??= ComputationSettings.Default();
        Rational pseudoPeriodSlope = pseudoPeriodHeight / pseudoPeriodLength;

        SubAdditiveCurve closure;
        if (pseudoPeriodSlope <= Slope)
        {
            if (StartSlope < pseudoPeriodSlope)
                closure = PeriodicSegmentClosureTypeA();
            else
                closure = PeriodicSegmentClosureTypeB();
        }
        else //pseudoPeriodSlope > Slope
        {
            if (EndSlope < pseudoPeriodSlope)
                closure = PeriodicSegmentClosureTypeC();
            else
                closure = PeriodicSegmentClosureTypeD();
        }

        if (settings.UseRepresentationMinimization)
            return new SubAdditiveCurve(closure.Optimize(), false);
        else
            return closure;

        //Local functions

        //In [BT07], first leaf branch of algorithm 11.
        SubAdditiveCurve PeriodicSegmentClosureTypeA()
        {
            int k0 = (int)Math.Floor((decimal)(pseudoPeriodLength / Length)) + 1;

            int bigK = 1;
            while (!CheckValidK(bigK))
                bigK++;

            int i0 = (int)Math.Ceiling((decimal)(bigK * StartTime / pseudoPeriodLength));

            #if DO_LOG
            logger.Trace($"Periodic segment closure type A. k0: {k0} ; bigK: {bigK} ; i0: {i0}");
            #endif

            List<Curve> fks = new List<Curve>();
            for (uint k = 1; k < k0; k++)
            {
                Curve fk = PeriodicSegmentConvolution(
                    pseudoPeriodLength: pseudoPeriodLength,
                    pseudoPeriodHeight: pseudoPeriodHeight,
                    k: k
                );

                fks.Add(fk);
            }

            List<Curve> gis = new List<Curve>();
            for (uint i = 0; i < i0; i++)
            {
                Curve gi = TransversalViewOfIteratedConvolutions(
                    pseudoPeriodLength: pseudoPeriodLength,
                    pseudoPeriodHeight: pseudoPeriodHeight,
                    i: i
                );

                gis.Add(gi);
            }

            return new SubAdditiveCurve( 
                Curve.Minimum(
                    Enumerable.Concat(fks, gis).ToList(),
                    settings
                ), false);

            bool CheckValidK(int candidateK)
            {
                Rational leftSide = Math.Ceiling((decimal)(candidateK * StartTime / pseudoPeriodLength))
                                    * (Slope - (pseudoPeriodHeight / pseudoPeriodLength));
                Rational rightSide =
                    (candidateK * StartTime / pseudoPeriodLength) * (Slope - RightLimitAtStartTime / StartTime);

                return leftSide <= rightSide;
            }
        }

        //In [BT07], second leaf branch of algorithm 11.
        SubAdditiveCurve PeriodicSegmentClosureTypeB()
        {
            int k0 = (int)Math.Floor((decimal)(pseudoPeriodLength / Length)) + 1;

            int bigK = 1;
            while (!CheckValidK(bigK))
                bigK++;

            int k1 = k0 + bigK;

            #if DO_LOG
            logger.Trace($"Periodic segment closure type B. k0: {k0} ; bigK: {bigK} ; k1: {k1}");
            #endif

            List<Curve> fks = new List<Curve>();
            for (uint k = 1; k < k1; k++)
            {
                Curve fk = PeriodicSegmentConvolution(
                    pseudoPeriodLength: pseudoPeriodLength,
                    pseudoPeriodHeight: pseudoPeriodHeight,
                    k: k
                );

                fks.Add(fk);
            }

            return new SubAdditiveCurve( 
                Curve.Minimum(fks, settings), 
                false
            );

            bool CheckValidK(int candidateK)
            {
                if (StartTime == 0)
                    return true;

                Rational leftSide = Math.Floor((decimal)(candidateK * StartTime / pseudoPeriodLength))
                                    * (Slope - (pseudoPeriodHeight / pseudoPeriodLength));
                Rational rightSide =
                    (candidateK * StartTime / pseudoPeriodLength) * (Slope - StartSlope);

                return leftSide >= rightSide;
            }
        }

        //In [BT07], third leaf branch of algorithm 11.
        SubAdditiveCurve PeriodicSegmentClosureTypeC()
        {
            int k0 = (int)Math.Floor((decimal)(pseudoPeriodLength / Length)) + 1;

            int bigK = 1;
            while (!CheckValidK(bigK))
                bigK++;

            int i0 = (int)Math.Floor((decimal)(bigK * EndTime / pseudoPeriodLength));

            #if DO_LOG
            logger.Trace($"Periodic segment closure type C. k0: {k0} ; bigK: {bigK} ; i0: {i0}");
            #endif

            List<Curve> fks = new List<Curve>();
            for (uint k = 1; k < k0; k++)
            {
                Curve fk = PeriodicSegmentConvolution(
                    pseudoPeriodLength: pseudoPeriodLength,
                    pseudoPeriodHeight: pseudoPeriodHeight,
                    k: k
                );

                fks.Add(fk);
            }

            List<Curve> gis = new List<Curve>();
            if (i0 == 0)
            {
                //The 0 case must always be included
                gis.Add(
                    TransversalViewOfIteratedConvolutions(
                        pseudoPeriodLength: pseudoPeriodLength,
                        pseudoPeriodHeight: pseudoPeriodHeight,
                        i: 0
                    )
                );
            }
            else
            {
                for (uint i = 0; i < i0; i++)
                {
                    Curve gi = TransversalViewOfIteratedConvolutions(
                        pseudoPeriodLength: pseudoPeriodLength,
                        pseudoPeriodHeight: pseudoPeriodHeight,
                        i: i
                    );

                    gis.Add(gi);
                }
            }

            return new SubAdditiveCurve( 
                Curve.Minimum(
                    Enumerable.Concat(fks, gis).ToList(),
                    settings
                ), false
            );

            bool CheckValidK(int candidateK)
            {
                Rational leftSide = Math.Floor((decimal)(candidateK * EndTime / pseudoPeriodLength))
                                    * ((pseudoPeriodHeight / pseudoPeriodLength) - Slope);
                Rational rightSide =
                    (candidateK * EndTime / pseudoPeriodLength) * ((LeftLimitAtEndTime / EndTime) - Slope);

                return leftSide >= rightSide;
            }
        }

        //In [BT07], fourth leaf branch of algorithm 11.
        SubAdditiveCurve PeriodicSegmentClosureTypeD()
        {
            int k0 = (int)Math.Floor((decimal)(pseudoPeriodLength / Length)) + 1;

            int bigK = 1;
            while (!CheckValidK(bigK))
                bigK++;

            int k1 = k0 + bigK;

            #if DO_LOG
            logger.Trace($"Periodic segment closure type B. k0: {k0} ; bigK: {bigK} ; k1: {k1}");
            #endif

            List<Curve> fks = new List<Curve>();
            for (uint k = 1; k < k1; k++)
            {
                Curve fk = PeriodicSegmentConvolution(
                    pseudoPeriodLength: pseudoPeriodLength,
                    pseudoPeriodHeight: pseudoPeriodHeight,
                    k: k
                );

                fks.Add(fk);
            }

            return new SubAdditiveCurve(
                Curve.Minimum(fks.ToList(), settings),
                false
            );

            bool CheckValidK(int candidateK)
            {
                Rational leftSide = Math.Ceiling((decimal)(candidateK * EndTime / pseudoPeriodLength))
                                    * ((pseudoPeriodHeight / pseudoPeriodLength) - Slope);
                Rational rightSide =
                    (candidateK * EndTime / pseudoPeriodLength) * ((LeftLimitAtEndTime / EndTime) - Slope);

                return leftSide <= rightSide;
            }
        }
    }

    /// <summary>
    /// Iterated convolutions of a periodic segment.
    /// </summary>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 12</remarks>
    internal Curve PeriodicSegmentConvolution(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        uint k)
    {
        if (k == 0)
            throw new ArgumentException("k must be greater than 0");   

        Rational pseudoPeriodSlope = pseudoPeriodHeight / pseudoPeriodLength;

        //ifs are inverted wrt [BT07] since cases k*length <= d have the same body
        if (k * Length <= pseudoPeriodLength)
        {
            return DisjointSegments();
        }
        else //k * Length > pseudoPeriodLength
        {
            if (pseudoPeriodSlope <= Slope)
            {
                return OverlapAbove();
            }
            else //pseudoPeriodSlope > Slope
            {
                return OverlapBelow();
            }
        }

        //local functions
        Curve DisjointSegments()
        {
            return new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        new Segment(
                            startTime: k * StartTime,
                            endTime: k * EndTime,
                            rightLimitAtStartTime: k * RightLimitAtStartTime,
                            slope: Slope)
                    },
                    fillFrom: 0,
                    fillTo: k * StartTime + pseudoPeriodLength
                ),
                pseudoPeriodStart: k * StartTime,
                pseudoPeriodLength: pseudoPeriodLength,
                pseudoPeriodHeight: pseudoPeriodHeight
            );
        }

        Curve OverlapAbove()
        {
            //In this implementation of Curve pseudo-periods are left-closed and right-open
            //This case however, as described in [BT07], has pseudo-periods left-open and right-closed.
            //So we need to write by hand the second pseudo-period and adjust parameters to
            //obtain an equivalent representation.

            Rational startTime = k * StartTime;  //period start for [1]
            Rational startRightLimit = k * RightLimitAtStartTime;

            Rational midTime = startTime + pseudoPeriodLength;   //adjusted period start
            Rational midPointValue = startRightLimit + Slope * pseudoPeriodLength;
            Rational midRightLimit = startRightLimit + pseudoPeriodHeight;

            Rational endTime = midTime + pseudoPeriodLength;

            return new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        new Segment(
                            startTime: startTime,
                            endTime: midTime,
                            rightLimitAtStartTime: startRightLimit,
                            slope: Slope
                        ),
                        new Point(
                            time: midTime,
                            value: midPointValue
                        ),
                        new Segment(
                            startTime: midTime,
                            endTime: endTime,
                            rightLimitAtStartTime: midRightLimit,
                            slope: Slope
                        )
                    },
                    fillFrom: 0,
                    fillTo: endTime
                ),
                pseudoPeriodStart: midTime,
                pseudoPeriodLength: pseudoPeriodLength,
                pseudoPeriodHeight: pseudoPeriodHeight
            );
        }

        Curve OverlapBelow()
        {
            Rational startTime = k * StartTime;
            Rational startRightLimit = k * RightLimitAtStartTime;

            Rational midTime = k * EndTime;
            Rational midValue = k * RightLimitAtStartTime + pseudoPeriodHeight + Slope * (k * Length - pseudoPeriodLength);

            Rational endTime = k * EndTime + pseudoPeriodLength;

            return new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        new Segment(
                            startTime: startTime,
                            endTime: midTime,
                            rightLimitAtStartTime: startRightLimit,
                            slope: Slope
                        ),
                        new Point(time: midTime, value: midValue),
                        new Segment(
                            startTime: midTime,
                            endTime: endTime,
                            rightLimitAtStartTime: midValue,
                            slope: Slope
                        )
                    },
                    fillFrom: 0,
                    fillTo: endTime
                ),
                pseudoPeriodStart: midTime,
                pseudoPeriodLength: pseudoPeriodLength,
                pseudoPeriodHeight: pseudoPeriodHeight,
                isPartialCurve: true
            );
        }
    }

    /// <summary>
    /// Transversal view of iterated convolutions of a periodic segment.
    /// </summary>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 13</remarks>
    internal Curve TransversalViewOfIteratedConvolutions(
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        uint i)
    {
        Rational k0 = Math.Floor((decimal) (pseudoPeriodLength / Length)) + 1;
        Rational pseudoPeriodicSlope = pseudoPeriodHeight / pseudoPeriodLength;

        if (pseudoPeriodicSlope <= Slope)
        {
            if (StartSlope < pseudoPeriodicSlope)
            {
                //Periodic segment convolution of type A
                //In [BT07], cases are > and >=, we deduced it's a typo in the second case

                if (StartTime > pseudoPeriodLength)
                {
                    return TransversalViewTypeA();
                }
                else //StartTime <= pseudoPeriodLength
                {
                    return TransversalViewTypeB();
                }
            }
            else
            {
                //Periodic segment convolution of type B, should not involve transversal views
                throw new ArgumentException("Periodic segment convolution of type B should not involve transversal views");
            }
        }
        else //pseudoPeriodicSlope > Slope
        {
            if (i == 0)
            {
                if(Slope <= StartSlope)
                    return TransversalViewTypeC();
                else
                    return TransversalViewTypeD();
            }
            else //i >= 1
            {
                if (EndTime >= pseudoPeriodLength) 
                {
                    return TransversalViewTypeE();
                }
                else //EndTime < pseudoPeriodLength
                {
                    if (EndSlope <= Slope)
                        return TransversalViewTypeF();
                    else
                        return TransversalViewTypeG();
                }
            }
        }

        //Local functions

        Curve TransversalViewTypeA()
        {
            Rational baseTime = k0 * StartTime + i * pseudoPeriodLength;
            Rational baseValue = k0 * RightLimitAtStartTime + i * pseudoPeriodHeight;

            return new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        new Segment(
                            startTime: baseTime,
                            endTime: baseTime + pseudoPeriodLength,
                            rightLimitAtStartTime: baseValue,
                            slope: Slope
                        ),
                        new Point(
                            time: baseTime + pseudoPeriodLength,
                            value: baseValue + pseudoPeriodLength * Slope
                        )
                    },
                    fillFrom: 0,
                    fillTo: baseTime + StartTime
                ),
                pseudoPeriodStart: baseTime,
                pseudoPeriodLength: StartTime,
                pseudoPeriodHeight: RightLimitAtStartTime
            );
        }

        Curve TransversalViewTypeB()
        {
            //Similarly to PeriodicSegmentConvolution::OverlapAbove,
            //we have to fill in the second period to mimic right-closeness

            Rational baseTime = k0 * StartTime + i * pseudoPeriodLength;
            Rational baseValue = k0 * RightLimitAtStartTime + i * pseudoPeriodHeight;

            Rational midTime = baseTime + StartTime;
            Rational midPointValue = baseValue + StartTime * Slope;
            Rational midRightLimit = (k0 + 1) * RightLimitAtStartTime + i * pseudoPeriodHeight;

            return new Curve(
                baseSequence: new Sequence(
                    elements: new Element[]
                    {
                        Point.Origin(),
                        new Segment(
                            startTime: baseTime,
                            endTime: baseTime + StartTime,
                            rightLimitAtStartTime: baseValue,
                            slope: Slope
                        ),
                        new Point(
                            time: midTime,
                            value: midPointValue
                        ),
                        new Segment(
                            startTime: midTime,
                            endTime: midTime + StartTime,
                            rightLimitAtStartTime: midRightLimit,
                            slope: Slope
                        )
                    },
                    fillFrom: 0,
                    fillTo: midTime + StartTime
                ),
                pseudoPeriodStart: midTime,
                pseudoPeriodLength: StartTime,
                pseudoPeriodHeight: RightLimitAtStartTime
            );
        }

        Curve TransversalViewTypeC()
        {
            Rational bigK0 = Rational.Max(
                Math.Floor((decimal) (StartTime / Length)) + 1,
                k0);

            List<Element> elements = new List<Element>() { Point.Origin() };

            for (Rational k = k0; k <= bigK0; k++)
            {
                elements.Add(
                    new Segment(
                        startTime: k * StartTime,
                        endTime: k * EndTime,
                        rightLimitAtStartTime: k * RightLimitAtStartTime,
                        slope: Slope
                    )
                );
            }

            Rational closingStart = bigK0 * EndTime;
            Rational closingEnd = (bigK0 + 1) * EndTime;
            Rational closingStartValue = (bigK0 + 1) * LeftLimitAtEndTime - Slope * EndTime;

            elements.AddRange( new Element[] {
                new Point(
                    time: closingStart,
                    value: closingStartValue
                ),
                new Segment(
                    startTime: closingStart,
                    endTime: closingEnd,
                    rightLimitAtStartTime: closingStartValue,
                    slope: Slope
                )
            });

            return new Curve(
                baseSequence: new Sequence(elements, fillFrom: 0, fillTo: EndTime),
                pseudoPeriodStart: closingStart,
                pseudoPeriodLength: EndTime,
                pseudoPeriodHeight: LeftLimitAtEndTime
            );
        }

        Curve TransversalViewTypeD()
        {
            Rational bigK0 = Rational.Max(
                Math.Floor((decimal)(StartTime / Length)) + 1,
                k0);

            List<Element> elements = new List<Element>() { Point.Origin() };

            for (Rational k = k0; k < bigK0; k++)
            {
                elements.Add(
                    new Segment(
                        startTime: k * StartTime,
                        endTime: k * EndTime,
                        rightLimitAtStartTime: k * RightLimitAtStartTime,
                        slope: Slope
                    )
                );
            }

            Rational closingStartTime = bigK0 * StartTime;
            Rational closingStartValue = bigK0 * RightLimitAtStartTime;

            Rational closingMidTime = (bigK0 + 1) * StartTime;
            Rational closingMidPointValue = closingStartValue + Slope * StartTime;
            Rational closingMidRightValue = closingStartValue + RightLimitAtStartTime;

            Rational closingEndTime = (bigK0 + 2) * StartTime;

            elements.AddRange(new Element[] {
                new Segment(
                    startTime: closingStartTime,
                    endTime: closingMidTime,
                    rightLimitAtStartTime: closingStartValue,
                    slope: Slope
                ),
                new Point(
                    time: closingMidTime,
                    value: closingMidPointValue
                ),
                new Segment(
                    startTime: closingMidTime,
                    endTime: closingEndTime,
                    rightLimitAtStartTime: closingMidRightValue,
                    slope: Slope
                ),
            });

            return new Curve(
                baseSequence: new Sequence(elements, fillFrom: 0, fillTo: closingEndTime),
                pseudoPeriodStart: closingMidTime,
                pseudoPeriodLength: StartTime,
                pseudoPeriodHeight: RightLimitAtStartTime
            );
        }

        Curve TransversalViewTypeE()
        {
            Rational baseTime = k0 * EndTime + (i - 1) * pseudoPeriodLength;
            Rational baseValue = k0 * LeftLimitAtEndTime + i * pseudoPeriodHeight - Slope * pseudoPeriodLength;

            Rational segmentEndTime = baseTime + pseudoPeriodLength;
            Rational periodEndTime = baseTime + EndTime;

            Sequence baseSequence = new Sequence(
                elements: new Element[]
                {
                    Point.Origin(),
                    new Point(
                        time: baseTime,
                        value: baseValue
                    ),
                    new Segment(
                        startTime: baseTime,
                        endTime: segmentEndTime,
                        rightLimitAtStartTime: baseValue,
                        slope: Slope
                    )
                },
                fillFrom: 0,
                fillTo: periodEndTime
            );

            return new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: baseTime,
                pseudoPeriodLength: EndTime,
                pseudoPeriodHeight: LeftLimitAtEndTime
            );
        }

        Curve TransversalViewTypeF()
        {
            Rational baseTime = k0 * EndTime + (i - 1) * pseudoPeriodLength;
            Rational baseValue = k0 * LeftLimitAtEndTime + i * pseudoPeriodHeight - Slope * pseudoPeriodLength;

            Rational periodEndTime = baseTime + EndTime;

            Sequence baseSequence = new Sequence(
                elements: new Element[]
                {
                    Point.Origin(),
                    new Point(
                        time: baseTime,
                        value: baseValue
                    ),
                    new Segment(
                        startTime: baseTime,
                        endTime: periodEndTime,
                        rightLimitAtStartTime: baseValue,
                        slope: Slope
                    )
                },
                fillFrom: 0,
                fillTo: periodEndTime
            );

            return new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: baseTime,
                pseudoPeriodLength: EndTime,
                pseudoPeriodHeight: LeftLimitAtEndTime
            );
        }

        Curve TransversalViewTypeG()
        {
            //Note that this case diverges from [BT07].
            //Applying the definition on an example, we found the implementation conceptually wrong.
            //This is the corrected version, matching the expected result in the test.

            Rational startTime = k0 * EndTime + (i - 1) * pseudoPeriodLength;
            Rational startValue = k0 * LeftLimitAtEndTime + i * pseudoPeriodHeight - Slope * pseudoPeriodLength;

            Rational midTime = startTime + pseudoPeriodLength;
            Rational midValue = (k0 + 1) * LeftLimitAtEndTime + i * pseudoPeriodHeight - Slope * EndTime;

            Rational periodEndTime = midTime + EndTime;

            Sequence baseSequence = new Sequence(
                elements: new Element[]
                {
                    Point.Origin(),
                    new Point(
                        time: startTime,
                        value: startValue
                    ),
                    new Segment(
                        startTime: startTime,
                        endTime: midTime,
                        rightLimitAtStartTime: startValue,
                        slope: Slope
                    ),
                    new Point(
                        time: midTime,
                        value: midValue
                    ),
                    new Segment(
                        startTime: midTime,
                        endTime: periodEndTime,
                        rightLimitAtStartTime: midValue,
                        slope: Slope
                    )
                },
                fillFrom: 0,
                fillTo: periodEndTime
            );

            return new Curve(
                baseSequence: baseSequence,
                pseudoPeriodStart: midTime,
                pseudoPeriodLength: EndTime,
                pseudoPeriodHeight: LeftLimitAtEndTime
            );
        }
    }

    #endregion Sub-additive closure
}