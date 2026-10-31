using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Provides LINQ extensions methods for <see cref="Sequence"/> and <see cref="Element"/>.
/// </summary>
public static partial class SequenceExtensions
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    #region Conversion

    /// <summary>
    /// Builds a <see cref="Sequence"/> object from the set of elements. 
    /// </summary>
    /// <param name="elements">Set of elements composing the sequence. Must be in uninterrupted order.</param>
    public static Sequence ToSequence(this IEnumerable<Element> elements)
        => new Sequence(elements);

    /// <summary>
    /// Builds a <see cref="Sequence"/> object from the set of elements. 
    /// Fills the gaps within [fillFrom, fillTo[ with $+\infty$.
    /// </summary>
    /// <param name="elements">Partial set of elements composing the sequence. Must be ordered, but can have gaps.</param>
    /// <param name="fillFrom">Left inclusive endpoint of the filling interval.</param>
    /// <param name="fillTo">Right exclusive endpoint of the filling interval.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    public static Sequence ToSequence(this IEnumerable<Element> elements, Rational fillFrom, Rational fillTo, Rational? fillWith = null)
        => new Sequence(elements, fillFrom, fillTo, fillWith);

    #endregion

    #region Cutting and filling

    /// <summary>
    /// Returns a cut of the sequence for a smaller support.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the new support.</param>
    /// <param name="cutEnd">Right endpoint of the new support.</param>
    /// <param name="isStartIncluded">If true, the support is left-closed.</param>
    /// <param name="isEndIncluded">If true, the support is right-closed.</param>
    /// <exception cref="ArgumentException">Thrown if the new support is not a subset of the current one.</exception>
    /// <remarks>Optimized for minimal allocations</remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Cut(
        this IEnumerable<Element> elements,
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        using var enumerator = new ElementCursor(elements);
        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        if (cutStart > cutEnd)
            throw new ArgumentException("Invalid interval.");

        if(cutStart < enumerator.Current.StartTime)
            throw new ArgumentException("Cut limits are out of the sequence support.");
        if (isStartIncluded && enumerator.Current is Segment && enumerator.Current.StartTime == cutStart)
            throw new ArgumentException("Cut includes endpoints that sequence does not.");

        if (cutStart == cutEnd)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Cut endpoints, if equal, must be both inclusive.");

            while (!enumerator.Current.IsDefinedFor(cutStart))
            {
                if (!enumerator.MoveNext())
                    throw new ArgumentException("Cut includes endpoints that sequence does not.");
            }

            Point p;
            if (enumerator.Current is Point p2)
                p = p2;
            else
                p = (enumerator.Current as Segment)!.Sample(cutStart);
            yield return p;
            yield break;
        }

        // Enumerate until cut starts
        bool IsBeforeStart(Element element)
        {
            switch (element)
            {
                case Point p:
                    return isStartIncluded ? p.Time < cutStart : p.Time <= cutStart;
                case Segment s:
                    return s.EndTime <= cutStart;
                default:
                    throw new InvalidCastException();
            }
        }

        while (IsBeforeStart(enumerator.Current))
        {
            if (!enumerator.MoveNext())
                throw new ArgumentException("Cut includes endpoints that sequence does not.");
        }

        Segment left;

        // first elements
        switch (enumerator.Current)
        {
            case Point p:
            {
                yield return p;
                if (!enumerator.MoveNext())
                    throw new ArgumentException("Collection ended after first point.");
                left = (Segment) enumerator.Current;
                break;
            }

            case Segment s:
            {
                if (s.StartTime < cutStart)
                {
                    var (_, p, r) = s.Split(cutStart);
                    if(isStartIncluded)
                        yield return p;
                    left = r;
                }
                else
                {
                    left = s;
                }
                break;
            }

            default:
                throw new InvalidCastException();
        }

        // At the start of each iteration, left is the current segment
        // first we check if it's the last
        // otherwise, we fetch a point and a segment, check for merging, and restore the initial condition
        while (true)
        {
            if (left.StartTime == cutEnd)
                yield break;
            else if (left.EndTime > cutEnd)
            {
                var (l, p, _) = left.Split(cutEnd);
                yield return l;
                if (isEndIncluded)
                    yield return p;
                yield break;
            }
            else if (left.EndTime == cutEnd)
            {
                yield return left;
                if (isEndIncluded)
                {
                    if (!enumerator.MoveNext())
                        throw new ArgumentException("Cut includes endpoints that sequence does not.");
                    yield return (Point)enumerator.Current;
                }
                yield break;
            }

            if (!enumerator.MoveNext())
                throw new ArgumentException("Cut limits are out of the sequence support.");

            var center = (Point)enumerator.Current;
            if (!enumerator.MoveNext())
                throw new ArgumentException("Cut limits are out of the sequence support.");
            var right = (Segment)enumerator.Current;

            // check for merging
            if (left.Slope == right.Slope && left.LeftLimitAtEndTime == center.Value && center.Value == right.RightLimitAtStartTime)
            {
                left = new Segment(
                    startTime: left.StartTime,
                    endTime: right.EndTime,
                    rightLimitAtStartTime: left.RightLimitAtStartTime,
                    slope: left.Slope
                );
            }
            else
            {
                yield return left;
                yield return center;
                left = right;
            }
        }
    }

    /// <summary>
    /// Fills the gaps of the set of elements within <paramref name="interval"/> with the given value, defaults to $+\infty$.
    /// </summary>
    /// <param name="elements">The set of elements, in time order and with non-overlapping supports.</param>
    /// <param name="interval">The filling interval.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    /// <returns>The elements, with gaps within the filling interval filled with constant elements of value <paramref name="fillWith"/>.</returns>
    /// <remarks>Elements outside the filling interval are kept unchanged. If the filling interval is empty, the elements are returned unchanged.</remarks>
    /// <exception cref="ArgumentException">During enumeration, if the elements are out of order or have overlapping supports.</exception>
    public static IEnumerable<Element> Fill(this IEnumerable<Element> elements, Interval interval, Rational? fillWith = null)
        => elements.Fill(interval.Lower, interval.Upper, interval.IsLowerIncluded, interval.IsUpperIncluded, fillWith);

    /// <summary>
    /// Fills the gaps of the set of elements within <paramref name="fillFrom"/> and <paramref name="fillTo"/> with the given value, defaults to $+\infty$.
    /// </summary>
    /// <param name="elements">The set of elements, in time order and with non-overlapping supports.</param>
    /// <param name="fillFrom">Left endpoint of the filling interval.</param>
    /// <param name="fillTo">Right endpoint of the filling interval.</param>
    /// <param name="isFromIncluded">If true, left endpoint is inclusive.</param>
    /// <param name="isToIncluded">If true, right endpoint is inclusive.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    /// <returns>The elements, with gaps within the filling interval filled with constant elements of value <paramref name="fillWith"/>.</returns>
    /// <remarks>Elements outside the filling interval are kept unchanged. If the filling interval is empty, the elements are returned unchanged.</remarks>
    /// <exception cref="ArgumentException">If the filling interval is not valid.</exception>
    /// <exception cref="ArgumentException">During enumeration, if the elements are out of order or have overlapping supports.</exception>
    public static IEnumerable<Element> Fill(
        this IEnumerable<Element> elements,
        Rational fillFrom,
        Rational fillTo,
        bool isFromIncluded = true,
        bool isToIncluded = false,
        Rational? fillWith = null
    )
    {
        if (fillFrom > fillTo)
            throw new ArgumentException("The filling interval is reversed.", nameof(fillTo));
        return Filled();

        IEnumerable<Element> Filled()
        {
            var value = fillWith ?? Rational.PlusInfinity;
            using var enumerator = elements.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                if (fillFrom == fillTo && !(isFromIncluded && isToIncluded))
                    yield break;
                if (isFromIncluded)
                    yield return new Point(fillFrom, value);
                if (fillFrom < fillTo)
                {
                    yield return Segment.Constant(fillFrom, fillTo, value);
                    if (isToIncluded)
                        yield return new Point(fillTo, value);
                }
                yield break;
            }

            var current = enumerator.Current;
            foreach (var filler in FillGap(fillFrom, current.StartTime, isFromIncluded, current is Segment))
                yield return filler;
            yield return current;

            while (enumerator.MoveNext())
            {
                var next = enumerator.Current;
                if (next.StartTime < current.EndTime ||
                    (next.StartTime == current.EndTime && current is Point && next is Point))
                    throw new ArgumentException("Elements must be ordered and have non-overlapping supports.", nameof(elements));

                foreach (var filler in FillGap(current.EndTime, next.StartTime, current is Segment, next is Segment))
                    yield return filler;
                yield return next;
                current = next;
            }

            foreach (var filler in FillGap(current.EndTime, fillTo, current is Segment, isToIncluded))
                yield return filler;

            // Intersect each missing interval with the requested filling interval.
            // Its endpoint flags distinguish missing boundary points from values supplied by the input.
            IEnumerable<Element> FillGap(Rational start, Rational end, bool includeStart, bool includeEnd)
            {
                var from = Rational.Max(start, fillFrom);
                var to = Rational.Min(end, fillTo);
                var leftIncluded = (from > start || includeStart) && (from > fillFrom || isFromIncluded);
                var rightIncluded = (to < end || includeEnd) && (to < fillTo || isToIncluded);
                if (from > to)
                    yield break;
                if (from == to)
                {
                    if (leftIncluded && rightIncluded)
                        yield return new Point(from, value);
                    yield break;
                }
                if (leftIncluded)
                    yield return new Point(from, value);
                yield return Segment.Constant(from, to, value);
                if (rightIncluded)
                    yield return new Point(to, value);
            }
        }
    }

    #endregion

    #region Merging

    /// <summary>
    /// Applies merge, whenever possible, to a set of elements.
    /// The result is the minimal set, in number of elements, to represent the same information.
    /// </summary>
    /// <param name="elements">Ordered, non-overlapping elements; gaps and missing boundary points are allowed.</param>
    /// <param name="doSort">If true, the elements are sorted before attempting to merge.</param>
    /// <param name="settings">Settings to forward to SortElements, if used.</param>
    /// <returns>A set where no further merges are possible.</returns>
    /// <remarks>
    /// Compatible adjoining segment–point–segment triples are merged; gaps and missing joining points are preserved.
    /// </remarks>
    /// <exception cref="ArgumentException">If input is empty, overlaps or is out of order.</exception>
    public static List<Element> Merge(this IReadOnlyList<Element> elements, bool doSort = false, ComputationSettings? settings = null)
    {
        var ordered = doSort ? elements.SortElements(settings) : elements;
        return ordered.MergeAsEnumerable()
            .ToList();
    }

    /// <summary>
    /// Merges the sequence segment-point-segment, in the provided order.
    /// Throws if the sequence cannot be merged.
    /// See <see cref="CanMergeTriplet"/> for merge conditions.
    /// </summary>
    /// <returns>The segment resulting from the merge.</returns>
    internal static Segment MergeTriplet(Segment left, Point point, Segment right)
    {
        if(!CanMergeTriplet(left, point, right))
            throw new ArgumentException("Segments cannot be merged");

        return new Segment(
            startTime: left.StartTime,
            endTime: right.EndTime,
            rightLimitAtStartTime: left.RightLimitAtStartTime,
            slope: left.Slope
        );
    }

    /// <summary>
    /// Tests if the sequence segment-point-segment, in the provided order, can be merged.
    /// This is true only if their graphs align.
    /// </summary>
    internal static bool CanMergeTriplet(Segment left, Point point, Segment right)
    {
        return 
            left.EndTime == point.Time &&
            point.Time == right.StartTime &&
            left.LeftLimitAtEndTime == point.Value &&
            point.Value == right.RightLimitAtStartTime &&
            left.Slope == right.Slope;
    }

    /// <summary>
    /// Applies merge, whenever possible, to a set of elements.
    /// The result is the minimal set, in number of elements, to represent the same information.
    /// </summary>
    /// <param name="elements">Ordered, non-overlapping elements; gaps and missing boundary points are allowed.</param>
    /// <returns>A set where no further merges are possible.</returns>
    /// <remarks>
    /// Compatible adjoining segment–point–segment triples are merged; gaps and missing joining points are preserved.
    /// Optimized for minimal allocations.
    /// </remarks>
    /// <exception cref="ArgumentException">If input is empty, overlaps or is out of order.</exception>
    public static IEnumerable<Element> MergeAsEnumerable(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var previous = enumerator.Current;
        var pending = previous;

        while (true)
        {
            if (pending is Point)
            {
                yield return pending;
                if (!ReadNext())
                    yield break;
                pending = enumerator.Current;
                continue;
            }

            var left = (Segment) pending;
            if (!ReadNext())
            {
                yield return left;
                yield break;
            }

            if (enumerator.Current is not Point center || center.Time != left.EndTime)
            {
                yield return left;
                pending = enumerator.Current;
                continue;
            }

            if (!ReadNext())
            {
                yield return left;
                yield return center;
                yield break;
            }

            if (enumerator.Current is Segment right && CanMergeTriplet(left, center, right))
            {
                pending = MergeTriplet(left, center, right);
            }
            else
            {
                yield return left;
                yield return center;
                pending = enumerator.Current;
            }
        }

        bool ReadNext()
        {
            if (!enumerator.MoveNext())
                return false;
            var next = enumerator.Current;
            if (next.StartTime < previous.EndTime ||
                (next.StartTime == previous.EndTime && previous is Point && next is Point))
                throw new ArgumentException("Elements must be ordered and have non-overlapping supports.", nameof(elements));
            previous = next;
            return true;
        }
    }

    #endregion

    #region Pointwise operators

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
    /// Computes the pointwise minimum of two streams over the intersection of their supports.
    /// </summary>
    /// <param name="a">The first operand, nonempty, uninterrupted and in time order.</param>
    /// <param name="b">The second operand, nonempty, uninterrupted and in time order.</param>
    /// <returns>The elements of the minimum in time order.</returns>
    /// <remarks>
    /// Within the intersection, the result has a point at each breakpoint of either operand and at each segment crossing, with segments between consecutive points.
    /// Use <see cref="MergeAsEnumerable"/> to merge adjacent compatible elements.
    /// </remarks>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time, when enumeration begins.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Minimum(this IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
            foreach (var element in Element.Minimum(left, right))
                yield return element;
    }

    /// <summary>
    /// Computes the pointwise maximum of two streams over the intersection of their supports.
    /// </summary>
    /// <param name="a">The first operand, nonempty, uninterrupted and in time order.</param>
    /// <param name="b">The second operand, nonempty, uninterrupted and in time order.</param>
    /// <returns>The elements of the maximum in time order.</returns>
    /// <remarks>
    /// Within the intersection, the result has a point at each breakpoint of either operand and at each segment crossing, with segments between consecutive points.
    /// Use <see cref="MergeAsEnumerable"/> to merge adjacent compatible elements.
    /// </remarks>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time, when enumeration begins.</exception>
    /// <exception cref="ArgumentException">If a consumed element violates uninterrupted time order.</exception>
    public static IEnumerable<Element> Maximum(this IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
            foreach (var element in Element.Maximum(left, right))
                yield return element;
    }

    #endregion

    #region Comparisons

    /// <summary>
    /// Checks whether the first nonempty, uninterrupted stream bounds the second from below over their overlap.
    /// </summary>
    /// <param name="a">The first stream in time order.</param>
    /// <param name="b">The second stream in time order.</param>
    /// <returns>Whether the bound holds at every shared point and throughout every shared segment.</returns>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time.</exception>
    internal static bool LessOrEqualOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                if (point.Value > ((Point) right).Value)
                    return false;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA > startB || segmentA.Slope > segmentB.Slope)
                        return false;
                }
                else if (startA > startB || segmentA.LeftLimitAt(end) > segmentB.LeftLimitAt(end))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Finds the first time at or immediately after which two uninterrupted streams differ over their overlap.
    /// </summary>
    /// <param name="a">The first nonempty stream in time order.</param>
    /// <param name="b">The second nonempty stream in time order.</param>
    /// <returns>The first differing time, or null when the streams agree throughout their overlap.</returns>
    internal static Rational? FirstInequivalenceOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                if (point.Value != ((Point) right).Value)
                    return point.Time;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA != startB || segmentA.Slope != segmentB.Slope)
                        return start;
                }
                else if (startA != startB || segmentA.LeftLimitAt(end) != segmentB.LeftLimitAt(end))
                    return start;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks a strict pointwise bound over the overlap of finite uninterrupted streams.
    /// </summary>
    /// <param name="a">The first nonempty stream in time order.</param>
    /// <param name="b">The second nonempty stream in time order.</param>
    /// <returns>Whether the first stream is strictly below the second at every shared time.</returns>
    internal static bool StrictlyLessOnOverlap(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        foreach (var (left, right) in OverlappingElements(a, b))
        {
            if (left is Point point)
            {
                if (point.Value >= ((Point) right).Value)
                    return false;
            }
            else
            {
                var segmentA = (Segment) left;
                var segmentB = (Segment) right;
                var start = Rational.Max(segmentA.StartTime, segmentB.StartTime);
                var end = Rational.Min(segmentA.EndTime, segmentB.EndTime);
                var startA = segmentA.RightLimitAt(start);
                var startB = segmentB.RightLimitAt(start);
                if (end.IsPlusInfinite && startA.IsFinite && startB.IsFinite)
                {
                    // Equal limits at infinity can hide different slopes.
                    if (startA > startB || segmentA.Slope > segmentB.Slope
                        || (startA == startB && segmentA.Slope == segmentB.Slope))
                        return false;
                }
                else
                {
                    var endA = segmentA.LeftLimitAt(end);
                    var endB = segmentB.LeftLimitAt(end);
                    if (startA > startB || endA > endB || (startA == startB && endA == endB))
                        return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region Overlap traversal

    /// <summary>
    /// Pairs points and overlapping segments of two nonempty, uninterrupted streams in time order.
    /// </summary>
    /// <param name="a">The first stream.</param>
    /// <param name="b">The second stream.</param>
    /// <returns>Point pairs at shared times and segment pairs covering each shared open interval.</returns>
    /// <exception cref="ArgumentException">If an operand is empty or the streams share no time.</exception>
    private static IEnumerable<(Element left, Element right)> OverlappingElements(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        using var cursorA = new ElementCursor(a);
        using var cursorB = new ElementCursor(b);
        if (!cursorA.MoveNext() || !cursorB.MoveNext())
            throw new ArgumentException("An operand is empty.");

        var t = Rational.Max(cursorA.Current.StartTime, cursorB.Current.StartTime);
        // The overlap includes its start unless an operand that starts there is open.
        var isPointDefined = (cursorA.Current is Point || cursorA.Current.StartTime < t)
            && (cursorB.Current is Point || cursorB.Current.StartTime < t);

        var hasYielded = false;
        while (true)
        {
            if (isPointDefined)
            {
                if (!cursorA.TryPointAt(t, out var pointA) || !cursorB.TryPointAt(t, out var pointB))
                    break;
                yield return (pointA, pointB);
                hasYielded = true;
            }

            if (!cursorA.TrySegmentAfter(t, out var segmentA) || !cursorB.TrySegmentAfter(t, out var segmentB))
                break;
            yield return (segmentA, segmentB);
            hasYielded = true;

            t = Rational.Min(segmentA.EndTime, segmentB.EndTime);
            isPointDefined = true;
        }

        if (!hasYielded)
            throw new ArgumentException("The streams share no time.");
    }

    #endregion

    #region Element lookup

    /// <summary>
    /// Returns the <see cref="Element"/> that describes the sequence in <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Element"/> describing the sequence at <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Element GetElementAt(this IEnumerable<Element> elements, Rational time)
    {
        using var cursor = new ElementCursor(elements);
        while (cursor.MoveNext())
        {
            if (cursor.Current.IsDefinedFor(time))
                return cursor.Current;
            if (cursor.Current.StartTime > time)
                break;
        }
        throw new ArgumentException("The given time is out of sequence support.", nameof(time));
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence before <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence before <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Segment GetSegmentBefore(this IEnumerable<Element> elements, Rational time)
    {
        using var cursor = new ElementCursor(elements);
        Segment? found = null;
        while (cursor.MoveNext())
        {
            if (cursor.Current is Segment segment && segment.EndTime <= time)
                found = segment;
        }
        return found ?? throw new ArgumentException("Sequence is not defined before the given time.", nameof(time));
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence after <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence after <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Segment GetSegmentAfter(this IEnumerable<Element> elements, Rational time)
    {
        using var cursor = new ElementCursor(elements);
        while (cursor.MoveNext())
        {
            if (cursor.Current.StartTime > time)
                break;
            if (cursor.Current is Segment segment && segment.EndTime > time)
                return segment;
        }
        throw new ArgumentException("Sequence is not defined after the given time.", nameof(time));
    }

    #endregion

    #region Ordering and sequence validation

    /// <summary>
    /// Checks if time order is respected, i.e. they are ordered first by start, then by end.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="AreInTimeOrder(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeOrder(this IEnumerable<Element> elements)
    {
        if (TryGetSpan(elements, out var span))
            return AreInTimeOrder(span);
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var next = enumerator.Current;
            if (next.StartTime < previous.StartTime ||
                (next.StartTime == previous.StartTime && next.EndTime < previous.EndTime))
                return false;
            previous = next;
        }
        return true;
    }

    /// <summary>
    /// Checks if time order is respected, i.e. they are ordered first by start, then by end.
    /// </summary>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeOrder(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var previous = elements[0];
        for (var i = 1; i < elements.Length; i++)
        {
            var next = elements[i];
            if (next.StartTime < previous.StartTime ||
                (next.StartTime == previous.StartTime && next.EndTime < previous.EndTime))
                return false;
            previous = next;
        }
        return true;
    }

    /// <summary>
    /// Checks if time order is respected, i.e. they are ordered first by start, then by end.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeOrder(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return AreInTimeOrder((ReadOnlySpan<Element>) elements);
    }

    /// <summary>
    /// Checks if the elements form a sequence.
    /// In addition to <see cref="AreInTimeOrder(IEnumerable{Element})"/> it requires non-overlapping supports, including no duplicate points, but allows gaps.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="AreInTimeSequence(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeSequence(this IEnumerable<Element> elements)
    {
        if (TryGetSpan(elements, out var span))
            return AreInTimeSequence(span);
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var previous = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var next = enumerator.Current;
            if (next.StartTime < previous.EndTime ||
                (next.StartTime == previous.EndTime && previous is Point && next is Point))
                return false;
            previous = next;
        }
        return true;
    }

    /// <summary>
    /// Checks if the elements form a sequence.
    /// In addition to <see cref="AreInTimeOrder(ReadOnlySpan{Element})"/> it requires non-overlapping supports, including no duplicate points, but allows gaps.
    /// </summary>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeSequence(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var previous = elements[0];
        for (var i = 1; i < elements.Length; i++)
        {
            var next = elements[i];
            if (next.StartTime < previous.EndTime ||
                (next.StartTime == previous.EndTime && previous is Point && next is Point))
                return false;
            previous = next;
        }
        return true;
    }

    /// <summary>
    /// Checks if the elements form a sequence.
    /// In addition to <see cref="AreInTimeOrder(Element[])"/> it requires non-overlapping supports, including no duplicate points, but allows gaps.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreInTimeSequence(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return AreInTimeSequence((ReadOnlySpan<Element>) elements);
    }

    /// <summary>
    /// Checks if the elements form a uninterrupted sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="AreUninterruptedSequence(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreUninterruptedSequence(this IEnumerable<Element> elements)
    {
        if (TryGetSpan(elements, out var span))
            return AreUninterruptedSequence(span);
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));

        Rational lastDefinedTime = enumerator.Current.EndTime;
        bool lastElementWasPoint = enumerator.Current is Point;

        while (enumerator.MoveNext())
        {
            var element = enumerator.Current;
            //Good sequences are point-segment and segment-point. So the two checks are good if they are equal to each other
            if (lastElementWasPoint != element is Segment)
                return false;

            if (element.StartTime != lastDefinedTime)
                return false;

            lastDefinedTime = element.EndTime;
            lastElementWasPoint = element is Point;
        }

        return true;
    }

    /// <summary>
    /// Checks if the elements form a uninterrupted sequence.
    /// </summary>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreUninterruptedSequence(this ReadOnlySpan<Element> elements)
    {
        if (elements.IsEmpty)
            throw new ArgumentException("The element stream is empty.", nameof(elements));

        Rational lastDefinedTime = elements[0].EndTime;
        bool lastElementWasPoint = elements[0] is Point;

        for (var i = 1; i < elements.Length; i++)
        {
            var element = elements[i];
            //Good sequences are point-segment and segment-point. So the two checks are good if they are equal to each other
            if (lastElementWasPoint != element is Segment)
                return false;

            if (element.StartTime != lastDefinedTime)
                return false;

            lastDefinedTime = element.EndTime;
            lastElementWasPoint = element is Point;
        }

        return true;
    }

    /// <summary>
    /// Checks if the elements form a uninterrupted sequence.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static bool AreUninterruptedSequence(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return AreUninterruptedSequence((ReadOnlySpan<Element>) elements);
    }

    /// <summary>
    /// Exposes the storage of an array, a list or the read-only view of a sequence's array as a span, without copying.
    /// </summary>
    /// <remarks>The span views the caller's storage, which must not change while the span is in use.</remarks>
    /// <returns>False for other collections and for null.</returns>
    internal static bool TryGetSpan(IEnumerable<Element> elements, out ReadOnlySpan<Element> span)
    {
        switch (elements)
        {
            case Element[] array:
                span = array;
                return true;
            case List<Element> list:
                span = CollectionsMarshal.AsSpan(list);
                return true;
            case ReadOnlyElementList view:
                span = view.AsSpan();
                return true;
            default:
                span = default;
                return false;
        }
    }

    /// <summary>
    /// Sorts the elements in time order.
    /// </summary>
    /// <exception cref="ArgumentException">If input is empty.</exception>
    public static IReadOnlyList<Element> SortElements(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
        if (elements.Count == 0)
            throw new ArgumentException("The element collection is empty.", nameof(elements));
        settings ??= ComputationSettings.Default();
        const int ParallelizationThreshold = 10_000;

        #if DO_LOG
        var sortStopwatch = Stopwatch.StartNew();
        #endif

        if (elements.AreInTimeOrder())
        {
            #if DO_LOG
            sortStopwatch.Stop();
            logger.Trace($"SortElements: took {sortStopwatch.Elapsed}, already sorted");
            #endif
            return elements;
        }
        else
        {
            var doParallel = settings.UseParallelSortElements && elements.Count >= ParallelizationThreshold;
            List<Element> result;
            if (doParallel)
            {
                result = elements
                    .AsParallel()
                    .OrderBy(element => element.StartTime)
                    .ThenBy(element => element.EndTime)
                    .ToList();
            }
            else
            {
                result = elements
                    .OrderBy(element => element.StartTime)
                    .ThenBy(element => element.EndTime)
                    .ToList();
            }

            #if DO_LOG
            sortStopwatch.Stop();
            #endif
            var alg = doParallel ? "parallel" : "serial";
            #if DO_LOG
            logger.Trace($"SortElements: took {sortStopwatch.Elapsed}, {alg} sort");
            #endif
            return result;
        }
    }

    #endregion

    #region Breakpoints and boundary values

    /// <summary>
    /// Enumerates a <see cref="Sequence"/> as a series of breakpoints.
    /// </summary>
    /// <param name="sequence">The sequence to process.</param>
    /// <remarks>Does not attempt merging.</remarks>
    public static IEnumerable<(Segment? left, Point center, Segment? right)> EnumerateBreakpoints(this Sequence sequence)
        => sequence.Elements.EnumerateBreakpoints();

    /// <summary>
    /// Enumerates a set of <see cref="Element"/>s as a series of breakpoints.
    /// </summary>
    /// <remarks>Does not attempt merging.</remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static IEnumerable<(Segment? left, Point center, Segment? right)> EnumerateBreakpoints(this IEnumerable<Element> elements)
    {
        using var enumerator = new ElementCursor(elements);
        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        Segment? left = null;
        Point center;
        Segment? right = null;

        // At the start of each iteration, left is the current segment (or null if we are at the start)
        // we fetch expecting the new breakpoint (possibly a segment, if we are at the start)
        // we then try fetching another segment, return the triplet, and restore the initial condition
        while (true)
        {
            if (enumerator.Current is Segment)
            {
                if (left == null)
                {
                    left = (Segment)enumerator.Current;
                    if (!enumerator.MoveNext())
                    {
                        // the sequence has no points
                        yield break;
                    }
                    else
                    {
                        center = (Point)enumerator.Current;
                    }
                }
                else
                    throw new ArgumentException("Invalid sequence of elements");
            }
            else
            {
                center = (Point)enumerator.Current;
            }

            if (!enumerator.MoveNext())
            {
                // the sequence ends with a point
                yield return (left, center, null);
                yield break;
            }
            else
            {
                right = (Segment)enumerator.Current;
                yield return (left, center, right);
                if(!enumerator.MoveNext())
                {
                    // the sequence ends with a segment
                    yield break;
                }
                else
                {
                    left = right;
                }
            }
        }
    }

    /// <summary>
    /// Enumerates the left-limit, the value and right-limit at the breakpoint.
    /// </summary>
    public static IEnumerable<Rational> GetBreakpointBoundaryValues(
        this (Segment? left, Point center, Segment? right) breakpoint
    )
    {
        var (left, center, right) = breakpoint;
        if (left is not null)
            yield return left.LeftLimitAtEndTime;
        yield return center.Value;
        if (right is not null)
            yield return right.RightLimitAtStartTime;
    }

    /// <summary>
    /// Enumerates, for each breakpoint, the left-limit, the value and right-limit at the breakpoint.
    /// </summary>
    public static IEnumerable<Rational> GetBreakpointsBoundaryValues(
        this IEnumerable<(Segment? left, Point center, Segment? right)> breakpoints)
    {
        return breakpoints
            .SelectMany(GetBreakpointBoundaryValues);
    }

    /// <summary>
    /// Enumerates for each point its value, and for each segment its right-limit at start and left-limit at end.
    /// </summary>
    public static IEnumerable<Rational> GetElementsBoundaryValues(this IEnumerable<Element> elements)
    {
        foreach (var element in elements)
        {
            if (element is Segment segment)
            {
                yield return segment.RightLimitAtStartTime;
                yield return segment.LeftLimitAtEndTime;
            }
            else if (element is Point point)
            {
                yield return point.Value;
            }
        }
    }

    /// <summary>
    /// Enumerates for each point its time and value, and for each segment 1) its start time and right-limit at start, 2) its end time and left-limit at end.
    /// </summary>
    /// <remarks>
    /// The returned pairs are not points since, particularly for the pairs coming from segments,
    /// it is not given that the value is attained at that time.
    /// </remarks>
    public static IEnumerable<(Rational time, Rational value)> GetElementsBoundaryPairs(this IEnumerable<Element> elements)
    {
        foreach (var element in elements)
        {
            if (element is Segment segment)
            {
                yield return (segment.StartTime, segment.RightLimitAtStartTime);
                yield return (segment.EndTime, segment.LeftLimitAtEndTime);
            }
            else if (element is Point point)
            {
                yield return (point.Time, point.Value);
            }
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsContinuous(this IEnumerable<Element> elements)
        => IsContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsContinuous(this ReadOnlySpan<Element> elements)
        => IsContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if(!reader.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        Rational lastValue = reader.Current is Point p ?
            p.Value :
            ((Segment)reader.Current).LeftLimitAtEndTime;

        while (reader.MoveNext())
        {
            switch(reader.Current)
            {
                case Point point:
                    if (point.Value == lastValue)
                        break;
                    else
                        return false;

                case Segment segment:
                    if (segment.RightLimitAtStartTime == lastValue)
                    {
                        lastValue = segment.LeftLimitAtEndTime;
                        break;
                    }
                    else
                        return false;

                default:
                    throw new InvalidCastException();
            }
        }

        return true;
    }

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsLeftContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this IEnumerable<Element> elements)
        => IsLeftContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this ReadOnlySpan<Element> elements)
        => IsLeftContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no left-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsLeftContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsLeftContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsLeftContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        Segment? previous = null;
        do
        {
            if (reader.Current is Point point && previous is { } left &&
                left.LeftLimitAtEndTime != point.Value)
                return false;
            previous = reader.Current as Segment;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsRightContinuous(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this IEnumerable<Element> elements)
        => IsRightContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this ReadOnlySpan<Element> elements)
        => IsRightContinuous(new ElementReader(elements));

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    /// <returns>Whether the represented function has the stated property.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsRightContinuous(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsRightContinuous((ReadOnlySpan<Element>) elements);
    }

    private static bool IsRightContinuous(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        Point? previous = null;
        do
        {
            if (reader.Current is Segment segment && previous is { } left &&
                left.Value != segment.RightLimitAtStartTime)
                return false;
            previous = reader.Current as Point;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is zero.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsZero(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsZero(this IEnumerable<Element> elements)
        => IsZero(new ElementReader(elements));

    /// <summary>True if every defined value is zero.</summary>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsZero(this ReadOnlySpan<Element> elements)
        => IsZero(new ElementReader(elements));

    /// <summary>True if every defined value is zero.</summary>
    /// <returns>True if every element is zero.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsZero(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsZero((ReadOnlySpan<Element>) elements);
    }

    private static bool IsZero(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsZero)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsInfinite(this IEnumerable<Element> elements)
        => IsInfinite(new ElementReader(elements));

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsInfinite(this ReadOnlySpan<Element> elements)
        => IsInfinite(new ElementReader(elements));

    /// <summary>True if every defined value is infinite, with either sign.</summary>
    /// <returns>True if every element is infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="HasPlusInfinity(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this IEnumerable<Element> elements)
        => HasPlusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this ReadOnlySpan<Element> elements)
        => HasPlusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is plus infinity.</summary>
    /// <returns>True if an element is plus infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasPlusInfinity(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return HasPlusInfinity((ReadOnlySpan<Element>) elements);
    }

    private static bool HasPlusInfinity(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (reader.Current.IsPlusInfinite)
                return true;
        } while (reader.MoveNext());
        return false;
    }

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="HasMinusInfinity(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this IEnumerable<Element> elements)
        => HasMinusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this ReadOnlySpan<Element> elements)
        => HasMinusInfinity(new ElementReader(elements));

    /// <summary>True if some defined value is minus infinity.</summary>
    /// <returns>True if an element is minus infinite.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool HasMinusInfinity(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return HasMinusInfinity((ReadOnlySpan<Element>) elements);
    }

    private static bool HasMinusInfinity(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (reader.Current.IsMinusInfinite)
                return true;
        } while (reader.MoveNext());
        return false;
    }

    /// <summary>True if every defined value is non-positive.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonPositive(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this IEnumerable<Element> elements)
        => IsNonPositive(new ElementReader(elements));

    /// <summary>True if every defined value is non-positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this ReadOnlySpan<Element> elements)
        => IsNonPositive(new ElementReader(elements));

    /// <summary>True if every defined value is non-positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonPositive(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsNonPositive((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonPositive(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value <= 0,
                Segment segment => segment.RightLimitAtStartTime <= 0 && segment.LeftLimitAtEndTime <= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>
    /// Segment endpoints are excluded; their limits need not satisfy a strict inequality.
    /// Arrays and lists are read as spans, through <see cref="IsPositive(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsPositive(this IEnumerable<Element> elements)
        => IsPositive(new ElementReader(elements));

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPositive(this ReadOnlySpan<Element> elements)
        => IsPositive(new ElementReader(elements));

    /// <summary>True if every defined value is strictly positive.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPositive(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsPositive((ReadOnlySpan<Element>) elements);
    }

    private static bool IsPositive(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value > 0,
                Segment segment => segment.IsConstant
                    ? segment.RightLimitAtStartTime > 0
                    : segment.RightLimitAtStartTime >= 0 && segment.LeftLimitAtEndTime >= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>
    /// Segment endpoints are excluded; their limits need not satisfy a strict inequality.
    /// Arrays and lists are read as spans, through <see cref="IsNegative(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNegative(this IEnumerable<Element> elements)
        => IsNegative(new ElementReader(elements));

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNegative(this ReadOnlySpan<Element> elements)
        => IsNegative(new ElementReader(elements));

    /// <summary>True if every defined value is strictly negative.</summary>
    /// <returns>Whether the sign condition holds, including infinite values in the extended order.</returns>
    /// <remarks>Segment endpoints are excluded; their limits need not satisfy a strict inequality.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNegative(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsNegative((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNegative(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            var matches = element switch
            {
                Point point => point.Value < 0,
                Segment segment => segment.IsConstant
                    ? segment.RightLimitAtStartTime < 0
                    : segment.RightLimitAtStartTime <= 0 && segment.LeftLimitAtEndTime <= 0,
                _ => throw new InvalidCastException()
            };
            if (!matches)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsConstant(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsConstant(this IEnumerable<Element> elements)
        => IsConstant(new ElementReader(elements));

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsConstant(this ReadOnlySpan<Element> elements)
        => IsConstant(new ElementReader(elements));

    /// <summary>True if all defined values equal one constant, possibly infinite.</summary>
    /// <returns>Whether every element has the same constant value.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsConstant(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsConstant((ReadOnlySpan<Element>) elements);
    }

    private static bool IsConstant(ElementReader elements)
    {
        elements.ThrowIfStarted();
        Rational? value = null;
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            if (element is Segment { IsConstant: false })
                return false;
            var current = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (value is { } previous && previous != current)
                return false;
            value = current;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// A stream containing one point with a finite value is affine.
    /// Enumeration stops when the result is false; remaining elements are not read or validated.
    /// Arrays and lists are read as spans, through <see cref="IsAffine(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsAffine(this IEnumerable<Element> elements)
        => IsAffine(new ElementReader(elements));

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// A span containing one point with a finite value is affine.
    /// Reading stops when the result is false; remaining elements are not read or validated.
    /// </remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsAffine(this ReadOnlySpan<Element> elements)
        => IsAffine(new ElementReader(elements));

    /// <summary>True if finite coefficients $a$ and $b$ exist such that $f(t)=at+b$ throughout the support.</summary>
    /// <returns>Whether the represented function is affine.</returns>
    /// <remarks>
    /// An array containing one point with a finite value is affine.
    /// Reading stops when the result is false; remaining elements are not read or validated.
    /// </remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsAffine(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsAffine((ReadOnlySpan<Element>) elements);
    }

    private static bool IsAffine(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        Rational? previousEndValue = null;
        Rational? slope = null;
        while (reader.MoveNext())
        {
            var element = reader.Current;
            if (!element.IsFinite)
                return false;

            var startValue = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (previousEndValue is { } previous && previous != startValue)
                return false;

            if (element is Segment segment)
            {
                if (slope is { } previousSlope && previousSlope != segment.Slope)
                    return false;

                slope = segment.Slope;
                previousEndValue = segment.LeftLimitAtEndTime;
            }
            else
            {
                previousEndValue = startValue;
            }
        }
        if (previousEndValue is null)
            throw new ArgumentException("Elements is an empty collection");
        return true;
    }

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonNegative(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this IEnumerable<Element> elements)
        => IsNonNegative(new ElementReader(elements));

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this ReadOnlySpan<Element> elements)
        => IsNonNegative(new ElementReader(elements));

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    /// <returns>Whether every point and segment value is non-negative.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonNegative(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsNonNegative((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonNegative(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            if (element is Point point)
            {
                    if (point.Value < 0) return false;
            }
            else if (element is Segment segment)
            {
                    if (segment.RightLimitAtStartTime < 0 || segment.LeftLimitAtEndTime < 0) return false;
            }
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonDecreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is non-decreasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this IEnumerable<Element> elements)
        => IsNonDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-decreasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this ReadOnlySpan<Element> elements)
        => IsNonDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-decreasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonDecreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsNonDecreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonDecreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.NonDecreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsIncreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is strictly increasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this IEnumerable<Element> elements)
        => IsIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly increasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this ReadOnlySpan<Element> elements)
        => IsIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly increasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsIncreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsIncreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsIncreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.StrictlyIncreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsNonIncreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is non-increasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this IEnumerable<Element> elements)
        => IsNonIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-increasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this ReadOnlySpan<Element> elements)
        => IsNonIncreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) \le f(s)$.
    /// </summary>
    /// <returns>Whether the elements are non-increasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsNonIncreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsNonIncreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsNonIncreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.NonIncreasing);
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsDecreasing(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>Whether the stream is strictly decreasing.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this IEnumerable<Element> elements)
        => IsDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly decreasing.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this ReadOnlySpan<Element> elements)
        => IsDecreasing(new ElementReader(elements));

    /// <summary>
    /// True if for any $t > s$, $f(t) &lt; f(s)$.
    /// </summary>
    /// <returns>Whether the elements are strictly decreasing.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsDecreasing(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsDecreasing((ReadOnlySpan<Element>) elements);
    }

    private static bool IsDecreasing(ElementReader elements)
    {
        return CheckMonotonicity(elements, Monotonicity.StrictlyDecreasing);
    }

    private enum Monotonicity
    {
        NonDecreasing,
        StrictlyIncreasing,
        NonIncreasing,
        StrictlyDecreasing
    }

    /// <summary>
    /// Checks slopes and adjacent boundary values in one traversal of uninterrupted elements in time order.
    /// </summary>
    /// <param name="elements">The nonempty element stream.</param>
    /// <param name="monotonicity">The required monotonicity.</param>
    /// <returns>Whether the requested monotonicity holds.</returns>
    /// <remarks>
    /// <see cref="ElementReader.MoveNext"/> checks adjoining times and alternating points and segments during traversal.
    /// An invalid unread tail is not checked when a monotonicity violation has already determined the result.
    /// </remarks>
    /// <exception cref="ArgumentException">If the stream is empty or a consumed element violates uninterrupted time order.</exception>
    private static bool CheckMonotonicity(ElementReader elements, Monotonicity monotonicity)
    {
        elements.ThrowIfStarted();
        Rational? previous = null;
        using var reader = elements;
        while (reader.MoveNext())
        {
            var element = reader.Current;
            var start = element is Point point ? point.Value : ((Segment) element).RightLimitAtStartTime;
            if (previous is { } value && (monotonicity switch
                {
                    Monotonicity.NonDecreasing or Monotonicity.StrictlyIncreasing => value > start,
                    Monotonicity.NonIncreasing or Monotonicity.StrictlyDecreasing => value < start,
                    _ => throw new ArgumentOutOfRangeException(nameof(monotonicity))
                }))
                return false;
            if (element is Segment segment)
            {
                if (monotonicity switch
                    {
                        Monotonicity.NonDecreasing => segment.Slope < 0,
                        Monotonicity.StrictlyIncreasing => segment.Slope <= 0,
                        Monotonicity.NonIncreasing => segment.Slope > 0,
                        Monotonicity.StrictlyDecreasing => segment.Slope >= 0,
                        _ => throw new ArgumentOutOfRangeException(nameof(monotonicity))
                    })
                    return false;
                previous = segment.LeftLimitAtEndTime;
            }
            else
                previous = start;
        }
        if (previous is null)
            throw new ArgumentException("Elements is an empty collection");
        return true;
    }

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsFinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsFinite(this IEnumerable<Element> elements)
        => IsFinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsFinite(this ReadOnlySpan<Element> elements)
        => IsFinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsFinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsFinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsFinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsFinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsMinusInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this IEnumerable<Element> elements)
        => IsMinusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this ReadOnlySpan<Element> elements)
        => IsMinusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsMinusInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsMinusInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsMinusInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsMinusInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="IsPlusInfinite(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this IEnumerable<Element> elements)
        => IsPlusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this ReadOnlySpan<Element> elements)
        => IsPlusInfinite(new ElementReader(elements));

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <returns>The result.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static bool IsPlusInfinite(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return IsPlusInfinite((ReadOnlySpan<Element>) elements);
    }

    private static bool IsPlusInfinite(ElementReader elements)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            if (!reader.Current.IsPlusInfinite)
                return false;
        } while (reader.MoveNext());
        return true;
    }

    #endregion

    #region Extrema

    /// <summary>
    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \le x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>

    public static Rational BreakpointSupValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Max();
    }

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>
    /// The time may be an excluded endpoint or plus infinity; it need not attain the bound.
    /// Arrays and lists are read as spans, through <see cref="SupValueAndArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this IEnumerable<Element> elements)
        => SupValueAndArg(new ElementReader(elements));

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this ReadOnlySpan<Element> elements)
        => SupValueAndArg(new ElementReader(elements));

    /// <summary>Finds the supremum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) SupValueAndArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return SupValueAndArg((ReadOnlySpan<Element>) elements);
    }

    private static (Rational Value, Rational Time) SupValueAndArg(ElementReader elements)
        => FindBoundaryExtremum(elements, BoundKind.Supremum);

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>
    /// The time may be an excluded endpoint or plus infinity; it need not attain the bound.
    /// Arrays and lists are read as spans, through <see cref="InfValueAndArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this IEnumerable<Element> elements)
        => InfValueAndArg(new ElementReader(elements));

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this ReadOnlySpan<Element> elements)
        => InfValueAndArg(new ElementReader(elements));

    /// <summary>Finds the infimum and its earliest boundary location in one traversal.</summary>
    /// <returns>The bound and the earliest time where it is a value or a one-sided limit.</returns>
    /// <remarks>The time may be an excluded endpoint or plus infinity; it need not attain the bound.</remarks>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static (Rational Value, Rational Time) InfValueAndArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return InfValueAndArg((ReadOnlySpan<Element>) elements);
    }

    private static (Rational Value, Rational Time) InfValueAndArg(ElementReader elements)
        => FindBoundaryExtremum(elements, BoundKind.Infimum);

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="SupValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational SupValue(this IEnumerable<Element> elements)
        => SupValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupValue(this ReadOnlySpan<Element> elements)
        => SupValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for every $t$ in the support, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return SupValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational SupValue(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Supremum).Value;
    }

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="SupArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational SupArg(this IEnumerable<Element> elements)
        => SupArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupArg(this ReadOnlySpan<Element> elements)
        => SupArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the supremum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational SupArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return SupArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational SupArg(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Supremum).Time;
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MaxValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this IEnumerable<Element> elements)
        => MaxValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this ReadOnlySpan<Element> elements)
        => MaxValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained maximum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return MaxValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MaxValue(ElementReader elements)
    {
        var (value, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Supremum);
        return attainingElement is null ? null : value;
    }

    /// <inheritdoc cref="MaxValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxValue(this IReadOnlyCollection<Element> elements)
    {
        return MaxValue((IEnumerable<Element>) elements);
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MaxArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this IEnumerable<Element> elements)
        => MaxArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this ReadOnlySpan<Element> elements)
        => MaxArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the maximum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return MaxArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MaxArg(ElementReader elements)
    {
        var (_, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Supremum);
        return attainingElement switch
        {
            Point point => point.Time,
            Segment segment => InteriorTime(segment),
            _ => null
        };
    }

    /// <inheritdoc cref="MaxArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MaxArg(this IReadOnlyCollection<Element> elements)
    {
        return MaxArg((IEnumerable<Element>) elements);
    }

    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \ge x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    public static Rational BreakpointInfValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Min();
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="InfValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational InfValue(this IEnumerable<Element> elements)
        => InfValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfValue(this ReadOnlySpan<Element> elements)
        => InfValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for every $t$ in the support, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return InfValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational InfValue(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Infimum).Value;
    }

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="InfArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational InfArg(this IEnumerable<Element> elements)
        => InfArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfArg(this ReadOnlySpan<Element> elements)
        => InfArg(new ElementReader(elements));

    /// <summary>Returns the earliest boundary location of the infimum.</summary>
    /// <returns>The earliest time where the bound is a value or a one-sided limit, including excluded endpoints.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational InfArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return InfArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational InfArg(ElementReader elements)
    {
        return FindBoundaryExtremum(elements, BoundKind.Infimum).Time;
    }

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MinValue(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinValue(this IEnumerable<Element> elements)
        => MinValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinValue(this ReadOnlySpan<Element> elements)
        => MinValue(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>The attained minimum, or null when it is not attained.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinValue(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return MinValue((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MinValue(ElementReader elements)
    {
        var (value, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Infimum);
        return attainingElement is null ? null : value;
    }

    /// <inheritdoc cref="MinValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinValue(this IReadOnlyCollection<Element> elements)
    {
        return MinValue((IEnumerable<Element>) elements);
    }
    
    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <remarks>
    /// Arrays and lists are read as spans, through <see cref="MinArg(ReadOnlySpan{Element})"/>.
    /// </remarks>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinArg(this IEnumerable<Element> elements)
        => MinArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty span in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the span is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinArg(this ReadOnlySpan<Element> elements)
        => MinArg(new ElementReader(elements));

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for every $t$ in the support and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    /// <returns>An argument in the support attaining the minimum, or null if unattained.
    /// An open plateau may have no earliest attaining argument.</returns>
    /// <param name="elements">A nonempty array in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If the array is empty.</exception>
    /// <exception cref="ArgumentException">If an element read violates uninterrupted time order.</exception>
    public static Rational? MinArg(this Element[] elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        return MinArg((ReadOnlySpan<Element>) elements);
    }

    private static Rational? MinArg(ElementReader elements)
    {
        var (_, attainingElement) = FindBoundAndAttainment(elements, BoundKind.Infimum);
        return attainingElement switch
        {
            Point point => point.Time,
            Segment segment => InteriorTime(segment),
            _ => null
        };
    }

    /// <inheritdoc cref="MinArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    /// <param name="elements">A nonempty stream in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentException">If the stream is empty.</exception>
    /// <exception cref="ArgumentException">If an element read during enumeration violates uninterrupted time order.</exception>
    public static Rational? MinArg(this IReadOnlyCollection<Element> elements)
    {
        return MinArg((IEnumerable<Element>) elements);
    }

    private enum BoundKind
    {
        Infimum,
        Supremum
    }

    /// <summary>
    /// Chooses a finite time strictly inside the segment's open support.
    /// </summary>
    /// <param name="segment">The segment to sample.</param>
    /// <returns>An interior time, without requiring a particular representative.</returns>
    /// <remarks>
    /// For a constant segment, every interior time attains its value, whereas its endpoints are excluded.
    /// </remarks>
    private static Rational InteriorTime(Segment segment)
    {
        if (segment.StartTime.IsFinite)
        {
            if (segment.EndTime.IsFinite)
            {
                // The midpoint lies strictly between the two finite endpoints.
                return segment.StartTime + (segment.EndTime - segment.StartTime) / 2;
            }
            else
            {
                // With no upper bound, moving one unit right stays inside the support.
                return segment.StartTime + 1;
            }
        }
        else
        {
            if (segment.EndTime.IsFinite)
            {
                // With no lower bound, moving one unit left stays inside the support.
                return segment.EndTime - 1;
            }
            else
            {
                // The support is the whole real line, so zero is an interior time.
                return 0;
            }
        }
    }

    private static (Rational Value, Rational Time) FindBoundaryExtremum(
        ElementReader elements, BoundKind kind)
    {
        elements.ThrowIfStarted();
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        var first = reader.Current;
        var bestValue = first is Point p ? p.Value : ((Segment) first).RightLimitAtStartTime;
        var bestTime = first.StartTime;
        do
        {
            if (reader.Current is Point point)
                Consider(point.Value, point.Time);
            else
            {
                var segment = (Segment) reader.Current;
                Consider(segment.RightLimitAtStartTime, segment.StartTime);
                Consider(segment.LeftLimitAtEndTime, segment.EndTime);
            }
        } while (reader.MoveNext());
        return (bestValue, bestTime);

        void Consider(Rational value, Rational time)
        {
            var improves = kind == BoundKind.Supremum ? value > bestValue : value < bestValue;
            if (improves || (value == bestValue && time < bestTime))
            {
                bestValue = value;
                bestTime = time;
            }
        }
    }

    /// <summary>
    /// Finds the extreme boundary value and the first element attaining it, retaining only the current candidate.
    /// </summary>
    /// <param name="elements">The element stream.</param>
    /// <param name="boundKind">The bound to find.</param>
    /// <returns>The extreme value and its first attaining element, or a null element when the bound is unattained.</returns>
    /// <exception cref="ArgumentException">If the stream is empty or an encountered element violates uninterrupted time order.</exception>
    private static (Rational bound, Element? attainingElement) FindBoundAndAttainment(ElementReader elements, BoundKind boundKind)
    {
        elements.ThrowIfStarted();
        Rational? extreme = null;
        Element? attainingElement = null;
        using var reader = elements;
        if (!reader.MoveNext())
            throw new ArgumentException("The element stream is empty.", nameof(elements));
        do
        {
            var element = reader.Current;
            Rational candidate;
            if (element is Point point)
                candidate = point.Value;
            else if (element is Segment segment)
                candidate = boundKind == BoundKind.Supremum
                    ? Rational.Max(segment.RightLimitAtStartTime, segment.LeftLimitAtEndTime)
                    : Rational.Min(segment.RightLimitAtStartTime, segment.LeftLimitAtEndTime);
            else
                continue;

            if (extreme is null || (boundKind == BoundKind.Supremum ? candidate > extreme.Value : candidate < extreme.Value))
            {
                extreme = candidate;
                attainingElement = element.AttainsValue(candidate) ? element : null;
            }
            else if (candidate == extreme.Value && attainingElement is null && element.AttainsValue(candidate))
                attainingElement = element;
        } while (reader.MoveNext());
        return (extreme!.Value, attainingElement);
    }

    #endregion

    #region Continuity transformations

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
            throw new ArgumentException("Elements is an empty collection");
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
            throw new ArgumentException("Elements is an empty collection");

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

    #endregion

    #region Pseudo-inverses

    /// <summary>
    /// Computes the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) >= x \right\}$, where $t \in Support(f)$ and $x \in Image(f)$.
    /// </summary>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\downarrow(x)$ is defined from $x = 0$, rather than only in $Image(f)$: the heights from 0 up to the first value of the sequence are first reached at its start.</param>
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
            throw new ArgumentException("Elements is an empty collection");

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
    /// $f^{-1}_\uparrow(x) = \inf \left\{ t : f(t) > x \right\}$, where $t \in Support(f)$ and $x \in Image(f)$.
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
                throw new ArgumentException("Elements is an empty collection");

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

    /// <summary>
    /// Filters elements whose value exceeds a ceiling threshold.
    /// </summary>
    /// <remarks>
    /// This method does not support decreasing segments.
    /// If a decreasing <see cref="Segment"/> is encountered, an <see cref="ArgumentException"/> is thrown.
    /// </remarks>
    internal static IEnumerable<Element> CutWithCeiling(
        this IEnumerable<Element> elements,
        Rational? ceiling,
        bool isCeilingIncluded = true)
    {
        if (ceiling is null || ceiling == Rational.PlusInfinity)
            return elements;
        else
            return _CutWithCeiling();

        IEnumerable<Element> _CutWithCeiling()
        {
            var _ceiling = (Rational) ceiling;
            using var enumerator = elements.GetEnumerator();
            var lastValue = Rational.MinusInfinity;
            
            while (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case Point p:
                    {
                        if (p.Value < _ceiling)
                            yield return p;
                        else if (isCeilingIncluded && p.Value == _ceiling)
                            yield return p;
                        else
                        {
                            // if there is a discontinuity crossing the ceiling, we yield a smaller one
                            if (lastValue < _ceiling)
                                yield return new Point(p.Time, _ceiling);
                            yield break; // we presume non-decreasing sequence
                        }

                        lastValue = p.Value;
                        break;
                    }

                    case Segment s:
                    {
                        if (s.Slope < 0)
                            throw new ArgumentException(
                                "cutCeiling is not supported for decreasing segments. " +
                                "If the input sequence contains decreasing segments, omit the cutCeiling parameter or ensure the sequence is non-decreasing.");

                        if (s.IsConstant)
                        {
                            var value = s.LeftLimitAtEndTime;
                            lastValue = value;
                            if (value < _ceiling)
                                yield return s;
                            else if (isCeilingIncluded && value == _ceiling)
                                yield return s;
                            else
                                yield break;
                        }
                        else if (s.LeftLimitAtEndTime <= _ceiling)
                        {
                            lastValue = s.LeftLimitAtEndTime;
                            yield return s;
                        }
                        else if (s.RightLimitAtStartTime < _ceiling)
                        {
                            var ceilingTime =  s.StartTime + (_ceiling - s.RightLimitAtStartTime) / s.Slope;
                            var (left, center, _) = s.Split(ceilingTime);
                            yield return left;
                            if (isCeilingIncluded)
                                yield return center;
                            yield break;
                        }
                        else
                            yield break;

                        break;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }
        }
    }

    #endregion

    #region Envelopes

    /// <summary>
    /// Computes the lower envelope of the set of elements given.
    /// $O(n \cdot \log(n))$ complexity.
    /// </summary>
    /// <remarks>Used for convolution</remarks>
    public static List<Element> LowerEnvelope(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if (!elements.Any())
            throw new ArgumentException("The set of elements is empty");

        #if DO_LOG
        var intervalsStopwatch = Stopwatch.StartNew();
        #endif
        var intervals = IntervalBucket.ComputeIntervals(elements, settings);
        #if DO_LOG
        intervalsStopwatch.Stop();
        logger.Debug($"Intervals computed: {elements.Count} elements, {intervalsStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif

        #if DO_LOG
        var lowerEnvelopeStopwatch = Stopwatch.StartNew();
        #endif
        List<Element> lowerElements;
        bool doParallel = settings.UseParallelLowerEnvelope;
        if (doParallel)
        {
            lowerElements = intervals
                .AsParallel().AsOrdered()
                .Select(i => i.LowerEnvelope())
                .SelectMany(element => element)
                .ToList();
        }
        else
        {
            lowerElements = intervals
                .Select(i => i.LowerEnvelope())
                .SelectMany(element => element)
                .ToList();
        }

        var sequence = lowerElements.Merge(settings: settings);
        #if DO_LOG
        lowerEnvelopeStopwatch.Stop();
        logger.Debug($"Lower envelopes computed: {elements.Count} elements, {lowerEnvelopeStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif
        return sequence;
    }

    /// <summary>
    /// Computes the lower envelope of the set of sequences given.
    /// </summary>
    /// <remarks>Used for partitioned convolution.</remarks>
    public static IReadOnlyList<Element> LowerEnvelope(this IReadOnlyList<Sequence> sequences, ComputationSettings? settings = null)
    {
        if (sequences.Count == 1)
            return sequences.First().Elements;
        else if(sequences.Count == 2)
            return Sequence.Minimum(sequences.First(), sequences.Last(), cutToOverlap: false, settings).Elements;
        else
        {
            settings ??= ComputationSettings.Default();
            const int ParallelizationThreshold = 8;

            bool doParallel = settings.UseParallelListLowerEnvelope && sequences.Count > ParallelizationThreshold;
            if (doParallel)
            {
                return sequences
                    .AsParallel()
                    .Aggregate((a, b) => Sequence.Minimum(a, b, cutToOverlap: false, settings))
                    .Elements;
            }
            else
            {
                return sequences
                    .Aggregate((a, b) => Sequence.Minimum(a, b, cutToOverlap: false, settings))
                    .Elements;
            }
        }
    }

    /// <summary>
    /// Computes the upper envelope of the set of elements given.
    /// $O(n \cdot \log(n))$ complexity.
    /// </summary>
    /// <param name="elements">The elements to process.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <remarks>Used for deconvolution</remarks>
    public static List<Element> UpperEnvelope(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if (!elements.Any())
            throw new ArgumentException("The set of elements is empty");

        #if DO_LOG
        var intervalsStopwatch = Stopwatch.StartNew();
        #endif
        var intervals = IntervalBucket.ComputeIntervals(elements, settings);
        #if DO_LOG
        intervalsStopwatch.Stop();
        logger.Debug($"Intervals computed: {elements.Count} elements, {intervalsStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif

        #if DO_LOG
        var upperEnvelopeStopwatch = Stopwatch.StartNew();
        #endif
        List<Element> upperElements;
        bool doParallel = settings.UseParallelUpperEnvelope;
        if (doParallel)
        {
            upperElements = intervals
                .AsParallel().AsOrdered()
                .Select(i => i.UpperEnvelope())
                .SelectMany(element => element)
                .ToList();
        }
        else
        {
            upperElements = intervals
                .Select(i => i.UpperEnvelope())
                .SelectMany(element => element)
                .ToList();
        }

        var sequence = upperElements.Merge(settings: settings);
        #if DO_LOG
        upperEnvelopeStopwatch.Stop();
        logger.Debug($"Lower envelopes computed: {elements.Count} elements, {upperEnvelopeStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif
        return sequence;
    }

    /// <summary>
    /// Computes the lower envelope of the set of sequences given.
    /// </summary>
    public static IReadOnlyList<Element> UpperEnvelope(this IReadOnlyList<Sequence> sequences, ComputationSettings? settings = null)
    {
        if (sequences.Count == 1)
            return sequences.First().Elements;
        else if(sequences.Count == 2)
            return Sequence.Maximum(sequences.First(), sequences.Last(), cutToOverlap: false, settings).Elements;
        else
        {
            settings ??= ComputationSettings.Default();
            const int ParallelizationThreshold = 8;

            bool doParallel = settings.UseParallelListUpperEnvelope && sequences.Count > ParallelizationThreshold;
            if (doParallel)
            {
                return sequences
                    .AsParallel()
                    .Aggregate((a, b) => Sequence.Maximum(a, b, cutToOverlap: false, settings))
                    .Elements;
            }
            else
            {
                return sequences
                    .Aggregate((a, b) => Sequence.Maximum(a, b, cutToOverlap: false, settings))
                    .Elements;
            }
        }
    }
    #endregion

}
