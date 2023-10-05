using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
using System.Diagnostics;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// A piecewise affine function with a limited support, defined between <see cref="DefinedFrom"/> and <see cref="DefinedUntil"/>.
/// Both ends can be either inclusive or exclusive.
/// </summary>
/// <docs position="2"/>
[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public sealed class Sequence : IEquatable<Sequence>, IToCodeString
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    #region Properties

    /// <summary>
    /// Set of elements composing the sequence.
    /// </summary>
    [JsonProperty(PropertyName = "elements")]
    public ReadOnlyCollection<Element> Elements { get; }

    /// <summary>
    /// Left endpoint of the support of the sequence.
    /// Can be either inclusive or exclusive, see <see cref="IsLeftClosed"/> or <see cref="IsLeftOpen"/>.
    /// </summary>
    public Rational DefinedFrom => Elements.First().StartTime;

    /// <summary>
    /// Right endpoint of the support of the sequence.
    /// Can be either inclusive or exclusive, see <see cref="IsRightClosed"/> or <see cref="IsRightOpen"/>.
    /// </summary>
    public Rational DefinedUntil => Elements.Last().EndTime;

    /// <summary>
    /// Length of the support of the sequence.
    /// </summary>
    public Rational Length => DefinedUntil - DefinedFrom;

    /// <summary>
    /// Number of points and segments composing the sequence.
    /// </summary>
    public int Count => Elements.Count;

    /// <summary>
    /// True if all elements of the sequence are finite.
    /// </summary>
    public bool IsFinite => Elements.All(e => e.IsFinite);

    /// <summary>
    /// True if all elements of the sequence are infinite.
    /// </summary>
    public bool IsInfinite => Elements.All(e => e.IsInfinite);

    /// <summary>
    /// True if all elements of the sequence are plus infinite.
    /// </summary>
    public bool IsPlusInfinite => Elements.All(e => e.IsPlusInfinite);

    /// <summary>
    /// True if all elements of the sequence are minus infinite.
    /// </summary>
    public bool IsMinusInfinite => Elements.All(e => e.IsMinusInfinite);

    /// <summary>
    /// The first instant around which the sequence is not infinite.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is finite.
    /// </summary>
    public Rational FirstFiniteTime => 
        Elements.FirstOrDefault(e => e.IsFinite)?.StartTime ?? Rational.PlusInfinity;

    /// <summary>
    /// The first instant after which the sequence is not infinite, starting from the right of <paramref name="time"/>.
    /// </summary>
    public Rational FirstFiniteTimeAfter(Rational time) => 
        Elements.FirstOrDefault(e => 
            (e.StartTime > time || (e.StartTime == time && e is not Point)) && e.IsFinite
        )?.StartTime ?? Rational.PlusInfinity;

    /// <summary>
    /// The first instant around which the sequence is not finite.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is infinite.
    /// </summary>
    public Rational FirstInfiniteTime =>
        Elements.FirstOrDefault(e => e.IsInfinite)?.StartTime ?? Rational.PlusInfinity;

    /// <summary>
    /// The first instant around which the curve is not 0.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is 0.
    /// </summary>
    public Rational FirstNonZeroTime =>
        Elements.FirstOrDefault(e => !e.IsZero)?.StartTime ?? Rational.PlusInfinity;

    /// <summary>
    /// True if the sequence is 0 for all $t$.
    /// </summary>
    public bool IsZero =>
        Elements.All(e => e.IsZero);

    /// <summary>
    /// True if there is no discontinuity within the sequence.
    /// </summary>
    public bool IsContinuous
    {
        get
        {
            Rational lastValue = Elements.First() is Point ? 
                ((Point)Elements.First()).Value :
                ((Segment)Elements.First()).LeftLimitAtEndTime;

            foreach (Element element in Elements.Skip(1))
            {
                switch(element)
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
    }

    /// <summary>
    /// True if there is left-discontinuity within the sequence.
    /// </summary>
    public bool IsLeftContinuous
        => _isLeftContinuous ??= TestIsLeftContinuous();

    private bool TestIsLeftContinuous()
        {
            foreach(var breakpoint in this.EnumerateBreakpoints())
            {
                if (breakpoint.left is { } s &&
                    s.LeftLimitAtEndTime != breakpoint.center.Value)
                    return false;
            }

            return true;
        }

    /// <summary>
    /// Private cache field for <see cref="IsLeftContinuous"/>
    /// </summary>
    internal bool? _isLeftContinuous;


    /// <summary>
    /// True if there is right-discontinuity within the sequence.
    /// </summary>
    public bool IsRightContinuous
        => _isRightContinuous ??= TestIsRightContinuous();

    private bool TestIsRightContinuous()
        {
            foreach (var breakpoint in this.EnumerateBreakpoints())
            {
                if (breakpoint.right is { } s &&
                    s.RightLimitAtStartTime != breakpoint.center.Value)
                    return false;
            }

            return true;
        }

    /// <summary>
    /// Private cache field for <see cref="IsRightContinuous"/>
    /// </summary>
    internal bool? _isRightContinuous;

    /// <summary>
    /// True if the sequence is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsContinuousAt(Rational time)
    {
        if (time == DefinedFrom)
        {
            if (IsLeftClosed)
                return RightLimitAt(time) == ValueAt(time);
            else
                return true;
        }
        else if (time == DefinedUntil)
        {
            if (IsRightClosed)
                return LeftLimitAt(time) == ValueAt(time);
            else
                return true;
        }
        else
        {
            return LeftLimitAt(time) == ValueAt(time) &&
                   RightLimitAt(time) == ValueAt(time);
        }
    }

    /// <summary>
    /// True if the sequence is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsLeftContinuousAt(Rational time)
    {
        if(time == DefinedUntil && !IsRightClosed)
            return true;
        else
            return LeftLimitAt(time) == ValueAt(time);
    }

    /// <summary>
    /// True if the sequence is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsRightContinuousAt(Rational time)
    { 
        if(time == DefinedFrom && !IsLeftClosed)
            return true;
        else
            return RightLimitAt(time) == ValueAt(time);
    } 

    /// <summary>
    /// True if the sequence is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    public bool IsNonNegative
        => MinValue() >= 0;

    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    public bool IsNonDecreasing
        => _isNonDecreasing ??= TestIsNonDecreasing();

    private bool TestIsNonDecreasing()
        {
            foreach (var breakpoint in this.EnumerateBreakpoints())
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
    /// Private cache field for <see cref="IsNonDecreasing"/>
    /// </summary>
    internal bool? _isNonDecreasing;

    /// <summary>
    /// True if <see cref="DefinedFrom"/> is exclusive.
    /// </summary>
    public bool IsLeftOpen => Elements.First() is Segment;

    /// <summary>
    /// True if <see cref="DefinedFrom"/> is inclusive.
    /// </summary>
    public bool IsLeftClosed => !IsLeftOpen;

    /// <summary>
    /// True if <see cref="DefinedUntil"/> is exclusive.
    /// </summary>
    public bool IsRightOpen => Elements.Last() is Segment;

    /// <summary>
    /// True if <see cref="DefinedUntil"/> is inclusive.
    /// </summary>
    public bool IsRightClosed => !IsRightOpen;

    /// <summary>
    /// True if the sequence consists of only a <see cref="Point"/>, meaning it is has 0 time length.
    /// </summary>
    public bool IsPoint => Elements.Count == 1 && Elements.Single() is Point;

    /// <summary>
    /// True if the the sequence starts with a plateau.
    /// </summary>
    public bool StartsWithPlateau
    {
        get
        {
            if (Elements.First() is Point p)
            {
                var s = (Segment) Elements.First(e => e is Segment);
                return s.IsConstant && s.RightLimitAtStartTime == p.Value;
            }
            else if (Elements.First() is Segment s)
            {
                return s.IsConstant;
            }
            else
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// True if the sequence ends with a plateau.
    /// </summary>
    public bool EndsWithPlateau
    {
        get
        {
            if (Elements.Last() is Point p)
            {
                var s = (Segment) Elements.Last(e => e is Segment);
                return s.IsConstant && s.LeftLimitAtEndTime == p.Value;
            }
            else if (Elements.Last() is Segment s)
            {
                return s.IsConstant;
            }
            else
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// Returns the end time of the plateau at the start of the sequence,
    /// or the sequence start if there is no such plateau.
    /// </summary>
    public Rational FirstPlateauEnd
    {
        get 
        {
            if(StartsWithPlateau)
                return Elements
                    .MergeAsEnumerable()
                    .GetSegmentAfter(DefinedFrom)
                    .EndTime;
            else
                return DefinedFrom;
        }
    }

    /// <summary>
    /// Returns the start time of the plateau at the end of the sequence,
    /// or the sequence end if there is no such plateau.
    /// </summary>
    public Rational LastPlateauStart
    {
        get 
        {
            if(EndsWithPlateau)
                return Elements
                    .MergeAsEnumerable()
                    .GetSegmentBefore(DefinedUntil)
                    .StartTime;
            else
                return DefinedUntil;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="elements">Set of elements composing the sequence. Must be in uninterrupted order.</param>
    [JsonConstructor]
    public Sequence(IEnumerable<Element> elements)
    {
        var list = elements.ToList();
        if (!list.AreUninterruptedSequence())
            throw new ArgumentException("Elements are not uninterrupted");

        Elements = new ReadOnlyCollection<Element>(list);
    }

    /// <summary>
    /// Constructor.
    /// Fills the gaps within [fillFrom, fillTo[ with the given value, defaults to $+\infty$.
    /// </summary>
    /// <param name="elements">Partial set of elements composing the sequence. Must be ordered, but can have gaps.</param>
    /// <param name="fillFrom">Left inclusive endpoint of the filling interval.</param>
    /// <param name="fillTo">Right exclusive endpoint of the filling interval.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    public Sequence(IEnumerable<Element> elements, Rational fillFrom, Rational fillTo, Rational? fillWith = null)
    {
        var filledElements = elements
            .Fill(fillFrom, fillTo, true, false, fillWith)
            .ToList();
        if (!filledElements.AreUninterruptedSequence())
            throw new ArgumentException("Elements are not uninterrupted after filling: malformed input");

        Elements = new ReadOnlyCollection<Element>(filledElements);
    }

    /// <summary>
    /// Constructs a sequence that is 0 between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">Left endpoint of the sequence</param>
    /// <param name="to">Right endpoint of the sequence</param>
    /// <param name="isStartIncluded"></param>
    /// <param name="isEndIncluded"></param>
    /// <returns></returns>
    public static Sequence Zero(Rational from, Rational to, bool isStartIncluded = true, bool isEndIncluded = false)
    {
        var elements = new List<Element> { };
        if(isStartIncluded)
            elements.Add(Point.Zero(from));
        elements.Add(Segment.Zero(from, to));
        if(isEndIncluded)
            elements.Add(Point.Zero(to));

        return new Sequence(elements);
    }

    /// <summary>
    /// Constructs a sequence that is $+\infty$ between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">Left endpoint of the sequence</param>
    /// <param name="to">Right endpoint of the sequence</param>
    /// <param name="isStartIncluded"></param>
    /// <param name="isEndIncluded"></param>
    public static Sequence PlusInfinite(Rational from, Rational to, bool isStartIncluded = true, bool isEndIncluded = false)
    {
        var elements = new List<Element> { };
        if(isStartIncluded)
            elements.Add(Point.PlusInfinite(from));
        elements.Add(Segment.PlusInfinite(from, to));
        if(isEndIncluded)
            elements.Add(Point.PlusInfinite(to));

        return new Sequence(elements);
    }

    /// <summary>
    /// Constructs a sequence that is $-\infty$ between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="from">Left endpoint of the sequence</param>
    /// <param name="to">Right endpoint of the sequence</param>
    /// <param name="isStartIncluded"></param>
    /// <param name="isEndIncluded"></param>
    public static Sequence MinusInfinite(Rational from, Rational to, bool isStartIncluded = true, bool isEndIncluded = false)
    {
        var elements = new List<Element> { };
        if(isStartIncluded)
            elements.Add(Point.MinusInfinite(from));
        elements.Add(Segment.MinusInfinite(from, to));
        if(isEndIncluded)
            elements.Add(Point.MinusInfinite(to));

        return new Sequence(elements);
    }

    /// <summary>
    /// Constructs a sequence that is <paramref name="value"/>> between <paramref name="from"/> and <paramref name="to"/>.
    /// </summary>
    /// <param name="value">Constant value of the sequence</param>
    /// <param name="from">Left endpoint of the sequence</param>
    /// <param name="to">Right endpoint of the sequence</param>
    /// <param name="isStartIncluded"></param>
    /// <param name="isEndIncluded"></param>
    /// <returns></returns>
    public static Sequence Constant(Rational value, Rational from, Rational to, bool isStartIncluded = true, bool isEndIncluded = false)
    {
        var elements = new List<Element> { };
        if(isStartIncluded)
            elements.Add(new Point(from, value));
        elements.Add(Segment.Constant(from, to, value));
        if(isEndIncluded)
            elements.Add(new Point(to, value));

        return new Sequence(elements);
    }

    #endregion

    #region Equality methods

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => (obj is Sequence sequence) && Equals(sequence);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hash = 0;
        foreach (var element in Elements)
            hash ^= element.GetHashCode();
        return hash;
    }

    /// <summary>
    /// Returns the hash code as hex string .
    /// </summary>
    public string GetHashString()
        => GetHashCode().ToString("X");

    /// <inheritdoc />
    public bool Equals(Sequence? other)
        => other is not null && Elements.SequenceEqual(other.Elements);

    /// <summary>
    /// Returns <code>true</code> if its operands are equal, <code>false</code> otherwise.
    /// </summary>
    public static bool operator ==(Sequence? a, Sequence? b) =>
        Equals(a, b);

    /// <summary>
    /// Returns <code>false</code> if its operands are equal, <code>true</code> otherwise.
    /// </summary>
    public static bool operator !=(Sequence? a, Sequence? b) =>
        !Equals(a, b);

    /// <summary>
    /// True if the sequences represent the same function.
    /// </summary>
    public bool Equivalent(Sequence sequence)
        => Equivalent(this, sequence);

    /// <summary>
    /// True if the sequences represent the same function.
    /// </summary>
    public static bool Equivalent(Sequence a, Sequence b)
        => a.Optimize() == b.Optimize();

    /// <summary>
    /// True if the <paramref name="a"/> and <paramref name="b"/> are ordered sequences of elements representing the same function, over the same interval. 
    /// </summary>
    /// <remarks>Optimized for minimal allocations</remarks>
    public static bool Equivalent(IEnumerable<Element> a, IEnumerable<Element> b)
    {
        using var enA = a.GetEnumerator();
        using var enB = b.GetEnumerator();
        while (true)
        {
            var aNext = enA.MoveNext();
            var bNext = enB.MoveNext();
            if (aNext != bNext) // one ended, the other not
                return false;
            if (!aNext && !bNext) // both ended
                return true;
            if(!Equals(enA.Current, enB.Current))   // next elements are different
                return false;

            // may continue to next elements
        }
    }

    //todo: write tests
    /// <summary>
    /// Returns the first time around which the functions represented by the sequences differ.
    /// Returns null if the two sequences represent the same function.
    /// Mostly useful to debug sequences that *should* be equivalent.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static Rational? FindFirstInequivalence(Sequence a, Sequence b)
    {
        var spotsA = GetSpots(a.Elements);
        var spotsB = GetSpots(b.Elements);
        var pairs = spotsA.Zip(spotsB);

        foreach (var pair in pairs)
        {
            if (pair.First != pair.Second)
                return Rational.Min(pair.First.Time, pair.Second.Time);
        }

        // multiple enumerations are preferred to materializing possibly long sequences
        var spotsACount = spotsA.Count();
        var spotsBCount = spotsB.Count();
        if (spotsACount != spotsBCount)
        {
            if (spotsACount > spotsBCount)
                return spotsA.ElementAt(spotsBCount).Time;
            else
                return spotsB.ElementAt(spotsACount).Time;
        }

        return null;

        IEnumerable<Point> GetSpots(IEnumerable<Element> seq)
        {
            foreach (var element in seq)
            {
                if (element is Point p)
                    yield return p;
                else if (element is Segment s)
                {
                    yield return new Point(time: s.StartTime, value: s.RightLimitAtStartTime);
                    yield return new Point(time: s.EndTime, value: s.LeftLimitAtEndTime);
                }
            }
        }
    }

    /// <summary>
    /// True if the first sequence is a lower bound for the second one, for their overlapping part.
    /// </summary>
    public static bool operator <=(Sequence a, Sequence b)
    {
        return LessOrEqual(a, b);
    }

    /// <summary>
    /// True if the first sequence is a lower bound for the second one, for their overlapping part.
    /// </summary>
    public static bool LessOrEqual(Sequence a, Sequence b, ComputationSettings? settings = null)
    {
        return a.Equivalent(Minimum(a, b, true, settings));
    }

    /// <summary>
    /// True if the first curve is an upper bound for the second one, for their overlapping part.
    /// </summary>
    public static bool operator >=(Sequence a, Sequence b)
    {
        return GreaterOrEqual(a, b);
    }

    /// <summary>
    /// True if the first sequence is an upper bound for the second one, for their overlapping part.
    /// </summary>
    public static bool GreaterOrEqual(Sequence a, Sequence b, ComputationSettings? settings = null)
    {
        return a.Equivalent(Maximum(a, b, true, settings));
    }

    /// <summary>
    /// Returns the opposite function, $g(t) = -f(t)$.
    /// </summary>
    public Sequence Negate()
    {
        if (IsZero)
            return this;

        return new Sequence(
            Elements.Select(e => -e)
        );
    }

    /// <summary>
    /// Returns the opposite function, $g(t) = -f(t)$.
    /// </summary>
    public static Sequence operator -(Sequence s)
        => s.Negate();

    #endregion

    #region Methods

    /// <summary>
    /// True if the sequence is defined at the given time.
    /// </summary>
    public bool IsDefinedAt(Rational time) => 
        time > DefinedFrom && time < DefinedUntil ||
        time == DefinedFrom && IsLeftClosed ||
        time == DefinedUntil && IsRightClosed;

    /// <summary>
    /// True if the sequence is defined before the given time.
    /// </summary>
    public bool IsDefinedBefore(Rational time) => 
        time > DefinedFrom && time <= DefinedUntil;

    /// <summary>
    /// True if the sequence is defined after the given time.
    /// </summary>
    public bool IsDefinedAfter(Rational time) => 
        time >= DefinedFrom && time < DefinedUntil;

    /// <summary>
    /// Computes the value of the sequence at the given <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The time of sampling.</param>
    /// <returns>Value of the segment at the given time or 0 if outside definition bounds.</returns>
    public Rational ValueAt(Rational time)
    {
        return GetElementAt(time).ValueAt(time);
    }

    /// <summary>
    /// Computes the right limit of the sequence at given <paramref name="time"/>
    /// </summary>
    /// <param name="time">The target time of the limit</param>
    /// <returns>The value of $f(t^+)$</returns>
    public Rational RightLimitAt(Rational time)
    {
        Segment segment = GetSegmentAfter(time);
        return segment.RightLimitAt(time);
    }

    /// <summary>
    /// Computes the left limit of the sequence at given <paramref name="time"/>.
    /// </summary>
    /// <param name="time">The target time of the limit.</param>
    /// <returns>The value of $f(t^-)$</returns>
    public Rational LeftLimitAt(Rational time)
    {
        Segment segment = GetSegmentBefore(time);
        return segment.LeftLimitAt(time);
    }

    /// <summary>
    /// Returns the <see cref="Element"/> that describes the sequence in <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given <paramref name="time"/> is out of sequence support.</exception> 
    /// <returns>The <see cref="Element"/> describing the sequence at <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a binary search, $O(\log(n))$</remarks>
    public Element GetElementAt(Rational time)
    {
        if(!IsDefinedAt(time))
            throw new ArgumentException("The given time is out of sequence support.");

        int firstIndex = FindFirstIndex(element => element.EndTime >= time);
        int lastIndex = FindLastIndex(element => element.StartTime <= time);

        return GetRange(firstIndex, lastIndex + 1)
            .Single(element => element.IsDefinedFor(time));
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence before <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given <paramref name="time"/> $-\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence before <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a binary search, $O(\log n)$</remarks>
    public Segment GetSegmentBefore(Rational time)
    {
        try
        {
            var targetIndex = FindFirstIndex(element => element.EndTime >= time);
            return (Segment) Elements[targetIndex];
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("Sequence is not defined before given time");
        }
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence after <paramref name="time"/>.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence after <paramref name="time"/>.</returns>
    /// <remarks>This method is implemented using a binary search, $O(\log n)$</remarks>
    public Segment GetSegmentAfter(Rational time)
    {
        try
        {
            var targetIndex = FindLastIndex(element => element.StartTime <= time);
            return (Segment) Elements[targetIndex];
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Sequence is not defined after given time");
        }
    }

    /// <summary>
    /// Returns the <see cref="Element"/> that describes the sequence in <paramref name="time"/>, and its index in the sequence.
    /// </summary>
    /// <param name="time">Time of the sample.</param>
    /// <param name="startingIndex">The index from which the search should start.</param>
    /// <exception cref="ArgumentException">Thrown if the given time is out of sequence support.</exception> 
    /// <returns>The <see cref="Element"/> describing the sequence at <paramref name="time"/>.</returns>
    /// <remarks>
    /// This method is implemented using a linear search, $O(n)$.
    /// It should be used in place of <see cref="GetElementAt"/> only for consecutive queries which traverse the sequence linearly, caching the index between calls.
    /// Thus, the overall cost will be $O(n) &lt; O(n log n)$.
    /// </remarks>
    public (Element element, int index) GetElementAt_Linear(Rational time, int startingIndex = 0)
    {
        if(!IsDefinedAt(time))
            throw new ArgumentException("The given time is out of sequence support.");

        if (startingIndex < 0 || startingIndex >= Count)
            throw new ArgumentException($"Invalid startingIndex: {startingIndex}");

        var targetIndex = Enumerable.Range(startingIndex, Count - startingIndex)
            .First(i => Elements[i].IsDefinedFor(time));
        return (element: Elements[targetIndex], index: targetIndex);
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence before time t, and its index in the sequence.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <param name="startingIndex">The index from which the search should start.</param>
    /// <exception cref="ArgumentException">Thrown if the given time $-\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence before time t.</returns>
    /// <remarks>
    /// This method is implemented using a linear search, $O(n)$.
    /// It should be used in place of <see cref="GetSegmentBefore"/> only for consecutive queries which traverse the sequence linearly, caching the index between calls.
    /// Thus, the overall cost will be $O(n) &lt; O(n~\log~n)$.
    /// </remarks>
    internal (Segment segment, int index) GetSegmentBefore_Linear(Rational time, int startingIndex = 0)
    {
        if (startingIndex < 0 || startingIndex >= Count)
            throw new ArgumentException($"Invalid startingIndex: {startingIndex}");

        try
        {
            var targetIndex = Enumerable.Range(startingIndex, Count - startingIndex)
                .First(i => Elements[i].EndTime >= time);
            return (segment: (Segment) Elements[targetIndex], index: targetIndex);
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("Sequence is not defined before given time");
        }
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the sequence after <paramref name="time"/>, and its index in the sequence.
    /// </summary>
    /// <param name="time">Time $t$ of the sample.</param>
    /// <param name="startingIndex">The index from which the search should start.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="startingIndex"/> is not a valid index.</exception>
    /// <exception cref="ArgumentException">Thrown if the given time $+\epsilon$ is out of sequence support.</exception>
    /// <returns>The <see cref="Segment"/> describing the sequence after time t.</returns>
    /// <remarks>
    /// This method is implemented using a linear search, $O(n)$.
    /// It should be used in place of <see cref="GetSegmentAfter"/> only for consecutive queries which traverse the sequence linearly, caching the index between calls.
    /// Thus, the overall cost will be $O(n) &lt; O(n \log n)$.
    /// </remarks>
    internal (Segment segment, int index) GetSegmentAfter_Linear(Rational time, int startingIndex = 0)
    {
        if (startingIndex < 0 || startingIndex >= Count)
            throw new ArgumentException($"Invalid startingIndex: {startingIndex}");

        try
        {
            var targetIndex = Enumerable.Range(startingIndex, Count - startingIndex)
                .First(i => Elements[i] is Segment s && s.EndTime > time && s.StartTime <= time);
            return (segment: (Segment) Elements[targetIndex], index: targetIndex);
        }
        catch (InvalidOperationException)
        {
            throw new ArgumentException("Sequence is not defined after given time");
        }
    }

    /// <summary>
    /// Computes the overlap between two sequences.
    /// </summary>
    /// <returns>The endpoints of the overlap interval, including whether its left/right closed, or null if there is none.</returns>
    public static (Rational start, Rational end, bool isLeftClosed, bool isRightClosed)? GetOverlap(Sequence a, Sequence b)
    {
        Rational start = Rational.Max(a.DefinedFrom, b.DefinedFrom);
        Rational end = Rational.Min(a.DefinedUntil, b.DefinedUntil);

        if (start > end ||
            start == end && !(a.IsPoint || b.IsPoint))
        {
            return null;
        }
        else
        {
            bool isLeftClosed = a.IsDefinedAt(start) && b.IsDefinedAt(start);
            bool isRightClosed = a.IsDefinedAt(end) && b.IsDefinedAt(end);

            return (start, end, isLeftClosed, isRightClosed);
        }
    }

    /// <summary>
    /// Computes the overlap between two sequences.
    /// </summary>
    /// <returns>The endpoints of the overlap interval, or null if there is none.</returns>
    public (Rational start, Rational end, bool isLeftClosed, bool isRightClosed)? GetOverlap(Sequence secondOperand)
        => GetOverlap(this, secondOperand);

    /// <summary>
    /// Returns a cut of the sequence for a smaller support.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the new support.</param>
    /// <param name="cutEnd">Right endpoint of the new support.</param>
    /// <param name="isStartIncluded">If true, the support is left-closed.</param>
    /// <param name="isEndIncluded">If true, the support is right-closed.</param>
    /// <exception cref="ArgumentException">Thrown if the new support is not a subset of the current one.</exception>
    public Sequence Cut(
        Rational cutStart, 
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (cutStart > cutEnd)
            throw new ArgumentException("Cut start cannot be after end.");

        if(cutStart < DefinedFrom || cutEnd > DefinedUntil)
            throw new ArgumentException("Cut limits are out of the sequence support.");

        if(isStartIncluded && !IsDefinedAt(cutStart) || isEndIncluded && !IsDefinedAt(cutEnd))
            throw new ArgumentException("Cut includes endpoints that sequence does not.");

        if (cutStart == cutEnd)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Cut endpoints, if equal, must be both inclusive.");

            var e = GetElementAt(cutStart);
            Point p;
            if (e is Point p2)
                p = p2;
            else
                p = (e as Segment)!.Sample(cutStart);
            return new Sequence(new Element[] {p});
        }

        // binary search for cut start
        int cutStartIndex = FindFirstIndex(IsPastStart);
        bool IsPastStart(Element element)
        {
            switch (element)
            {
                case Point point:
                    return isStartIncluded ? point.Time >= cutStart : point.Time > cutStart;

                case Segment segment:
                    return segment.EndTime > cutStart;

                default:
                    throw new InvalidCastException();
            }
        }

        // binary search for cut end
        int cutEndIndex = FindLastIndex(IsBeforeEnd, cutStartIndex);
        bool IsBeforeEnd(Element element)
        {
            switch (element)
            {
                case Point point:
                    return isEndIncluded ? point.Time <= cutEnd : point.Time < cutEnd;

                case Segment segment:
                    return segment.StartTime < cutEnd;

                default:
                    throw new InvalidCastException();
            }
        }

        // random-access selection of elements
        var cutElements = new List<Element>();
        for (int i = cutStartIndex; i <= cutEndIndex; i++)
        {
            var element = Elements[i];
            if (i == cutStartIndex && element is Segment sStart && sStart.StartTime < cutStart)
            {
                var (_, point, right) = sStart.Split(cutStart);
                if(isStartIncluded)
                    cutElements.Add(point);

                if (i == cutEndIndex && right.EndTime > cutEnd)
                {
                    //edge case: cut is all within one segment
                    var (l, p, _) = right.Split(cutEnd);
                    cutElements.Add(l);
                    if(isEndIncluded)
                        cutElements.Add(p);
                }
                else
                    cutElements.Add(right);
            }
            else if (i == cutEndIndex && element is Segment sEnd && sEnd.EndTime > cutEnd)
            {
                var (left, point, _) = sEnd.Split(cutEnd);
                cutElements.Add(left);
                if(isEndIncluded)
                    cutElements.Add(point);
            }
            else
            {
                cutElements.Add(element);
            }
        }

        return new Sequence(cutElements);
    }

    /// <summary>
    /// Returns a cut of the sequence for a smaller support.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the new support.</param>
    /// <param name="cutEnd">Right endpoint of the new support.</param>
    /// <param name="isStartIncluded">If true, the support is left-closed.</param>
    /// <param name="isEndIncluded">If true, the support is right-closed.</param>
    /// <exception cref="ArgumentException">Thrown if the new support is not a subset of the current one.</exception>
    /// <remarks>Optimized for minimal allocations.</remarks>
    public IEnumerable<Element> CutAsEnumerable(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        return Elements.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
    }

    //todo: write tests for FindFirstIndex, FindLastIndex

    /// <summary>
    /// Runs a binary search for the first element of the sequence that satisfies the predicate, and returns its index.
    /// </summary>
    /// <param name="predicate">The predicate that the element is to be tested against.</param>
    /// <param name="start">Inclusive zero-based index of the start of the range of elements to search within. If not set, defaults to 0.</param>
    /// <param name="end">Non-inclusive zero-based index of the end of the range of elements to search within. If not set, defaults to <see cref="Count"/>.</param>
    /// <returns>The index of the first element that satisfies the predicate, or $-1$ if not found.</returns>
    /// <remarks>As it is based on a binary search, the predicate must divide the sequence in (up to) two parts: the first in which it is `false`, the second in which it is `true`.</remarks>
    /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> do not denote a valid range of elements in the Sequence.</exception>
    public int FindFirstIndex(Predicate<Element> predicate, int? start = null, int? end = null)
    {
        int a = start ?? 0;
        int b = (end ?? Count) - 1;

        if (a > b)
            throw new ArgumentException("Start must be lower or equal than end");

        while (true)
        {
            if (a == b)
                break;

            int middle = (a + b) / 2;
            if (predicate(Elements[middle]))
                b = middle;
            else
                a = a != middle ? middle : b;
        }

        return predicate(Elements[a]) ?  a : -1;
    }

    /// <summary>
    /// Runs a binary search for the last element of the sequence that satisfies the predicate, and returns its index.
    /// </summary>
    /// <param name="predicate">The predicate that the element is to be tested against.</param>
    /// <param name="start">Inclusive zero-based index of the start of the range of elements to search within. If not set, defaults to 0.</param>
    /// <param name="end">Non-inclusive zero-based index of the end of the range of elements to search within. If not set, defaults to <see cref="Count"/>.</param>
    /// <returns>The index of the last element that satisfies the predicate, or -1 if not found</returns>
    /// <remarks>As it is based on a binary search, the predicate must divide the sequence in (up to) two parts: the first in which it is `true`, the second in which it is `false`.</remarks>
    /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> do not denote a valid range of elements in the Sequence.</exception>
    public int FindLastIndex(Predicate<Element> predicate, int? start = null, int? end = null)
    {
        int a = start ?? 0;
        int b = ( end ?? Count ) - 1;

        if (a > b)
            throw new ArgumentException("Start must be lower or equal than end");

        while (true)
        {
            if(a == b)
                break;

            int middle = (a + b) % 2 == 1 ? 
                (a + b) / 2 + 1 :
                (a + b) / 2;
            if (predicate(Elements[middle]))
                a = middle;
            else
                b = b != middle ? middle : a;
        }

        return predicate(Elements[a]) ?  a : -1;
    }

    /// <summary>
    /// Retrieves a range of elements from the sequence, via zero-based indexing.
    /// </summary>
    /// <param name="start">Zero-based index at which the range starts.</param>
    /// <param name="end">Non-inclusive zero-based index at which the range ends.</param>
    /// <returns>The range of elements</returns>
    /// <exception cref="ArgumentException"><paramref name="start"/> and <paramref name="end"/> do not denote a valid range of elements in the Sequence.</exception>
    public IEnumerable<Element> GetRange(int start, int end)
    {
        if (start > end)
            throw new ArgumentException("Start must be lower than end");

        for (int i = start; i < end; i++)
            yield return Elements[i];
    }

    /// <summary>
    /// Returns an equivalent sequence optimized to have the minimum amount of segments.
    /// </summary>
    /// <remarks>The result is a well-formed sequence [ZS23]</remarks>
    public Sequence Optimize()
    {
        var mergedElements = Elements.Merge();
        if (Elements.SequenceEqual(mergedElements))
            return this;
        else
            return new Sequence(mergedElements);
    }

    /// <summary>
    /// Returns an equivalent sequence guaranteed to have a <see cref="Point"/> at the given time.
    /// </summary>
    /// <param name="time">Time of the split.</param>
    /// <exception cref="ArgumentException">Thrown if the time of split is outside the sequence support.</exception>
    /// <returns>A new sequence with the enforced split, or this if it's already enforced.</returns>
    public Sequence EnforceSplitAt(Rational time)
    {
        if(!IsDefinedAt(time))
            throw new ArgumentException("The sequence is not defined for the given split time.");

        Element target = GetElementAt(time);
        if (target is Point)
            return this;
        else
        {
            var splitResult = ((Segment)target).Split(time);
            var segments = new List<Element>();

            foreach (Element element in Elements)
            {
                if(element != target)
                    segments.Add(element);
                else
                {
                    segments.Add(splitResult.leftSegment);
                    segments.Add(splitResult.point);
                    segments.Add(splitResult.rightSegment);
                }
            }

            return new Sequence(segments);
        }
    }

    /// <summary>
    /// If the sequence is upper-bounded, i.e. $f(t) &lt;= x$ for any $t$, returns $x$.
    /// Otherwise, returns <see cref="Rational.PlusInfinity"/>
    /// </summary>
    public Rational MaxValue()
    {
        var breakpoints = this.EnumerateBreakpoints();
        return breakpoints.GetBreakpointsValues().Max();
    }

    /// <summary>
    /// If the sequence is lower-bounded, i.e. $f(t) &gt;= x$ for any $t$, returns $x$.
    /// Otherwise, returns <see cref="Rational.MinusInfinity"/>
    /// </summary>
    public Rational MinValue()
    {
        var breakpoints = this.EnumerateBreakpoints();
        return breakpoints.GetBreakpointsValues().Min();
    }

    /// <summary>
    /// Fills the gaps of the set of elements within <paramref name="fillFrom"/> and <paramref name="fillTo"/> with the given value, defaults to $+\infty$.
    /// </summary>
    /// <param name="elements">The set of elements. Must be in order.</param>
    /// <param name="fillFrom">Left endpoint of the filling interval.</param>
    /// <param name="fillTo">Right endpoint of the filling interval.</param>
    /// <param name="isFromIncluded">If true, left endpoint is inclusive.</param>
    /// <param name="isToIncluded">If true, right endpoint is inclusive.</param>
    /// <param name="fillWith">The value filled in. Defaults to $+\infty$</param>
    /// <returns></returns>
    public static IEnumerable<Element> Fill(
        IEnumerable<Element> elements,
        Rational fillFrom,
        Rational fillTo,
        bool isFromIncluded = true,
        bool isToIncluded = false,
        Rational? fillWith = null
    )
        => elements.Fill(fillFrom, fillTo, isFromIncluded, isToIncluded, fillWith);

    #endregion Methods

    #region Json Methods

    /// <summary>
    /// Returns string serialization in Json format.
    /// </summary>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new RationalConverter());
    }

    /// <summary>
    /// Deserializes a Sequence.
    /// </summary>
    public static Sequence FromJson(string json)
    {
        var sequence = JsonConvert.DeserializeObject<Sequence>(json, new RationalConverter());
        if (sequence == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return sequence;
    }

    /// <summary>
    /// Returns a string containing C# code to create this Sequence.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new Sequence({newline}");
        sb.Append($"{Elements.ToCodeString(formatted, indentation + 1)}{newline}");
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

    #endregion Json Methods

    #region Basic manipulations

    /// <summary>
    /// Scales the sequence by a multiplicative factor.
    /// </summary>
    public Sequence Scale(Rational scaling)
    {
        var scaledElements = Elements
            //.AsParallel().AsOrdered()
            .Select(e => e.Scale(scaling));

        return new Sequence(scaledElements);
    }

    /// <summary>
    /// Scales the sequence by a multiplicative factor.
    /// </summary>
    public static Sequence operator *(Sequence sequence, Rational scaling)
    {
        return sequence.Scale(scaling);
    }

    /// <summary>
    /// Scales the sequence by a multiplicative factor.
    /// </summary>
    public static Sequence operator *(Rational scaling, Sequence sequence)
    {
        return sequence.Scale(scaling);
    }

    /// <summary>
    /// Translates forward the support by the given time quantity.
    /// </summary>
    public Sequence Delay(Rational delay, bool prependWithZero = true)
    {
        if (delay < 0)
            throw new ArgumentException("Delay must be >= 0");

        if (delay == 0)
            return this;

        var delayedElements = new List<Element>();

        if (prependWithZero)
        {
            if (IsLeftClosed)
                delayedElements.Add(Point.Zero(DefinedFrom));

            delayedElements.Add(
                Segment.Zero(
                    startTime: DefinedFrom,
                    endTime: DefinedFrom + delay
                )
            );
        }

        delayedElements.AddRange(Elements
            .Select(e => e.Delay(delay)));

        return new Sequence(delayedElements);
    }

    /// <summary>
    /// Translates backwards the support by the given time quantity.
    /// </summary>
    public Sequence Anticipate(Rational time)
    {
        if (time < 0)
            throw new ArgumentException("Time must be >= 0");

        if (time == 0)
            return this;

        var elements = new List<Element>();
        foreach (var element in Elements)
        {
            if (element is Point p)
            {
                if(p.Time < time)
                    continue;
                else
                    elements.Add(p.Anticipate(time));
            }
            else
            {
                var s = (Segment)element;
                if (s.StartTime < time)
                {
                    if (s.IsDefinedFor(time))
                    {
                        var (_, center, right) = s.Split(time);
                        elements.Add(center.Anticipate(time));
                        elements.Add(right.Anticipate(time));
                    }
                    else
                        continue;
                }
                else
                {
                    elements.Add(s.Anticipate(time));
                }
            }
        }

        return new Sequence(elements);
    }

    /// <summary>
    /// Shifts the sequence vertically by an additive factor.
    /// </summary>
    public Sequence VerticalShift(Rational shift, bool exceptOrigin = true)
    {
        if (shift == 0)
            return this;

        var offsettedElements = Elements
            .Select(e =>
            {
                if (exceptOrigin && e is Point p && p.Time.IsZero)
                {
                    return e;
                }
                else
                {
                    return e.VerticalShift(shift);
                }
            });

        return new Sequence(offsettedElements);
    }

    /// <summary>
    /// Shifts the sequence vertically by an additive factor.
    /// </summary>
    public static Sequence operator +(Sequence sequence, Rational shift)
    {
        return sequence.VerticalShift(shift);
    }

    /// <summary>
    /// Shifts the sequence vertically by an additive factor.
    /// </summary>
    public static Sequence operator +(Rational shift, Sequence sequence)
    {
        return sequence.VerticalShift(shift);
    }

    /// <summary>
    /// Concatenates two sequences.
    /// </summary>
    /// <param name="a">The first sequence of the concatenation. It must be finite in its right end.</param>
    /// <param name="b">The second sequence of the concatenation. It must be finite in its left end.</param>
    /// <param name="preserveDelay">
    /// If $b$ starts later than $t = 0$, and this option is `true`, this gap will be preserved in the concatenation with an infinite-valued segment.
    /// Otherwise, the gap is removed as if $b$ started from $t = 0$.
    /// </param>
    /// <param name="preserveShift">
    /// Let $b(0)$ be starting value of $b$, if $b(0)$ is not $0$ and this option is `true`, this shift will be preserved in the concatenation.
    /// Otherwise, the shift is removed as if $b(0) = 0$.
    /// </param>
    /// <returns>The sequence obtained from concatenating sequence <paramref name="b"/> at the end of sequence <paramref name="a"/>.</returns>
    /// <exception cref="ArgumentException">If either sequence is infinite at the end where concatenation happens.</exception>
    public static Sequence Concat(Sequence a, Sequence b, bool preserveDelay = false, bool preserveShift = false)
    {
        var aEndingValue = a.IsRightClosed ? a.ValueAt(a.DefinedUntil) : a.LeftLimitAt(a.DefinedUntil);
        var bStartingValue = b.IsLeftClosed ? b.ValueAt(b.DefinedFrom) : b.RightLimitAt(b.DefinedFrom);

        if (aEndingValue.IsInfinite)
            throw new ArgumentException("Left sequence is infinite in its right end, cannot concatenate.");

        if (bStartingValue.IsInfinite)
            throw new ArgumentException("Right sequence is infinite in its left end, cannot concatenate.");

        var displacedB = preserveDelay ? b.Delay(a.DefinedUntil, prependWithZero: false) : b.Delay(a.DefinedUntil - b.DefinedFrom, prependWithZero: false);
        displacedB = preserveShift ? displacedB.VerticalShift(aEndingValue) : displacedB.VerticalShift(aEndingValue - bStartingValue);

        IEnumerable<Element> elements;
        if (a.IsRightOpen && b.IsLeftOpen)
            elements = a.Elements
                .Append(new Point(time: a.DefinedUntil, value: aEndingValue))
                .Concat(displacedB.Elements);
        else
            elements = a.Elements
                .Concat(displacedB.Elements);

        var sequence = new Sequence(elements);
        return sequence;
    }

    /// <summary>
    /// Concatenates a set of sequences, in the order they are provided.
    /// </summary>
    /// <param name="sequences">
    /// The sequences to be concatenated.
    /// They must be finite at each point of concatenation.
    /// </param>
    /// <param name="preserveDelay">
    /// For each concatenation, let $b$ be the second sequence.
    /// If $b$ starts later than $t = 0$, and this option is `true`, this gap will be preserved in the concatenation with an infinite-valued segment.
    /// Otherwise, the gap is removed as if $b$ started from $t = 0$.
    /// </param>
    /// <param name="preserveShift">
    /// For each concatenation, let $b$ be the second sequence.
    /// Let $b(0)$ be starting value of $b$, if $b(0)$ is not $0$ and this option is `true`, this shift will be preserved in the concatenation.
    /// Otherwise, the shift is removed as if $b(0) = 0$.
    /// </param>
    /// <returns>The sequence obtained from concatenating all the sequences.</returns>
    /// <exception cref="ArgumentException">If any sequence is infinite at an end where concatenation happens</exception>
    public static Sequence Concat(IEnumerable<Sequence> sequences, bool preserveDelay = false, bool preserveShift = false)
    {
        return sequences.Aggregate((a, b) =>
            Concat(a, b, preserveDelay: preserveDelay, preserveShift: preserveShift));
    }

    /// <summary>
    /// Computes a non-negative version of this sequence, 
    /// i.e. a curve $g(t) = f(t)$ if $f(t) > 0$, $g(t) = 0$ otherwise.
    /// </summary>
    public Sequence ToNonNegative()
        => Maximum(
            this, 
            Sequence.Zero(DefinedFrom, DefinedUntil, isEndIncluded: true), 
            cutToOverlap: true
        );

    /// <summary>
    /// Computes a left-continuous version of this sequence.
    /// </summary>
    public Sequence ToLeftContinuous()
        => Elements.ToLeftContinuous().ToSequence();

    /// <summary>
    /// Computes a right-continuous version of this sequence.
    /// </summary>
    public Sequence ToRightContinuous()
        => Elements.ToRightContinuous().ToSequence();

    /// <summary>
    /// Computes the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) >= x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\downarrow(x)$ be defined from $x = 0$.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS23b]. 
    /// </remarks>
    public Sequence LowerPseudoInverse(bool startFromZero = true)
    {
        return Elements
            .LowerPseudoInverse(startFromZero)
            .ToSequence();
    }

    /// <summary>
    /// Computes the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf \left\{ t : f(t) > x \right\} = \sup \left\{ t : f(t) &lt;= x \right\}$.
    /// </summary>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\uparrow(x)$ be defined from $x = 0$.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is right-continuous, thus is revertible, i.e. $\left(f^{-1}_\uparrow\right)^{-1}_\uparrow = f$, only if $f$ is right-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS23b].
    /// </remarks>
    public Sequence UpperPseudoInverse(bool startFromZero = true)
    {
        return Elements
            .UpperPseudoInverse(startFromZero)
            .ToSequence();
    }

    #endregion Basic manipulations

    #region Addition and Subtraction operators

    /// <summary>
    /// Adds two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>The sequence resulting from the sum.</returns>
    public static Sequence Addition(Sequence a, Sequence b)
    {
        var overlap = GetOverlap(a, b);
        if (overlap == null)
            throw new ArgumentException("The sequences do not overlap.");

        var (start, end, isLeftClosed, isRightClosed) = overlap ?? default;

        var aCut = a.Cut(start, end, isLeftClosed, isRightClosed);
        var bCut = b.Cut(start, end, isLeftClosed, isRightClosed);

        var intervals = Interval.ComputeIntervals(aCut, bCut);
        var sumElements = intervals
            .Select(interval => Element.Addition(interval.Elements))
            .ToList();

        IEnumerable<Element> mergedElements = sumElements.Merge();

        return new Sequence(mergedElements);
    }

    /// <summary>
    /// Adds two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="b"></param>
    /// <returns>The sequence resulting from the sum.</returns>
    public Sequence Addition(Sequence b)
        => Addition(this, b);

    /// <summary>
    /// Adds two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>The sequence resulting from the sum.</returns>
    public static Sequence operator +(Sequence a, Sequence b)
        => Addition(a, b);

    /// <summary>
    /// Subtracts two sequences, over their overlapping parts.
    /// </summary>
    /// <returns>The sequence resulting from the sum.</returns>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <remarks>The operation does not enforce non-negative values.</remarks>
    public static Sequence Subtraction(Sequence a, Sequence b, bool nonNegative = true)
        => nonNegative ?
            Addition(a, -b).ToNonNegative() :
            Addition(a, -b);

    /// <summary>
    /// Subtracts two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="b"></param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <returns>The sequence resulting from the sum.</returns>
    public Sequence Subtraction(Sequence b, bool nonNegative = true)
        => Subtraction(this, b, nonNegative);

    /// <summary>
    /// Subtracts two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <remarks>
    /// The result is forced to be non-negative.
    /// Use <see cref="Subtraction(Unipi.Nancy.MinPlusAlgebra.Sequence, Unipi.Nancy.MinPlusAlgebra.Sequence,bool)"/> to have results with negative values.
    /// </remarks>
    public static Sequence operator -(Sequence a, Sequence b)
        => Subtraction(a, b);

    #endregion Addition and Subtraction operators

    #region Minimum operator

    /// <summary>
    /// Computes minimum of two sequences.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="cutToOverlap">If set, the minimum is performed over the overlapping part.</param>
    /// <param name="settings"></param>
    /// <returns>The sequence resulting from the minimum.</returns>
    public static Sequence Minimum(Sequence a, Sequence b, bool cutToOverlap = true, ComputationSettings? settings = null)
    {
        if (cutToOverlap)
        {
            var overlap = GetOverlap(a, b);
            if (overlap == null)
                throw new ArgumentException("The sequences don't overlap");
            else
            {
                var (start, end, isLeftClosed, isRightClosed) = overlap ?? default;

                a = a.Cut(start, end, isLeftClosed, isRightClosed);
                b = b.Cut(start, end, isLeftClosed, isRightClosed);
            }
        }

        var lowerEnvelopeElements = LowerEnvelope(a, b, settings);

        if (cutToOverlap)
            return new Sequence(lowerEnvelopeElements);
        else
            return new Sequence(
                elements: lowerEnvelopeElements,
                fillFrom: Rational.Min(a.DefinedFrom, b.DefinedFrom),
                fillTo: Rational.Max(a.DefinedUntil, b.DefinedUntil)
            );
    }

    /// <summary>
    /// Computes minimum of two sequences.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="cutToOverlap">If set, the minimum is performed over the overlapping part.</param>
    /// <param name="settings"></param>
    /// <returns>The sequence resulting from the minimum.</returns>
    public Sequence Minimum(Sequence b, bool cutToOverlap = true, ComputationSettings? settings = null)
        => Minimum(this, b, cutToOverlap, settings);

    /// <summary>
    /// Computes the lower envelope of the pair of sequences given.
    /// $O(n)$ complexity.
    /// </summary>
    /// <remarks>Used for minimum</remarks>
    public static List<Element> LowerEnvelope(Sequence a, Sequence b, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        var intervals = Interval.ComputeIntervals(a, b);
        List<Element> lowerElements;
        if (settings.UseParallelLowerEnvelope)
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

        return lowerElements.Merge();
    }

    #endregion Minimum operator

    #region Maximum operator

    /// <summary>
    /// Computes maximum of two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="cutToOverlap">If set, the maximum is performed over the overlapping part.</param>
    /// <param name="settings"></param>
    /// <returns>The sequence resulting from the maximum.</returns>
    public static Sequence Maximum(Sequence a, Sequence b, bool cutToOverlap = true, ComputationSettings? settings = null)
    {
        if(cutToOverlap)
        {
            var overlap = GetOverlap(a, b);
            if (overlap == null)
                throw new ArgumentException("The sequences don't overlap");
            {
                var (start, end, isLeftClosed, isRightClosed) = overlap ?? default;

                a = a.Cut(start, end, isLeftClosed, isRightClosed);
                b = b.Cut(start, end, isLeftClosed, isRightClosed);
            }
        }

        var upperEnvelopeElements = UpperEnvelope(a, b, settings);

        if (cutToOverlap)
            return new Sequence(upperEnvelopeElements);
        else
            return new Sequence(
                elements: upperEnvelopeElements,
                fillFrom: Rational.Min(a.DefinedFrom, b.DefinedFrom),
                fillTo: Rational.Max(a.DefinedUntil, b.DefinedUntil),
                fillWith: Rational.MinusInfinity
            );
    }

    /// <summary>
    /// Computes maximum of two sequences, over their overlapping parts.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="cutToOverlap">If set, the maximum is performed over the overlapping part.</param>
    /// <param name="settings"></param>
    /// <returns>The sequence resulting from the maximum.</returns>
    public Sequence Maximum(Sequence b, bool cutToOverlap = true, ComputationSettings? settings = null)
        => Maximum(a: this, b, cutToOverlap, settings);

    /// <summary>
    /// Computes the lower envelope of the pair of sequences given.
    /// $O(n)$ complexity.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <remarks>Used for maximum.</remarks>
    public static List<Element> UpperEnvelope(Sequence a, Sequence b, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        var intervals = Interval.ComputeIntervals(a, b);

        List<Element> upperElements;
        if (settings.UseParallelUpperEnvelope)
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

        return upperElements.Merge();
    }

    #endregion Maximum operator

    #region Convolution operator

    /// <summary>
    /// Computes the convolution of two sequences, $a \otimes b$.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="cutCeiling">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="isEndIncluded"></param>
    /// <param name="isCeilingIncluded"></param>
    /// <param name="useIsomorphism"></param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Described in [BT08], Section 4.4 point 3</remarks>
    public static Sequence Convolution(
        Sequence f,
        Sequence g,
        ComputationSettings? settings = null,
        Rational? cutEnd = null,
        Rational? cutCeiling = null,
        bool isEndIncluded = false,
        bool isCeilingIncluded = true,
        bool useIsomorphism = false)
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a [{a.GetHashString()}] of length {a.Count}:\n {a} \n and sequence b [{b.GetHashString()}] of length {b.Count}:\n {b}");
        #endif

        settings ??= ComputationSettings.Default();
        cutEnd ??= Rational.PlusInfinity;
        cutCeiling ??= Rational.PlusInfinity;

        if (
            settings.UseBySequenceConvolutionIsomorphismOptimization && 
            f.IsLeftContinuous && g.IsLeftContinuous &&
            f.IsNonDecreasing && g.IsNonDecreasing
        )
        {
            #if CONJECTURE_GENERALIZE_BYSEQUENCE_CONVOLUTION_ISOSPEED
            // the conjecture consists in generalizing use of by-sequence isospeed
            // by checking that the result length is equal to the shortest operand length
            var ta_f = f.DefinedFrom;
            var ta_g = g.DefinedFrom;
            var tb_f = f.DefinedUntil;
            var tb_g = g.DefinedUntil;

            var lf = tb_f - ta_f;
            var lg = tb_g - ta_g;
            var shortestLength = Rational.Min(lf, lg);
            var limitCutEnd = ta_f + ta_g + shortestLength;

            bool cutEndCheck;
            if (useIsomorphism)
                cutEndCheck = true;
            else
                cutEndCheck = cutEnd < limitCutEnd;
            
            if (cutEndCheck)
            #else
            if (useIsomorphism)
            #endif
            {
                // todo: fill in reference
                // the following heuristic roughly computes how many elementary convolutions would be involved
                // using direct or inverse method to choose which one to perform
                // discussed in [TBP23]

                var aConstantSegments = f.Elements.Count(e => e is Segment {IsConstant: true});
                var aNonConstantSegments = f.Elements.Count(e => e is Segment {IsConstant: false});
                var aPoints = f.Elements.Count(e => e is Point);
                var aPointsNotKept = f.EndsWithPlateau ? aConstantSegments - 1 : aConstantSegments;
                var aPointsKept = aPoints - aPointsNotKept;
                var aKept = aNonConstantSegments + aPointsKept;
                var aNotKept = aConstantSegments + aPointsNotKept;

                var bConstantSegments = g.Elements.Count(e => e is Segment {IsConstant: true});
                var bNonConstantSegments = g.Elements.Count(e => e is Segment {IsConstant: false});
                var bPoints = g.Elements.Count(e => e is Point);
                var bPointsNotKept = g.EndsWithPlateau ? bConstantSegments - 1 : bConstantSegments;
                var bPointsKept = bPoints - bPointsNotKept;
                var bKept = bNonConstantSegments + bPointsKept;
                var bNotKept = bConstantSegments + bPointsNotKept;

                var aDiscontinuities = f.EnumerateBreakpoints()
                    .Count(bp => bp.right != null && bp.center.Value != bp.right.RightLimitAtStartTime);
                var bDiscontinuities = g.EnumerateBreakpoints()
                    .Count(bp => bp.right != null && bp.center.Value != bp.right.RightLimitAtStartTime);
                var aReplaced = 2 * aDiscontinuities;
                var bReplaced = 2 * bDiscontinuities;

                var directCount = aNotKept * bNotKept + aNotKept * bKept + aKept * bNotKept;
                var inverseCount = aReplaced * bReplaced + aReplaced * bKept + aKept * bReplaced;

                if (directCount > inverseCount)
                {
                    var ta_f_prime = f.FirstPlateauEnd;
                    var ta_g_prime = g.FirstPlateauEnd;

                    var a_upi = f.UpperPseudoInverse(false);
                    var b_upi = g.UpperPseudoInverse(false);
                    var maxp = Sequence.MaxPlusConvolution(
                        a_upi, b_upi,
                        cutEnd: cutCeiling, cutCeiling: cutEnd,
                        isEndIncluded: true, isCeilingIncluded: true,
                        settings: settings with {UseBySequenceConvolutionIsomorphismOptimization = false});
                    var inverse_raw = maxp.LowerPseudoInverse(false);

                    Sequence inverse;
                    if (ta_f_prime == f.DefinedFrom && ta_g_prime == g.DefinedFrom)
                    {
                        inverse = inverse_raw;
                    }
                    else
                    {
                        // note: does not handle left-open sequences
                        var ext = Sequence.Constant(
                            f.ValueAt(f.DefinedFrom) + g.ValueAt(g.DefinedFrom),
                            f.DefinedFrom + g.DefinedFrom,
                            ta_f_prime + ta_g_prime
                        );
                        inverse = Sequence.Minimum(ext, inverse_raw, false);
                    }

                    if (cutCeiling == Rational.PlusInfinity)
                        return inverse;
                    else
                        return inverse.Elements
                            .CutWithCeiling(cutCeiling, isCeilingIncluded)
                            .ToSequence();
                }
            }
        }

        if (f.IsPlusInfinite || g.IsPlusInfinite)
        {
            var start = f.DefinedFrom + g.DefinedFrom;
            var end = Rational.Min(f.DefinedUntil + g.DefinedUntil, cutEnd.Value);
            return PlusInfinite(start, end);
            // return PlusInfinite(start, end, isLeftClosed, isRightClosed);
        }

        var areSequenceEqual = Equivalent(f, g);
        #if DO_LOG
        var countStopwatch = Stopwatch.StartNew();
        #endif
        var pairsCount = GetElementPairs(true).LongCount();
        #if DO_LOG
        countStopwatch.Stop();
        if(cutCeiling != Rational.PlusInfinity)
            logger.Trace($"Convolution: counted {pairsCount} pairs (ignoring cutCeiling!) in {countStopwatch.Elapsed}");
        else
            logger.Trace($"Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
        #endif

        if (cutEnd.Value.IsFinite)
        {
            var earliestElement = f.DefinedFrom + g.DefinedFrom;
            #if DO_LOG
            logger.Trace($"cutEnd set to {cutEnd}, earliest element will be {earliestElement}");
            #endif
            if(cutEnd < earliestElement)
                throw new Exception("Convolution is cut before it starts");
        }
        #if DO_LOG
        if(cutCeiling != Rational.PlusInfinity)
            logger.Trace($"cutCeiling set to {cutCeiling}");
        #endif

        IEnumerable<Element> result;
        if (settings.UseConvolutionPartitioning && pairsCount > settings.ConvolutionPartitioningThreshold)
            result = PartitionedConvolution();
        else
        if (settings.UseParallelConvolution && pairsCount > settings.ConvolutionParallelizationThreshold)
            result = ParallelConvolution();
        else
            result = SerialConvolution();

        if (cutCeiling == Rational.PlusInfinity)
            return result.ToSequence();
        else
            return result
                .CutWithCeiling(cutCeiling, isCeilingIncluded)
                .ToSequence();

        // Returns the pairs to be involved in the convolution.
        // If fastIteration is true, some checks are skipped - this is mainly for counting purposes.
        IEnumerable<(Element ea, Element eb)> GetElementPairs(bool fastIteration = false)
        {
            var elementPairs = f.Elements
                .Where(ea => ea.IsFinite)
                .SelectMany(ea => g.Elements
                    .Where(eb => eb.IsFinite)
                    .Where(eb => PairBeforeEnd(ea, eb))                    
                    .Where(eb => fastIteration || PairBelowCeiling(ea, eb))
                    .Select(eb => (a: ea, b: eb))
                );

            // if self-convolution, filter out symmetric pairs
            if (areSequenceEqual)
                elementPairs = elementPairs.Where(pair => pair.a.StartTime <= pair.b.StartTime);

            return elementPairs;

            bool PairBeforeEnd(Element ea, Element eb)
            {
                if (cutEnd == Rational.PlusInfinity)
                    return true;

                if (isEndIncluded)
                {
                    if (ea is Point pa && eb is Point pb)
                        return pa.Time + pb.Time <= cutEnd;
                    else
                        return ea.StartTime + eb.StartTime < cutEnd;
                }
                else
                    return ea.StartTime + eb.StartTime < cutEnd;
            }

            bool PairBelowCeiling(Element ea, Element eb)
            {
                if (cutCeiling == Rational.PlusInfinity) 
                    return true;

                var ea_s = GetStart(ea);
                var eb_s = GetStart(eb);
                if(isCeilingIncluded)
                {
                    if ((ea is not Segment || ea is Segment { IsConstant: true }) && 
                        (eb is not Segment || eb is Segment { IsConstant: true }))
                        return ea_s + eb_s <= cutCeiling;
                    else
                        return ea_s + eb_s < cutCeiling;
                }
                else
                    return ea_s + eb_s < cutCeiling;

                Rational GetStart(Element e)
                {
                    switch (e)
                    {
                        case Point p:
                            return p.Value;
                        case Segment s:
                            return s.RightLimitAtStartTime;
                        default:
                            throw new InvalidCastException();
                    }
                }
            }
        }

        IEnumerable<Element> SerialConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running serial convolution, {pairsCount} pairs.");
            #endif

            var convolutionElements = GetElementPairs() 
                .SelectMany(pair => Element.Convolution(pair.ea, pair.eb, cutEnd, cutCeiling))
                .ToList();

            var lowerEnvelope = convolutionElements.LowerEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
                return lowerEnvelope;
            else
                // gaps may be expected
                return lowerEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil
                );
        }

        IEnumerable<Element> ParallelConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running parallel convolution, {pairsCount} pairs.");
            #endif

            var convolutionElements = GetElementPairs()
                .AsParallel()
                .SelectMany(pair => Element.Convolution(pair.ea, pair.eb, cutEnd, cutCeiling))
                .ToList();

            var lowerEnvelope = convolutionElements.LowerEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
                return lowerEnvelope;
            else
                // gaps may be expected
                return lowerEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil
                );
        }

        // The elementPairs are partitioned in smaller sets (without any ordering)
        // From each set, the convolutions are computed and then their lower envelope
        // The resulting sequences will have gaps
        // Those partial convolutions are then merged via Sequence.LowerEnvelope
        IEnumerable<Element> PartitionedConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running partitioned convolution, {pairsCount} pairs.");
            #endif

            var partialConvolutions = PartitionConvolutionElements()
                .Select(elements => 
                    elements.LowerEnvelope(settings)
                    .Fill(
                        fillFrom: f.DefinedFrom + g.DefinedFrom,
                        fillTo: f.DefinedUntil + g.DefinedUntil
                    )
                    .ToSequence()
                )
                .ToList();

            #if DO_LOG
            logger.Trace($"Partitioned convolutions computed, proceding with lower envelope of {partialConvolutions.Count} sequences");
            #endif

            var lowerEnvelope = partialConvolutions.LowerEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
            {
                return lowerEnvelope
                    .Where(e => e.IsFinite);
            }
            else
            {
                // gaps may be expected
                return lowerEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil
                );
            }

            IEnumerable<IReadOnlyList<Element>> PartitionConvolutionElements()
            {
                #if DO_LOG
                int partitionsCount = (int)
                    Math.Ceiling((double)pairsCount / settings.ConvolutionPartitioningThreshold);
                logger.Trace($"Partitioning {pairsCount} pairs in {partitionsCount} chunks of {settings.ConvolutionPartitioningThreshold}.");
                #endif

                var partitions = GetElementPairs()
                    .Chunk(settings.ConvolutionPartitioningThreshold);

                foreach (var partition in partitions)
                {
                    IReadOnlyList<Element> convolutionElements;
                    if (settings.UseParallelConvolution)
                    {
                        convolutionElements = partition
                            .AsParallel()
                            .SelectMany(pair => Element.Convolution(pair.ea, pair.eb, cutEnd, cutCeiling))
                            .ToList()
                            .SortElements(settings);    
                    }
                    else
                    {
                        convolutionElements = partition
                            .SelectMany(pair => Element.Convolution(pair.ea, pair.eb, cutEnd, cutCeiling))
                            .ToList()
                            .SortElements(settings);
                    }

                    yield return convolutionElements;
                }
            }
        }
    }

    /// <summary>
    /// Computes the convolution of two sequences, $f \otimes g$.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Described in [BT08], Section 4.4 point 3</remarks>
    public Sequence Convolution(Sequence sequence, ComputationSettings? settings = null, Rational? cutEnd = null)
        => Convolution(this, sequence, settings, cutEnd);

    #endregion Convolution operator

    #region EstimateConvolution

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the convolution of the two sequences,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="cutCeiling">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is `true`.
    /// </returns>
    public static long EstimateConvolution(
        Sequence a, 
        Sequence b, 
        ComputationSettings? settings = null, 
        Rational? cutEnd = null, 
        Rational? cutCeiling = null, 
        bool countElements = false
    )
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a [{a.GetHashString()}] of length {a.Count}:\n {a} \n and sequence b [{b.GetHashString()}] of length {b.Count}:\n {b}");
        #endif
        settings ??= ComputationSettings.Default();
        cutEnd ??= Rational.PlusInfinity;
        cutCeiling ??= Rational.PlusInfinity;

        if (a.IsInfinite || b.IsInfinite)
        {
            // var start = a.DefinedFrom + b.DefinedFrom;
            // var end = Rational.Min(a.DefinedUntil + b.DefinedUntil, cutEnd.Value);
            return 0;
        }

        var areSequenceEqual = Equivalent(a, b);
        if (!countElements)
        {
            #if DO_LOG
            var countStopwatch = Stopwatch.StartNew();
            #endif
            var pairsCount = GetElementPairs().LongCount();
            #if DO_LOG
            countStopwatch.Stop();
            logger.Debug($"Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
            #endif
            return pairsCount;
        }
        else
        {
            #if DO_LOG
            var deepCountStopwatch = Stopwatch.StartNew();
            #endif
            var deepCount = GetElementPairs()
                .SelectMany(p => p.ea.Convolution(p.eb))
                .LongCount();
            #if DO_LOG
            deepCountStopwatch.Stop();
            logger.Debug($"Convolution: counted {deepCount} total elements in {deepCountStopwatch.Elapsed}");
            #endif
            return deepCount;
        }

        IEnumerable<(Element ea, Element eb)> GetElementPairs()
        {
            var elementPairs = a.Elements
                .Where(ea => ea.IsFinite)
                .SelectMany(ea => b.Elements
                    .Where(eb => eb.IsFinite)
                    .Where(eb => ea.StartTime + eb.StartTime < cutEnd)
                    .Where(eb => PairBelowCeiling(ea, eb))
                    .Select(eb => (a: ea, b: eb))
                );

            // if self-convolution, filter out symmetric pairs
            if (areSequenceEqual)
                elementPairs = elementPairs.Where(pair => pair.a.StartTime <= pair.b.StartTime);

            return elementPairs;

            bool PairBelowCeiling(Element ea, Element eb)
            {
                if (cutCeiling == Rational.PlusInfinity) 
                    return true;

                var (ea_s, ea_e) = GetEndpoints(ea);
                var (eb_s, eb_e) = GetEndpoints(eb);
                if (ea_s + eb_s <= cutCeiling || ea_e + eb_e <= cutCeiling)
                    return true;
                else
                    return false;

                (Rational startValue, Rational endValue) GetEndpoints(Element e)
                {
                    switch (e)
                    {
                        case Point p:
                            return (p.Value, p.Value);
                        case Segment s:
                            return (s.RightLimitAtStartTime, s.LeftLimitAtEndTime);
                        default:
                            throw new InvalidCastException();
                    }
                }
            }
        }
    }

    #endregion

    #region Deconvolution operator

    /// <summary>
    /// Computes the deconvolution of two sequences, $a \oslash b$.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="cutStart">If not null, element deconvolutions whose result ends strictly before this time are skipped.</param>
    /// <param name="cutEnd">If not null, the result is cut or filled with $+\infty$ up to this time, endpoint excluded.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution.</returns>
    /// <remarks>Described in [BT08], Section 4.5</remarks>
    public static Sequence Deconvolution(
        Sequence a, Sequence b, 
        Rational? cutStart = null, Rational? cutEnd = null, 
        ComputationSettings? settings = null
    )
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a:\n {a} \n and sequence b:\n {b}");
        #endif

        settings ??= ComputationSettings.Default();

        var elementPairs = a.Elements
            .SelectMany(ea => b.Elements
                .Select(eb => (a: ea, b: eb))
            )
            .Where(pair => pair.a.IsFinite && pair.b.IsFinite)
            .Where(pair => cutStart == null || pair.a.EndTime - pair.b.StartTime >= cutStart)
            .ToList();
        var pairsCount = elementPairs.Count;

        List<Element> result;
        if (settings.UseParallelDeconvolution && pairsCount > settings.DeconvolutionParallelizationThreshold)
            result = ParallelDeconvolution();
        else
            result = SerialDeconvolution();

        var resultStart = result.First().StartTime;
        var resultEnd = result.Last().EndTime;

        if (cutStart != null || cutEnd != null)
        {
            IEnumerable<Element> cutResult = result;
            if (cutEnd != null)
                cutResult = cutResult.Fill(resultStart, cutEnd.Value);

            var start = cutStart != null ? cutStart.Value : resultStart;
            var end = cutEnd != null ? cutEnd.Value : resultEnd;
            cutResult = cutResult.Cut(start, end);
            return cutResult.ToSequence();
        }
        else
        {
            return result.ToSequence();
        }

        List<Element> SerialDeconvolution()
        {
            var deconvolutionElements = elementPairs
                .SelectMany( pair => Element.Deconvolution(pair.a, pair.b))
                .ToList();

            return deconvolutionElements
                .UpperEnvelope(settings);
        }

        List<Element> ParallelDeconvolution()
        {
            var deconvolutionElements = elementPairs
                .AsParallel()
                .SelectMany( pair => Element.Deconvolution(pair.a, pair.b))
                .ToList();

            return deconvolutionElements
                .UpperEnvelope(settings);
        }
    }

    /// <summary>
    /// Computes the deconvolution of two sequences, $f \oslash g$.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="cutStart">If not null, element deconvolutions whose result ends strictly before this time are skipped.</param>
    /// <param name="cutEnd">If not null, the result is cut or filled with $+\infty$ up to this time, endpoint excluded.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution.</returns>
    /// <remarks>Described in [BT08], Section 4.5</remarks>
    public Sequence Deconvolution(Sequence sequence, Rational? cutStart = null, Rational? cutEnd = null, ComputationSettings? settings = null)
        => Deconvolution(a: this, b: sequence, cutStart, cutEnd, settings);

    #endregion Deconvolution operator

    #region Max-plus operators

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="cutCeiling">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <param name="isEndIncluded"></param>
    /// <param name="isCeilingIncluded"></param>
    /// <param name="useIsomorphism"></param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT08], Section 4.4 point 3</remarks>
    public static Sequence MaxPlusConvolution(
        Sequence f, 
        Sequence g, 
        ComputationSettings? settings = null, 
        Rational? cutEnd = null, 
        Rational? cutCeiling = null,
        bool isEndIncluded = false,
        bool isCeilingIncluded = false,
        bool useIsomorphism = false)
    {
        #if MAX_CONV_AS_NEGATIVE_MIN_CONV
        return -Convolution(-a, -b, settings, cutEnd, -cutCeiling);
        #else

        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Max-plus Convolution between sequence a [{a.GetHashString()}] of length {a.Count}:\n {a} \n and sequence b [{b.GetHashString()}] of length {b.Count}:\n {b}");
        #endif

        settings ??= ComputationSettings.Default();
        cutEnd ??= Rational.PlusInfinity;
        cutCeiling ??= Rational.PlusInfinity;

        if (
            settings.UseBySequenceConvolutionIsomorphismOptimization && 
            f.IsRightContinuous && g.IsRightContinuous &&
            f.IsNonDecreasing && g.IsNonDecreasing
        )
        {
            #if CONJECTURE_GENERALIZE_BYSEQUENCE_CONVOLUTION_ISOSPEED
            // the conjecture consists in generalizing use of by-sequence isospeed
            // by checking that the result length is equal to the shortest operand length
            var ta_f = f.DefinedFrom;
            var ta_g = g.DefinedFrom;
            var tb_f = f.DefinedUntil;
            var tb_g = g.DefinedUntil;

            var lf = tb_f - ta_f;
            var lg = tb_g - ta_g;
            var shortestLength = Rational.Min(lf, lg);
            var limitCutEnd = ta_f + ta_g + shortestLength;

            bool cutEndCheck;
            if(useIsomorphism)
                cutEndCheck = true;
            else
                cutEndCheck = cutEnd < limitCutEnd;

            if (cutEndCheck)
            #else
            if (useIsomorphism)
            #endif
            {
                // todo: fill in reference
                // the following heuristic roughly computes how many elementary convolutions would be involved
                // using direct or inverse method to choose which one to perform
                // discussed in [TBP23]

                var aConstantSegments = f.Elements.Count(e => e is Segment {IsConstant: true});
                var aNonConstantSegments = f.Elements.Count(e => e is Segment {IsConstant: false});
                var aPoints = f.Elements.Count(e => e is Point);
                var aPointsNotKept = f.EndsWithPlateau ? aConstantSegments - 1 : aConstantSegments;
                var aPointsKept = aPoints - aPointsNotKept;
                var aKept = aNonConstantSegments + aPointsKept;
                var aNotKept = aConstantSegments + aPointsNotKept;

                var bConstantSegments = g.Elements.Count(e => e is Segment {IsConstant: true});
                var bNonConstantSegments = g.Elements.Count(e => e is Segment {IsConstant: false});
                var bPoints = g.Elements.Count(e => e is Point);
                var bPointsNotKept = g.EndsWithPlateau ? bConstantSegments - 1 : bConstantSegments;
                var bPointsKept = bPoints - bPointsNotKept;
                var bKept = bNonConstantSegments + bPointsKept;
                var bNotKept = bConstantSegments + bPointsNotKept;

                var aDiscontinuities = f.EnumerateBreakpoints()
                    .Count(bp => bp.right != null && bp.center.Value != bp.right.RightLimitAtStartTime);
                var bDiscontinuities = g.EnumerateBreakpoints()
                    .Count(bp => bp.right != null && bp.center.Value != bp.right.RightLimitAtStartTime);
                var aReplaced = 2 * aDiscontinuities;
                var bReplaced = 2 * bDiscontinuities;

                var directCount = aNotKept * bNotKept + aNotKept * bKept + aKept * bNotKept;
                var inverseCount = aReplaced * bReplaced + aReplaced * bKept + aKept * bReplaced;

                if (directCount > inverseCount)
                {
                    var tb_f_prime = f.LastPlateauStart;
                    var tb_g_prime = g.LastPlateauStart;

                    var f_lpi = f.LowerPseudoInverse(false);
                    var g_lpi = g.LowerPseudoInverse(false);
                    var minp = Sequence.Convolution(
                        f_lpi, g_lpi,
                        cutEnd: cutCeiling, cutCeiling: cutEnd,
                        isEndIncluded: true, isCeilingIncluded: true,
                        settings: settings with {UseBySequenceConvolutionIsomorphismOptimization = false});
                    var inverse_raw = minp.UpperPseudoInverse(false);
                    if (inverse_raw.DefinedUntil < cutEnd)
                    {
                        // this means that, in minp, there was a discontinuity over the cutCeiling
                        // hence, there should be a constant segment until cutEnd
                        var missingSegment = Segment.Constant(
                            inverse_raw.DefinedUntil,
                            (Rational) cutEnd,
                            ((Point) inverse_raw.Elements.Last()).Value
                        );
                        inverse_raw = inverse_raw.Elements
                            .Append(missingSegment)
                            .ToSequence();
                    }

                    Sequence inverse;
                    if (tb_f_prime == f.DefinedUntil && tb_g_prime == g.DefinedUntil)
                    {
                        inverse = inverse_raw;
                    }
                    else
                    {
                        // todo: can be optimized by applying horizontal and vertical filtering 
                        var missingElements = new List<Element> { };
                        if (tb_f_prime < f.DefinedUntil)
                        {
                            var pf = f.GetElementAt(tb_f_prime);
                            var sf = f.GetSegmentAfter(tb_f_prime);
                            foreach (var eg in g.Elements)
                            {
                                missingElements.AddRange(Element.MaxPlusConvolution(pf, eg));
                                missingElements.AddRange(Element.MaxPlusConvolution(sf, eg));
                            }
                        }

                        if (tb_g_prime < g.DefinedUntil)
                        {
                            var pg = g.GetElementAt(tb_g_prime);
                            var sg = g.GetSegmentAfter(tb_g_prime);
                            foreach (var ef in f.Elements)
                            {
                                missingElements.AddRange(Element.MaxPlusConvolution(ef, pg));
                                missingElements.AddRange(Element.MaxPlusConvolution(ef, sg));
                            }
                        }

                        var upperEnvelope = missingElements.UpperEnvelope();
                        var ext = upperEnvelope.ToSequence(
                            fillFrom: upperEnvelope.First().StartTime, 
                            fillTo: upperEnvelope.Last().EndTime, 
                            fillWith: Rational.MinusInfinity
                        );
                        inverse = Sequence.Maximum(ext, inverse_raw, false);
                    }

                    if (cutCeiling == Rational.PlusInfinity)
                        return inverse;
                    else
                        return inverse.Elements
                            .CutWithCeiling(cutCeiling, isCeilingIncluded)
                            .ToSequence();
                }
            }
        }

        if (f.IsMinusInfinite || g.IsMinusInfinite)
        {
            var start = f.DefinedFrom + g.DefinedFrom;
            var end = Rational.Min(f.DefinedUntil + g.DefinedUntil, cutEnd.Value);
            return MinusInfinite(start, end);
        }

        var areSequenceEqual = Equivalent(f, g);
        #if DO_LOG
        var countStopwatch = Stopwatch.StartNew();
        #endif
        var pairsCount = GetElementPairs().LongCount();
        #if DO_LOG
        countStopwatch.Stop();
        logger.Trace($"Max-plus Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
        #endif

        if (cutEnd.Value.IsFinite)
        {
            var earliestElement = f.DefinedFrom + g.DefinedFrom;
            #if DO_LOG
            logger.Trace($"cutEnd set to {cutEnd}, earliest element will be {earliestElement}");
            #endif
            if(cutEnd < earliestElement)
                throw new Exception("Convolution is cut before it starts");
        }

        IEnumerable<Element> result;
        if (settings.UseConvolutionPartitioning && pairsCount > settings.ConvolutionPartitioningThreshold)
            result = PartitionedConvolution();
        else
        if (settings.UseParallelConvolution && pairsCount > settings.ConvolutionParallelizationThreshold)
            result = ParallelConvolution();
        else
            result = SerialConvolution();

        if (cutCeiling == Rational.PlusInfinity)
            return result.ToSequence();
        else
            return result
                .CutWithCeiling(cutCeiling, isCeilingIncluded)
                .ToSequence();

        IEnumerable<(Element ea, Element eb)> GetElementPairs()
        {
            var elementPairs = f.Elements
                .Where(ea => ea.IsFinite)
                .SelectMany(ea => g.Elements
                    .Where(eb => eb.IsFinite)
                    .Where(eb => PairBeforeEnd(ea, eb))
                    .Select(eb => (a: ea, b: eb))
                );

            // if self-convolution, filter out symmetric pairs
            if (areSequenceEqual)
                elementPairs = elementPairs.Where(pair => pair.a.StartTime <= pair.b.StartTime);

            if (cutCeiling != Rational.PlusInfinity)
            {
                if (isCeilingIncluded)
                {
                    var firstSafeCeiling = elementPairs
                        .Select(p => ConvolutionStartingValue(p.a, p.b))
                        .Where(v => v > cutCeiling)
                        .Cast<Rational?>()
                        .DefaultIfEmpty(null)
                        .Min();
                    
                    if (firstSafeCeiling != null)
                    {
                        elementPairs = elementPairs 
                            .Where(p => ConvolutionStartingValue(p.a, p.b) <= firstSafeCeiling);
                    }
                }
                else
                {
                    elementPairs = elementPairs.Where(p =>  ConvolutionStartingValue(p.a, p.b) <= cutCeiling);
                }
                
                Rational ConvolutionStartingValue(Element ea, Element eb)
                {
                    return GetStart(ea) + GetStart(eb);
                        
                    Rational GetStart(Element e)
                    {
                        switch (e)
                        {
                            case Point p:
                                return p.Value;
                            case Segment s:
                                return s.RightLimitAtStartTime;
                            default:
                                throw new InvalidCastException();
                        }
                    }
                }
            }
            else
                return elementPairs;
           
            return elementPairs;

            bool PairBeforeEnd(Element ea, Element eb)
            {
                if (cutEnd == Rational.PlusInfinity)
                    return true;

                if (isEndIncluded)
                {
                    if (ea is Point pa && eb is Point pb)
                        return pa.Time + pb.Time <= cutEnd;
                    else
                        return ea.StartTime + eb.StartTime < cutEnd;
                }
                else
                    return ea.StartTime + eb.StartTime < cutEnd;
            }
        }

        IEnumerable<Element> SerialConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running serial convolution, {pairsCount} pairs.");
            #endif

            var convolutionElements = GetElementPairs() 
                .SelectMany(pair => Element.MaxPlusConvolution(pair.ea, pair.eb, cutEnd))
                .ToList();

            var upperEnvelope = convolutionElements.UpperEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
                return upperEnvelope;
            else
                // gaps may be expected
                return upperEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil,
                    fillWith: Rational.MinusInfinity
                );
        }

        IEnumerable<Element> ParallelConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running parallel convolution, {pairsCount} pairs.");
            #endif

            var convolutionElements = GetElementPairs()
                .AsParallel()
                .SelectMany(pair => Element.MaxPlusConvolution(pair.ea, pair.eb, cutEnd))
                .ToList();

            var upperEnvelope = convolutionElements.UpperEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
                return upperEnvelope;
            else
                // gaps may be expected
                return upperEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil,
                    fillWith: Rational.MinusInfinity
                );
        }

        // The elementPairs are partitioned in smaller sets (without any ordering)
        // From each set, the convolutions are computed and then their lower envelope
        // The resulting sequences will have gaps
        // Those partial convolutions are then merged via Sequence.UpperEnvelope
        IEnumerable<Element> PartitionedConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running partitioned convolution, {pairsCount} pairs.");
            #endif

            var partialConvolutions = PartitionConvolutionElements()
                .Select(elements =>
                    elements.UpperEnvelope(settings)
                    .Fill(
                        fillFrom: f.DefinedFrom + g.DefinedFrom,
                        fillTo: f.DefinedUntil + g.DefinedUntil,
                        fillWith: Rational.MinusInfinity
                    )
                    .ToSequence()
                )
                .ToList();

            #if DO_LOG
            logger.Trace($"Partitioned convolutions computed, proceding with lower envelope of {partialConvolutions.Count} sequences");
            #endif

            var upperEnvelope = partialConvolutions.UpperEnvelope(settings);
            if (f.IsFinite && g.IsFinite)
            {
                return upperEnvelope
                    .Where(e => e.IsFinite);
            }
            else
            {
                // gaps may be expected
                return upperEnvelope.Fill(
                    fillFrom: f.DefinedFrom + g.DefinedFrom,
                    fillTo: f.DefinedUntil + g.DefinedUntil,
                    fillWith: Rational.MinusInfinity
                );
            }

            IEnumerable<IReadOnlyList<Element>> PartitionConvolutionElements()
            {
                int partitionsCount = (int)
                    Math.Ceiling((double)pairsCount / settings.ConvolutionPartitioningThreshold);
                #if DO_LOG
                logger.Trace($"Partitioning {pairsCount} pairs in {partitionsCount} chunks of {settings.ConvolutionPartitioningThreshold}.");
                #endif

                var partitions = GetElementPairs()
                    .Chunk(settings.ConvolutionPartitioningThreshold);

                foreach (var partition in partitions)
                {
                    IReadOnlyList<Element> convolutionElements;
                    if (settings.UseParallelConvolution)
                    {
                        convolutionElements = partition
                            .AsParallel()
                            .SelectMany(pair => Element.MaxPlusConvolution(pair.ea, pair.eb, cutEnd))
                            .ToList()
                            .SortElements(settings);    
                    }
                    else
                    {
                        convolutionElements = partition
                            .SelectMany(pair => Element.MaxPlusConvolution(pair.ea, pair.eb, cutEnd))
                            .ToList()
                            .SortElements(settings);
                    }

                    yield return convolutionElements;
                }
            }
        }
        #endif
    }

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public Sequence MaxPlusConvolution(Sequence sequence, ComputationSettings? settings = null, Rational? cutEnd = null)
        => MaxPlusConvolution(this, sequence, settings, cutEnd);

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus deconvolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static Sequence MaxPlusDeconvolution(Sequence a, Sequence b, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Computing max-plus deconvolution");
        #endif
        return -Deconvolution(-a, -b, settings: settings);
    }

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus deconvolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public Sequence MaxPlusDeconvolution(Sequence sequence, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Computing max-plus deconvolution");
        #endif
        return -Deconvolution(-this, -sequence, settings: settings);
    }

    #endregion Max-plus operators

    #region Composition

    /// <summary>
    /// Compute the composition $f(g(t))$, over a limited interval.
    /// </summary>
    /// <param name="f">Outer function, defined in $[g(a), g(b^-)[$ or $[g(a), g(b^-)]$.</param>
    /// <param name="g">Inner function, non-negative and non-decreasing, defined in $[a, b[$.</param>
    /// <exception cref="ArgumentException">If the operands are not defined as expected.</exception>
    /// <returns>The result of the composition.</returns>
    /// <remarks>Algorithmic properties discussed in [ZNS23b].</remarks>
    public static Sequence Composition(Sequence f, Sequence g)
    {
        if (g.IsLeftOpen || g.IsRightClosed)
            throw new ArgumentException("g must be defined in a interval [a, b[");
        if (!g.IsNonNegative)
            throw new ArgumentException("g must be non-negative");
        if (!g.IsNonDecreasing)
            throw new ArgumentException("g must be non-decreasing");
        if (f.IsLeftOpen || 
            f.DefinedFrom != g.ValueAt(g.DefinedFrom) || f.DefinedUntil != g.LeftLimitAt(g.DefinedUntil))
            throw new ArgumentException("f must be defined in a interval [g(a), g(b^-)[ or [g(a), g(b^-)]");

        var gTimes = g.EnumerateBreakpoints()
            .Select(bp => bp.center.Time)
            .Append(g.DefinedUntil);
        var gInverse = g.LowerPseudoInverse();
        var fTimes = f.IsRightClosed
            ? f.EnumerateBreakpoints()
                .SkipLast(1)
                .Select(bp => gInverse.ValueAt(bp.center.Time))
            : f.EnumerateBreakpoints()
                .Select(bp => gInverse.ValueAt(bp.center.Time));

        var times = gTimes.Concat(fTimes)
            .OrderBy(t => t)
            .OrderedDistinct();
        var elements = EnumerateComposition(times).ToList();
        var merged = elements.Merge();
        var result = new Sequence(merged);
        return result;

        IEnumerable<Element> EnumerateComposition(IEnumerable<Rational> times)
        {
            // We need two breakpoints to compute the composition between them
            // Thus at each time we build the point and segment for the prevTime
            // For the last one, which is g.DefinedUntil, we do not define the composition because the support is right-open
            Rational? prevTime = null;
            int lastIndexF = 0, lastIndexG = 0;
            foreach (var time in times)
            {
                if (prevTime is { } pTime)
                {
                    var gValue = g.ValueAt(pTime);
                    var gRightLimit = g.RightLimitAt(pTime);
                    var p = new Point(pTime, f.ValueAt(gValue));
                    yield return p;
                    var (gSegment, gIndex) = g.GetSegmentAfter_Linear(pTime, lastIndexG);
                    lastIndexG = gIndex;
                    if (gSegment.Slope != 0)
                    {
                        var (fSegment, fIndex) = f.GetSegmentAfter_Linear(gRightLimit, lastIndexF);
                        lastIndexF = fIndex;
                        var s = new Segment(
                            pTime,
                            time,
                            fSegment.RightLimitAt(gRightLimit),
                            gSegment.Slope * fSegment.Slope
                        );
                        yield return s;
                    }
                    else
                    {
                        var (fElement, fIndex) = f.GetElementAt_Linear(gRightLimit, lastIndexF);
                        lastIndexF = fIndex;
                        var s = new Segment(
                            pTime,
                            time,
                            fElement.ValueAt(gRightLimit),
                            0
                        );
                        yield return s;
                    }
                }

                prevTime = time;
            }
        }
    }

    /// <summary>
    /// Compute the composition of this sequence, $f$, and $g$, i.e. $f(g(t))$, over a limited interval.
    /// This sequence must be defined in $[g(a), g(b^-)[$ or $[g(a), g(b^-)]$.
    /// </summary>
    /// <param name="g">Inner function, non-negative and non-decreasing, defined in $[a, b[$.</param>
    /// <exception cref="ArgumentException">If the operands are not defined as expected.</exception>
    /// <returns>The result of the composition.</returns>
    public Sequence Composition(Sequence g)
        => Composition(this, g);

    #endregion Composition
}