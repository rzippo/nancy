using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
using System.Diagnostics;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Groups segments which are fully defined over a given interval of time.
/// Used in sequence operators to group elements by overlap, so that element operators can be applied straightforwardly.
/// </summary>
internal class Interval
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// The types of overlap that a segment can have with an interval
    /// </summary>
    public enum OverlapTypes
    {
        NoOverlap,
        SegmentStartContained,
        SegmentEndContained,
        SegmentFullyContained,
        SegmentSupportContainsInterval,
        PointInside
    }

    #region Properties

    /// <summary>
    /// Left endpoint of the interval
    /// </summary>
    public Rational Start { get; }

    /// <summary>
    /// Right endpoint of the interval
    /// </summary>
    public Rational End { get; }

    /// <summary>
    /// A set of segments whose support contains this interval
    /// </summary>
    internal List<Element> Elements
        => ComputeElements();

    private List<Element> ElementBag { get; } = new();

    private List<Element> ComputeElements()
    {
        return ElementBag
            .Select(FitElement)
            .ToList();

        Element FitElement(Element item)
        {
            if (IsPointInterval)
            {
                switch (item)
                {
                    case Point p:
                        return p;

                    case Segment s:
                        return s.Sample(Start);

                    default:
                        throw new InvalidCastException();
                }
            }
            else
            {
                switch (item)
                {
                    case Point:
                        throw new InvalidOperationException("A segment interval should not contain a point");

                    case Segment s:
                        return s.Cut(Start, End);

                    default:
                        throw new InvalidCastException();
                }
            }
        }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Constructs a segment interval, open endend
    /// </summary>
    public Interval(Rational start, Rational end)
    {
        Start = start;
        End = end;

        IsPointInterval = Start == End;
    }

    /// <summary>
    /// Constructs a point interval
    /// </summary>
    public Interval(Rational time)
    {
        Start = time;
        End = time;

        IsPointInterval = true;
    }

    #endregion

    #region Computed properties

    /// <summary>
    /// True if the interval is point-sized
    /// </summary>
    public bool IsPointInterval { get; }

    /// <summary>
    /// True if the interval has non-zero length
    /// </summary>
    public bool IsSegmentInterval => !IsPointInterval;

    #endregion

    #region Collection interface methods

    /// <summary>
    /// Adds an element to the interval, doing all validations checks.
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="ArgumentException"></exception>
    internal void Add(Element item)
    {
        if (IsPointInterval)
        {
            if (!item.IsDefinedFor(Start))
                throw new ArgumentException("Element not defined for the point interval");

            ElementBag.Add(item);
        }
        else
        {
            switch (item)
            {
                case Point:
                    throw new ArgumentException("Cannot add a point to a segment interval");

                case Segment s:
                {
                    if (!(s.StartTime <= Start && s.EndTime >= End))
                        throw new ArgumentException("The given segment does not match interval boundaries");

                    ElementBag.Add(s);
                    break;
                }

                default:
                    throw new InvalidCastException();
            }
        }
    }

    /// <summary>
    /// Adds a collection of <see cref="Element"/>s to the interval.
    /// Unless <paramref name="doChecks"/> is <code>true</code>, no validation check is done.  
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="doChecks"></param>
    internal void AddRange(IEnumerable<Element> collection, bool doChecks = false)
    {
        if (doChecks)
            foreach (var item in collection)
                Add(item);
        else
            ElementBag.AddRange(collection);
    }

    /// <summary>
    /// Shortcut for ElementBag.Count
    /// </summary>
    public int Count => ElementBag.Count;

    #endregion

    #region Methods

    /// <summary>
    /// Classifies the element by type of overlap with the interval
    /// </summary>
    public OverlapTypes Classify(Element element)
    {
        switch (element)
        {
            case Point p:
                return ClassifyPoint(p);

            case Segment s:
                return ClassifySegment(s);

            default:
                throw new InvalidCastException();
        }

        OverlapTypes ClassifyPoint(Point p)
        {
            if (IsPointInterval)
            {
                if (p.Time == Start)
                    return OverlapTypes.PointInside;
            }
            else
            {
                if (p.Time > Start && p.Time < End)
                    return OverlapTypes.PointInside;
            }

            return OverlapTypes.NoOverlap;
        }

        OverlapTypes ClassifySegment(Segment s)
        {
            if (IsPointInterval && (s.StartTime == Start || s.EndTime == Start))
                return OverlapTypes.NoOverlap;

            bool isIntervalContainedInSegment = Start >= s.StartTime && End <= s.EndTime;
            if (isIntervalContainedInSegment)
            {
                return OverlapTypes.SegmentSupportContainsInterval;
            }
            else
            {
                bool isStartInside = s.StartTime > Start && s.StartTime < End;
                bool isEndInside = s.EndTime > Start && s.EndTime < End;

                if (isStartInside)
                {
                    return isEndInside ? OverlapTypes.SegmentFullyContained : OverlapTypes.SegmentStartContained;
                }
                else if (isEndInside)
                {
                    return OverlapTypes.SegmentEndContained;
                }
                else return OverlapTypes.NoOverlap;
            }
        }
    }

    /// <summary>
    /// True if there is at least partial overlap between the element support and the interval
    /// </summary>
    public bool OverlapsWith(Element element) => Classify(element) != OverlapTypes.NoOverlap;

    /// <summary>
    /// Splits the interval to maintain the invariant of segments fully contained in open intervals,
    /// interleaved with point intervals containing points and samples
    /// </summary>
    public List<Interval> SplitOver(Segment segment)
    {
        switch (Classify(segment))
        {
            case OverlapTypes.SegmentStartContained:
            {
                Interval beforeSegmentStart = new Interval(Start, segment.StartTime);
                beforeSegmentStart.AddRange(Elements);

                Interval atSegmentStart = new Interval(segment.StartTime);
                atSegmentStart.AddRange(Elements);

                Interval afterSegmentStart = new Interval(segment.StartTime, End);
                afterSegmentStart.AddRange(Elements);
                afterSegmentStart.Add(segment);

                return new List<Interval> {beforeSegmentStart, atSegmentStart, afterSegmentStart};
            }

            case OverlapTypes.SegmentEndContained:
            {
                Interval beforeSegmentEnd = new Interval(Start, segment.EndTime);
                beforeSegmentEnd.AddRange(Elements);
                beforeSegmentEnd.Add(segment);

                Interval atSegmentEnd = new Interval(segment.EndTime);
                atSegmentEnd.AddRange(Elements);

                Interval afterSegmentEnd = new Interval(segment.EndTime, End);
                afterSegmentEnd.AddRange(Elements);

                return new List<Interval> {beforeSegmentEnd, atSegmentEnd, afterSegmentEnd};
            }

            case OverlapTypes.SegmentFullyContained:
            {
                Interval beforeSegmentStart = new Interval(Start, segment.StartTime);
                beforeSegmentStart.AddRange(Elements);

                Interval atSegmentStart = new Interval(segment.StartTime);
                atSegmentStart.AddRange(Elements);

                Interval whileSegmentExists = new Interval(segment.StartTime, segment.EndTime);
                whileSegmentExists.AddRange(Elements);
                whileSegmentExists.Add(segment);

                Interval atSegmentEnd = new Interval(segment.EndTime);
                atSegmentEnd.AddRange(Elements);

                Interval afterSegmentEnd = new Interval(segment.EndTime, End);
                afterSegmentEnd.AddRange(Elements);

                return new List<Interval>
                {
                    beforeSegmentStart,
                    atSegmentStart,
                    whileSegmentExists,
                    atSegmentEnd,
                    afterSegmentEnd
                };
            }

            case OverlapTypes.NoOverlap:
                throw new ArgumentException();
            case OverlapTypes.SegmentSupportContainsInterval:
                throw new ArgumentException();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Splits the interval so that one piece has the given segment in its set,
    /// while maintaining the invariant that each piece is fully contained in its segments' supports.
    /// </summary>
    public List<Interval> SplitOver(Point point)
    {
        switch (Classify(point))
        {
            case OverlapTypes.SegmentFullyContained:
            case OverlapTypes.PointInside:
            {
                Interval beforePoint = new Interval(Start, point.Time);
                beforePoint.AddRange(Elements);

                Interval atPoint = new Interval(point.Time);
                atPoint.AddRange(Elements);
                atPoint.Add(point);

                Interval afterPoint = new Interval(point.Time, End);
                afterPoint.AddRange(Elements);

                return new List<Interval> {beforePoint, atPoint, afterPoint};
            }

            case OverlapTypes.NoOverlap:
                throw new ArgumentException();
            case OverlapTypes.SegmentSupportContainsInterval:
                throw new ArgumentException();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region Static methods

    /// <summary>
    /// Computes the minimal set of intervals needed to group the given elements by overlap and populates those intervals.
    /// General algorithm that assumes no ordering between the elements. Uses a RangeTree, with O(n logn) complexity.
    /// </summary>
    /// <param name="elements">The set of elements to group by overlaps</param>
    /// <param name="settings">Computation settings, to fine-tune algorithm performance</param>
    /// <returns>A set of intervals grouping up the elements by their overlaps</returns>
    public static List<Interval> ComputeIntervals(
        IReadOnlyList<Element> elements,
        ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        if (!elements.Any())
            throw new Exception("Computing interval of no elements");

        #if DO_LOG
        var prebuildStopwatch = Stopwatch.StartNew();
        #endif
        var intervalTree = new IntervalTree(GetPrebuiltIntervals(), settings);
        #if DO_LOG
        prebuildStopwatch.Stop();
        logger.Trace(
            $"Computing intervals for {elements.Count} elements. Pre-building of range tree took {prebuildStopwatch.Elapsed}, allocated {intervalTree.Count} intervals.");
        #endif

        #if DO_LOG
        var processingStopwatch = Stopwatch.StartNew();
        #endif
        bool doParallel = settings.UseParallelComputeIntervals &&
                          elements.Count >= settings.ParallelComputeIntervalsThreshold;
        if (doParallel)
        {
            #if DO_LOG
            logger.Trace($"Using parallel processing algorithm.");
            #endif
            ParallelProcessing();
        }
        else
        {
            #if DO_LOG
            logger.Trace($"Using serial processing algorithm.");
            #endif
            SerialProcessing();
        }
        #if DO_LOG
        processingStopwatch.Stop();
        logger.Trace($"Computed {intervalTree.Count} intervals, took {processingStopwatch.Elapsed}");
        #endif

        #if DO_LOG
        var postprocessStopwatch = Stopwatch.StartNew();
        #endif
        var toRet = intervalTree.Intervals
            .Where(i => i.Count > 0)
            .ToList();
        #if DO_LOG
        postprocessStopwatch.Stop();
        logger.Trace(
            $"Filter intervals took {postprocessStopwatch.Elapsed}. Non-empty intervals: {toRet.Count} out of {intervalTree.Intervals.Count}");
        #endif

        return toRet;

        //Local functions

        List<Interval> GetPrebuiltIntervals()
        {
            var doParallel = settings.UseParallelComputeIntervals &&
                             elements.Count > settings.ParallelComputeIntervalsThreshold;
            if (doParallel)
            {
                //parallel algorithm
                var times = elements
                    .AsParallel()
                    .SelectMany(GetTimes)
                    .GroupBy(t => t)
                    .Select(grp => grp.Key)
                    .ToList();

                var orderedTimes = times
                    .AsParallel()
                    .OrderBy(t => t)
                    .ToList();

                var timePairs = orderedTimes
                    .Zip(orderedTimes.Skip(1), (a, b) => (a, b));

                var prebuiltIntervals = timePairs
                    .AsParallel()
                    .SelectMany(pair => new[]
                    {
                        new Interval(pair.a),
                        new Interval(pair.a, pair.b)
                    })
                    .Append(new Interval(orderedTimes.Last())) //tail interval
                    .ToList();

                return prebuiltIntervals;
            }
            else
            {
                //single thread algorithm
                var times = elements
                    .SelectMany(GetTimes)
                    .GroupBy(t => t)
                    .Select(grp => grp.First())
                    .ToList();

                var orderedTimes = times
                    .OrderBy(t => t)
                    .ToList();

                var timePairs = Enumerable
                    .Zip(orderedTimes, orderedTimes.Skip(1), (a, b) => (a, b));

                var prebuiltIntervals = timePairs
                    .SelectMany(pair => new[]
                    {
                        new Interval(pair.a),
                        new Interval(pair.a, pair.b)
                    })
                    .Append(new Interval(orderedTimes.Last())) //tail interval
                    .ToList();

                return prebuiltIntervals;
            }

            IEnumerable<Rational> GetTimes(Element e)
            {
                switch (e)
                {
                    case Point p:
                    {
                        yield return p.Time;
                        yield break;
                    }

                    case Segment s:
                    {
                        yield return s.StartTime;
                        yield return e.EndTime;
                        yield break;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }
        }

        void SerialProcessing()
        {
            foreach (var element in elements)
            {
                ProcessElement(element);
            }

            // Finds the overlapping intervals and adds the element to them.
            void ProcessElement(Element element)
            {
                switch (element)
                {
                    case Point p:
                    {
                        var interval = intervalTree.Query(p.Time);

                        if (interval == null)
                            throw new InvalidOperationException("No interval found for point");

                        if (!interval.IsPointInterval)
                            throw new InvalidOperationException("Interval for point was a segment interval");

                        interval.Add(p);
                        return;
                    }

                    case Segment s:
                    {
                        var intervals = intervalTree
                            .Query(s.StartTime, s.EndTime);

                        using var enumerator = intervals.GetEnumerator();

                        if (!enumerator.MoveNext())
                            throw new InvalidOperationException("No interval found for segment");

                        Rational expectedStart = s.StartTime;
                        bool lastIntervalWasPoint = true;

                        do 
                        {
                            var interval = enumerator.Current;
                            if (interval.Start > expectedStart)
                                throw new InvalidOperationException("Found gap between intervals for segment");

                            if (lastIntervalWasPoint == interval.IsPointInterval)
                                throw new InvalidOperationException("Invalid sequence of intervals");

                            if (interval.Classify(s) != OverlapTypes.SegmentSupportContainsInterval)
                                throw new InvalidOperationException("Interval too large for segment");

                            interval.Add(s);
                            expectedStart = interval.End;
                            lastIntervalWasPoint = interval.IsPointInterval;
                        } 
                        while (enumerator.MoveNext());

                        if (expectedStart != s.EndTime)
                            throw new InvalidOperationException("Found gap at tail of intervals for segment");

                        return;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }
        }

        void ParallelProcessing()
        {
            var overlaps = elements
                .AsParallel()
                .SelectMany(GetOverlaps);

            if (settings.UseParallelInsertionComputeIntervals)
            {
                var intervalGroups = overlaps
                    .GroupBy(ov => ov.interval);

                var intervalElementsPairs = intervalGroups
                    .AsParallel()
                    .Select(pair => 
                        (interval: pair.Key, elements: pair.Select(overlap => overlap.element)));

                intervalElementsPairs
                    .ForAll(pair => pair.interval.AddRange(pair.elements));
            }
            else
            {
                foreach(var overlap in overlaps)
                    overlap.interval.Add(overlap.element);
            }

            // foreach (var (element, intervals) in overlaps)
            //     ProcessOverlap(element, intervals);

            //Queries the range tree
            //As the index is already built, read-only operations are thread safe.
            IEnumerable<(Element element, Interval interval)> GetOverlaps(Element element)
            {
                switch (element)
                {
                    case Point p:
                    {
                        var interval = intervalTree.Query(p.Time);
                        if (interval != null)
                        {
                            yield return (p, interval);
                            yield break;
                        }
                        else
                            throw new InvalidOperationException("No point interval found");
                    }

                    case Segment s:
                    {
                        var intervals = intervalTree
                            .Query(s.StartTime, s.EndTime);

                        foreach (var interval in intervals)
                            yield return (s, interval);
                        yield break;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }
        }
    }

    /// <summary>
    /// Computes the minimal set of intervals needed to group the given elements by overlap and populates those intervals.
    /// Optimized algorithm for a sequence pair. Uses existing ordering within sequences to achieve O(n) complexity.
    /// </summary>
    /// <param name="a">First sequence</param>
    /// <param name="b">Second sequence</param>
    /// <returns>A set of intervals grouping up the elements of the sequences by their overlaps</returns>
    public static List<Interval> ComputeIntervals(Sequence a, Sequence b)
    {
        #if DO_LOG
        logger.Trace($"Start: linear compute intervals, sequences of lengths {a.Count} and {b.Count}");
        var stopwatch = Stopwatch.StartNew();
        #endif

        var intervals = IntervalsIterator().ToList();

        #if DO_LOG
        stopwatch.Stop();
        logger.Trace(
            $"Done: linear compute intervals, {intervals.Count} intervals computed in {stopwatch.ElapsedMilliseconds} milliseconds");
        #endif

        return intervals;

        IEnumerable<Interval> IntervalsIterator()
        {
            Interval? currentInterval = null;

            foreach (var element in ElementsIterator(a, b))
            {
                switch (element)
                {
                    case Point p:
                    {
                        currentInterval ??= new Interval(p.Time);

                        if (currentInterval.IsPointInterval && currentInterval.Start == p.Time)
                        {
                            currentInterval.Add(p);
                            continue;
                        }
                        else
                        {
                            yield return currentInterval;
                            currentInterval = new Interval(p.Time);
                            currentInterval.Add(p);
                            continue;
                        }
                    }

                    case Segment s:
                    {
                        currentInterval ??= new Interval(s.StartTime, s.EndTime);

                        if (currentInterval.IsSegmentInterval &&
                            currentInterval.Start == s.StartTime &&
                            currentInterval.End == s.EndTime)
                        {
                            currentInterval.Add(s);
                            continue;
                        }
                        else
                        {
                            yield return currentInterval;
                            currentInterval = new Interval(s.StartTime, s.EndTime);
                            currentInterval.Add(s);
                            continue;
                        }
                    }
                }
            }

            yield return currentInterval!;
        }
    }

    internal static IEnumerable<Element> ElementsIterator(Sequence a, Sequence b)
    {
        int indexA = 0;
        int indexB = 0;

        //if other than -inf, the top element is being cut
        Rational currentTime = Rational.MinusInfinity;

        while (indexA < a.Count || indexB < b.Count)
        {
            var nextA = a.Elements.ElementAtOrDefault(indexA);
            var nextB = b.Elements.ElementAtOrDefault(indexB);

            {
                if (nextA is Segment sA && sA.IsDefinedFor(currentTime))
                {
                    var (_, _, right) = sA.Split(currentTime);
                    nextA = right;
                }

                if (nextB is Segment sB && sB.IsDefinedFor(currentTime))
                {
                    var (_, _, right) = sB.Split(currentTime);
                    nextB = right;
                }
            }

            if (nextA == null)
            {
                yield return nextB!;
                indexB++;
                continue;
            }

            if (nextB == null)
            {
                yield return nextA;
                indexA++;
                continue;
            }

            if (nextA.StartTime < nextB.StartTime)
            {
                if (nextA is Point pA)
                {
                    yield return pA;
                    indexA++;
                    continue;
                }
                else
                {
                    var sA = (Segment) nextA;
                    if (sA.EndTime <= nextB.StartTime)
                    {
                        yield return sA;
                        indexA++;
                        continue;
                    }
                    else
                    {
                        var (left, point, _) = sA.Split(nextB.StartTime);
                        yield return left;
                        yield return point;
                        currentTime = nextB.StartTime;
                        continue;
                    }
                }
            }
            else if (nextA.StartTime > nextB.StartTime)
            {
                if (nextB is Point pB)
                {
                    yield return pB;
                    indexB++;
                    continue;
                }
                else
                {
                    var sB = (Segment) nextB;
                    if (sB.EndTime <= nextA.StartTime)
                    {
                        yield return sB;
                        indexB++;
                        continue;
                    }
                    else
                    {
                        var (left, point, _) = sB.Split(nextA.StartTime);
                        yield return left;
                        yield return point;
                        currentTime = nextA.StartTime;
                        continue;
                    }
                }
            }
            else
            {
                //nextA.StartTime == nextB.StartTime
                if (nextA is Point pA)
                {
                    yield return pA;
                    indexA++;
                    continue;
                }

                if (nextB is Point pB)
                {
                    yield return pB;
                    indexB++;
                    continue;
                }

                var sA = (Segment) nextA;
                var sB = (Segment) nextB;

                if (sA.EndTime == sB.EndTime)
                {
                    yield return sA;
                    yield return sB;
                    indexA++;
                    indexB++;
                    continue;
                }
                else
                {
                    if (sA.EndTime > sB.EndTime)
                    {
                        var (left, point, _) = sA.Split(sB.EndTime);
                        yield return sB;
                        yield return left;
                        yield return point;
                        indexB++;
                        currentTime = sB.EndTime;
                        continue;
                    }
                    else
                    {
                        var (left, point, _) = sB.Split(sA.EndTime);
                        yield return sA;
                        yield return left;
                        yield return point;
                        indexA++;
                        currentTime = sA.EndTime;
                        continue;
                    }
                }
            }
        }
    }

    #endregion

    #region Envelopes

    /// <summary>
    /// Computes the interval lower envelope.
    /// </summary>
    /// <returns>The lower envelope of the elements contained in the interval.</returns>
    /// <remarks>This method is similar to Element.Minimum, but it skips many checks due to hypotheses given by the interval.</remarks>
    public List<Element> LowerEnvelope()
    {
        if (IsPointInterval)
        {
            var minValue = ElementBag
                .Select(e => e.ValueAt(Start))
                .Min();

            return new List<Element>
            {
                new Point(
                    time: Start,
                    value: minValue
                )
            };
        }
        else
        {
            var segments = ElementBag
                .Select(e => (Segment)e)
                .ToList();

            if(segments.Any(s => s.IsMinusInfinite))
                return new List<Element> { Segment.MinusInfinite(this.Start, this.End) };

            if (segments.All(s => s.IsPlusInfinite))
                return new List<Element> { Segment.PlusInfinite(this.Start, this.End) };

            segments = segments
                .Where(e => e.IsFinite)
                .GroupBy(s => s.Slope)
                .OrderByDescending(g => g.Key)
                .Select(SelectLowest)
                .ToList();

            //Each single segment is cast to a collection of elements with one item
            //This is to use a single merge algorithm
            var lowerEnvelopes = segments
                .Select(s => new List<Element> {s.Cut(Start, End)})
                .ToList();

            var lowerEnvelope = LowerEnvelope(lowerEnvelopes);
            return lowerEnvelope;

            Segment SelectLowest(IEnumerable<Segment> segments)
            {
                return segments.MinBy(s => s.RightLimitAt(Start))!;
            }
        }
    }

    //This is a single thread divide and conquer implementation
    //Simpler to write and debug, should be rewrote with parallelism later
    internal static List<Element> LowerEnvelope(List<List<Element>> envelopes)
    {
        switch (envelopes.Count)
        {
            case 1:
            {
                return envelopes.Single();
            }

            case 2:
            {
                var a = envelopes.First();
                var b = envelopes.Last();
                return Conquer(a, b);
            }

            default:
            {
                //Divide
                var halfCount = envelopes.Count / 2;
                var left = envelopes
                    .Take(halfCount)
                    .ToList();
                var right = envelopes
                    .Skip(halfCount)
                    .ToList();

                var leftEnvelope = LowerEnvelope(left);
                var rightEnvelope = LowerEnvelope(right);

                return Conquer(leftEnvelope, rightEnvelope);
            }
        }

        //https://www.youtube.com/watch?v=n_W16PLVrE8
        List<Element> Conquer(List<Element> higherSlopes, List<Element> lowerSlopes)
        {
            Rational pStar = Rational.MinusInfinity;
            int higherSlopeIdx = higherSlopes.Count - 1;
            int lowerSlopeIdx = lowerSlopes.Count - 1;
            bool doSearch = true;

            while (doSearch && higherSlopeIdx >= 0 && lowerSlopeIdx >= 0)
            {
                var higherSlopeElement = higherSlopes[higherSlopeIdx]; //lE in short
                var lowerSlopeElement = lowerSlopes[lowerSlopeIdx]; //hE in short

                switch (lowerSlopeElement)
                {
                    case Point lP:
                    {
                        switch (higherSlopeElement)
                        {
                            case Point hP:
                            {
                                if (lP.Time == hP.Time)
                                {
                                    if (lP.Value == hP.Value)
                                    {
                                        //intersection found
                                        pStar = lP.Time;
                                        doSearch = false;
                                        break;
                                    }
                                    else
                                    {
                                        //no intersection found
                                        //as lE is a point, it's no use to keep it further
                                        //advance both lE and hE
                                        lowerSlopeIdx--;
                                        higherSlopeIdx--;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (lP.Time > hP.Time)
                                        throw new InvalidOperationException("Should not be here");

                                    //advance hE
                                    higherSlopeIdx--;
                                    continue;
                                }
                            }

                            case Segment hS:
                            {
                                if (hS.IsDefinedFor(lP.Time))
                                {
                                    if (hS.ValueAt(lP.Time) == lP.Value)
                                    {
                                        //intersection found
                                        pStar = lP.Time;
                                        doSearch = false;
                                        break;
                                    }
                                    else
                                    {
                                        //no intersection found
                                        //as lE is a point, it's no use to keep it further
                                        //advance lE
                                        lowerSlopeIdx--;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (lP.Time > hS.StartTime)
                                        throw new InvalidOperationException("Should not be here");

                                    //advance hE
                                    higherSlopeIdx--;
                                    continue;
                                }
                            }

                            default:
                                throw new InvalidCastException();
                        }

                        break;
                    }

                    case Segment lS:
                    {
                        switch (higherSlopeElement)
                        {
                            case Point hP:
                            {
                                if (!lS.IsDefinedFor(hP.Time))
                                    throw new InvalidOperationException("Should not be here");

                                if (lS.ValueAt(hP.Time) == hP.Value)
                                {
                                    //intersection found
                                    pStar = hP.Time;
                                    doSearch = false;
                                    break;
                                }
                                else
                                {
                                    //no intersection found
                                    //advance hE
                                    higherSlopeIdx--;
                                    continue;
                                }
                            }

                            case Segment hS:
                            {
                                var intersection = Segment.GetIntersection(lS, hS);
                                if (intersection != null)
                                {
                                    //intersection found
                                    pStar = intersection.Time;
                                    doSearch = false;
                                    break;
                                }
                                else
                                {
                                    if (hS.StartTime <= lS.StartTime)
                                    {
                                        //advance lE
                                        lowerSlopeIdx--;
                                        continue;
                                    }
                                    else
                                    {
                                        //advance hE
                                        higherSlopeIdx--;
                                        continue;
                                    }
                                }
                            }

                            default:
                                throw new InvalidCastException();
                        }

                        break;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }

            if (pStar == Rational.MinusInfinity)
            {
                //no intersection has been found, one is always above the other
                //distinguish based on start value

                var lowerSlopeStartValue = ((Segment)lowerSlopes.First()).RightLimitAtStartTime;
                var higherSlopeStartValue = ((Segment)higherSlopes.First()).RightLimitAtStartTime;

                if (lowerSlopeStartValue <= higherSlopeStartValue)
                {
                    //Mind that, if start value is equal, we already know the relation between slopes i.e. lowerSlopes will be below
                    return lowerSlopes;
                }
                else
                {
                    return higherSlopes;
                }
            }
            else
            {
                //intersection has been found
                //result is constructed with higher slopes up to pStar, then lower slopes

                var elements = new List<Element>();

                //higher slopes first. Loop is exited when pStar is reached
                foreach (var element in higherSlopes)
                {
                    if (element is Segment s)
                    {
                        if (s.IsDefinedFor(pStar))
                        {
                            //segment crosses the intersection, must add until that point
                            var (segment, point, _) = s.Split(pStar);
                            elements.Add(segment);
                            elements.Add(point);
                            break;
                        }
                        else
                        {
                            //segment does not cross the intersection, add as a whole
                            elements.Add(s);
                        }
                    }
                    else
                    {
                        var p = (Point) element;
                        elements.Add(p);

                        //if point is on the intersection, exit the loop
                        if (p.Time == pStar)
                            break;
                    }
                }

                //lower slopes second. Elements before pStar must be skipped
                foreach (var element in lowerSlopes)
                {
                    switch (element)
                    {
                        case Point p:
                        {
                            if (p.Time > pStar)
                                elements.Add(p);

                            break;
                        }

                        case Segment s:
                        {
                            if (s.EndTime > pStar)
                            {
                                if (s.IsDefinedFor(pStar))
                                {
                                    var (_, _, right) = s.Split(pStar);
                                    elements.Add(right);
                                }
                                else
                                {
                                    elements.Add(s);
                                }
                            }

                            break;
                        }

                        default:
                            throw new InvalidCastException();
                    }
                }

                return elements;
            }
        }
    }

    /// <summary>
    /// Computes the interval upper envelope.
    /// </summary>
    /// <returns>The upper envelope of the elements contained in the interval.</returns>
    /// <remarks>This method is similar to Element.Maximum, but it skips many checks due to hypotheses given by the interval.</remarks>
    public List<Element> UpperEnvelope()
    {
        if (IsPointInterval)
        {
            var maxValue = ElementBag
                .Select(e => e.ValueAt(Start))
                .Max();

            return new List<Element>
            {
                new Point(
                    time: Start,
                    value: maxValue
                )
            };
        }
        else
        {
            var segments = ElementBag
                .Select(e => (Segment)e)
                .ToList();

            if (segments.Any(s => s.IsPlusInfinite))
                return new List<Element> { Segment.PlusInfinite(this.Start, this.End) };

            if (segments.All(s => s.IsMinusInfinite))
                return new List<Element> { Segment.MinusInfinite(this.Start, this.End) };

            segments = segments
                .Where(e => e.IsFinite)
                .GroupBy(s => s.Slope)
                .OrderBy(g => g.Key)
                .Select(SelectHighest)
                .ToList();

            //Each single segment is cast to a collection of elements with one item
            //This is to use a single merge algorithm
            var upperEnvelopes = segments
                .Select(s => new List<Element> {s.Cut(Start, End)})
                .ToList();

            var upperEnvelope = UpperEnvelope(upperEnvelopes);
            return upperEnvelope;

            Segment SelectHighest(IEnumerable<Segment> segments)
            {
                return segments.MaxBy(s => s.RightLimitAt(Start))!;
            }
        }
    }

    //This is a single thread divide and conquer implementation
    //Simpler to write and debug, should be rewrote with parallelism later
    internal static List<Element> UpperEnvelope(List<List<Element>> envelopes)
    {
        switch (envelopes.Count)
        {
            case 1:
            {
                return envelopes.Single();
            }

            case 2:
            {
                var a = envelopes.First();
                var b = envelopes.Last();
                return Conquer(a, b);
            }

            default:
            {
                //Divide
                var halfCount = envelopes.Count / 2;
                var left = envelopes
                    .Take(halfCount)
                    .ToList();
                var right = envelopes
                    .Skip(halfCount)
                    .ToList();

                var leftEnvelope = UpperEnvelope(left);
                var rightEnvelope = UpperEnvelope(right);

                return Conquer(leftEnvelope, rightEnvelope);
            }
        }

        //https://www.youtube.com/watch?v=n_W16PLVrE8 but reversed
        List<Element> Conquer(List<Element> lowerSlopes, List<Element> higherSlopes)
        {
            Rational pStar = Rational.MinusInfinity;
            int higherSlopeIdx = 0;
            int lowerSlopeIdx = 0;
            bool doSearch = true;

            while (doSearch && higherSlopeIdx < higherSlopes.Count && lowerSlopeIdx < lowerSlopes.Count)
            {
                var higherSlopeElement = higherSlopes[higherSlopeIdx]; //lE in short
                var lowerSlopeElement = lowerSlopes[lowerSlopeIdx]; //hE in short

                switch (lowerSlopeElement)
                {
                    case Point lP:
                    {
                        switch (higherSlopeElement)
                        {
                            case Point hP:
                            {
                                if (lP.Time == hP.Time)
                                {
                                    if (lP.Value == hP.Value)
                                    {
                                        //intersection found
                                        pStar = lP.Time;
                                        doSearch = false;
                                        break;
                                    }
                                    else
                                    {
                                        //no intersection found
                                        //as lE is a point, it's no use to keep it further
                                        //advance both lE and hE
                                        lowerSlopeIdx++;
                                        higherSlopeIdx++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (lP.Time > hP.Time)
                                        throw new InvalidOperationException("Should not be here");

                                    //advance hE
                                    higherSlopeIdx++;
                                    continue;
                                }
                            }

                            case Segment hS:
                            {
                                if (hS.IsDefinedFor(lP.Time))
                                {
                                    if (hS.ValueAt(lP.Time) == lP.Value)
                                    {
                                        //intersection found
                                        pStar = lP.Time;
                                        doSearch = false;
                                        break;
                                    }
                                    else
                                    {
                                        //no intersection found
                                        //as lE is a point, it's no use to keep it further
                                        //advance lE
                                        lowerSlopeIdx++;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (lP.Time < hS.StartTime)
                                        throw new InvalidOperationException("Should not be here");

                                    //advance hE
                                    higherSlopeIdx++;
                                    continue;
                                }
                            }

                            default:
                                throw new InvalidCastException();
                        }

                        break;
                    }

                    case Segment lS:
                    {
                        switch (higherSlopeElement)
                        {
                            case Point hP:
                            {
                                if (!lS.IsDefinedFor(hP.Time))
                                    throw new InvalidOperationException("Should not be here");

                                if (lS.ValueAt(hP.Time) == hP.Value)
                                {
                                    //intersection found
                                    pStar = hP.Time;
                                    doSearch = false;
                                    break;
                                }
                                else
                                {
                                    //no intersection found
                                    //advance hE
                                    higherSlopeIdx++;
                                    continue;
                                }
                            }

                            case Segment hS:
                            {
                                var intersection = Segment.GetIntersection(lS, hS);
                                if (intersection != null)
                                {
                                    //intersection found
                                    pStar = intersection.Time;
                                    doSearch = false;
                                    break;
                                }
                                else
                                {
                                    if (hS.EndTime >= lS.EndTime)
                                    {
                                        //advance lE
                                        lowerSlopeIdx++;
                                        continue;
                                    }
                                    else
                                    {
                                        //advance hE
                                        higherSlopeIdx++;
                                        continue;
                                    }
                                }
                            }

                            default:
                                throw new InvalidCastException();
                        }

                        break;
                    }

                    default:
                        throw new InvalidCastException();
                }
            }

            if (pStar == Rational.MinusInfinity)
            {
                //no intersection has been found, one is always above the other
                //distinguish based on start value

                var lowerSlopeStartValue = ((Segment)lowerSlopes.First()).RightLimitAtStartTime;
                var higherSlopeStartValue = ((Segment)higherSlopes.First()).RightLimitAtStartTime;

                if (higherSlopeStartValue >= lowerSlopeStartValue)
                {
                    //Mind that, if start value is equal, we already know the relation between slopes i.e. higherSlopes will be above
                    return higherSlopes;
                }
                else
                {
                    return lowerSlopes;
                }
            }
            else
            {
                //intersection has been found
                //result is constructed with lower slopes up to pStar, then higher slopes

                var elements = new List<Element>();

                //lower slopes first. Loop is exited when pStar is reached
                foreach (var element in lowerSlopes)
                {
                    if (element is Segment s)
                    {
                        if (s.IsDefinedFor(pStar))
                        {
                            //segment crosses the intersection, must add until that point
                            var (segment, point, _) = s.Split(pStar);
                            elements.Add(segment);
                            elements.Add(point);
                            break;
                        }
                        else
                        {
                            //segment does not cross the intersection, add as a whole
                            elements.Add(s);
                        }
                    }
                    else
                    {
                        var p = (Point)element;
                        elements.Add(p);

                        //if point is on the intersection, exit the loop
                        if (p.Time == pStar)
                            break;
                    }
                }

                //higher slopes second. Elements before pStar must be skipped
                foreach (var element in higherSlopes)
                {
                    switch (element)
                    {
                        case Point p:
                        {
                            if (p.Time > pStar)
                                elements.Add(p);

                            break;
                        }

                        case Segment s:
                        {
                            if (s.EndTime > pStar)
                            {
                                if (s.IsDefinedFor(pStar))
                                {
                                    var (_, _, right) = s.Split(pStar);
                                    elements.Add(right);
                                }
                                else
                                {
                                    elements.Add(s);
                                }
                            }

                            break;
                        }

                        default:
                            throw new InvalidCastException();
                    }
                }

                return elements;
            }
        }
    }

    #endregion Envelopes
}

/// <summary>
/// Provides LINQ extensions methods for <see cref="Interval"/>.
/// </summary>
internal static class IntervalExtensions
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Checks if time order is respected, i.e. they are ordered first by start, then by end
    /// </summary>
    internal static bool AreInTimeOrder(this IEnumerable<Interval> intervals)
    {
        var lastStartTime = Rational.MinusInfinity;
        var lastEndTime = Rational.MinusInfinity;

        foreach (var interval in intervals)
        {
            if (interval.Start >= lastStartTime)
            {
                if (interval.Start == lastStartTime)
                {
                    if (interval.End >= lastEndTime)
                    {
                        lastStartTime = interval.Start;
                        lastEndTime = interval.End;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    lastStartTime = interval.Start;
                    lastEndTime = interval.End;
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
    /// Checks if the intervals form a sequence.
    /// In addition to <see cref="AreInTimeOrder(IEnumerable{Interval})"/> it requires non-overlapping, but it allows gaps.
    /// </summary>
    internal static bool AreInTimeSequence(this IEnumerable<Interval> intervals)
    {
        var nextExpectedTime = Rational.MinusInfinity;
        foreach (var interval in intervals)
        {
            if (interval.Start >= nextExpectedTime)
            {
                nextExpectedTime = interval.End;
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
    /// Sorts the intervals in time order
    /// </summary>
    internal static IReadOnlyList<Interval> SortIntervals(this IReadOnlyList<Interval> intervals, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        const int ParallelizationThreshold = 10_000;

        #if DO_LOG
        var sortStopwatch = Stopwatch.StartNew();
        #endif

        if (intervals.AreInTimeOrder())
        {
            #if DO_LOG
            sortStopwatch.Stop();
            logger.Trace($"SortIntervals: took {sortStopwatch.Elapsed}, already sorted");
            #endif
            return intervals;
        }
        else
        {
            var doParallel = settings.UseParallelComputeIntervals && intervals.Count >= ParallelizationThreshold;
            List<Interval> sorted;
            if (doParallel)
            {
                sorted = intervals
                    .AsParallel()
                    .OrderBy(i => i.Start)
                    .ThenBy(i => i.End)
                    .ToList();
            }
            else
            {
                sorted = intervals
                    .OrderBy(i => i.Start)
                    .ThenBy(i => i.End)
                    .ToList();
            }

            #if DO_LOG
            sortStopwatch.Stop();
            #endif
            var alg = doParallel ? "parallel" : "serial";
            #if DO_LOG
            logger.Trace($"SortIntervals: took {sortStopwatch.Elapsed}, {alg} sort");
            #endif
            return sorted;
        }
    }

} 