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
    /// Fills the gaps within [fillFrom, fillTo[ with $+\infty$.
    /// </summary>
    /// <param name="elements">Partial set of elements composing the sequence. Must be ordered, but can have gaps.</param>
    /// <param name="fillFrom">Left inclusive extreme of the filling interval.</param>
    /// <param name="fillTo">Right exclusive extreme of the filling interval.</param>
    public Sequence(IEnumerable<Element> elements, Rational fillFrom, Rational fillTo)
    {
        var filledElements = elements
            .Fill(fillFrom, fillTo)
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
    /// <remarks>The result is a well-formed sequence [ZS22]</remarks>
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
    /// Computes the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) >= x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    /// <param name="startFromZero">If true, it is assumed that $f^{-1}_\downarrow(x)$ be defined from $x = 0$.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS22]. 
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
    /// Algorithmic properties discussed in [ZNS22].
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
    /// <returns>The sequence resulting from the sum.</returns>
    public Sequence Addition(Sequence b)
        => Addition(this, b);

    /// <summary>
    /// Adds two sequences, over their overlapping parts.
    /// </summary>
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
                fillTo: Rational.Max(a.DefinedUntil, b.DefinedUntil)
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
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Described in [BT07], section 4.4.3</remarks>
    public static Sequence Convolution(Sequence a, Sequence b, ComputationSettings? settings = null, Rational? cutEnd = null)
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a [{a.GetHashString()}] of length {a.Count}:\n {a} \n and sequence b [{b.GetHashString()}] of length {b.Count}:\n {b}");
        #endif

        settings ??= ComputationSettings.Default();
        cutEnd ??= Rational.PlusInfinity;

        if (a.IsInfinite || b.IsInfinite)
        {
            var start = a.DefinedFrom + b.DefinedFrom;
            var end = Rational.Min(a.DefinedUntil + b.DefinedUntil, cutEnd.Value);
            return new Sequence(elements: new Element[]
            {
                Point.PlusInfinite(start),
                Segment.PlusInfinite(start, end)
            });
        }

        var areSequenceEqual = Equivalent(a, b);
        #if DO_LOG
        var countStopwatch = Stopwatch.StartNew();
        #endif
        var pairsCount = GetElementPairs().LongCount();
        #if DO_LOG
        countStopwatch.Stop();
        logger.Trace($"Convolution: counted {pairsCount} pairs in {countStopwatch.Elapsed}");
        #endif
            
        if (cutEnd.Value.IsFinite)
        {
            var earliestElement = a.DefinedFrom + b.DefinedFrom;
            #if DO_LOG
            logger.Trace($"cutEnd set to {cutEnd}, earliest element will be {earliestElement}");
            #endif
            if(cutEnd < earliestElement)
                throw new Exception("Convolution is cut before it starts");
        }
            
        if (settings.UseConvolutionPartitioning && pairsCount > settings.ConvolutionPartitioningThreshold)
            return PartitionedConvolution();
        else
        if (settings.UseParallelConvolution && pairsCount > settings.ConvolutionParallelizationThreshold)
            return ParallelConvolution();
        else
            return SerialConvolution();

        IEnumerable<(Element ea, Element eb)> GetElementPairs()
        {
            var elementPairs = a.Elements
                .Where(ea => ea.IsFinite)
                .SelectMany(ea => b.Elements
                    .Where(eb => eb.IsFinite)
                    .Where(eb => ea.StartTime + eb.StartTime < cutEnd)
                    .Select(eb => (a: ea, b: eb))
                );

            // if self-convolution, filter out symmetric pairs
            return areSequenceEqual ?
                elementPairs.Where(pair => pair.a.StartTime <= pair.b.StartTime) :
                elementPairs;
        }

        Sequence SerialConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running serial convolution, {pairsCount} pairs.");
            #endif
                
            var convolutionElements = GetElementPairs() 
                .SelectMany(pair => pair.ea.Convolution(pair.eb))
                .ToList();

            if (a.IsFinite && b.IsFinite)
            {
                return new Sequence(
                    elements: convolutionElements.LowerEnvelope(settings)
                );
            }
            else
            {
                // gaps may be expected
                return new Sequence(
                    elements: convolutionElements.LowerEnvelope(settings),
                    fillFrom: a.DefinedFrom + b.DefinedFrom,
                    fillTo: a.DefinedUntil + b.DefinedUntil
                );
            }
        }

        Sequence ParallelConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running parallel convolution, {pairsCount} pairs.");
            #endif

            var convolutionElements = GetElementPairs()
                .AsParallel()
                .SelectMany(pair => Element.Convolution(pair.ea, pair.eb))
                .ToList();

            if (a.IsFinite && b.IsFinite)
            {
                return new Sequence(
                    elements: convolutionElements.LowerEnvelope(settings)
                );
            }
            else
            {
                // gaps may be expected
                return new Sequence(
                    elements: convolutionElements.LowerEnvelope(settings),
                    fillFrom: a.DefinedFrom + b.DefinedFrom,
                    fillTo: a.DefinedUntil + b.DefinedUntil
                );
            }
        }

        // The elementPairs are partitioned in smaller sets (without any ordering)
        // From each set, the convolutions are computed and then their lower envelope
        // The resulting sequences will have gaps
        // Those partial convolutions are then merged via Sequence.LowerEnvelope
        Sequence PartitionedConvolution()
        {
            #if DO_LOG
            logger.Trace($"Running partitioned convolution, {pairsCount} pairs.");
            #endif
                
            var partialConvolutions = PartitionConvolutionElements()
                .Select(elements => new Sequence(
                    elements.LowerEnvelope(settings),
                    fillFrom: a.DefinedFrom + b.DefinedFrom,
                    fillTo: a.DefinedUntil + b.DefinedUntil
                ))
                .ToList();

            #if DO_LOG
            logger.Trace($"Partitioned convolutions computed, proceding with lower envelope of {partialConvolutions.Count} sequences");
            #endif
                
            if (a.IsFinite && b.IsFinite)
            {
                return new Sequence(
                    elements: partialConvolutions.LowerEnvelope(settings)
                );
            }
            else
            {
                // gaps may be expected
                return new Sequence(
                    elements: partialConvolutions.LowerEnvelope(settings),
                    fillFrom: a.DefinedFrom + b.DefinedFrom,
                    fillTo: a.DefinedUntil + b.DefinedUntil
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
                            .SelectMany(pair => pair.ea.Convolution(pair.eb))
                            .ToList()
                            .SortElements(settings);    
                    }
                    else
                    {
                        convolutionElements = partition
                            .SelectMany(pair => pair.ea.Convolution(pair.eb))
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
    /// <remarks>Described in [BT07], section 4.4.3</remarks>
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
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is `true`.
    /// </returns>
    public static long EstimateConvolution(Sequence a, Sequence b, ComputationSettings? settings = null, Rational? cutEnd = null, bool countElements = false)
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a [{a.GetHashString()}] of length {a.Count}:\n {a} \n and sequence b [{b.GetHashString()}] of length {b.Count}:\n {b}");
        #endif
        settings ??= ComputationSettings.Default();
        cutEnd ??= Rational.PlusInfinity;
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
                    .Where(eb => ea.StartTime + eb.StartTime <= cutEnd)
                    .Select(eb => (a: ea, b: eb))
                );

            // if self-convolution, filter out symmetric pairs
            return areSequenceEqual ?
                elementPairs.Where(pair => pair.a.StartTime <= pair.b.StartTime) :
                elementPairs;
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
    /// <param name="cutEnd">If not null, the result is cut or filled with $+\infty$ up to this time, extreme excluded.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution.</returns>
    /// <remarks>Described in [BT07], section 4.5</remarks>
    public static Sequence Deconvolution(Sequence a, Sequence b, Rational? cutStart = null, Rational? cutEnd = null, ComputationSettings? settings = null)
    {
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Convolution between sequence a:\n {a} \n and sequence b:\n {b}");
        #endif

        const int ParallelizationThreshold = 1_000;

        var elementPairs = a.Elements
            .SelectMany(ea => b.Elements
                .Select(eb => (a: ea, b: eb))
            )
            .Where(pair => pair.a.IsFinite && pair.b.IsFinite)
            .Where(pair => cutStart == null || pair.a.EndTime - pair.b.StartTime >= cutStart)
            .ToList();

        List<Element> result;
        if (elementPairs.Count <= ParallelizationThreshold)
            result = SimpleDeconvolution();
        else
            result = ParallelDeconvolution();

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
        
        List<Element> SimpleDeconvolution()
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
    /// <param name="cutEnd">If not null, the result is cut or filled with $+\infty$ up to this time, extreme excluded.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution.</returns>
    /// <remarks>Described in [BT07], section 4.5</remarks>
    public Sequence Deconvolution(Sequence sequence, Rational? cutStart = null, Rational? cutEnd = null, ComputationSettings? settings = null)
        => Deconvolution(a: this, b: sequence, cutStart, cutEnd, settings);

    #endregion Deconvolution operator
    
    #region Max-plus operators

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static Sequence MaxPlusConvolution(Sequence a, Sequence b, ComputationSettings? settings, Rational? cutEnd = null)
    {
        #if DO_LOG
        logger.Trace("Computing max-plus convolution");
        #endif
        return -Convolution(-a, -b, settings, cutEnd);
    }

    /// <summary>
    /// Computes the max-plus convolution of two sequences.
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="settings"></param>
    /// <param name="cutEnd">If defined, computation of convolutions beyond the given limit will be omitted.</param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public Sequence MaxPlusConvolution(Sequence sequence, ComputationSettings? settings, Rational? cutEnd = null)
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
    /// <remarks>Algorithmic properties discussed in [ZNS22].</remarks>
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
            // For the last one, which is g.DefinedFrom, we do not define the composition because the support is right-open
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
    /// <param name="fillFrom">Left inclusive extreme of the filling interval.</param>
    /// <param name="fillTo">Right exclusive extreme of the filling interval.</param>
    public static Sequence ToSequence(this IEnumerable<Element> elements, Rational fillFrom, Rational fillTo)
        => new Sequence(elements, fillFrom, fillTo);
    
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
    public static IEnumerable<Element> Merge(this IEnumerable<Element> elements)
    {
        using var enumerator = elements.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        // skip first point, if any
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
    public static IEnumerable<Rational> GetBreakpointValues(
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
    public static IEnumerable<Rational> GetBreakpointsValues(
        this IEnumerable<(Segment? left, Point center, Segment? right)> breakpoints)
    {
        return breakpoints
            .SelectMany(GetBreakpointValues);
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
    /// If the sequence is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public static Rational SupValue(this IEnumerable<Element> elements)
    {
        var breakpoints = elements.EnumerateBreakpoints();
        return breakpoints.GetBreakpointsValues().Max();
    }
    
    /// <summary>
    /// If the sequence is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for any $t \ge 0$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public static Rational InfValue(this IEnumerable<Element> elements)
    {
        var breakpoints = elements.EnumerateBreakpoints();
        return breakpoints.GetBreakpointsValues().Min();
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
            elements.Merge().SkipUntilValue(0):
            elements.Merge();
        
        using var enumerator = merged.GetEnumerator();

        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        Rational previousValue = startFromZero ? Rational.Zero : Rational.MinusInfinity;
        bool wasPreviousPoint = false;
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
                            if (!wasPreviousPoint)
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
                    else if (p.Value == previousValue && !wasPreviousPoint)
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
                            // do nothing, as the left-continuity point has already been processed
                        }
                        else if (s.RightLimitAtStartTime > previousValue)
                        {
                            // right-discontinuity, becomes constant segment
                            yield return new Segment(
                                startTime: previousValue,
                                endTime: s.RightLimitAtStartTime,
                                rightLimitAtStartTime: s.StartTime,
                                slope: 0
                            );
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
            elements.Merge().SkipUntilValue(0):
            elements.Merge();
        
        using var enumerator = merged.GetEnumerator();

        if (!enumerator.MoveNext())
            throw new ArgumentException("Elements is an empty collection");

        Rational previousValue = startFromZero ? Rational.Zero : Rational.MinusInfinity;
        bool wasPreviousPoint = false;
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
                            if (!wasPreviousPoint)
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
                    else if (p.Value == previousValue && !wasPreviousPoint)
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
                            
                            // right-discontinuity, becomes constant segment
                            yield return new Segment(
                                startTime: previousValue,
                                endTime: s.RightLimitAtStartTime,
                                rightLimitAtStartTime: s.StartTime,
                                slope: 0
                            );
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
        var intervals = Interval.ComputeIntervals(elements, settings);
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
        var intervals = Interval.ComputeIntervals(elements, settings);
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