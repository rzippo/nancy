using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// Adds two streams of elements over the intersection of their supports.
    /// </summary>
    /// <param name="a">The first operand, in time order and uninterrupted, as the elements of a <see cref="Sequence"/>.</param>
    /// <param name="b">The second operand, as <paramref name="a"/>.</param>
    /// <returns>The elements of the sum in time order.</returns>
    /// <remarks>
    /// Within the intersection, the result has a point at each breakpoint of either operand and a segment between consecutive breakpoints.
    /// Use <see cref="MergeAsEnumerable"/> to merge adjacent compatible elements.
    /// </remarks>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time, when the result is first enumerated.</exception>
    /// <exception cref="UndeterminedResultException">If the operands are opposite infinities at a shared time, when the result reaches it.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Addition(this IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
            yield return Element.Addition(left, right);
    }

    /// <summary>
    /// Subtracts two streams of elements over the intersection of their supports.
    /// </summary>
    /// <param name="a">The first operand, in time order and uninterrupted, as the elements of a <see cref="Sequence"/>.</param>
    /// <param name="b">The second operand, as <paramref name="a"/>.</param>
    /// <returns>The elements of the difference in time order.</returns>
    /// <remarks>
    /// Within the intersection, the result has a point at each breakpoint of either operand and a segment between consecutive breakpoints.
    /// Use <see cref="MergeAsEnumerable"/> to merge adjacent compatible elements.
    /// The result may contain negative values.
    /// </remarks>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time, when the result is first enumerated.</exception>
    /// <exception cref="UndeterminedResultException">If the operands are the same infinity at a shared time, when the result reaches it.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Subtraction(this IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
            yield return Element.Subtraction(left, right);
    }

    /// <summary>
    /// Negates each element of a stream.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <returns>The negated elements over the same support.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Negate(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            yield return cursor.Current.Negate();
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Multiplies the values of an element stream by a scalar.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <param name="scaling">The multiplicative factor.</param>
    /// <returns>The scaled elements over the same support.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Scale(this IEnumerable<Element> elements, Rational scaling)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            yield return cursor.Current.Scale(scaling);
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Adds an offset to the values of an element stream.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <param name="shift">The additive offset.</param>
    /// <param name="exceptOrigin">Whether to preserve a point at time zero.</param>
    /// <returns>The shifted elements over the same support.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> VerticalShift(this IEnumerable<Element> elements, Rational shift, bool exceptOrigin = false)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            yield return shift == 0 || (exceptOrigin && cursor.Current is Point { Time.IsZero: true })
                ? cursor.Current
                : cursor.Current.VerticalShift(shift);
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Delays the support of an element stream by a finite non-negative time.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <param name="delay">The non-negative delay.</param>
    /// <param name="prependWithZero">Whether to fill the added initial interval with zero.</param>
    /// <returns>The delayed elements, including the optional initial zero interval.</returns>
    /// <remarks>
    /// The zero prefix preserves the outer left endpoint's inclusion.
    /// At its join with a delayed open start, the value is zero; at a delayed closed start, the original point supplies the value.
    /// A zero delay yields the original element objects while validating their order.
    /// A positive delay with a zero prefix requires a finite left endpoint; this is checked during enumeration before yielding any elements.
    /// </remarks>
    /// <exception cref="ArgumentException">If the delay is negative or infinite.</exception>
    /// <exception cref="ArgumentException">If <paramref name="delay"/> is positive, <paramref name="prependWithZero"/> is true, and the support is left-unbounded.</exception>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Delay(this IEnumerable<Element> elements, Rational delay, bool prependWithZero = true)
    {
        if (delay < 0 || delay.IsInfinite)
            throw new ArgumentException("Delay must be finite and non-negative.");
        return Delayed();

        IEnumerable<Element> Delayed()
        {
            using var cursor = new ElementCursor(elements);
            if (!cursor.MoveNext())
                throw new ArgumentException("The element stream is empty.", nameof(elements));

            if (delay == 0)
            {
                do
                {
                    yield return cursor.Current;
                } while (cursor.MoveNext());
                yield break;
            }

            if (prependWithZero)
            {
                var start = cursor.Current.StartTime;
                if (start.IsMinusInfinite)
                    throw new ArgumentException("Cannot prepend zero to left-unbounded support for a positive delay.", nameof(prependWithZero));
                if (cursor.Current is Point)
                    yield return Point.Zero(start);
                yield return Segment.Zero(start, start + delay);
                if (cursor.Current is Segment)
                    yield return Point.Zero(start + delay);
            }
            do
            {
                yield return cursor.Current.Delay(delay);
            } while (cursor.MoveNext());
        }
    }

    /// <summary>
    /// Translates an element stream by a finite non-negative advance, $f(t + T)$, including times before zero.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <param name="time">The non-negative advance.</param>
    /// <returns>The translated elements over the shifted support.</returns>
    /// <exception cref="ArgumentException">If the advance is negative or infinite.</exception>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Forward(this IEnumerable<Element> elements, Rational time)
    {
        if (time < 0 || time.IsInfinite)
            throw new ArgumentException("Advance must be finite and non-negative.");
        return Advanced();

        IEnumerable<Element> Advanced()
        {
            using var cursor = new ElementCursor(elements);
            if (!cursor.MoveNext())
                throw new ArgumentException("The element stream is empty.", nameof(elements));

            if (time == 0)
            {
                do
                {
                    yield return cursor.Current;
                } while (cursor.MoveNext());
                yield break;
            }

            do
            {
                yield return cursor.Current.Forward(time);
            } while (cursor.MoveNext());
        }
    }

    /// <summary>
    /// Shifts an element stream horizontally, using a zero prefix for positive shifts and a pure translation for negative shifts.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <param name="shift">The finite signed shift.</param>
    /// <returns>The elements following the support conventions of the sequence horizontal shift.</returns>
    /// <exception cref="ArgumentException">If the shift is infinite.</exception>
    /// <exception cref="ArgumentException">During enumeration, if <paramref name="shift"/> is positive and the support is left-unbounded.</exception>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> HorizontalShift(this IEnumerable<Element> elements, Rational shift)
    {
        if (shift.IsInfinite) throw new ArgumentException("Shift must be finite.");
        return shift >= 0 ? elements.Delay(shift) : elements.Forward(-shift);
    }

    /// <summary>
    /// Enumerates the floor of an element stream.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <returns>Elements of the floor, with integer crossings included.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Floor(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            foreach (var element in cursor.Current.Floor())
                yield return element;
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Enumerates the ceiling of an element stream.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <returns>Elements of the ceiling, with integer crossings included.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Ceil(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            foreach (var element in cursor.Current.Ceil())
                yield return element;
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Clamps the values of an element stream below at zero.
    /// </summary>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <returns>The clamped elements, including crossings of zero.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> ToNonNegative(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = cursor.Current;
            if (element is Point point)
                yield return new Point(point.Time, Rational.Max(point.Value, 0));
            else
                foreach (var result in Segment.Maximum((Segment) element, Segment.Zero(element.StartTime, element.EndTime)))
                    yield return result;
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Computes a left-continuous version of this sequence.
    /// </summary>
    /// <returns>Elements in time order, with each point set to the left limit where one exists.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    public static IEnumerable<Element> ToLeftContinuous(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        Segment? left = null;
        var seen = false;
        while (cursor.MoveNext())
        {
            seen = true;
            if (cursor.Current is Segment segment)
            {
                left = segment;
                yield return segment;
            }
            else
            {
                var point = (Point) cursor.Current;
                yield return left is not null && left.LeftLimitAtEndTime != point.Value
                    ? new Point(point.Time, left.LeftLimitAtEndTime)
                    : point;
            }
        }
        if (!seen)
            throw new ArgumentException("The element stream is empty.", nameof(elements));
    }

    /// <summary>
    /// Computes a right-continuous version of this sequence.
    /// </summary>
    /// <returns>Elements in time order, with each point set to the right limit where one exists.</returns>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    public static IEnumerable<Element> ToRightContinuous(this IEnumerable<Element> elements)
    {
        using var cursor = new ElementCursor(elements);
        if (!cursor.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));

        do
        {
            if (cursor.Current is Segment initial)
            {
                yield return initial;
                continue;
            }

            var point = (Point) cursor.Current;
            if (!cursor.MoveNext())
            {
                yield return point;
                yield break;
            }
            var right = (Segment) cursor.Current;
            yield return right.RightLimitAtStartTime != point.Value
                ? new Point(point.Time, right.RightLimitAtStartTime)
                : point;
            yield return right;
        } while (cursor.MoveNext());
    }
}
