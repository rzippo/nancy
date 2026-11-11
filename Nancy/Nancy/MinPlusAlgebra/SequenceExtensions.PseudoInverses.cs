using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
    /// <summary>
    /// Computes the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\}$, where $t \in Support(f)$ and $x \in Image(f)$.
    /// </summary>
    /// <param name="startFromZero">
    /// If true, $f^{-1}_\downarrow(x)$ is defined from $x = 0$, rather than only in $Image(f)$.
    /// At a height from 0 below $Image(f)$, the result is the start of the support.
    /// If every value of the sequence is negative, the result is a point at height 0 with value $+\infty$.
    /// </param>
    /// <exception cref="ArgumentException">If the elements are not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous.
    /// See [DNC18] § 3.2.1
    /// If there are jumps or plateaus at the endpoints, those may also be lost during pseudoinversion. 
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> LowerPseudoInverse(this IEnumerable<Element> elements, bool startFromZero = false)
    {
        var merged = startFromZero ?
            ValidatedElements().MergeAsEnumerable().SkipUntilValue(0):
            ValidatedElements().MergeAsEnumerable();

        using var enumerator = merged.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            if (!startFromZero)
                throw new ArgumentException("The element stream is empty.", nameof(elements));
            // no value reaches 0: no instant has f(t) >= 0, and the infimum of the empty set is +inf
            yield return Point.PlusInfinite(0);
            yield break;
        }

        Rational previousValue = startFromZero ? Rational.Zero : Rational.MinusInfinity;
        bool? wasPreviousPoint = null;
        do
        {
            switch (enumerator.Current)
            {
                case Point p:
                {
                    if (p.Value > previousValue)
                    {
                        if (previousValue > Rational.MinusInfinity)
                        {
                            // left-discontinuity, becomes constant segment
                            if (wasPreviousPoint != true)
                            {
                                yield return new Point(time: previousValue, value: p.Time);
                            }
                            yield return new Segment(
                                startTime: previousValue,
                                endTime: p.Value,
                                rightLimitAtStartTime: p.Time,
                                slope: 0
                            );
                        }
                        yield return p.Inverse();
                        previousValue = p.Value;
                        wasPreviousPoint = true;
                    }
                    else if (p.Value == previousValue && wasPreviousPoint != true)
                    {
                        yield return p.Inverse();
                        previousValue = p.Value;
                        wasPreviousPoint = true;
                    }
                    else if (p.Value < previousValue)
                    {
                        throw new ArgumentException("The sequence is not non-decreasing.");
                    }

                    break;
                }

                case Segment s:
                {
                    if (s.IsConstant)
                    {
                        // the segment itself is skipped, as constant segments become right-discontinuities
                        if (s.RightLimitAtStartTime == previousValue)
                        {
                            if (wasPreviousPoint == null)
                            {
                                // this is the first element of the sequence, so the left-continuity point must be processed here
                                yield return new Point(time: s.RightLimitAtStartTime, value: s.StartTime);
                                wasPreviousPoint = true;
                            }
                            else
                            {
                                // do nothing, as the left-continuity point has already been processed
                            }
                        }
                        else if (s.RightLimitAtStartTime > previousValue)
                        {
                            if (wasPreviousPoint == null && previousValue > Rational.MinusInfinity)
                            {
                                // the sequence starts above 0, with no point at its start: the start reaches the heights from 0 on
                                yield return new Point(time: previousValue, value: s.StartTime);
                                wasPreviousPoint = true;
                            }

                            if (wasPreviousPoint == true)
                            {
                                // right-discontinuity, becomes constant segment
                                yield return new Segment(
                                    startTime: previousValue,
                                    endTime: s.RightLimitAtStartTime,
                                    rightLimitAtStartTime: s.StartTime,
                                    slope: 0
                                );
                            }

                            // left-continuity point as inverse of constant segment
                            yield return new Point(
                                time: s.RightLimitAtStartTime,
                                value: s.StartTime
                            );
                            previousValue = s.RightLimitAtStartTime;
                            wasPreviousPoint = true;
                        }
                        else if (s.RightLimitAtStartTime < previousValue)
                        {
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        }
                    }
                    else if (s.Slope > 0)
                    {
                        if (s.RightLimitAtStartTime > previousValue)
                        {
                            if (previousValue != Rational.MinusInfinity)
                            {
                                if (wasPreviousPoint == null)
                                    // the sequence starts above 0, with no point at its start: the start reaches the heights from 0 on
                                    yield return new Point(time: previousValue, value: s.StartTime);

                                // right-discontinuity, becomes constant segment
                                yield return new Segment(
                                    startTime: previousValue,
                                    endTime: s.RightLimitAtStartTime,
                                    rightLimitAtStartTime: s.StartTime,
                                    slope: 0
                                );
                                yield return new Point(
                                    time: s.RightLimitAtStartTime,
                                    value: s.StartTime
                                );
                            }
                            // then the segment inverse
                            yield return s.Inverse();
                            previousValue = s.LeftLimitAtEndTime;
                            wasPreviousPoint = false;
                        }
                        else if (s.RightLimitAtStartTime == previousValue)
                        {
                            if (wasPreviousPoint == null && previousValue > Rational.MinusInfinity)
                                // the sequence starts at 0, with no point at its start: the start reaches 0
                                yield return new Point(time: previousValue, value: s.StartTime);

                            // right-continuity, simple inverse
                            yield return s.Inverse();
                            previousValue = s.LeftLimitAtEndTime;
                            wasPreviousPoint = false;
                        }
                        else if (s.RightLimitAtStartTime < previousValue)
                        {
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        }
                    }
                    else if (s.Slope < 0)
                    {
                        throw new ArgumentException("The sequence is not non-decreasing.");
                    }

                    break;
                }
            }
        }
        while (enumerator.MoveNext());

        IEnumerable<Element> ValidatedElements()
        {
            using var cursor = new ElementCursor(elements);
            if (!cursor.MoveNext())
                throw new ArgumentException("The element stream is empty.", nameof(elements));
            do
            {
                yield return cursor.Current;
            } while (cursor.MoveNext());
        }

    }

    /// <summary>
    /// Computes the upper pseudo-inverse function,
    /// $f^{-1}_\uparrow(x) = \sup \left\{ t : f(t) \le x \right\}$, where $t \in Support(f)$ and $x \in Image(f)$.
    /// </summary>
    /// <param name="startFromZero">
    /// If true, $f^{-1}_\uparrow(x)$ is defined from $x = 0$, rather than only in $Image(f)$.
    /// At a height from 0 below $Image(f)$, the result is $-\infty$.
    /// If every value of the sequence is negative, the result is a point at height 0 whose value is the end of the support.
    /// </param>
    /// <exception cref="ArgumentException">If the elements are not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is right-continuous, thus is revertible, i.e. $\left(f^{-1}_\uparrow\right)^{-1}_\uparrow = f$, only if $f$ is right-continuous.
    /// See [DNC18] § 3.2.1
    /// If there are jumps or plateaus at the endpoints, those may also be lost during pseudoinversion. 
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> UpperPseudoInverse(this IEnumerable<Element> elements, bool startFromZero = false)
    {
        var source = ValidatedElements();
        var merged = (startFromZero ? WithNonNegativePrefix(source) : source).MergeAsEnumerable();
        var upi = Inverse();

        // from 0 up to the first value of the sequence, no instant has f(t) <= y, and the supremum is -inf
        return startFromZero ? WithPrefix() : upi;

        IEnumerable<Element> WithPrefix()
        {
            using var enumerator = upi.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new ArgumentException("The inverse has no elements.", nameof(elements));
            var first = enumerator.Current;
            if (first.StartTime > 0)
            {
                yield return Point.MinusInfinite(0);
                yield return Segment.MinusInfinite(0, first.StartTime);
            }
            if (first.StartTime >= 0 && first is Segment)
                yield return Point.MinusInfinite(first.StartTime);
            do
            {
                yield return enumerator.Current;
            } while (enumerator.MoveNext());
        }

        IEnumerable<Element> WithNonNegativePrefix(IEnumerable<Element> source)
        {
            // For $y >= 0$, $f(t) <= y$ exactly when $\max(f(t), 0) \le y$.
            // Keep the negative prefix's times, and check its original values before replacing them with zero.
            using var enumerator = source.GetEnumerator();
            var previousValue = Rational.MinusInfinity;
            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case Point point:
                        if (point.Value < previousValue)
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        previousValue = point.Value;
                        if (point.Value < 0)
                        {
                            yield return Point.Zero(point.Time);
                            continue;
                        }
                        yield return point;
                        break;

                    case Segment segment:
                        if (segment.Slope < 0 || segment.RightLimitAtStartTime < previousValue)
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        previousValue = segment.LeftLimitAtEndTime;
                        if (segment.RightLimitAtStartTime < 0)
                        {
                            if (segment.LeftLimitAtEndTime <= 0)
                            {
                                yield return Segment.Zero(segment.StartTime, segment.EndTime);
                                continue;
                            }

                            var (_, crossing, right) = segment.Split(segment.Inverse().ValueAt(0));
                            yield return Segment.Zero(segment.StartTime, crossing.Time);
                            yield return crossing;
                            yield return right;
                        }
                        else
                        {
                            yield return segment;
                        }
                        break;

                    default:
                        throw new InvalidCastException();
                }

                // From the first nonnegative value on, preserve every element so the inverse also sees any later decrease.
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
                yield break;
            }
        }

        IEnumerable<Element> Inverse()
        {
            using var enumerator = merged.GetEnumerator();

            if (!enumerator.MoveNext())
                throw new ArgumentException("The element stream is empty.", nameof(elements));

            Rational previousValue = Rational.MinusInfinity;
            bool? wasPreviousPoint = null;
            Point? heldPoint = null; // points are held back in case there is a right-discontinuity to introduce instead 
            do
            {
                switch (enumerator.Current)
                {
                    case Point p:
                    {
                        if (p.Value > previousValue)
                        {
                            if (previousValue > Rational.MinusInfinity)
                            {
                                // left-discontinuity, becomes constant segment
                                if (wasPreviousPoint != true)
                                {
                                    yield return new Point(time: previousValue, value: p.Time);
                                }
                                yield return new Segment(
                                    startTime: previousValue,
                                    endTime: p.Value,
                                    rightLimitAtStartTime: p.Time,
                                    slope: 0
                                );
                            }
                            // hold back in case the next segment is constant
                            // thus a right-discontinuity should be introduced instead
                            heldPoint = (Point) p.Inverse();
                            previousValue = p.Value;
                            wasPreviousPoint = true;
                        }
                        else if (p.Value == previousValue && wasPreviousPoint != true)
                        {
                            // hold back in case the next segment is constant
                            // thus a right-discontinuity should be introduced instead
                            heldPoint = (Point) p.Inverse();
                            previousValue = p.Value;
                            wasPreviousPoint = true;
                        }
                        else if (p.Value < previousValue)
                        {
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        }

                        break;
                    }

                    case Segment s:
                    {
                        if (s.IsConstant)
                        {
                            // the segment itself is skipped, as constant segments become left-discontinuities
                            if (s.RightLimitAtStartTime == previousValue)
                            {
                                // discard the point held, push right-continuity point instead
                                yield return new Point(
                                    time: s.RightLimitAtStartTime,
                                    value: s.EndTime
                                );
                                heldPoint = null;
                                previousValue = s.RightLimitAtStartTime;
                                wasPreviousPoint = true;
                            }
                            else if (s.RightLimitAtStartTime > previousValue)
                            {
                                if (heldPoint is not null)
                                {
                                    yield return heldPoint;
                                    heldPoint = null;
                                }

                                if (wasPreviousPoint == true)
                                {
                                    // right-discontinuity, becomes constant segment
                                    yield return new Segment(
                                        startTime: previousValue,
                                        endTime: s.RightLimitAtStartTime,
                                        rightLimitAtStartTime: s.StartTime,
                                        slope: 0
                                    );
                                }

                                // right-continuity point as inverse of constant segment
                                yield return new Point(
                                    time: s.RightLimitAtStartTime,
                                    value: s.EndTime
                                );
                                previousValue = s.RightLimitAtStartTime;
                                wasPreviousPoint = true;
                            }
                            else if (s.RightLimitAtStartTime < previousValue)
                            {
                                throw new ArgumentException("The sequence is not non-decreasing.");
                            }
                        }
                        else if (s.Slope > 0)
                        {
                            if (heldPoint is not null)
                            {
                                yield return heldPoint;
                                heldPoint = null;
                            }

                            if (s.RightLimitAtStartTime > previousValue)
                            {
                                if (previousValue != Rational.MinusInfinity)
                                {
                                    // right-discontinuity, becomes constant segment
                                    yield return new Segment(
                                        startTime: previousValue,
                                        endTime: s.RightLimitAtStartTime,
                                        rightLimitAtStartTime: s.StartTime,
                                        slope: 0
                                    );
                                    yield return new Point(
                                        time: s.RightLimitAtStartTime,
                                        value: s.StartTime
                                    );
                                }
                                // then the segment inverse
                                yield return s.Inverse();
                                previousValue = s.LeftLimitAtEndTime;
                                wasPreviousPoint = false;
                            }
                            else if (s.RightLimitAtStartTime == previousValue)
                            {
                                // right-continuity, simple inverse
                                yield return s.Inverse();
                                previousValue = s.LeftLimitAtEndTime;
                                wasPreviousPoint = false;
                            }
                            else if (s.RightLimitAtStartTime < previousValue)
                            {
                                throw new ArgumentException("The sequence is not non-decreasing.");
                            }
                        }
                        else if (s.Slope < 0)
                        {
                            throw new ArgumentException("The sequence is not non-decreasing.");
                        }

                        break;
                    }
                }
            }
            while (enumerator.MoveNext());

            if (heldPoint is not null)
                yield return heldPoint;
        }

        IEnumerable<Element> ValidatedElements()
        {
            using var cursor = new ElementCursor(elements);
            if (!cursor.MoveNext())
                throw new ArgumentException("The element stream is empty.", nameof(elements));
            do
            {
                yield return cursor.Current;
            } while (cursor.MoveNext());
        }

    }

    /// <summary>
    /// Skips the prefix below <paramref name="value"/>, splitting a rising segment where it first reaches that height.
    /// </summary>
    /// <param name="elements">Elements in uninterrupted time order, which is assumed rather than validated.</param>
    /// <param name="value">The first height to retain.</param>
    /// <returns>The remaining elements; empty if the input is empty or the height is never reached.</returns>
    /// <remarks>
    /// Monotonicity is checked on each consumed element, including the retained tail, before that element is returned.
    /// Fully enumerating the result validates monotonicity over the entire input; stopping early leaves the unread tail unchecked.
    /// </remarks>
    /// <exception cref="ArgumentException">If the consumed elements are not non-decreasing.</exception>
    internal static IEnumerable<Element> SkipUntilValue(this IEnumerable<Element> elements, Rational value)
    {
        using var enumerator = elements.GetEnumerator();
        var previousValue = Rational.MinusInfinity;
        var thresholdReached = false;
        while (enumerator.MoveNext())
        {
            switch (enumerator.Current)
            {
                case Point point:
                {
                    if (point.Value < previousValue)
                        throw new ArgumentException("The sequence is not non-decreasing.");
                    previousValue = point.Value;
                    if (!thresholdReached && point.Value < value)
                        continue;
                    yield return point;
                    break;
                }

                case Segment segment:
                {
                    if (segment.Slope < 0 || segment.RightLimitAtStartTime < previousValue)
                        throw new ArgumentException("The sequence is not non-decreasing.");
                    previousValue = segment.LeftLimitAtEndTime;
                    if (thresholdReached)
                    {
                        yield return segment;
                        break;
                    }
                    if (segment.IsConstant && segment.LeftLimitAtEndTime < value)
                        continue;
                    if (!segment.IsConstant && segment.LeftLimitAtEndTime <= value)
                        continue;
                    if (segment.RightLimitAtStartTime < value)
                    {
                        var (_, center, right) = segment.Split(segment.Inverse().ValueAt(value));
                        yield return center;
                        yield return right;
                    }
                    else
                        yield return segment;
                    break;
                }

                default:
                    throw new InvalidCastException();
            }

            thresholdReached = true;
        }
    }
}
