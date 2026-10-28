using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{

    /// <summary>
    /// Enumerates the pointwise minimum of two streams, over their overlapping support or their full combined time span.
    /// </summary>
    /// <param name="a">The first nonempty uninterrupted stream in time order.</param>
    /// <param name="b">The second nonempty uninterrupted stream in time order.</param>
    /// <param name="cutToOverlap">Whether to restrict the result to intersecting support.</param>
    /// <returns>The lower envelope in time order.</returns>
    /// <remarks>
    /// With <paramref name="cutToOverlap"/> false, the result spans the earliest start through the latest end.
    /// Where only one operand is defined, its value is used; uncovered times have value $+\infty$.
    /// Each outer endpoint is included exactly when an operand defines a value there.
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand is empty or violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Minimum(this IEnumerable<Element> a, IEnumerable<Element> b, bool cutToOverlap)
        => cutToOverlap ? a.Minimum(b) : EnvelopeUnion(a, b, EnvelopeKind.Lower);

    /// <summary>
    /// Enumerates the pointwise maximum of two streams, over their overlapping support or their full combined time span.
    /// </summary>
    /// <param name="a">The first nonempty uninterrupted stream in time order.</param>
    /// <param name="b">The second nonempty uninterrupted stream in time order.</param>
    /// <param name="cutToOverlap">Whether to restrict the result to intersecting support.</param>
    /// <returns>The upper envelope in time order.</returns>
    /// <remarks>
    /// With <paramref name="cutToOverlap"/> false, the result spans the earliest start through the latest end.
    /// Where only one operand is defined, its value is used; uncovered times have value $-\infty$.
    /// Each outer endpoint is included exactly when an operand defines a value there.
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand is empty or violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Maximum(this IEnumerable<Element> a, IEnumerable<Element> b, bool cutToOverlap)
        => cutToOverlap ? a.Maximum(b) : EnvelopeUnion(a, b, EnvelopeKind.Upper);

    /// <summary>
    /// Enumerates the minimum of a finite nonempty collection of element streams over their filled union.
    /// </summary>
    /// <param name="streams">Nonempty uninterrupted operand streams in time order.</param>
    /// <returns>The minimum in time order, with $+\infty$ in uncovered gaps.</returns>
    /// <remarks>
    /// A single operand retains its support.
    /// With multiple operands, each outer endpoint is included exactly when an operand defines a value there.
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand is empty or violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Minimum(this IEnumerable<IEnumerable<Element>> streams)
        => EnvelopeMany(streams, EnvelopeKind.Lower);

    /// <summary>
    /// Enumerates the maximum of a finite nonempty collection of element streams over their filled union.
    /// </summary>
    /// <param name="streams">Nonempty uninterrupted operand streams in time order.</param>
    /// <returns>The maximum in time order, with $-\infty$ in uncovered gaps.</returns>
    /// <remarks>
    /// A single operand retains its support.
    /// With multiple operands, each outer endpoint is included exactly when an operand defines a value there.
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand is empty or violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Maximum(this IEnumerable<IEnumerable<Element>> streams)
        => EnvelopeMany(streams, EnvelopeKind.Upper);

    /// <summary>
    /// Combines a finite collection of streams into a lower or upper envelope through successive binary sweeps.
    /// </summary>
    /// <param name="streams">Nonempty uninterrupted streams in time order.</param>
    /// <param name="kind">Whether to compute the lower or upper envelope.</param>
    /// <returns>The unmerged envelope with state proportional to the operand count.</returns>
    /// <remarks>A single operand retains its original support; two or more operands use the filled span of <see cref="EnvelopeUnion"/>.</remarks>
    /// <exception cref="ArgumentException">If the collection or a consumed operand is empty, or a consumed element violates uninterrupted time order.</exception>
    private static IEnumerable<Element> EnvelopeMany(IEnumerable<IEnumerable<Element>> streams, EnvelopeKind kind)
    {
        IEnumerable<Element>? result = null;
        foreach (var stream in streams)
        {
            result = result is null ? stream : EnvelopeUnion(result, stream, kind);
        }
        if (result is null)
        {
            throw new ArgumentException("The operand collection is empty.");
        }

        // This cursor also validates the single-operand case, where no binary sweep was created.
        using var cursor = new ElementCursor(result);
        if (!cursor.MoveNext())
        {
            throw new ArgumentException("An operand is empty.");
        }
        do
        {
            yield return cursor.Current;
        } while (cursor.MoveNext());
    }

    /// <summary>
    /// Enumerates the lower or upper envelope over the full time span of two uninterrupted streams.
    /// </summary>
    /// <param name="a">The first nonempty stream in time order.</param>
    /// <param name="b">The second nonempty stream in time order.</param>
    /// <param name="kind">Whether to compute the lower or upper envelope.</param>
    /// <returns>The unmerged envelope, including each outer endpoint only when an operand defines a value there.</returns>
    /// <remarks>
    /// Undefined operand values are treated as $+\infty$ for the minimum and $-\infty$ for the maximum.
    /// This uses the other operand where it is defined and fills times where neither operand is defined.
    /// The current input elements determine the next partition boundary, without materializing <see cref="IntervalBucket.ComputeIntervals(Sequence, Sequence)"/>.
    /// Each cursor retains one input element and validates uninterrupted time order as it advances.
    /// </remarks>
    /// <exception cref="ArgumentException">If a consumed operand is empty or violates uninterrupted time order.</exception>
    private static IEnumerable<Element> EnvelopeUnion(IEnumerable<Element> a, IEnumerable<Element> b, EnvelopeKind kind)
    {
        using var left = new ElementCursor(a);
        using var right = new ElementCursor(b);
        var hasLeft = left.MoveNext();
        var hasRight = right.MoveNext();
        if (!hasLeft || !hasRight)
        {
            throw new ArgumentException("An operand is empty.");
        }
        var undefinedValue = kind == EnvelopeKind.Upper ? Rational.MinusInfinity : Rational.PlusInfinity;
        var start = Rational.Min(left.Current.StartTime, right.Current.StartTime);
        var time = start;
        Rational Combine(Rational x, Rational y) => kind == EnvelopeKind.Upper ? Rational.Max(x, y) : Rational.Min(x, y);
        while (true)
        {
            // Move past open segments ending here so their boundary points remain available for evaluation.
            while (hasLeft && left.Current is Segment endedLeft && endedLeft.EndTime <= time)
            {
                hasLeft = left.MoveNext();
            }
            while (hasRight && right.Current is Segment endedRight && endedRight.EndTime <= time)
            {
                hasRight = right.MoveNext();
            }
            if (!hasLeft && !hasRight)
            {
                yield break;
            }

            // A boundary inside the combined span has a value even when both operands are undefined there.
            // The final endpoint is included only when one operand supplies its value.
            var leftIsDefinedAtTime = hasLeft && left.Current.IsDefinedFor(time);
            var rightIsDefinedAtTime = hasRight && right.Current.IsDefinedFor(time);
            var hasLaterSupport = (hasLeft && left.Current.EndTime > time) || (hasRight && right.Current.EndTime > time);
            if (leftIsDefinedAtTime || rightIsDefinedAtTime || (time > start && hasLaterSupport))
            {
                var leftValue = leftIsDefinedAtTime ? left.Current.ValueAt(time) : undefinedValue;
                var rightValue = rightIsDefinedAtTime ? right.Current.ValueAt(time) : undefinedValue;
                yield return new Point(time, Combine(leftValue, rightValue));
            }

            // Point values are settled; advance to the elements governing the following open interval.
            if (hasLeft && left.Current is Point leftPoint && leftPoint.Time == time)
            {
                hasLeft = left.MoveNext();
            }
            if (hasRight && right.Current is Point rightPoint && rightPoint.Time == time)
            {
                hasRight = right.MoveNext();
            }
            if (!hasLeft && !hasRight)
            {
                yield break;
            }

            // An operand either covers the next interval, starts later, or has ended.
            var leftSegment = hasLeft && left.Current is Segment l && l.StartTime <= time ? l : null;
            var rightSegment = hasRight && right.Current is Segment r && r.StartTime <= time ? r : null;
            var nextLeftBoundary = !hasLeft ? Rational.PlusInfinity : leftSegment?.EndTime ?? left.Current.StartTime;
            var nextRightBoundary = !hasRight ? Rational.PlusInfinity : rightSegment?.EndTime ?? right.Current.StartTime;
            var end = Rational.Min(nextLeftBoundary, nextRightBoundary);
            if (end <= time)
            {
                throw new ArgumentException("Elements must be uninterrupted and in time order.");
            }

            var leftInterval = leftSegment?.Cut(time, end) ?? Segment.Constant(time, end, undefinedValue);
            var rightInterval = rightSegment?.Cut(time, end) ?? Segment.Constant(time, end, undefinedValue);
            // The segment envelope may introduce an intersection inside this interval, before the next input boundary.
            var intervalEnvelope = kind == EnvelopeKind.Upper
                ? Segment.Maximum(leftInterval, rightInterval)
                : Segment.Minimum(leftInterval, rightInterval);
            foreach (var element in intervalEnvelope)
            {
                yield return element;
            }
            time = end;
        }
    }
}
