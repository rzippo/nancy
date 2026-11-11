using System;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Utility;

#if DO_LOG
using System.Diagnostics;
using NLog;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

public static partial class SequenceExtensions
{
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
    /// Combines a finite collection of streams into a lower or upper envelope through a balanced tree of binary sweeps.
    /// </summary>
    /// <param name="streams">Nonempty uninterrupted streams in time order.</param>
    /// <param name="kind">Whether to compute the lower or upper envelope.</param>
    /// <returns>The envelope merged at each binary node, with state proportional to the operand count.</returns>
    /// <remarks>A single operand retains its original support; two or more operands use the filled span of <see cref="EnvelopeUnion"/>.</remarks>
    /// <exception cref="ArgumentException">If the collection or a consumed operand is empty, or a consumed element violates uninterrupted time order.</exception>
    private static IEnumerable<Element> EnvelopeMany(IEnumerable<IEnumerable<Element>> streams, EnvelopeKind kind)
    {
        var operands = BalancedReduction.GetIndexedOperands(streams);
        if (operands.Count == 0)
        {
            throw new ArgumentException("The operand collection is empty.", nameof(streams));
        }

        var result = BalancedReduction.Aggregate(operands,
            (left, right) => EnvelopeUnion(left, right, kind).MergeAsEnumerable(), degreeOfParallelism: 1);

        // This cursor also validates the single-operand case, where no binary sweep was created.
        using var cursor = new ElementCursor(result);
        if (!cursor.MoveNext())
        {
            throw new ArgumentException("An operand stream is empty.", nameof(streams));
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
            throw new ArgumentException("The element stream is empty.", !hasLeft ? nameof(a) : nameof(b));
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

    /// <summary>
    /// Enumerates ordered bucket envelopes without materializing their concatenation.
    /// </summary>
    /// <remarks>Iterate each List directly to avoid boxing one list enumerator per bucket.</remarks>
    internal static IEnumerable<Element> FlattenEnvelopes(this IEnumerable<List<Element>> envelopes)
    {
        foreach (var envelope in envelopes)
            foreach (var element in envelope)
                yield return element;
    }

    /// <summary>
    /// Computes the lower envelope of the set of elements given.
    /// $O(n \cdot \log(n))$ complexity.
    /// </summary>
    /// <remarks>Used for convolution</remarks>
    /// <exception cref="ArgumentException">If the set of elements is empty.</exception>
    public static List<Element> LowerEnvelope(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if (!elements.Any())
            throw new ArgumentException("The set of elements is empty.", nameof(elements));

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
        IEnumerable<Element> lowerElements;
        bool doParallel = settings.UseParallelLowerEnvelope &&
            elements.Count >= settings.ElementsLowerEnvelopeParallelizationThreshold;
        if (doParallel)
        {
            var envelopes = new List<Element>[intervals.Count];
            try
            {
                Parallel.For(0, intervals.Count, new ParallelOptions { MaxDegreeOfParallelism = settings.DegreeOfParallelism },
                    i => envelopes[i] = intervals[i].LowerEnvelope());
            }
            catch (AggregateException aggregate)
            {
                // Flatten failures from nested parallel loops and rethrow one worker failure, preserving its original stack.
                ExceptionDispatchInfo.Capture(aggregate.Flatten().InnerExceptions[0]).Throw();
                throw;
            }

            lowerElements = envelopes.FlattenEnvelopes();
        }
        else
        {
            lowerElements = intervals
                .Select(interval => interval.LowerEnvelope())
                .FlattenEnvelopes();
        }

        var sequence = lowerElements.MergeAsEnumerable()
            .ToList();
        #if DO_LOG
        lowerEnvelopeStopwatch.Stop();
        logger.Debug($"Lower envelopes computed: {elements.Count} elements, {lowerEnvelopeStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif
        return sequence;
    }

    /// <summary>
    /// Computes the lower envelope of the set of sequences given.
    /// </summary>
    /// <remarks>
    /// Used for partitioned convolution.
    /// </remarks>
    /// <exception cref="ArgumentException">If the set of sequences is empty.</exception>
    public static IReadOnlyList<Element> LowerEnvelope(this IReadOnlyList<Sequence> sequences, ComputationSettings? settings = null)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("The set of sequences is empty.", nameof(sequences));
        if (sequences.Count == 1)
            return sequences.First().Elements;
        else if(sequences.Count == 2)
            return Sequence.Minimum(sequences.First(), sequences.Last(), cutToOverlap: false, settings).Elements;
        else
        {
            settings ??= ComputationSettings.Default();
            bool doParallel = settings.UseParallelListLowerEnvelope && sequences.Count >= settings.ListLowerEnvelopeParallelizationThreshold;
            return BalancedReduction.Aggregate(sequences,
                    (a, b) => Sequence.Minimum(a, b, cutToOverlap: false, settings), doParallel ? settings.DegreeOfParallelism : 1)
                .Elements;
        }
    }

    /// <summary>
    /// Computes the upper envelope of the set of elements given.
    /// $O(n \cdot \log(n))$ complexity.
    /// </summary>
    /// <param name="elements">The elements to process.</param>
    /// <param name="settings">Optional settings for the operation.</param>
    /// <remarks>Used for deconvolution</remarks>
    /// <exception cref="ArgumentException">If the set of elements is empty.</exception>
    public static List<Element> UpperEnvelope(this IReadOnlyList<Element> elements, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        if (!elements.Any())
            throw new ArgumentException("The set of elements is empty.", nameof(elements));

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
        IEnumerable<Element> upperElements;
        bool doParallel = settings.UseParallelUpperEnvelope &&
            elements.Count >= settings.ElementsUpperEnvelopeParallelizationThreshold;
        if (doParallel)
        {
            var envelopes = new List<Element>[intervals.Count];
            try
            {
                Parallel.For(0, intervals.Count, new ParallelOptions { MaxDegreeOfParallelism = settings.DegreeOfParallelism },
                    i => envelopes[i] = intervals[i].UpperEnvelope());
            }
            catch (AggregateException aggregate)
            {
                // Flatten failures from nested parallel loops and rethrow one worker failure, preserving its original stack.
                ExceptionDispatchInfo.Capture(aggregate.Flatten().InnerExceptions[0]).Throw();
                throw;
            }

            upperElements = envelopes.FlattenEnvelopes();
        }
        else
        {
            upperElements = intervals
                .Select(interval => interval.UpperEnvelope())
                .FlattenEnvelopes();
        }

        var sequence = upperElements.MergeAsEnumerable()
            .ToList();
        #if DO_LOG
        upperEnvelopeStopwatch.Stop();
        logger.Debug($"Upper envelopes computed: {elements.Count} elements, {upperEnvelopeStopwatch.ElapsedMilliseconds} milliseconds ");
        #endif
        return sequence;
    }

    /// <summary>
    /// Computes the upper envelope of the set of sequences given.
    /// </summary>
    /// <exception cref="ArgumentException">If the set of sequences is empty.</exception>
    public static IReadOnlyList<Element> UpperEnvelope(this IReadOnlyList<Sequence> sequences, ComputationSettings? settings = null)
    {
        if (sequences.Count == 0)
            throw new ArgumentException("The set of sequences is empty.", nameof(sequences));
        if (sequences.Count == 1)
            return sequences.First().Elements;
        else if(sequences.Count == 2)
            return Sequence.Maximum(sequences.First(), sequences.Last(), cutToOverlap: false, settings).Elements;
        else
        {
            settings ??= ComputationSettings.Default();
            bool doParallel = settings.UseParallelListUpperEnvelope && sequences.Count >= settings.ListUpperEnvelopeParallelizationThreshold;
            return BalancedReduction.Aggregate(sequences,
                    (a, b) => Sequence.Maximum(a, b, cutToOverlap: false, settings), doParallel ? settings.DegreeOfParallelism : 1)
                .Elements;
        }
    }
}
