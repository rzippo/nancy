using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Provides LINQ extensions methods for <see cref="Sequence"/> and <see cref="Element"/>.
/// </summary>
public static class SequenceExtensions
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

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

    /// <summary>
    /// Returns a cut of the sequence for a smaller support.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="cutStart">Left endpoint of the new support.</param>
    /// <param name="cutEnd">Right endpoint of the new support.</param>
    /// <param name="isStartIncluded">If true, the support is left-closed.</param>
    /// <param name="isEndIncluded">If true, the support is right-closed.</param>
    /// <exception cref="ArgumentException">Thrown if the new support is not a subset of the current one.</exception>
    /// <remarks>Optimized for minimal allocations</remarks>
    public static IEnumerable<Element> Cut(
        this IEnumerable<Element> elements,
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        using var enumerator = elements.GetEnumerator();
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
    /// Fills the gaps of the set of elements within <paramref name="fillFrom"/> and <paramref name="fillTo"/>
    /// with the given value, defaults to $+\infty$.
    /// </summary>
    /// <param name="elements">The set of elements. Must be in order.</param>
    /// <param name="fillFrom">Left endpoint of the filling interval.</param>
    /// <param name="fillTo">Right endpoint of the filling interval.</param>
    /// <param name="isFromIncluded">If true, left endpoint is inclusive.</param>
    /// <param name="isToIncluded">If true, right endpoint is inclusive.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    /// <returns></returns>
    public static IEnumerable<Element> Fill(
        this IEnumerable<Element> elements,
        Rational fillFrom,
        Rational fillTo,
        bool isFromIncluded = true,
        bool isToIncluded = false,
        Rational? fillWith = null
    )
    {
        Rational value = fillWith ?? Rational.PlusInfinity;
        Rational expectedStart = fillFrom;
        bool isExpectingPoint = isFromIncluded;
        foreach (var element in elements)
        {
            if(expectedStart > element.StartTime)
                throw new ArgumentException("Segments out of order");

            if (expectedStart < element.StartTime)
            {
                if (isExpectingPoint)
                    yield return new Point(expectedStart, value);
                yield return fillSegment(expectedStart, element.StartTime);
                if(element is Segment)
                    yield return new Point(element.StartTime, value);
            }

            if (expectedStart == element.StartTime && !(element is Point) && isExpectingPoint)
            {
                yield return new Point(expectedStart, value);
            }

            yield return element;
            expectedStart = element.EndTime;
            isExpectingPoint = element is Segment;
        }

        if (expectedStart < fillTo)
        {
            if (isExpectingPoint)
                yield return new Point(expectedStart, value);
            yield return fillSegment(expectedStart, fillTo);
            isExpectingPoint = true;
        }

        if(isToIncluded && isExpectingPoint)
            yield return new Point(fillTo, value);

        Segment fillSegment(Rational start, Rational end)
        {
            if (value.IsPlusInfinite)
            {
                return Segment.PlusInfinite(start, end);
            }
            else
            {
                return Segment.Constant(start, end, value);
            }
        }
    }

    /// <summary>
    /// Applies merge, whenever possible, to a set of elements.
    /// The result is the minimal set, in number of elements, to represent the same information.
    /// </summary>
    /// <param name="elements">The elements to merge.</param>
    /// <param name="doSort">If true, the elements are sorted before attempting to merge.</param>
    /// <param name="settings">Settings to forward to SortElements, if used.</param>
    /// <returns>A set where no further merges are possible.</returns>
    public static List<Element> Merge(this IReadOnlyList<Element> elements, bool doSort = false, ComputationSettings? settings = null)
    {
        var reorderedElements = doSort ? elements.SortElements(settings): elements;

        List<Element> mergedElements = new List<Element>();

        int index = 0;
        while (index < reorderedElements.Count)
        {
            switch(reorderedElements[index])
            {
                case Point point:
                    mergedElements.Add(point);
                    break;

                case Segment _:
                    var segment = MergeAhead();
                    mergedElements.Add(segment);
                    break;
            }

            index++;

            Segment MergeAhead()
            {
                Segment left = (Segment)reorderedElements[index];

                while (index < reorderedElements.Count - 2)
                {
                    if (
                        reorderedElements[index + 1] is Point point &&
                        reorderedElements[index + 2] is Segment right &&
                        CanMergeTriplet(left, point, right)
                    )
                    {
                        left = MergeTriplet(left, point, right);
                        index += 2;
                    }
                    else
                    {
                        break;
                    }
                }

                return left;
            }                
        }

        return mergedElements;
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
    /// <param name="elements">The elements to merge, must be sorted</param>
    /// <returns>A set where no further merges are possible.</returns>
    /// <remarks>Optimized for minimal allocations</remarks>
    public static IEnumerable<Element> MergeAsEnumerable(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        // pass on first point, if any
        if (enumerator.Current is Point)
        {
            yield return enumerator.Current;
            if(!enumerator.MoveNext())
                yield break;
        }

        Segment left;
        if (enumerator.Current is not Segment)
            throw new ArgumentException("Invalid sequence of elements");
        left = (Segment) enumerator.Current;

        // At the start of each iteration, left is the current segment
        // as long as we don't hit the end,
        // we fetch a point and a segment, check for merging, and restore the initial condition
        while (true)
        {
            if (!enumerator.MoveNext())
            {
                yield return left;
                yield break;
            }

            if (enumerator.Current is not Point center)
                throw new ArgumentException("Invalid sequence of elements");

            if (!enumerator.MoveNext())
            {
                yield return left;
                yield return center;
                yield break;
            }

            if (enumerator.Current is not Segment right)
                throw new ArgumentException("Invalid sequence of elements");

            if (SequenceExtensions.CanMergeTriplet(left, center, right))
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
    /// Returns the <see cref="Element"/> that describes the sequence in <paramref name="time"/>.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Element"/> describing the sequence at <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    public static Element GetElementAt(this IEnumerable<Element> elements, Rational time)
    {
        try
        {
            var targetElement = elements.Single(e => e.IsDefinedFor(time));
            return targetElement;
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("The given time is out of sequence support.");
        }
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence before <paramref name="time"/>.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence before <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    public static Segment GetSegmentBefore(this IEnumerable<Element> elements, Rational time)
    {
        try
        {
            var targetElement = elements.Last(e => e is Segment s && s.EndTime <= time);
            return (Segment) targetElement;
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Sequence is not defined after given time");
        }
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence after <paramref name="time"/>.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence after <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a linear search, $O(n)$</remarks>
    public static Segment GetSegmentAfter(this IEnumerable<Element> elements, Rational time)
    {
        try
        {
            var targetElement = elements.First(element => element.EndTime > time);
            return (Segment) targetElement;
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Sequence is not defined after given time");
        }
    }

    /// <summary>
    /// Checks if time order is respected, i.e. they are ordered first by start, then by end.
    /// </summary>
    public static bool AreInTimeOrder(this IEnumerable<Element> elements)
    {
        var lastStartTime = Rational.MinusInfinity;
        var lastEndTime = Rational.MinusInfinity;

        foreach (var element in elements)
        {
            if (element.StartTime >= lastStartTime)
            {
                if (element.StartTime == lastStartTime)
                {
                    if (element.EndTime >= lastEndTime)
                    {
                        lastStartTime = element.StartTime;
                        lastEndTime = element.EndTime;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    lastStartTime = element.StartTime;
                    lastEndTime = element.EndTime;
                    continue;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the elements form a sequence.
    /// In addition to <see cref="AreInTimeOrder(IEnumerable{Element})"/> it requires non-overlapping, but it allows gaps.
    /// </summary>
    public static bool AreInTimeSequence(this IEnumerable<Element> elements)
    {
        var nextExpectedTime = Rational.MinusInfinity;
        foreach (var element in elements)
        {
            if (element.StartTime >= nextExpectedTime)
            {
                nextExpectedTime = element.EndTime;
                continue;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the elements form a uninterrupted sequence.
    /// </summary>
    public static bool AreUninterruptedSequence(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("The collection is empty");

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
    /// Sorts the elements in time order.
    /// </summary>
    public static IReadOnlyList<Element> SortElements(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
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

    /// <summary>
    /// Enumerates a <see cref="Sequence"/> as a series of breakpoints.
    /// </summary>
    /// <param name="sequence"></param>
    /// <remarks>Does not attempt merging.</remarks>
    public static IEnumerable<(Segment? left, Point center, Segment? right)> EnumerateBreakpoints(this Sequence sequence)
        => sequence.Elements.EnumerateBreakpoints();

    /// <summary>
    /// Enumerates a set of <see cref="Element"/>s as a series of breakpoints.
    /// </summary>
    /// <param name="elements">The elements to enumerate, must be sorted.</param>
    /// <remarks>Does not attempt merging.</remarks>
    public static IEnumerable<(Segment? left, Point center, Segment? right)> EnumerateBreakpoints(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
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

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    public static bool IsContinuous(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if(!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        Rational lastValue = enumerator.Current is Point p ? 
            p.Value :
            ((Segment)enumerator.Current).LeftLimitAtEndTime;

        while (enumerator.MoveNext())
        {
            switch(enumerator.Current)
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
    public static bool IsLeftContinuous(this IEnumerable<Element> elements)
    {
        foreach(var breakpoint in elements.EnumerateBreakpoints())
        {
            if (breakpoint.left is { } s &&
                s.LeftLimitAtEndTime != breakpoint.center.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// True if there is no right-discontinuity within the sequence.
    /// </summary>
    public static bool IsRightContinuous(this IEnumerable<Element> elements)
    {
        foreach (var breakpoint in elements.EnumerateBreakpoints())
        {
            if (breakpoint.right is { } s &&
                s.RightLimitAtStartTime != breakpoint.center.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    public static bool IsNonNegative(this IEnumerable<Element> elements)
    {
        return elements.InfValue() >= 0;
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    public static bool IsNonDecreasing(this IEnumerable<Element> elements)
    {
        foreach (var breakpoint in elements.EnumerateBreakpoints())
        {
            if (
                (breakpoint.left is Segment l && ( l.Slope < 0 || l.LeftLimitAtEndTime > breakpoint.center.Value )) ||
                (breakpoint.right is Segment r && ( r.Slope < 0 || breakpoint.center.Value > r.RightLimitAtStartTime ))
            )
                return false;
        }
        return true;
    }

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    public static bool IsIncreasing(this IEnumerable<Element> elements)
    {
        foreach (var breakpoint in elements.EnumerateBreakpoints())
        {
            if (
                (breakpoint.left is Segment l && ( l.Slope <= 0 || l.LeftLimitAtEndTime > breakpoint.center.Value )) ||
                (breakpoint.right is Segment r && ( r.Slope <= 0 || breakpoint.center.Value > r.RightLimitAtStartTime ))
            )
                return false;
        }
        return true;
    }

    /// <summary>
    /// True if for any $t$, $\left|f(t)\right| &lt; +\infty$.
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    public static bool IsFinite(this IEnumerable<Element> elements)
    {
        return elements.All(e => e.IsFinite);
    }

    /// <summary>
    /// True if for any $t$, $f(t) = -\infty$.
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    public static bool IsMinusInfinite(this IEnumerable<Element> elements)
    {
        return elements.All(e => e.IsMinusInfinite);
    }

    /// <summary>
    /// True if for any $t$, $f(t) = +\infty$.
    /// </summary>
    /// <param name="elements"></param>
    /// <returns></returns>
    public static bool IsPlusInfinite(this IEnumerable<Element> elements)
    {
        return elements.All(e => e.IsPlusInfinite);
    }

    /// <summary>
    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \le x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>

    public static Rational BreakpointSupValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Max();
    }

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public static Rational SupValue(this IEnumerable<Element> elements)
    {
        return elements.GetElementsBoundaryValues().Max();
    }

    /// <summary>
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$,
    /// returns the first time around which $f(t)$ gets close to $x$ (i.e., either it attains the value or has it as a limit).
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public static Rational SupArg(this IEnumerable<Element> elements)
    {
        var supAndArg = elements
            .GetElementsBoundaryPairs()
            .MaxBy(pair => pair.value);
        return supAndArg.time;
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for any $t \ge 0$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public static Rational? MaxValue(this IEnumerable<Element> elements)
    {
        var list = elements.ToList();
        var sup = list.SupValue();
        if (list.Any(e => e.AttainsValue(sup)))
            return sup;
        else
            return null;
    }

    /// <inheritdoc cref="MaxValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    public static Rational? MaxValue(this IReadOnlyCollection<Element> elements)
    {
        var sup = elements.SupValue();
        if (elements.Any(e => e.AttainsValue(sup)))
            return sup;
        else
            return null;
    }

    /// <summary>
    /// If the sequence has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for any $t \ge 0$ and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    public static Rational? MaxArg(this IEnumerable<Element> elements)
    {
        var list = elements.ToList();
        var sup = list.SupValue();
        var attainingElement = list
            .FirstOrDefault(e => e.AttainsValue(sup));

        return attainingElement switch
        {
            Point p => p.Time,
            Segment s => s.TimeAt(sup),
            _ => null
        };
    }

    /// <inheritdoc cref="MaxArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    public static Rational? MaxArg(this IReadOnlyCollection<Element> elements)
    {
        var sup = elements.SupValue();
        var attainingElement = elements
            .FirstOrDefault(e => e.AttainsValue(sup));

        return attainingElement switch
        {
            Point p => p.Time,
            Segment s => s.TimeAt(sup),
            _ => null
        };
    }

    /// If the sequence is upper-bounded around the breakpoint $t_c$, i.e., exists $x$ such that $f(t) \ge x$ for any $t \in [t_c - \varepsilon, t_c + \varepsilon]$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    public static Rational BreakpointInfValue(this (Segment? left, Point center, Segment? right) breakpoint)
    {
        return breakpoint.GetBreakpointBoundaryValues().Min();
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for any $t \ge 0$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public static Rational InfValue(this IEnumerable<Element> elements)
    {
        return elements.GetElementsBoundaryValues().Min();
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for any $t \ge 0$,
    /// returns the first time around which $f(t)$ gets close to $x$ (i.e., either it attains the value or has it as a limit).
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public static Rational InfArg(this IEnumerable<Element> elements)
    {
        var supAndArg = elements
            .GetElementsBoundaryPairs()
            .MinBy(pair => pair.value);
        return supAndArg.time;
    }

    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for any $t \ge 0$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public static Rational? MinValue(this IEnumerable<Element> elements)
    {
        var list = elements.ToList();
        var inf = list.InfValue();
        if (list.Any(e => e.AttainsValue(inf)))
            return inf;
        else
            return null;
    }

    /// <inheritdoc cref="MinValue(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    public static Rational? MinValue(this IReadOnlyCollection<Element> elements)
    {
        var list = elements.ToList();
        var inf = list.InfValue();
        if (list.Any(e => e.AttainsValue(inf)))
            return inf;
        else
            return null;
    }
    
    /// <summary>
    /// If the sequence has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for any $t \ge 0$ and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    public static Rational? MinArg(this IEnumerable<Element> elements)
    {
        var list = elements.ToList();
        var inf = list.InfValue();
        var attainingElement = list
            .FirstOrDefault(e => e.AttainsValue(inf));

        return attainingElement switch
        {
            Point p => p.Time,
            Segment s => s.TimeAt(inf),
            _ => null
        };
    }

    /// <inheritdoc cref="MinArg(System.Collections.Generic.IEnumerable{Unipi.Nancy.MinPlusAlgebra.Element})"/>
    public static Rational? MinArg(this IReadOnlyCollection<Element> elements)
    {
        var inf = elements.InfValue();
        var attainingElement = elements
            .FirstOrDefault(e => e.AttainsValue(inf));

        return attainingElement switch
        {
            Point p => p.Time,
            Segment s => s.TimeAt(inf),
            _ => null
        };
    }

    /// <summary>
    /// Computes a left-continuous version of this sequence. 
    /// </summary>
    public static IEnumerable<Element> ToLeftContinuous(this IEnumerable<Element> elements)
    {
        var isStart = true;
        foreach (var (left, point, right) in elements.EnumerateBreakpoints())
        {
            if (left != null)
            {
                if (isStart)
                    yield return left;

                if (point.Value == left.LeftLimitAtEndTime)
                    yield return point;
                else
                    yield return new Point(point.Time, left.LeftLimitAtEndTime);
            }
            else
                yield return point;

            if (right != null)
                yield return right;

            isStart = false;
        }
    }

    /// <summary>
    /// Computes a right-continuous version of this sequence. 
    /// </summary>
    public static IEnumerable<Element> ToRightContinuous(this IEnumerable<Element> elements)
    {
        var isStart = true;
        foreach (var (left, point, right) in elements.EnumerateBreakpoints())
        {
            if (isStart && left != null)
                yield return left;
                
            if (right != null)
            {
                if (point.Value == right.RightLimitAtStartTime)
                    yield return point;
                else
                    yield return new Point(point.Time, right.RightLimitAtStartTime);

                yield return right;
            }
            else
                yield return point;

            isStart = false;
        }
    }

    /// <summary>
    /// Computes the lower pseudo-inverse function,
    /// $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) >= x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\downarrow(x)$ be defined from $x = 0$.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <exception cref="ArgumentException">If the collection is empty.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous.
    /// See [DNC18] § 3.2.1 
    /// </remarks>
    public static IEnumerable<Element> LowerPseudoInverse(this IEnumerable<Element> elements, bool startFromZero = true)
    {
        var merged = startFromZero ?
            elements.MergeAsEnumerable().SkipUntilValue(0):
            elements.MergeAsEnumerable();

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
                            }
                            else
                            {
                                // do nothing, as the left-continuity point has already been processed
                            }
                        }
                        else if (s.RightLimitAtStartTime > previousValue)
                        {
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
    }

    /// <summary>
    /// Computes the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf \left\{ t : f(t) > x \right\} = \sup \left\{ t : f(t) &lt;= x \right\}$.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\uparrow(x)$ be defined from $x = 0$.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <exception cref="ArgumentException">If the collection is empty.</exception>
    /// <remarks>
    /// The result of this operation is right-continuous, thus is revertible, i.e. $\left(f^{-1}_\uparrow\right)^{-1}_\uparrow = f$, only if $f$ is right-continuous.
    /// See [DNC18] § 3.2.1 
    /// </remarks>
    public static IEnumerable<Element> UpperPseudoInverse(this IEnumerable<Element> elements, bool startFromZero = true)
    {
        var merged = startFromZero ?
            elements.MergeAsEnumerable().SkipUntilValue(0):
            elements.MergeAsEnumerable();

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

    /// <summary>
    /// Skips elements until <paramref name="value"/> is reached, i.e. while $f(t) &lt; v$.
    /// </summary>
    internal static IEnumerable<Element> SkipUntilValue(this IEnumerable<Element> elements, Rational value)
    {
        using var enumerator = elements.GetEnumerator();
        while (enumerator.MoveNext())
        {
            switch (enumerator.Current)
            {
                case Point p:
                {
                    if (p.Value < value)
                        continue;
                    else
                        yield return p;
                    break;
                }

                case Segment s:
                {
                    if (s.Slope < 0)
                        throw new ArgumentException("Segments must be non-decreasing");

                    if (s.IsConstant && s.LeftLimitAtEndTime < value)
                        continue;
                    else if (!s.IsConstant && s.LeftLimitAtEndTime <= value)
                        continue;
                    else if (s.RightLimitAtStartTime < value)
                    {
                        var (_, center, right) = s.Split(
                            s.Inverse().ValueAt(value)
                        );
                        yield return center;
                        yield return right;
                    }
                    else
                        yield return s;
                    break;
                }

                default:
                    throw new InvalidCastException();
            }
        }
    }

    // todo: write docs
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
                            throw new ArgumentException("Segments must be non-decreasing");

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

        var sequence = lowerElements.Merge();
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
            return Sequence.LowerEnvelope(sequences.First(), sequences.Last(), settings);
        else
        {
            settings ??= ComputationSettings.Default();
            const int ParallelizationThreshold = 8;

            bool doParallel = settings.UseParallelListLowerEnvelope && sequences.Count > ParallelizationThreshold;
            if (doParallel)
            {
                return sequences
                    .AsParallel()
                    .Aggregate((a, b) =>
                    {
                        var elements = Sequence.LowerEnvelope(a, b);
                        return new Sequence(elements, elements.First().StartTime, elements.Last().EndTime);
                    })
                    .Elements;
            }
            else
            {
                return sequences
                    .Aggregate((a, b) =>
                    {
                        var elements = Sequence.LowerEnvelope(a, b);
                        return new Sequence(elements, elements.First().StartTime, elements.Last().EndTime);
                    })
                    .Elements;
            }
        }
    }

    /// <summary>
    /// Computes the upper envelope of the set of elements given.
    /// $O(n \cdot \log(n))$ complexity.
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="settings"></param>W
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

        var sequence = upperElements.Merge();
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
            return Sequence.UpperEnvelope(sequences.First(), sequences.Last(), settings);
        else
        {
            settings ??= ComputationSettings.Default();
            const int ParallelizationThreshold = 8;

            bool doParallel = settings.UseParallelListUpperEnvelope && sequences.Count > ParallelizationThreshold;
            if (doParallel)
            {
                return sequences
                    .AsParallel()
                    .Aggregate((a, b) =>
                    {
                        var elements = Sequence.UpperEnvelope(a, b);
                        return new Sequence(elements, elements.First().StartTime, elements.Last().EndTime);
                    })
                    .Elements;
            }
            else
            {
                return sequences
                    .Aggregate((a, b) =>
                    {
                        var elements = Sequence.UpperEnvelope(a, b);
                        return new Sequence(elements, elements.First().StartTime, elements.Last().EndTime);
                    })
                    .Elements;
            }
        }
    }
}