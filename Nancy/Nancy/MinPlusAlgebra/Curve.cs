using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;

#if DO_LOG
using NLog;
using System.Diagnostics;
#endif

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Class of functions on which NetCal operators are defined.
/// They are piecewise affine and ultimately pseudo-periodic.
/// Pseudo-periodic means that $f(t + d) = f(t) + c$, where $c$ is the step gained after each pseudo-period.
/// Ultimately means that the function has such property for $t \ge T$.
/// </summary>
/// <remarks>
/// Implementation of data structure described in [BT07] Section 4.1
/// </remarks>
/// <docs position="1"/>
[JsonObject(MemberSerialization.OptIn)]
public class Curve
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    #region Properties

    /// <summary>
    /// Point in time after which the curve has a pseudo-periodic behavior.
    /// </summary>
    /// <remarks>
    /// Referred to as $T$ or Rank in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "periodStart")]
    public Rational PseudoPeriodStart { get; init; }

    /// <summary>
    /// Time length of each pseudo-period.
    /// </summary>
    /// <remarks>
    /// Referred to as $d$ in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "periodLength")]
    public Rational PseudoPeriodLength { get; init; }

    /// <summary>
    /// Static value gain applied after each pseudo-period.
    /// If it's 0, the curve is truly periodic.
    /// </summary>
    /// <remarks>
    /// Referred to as $c$ in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "periodHeight")]
    public Rational PseudoPeriodHeight { get; init; }

    /// <summary>
    /// Average slope of curve in pseudo-periodic behavior.
    /// If it's 0, the curve is truly periodic.
    /// </summary>
    public Rational PseudoPeriodAverageSlope =>
        PseudoPeriodicSequence.IsInfinite
            ? (PseudoPeriodicElements.First().IsPlusInfinite ? Rational.PlusInfinity : Rational.MinusInfinity)
            : PseudoPeriodHeight / PseudoPeriodLength;

    /// <summary>
    /// End time of the first pseudo period.
    /// </summary>
    public Rational FirstPseudoPeriodEnd =>
        PseudoPeriodStart + PseudoPeriodLength;

    /// <summary>
    /// End time of the second pseudo period.
    /// </summary>
    public Rational SecondPseudoPeriodEnd =>
        FirstPseudoPeriodEnd + PseudoPeriodLength;

    /// <summary>
    /// <see cref="Sequence"/> describing behavior of the curve in $[0, T + d[$.
    /// Combined with the UPP property, this is also allows to derive $f(t)$ for any $t \ge T + d$.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_1, ..., t_k]$ in [BT07] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "sequence")]
    public Sequence BaseSequence { get; init; }

    /// <summary>
    /// True if the curve has finite value for any $t$.
    /// </summary>
    public bool IsFinite => BaseSequence.IsFinite;

    /// <summary>
    /// The first instant around which the curve is not infinite.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is finite.
    /// </summary>
    public Rational FirstFiniteTime => BaseSequence.FirstFiniteTime;

    /// <summary>
    /// The first instant around which the curve is not infinite, excluding the origin point.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is finite.
    /// </summary>
    public Rational FirstFiniteTimeExceptOrigin
    {
        get
        {
            var inBaseSequence = BaseSequence.FirstFiniteTimeAfter(0);
            if (inBaseSequence.IsFinite)
                return inBaseSequence;
            else if (ValueAt(PseudoPeriodStart).IsFinite && PseudoPeriodHeight.IsFinite)
                return FirstPseudoPeriodEnd;
            else
                return Rational.PlusInfinity;
        }
    }

    /// <summary>
    /// The first instant around which the curve is not 0.
    /// Does not specify whether it's inclusive or not, i.e. if $f(\overline{t}) = 0$.
    /// </summary>
    public Rational FirstNonZeroTime =>
        IsIdenticallyZero
            ? Rational.PlusInfinity
            : Rational.Min(BaseSequence.FirstNonZeroTime, PseudoPeriodStart + PseudoPeriodLength);

    /// <summary>
    /// True if the curve has 0 value for any $t$.
    /// </summary>
    public bool IsIdenticallyZero =>
        BaseSequence.IsIdenticallyZero && PseudoPeriodHeight.IsZero;

    /// <summary>
    /// True if there is no infinite value or discontinuity within the curve.
    /// </summary>
    public bool IsContinuous
    {
        get
        {
            if (!BaseSequence.IsContinuous)
                return false;

            Rational pseudoPeriodStartValue = ((Point)PseudoPeriodicElements.First()).Value;
            Rational pseudoPeriodLastValue = ((Segment)PseudoPeriodicElements.Last()).LeftLimitAtEndTime;

            bool isPeriodicContinuous = pseudoPeriodStartValue + PseudoPeriodHeight == pseudoPeriodLastValue;

            return isPeriodicContinuous;
        }
    }

    /// <summary>
    /// True if there is no infinite or discontinuity within the curve, except at most in origin.
    /// </summary>
    public bool IsContinuousExceptOrigin
    {
        get
        {
            if (!BaseSequence
                    .Cut(0, BaseSequence.DefinedUntil, isStartInclusive: false)
                    .IsContinuous)
                return false;

            Rational pseudoPeriodStartValue = ((Point)PseudoPeriodicElements.First()).Value;
            Rational pseudoPeriodLastValue = ((Segment)PseudoPeriodicElements.Last()).LeftLimitAtEndTime;

            bool isPeriodicContinuous = pseudoPeriodStartValue + PseudoPeriodHeight == pseudoPeriodLastValue;

            return isPeriodicContinuous;
        }
    }

    /// <summary>
    /// True if there is no infinite or left-discontinuity within the curve.
    /// </summary>
    public bool IsLeftContinuous
        => Cut(0, SecondPseudoPeriodEnd).IsLeftContinuous;

    /// <summary>
    /// True if there is no infinite or right-discontinuity within the curve.
    /// </summary>
    public bool IsRightContinuous
        => Cut(0, SecondPseudoPeriodEnd).IsRightContinuous;
    
    /// <summary>
    /// True if the curve is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsContinuousAt(Rational time)
        => time > 0 ? 
            IsLeftContinuousAt(time) && IsRightContinuousAt(time) : IsRightContinuousAt(time);
    
    /// <summary>
    /// True if the curve is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsLeftContinuousAt(Rational time)
        => LeftLimitAt(time) == ValueAt(time);
    
    /// <summary>
    /// True if the curve is continuous at <paramref name="time"/>.
    /// </summary>
    public bool IsRightContinuousAt(Rational time)
        => RightLimitAt(time) == ValueAt(time);
    
    /// <summary>
    /// True if the curve is non-negative, i.e. $f(t) \ge 0$ for any $t$.
    /// </summary>
    public bool IsNonNegative
        => MinValue() >= 0;
    
    /// <summary>
    /// The first instant around which the curve is non-negative.
    /// Does not specify whether it's inclusive or not, i.e. if $f(\overline{t}) >= 0$.
    /// </summary>
    public Rational FirstNonNegativeTime
    {
        get
        {
            if (IsNonNegative)
                return 0;

            var t = FindFirstNonNegativeInSequence(BaseSequence.Elements);
            if (t != Rational.PlusInfinity)
                return t;

            if (PseudoPeriodAverageSlope <= 0)
                return Rational.PlusInfinity;
            else
            {
                var k = (-ValueAt(FirstPseudoPeriodEnd) / PseudoPeriodAverageSlope).FastFloor();
                return FindFirstNonNegativeInSequence(
                    CutAsEnumerable(PseudoPeriodStart + k * PseudoPeriodLength, PseudoPeriodStart + (k + 1) * PseudoPeriodLength, isEndInclusive: true)
                );
            }
            
            Rational FindFirstNonNegativeInSequence(IEnumerable<Element> elements)
            {
                foreach (var element in elements)
                {
                    switch (element)
                    {
                        case Point p:
                        {
                            if (p.Value >= 0)
                                return p.Time;
                            break;
                        }
                        case Segment s:
                        {
                            if (s.RightLimitAtStartTime >= 0)
                                return s.StartTime;
                            if (s.Slope > 0)
                            {
                                var t = s.StartTime + (-s.RightLimitAtStartTime / s.Slope);
                                if (t <= s.EndTime)
                                    return t;
                            }
                            break;
                        }
                    }
                }
                return Rational.PlusInfinity;
            }
        }
    }
    
    /// <summary>
    /// True if for any $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    public bool IsNonDecreasing
        => Cut(0, SecondPseudoPeriodEnd).IsNonDecreasing;

    /// <summary>
    /// True if for all $t \ge$ <see cref="PseudoPeriodStart"/> the curve is finite.
    /// </summary>
    /// <remarks>
    /// This property does not check if $f(t), t &lt;$ <see cref="PseudoPeriodStart"/> is either infinite, finite or both.
    /// </remarks>
    public bool IsUltimatelyFinite =>
        PseudoPeriodicElements.All(elem => elem.IsFinite);

    /// <summary>
    /// True if, for some $T$, $\left|f(t)\right| = +\infty$ for all $t \ge T$,
    /// while for $\left|f(t)\right| &lt; +\infty$ for all $t &lt; T$.
    /// </summary>
    public bool IsUltimatelyInfinite
    {
        get
        {
            using var enumerator = BaseSequence.Elements.GetEnumerator();
            enumerator.MoveNext();
            while (enumerator.Current.IsFinite)
            {
                if (!enumerator.MoveNext())
                    return false;   // if reached, the base sequence is finite
            }

            if (enumerator.Current is Segment)
                return false;   // weakly U.I. but not U.I.

            var sign = enumerator.Current.IsPlusInfinite ? 1 : -1;
            
            // loop through the rest, ensuring they are all infinite
            while (enumerator.MoveNext())
            {
                if (!enumerator.Current.IsInfinite || 
                    !(sign > 0 ? enumerator.Current.IsPlusInfinite : enumerator.Current.IsMinusInfinite))  // ensure the sign stays the same
                    return false;
            }

            return true;
        }
    }
        
    /// <summary>
    /// True if, for some $T$, $\left|f(t)\right| = +\infty$ for all $t > T$,
    /// while for $\left|f(t)\right| &lt; +\infty$ for all $t &lt; T$.
    /// </summary>
    /// <remarks>
    /// The value $f(T)$ is not specified either way.
    /// <see cref="IsUltimatelyInfinite"/> implies <see cref="IsWeaklyUltimatelyInfinite"/>.
    /// </remarks>
    public bool IsWeaklyUltimatelyInfinite
    {
        get
        {
            using var enumerator = Extend(SecondPseudoPeriodEnd).Elements.GetEnumerator();
            enumerator.MoveNext();
            while (enumerator.Current.IsFinite)
            {
                if (!enumerator.MoveNext())
                    return false;   // if reached, the base sequence is finite
            }
            
            var sign = enumerator.Current.IsPlusInfinite ? 1 : -1;
            
            // loop through the rest, ensuring they are all infinite
            while (enumerator.MoveNext())
            {
                if (!enumerator.Current.IsInfinite || 
                    !(sign > 0 ? enumerator.Current.IsPlusInfinite : enumerator.Current.IsMinusInfinite))  // ensure the sign stays the same
                    return false;
            }

            return true;
        }
    }
    
    /// <summary>
    /// True if for all $t >$ <see cref="PseudoPeriodStart"/> the curve is either always finite or always infinite.
    /// </summary>
    public bool IsUltimatelyPlain =>
        IsUltimatelyFinite || IsWeaklyUltimatelyInfinite;

    /// <summary>
    /// True if for all $t \ge$ <see cref="PseudoPeriodStart"/> the curve is affine.
    /// </summary>
    public bool IsUltimatelyAffine =>
        IsUltimatelyFinite
        && PseudoPeriodicElements.Count() == 2
        && ((Segment)PseudoPeriodicElements.Last()).Slope == PseudoPeriodAverageSlope
        && IsContinuousAt(FirstPseudoPeriodEnd);

    /// <summary>
    /// True if for $t \ge$ <see cref="PseudoPeriodStart"/> the curve is constant.
    /// </summary>
    public bool IsUltimatelyConstant =>
        IsUltimatelyAffine && PseudoPeriodAverageSlope == 0;

    // todo: add reference to proof

    /// <summary>
    /// True if the curve is sub-additive, i.e. $f(t+s) \le f(t) + f(s)$.
    /// </summary>
    /// <remarks>
    /// Based on the following property: $f(0) \ge 0, f$ is sub-additive $\iff f^\circ = f^\circ \otimes f^\circ$,
    /// where $f^\circ$ is defined in <see cref="Curve.WithZeroOrigin"/>.
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    public virtual bool IsSubAdditive
    {
        get
        {
            return _IsSubAdditive ??= CheckIsSubAdditive();

            bool CheckIsSubAdditive()
            {
                if (ValueAt(0) >= 0)
                {
                    var f_circ = WithZeroOrigin();
                    var selfConv = Convolution(f_circ, f_circ);
                    return Equivalent(f_circ, selfConv);
                }
                else
                {
                    // this check is more restrictive than the property described in [DNC18]
                    return Equivalent(MinusInfinite());
                }
            }
        }
    }

    /// <summary>
    /// Private cache field for IsSubAdditive.
    /// </summary>
    private bool? _IsSubAdditive;

    /// <summary>
    /// True if the curve is sub-additive with $f(0) = 0$.
    /// </summary>
    public bool IsRegularSubAdditive
        => IsSubAdditive && ValueAt(0) == 0;

    /// <summary>
    /// True if the curve is super-additive, i.e. $f(t+s) \ge f(t) + f(s)$.
    /// </summary>
    /// <remarks>
    /// Based on the following property: $f$ is super-additive $\iff f^\circ = f^\circ \overline{\otimes} f^\circ$,
    /// where $f^\circ$ is defined in <see cref="Curve.WithZeroOrigin"/>.
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    public virtual bool IsSuperAdditive
    {
        get
        {
            return _IsSuperAdditive ??= CheckIsSuperAdditive();

            bool CheckIsSuperAdditive()
            {
                if(ValueAt(0) <= 0)
                {
                    var f_circ = WithZeroOrigin();
                    var selfConv = MaxPlusConvolution(f_circ, f_circ);
                    return Equivalent(f_circ, selfConv);
                }
                else
                {
                    // this check may be more restrictive than necessary 
                    return Equivalent(PlusInfinite());
                }
            }
        }
    }
        
    /// <summary>
    /// Private cache field for IsSuperAdditive.
    /// </summary>
    private bool? _IsSuperAdditive;

    /// <summary>
    /// True if the curve is super-additive with $f(0) = 0$.
    /// </summary>
    public bool IsRegularSuperAdditive
        => IsSuperAdditive && ValueAt(0) == 0;


    /// <summary>
    /// Tests if the curve is concave, 
    /// i.e. for any two points $(t, f(t))$ the straight line joining them is below $f$.
    /// </summary>
    /// <remarks>
    /// The property is checked via the following property: $f$ is concave $\iff$ 
    /// a) $f$ is continuous, or it is continuous for $t > 0$ and $f(0) \le f(0^+)$, and
    /// b) $f$ is composed of segments with decreasing slopes.
    /// </remarks>
    public bool IsConcave
    {
        get
        {
            if (IsContinuousExceptOrigin && ValueAt(0) <= RightLimitAt(0) && IsUltimatelyAffine)
            {
                var prevSlope = Rational.PlusInfinity;
                foreach (var element in BaseSequence.Elements)
                {
                    if (element is Segment s)
                    {
                        if (s.Slope > prevSlope)
                            return false;
                        prevSlope = s.Slope;
                    }
                }

                return true;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// Tests if the curve is concave with $f(0) = 0$.
    /// </summary>
    public bool IsRegularConcave
        => IsConcave && ValueAt(0) == 0;

    /// <summary>
    /// Tests if the curve is convex, 
    /// i.e. for any two points $(t, f(t))$ the straight line joining them is above $f$.
    /// </summary>
    /// <remarks>
    /// The property is checked via the following property: $f$ is convex $\iff$ 
    /// a) $f$ is continuous, or it is continuous for $t > 0$ and $f(0) \ge f(0^+)$, and
    /// b) $f$ is composed of segments with increasing slopes.
    /// </remarks>
    public bool IsConvex
    {
        get
        {
            if (IsContinuousExceptOrigin && ValueAt(0) >= RightLimitAt(0) && IsUltimatelyAffine)
            {
                var prevSlope = Rational.MinusInfinity;
                foreach (var element in BaseSequence.Elements)
                {
                    if (element is Segment s)
                    {
                        if (s.Slope < prevSlope)
                            return false;
                        prevSlope = s.Slope;
                    }
                }

                return true;
            }
            else
                return false;
        }
    }

    /// <summary>
    /// Tests if the curve is convex with $f(0) = 0$.
    /// </summary>
    public bool IsRegularConvex
        => IsConvex && ValueAt(0) == 0;

    /// <summary>
    /// True if pseudo-periodic behavior starts at $T > 0$.
    /// </summary>
    public bool HasTransient =>
        PseudoPeriodStart > 0;

    /// <summary>
    /// Sequence describing the curve in $[0, T[$, before pseudo-periodic behavior.
    /// </summary>
    public Sequence? TransientSequence 
        => HasTransient ? BaseSequence.Cut(0, PseudoPeriodStart) : null;

    /// <summary>
    /// Elements describing the curve from $[0, T[$, before pseudo-periodic behavior.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_1, ..., t_{i_0 - 1}]$ in [BT07] Section 4.1
    /// </remarks>
    public IEnumerable<Element> TransientElements 
        => TransientSequence?.Elements ?? Enumerable.Empty<Element>();

    /// <summary>
    /// Sequence describing the pseudo-periodic behavior of the curve in $[T, T + d[$.
    /// </summary>
    public Sequence PseudoPeriodicSequence =>
        BaseSequence.Cut(PseudoPeriodStart, FirstPseudoPeriodEnd);

    /// <summary>
    /// Elements describing the pseudo-periodic behavior of the curve in $[T, T + d[$.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_{i_0}, ..., t_k]$ in [BT07] Section 4.1
    /// </remarks>
    public IEnumerable<Element> PseudoPeriodicElements =>
        PseudoPeriodicSequence.Elements;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="baseSequence">Describes the curve in $[0, T + d[$.</param>
    /// <param name="pseudoPeriodStart">Time from which the curve is pseudo-periodic, $T$.</param>
    /// <param name="pseudoPeriodLength">Length of each pseudo-period, $d$.</param>
    /// <param name="pseudoPeriodHeight">Height gained after each pseudo-period, $c$.</param>
    /// <param name="isPartialCurve">True if the curve is partially described, and should be filled by pre- and post-pending $+\infty$ to <see cref="BaseSequence"/>.</param>
    public Curve(
        Sequence baseSequence,
        Rational pseudoPeriodStart,
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight,
        bool isPartialCurve = false
    )
    {
        if (baseSequence.DefinedFrom != 0 || baseSequence.DefinedUntil != pseudoPeriodStart + pseudoPeriodLength)
        {
            if (isPartialCurve)
            {
                baseSequence = new Sequence(
                    elements: baseSequence.Elements,
                    fillFrom: Rational.Zero,
                    fillTo: pseudoPeriodStart + pseudoPeriodLength);
            }
            else
            {
                throw new ArgumentException("Base sequence must start at t = 0 and end at T + d");
            }
        }

        BaseSequence = baseSequence
            .Optimize()
            .Cut(cutStart: 0, cutEnd: pseudoPeriodStart + pseudoPeriodLength, isStartInclusive: true, isEndInclusive: false)
            .EnforceSplitAt(pseudoPeriodStart);
        PseudoPeriodStart = pseudoPeriodStart;
        PseudoPeriodLength = pseudoPeriodLength;
        PseudoPeriodHeight = pseudoPeriodHeight;
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other">The <see cref="Curve"/> object to copy from.</param>
    public Curve(Curve other)
    {
        BaseSequence = other.BaseSequence;
        PseudoPeriodStart = other.PseudoPeriodStart;
        PseudoPeriodLength = other.PseudoPeriodLength;
        PseudoPeriodHeight = other.PseudoPeriodHeight;
    }

    /// <summary>
    /// Constructs a curve that is equal to $+\infty$ over any $t$.
    /// </summary>
    public static Curve PlusInfinite()
    {
        return new Curve(
            baseSequence: new Sequence(new Element[] {
                Point.PlusInfinite(0),
                Segment.PlusInfinite(0, 1)
            }),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );
    }
        
    /// <summary>
    /// Constructs a curve that is equal to $-\infty$ over any $t$.
    /// </summary>
    public static Curve MinusInfinite()
    {
        return new Curve(
            baseSequence: new Sequence(new Element[] {
                Point.MinusInfinite(0),
                Segment.MinusInfinite(0, 1)
            }),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );
    }

    /// <summary>
    /// Constructs a curve that is equal to 0 over any $t$.
    /// </summary>
    public static Curve Zero()
    {
        return new ConstantCurve(0);
    }
        
    #endregion

    #region Equality and comparison methods

    /// <inheritdoc cref="object.Equals(object?)"/>
    public override bool Equals(object? obj)
    {
        if (!(obj is Curve curve))
            return false;

        return (BaseSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight: PseudoPeriodHeight) ==
               (curve.BaseSequence, curve.PseudoPeriodStart, curve.PseudoPeriodLength, curve.PseudoPeriodHeight);
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode()
        => (BaseSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight: PseudoPeriodHeight).GetHashCode();

    /// <summary>
    /// Returns <code>true</code> if its operands are equal, <code>false</code> otherwise
    /// </summary>
    public static bool operator ==(Curve? a, Curve? b) =>
        Equals(a, b);

    /// <summary>
    /// Returns <code>false</code> if its operands are equal, <code>true</code> otherwise
    /// </summary>
    public static bool operator !=(Curve? a, Curve? b) =>
        !Equals(a, b);

    /// <summary>
    /// True if the curves represent the same function.
    /// </summary>
    public bool Equivalent(Curve curve)
        => Equivalent(this, curve);

    /// <summary>
    /// True if the curves represent the same function.
    /// </summary>
    public static bool Equivalent(Curve a, Curve b, ComputationSettings? settings = null)
    {
        Rational extensionTime = Rational.Max(a.FirstPseudoPeriodEnd, b.FirstPseudoPeriodEnd);
            
        var seqA = a.CutAsEnumerable(0, extensionTime, true, true, settings);
        var seqB = b.CutAsEnumerable(0,extensionTime, true, true, settings);

        return Sequence.Equivalent(seqA, seqB);
    }

    //todo: write tests
    /// <summary>
    /// Returns the first time around which the functions represented by the curves differ.
    /// Returns null if the two curves represent the same function.
    /// Mostly useful to debug curves that *should* be equivalent.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static Rational? FindFirstInequivalence(Curve a, Curve b)
    {
        //Should be good enough
        Rational extensionTime = Rational.Max(a.FirstPseudoPeriodEnd, b.FirstPseudoPeriodEnd);

        var seqA = a.CutAsEnumerable(0, extensionTime, true, true);
        var seqB = b.CutAsEnumerable(0,extensionTime, true, true);

        var spotsA = GetSpots(seqA);
        var spotsB = GetSpots(seqB);
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
    /// True if the curves represent the same function, except for origin.
    /// </summary>
    public bool EquivalentExceptOrigin(Curve curve)
    {
        Rational extensionTime = Rational.Max(
            PseudoPeriodStart + 3 * PseudoPeriodLength,
            curve.PseudoPeriodStart + 3 * curve.PseudoPeriodLength
        );

        Sequence a = Extend(extensionTime)
            .Cut(0, extensionTime, isStartInclusive: false)
            .Optimize();
        Sequence b = curve.Extend(extensionTime)
            .Cut(0, extensionTime, isStartInclusive: false)
            .Optimize();

        return a == b;
    }

    /// <summary>
    /// True if the first curve is a lower bound for the second one.
    /// </summary>
    public static bool operator <=(Curve a, Curve b)
    {
        return a.Equivalent(Minimum(a, b));
    }
        
    /// <summary>
    /// True if the first curve is an upper bound for the second one.
    /// </summary>
    public static bool operator >=(Curve a, Curve b)
    {
        return b <= a;
    }

    /// <summary>
    /// Checks if there is dominance between the curves given and, if so, returns their order.
    /// </summary>
    /// <remarks>If there is no dominance, the ordering has no actual meaning</remarks>
    public static (bool Verified, Curve Lower, Curve Upper) Dominance(Curve a, Curve b, ComputationSettings? settings = null)
    {
        if (a == b)
            return (true, a, b);
                
        var minimum = Minimum(a, b, settings);
        if (Equivalent(a, minimum))
            return (true, a, b);
        else if (Equivalent(b, minimum))
            return (true, b, a);
        else
            return (false, a, b);
    }
        
    /// <summary>
    /// Checks if there is asymptotic dominance between the curves given and, if so, returns their order.
    /// </summary>
    /// <remarks>If there is no dominance, the ordering has no actual meaning</remarks>
    public static (bool Verified, Curve Lower, Curve Upper) AsymptoticDominance(Curve a, Curve b, ComputationSettings? settings = null)
    {
        if (a.PseudoPeriodAverageSlope < b.PseudoPeriodAverageSlope)
            return (true, a, b);
        else if (b.PseudoPeriodAverageSlope < a.PseudoPeriodAverageSlope)
            return (true, b, a);
        else
        {
            var minimum = Minimum(a, b);
            if (a.Match(minimum.PseudoPeriodicSequence))
                return (true, a, b);
            else if (b.Match(minimum.PseudoPeriodicSequence))
                return (true, b, a);
            else
                return (false, a, b);
        }
    }
        
    /// <summary>
    /// True if the curve is below the point
    /// </summary>
    public static bool operator <=(Curve c, Point p)
    {
        return c.ValueAt(p.Time) <= p.Value;
    }

    /// <summary>
    /// True if the curve is above the point
    /// </summary>
    public static bool operator >=(Curve c, Point p)
    {
        return c.ValueAt(p.Time) >= p.Value;
    }

    /// <summary>
    /// Returns the opposite function, $g(t) = -f(t)$.
    /// </summary>
    public Curve Negate()
    {
        if (IsIdenticallyZero)
            return this;
            
        return new Curve(
            baseSequence: -BaseSequence,
            pseudoPeriodStart: PseudoPeriodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: -PseudoPeriodHeight
        );
    }

    /// <summary>
    /// Returns the opposite function, $g(t) = -f(t)$
    /// </summary>
    public static Curve operator -(Curve c)
        => c.Negate();

    #endregion

    #region Methods

    /// <summary>
    /// Computes the value of the curve at given time $t$.
    /// </summary>
    /// <param name="time">The time of sampling.</param>
    /// <param name="settings"></param>
    /// <returns>The value of $f(t)$.</returns>
    public Rational ValueAt(Rational time, ComputationSettings? settings = null)
    {
        Element element = GetActiveElementAt(time, settings);
        return element.ValueAt(time);
    }

    /// <summary>
    /// Computes the right limit of the curve at given time $t$.
    /// </summary>
    /// <param name="time">The target time of the limit.</param>
    /// <returns>The value of $f(t^+)$.</returns>
    public Rational RightLimitAt(Rational time)
    {
        Segment segment = GetActiveSegmentAfter(time);
        return (segment.StartTime == time) ?
            segment.RightLimitAtStartTime 
            : segment.ValueAt(time);
    }

    /// <summary>
    /// Computes the left limit of the curve at given time $t$.
    /// </summary>
    /// <param name="time">The target time of the limit.</param>
    /// <returns>The value of $f(t^-)$.</returns>
    public Rational LeftLimitAt(Rational time)
    {
        if (time == 0)
            throw new ArgumentException("A curve is not defined for t < 0");
                
        Segment segment = GetActiveSegmentBefore(time);
        return (segment.EndTime == time) ?
            segment.LeftLimitAtEndTime 
            : segment.ValueAt(time);
    }

    /// <summary>
    /// Returns the <see cref="Element"/> that describes the curve in time t.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException">Thrown if t is less than 0.</exception>
    /// <returns>The element describing the curve at time t.</returns>
    public Element GetActiveElementAt(Rational time, ComputationSettings? settings = null)
    {
        if (time < 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time < FirstPseudoPeriodEnd)
            return BaseSequence.GetActiveElementAt(time);

        //otherwise
        return GetExtensionSequenceAt(time, settings).GetActiveElementAt(time);
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the curve before time t.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if time 0 is given, as a curve is not defined before 0.</exception>
    /// <returns>The <see cref="Segment"/> describing the curve before time t, or null if there is none.</returns>
    public Segment GetActiveSegmentBefore(Rational time)
    {
        if (time == 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time <= FirstPseudoPeriodEnd)
            return BaseSequence.GetActiveSegmentBefore(time);

        //otherwise
        return GetExtensionSequenceBefore(time).GetActiveSegmentBefore(time);
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the curve after time t.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <returns>The <see cref="Segment"/> describing the curve after time t.</returns>
    public Segment GetActiveSegmentAfter(Rational time)
    {
        if (time < FirstPseudoPeriodEnd)
            return BaseSequence.GetActiveSegmentAfter(time);
        else
            return GetExtensionSequenceAt(time).GetActiveSegmentAfter(time);
    }
        
    /// <summary>
    /// True if the given element matches, in its domain, with the curve.
    /// </summary>
    public bool Match(Element element, ComputationSettings? settings = null)
    {
        switch (element)
        {
            case Point p:
                return ValueAt(p.Time, settings) == p.Value;

            case Segment s:
            {
                var cut = Cut(s.StartTime, s.EndTime, false, false, settings);
                if (cut.Elements.Count == 1 && cut.Elements.Single() is Segment s2)
                    return s == s2;
                else
                    return false;
            }
                
            default:
                throw new InvalidCastException();
        }
    }

    /// <summary>
    /// True if the given sequence matches, in its domain, with the curve.
    /// </summary>
    public bool Match(Sequence sequence, ComputationSettings? settings = null)
    {
        return sequence.Elements
            .All(e => Match(e, settings));
    }
        
    private Sequence GetExtensionSequenceAt(Rational time, ComputationSettings? settings = null)
    {
        int targetPeriodIndex = (int)Math.Floor((decimal)((time - FirstPseudoPeriodEnd) / PseudoPeriodLength)) + 1;
        return ComputeExtensionSequence(targetPeriodIndex, settings);
    }

    private Sequence GetExtensionSequenceBefore(Rational time)
    {
        int targetPeriodIndex = (int)Math.Ceiling((decimal)((time - FirstPseudoPeriodEnd) / PseudoPeriodLength));
        return ComputeExtensionSequence(targetPeriodIndex);
    }

    /// <summary>
    /// Returns a cut of the curve limited to the given domain.
    /// </summary>
    /// <param name="cutStart">Left extreme of the domain.</param>
    /// <param name="cutEnd">Right extreme of the domain.</param>
    /// <param name="isStartInclusive">If true, the domain is left-closed.</param>
    /// <param name="isEndInclusive">If true, the domain is right-closed.</param>
    /// <param name="settings"></param>
    /// <returns>A list of elements equivalently defined within the given domain.</returns>
    /// <remarks>Optimized for minimal allocations.</remarks>
    public IEnumerable<Element> CutAsEnumerable(
        Rational cutStart,
        Rational cutEnd,
        bool isStartInclusive = true,
        bool isEndInclusive = false,
        ComputationSettings? settings = null
    )
    {
        settings ??= ComputationSettings.Default();
            
        if (cutEnd > BaseSequence.DefinedUntil || (isEndInclusive && cutEnd == BaseSequence.DefinedUntil))
        {
            if (cutStart > BaseSequence.DefinedUntil)
            {
                var startingPseudoPeriodIndex = ((cutStart - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                if (isEndInclusive) 
                    endingPseudoPeriodIndex++;

                var indexes = settings.UseParallelExtend
                    ? Enumerable.Range(startingPseudoPeriodIndex,
                            (endingPseudoPeriodIndex - startingPseudoPeriodIndex + 1))
                        .AsParallel()
                    : Enumerable.Range(startingPseudoPeriodIndex,
                        (endingPseudoPeriodIndex - startingPseudoPeriodIndex + 1));
                    
                var elements = indexes
                    .Select(i => ComputeExtensionSequenceAsEnumerable(i, settings))
                    .SelectMany(en => en);

                return elements.Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive);
            }
            else
            {
                var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                if (isEndInclusive) 
                    endingPseudoPeriodIndex++;

                var indexes = settings.UseParallelExtend
                    ? Enumerable.Range(1, endingPseudoPeriodIndex).AsParallel()
                    : Enumerable.Range(1, endingPseudoPeriodIndex);

                var extensionElements = indexes
                    .Select(i => ComputeExtensionSequenceAsEnumerable(i, settings))
                    .SelectMany(en => en);

                var elements = BaseSequence.Elements.Concat(extensionElements);

                return elements.Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive);
            }
        }
        else
        {
            return BaseSequence
                .CutAsEnumerable(cutStart, cutEnd, isStartInclusive, isEndInclusive);
        }
    }

    /// <summary>
    /// Returns a cut of the curve limited to the given domain.
    /// </summary>
    /// <param name="cutStart">Left extreme of the domain.</param>
    /// <param name="cutEnd">Right extreme of the domain.</param>
    /// <param name="isStartInclusive">If true, the domain is left-closed.</param>
    /// <param name="isEndInclusive">If true, the domain is right-closed.</param>
    /// <param name="settings"></param>
    /// <returns>A sequence equivalently defined within the given domain.</returns>
    public Sequence Cut(
        Rational cutStart, 
        Rational cutEnd,
        bool isStartInclusive = true,
        bool isEndInclusive = false,
        ComputationSettings? settings = null
    )
    {
        if (cutStart < 0)
            throw new ArgumentException("Cannot cut from before zero");
        
        if (cutEnd.IsInfinite || cutStart.IsInfinite)
            throw new ArgumentException("Cannot cut to infinity");

        if (cutStart > cutEnd)
            throw new ArgumentException("Cut start cannot be after end.");
        
        settings ??= ComputationSettings.Default();
            
        if (cutEnd > BaseSequence.DefinedUntil || (isEndInclusive && cutEnd == BaseSequence.DefinedUntil))
        {
            if (IsUltimatelyAffine)
            {
                if (cutStart >= PseudoPeriodStart)
                {
                    var bsLastSegment = (Segment) BaseSequence.Elements.Last();
                    var pseudoPeriodStartValue = bsLastSegment.RightLimitAtStartTime;
                    var valueAtStart = pseudoPeriodStartValue +
                                       (cutStart - bsLastSegment.StartTime) * bsLastSegment.Slope;
                    var elements = new List<Element>();
                    if(isStartInclusive)
                        elements.Add(new Point(cutStart, valueAtStart));
                    elements.Add(new Segment(cutStart, cutEnd, valueAtStart, bsLastSegment.Slope));
                    if (isEndInclusive)
                    {
                        var valueAtEnd = pseudoPeriodStartValue +
                                       (cutEnd - bsLastSegment.StartTime) * bsLastSegment.Slope;
                        elements.Add(new Point(cutEnd, valueAtEnd));
                    }

                    return elements
                        .ToSequence();
                }
                else
                {
                    var bsLastSegment = (Segment) BaseSequence.Elements.Last();
                    var lastSegment = new Segment(
                        startTime: PseudoPeriodStart,
                        endTime: cutEnd,
                        rightLimitAtStartTime: bsLastSegment.RightLimitAtStartTime,
                        slope: bsLastSegment.Slope
                    );
                    var elements = BaseSequence.Elements
                        .SkipLast(1)
                        .Append(lastSegment);
                    
                    if (isEndInclusive)
                    {
                        var lastPoint = new Point(cutEnd, lastSegment.LeftLimitAtEndTime);
                        elements = elements.Append(lastPoint);
                    }

                    return elements
                        .Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive)
                        .ToSequence();
                }
            }
            else
            {
                if (cutStart > BaseSequence.DefinedUntil)
                {
                    var startingPseudoPeriodIndex = ((cutStart - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    if (isEndInclusive)
                        endingPseudoPeriodIndex++;

                    var indexes = Enumerable.Range(startingPseudoPeriodIndex,
                        (endingPseudoPeriodIndex - startingPseudoPeriodIndex + 1));

                    IEnumerable<Element> elements;
                    if (settings.UseParallelExtend)
                    {
                        elements = indexes
                            .AsParallel().AsOrdered()
                            .Select(i => ComputeExtensionSequence(i, settings))
                            .SelectMany(seq => seq.Elements);
                    }
                    else
                    {
                        elements = indexes
                            .Select(i => ComputeExtensionSequence(i, settings))
                            .SelectMany(seq => seq.Elements);
                    }

                    return elements
                        .Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive)
                        .ToSequence();
                }
                else
                {
                    var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    if (isEndInclusive)
                        endingPseudoPeriodIndex++;

                    var indexes = Enumerable.Range(1, endingPseudoPeriodIndex);

                    IEnumerable<Element> extensionElements;
                    if (settings.UseParallelExtend)
                    {
                        extensionElements = indexes
                            .AsParallel().AsOrdered()
                            .Select(i => ComputeExtensionSequence(i, settings))
                            .SelectMany(seq => seq.Elements);
                    }
                    else
                    {
                        extensionElements = indexes
                            .Select(i => ComputeExtensionSequence(i, settings))
                            .SelectMany(seq => seq.Elements);
                    }

                    var elements =
                        BaseSequence.Elements.Concat(extensionElements);

                    return elements
                        .Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive)
                        .ToSequence();
                }
            }
        }
        else
        {
            return BaseSequence
                .Cut(cutStart, cutEnd, isStartInclusive, isEndInclusive);
        }
    }

    /// <summary>
    /// Generates an extended view on the curve, describing it in $[0, t_{end}[$.
    /// </summary>
    /// <param name="targetEnd">
    /// Exclusive end $t_{end}$ of the resulting <see cref="Sequence"/>.
    /// It is _not_ required to be greater than <see cref="FirstPseudoPeriodEnd"/>.  
    /// </param>
    /// <param name="settings"></param>
    /// <returns>A sequence equivalently defined from 0 to $t_{end}$.</returns>
    /// <remarks>
    /// This is a shorthand for <see cref="Cut"/>
    /// which follows, minus the restrictions, the definition in [BT07] Section 4.1
    /// </remarks>
    public Sequence Extend(Rational targetEnd, ComputationSettings? settings = null)
        => Cut(0, targetEnd, settings: settings);

    private Sequence ComputeExtensionSequence(int pseudoPeriodIndex, ComputationSettings? settings = null)
    {
        settings = settings ?? new ComputationSettings();

        var pseudoPeriodicElements = settings.UseParallelComputeExtensionSequences ?
            PseudoPeriodicElements.AsParallel().AsOrdered() :
            PseudoPeriodicElements;

        var periodicSegmentsCopy = pseudoPeriodicElements
            .Select(element => GetPseudoPeriodicCopy(element, pseudoPeriodIndex));

        var extensionSequence = new Sequence(periodicSegmentsCopy);
        return extensionSequence;

        //local function used to copy elements
        Element GetPseudoPeriodicCopy(
            Element element,
            int periodIndex)
        {
            switch (element)
            {
                case Point point:
                    return new Point(
                        time: point.Time + PseudoPeriodLength * periodIndex,
                        value: point.Value + PseudoPeriodHeight * periodIndex
                    );
                case Segment segment:
                    return new Segment(
                        startTime: segment.StartTime + PseudoPeriodLength * pseudoPeriodIndex,
                        endTime: segment.EndTime + PseudoPeriodLength * pseudoPeriodIndex,
                        rightLimitAtStartTime: segment.RightLimitAtStartTime + PseudoPeriodHeight * pseudoPeriodIndex,
                        slope: segment.Slope
                    );

                default:
                    throw new InvalidCastException();
            }
        }
    }

    private IEnumerable<Element> ComputeExtensionSequenceAsEnumerable(int pseudoPeriodIndex, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        bool doParallel = settings.UseParallelComputeExtensionSequences;
        IEnumerable<Element> periodicSegmentsCopy;
        if (doParallel)
        {
            periodicSegmentsCopy = PseudoPeriodicElements
                .AsParallel().AsOrdered()
                .Select(element => GetPseudoPeriodicCopy(element, pseudoPeriodIndex));
        }
        else
        {
            periodicSegmentsCopy = PseudoPeriodicElements
                .Select(element => GetPseudoPeriodicCopy(element, pseudoPeriodIndex));
        }
            
        return periodicSegmentsCopy;

        //local function used to copy elements
        Element GetPseudoPeriodicCopy(
            Element element,
            int periodIndex)
        {
            switch (element)
            {
                case Point point:
                    return new Point(
                        time: point.Time + PseudoPeriodLength * periodIndex,
                        value: point.Value + PseudoPeriodHeight * periodIndex
                    );
                case Segment segment:
                    return new Segment(
                        startTime: segment.StartTime + PseudoPeriodLength * pseudoPeriodIndex,
                        endTime: segment.EndTime + PseudoPeriodLength * pseudoPeriodIndex,
                        rightLimitAtStartTime: segment.RightLimitAtStartTime + PseudoPeriodHeight * pseudoPeriodIndex,
                        slope: segment.Slope
                    );

                default:
                    throw new InvalidCastException();
            }
        }
    }

    /// <summary>
    /// Returns the number of elements of a cut of the curve limited to the given domain.
    /// </summary>
    /// <param name="cutStart">Left extreme of the domain.</param>
    /// <param name="cutEnd">Right extreme of the domain.</param>
    /// <param name="isStartInclusive">If true, the domain is left-closed.</param>
    /// <param name="isEndInclusive">If true, the domain is right-closed.</param>
    /// <returns>The number of elements of the sequence equivalently defined within the given domain.</returns>
    public int Count(
        Rational cutStart,
        Rational cutEnd,
        bool isStartInclusive = true,
        bool isEndInclusive = false
    )
    {
        return CutAsEnumerable(cutStart, cutEnd, isStartInclusive, isEndInclusive).Count();
    }
        
    /// <summary>
    /// Computes the first time the curve is at or above given <paramref name="value"/>, i.e. $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\}$
    /// </summary>
    /// <param name="value">The value to reach.</param>
    /// <returns>The first time t at which $f(t)$ = value, or $+\infty$ if it is never reached.</returns>
    /// <remarks>
    /// The current implementation uses <see cref="ToNonDecreasing"/> and <see cref="LowerPseudoInverse"/>.
    /// Thus it is useful as a shortcut but not to optimize computation of $f^{-1}_\downarrow(x)$ for a single point.
    /// </remarks>
    public Rational TimeAt(Rational value)
    {
        return this
            .ToNonDecreasing()
            .LowerPseudoInverse()
            .ValueAt(value);
    }
    
    #endregion Methods

    #region Json Methods

    /// <summary>
    /// Returns string serialization in Json format.
    /// </summary>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new GenericCurveConverter());
    }

    /// <summary>
    /// Deserializes a Curve.
    /// </summary>
    public static Curve FromJson(string json)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(json, new GenericCurveConverter());
        if (curve == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return curve;
    }

    #endregion Json Methods
        
    #region Basic manipulations

    /// <summary>
    /// Scales the curve by a multiplicative factor, i.e. $g(t) = k \cdot f(t)$.
    /// </summary>
    public virtual Curve Scale(Rational scaling)
    {
        return new Curve(
            baseSequence: BaseSequence * scaling,
            pseudoPeriodStart: PseudoPeriodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodLength * PseudoPeriodAverageSlope * scaling
        );
    }

    /// <summary>
    /// Scales the curve by a multiplicative factor, i.e. $g(t) = k \cdot f(t)$.
    /// </summary>
    public static Curve operator *(Curve curve, Rational scaling)
    {
        return curve.Scale(scaling);
    }

    /// <summary>
    /// Scales the curve by a multiplicative factor, i.e. $g(t) = k \cdot f(t)$.
    /// </summary>
    public static Curve operator *(Rational scaling, Curve curve)
    {
        return curve.Scale(scaling);
    }

    /// <summary>
    /// Convolution between two curves, $f \otimes g$.
    /// </summary>
    public static Curve operator *(Curve c1, Curve c2)
    {
        return Convolution(c1, c2);
    }

    /// <summary>
    /// Delays the curve, adding a 0-valued padding at the start.
    /// </summary>
    public virtual Curve DelayBy(Rational delay)
    {
        if (delay < 0)
            throw new ArgumentException("Delay must be >= 0");

        if (delay == 0)
            return this;

        return new Curve(
            baseSequence: BaseSequence.Delay(delay),
            pseudoPeriodStart: PseudoPeriodStart + delay,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodHeight
        );
    }
        
    /// <summary>
    /// Anticipates the curve, removing the parts from 0 to the given time.
    /// </summary>
    public virtual Curve AnticipateBy(Rational time)
    {
        if (time < 0)
            throw new ArgumentException("Time must be >= 0");

        if (time == 0)
            return this;

        if (time <= PseudoPeriodStart)
        {
            return new Curve(
                baseSequence: BaseSequence.Anticipate(time),
                pseudoPeriodStart: PseudoPeriodStart - time,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
        else
        {
            return new Curve(
                baseSequence: Extend(FirstPseudoPeriodEnd + time).Anticipate(time),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
    }

    /// <summary>
    /// Shifts the curve vertically by an additive factor, i.e. $g(t) = k + f(t)$.
    /// </summary>
    public virtual Curve VerticalShift(Rational shift, bool exceptOrigin = true)
    {
        if (shift == 0)
            return this;

        if (shift == Rational.PlusInfinity)
            return exceptOrigin ? PlusInfinite().WithZeroOrigin() : PlusInfinite();

        if (exceptOrigin && IsIdenticallyZero)
            return new ConstantCurve(shift);
        
        return new Curve(
            baseSequence: BaseSequence.VerticalShift(shift, exceptOrigin),
            pseudoPeriodStart: PseudoPeriodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodHeight
        );
    }

    /// <summary>
    /// Shifts the curve vertically by an additive factor, i.e. $g(t) = k + f(t)$.
    /// </summary>
    public static Curve operator +(Curve curve, Rational shift)
    {
        return curve.VerticalShift(shift);
    }

    /// <summary>
    /// Shifts the curve vertically by an additive factor.
    /// </summary>
    public static Curve operator +(Rational shift, Curve curve)
    {
        return curve.VerticalShift(shift);
    }
    
    /// <summary>
    /// Computes a non-negative version of this curve, 
    /// i.e. a curve $g(t) = f(t)$ if $f(t) > 0$, $g(t) = 0$ otherwise.
    /// </summary>
    /// <remarks>
    /// Implements the _non-negative closure_ defined in [DNC18] p. 45 .
    /// </remarks>
    public Curve ToNonNegative()
        => Maximum(this, Curve.Zero());

    /// <summary>
    /// Computes a non-decreasing version of this curve,
    /// i.e. the lowest curve $g(t) \ge f(t)$ so that $g(t + s) \ge g(t)$ for any $t, s \ge 0$.
    /// </summary>
    /// <remarks>
    /// This implements the _non-decreasing closure_ defined in [DNC18] p. 45, although the implementation differs.
    /// </remarks>
    public Curve ToNonDecreasing()
    {
        if (IsNonDecreasing)
            return this;

        // the following implementation is more efficient than the definition in [DNC18] p. 45, which uses the max-plus convolution,
        // since here we add terms to the global maximum only if there actually is a decrease.
        
        // this list will contain the curve to transform plus,
        // for each breakpoint at which a decrease happens, a constant segment with the max value at the breakpoint and $-\infty$ before it.
        List<Curve> curves = new (){ this };

        if (HasTransient)
        {
            foreach (var (left, center, right) in TransientSequence!.EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).GetBreakpointValues().Max();
                    curves.Add(GetLowerboundCurve(time, value));
                }
            }
        }
        
        if(PseudoPeriodAverageSlope > 0)
        {
            foreach (var (left, center, right) in 
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndInclusive: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).GetBreakpointValues().Max();
                    curves.Add(GetPeriodicLowerboundCurve(time, value));
                }
            }   
        }
        else
        {
            foreach (var (left, center, right) in 
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndInclusive: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).GetBreakpointValues().Max();
                    curves.Add(GetLowerboundCurve(time, value));
                }
            }
        }
        
        return Maximum(curves);

        Curve GetLowerboundCurve(Rational time, Rational value)
        {
            List<Element> elements;
            if (time > 0)
            {
                elements = new()
                {
                    Point.MinusInfinite(0),
                    Segment.MinusInfinite(0, time),
                    new Point(time, value),
                    new Segment(time, time + 1, value, 0)
                };
            }
            else
            {
                elements = new()
                {
                    new Point(time, value),
                    new Segment(time, time + 1, value, 0)
                };
            }
                
            return new Curve(
                baseSequence: new Sequence(elements),
                pseudoPeriodStart: time,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        
        Curve GetPeriodicLowerboundCurve(Rational time, Rational value)
        {
            List<Element> elements;
            if (time > 0)
            {
                elements = new()
                {
                    Point.MinusInfinite(0),
                    Segment.MinusInfinite(0, time),
                    new Point(time, value),
                    new Segment(time, time + PseudoPeriodLength, value, 0)
                };
            }
            else
            {
                elements = new()
                {
                    new Point(time, value),
                    new Segment(time, time + PseudoPeriodLength, value, 0)
                };
            }
                
            return new Curve(
                baseSequence: new Sequence(elements),
                pseudoPeriodStart: time,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
    }

    /// <summary>
    /// Enforces $f(0) = 0$, i.e. it returns $f^\circ = \min \left( f, \delta_0 \right)$.
    /// </summary>
    public Curve WithZeroOrigin()
    {
        if (this.ValueAt(0) == 0)
        {
            return this;
        }
        else
        {
            return Minimum(this, new DelayServiceCurve(0));
        }
    }
    
    /// <summary>
    /// Computes the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS22]. 
    /// </remarks>
    public Curve LowerPseudoInverse()
    {
        if (!IsNonDecreasing)
            throw new ArgumentException("The pseudo-inverse is defined only for non-decreasing functions");

        if (IsUltimatelyConstant)
        {
            var curve = this.TransientReduction();  // ensure the constant part starts at T
            var constant_value = curve.ValueAt(curve.PseudoPeriodStart);
            var constant_start = curve.PseudoPeriodStart;
            var transient_lpi = curve.TransientElements.LowerPseudoInverse();
            var lpi = transient_lpi
                .Append(new Point(constant_value, constant_start))
                .Append(Segment.PlusInfinite(constant_value, constant_value + 2));
            
            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: constant_value + 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (IsWeaklyUltimatelyInfinite)
        {
            var lastFiniteTime = BaseSequence.FirstInfiniteTime;
            var lastFiniteValue = BaseSequence.LeftLimitAt(lastFiniteTime);
            var transient_lpi = BaseSequence.CutAsEnumerable(0, lastFiniteTime).LowerPseudoInverse();
            var lpi = transient_lpi
                .Append(new Point(lastFiniteValue, lastFiniteTime))
                .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: lastFiniteValue,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (!IsNonNegative)
        {
            var T = Rational.Max(FirstPseudoPeriodEnd, FirstNonNegativeTime);
            if (ValueAt(T) < 0)
                T += PseudoPeriodLength;
            var sequence = CutAsEnumerable(0, T + PseudoPeriodLength, isEndInclusive: true)
                .LowerPseudoInverse()
                .SkipLast(1)
                .ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: ValueAt(T),
                pseudoPeriodLength: PseudoPeriodHeight,
                pseudoPeriodHeight: PseudoPeriodLength
            ).TransientReduction();
        }
        else
        {
            // the point at the right extreme is included in case there is a left-discontinuity at the end of the pseudo-period
            // the pseudo-inverse of said point is then removed from the result
            var sequence = CutAsEnumerable(0, SecondPseudoPeriodEnd, isEndInclusive: true)
                .LowerPseudoInverse()
                .SkipLast(1)
                .ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: ValueAt(FirstPseudoPeriodEnd),
                pseudoPeriodLength: PseudoPeriodHeight,
                pseudoPeriodHeight: PseudoPeriodLength
            ).TransientReduction();
        }
    }

    /// <summary>
    /// Computes the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is right-continuous, thus is revertible, i.e. $\left(f^{-1}_\uparrow\right)^{-1}_\uparrow = f$, only if $f$ is right-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS22]. 
    /// </remarks>
    public Curve UpperPseudoInverse()
    {
        if (!IsNonDecreasing)
            throw new ArgumentException("The pseudo-inverse is defined only for non-decreasing functions");
        
        if (IsUltimatelyConstant)
        {
            var curve = this.TransientReduction();  // ensure the constant part starts at T
            var constant_value = curve.ValueAt(curve.PseudoPeriodStart);
            var constant_start = curve.PseudoPeriodStart;
            var transient_lpi = curve.TransientElements.UpperPseudoInverse();
            var lpi = transient_lpi
                .Append(Point.PlusInfinite(constant_value))
                .Append(Segment.PlusInfinite(constant_value, constant_value + 1));
            
            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: constant_value,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (IsWeaklyUltimatelyInfinite)
        {
            var lastFiniteTime = BaseSequence.FirstInfiniteTime;
            var lastFiniteValue = BaseSequence.LeftLimitAt(lastFiniteTime);
            var transient_lpi = BaseSequence.CutAsEnumerable(0, lastFiniteTime).UpperPseudoInverse();
            var lpi = transient_lpi
                .Append(new Point(lastFiniteValue, lastFiniteTime))
                .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: lastFiniteValue,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (!IsNonNegative)
        {
            var T = Rational.Max(FirstPseudoPeriodEnd, FirstNonNegativeTime);
            if (ValueAt(T) < 0)
                T += PseudoPeriodLength;
            var sequence = CutAsEnumerable(0, T + PseudoPeriodLength, isEndInclusive: true)
                .UpperPseudoInverse()
                .SkipLast(1)
                .ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: ValueAt(T),
                pseudoPeriodLength: PseudoPeriodHeight,
                pseudoPeriodHeight: PseudoPeriodLength
            ).TransientReduction();
        }
        else
        {
            // the point at the right extreme is included in case there is a left-discontinuity at the end of the pseudo-period
            // the pseudo-inverse of said point is then removed from the result
            var sequence = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndInclusive: true)
                .UpperPseudoInverse()
                .SkipLast(1)
                .ToSequence();
            
            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: ValueAt(PseudoPeriodStart),
                pseudoPeriodLength: PseudoPeriodHeight,
                pseudoPeriodHeight: PseudoPeriodLength
            );
        }
    }

    /// <summary>
    /// Computes the horizontal deviation between the two curves, $h(a, b)$.
    /// If <paramref name="a"/> is an arrival curve and <paramref name="b"/> a service curve, the result will be the worst-case delay.
    /// </summary>
    /// <param name="a">Must be non-decreasing.</param>
    /// <param name="b">Must be non-decreasing.</param>
    /// <returns>A non-negative horizontal deviation.</returns>
    public static Rational HorizontalDeviation(Curve a, Curve b)
    {
        if (!a.IsNonDecreasing || !b.IsNonDecreasing)
            throw new ArgumentException("The arguments must be non-decreasing.");
        
        if (a is SigmaRhoArrivalCurve sr && b is RateLatencyServiceCurve rl)
        {
            if(rl.Rate >= sr.Rho)
                return rl.Latency + sr.Sigma / rl.Rate;
            else
                return Rational.PlusInfinity;
        }
        else
        {
            // the following are mathematically equivalent methods
            #if false
            var a_upi = a.UpperPseudoInverse();
            var b_upi = b.UpperPseudoInverse();
            var hDev = -MaxPlusDeconvolution(a_lpi, b_lpi).ValueAt(0);
            #endif
            #if false
            var hDev = b.LowerPseudoInverse()
                .Composition(a)
                .Deconvolution(new RateLatencyServiceCurve(1, 0))
                .ValueAt(0);
            #endif
            #if true
            var hDev = b.LowerPseudoInverse()
                .Composition(a)
                .Subtraction(new RateLatencyServiceCurve(1, 0))
                .MaxValue();
            #endif
            return hDev;
        }
    }

    /// <summary>
    /// Computes the vertical deviation between the two curves, $v(a, b)$.
    /// If <paramref name="a"/> is an arrival curve and <paramref name="b"/> a service curve, the result will be the worst-case backlog.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>A non-negative vertical deviation.</returns>
    public static Rational VerticalDeviation(Curve a, Curve b)
    {
        if (a is SigmaRhoArrivalCurve sr && b is RateLatencyServiceCurve dr)
        {
            if(dr.Rate >= sr.Rho)
                return sr.Sigma + dr.Latency * sr.Rho;
            else
                return Rational.PlusInfinity;
        }
        else
        {
            var diff = a - b;
            return diff.MaxValue();
        }
    }

    /// <summary>
    /// If the curve is upper-bounded, i.e. $f(t) \le x$ for any $t$, returns $x$
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public Rational MaxValue()
    {
        if (PseudoPeriodAverageSlope <= 0)
        {
            var breakpoints = Cut(0, FirstPseudoPeriodEnd, isEndInclusive: true).EnumerateBreakpoints();
            return breakpoints.GetBreakpointsValues().Max();
        }
        else
        {
            return Rational.PlusInfinity;
        }
    }

    /// <summary>
    /// If the curve is lower-bounded, i.e. $f(t) \ge x$ for any $t$, returns $x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public Rational MinValue()
    {
        if (PseudoPeriodAverageSlope >= 0)
        {
            var breakpoints = Cut(0, FirstPseudoPeriodEnd, isEndInclusive: true).EnumerateBreakpoints();
            return breakpoints.GetBreakpointsValues().Min();
        }
        else
        {
            return Rational.MinusInfinity;
        }
    }

    #endregion

    #region Optimization

    /// <summary>
    /// Optimizes Curve representation by anticipating periodic start and reducing period length.
    /// </summary>
    /// <returns>An equivalent minimal representation for the same curve.</returns>
    /// <remarks>This method implements representation minimization, as discussed in [ZS22].</remarks>
    public Curve Optimize()
    {
        //Attempts all optimizations methods in sequence
        var optimizedCurve = this
            .PeriodFactorization()
            .AffineNormalization()
            .TransientReduction();

        #if DO_LOG    
        if (optimizedCurve != this)
        {
            logger.Debug($"Optimization: " +
                         $"T {PseudoPeriodStart} -> {optimizedCurve.PseudoPeriodStart}, " +
                         $"d {PseudoPeriodLength} -> {optimizedCurve.PseudoPeriodLength}, " +
                         $"elements {BaseSequence.Count} -> {optimizedCurve.BaseSequence.Count}");
        }
        #endif

        return optimizedCurve;
    }
        
    //Tests if pseudo period is composed of repetitions of a simpler pattern.
    //If so, excess parts are removed.
    internal Curve PeriodFactorization()
    {
        if (PseudoPeriodicElements.Count() <= 2)
            return this;
        if (PseudoPeriodHeight.IsInfinite)
            return this;
            
        bool optimized = false;
            
        var periodSequence = PseudoPeriodicSequence;
        var periodLength = PseudoPeriodLength;
        var periodHeight = PseudoPeriodHeight;

        bool anotherRound = true;

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif
        while (anotherRound)
        {
            anotherRound = FactorizationRound();
        }
        #if DO_LOG
        timer.Stop();
        #endif

        if (optimized)
        {
            #if DO_LOG
            logger.Trace($"Optimization: PeriodFactorization, took {timer.Elapsed} From at {PseudoPeriodicSequence.Count} to {periodSequence.Count}");
            #endif

            var elements = new List<Element>();
            elements.AddRange(TransientElements);
            elements.AddRange(periodSequence.Elements);
                
            return new Curve(
                baseSequence: new Sequence(elements),
                pseudoPeriodStart: PseudoPeriodStart,
                pseudoPeriodLength: periodLength,
                pseudoPeriodHeight: periodHeight
            );
        }
        else
        {
            #if DO_LOG
            logger.Trace($"Optimization failed: PeriodFactorization, took {timer.Elapsed} Left at {periodSequence.Count}");
            #endif
            return this;
        }

        bool FactorizationRound()
        {
            // any point in ]T, T+d[ is surely a breakpoint
            int breakPoints = periodSequence.Elements
                .Count(e => e is Point) - 1;
                
            // find out if between period end and next start there is another breakpoint
            var startingSlope = (periodSequence.Elements.First(e => e is Segment) as Segment)!.Slope;
            var endingSlope = (periodSequence.Elements.Last(e => e is Segment) as Segment)!.Slope;
            var startingValue = (periodSequence.Elements.First(e => e is Point) as Point)!.Value;
            var startingRightLimit = (periodSequence.Elements.First(e => e is Segment) as Segment)!.RightLimitAtStartTime;
            var endingLeftLimit = (periodSequence.Elements.Last(e => e is Segment) as Segment)!.LeftLimitAtEndTime;
            if (startingValue.IsInfinite || endingLeftLimit.IsInfinite)
            {
                var firstSegment = periodSequence.Elements.First(e => e is Segment) as Segment;
                var mergeableInfinite =
                    endingLeftLimit.IsInfinite && startingValue.IsInfinite && firstSegment!.IsInfinite;
                    
                if(!mergeableInfinite)
                    breakPoints += 1;
            }
            else
            {
                var verticalDiff = endingLeftLimit - startingValue;
                if (
                    startingValue != startingRightLimit || // is there a right-discontinuity? 
                    verticalDiff != periodHeight || // is there a left-discontinuity?
                    startingSlope != endingSlope // is there a slope change?
                )
                    breakPoints += 1;
            }
                
            foreach (int primeNumber in PrimeDivisors(breakPoints))
            {
                if (IsPrimeFactor(primeNumber))
                {
                    periodLength /= primeNumber;
                    periodHeight /= primeNumber;
                    periodSequence = periodSequence
                        .Cut(PseudoPeriodStart,PseudoPeriodStart + periodLength);

                    optimized = true;
                    return true;
                }
            }

            return false;
        }
            
        IEnumerable<int> PrimeNumbers(int max)
        {
            // Initialized to the first 1000 primes
            List<int> hardcodedPrimes = new()
            {
                2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013, 1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069, 1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151, 1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223, 1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291, 1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373, 1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451, 1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511, 1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583, 1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657, 1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733, 1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811, 1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889, 1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987, 1993, 1997, 1999, 2003, 2011, 2017, 2027, 2029, 2039, 2053, 2063, 2069, 2081, 2083, 2087, 2089, 2099, 2111, 2113, 2129, 2131, 2137, 2141, 2143, 2153, 2161, 2179, 2203, 2207, 2213, 2221, 2237, 2239, 2243, 2251, 2267, 2269, 2273, 2281, 2287, 2293, 2297, 2309, 2311, 2333, 2339, 2341, 2347, 2351, 2357, 2371, 2377, 2381, 2383, 2389, 2393, 2399, 2411, 2417, 2423, 2437, 2441, 2447, 2459, 2467, 2473, 2477, 2503, 2521, 2531, 2539, 2543, 2549, 2551, 2557, 2579, 2591, 2593, 2609, 2617, 2621, 2633, 2647, 2657, 2659, 2663, 2671, 2677, 2683, 2687, 2689, 2693, 2699, 2707, 2711, 2713, 2719, 2729, 2731, 2741, 2749, 2753, 2767, 2777, 2789, 2791, 2797, 2801, 2803, 2819, 2833, 2837, 2843, 2851, 2857, 2861, 2879, 2887, 2897, 2903, 2909, 2917, 2927, 2939, 2953, 2957, 2963, 2969, 2971, 2999, 3001, 3011, 3019, 3023, 3037, 3041, 3049, 3061, 3067, 3079, 3083, 3089, 3109, 3119, 3121, 3137, 3163, 3167, 3169, 3181, 3187, 3191, 3203, 3209, 3217, 3221, 3229, 3251, 3253, 3257, 3259, 3271, 3299, 3301, 3307, 3313, 3319, 3323, 3329, 3331, 3343, 3347, 3359, 3361, 3371, 3373, 3389, 3391, 3407, 3413, 3433, 3449, 3457, 3461, 3463, 3467, 3469, 3491, 3499, 3511, 3517, 3527, 3529, 3533, 3539, 3541, 3547, 3557, 3559, 3571, 3581, 3583, 3593, 3607, 3613, 3617, 3623, 3631, 3637, 3643, 3659, 3671, 3673, 3677, 3691, 3697, 3701, 3709, 3719, 3727, 3733, 3739, 3761, 3767, 3769, 3779, 3793, 3797, 3803, 3821, 3823, 3833, 3847, 3851, 3853, 3863, 3877, 3881, 3889, 3907, 3911, 3917, 3919, 3923, 3929, 3931, 3943, 3947, 3967, 3989, 4001, 4003, 4007, 4013, 4019, 4021, 4027, 4049, 4051, 4057, 4073, 4079, 4091, 4093, 4099, 4111, 4127, 4129, 4133, 4139, 4153, 4157, 4159, 4177, 4201, 4211, 4217, 4219, 4229, 4231, 4241, 4243, 4253, 4259, 4261, 4271, 4273, 4283, 4289, 4297, 4327, 4337, 4339, 4349, 4357, 4363, 4373, 4391, 4397, 4409, 4421, 4423, 4441, 4447, 4451, 4457, 4463, 4481, 4483, 4493, 4507, 4513, 4517, 4519, 4523, 4547, 4549, 4561, 4567, 4583, 4591, 4597, 4603, 4621, 4637, 4639, 4643, 4649, 4651, 4657, 4663, 4673, 4679, 4691, 4703, 4721, 4723, 4729, 4733, 4751, 4759, 4783, 4787, 4789, 4793, 4799, 4801, 4813, 4817, 4831, 4861, 4871, 4877, 4889, 4903, 4909, 4919, 4931, 4933, 4937, 4943, 4951, 4957, 4967, 4969, 4973, 4987, 4993, 4999, 5003, 5009, 5011, 5021, 5023, 5039, 5051, 5059, 5077, 5081, 5087, 5099, 5101, 5107, 5113, 5119, 5147, 5153, 5167, 5171, 5179, 5189, 5197, 5209, 5227, 5231, 5233, 5237, 5261, 5273, 5279, 5281, 5297, 5303, 5309, 5323, 5333, 5347, 5351, 5381, 5387, 5393, 5399, 5407, 5413, 5417, 5419, 5431, 5437, 5441, 5443, 5449, 5471, 5477, 5479, 5483, 5501, 5503, 5507, 5519, 5521, 5527, 5531, 5557, 5563, 5569, 5573, 5581, 5591, 5623, 5639, 5641, 5647, 5651, 5653, 5657, 5659, 5669, 5683, 5689, 5693, 5701, 5711, 5717, 5737, 5741, 5743, 5749, 5779, 5783, 5791, 5801, 5807, 5813, 5821, 5827, 5839, 5843, 5849, 5851, 5857, 5861, 5867, 5869, 5879, 5881, 5897, 5903, 5923, 5927, 5939, 5953, 5981, 5987, 6007, 6011, 6029, 6037, 6043, 6047, 6053, 6067, 6073, 6079, 6089, 6091, 6101, 6113, 6121, 6131, 6133, 6143, 6151, 6163, 6173, 6197, 6199, 6203, 6211, 6217, 6221, 6229, 6247, 6257, 6263, 6269, 6271, 6277, 6287, 6299, 6301, 6311, 6317, 6323, 6329, 6337, 6343, 6353, 6359, 6361, 6367, 6373, 6379, 6389, 6397, 6421, 6427, 6449, 6451, 6469, 6473, 6481, 6491, 6521, 6529, 6547, 6551, 6553, 6563, 6569, 6571, 6577, 6581, 6599, 6607, 6619, 6637, 6653, 6659, 6661, 6673, 6679, 6689, 6691, 6701, 6703, 6709, 6719, 6733, 6737, 6761, 6763, 6779, 6781, 6791, 6793, 6803, 6823, 6827, 6829, 6833, 6841, 6857, 6863, 6869, 6871, 6883, 6899, 6907, 6911, 6917, 6947, 6949, 6959, 6961, 6967, 6971, 6977, 6983, 6991, 6997, 7001, 7013, 7019, 7027, 7039, 7043, 7057, 7069, 7079, 7103, 7109, 7121, 7127, 7129, 7151, 7159, 7177, 7187, 7193, 7207, 7211, 7213, 7219, 7229, 7237, 7243, 7247, 7253, 7283, 7297, 7307, 7309, 7321, 7331, 7333, 7349, 7351, 7369, 7393, 7411, 7417, 7433, 7451, 7457, 7459, 7477, 7481, 7487, 7489, 7499, 7507, 7517, 7523, 7529, 7537, 7541, 7547, 7549, 7559, 7561, 7573, 7577, 7583, 7589, 7591, 7603, 7607, 7621, 7639, 7643, 7649, 7669, 7673, 7681, 7687, 7691, 7699, 7703, 7717, 7723, 7727, 7741, 7753, 7757, 7759, 7789, 7793, 7817, 7823, 7829, 7841, 7853, 7867, 7873, 7877, 7879, 7883, 7901, 7907, 7919
            };

            if(max <= 7919)
                foreach (var prime in hardcodedPrimes.TakeWhile(p => p <= max))
                    yield return prime;
            else
            {
                foreach (var prime in hardcodedPrimes)
                    yield return prime;

                var primes = new List<int>(hardcodedPrimes);
                for (int candidate = 7927; candidate <= max; candidate += 2)
                {
                    if (primes.All(p => candidate % p > 0))
                    {
                        yield return candidate;
                        primes.Add(candidate);
                    }
                }
            }
        }

        // find all distinct prime divisors of n
        IEnumerable<int> PrimeDivisors(int n)
        {
            foreach (int p in PrimeNumbers((int) Math.Ceiling(Math.Sqrt(n))))
            {
                if (n % p == 0)
                {
                    while (n % p == 0)
                        n = n / p;
                        
                    yield return p;
                }
            }

            if (n != 1)
                yield return n;
        }
            
        bool IsPrimeFactor(int primeNumber)
        {
            var length = periodLength / primeNumber;
            var step = periodHeight / primeNumber;

            bool isPrimeFactor = Enumerable.Range(0, primeNumber - 1)
                //.AsParallel()
                .All(i => CompareSubsequences(i));

            return isPrimeFactor;

            bool CompareSubsequences(int i)
            {
                var leftCut = periodSequence
                    .Cut(PseudoPeriodStart + i * length, PseudoPeriodStart + (i + 1) * length)
                    .Delay(length, prependWithZero: false)
                    .VerticalShift(step);
                var rightCut = periodSequence
                    .Cut(PseudoPeriodStart + (i + 1) * length, PseudoPeriodStart + (i + 2) * length);

                return leftCut == rightCut;
            }
        }
    }

    /// <summary>
    /// If the curve is Ultimately Affine, the period is normalized to $d = 1$
    /// </summary>
    internal Curve AffineNormalization()
    {
        if (this.IsUltimatelyAffine && PseudoPeriodLength != 1)
        {
            return new Curve(
                baseSequence: Cut(0, PseudoPeriodStart + 1),
                pseudoPeriodStart: PseudoPeriodStart,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: PseudoPeriodAverageSlope
            );
        }
        else
        {
            return this;
        }
    }

    /// <summary>
    /// Tests if the transient contains period behavior, if so it brings forward period start.
    /// </summary>
    internal Curve TransientReduction()
    {
        if (this.IsUltimatelyAffine)
        {
            // simplified algorithm for affine tails
            if (BaseSequence.Count < 4)
                return this;
            
            var affineSegment = (Segment) BaseSequence.Elements[^1];
            var affinePoint = (Point) BaseSequence.Elements[^2];
            var candidateSegment = (Segment) BaseSequence.Elements[^3];
            var candidatePoint = (Point) BaseSequence.Elements[^4];

            if (
                SequenceExtensions.CanMergeTriplet(candidateSegment, affinePoint, affineSegment) &&
                candidatePoint.Value == candidateSegment.RightLimitAtStartTime
            )
            {
                var t = candidatePoint.Time;
                var d = t > 0 ? t / 10 : 1;
                var c = affineSegment.Slope * d;
                return new Curve(
                    baseSequence: Cut(0, t + d),
                    pseudoPeriodStart: t,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c
                );
            }
            else
                return this;
        }
        else
        {
            bool optimized = false;
            
            var sequence = BaseSequence;
            var pseudoPeriodicSequence = PseudoPeriodicSequence;
            var periodStart = PseudoPeriodStart;
            var periodLength = PseudoPeriodLength;

            #if DO_LOG
            var timer_1 = Stopwatch.StartNew();
            #endif
            ByPeriod();
            #if DO_LOG
            timer_1.Stop();
            var timer_2 = Stopwatch.StartNew();
            #endif
            BySegment();
            #if DO_LOG
            timer_2.Stop();
            #endif

            if (optimized)
            {
                #if DO_LOG
                logger.Trace($"Optimization: TransientReduction, took {timer_1.Elapsed} by period, {timer_2.Elapsed} by segment");
                #endif

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: periodStart,
                    pseudoPeriodLength: periodLength,
                    pseudoPeriodHeight: periodLength * PseudoPeriodAverageSlope
                );
            }
            else
            {
                #if DO_LOG
                logger.Trace($"Optimization failed: TransientReduction, took {timer_1.Elapsed} by period, {timer_2.Elapsed} by segment");
                #endif
                return this;
            }

            void ByPeriod()
            {
                bool tryShorten = periodStart - periodLength > 0;
                // int iteration = 0;
                while (tryShorten)
                {
                    // iteration++;
                    // logger.Trace($"TransientReduction by period, iteration #{iteration}");

                    var candidatePeriod = sequence.Cut(periodStart - periodLength, periodStart).Optimize();
                    if (candidatePeriod.VerticalShift(PseudoPeriodHeight).Delay(periodLength, prependWithZero: false) ==
                        pseudoPeriodicSequence)
                    {
                        periodStart -= periodLength;
                        pseudoPeriodicSequence = candidatePeriod;

                        optimized = true;
                        tryShorten = periodStart - periodLength > 0;
                    }
                    else
                    {
                        tryShorten = false;
                    }
                }

                sequence = sequence.Cut(0, periodStart + periodLength);
            }

            void BySegment()
            {
                var tailSequence = getPeriodTail();
                bool tryShorten = periodStart - tailSequence.Length > 0;

                while (tryShorten)
                {
                    var candidateSequence = sequence.Cut(periodStart - tailSequence.Length, periodStart).Optimize();
                    var shiftedCandidateSequence = candidateSequence.VerticalShift(PseudoPeriodHeight)
                        .Delay(periodLength, prependWithZero: false);
                    if (shiftedCandidateSequence == tailSequence)
                    {
                        periodStart -= tailSequence.Length;
                        sequence = sequence.Cut(0, periodStart + periodLength);

                        optimized = true;

                        tailSequence = getPeriodTail();
                        tryShorten = periodStart - tailSequence.Length > 0;
                    }
                    else
                    {
                        tryShorten = false;
                    }
                }

                Sequence getPeriodTail()
                {
                    var tailElements = sequence.Elements.Reverse().Take(2).Reverse();
                    return new Sequence(tailElements);
                }
            }
        }
    }
        
    #endregion Optimization

    #region Addition and Subtraction operators

    /// <summary>
    /// Implements (min, +)-algebra addition operation.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Defined in [BT07] Section 4.2 </remarks>
    public virtual Curve Addition(Curve b)
    {
        Rational T = Rational.Max(PseudoPeriodStart, b.PseudoPeriodStart);
        Rational d = Rational.LeastCommonMultiple(PseudoPeriodLength, b.PseudoPeriodLength);
        Rational c = d * (PseudoPeriodAverageSlope + b.PseudoPeriodAverageSlope);

        Sequence extendedSequence1 = Extend(T + d);
        Sequence extendedSequence2 = b.Extend(T + d);

        Sequence baseSequence = extendedSequence1 + extendedSequence2;
        return new Curve(
            baseSequence: baseSequence,
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );
    }

    /// <summary>
    /// Implements (min, +)-algebra addition operation.
    /// </summary>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Defined in [BT07] Section 4.2 </remarks>
    public static Curve Addition(Curve a, Curve b)
        => a.Addition(b);

    /// <summary>
    /// Implements (min, +)-algebra addition operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall addition.</returns>
    public static Curve Addition(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        bool doParallel = settings.UseParallelListAddition;
        Curve result;
        if (doParallel)
        {
            result = curves
                .AsParallel()
                .Aggregate(Addition);
        }
        else
        {
            result = curves
                .Aggregate(Addition);
        }

        return result;
    }

    /// <summary>
    /// Implements (min, +)-algebra addition operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall addition.</returns>
    public static Curve Addition(IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //todo: move these to ComputationSettings
        const int Parallelization_CountThreshold = 8;

        const int Parallelization_ComplexityMinimumCount = 4;
        const int Parallelization_ComplexityThreshold = 5_000;

        if (!curves.Any())
            throw new ArgumentException("The enumerable is empty");
        if (curves.Count == 1)
            return curves.Single();
        if (curves.Count == 2)
            return Addition(curves.ElementAt(0), curves.ElementAt(1));

        bool parallelizeDueCount =
            settings.UseParallelListAddition &&
            curves.Count >= Parallelization_CountThreshold;
        bool parallelizeDueComplexity =
            settings.UseParallelListAddition &&
            curves.Count >= Parallelization_ComplexityMinimumCount &&
            curves.Average(c => c.BaseSequence.Count) >= Parallelization_ComplexityThreshold;

        if (parallelizeDueCount || parallelizeDueComplexity)
        {
            return curves
                .AsParallel()
                .Aggregate(Addition);
        }
        else
        {
            return curves
                .Aggregate(Addition);
        }
    }

    /// <summary>
    /// Implements (min, +)-algebra addition operation.
    /// </summary>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Defined in [BT07] Section 4.2 </remarks>
    public static Curve operator +(Curve a, Curve b)
        => a.Addition(b);
        
    /// <summary>
    /// Implements subtraction operation between two curves.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <returns>The curve resulting from the subtraction.</returns>
    public virtual Curve Subtraction(Curve b, bool nonNegative = true)
        => nonNegative ? 
            Addition(-b).ToNonNegative() : 
            Addition(-b);

    /// <summary>
    /// Implements subtraction operation between two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <returns>The curve resulting from the subtraction.</returns>
    public static Curve Subtraction(Curve a, Curve b, bool nonNegative = true)
        => a.Subtraction(b, nonNegative);

    /// <summary>
    /// Implements subtraction operation between two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>The non-negative curve resulting from the subtraction.</returns>
    /// <remarks>
    /// The result is forced to be non-negative.
    /// Use <see cref="Subtraction(Unipi.Nancy.MinPlusAlgebra.Curve, Unipi.Nancy.MinPlusAlgebra.Curve,bool)"/> to have results with negative values.
    /// </remarks>
    public static Curve operator -(Curve a, Curve b)
        => a.Subtraction(b);

    #endregion Addition and Subtraction operators

    #region Minimum and maximum operators

    /// <summary>
    /// Implements (min, +)-algebra minimum operation over two curves.
    /// </summary>
    /// <param name="curve">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the minimum.</returns>
    /// <remarks>
    /// Defined in [BT07] Section 4.3
    /// </remarks>
    public virtual Curve Minimum(Curve curve, ComputationSettings? settings = null)
    {
        // Renaming for simmetry
        var a = this;
        var b = curve;

        settings ??= ComputationSettings.Default();
        
        #if DO_LOG
        logger.Trace($"Computing minimum of f1 ({a.BaseSequence.Count} elements, T: {a.PseudoPeriodStart} d: {a.PseudoPeriodLength})" +
                     $" and f2 ({b.BaseSequence.Count} elements, T: {b.PseudoPeriodStart} d: {b.PseudoPeriodLength})");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        Rational c, d, T; //Values for the resultFunction

        if (a.PseudoPeriodAverageSlope == b.PseudoPeriodAverageSlope)
        {
            d = EarliestValidLength();
            c = d * a.PseudoPeriodAverageSlope;
            T = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart);

            Rational EarliestValidLength()
            {
                //Optimization: avoid enlargement of lengths if a curve is Ultimately Affine
                if (a.IsUltimatelyAffine)
                    return b.PseudoPeriodLength;
                if (b.IsUltimatelyAffine)
                    return a.PseudoPeriodLength;

                //Optimization: avoid enlargement of lengths if a curve is trivially below the other
                var maxBaseSequenceEnd = Rational.Max(a.FirstPseudoPeriodEnd, b.FirstPseudoPeriodEnd);
                var aBaseCut = a.Cut(0, maxBaseSequenceEnd, settings: settings);
                var bBaseCut = b.Cut(0, maxBaseSequenceEnd, settings: settings);
                if (Sequence.LessOrEqual(aBaseCut, bBaseCut, settings) && a.PseudoPeriodAverageSlope <= b.PseudoPeriodAverageSlope)
                    return a.PseudoPeriodLength;
                if (Sequence.LessOrEqual(bBaseCut, aBaseCut, settings) && b.PseudoPeriodAverageSlope <= a.PseudoPeriodAverageSlope)
                    return b.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            }
        }
        else
        {
            Curve ultimatelyLower, ultimatelyHigher;

            if (a.PseudoPeriodAverageSlope < b.PseudoPeriodAverageSlope)
            {
                ultimatelyLower = a;
                ultimatelyHigher = b;
            }
            else
            {
                ultimatelyLower = b;
                ultimatelyHigher = a;
            }

            d = ultimatelyLower.PseudoPeriodLength;
            c = ultimatelyLower.PseudoPeriodHeight;

            if (ultimatelyHigher.PseudoPeriodAverageSlope.IsFinite && ultimatelyLower.PseudoPeriodAverageSlope.IsFinite)
            {
                Rational boundsIntersection = BoundsIntersection(ultimatelyLower: ultimatelyLower, ultimatelyHigher: ultimatelyHigher);
                T = Rational.Max(boundsIntersection, a.PseudoPeriodStart, b.PseudoPeriodStart);
#if TRACE_MIN_MAX_EXTENSIONS
                #if DO_LOG
                logger.Trace($"Minimum, different slopes: {a.PseudoPeriodAverageSlope} ~ {(decimal)a.PseudoPeriodAverageSlope}, {b.PseudoPeriodAverageSlope} ~ {(decimal)b.PseudoPeriodAverageSlope}");
                #endif
                logger.Trace($"Minimum, different slopes: must extend from \n" +
                             $"{a.PseudoPeriodStart} ~ {(decimal) a.PseudoPeriodStart} \n" +
                             $"{b.PseudoPeriodStart} ~ {(decimal) b.PseudoPeriodStart} \n" +
                             $"to {T} ~ {(decimal) T}");
#endif
            }
            else
            {
                T = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart);
            }
        }

        #if DO_LOG
        logger.Debug($"Minimum: extending from T1 {a.PseudoPeriodStart} d1 {a.PseudoPeriodLength}  T2 {b.PseudoPeriodStart} d2 {b.PseudoPeriodLength} to T {T} d {d}");
        #endif

        Sequence extendedSegmentsF1 = a.Extend(T + d, settings);
        Sequence extendedSegmentsF2 = b.Extend(T + d, settings);

        #if DO_LOG
        logger.Debug($"Minimum: extending from {a.BaseSequence.Count} and {b.BaseSequence.Count} to {extendedSegmentsF1.Count} and {extendedSegmentsF2.Count}");
        #endif

        Sequence minSequence = extendedSegmentsF1.Minimum(extendedSegmentsF2, settings: settings);

        Curve result = new Curve
        (
            baseSequence: minSequence,
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );

        var retVal = settings.AutoOptimize ? result.Optimize() : result;

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Minimum: took {timer.Elapsed}; a {a.BaseSequence.Count} b {b.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {retVal}");
        #endif
        return retVal;
    }

    //Bounds intersection is proved in [BT07] proposition 4, proof 1
    private static Rational BoundsIntersection(Curve ultimatelyLower, Curve ultimatelyHigher)
    {
        //Bounds are computed relative to origin-passing lines with slope
        //equal to the functions' periodic slopes.

        //fl(t) <= M + fl.PseudoPeriodAverageSlope * t; t >= fl.PseudoPeriodStartTime
        Rational M = DeviationsFromAverageSlopeLine(ultimatelyLower)
            .Max();

        //fh(t) >= m + fh.PseudoPeriodAverageSlope * t; t >= fh.PseudoPeriodStartTime
        Rational m = DeviationsFromAverageSlopeLine(ultimatelyHigher)
            .Min();

        //Intersection points between the two bounds
        var boundIntersection = (M - m) / (ultimatelyHigher.PseudoPeriodAverageSlope - ultimatelyLower.PseudoPeriodAverageSlope);
        return boundIntersection;

        List<Rational> DeviationsFromAverageSlopeLine(Curve f)
        {
            var deviations = new List<Rational>();
            foreach (var element in f.PseudoPeriodicElements)
            {
                //As the bounds are meaningful only for finite elements, we exclude infinite ones.
                if (element.IsInfinite)
                    continue;

                switch (element)
                {
                    case Point p:
                        deviations.Add(p.Value - f.PseudoPeriodAverageSlope * p.Time);
                        break;

                    case Segment s:
                        deviations.Add(s.RightLimitAtStartTime - f.PseudoPeriodAverageSlope * s.StartTime);
                        deviations.Add(s.LeftLimitAtEndTime - f.PseudoPeriodAverageSlope * s.EndTime);
                        break;

                    default:
                        throw new InvalidCastException();
                }
            }
            return deviations;
        }
    }

    /// <summary>
    /// Implements (min, +)-algebra minimum operation over two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the minimum.</returns>
    /// <remarks>
    /// Defined in [BT07] Section 4.3
    /// </remarks>
    public static Curve Minimum(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Minimum(b, settings);

    /// <summary>
    /// Implements (min, +)-algebra minimum operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall minimum.</returns>
    public static Curve Minimum(IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
            
        //todo: move these to ComputationSettings
        const int Parallelization_CountThreshold = 8;

        const int Parallelization_ComplexityMinimumCount = 4;
        const int Parallelization_ComplexityThreshold = 5_000;

        if (!curves.Any())
            throw new ArgumentException("The enumerable is empty");
        if (curves.Count == 1)
            return curves.Single();
        if (curves.Count == 2)
            return Minimum(curves.ElementAt(0), curves.ElementAt(1), settings);

        bool parallelizeDueCount = 
            settings.UseParallelListMinimum && 
            curves.Count >= Parallelization_CountThreshold;
        bool parallelizeDueComplexity =
            settings.UseParallelListMinimum &&
            curves.Count >= Parallelization_ComplexityMinimumCount &&
            curves.Average(c => c.BaseSequence.Count) >= Parallelization_ComplexityThreshold;

        if (parallelizeDueCount || parallelizeDueComplexity)
        {
            return curves
                .AsParallel()
                .Aggregate((a, b) => Minimum(a, b, settings));
        }
        else
        {
            Curve current = curves.First();
            int i = 1;
            foreach (var curve in curves.Skip(1))
            {
                #if DO_LOG
                logger.Trace($"Minimum {i} of {curves.Count - 1}");
                #endif
                i++;
                current = Minimum(current, curve, settings);
            }

            return current;
        }
    }

    /// <summary>
    /// Implements (min, +)-algebra minimum operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall minimum.</returns>
    public static Curve Minimum(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        bool doParallel = settings.UseParallelListMinimum;
        Curve result;
        if (doParallel)
        {
            result = curves
                .AsParallel()
                .Aggregate((a, b) => Minimum(a, b, settings));
        }
        else
        {
            result = curves
                .Aggregate((a, b) => Minimum(a, b, settings));
        }

        return result;
    }

    /// <summary>
    /// Implements (max, +)-algebra maximum operation over two curves.
    /// </summary>
    /// <param name="curve">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the maximum.</returns>
    /// <remarks>
    /// Defined in [BT07] Section 4.3
    /// </remarks>
    public virtual Curve Maximum(Curve curve, ComputationSettings? settings = null)
    {
        // Renaming for simmetry
        var a = this;
        var b = curve;

        settings ??= ComputationSettings.Default();

        #if DO_LOG
        logger.Trace($"Computing maximum of f1 ({a.BaseSequence.Count} elements, T: {a.PseudoPeriodStart} d: {a.PseudoPeriodLength})" +
                     $" and f2 ({b.BaseSequence.Count} elements, T: {b.PseudoPeriodStart} d: {b.PseudoPeriodLength})");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        Rational c, d, T; //Values for the resultFunction

        if (a.PseudoPeriodAverageSlope == b.PseudoPeriodAverageSlope)
        {
            d = EarliestValidLength();
            c = d * a.PseudoPeriodAverageSlope;
            T = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart);

            Rational EarliestValidLength()
            {
                //Optimization: avoid enlargement of lengths if a curve is Ultimately Affine
                if (a.IsUltimatelyAffine)
                    return b.PseudoPeriodLength;
                if (b.IsUltimatelyAffine)
                    return a.PseudoPeriodLength;

                //Optimization: avoid enlargement of lengths if a curve is trivially above the other
                var maxBaseSequenceEnd = Rational.Max(a.FirstPseudoPeriodEnd, b.FirstPseudoPeriodEnd);
                var aBaseCut = a.Cut(0, maxBaseSequenceEnd, settings: settings);
                var bBaseCut = b.Cut(0, maxBaseSequenceEnd, settings: settings);
                if (Sequence.GreaterOrEqual(aBaseCut, bBaseCut, settings) && a.PseudoPeriodAverageSlope >= b.PseudoPeriodAverageSlope)
                    return a.PseudoPeriodLength;
                if (Sequence.GreaterOrEqual(bBaseCut, aBaseCut, settings) && b.PseudoPeriodAverageSlope >= a.PseudoPeriodAverageSlope)
                    return b.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            }
        }
        else
        {
            Curve ultimatelyLower, ultimatelyHigher;

            if (a.PseudoPeriodAverageSlope < b.PseudoPeriodAverageSlope)
            {
                ultimatelyLower = a;
                ultimatelyHigher = b;
            }
            else
            {
                ultimatelyLower = b;
                ultimatelyHigher = a;
            }

            d = ultimatelyHigher.PseudoPeriodLength;
            c = ultimatelyHigher.PseudoPeriodHeight;

            if (ultimatelyHigher.PseudoPeriodAverageSlope.IsFinite && ultimatelyLower.PseudoPeriodAverageSlope.IsFinite)
            {
                Rational boundsIntersection = BoundsIntersection(ultimatelyLower: ultimatelyLower, ultimatelyHigher: ultimatelyHigher);
                T = Rational.Max(boundsIntersection, a.PseudoPeriodStart, b.PseudoPeriodStart);
#if TRACE_MIN_MAX_EXTENSIONS
                #if DO_LOG
                logger.Trace($"Maximum, different slopes: {a.PseudoPeriodAverageSlope} ~ {(decimal)a.PseudoPeriodAverageSlope}, {b.PseudoPeriodAverageSlope} ~ {(decimal)b.PseudoPeriodAverageSlope}");
                #endif
                logger.Trace($"Maximum, different slopes: must extend from \n" +
                             $"{a.PseudoPeriodStart} ~ {(decimal)a.PseudoPeriodStart} \n" +
                             $"{b.PseudoPeriodStart} ~ {(decimal)b.PseudoPeriodStart} \n" +
                             $"to {T} ~ {(decimal)T}");
#endif
            }
            else
            {
                T = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart);
            }
        }

        #if DO_LOG
        logger.Debug($"Maximum: extending from T1 {a.PseudoPeriodStart} d1 {a.PseudoPeriodLength}  T2 {b.PseudoPeriodStart} d2 {b.PseudoPeriodLength} to T {T} d {d}");
        #endif

        Sequence extendedSegmentsF1 = a.Extend(T + d, settings);
        Sequence extendedSegmentsF2 = b.Extend(T + d, settings);

        #if DO_LOG
        logger.Debug($"Maximum: extending from {a.BaseSequence.Count} and {b.BaseSequence.Count} to {extendedSegmentsF1.Count} and {extendedSegmentsF2.Count}");
        #endif

        Sequence maxSequence = extendedSegmentsF1.Maximum(extendedSegmentsF2, settings: settings);

        Curve result = new Curve
        (
            baseSequence: maxSequence,
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );

        var retVal = settings.AutoOptimize ? result.Optimize() : result;

        #if DO_LOG
        timer.Stop();
        #endif
        #if DO_LOG
        logger.Debug($"Maximum: took {timer.Elapsed}; a {a.BaseSequence.Count} b {b.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {retVal}");
        #endif
        return retVal;
    }

    /// <summary>
    /// Implements (max, +)-algebra maximum operation over two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the maximum.</returns>
    /// <remarks>
    /// Defined in [BT07] Section 4.3
    /// </remarks>
    public static Curve Maximum(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Maximum(b, settings);

    /// <summary>
    /// Implements (max, +)-algebra maximum operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall maximum.</returns>
    public static Curve Maximum(IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //todo: move these to ComputationSettings
        const int Parallelization_CountThreshold = 8;

        const int Parallelization_ComplexityMinimumCount = 4;
        const int Parallelization_ComplexityThreshold = 5_000;

        if (!curves.Any())
            throw new ArgumentException("The enumerable is empty");
        if (curves.Count == 1)
            return curves.Single();
        if (curves.Count == 2)
            return Maximum(curves.ElementAt(0), curves.ElementAt(1), settings);

        bool parallelizeDueCount =
            settings.UseParallelListMaximum &&
            curves.Count >= Parallelization_CountThreshold;
        bool parallelizeDueComplexity =
            settings.UseParallelListMaximum &&
            curves.Count >= Parallelization_ComplexityMinimumCount &&
            curves.Average(c => c.BaseSequence.Count) >= Parallelization_ComplexityThreshold;

        if (parallelizeDueCount || parallelizeDueComplexity)
        {
            return curves
                .AsParallel()
                .Aggregate((a, b) => Maximum(a, b, settings));
        }
        else
        {
            Curve current = curves.First();
            int i = 1;
            foreach (var curve in curves.Skip(1))
            {
                #if DO_LOG
                logger.Trace($"Maximum {i} of {curves.Count - 1}");
                #endif
                i++;
                current = Maximum(current, curve, settings);
            }

            return current;
        }
    }


    /// <summary>
    /// Implements (max, +)-algebra maximum operation over a set of curves.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall maximum.</returns>
    public static Curve Maximum(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        bool doParallel = settings.UseParallelListMaximum;
        Curve result;
        if (doParallel)
        {
            result = curves
                .AsParallel()
                .Aggregate((a, b) => Maximum(a, b, settings));
        }
        else
        {
            result = curves
                .Aggregate((a, b) => Maximum(a, b, settings));
        }

        return result;
    }

    /// <summary>
    /// Computes the minimum between the curve and element.
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its domain.
    /// </summary>
    public Curve Minimum(Element e, ComputationSettings? settings = null)
    {
        var s = Sequence.Fill(new Element[] { e }, 0, e.EndTime + 2).ToSequence();
        var ce = new Curve(
            baseSequence: s,
            pseudoPeriodStart: e.EndTime + 1,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );
        return this.Minimum(ce, settings);
    }

    /// <summary>
    /// Computes the minimum between the curve and element.
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its domain.
    /// </summary>
    public static Curve Minimum(Curve c, Element e, ComputationSettings? settings = null)
        => c.Minimum(e, settings);
    
    /// <summary>
    /// Computes the maximum between the curve and element.
    /// The element is considered to have $e(t) = -\infty$ for any $t$ outside its domain.
    /// </summary>
    public Curve Maximum(Element e, ComputationSettings? settings = null)
    {
        var s = Sequence.Fill(new Element[] { e }, 0, e.EndTime + 2, fillWith: Rational.MinusInfinity).ToSequence();
        var ce = new Curve(
            baseSequence: s,
            pseudoPeriodStart: e.EndTime + 1,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 0
        );
        return this.Maximum(ce, settings);
    }

    /// <summary>
    /// Computes the maximum between the curve and element.
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its domain.
    /// </summary>
    public static Curve Maximum(Curve c, Element e, ComputationSettings? settings = null)
        => c.Maximum(e, settings);
    
    #endregion Minimum and maximum operators

    #region Convolution operator

    /// <summary>
    /// Computes the convolution of the two curves, $f \otimes g$.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the convolution.</returns>
    /// <remarks>Described in [BT07] Section 4.4</remarks>
    public virtual Curve Convolution(Curve curve, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
            
        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var a = this;
        var b = curve;

        //Checks for convolution with infinite curves
        if (a.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return b + a.ValueAt(0);
        if (b.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return a + b.ValueAt(0);

        //Checks for convolution of positive curve with zero
        if (a.IsIdenticallyZero || b.IsIdenticallyZero)
        {
            if (a.IsIdenticallyZero && b.IsNonNegative)
                return a;
            if (b.IsIdenticallyZero && a.IsNonNegative)
                return b;
        }
        
        #if DO_LOG
        logger.Trace($"Computing convolution of f1 ({a.BaseSequence.Count} elements, T: {a.PseudoPeriodStart} d: {a.PseudoPeriodLength})" +
                     $" and f2 ({b.BaseSequence.Count} elements, T: {b.PseudoPeriodStart} d: {b.PseudoPeriodLength})");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"f1:\n {a} \n f2:\n {b}");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        Curve result;
        if (settings.SinglePassConvolution && a.PseudoPeriodAverageSlope == b.PseudoPeriodAverageSlope)
        {
            #if DO_LOG
            logger.Trace("Convolution: same slope, single pass");
            #endif
            var d = Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            var T = a.PseudoPeriodStart + b.PseudoPeriodStart + d;
            var c = a.PseudoPeriodAverageSlope * d;

            var cutEnd = T + d;
            var aCut = a.Cut(0, cutEnd, settings: settings);
            var bCut = b.Cut(0, cutEnd, settings: settings);
            var seq = Sequence.Convolution(aCut, bCut, settings, cutEnd);

            result = new Curve(
                baseSequence: seq.Cut(0, cutEnd),
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );
            if (settings.AutoOptimize)
                result = result.Optimize();
        }
        else
        {
            var terms = new List<Curve>();
            if (Equivalent(a, b, settings))
            {
                // self convolution: skip duplicate middle term
                if (a.HasTransient)
                    terms.Add(ConvolutionTransientTransient(a, a));

                if (a.HasTransient && !a.IsWeaklyUltimatelyInfinite)
                    terms.Add(ConvolutionTransientPeriodic(a, a));

                if (!a.IsWeaklyUltimatelyInfinite)
                    terms.Add(ConvolutionPeriodicPeriodic(a, a));
            }
            else
            {
                if (a.HasTransient)
                {
                    if (b.HasTransient)
                        terms.Add(ConvolutionTransientTransient(a, b));
                    if (!b.IsWeaklyUltimatelyInfinite)
                        terms.Add(ConvolutionTransientPeriodic(a, b));
                }

                if (!a.IsWeaklyUltimatelyInfinite)
                {
                    if (b.HasTransient)
                        terms.Add(ConvolutionTransientPeriodic(b, a));
                    if (!b.IsWeaklyUltimatelyInfinite)
                        terms.Add(ConvolutionPeriodicPeriodic(a, b));
                }
            }
            result = Minimum(terms, settings);
        }

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Convolution: took {timer.Elapsed}; a {a.BaseSequence.Count} b {b.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {result}");
        #endif
        return result;

        // Computes a partial convolution term, that is the convolution of two transient parts.
        // Described in [BT07] Section 4.4.3
        Curve ConvolutionTransientTransient(
            Curve firstTransientCurve,
            Curve secondTransientCurve)
        {
            Sequence firstTransientSequence = new Sequence(firstTransientCurve.TransientElements);
            Sequence secondTransientSequence = new Sequence(secondTransientCurve.TransientElements);

            #if DO_LOG
            logger.Trace("Convolution: transient x transient");
            #endif
            Sequence convolution = Sequence.Convolution(firstTransientSequence, secondTransientSequence, settings);

            //Has no actual meaning
            Rational d = Rational.Max(firstTransientCurve.PseudoPeriodLength, secondTransientCurve.PseudoPeriodLength);

            Rational T = convolution.DefinedUntil;

            Sequence extendedConvolution = new Sequence(
                elements: convolution.Elements,
                fillFrom: 0,
                fillTo: T + d
            );

            var result =  new Curve(
                baseSequence: extendedConvolution,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: Rational.PlusInfinity
            );
                
            return settings.AutoOptimize ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of a transient part and a pseudo-periodic one.
        // Described in [BT07] Sections 4.4.4 and .5
        Curve ConvolutionTransientPeriodic(
            Curve transientCurve,
            Curve periodicCurve)
        {
            Rational T = transientCurve.PseudoPeriodStart + periodicCurve.PseudoPeriodStart;
            Rational d = periodicCurve.PseudoPeriodLength;
            Rational c = periodicCurve.PseudoPeriodHeight;

            var transientSequence = new Sequence(transientCurve.TransientElements);
            var periodicSequence = periodicCurve.Cut(periodicCurve.PseudoPeriodStart, T + d, settings: settings);

            #if DO_LOG
            logger.Trace("Convolution: transient x periodic");
            #endif
            var limitedConvolution = Sequence.Convolution(transientSequence, periodicSequence, settings);

            var result = new Curve(
                baseSequence: limitedConvolution.Cut(periodicCurve.PseudoPeriodStart, T + d),
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c,
                isPartialCurve: true
            );
                
            return settings.AutoOptimize ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of two pseudo-periodic parts.
        // Described in [BT07] Section 4.4.6
        Curve ConvolutionPeriodicPeriodic(
            Curve firstPeriodicCurve,
            Curve secondPeriodicCurve)
        {
            Rational d = EarliestValidLength();
            var t1 = firstPeriodicCurve.PseudoPeriodStart;
            var t2 = secondPeriodicCurve.PseudoPeriodStart;
            var T = t1 + t2 + d;
            Rational c = d * Rational.Min(firstPeriodicCurve.PseudoPeriodAverageSlope, secondPeriodicCurve.PseudoPeriodAverageSlope);

            #if DO_LOG
            logger.Debug(
                $"Convolution: extending from T1 {t1} d1 {firstPeriodicCurve.PseudoPeriodLength}  T2 {t2} d2 {secondPeriodicCurve.PseudoPeriodLength} to T {T} d {d}");
            #endif

            Sequence firstPeriodicSequence = firstPeriodicCurve.Cut(t1, t1 + 2*d, settings: settings);
            Sequence secondPeriodicSequence = secondPeriodicCurve.Cut(t2, t2 + 2*d, settings: settings);

            #if DO_LOG
            logger.Debug(
                $"Convolution: extending from {firstPeriodicCurve.PseudoPeriodicSequence.Count} and {secondPeriodicCurve.PseudoPeriodicSequence.Count} to {firstPeriodicSequence.Count} and {secondPeriodicSequence.Count}");
            #endif

            #if DO_LOG
            logger.Trace("Convolution: periodic x periodic");
            #endif
            var cutEnd = T + d;
            Sequence limitedConvolution = Sequence.Convolution(firstPeriodicSequence, secondPeriodicSequence, settings, cutEnd);

            var result = new Curve(
                baseSequence: limitedConvolution.Cut(t1 + t2, cutEnd),
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c,
                isPartialCurve: true
            );
                
            return settings.AutoOptimize ? result.Optimize() : result;
                
            Rational EarliestValidLength()
            {
                //Optimization: avoid enlargment of lengths if a curve is Ultimately Affine
                if (firstPeriodicCurve.IsUltimatelyAffine)
                    return secondPeriodicCurve.PseudoPeriodLength;
                if (secondPeriodicCurve.IsUltimatelyAffine)
                    return firstPeriodicCurve.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(firstPeriodicCurve.PseudoPeriodLength, secondPeriodicCurve.PseudoPeriodLength);
            }
        }
    }

    /// <summary>
    /// Computes the convolution of the two curves, $a \otimes b$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Described in [BT07] Section 4.4</remarks>
    public static Curve Convolution(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Convolution(b, settings);

    /// <summary>
    /// Computes the convolution of a set of curves, $\bigotimes{f_i}$.
    /// </summary>
    /// <param name="curves">The set of curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall convolution.</returns>
    public static Curve Convolution(IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        const int ParallelizationThreshold = 8;

        #if DO_LOG
        logger.Debug($"Generic list convolution, {curves.Count} curves");
        #endif

        if (settings.UseParallelListConvolution && curves.Count > ParallelizationThreshold)
        {
            return curves
                .AsParallel()
                .Aggregate((a , b) => Convolution(a, b, settings));
        }
        else
        {
            return curves
                .Aggregate((a , b) => Convolution(a, b, settings));
        }
    }
        
    /// <summary>
    /// Computes the convolution of a set of curves, $\bigotimes{f_i}$.
    /// </summary>
    /// <param name="curves">The set of curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall convolution.</returns>
    /// <remarks>Optimized for minimal allocations.</remarks>
    public static Curve Convolution(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        #if DO_LOG
        logger.Debug($"Generic list convolution, IEnumerable");
        #endif

        if (settings.UseParallelListConvolution)
        {
            return curves
                .AsParallel()
                .Aggregate((a , b) => Convolution(a, b, settings));
        }
        else
        {
            return curves
                .Aggregate((a , b) => Convolution(a, b, settings));
        }
    }

    #endregion Convolution operator

    #region EstimateConvolution
 
    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public virtual long EstimateConvolution(Curve curve, bool countElements = false, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
            
        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var a = this;
        var b = curve;
            
        //Checks for convolution with infinite curves
        if (a.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return 0;
        if (b.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return 0;

        //Checks for convolution with null curves
        if (a.IsIdenticallyZero || b.IsIdenticallyZero)
            return 0;

        #if DO_LOG
        logger.Trace($"Estimating convolution of f1 ({a.BaseSequence.Count} elements, T: {a.PseudoPeriodStart} d: {a.PseudoPeriodLength})" +
                     $" and f2 ({b.BaseSequence.Count} elements, T: {b.PseudoPeriodStart} d: {b.PseudoPeriodLength})");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"f1:\n {a} \n f2:\n {b}");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        long result;
        if (settings.SinglePassConvolution && a.PseudoPeriodAverageSlope == b.PseudoPeriodAverageSlope)
        {
            #if DO_LOG
            logger.Trace("Convolution: same slope, single pass");
            #endif
            var d = Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            var T = a.PseudoPeriodStart + b.PseudoPeriodStart + d;
            var c = a.PseudoPeriodAverageSlope * d;

            var aCut = a.Cut(0, T + d);
            var bCut = b.Cut(0, T + d);
            result = Sequence.EstimateConvolution(aCut, bCut, settings, T + d, countElements);
        }
        else
        {
            var terms = new List<long>();
            if (Equivalent(a, b))
            {
                // self convolution: skip duplicate middle term
                if (a.HasTransient)
                    terms.Add(EstimateTransientTransient(a, a));

                if (a.HasTransient && !a.IsWeaklyUltimatelyInfinite)
                    terms.Add(EstimateTransientPeriodic(a, a));

                if (!a.IsWeaklyUltimatelyInfinite)
                    terms.Add(EstimatePeriodicPeriodic(a, a));
            }
            else
            {
                if (a.HasTransient)
                {
                    if (b.HasTransient)
                        terms.Add(EstimateTransientTransient(a, b));
                    if (!b.IsWeaklyUltimatelyInfinite)
                        terms.Add(EstimateTransientPeriodic(a, b));
                }

                if (!a.IsWeaklyUltimatelyInfinite)
                {
                    if (b.HasTransient)
                        terms.Add(EstimateTransientPeriodic(b, a));
                    if (!b.IsWeaklyUltimatelyInfinite)
                        terms.Add(EstimatePeriodicPeriodic(a, b));
                }
            }
            result = terms.Sum();
        }

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Estimate convolution: took {timer.Elapsed}; a {a.BaseSequence.Count} b {b.BaseSequence.Count} => [{countElements}] {result}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {result}");
        #endif
        return result;

        // Computes a partial convolution term, that is the convolution of two transient parts.
        // Described in [BT07] Section 4.4.3
        long EstimateTransientTransient(
            Curve firstTransientCurve,
            Curve secondTransientCurve)
        {
            Sequence firstTransientSequence = new Sequence(firstTransientCurve.TransientElements);
            Sequence secondTransientSequence = new Sequence(secondTransientCurve.TransientElements);

            #if DO_LOG
            logger.Trace("Estimate convolution: transient x transient");
            #endif
            var result = Sequence.EstimateConvolution(firstTransientSequence, secondTransientSequence, settings, countElements: countElements);
                
            return result;
        }

        // Computes a partial convolution term, that is the convolution of a transient part and a pseudo-periodic one.
        // Described in [BT07] Sections 4.4.4 and .5
        long EstimateTransientPeriodic(
            Curve transientCurve,
            Curve periodicCurve)
        {
            Rational T = transientCurve.PseudoPeriodStart + periodicCurve.PseudoPeriodStart;
            Rational d = periodicCurve.PseudoPeriodLength;
            Rational c = periodicCurve.PseudoPeriodHeight;

            var transientSequence = new Sequence(transientCurve.TransientElements);
            var periodicSequence = periodicCurve.Cut(periodicCurve.PseudoPeriodStart, T + d);

            #if DO_LOG
            logger.Trace("Estimate convolution: transient x periodic");
            #endif
            var result = Sequence.EstimateConvolution(transientSequence, periodicSequence, settings, countElements: countElements);
                
            return result;
        }

        // Computes a partial convolution term, that is the convolution of two pseudo-periodic parts.
        // Described in [BT07] Section 4.4.6
        long EstimatePeriodicPeriodic(
            Curve firstPeriodicCurve,
            Curve secondPeriodicCurve)
        {
            Rational d = EarliestValidLength();
            var t1 = firstPeriodicCurve.PseudoPeriodStart;
            var t2 = secondPeriodicCurve.PseudoPeriodStart;
            var T = t1 + t2 + d;
            Rational c = d * Rational.Min(firstPeriodicCurve.PseudoPeriodAverageSlope, secondPeriodicCurve.PseudoPeriodAverageSlope);

            #if DO_LOG
            logger.Debug($"Estimate convolution: extending from T1 {t1} d1 {firstPeriodicCurve.PseudoPeriodLength}  T2 {t2} d2 {secondPeriodicCurve.PseudoPeriodLength} to T {T} d {d}");
            #endif
                
            Sequence firstPeriodicSequence = firstPeriodicCurve.Cut(t1, t1 + 2*d);
            Sequence secondPeriodicSequence = secondPeriodicCurve.Cut(t2, t2 + 2*d);

            #if DO_LOG
            logger.Debug($"Estimate convolution: extending from {firstPeriodicCurve.PseudoPeriodicSequence.Count} and {secondPeriodicCurve.PseudoPeriodicSequence.Count} to {firstPeriodicSequence.Count} and {secondPeriodicSequence.Count}");
            #endif

            #if DO_LOG
            logger.Trace("Estimate convolution: periodic x periodic");
            #endif
            var result = Sequence.EstimateConvolution(firstPeriodicSequence, secondPeriodicSequence, settings, T + d, countElements: countElements);

            return result;
                
            Rational EarliestValidLength()
            {
                //Optimization: avoid enlargement of lengths if a curve is Ultimately Affine
                if (firstPeriodicCurve.IsUltimatelyAffine)
                    return secondPeriodicCurve.PseudoPeriodLength;
                if (secondPeriodicCurve.IsUltimatelyAffine)
                    return firstPeriodicCurve.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(firstPeriodicCurve.PseudoPeriodLength, secondPeriodicCurve.PseudoPeriodLength);
            }
        }
    }
        
    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public static long EstimateConvolution(Curve a, Curve b, bool countElements = false, ComputationSettings? settings = null)
        => a.EstimateConvolution(b, countElements, settings);

    #endregion EstimateConvolution

    #region Deconvolution operator

    /// <summary>
    /// Computes the deconvolution of two curves, $f \oslash g$.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution.</returns>
    /// <remarks>
    /// The result is not forced to have $f(0) = 0$, see <see cref="WithZeroOrigin"/> to have this property.
    /// Described in [BT07] Section 4.5 .
    /// </remarks>
    public virtual Curve Deconvolution(Curve curve, ComputationSettings? settings = null)
    {
        if (PseudoPeriodAverageSlope > curve.PseudoPeriodAverageSlope)
        {
            return PlusInfinite();
        }
        else
        {
            Rational T = Rational.Max(PseudoPeriodStart, curve.PseudoPeriodStart) + Rational.LeastCommonMultiple(PseudoPeriodLength, curve.PseudoPeriodLength);

            Sequence firstCut = Cut(0, T + FirstPseudoPeriodEnd, settings: settings);
            Sequence secondCut = curve.Cut(0, T, settings: settings);
            Sequence cutDeconvolution = Sequence.Deconvolution(firstCut, secondCut, 0, FirstPseudoPeriodEnd, settings).Optimize();

            return new Curve(
                baseSequence: cutDeconvolution,
                pseudoPeriodStart: PseudoPeriodStart,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
    }

    /// <summary>
    /// Computes the deconvolution of the two curves, $a \oslash b$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution</returns>
    /// <remarks>
    /// The result is not forced to have $f(0) = 0$, see <see cref="WithZeroOrigin"/> to have this property.
    /// Described in [BT07] Section 4.5 .
    /// </remarks>
    public static Curve Deconvolution(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Deconvolution(b, settings);

    #endregion Deconvolution operator

    #region Sub-additive closure

    /// <summary>
    /// Computes the sub-additive closure of the curve.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 7</remarks>
    public static SubAdditiveCurve SubAdditiveClosure(Curve curve, ComputationSettings? settings = null)
        => curve.SubAdditiveClosure(settings);
        
    /// <summary>
    /// Computes the sub-additive closure of the curve.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Described in [BT07] Section 4.6 as algorithm 7</remarks>
    public virtual SubAdditiveCurve SubAdditiveClosure(ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Debug($"Computing closure of {TransientElements.Count()} transient and {PseudoPeriodicElements.Count()} pseudo-periodic elements.");
        #endif
        settings ??= ComputationSettings.Default();
            
        var transientClosures = TransientElements
            .Select(element => element.SubAdditiveClosure(settings));

        var periodicClosures = PseudoPeriodicElements
            .Select(element => element.SubAdditiveClosure(
                pseudoPeriodHeight: PseudoPeriodHeight,
                pseudoPeriodLength: PseudoPeriodLength,
                settings: settings
            ));

        var elementClosuresEnumerable = Enumerable.Concat(transientClosures, periodicClosures);
        var elementClosures = settings.UseParallelism
            ? elementClosuresEnumerable.AsParallel().ToList()
            : elementClosuresEnumerable.ToList();
        
        #if DO_LOG
        logger.Debug($"Computed individual closures, next is convolution step.");
        #endif

        if (settings.UseSubAdditiveConvolutionOptimizations)
            return SubAdditiveCurve.Convolution(elementClosures, settings);
        else
            return new SubAdditiveCurve(Curve.Convolution(elementClosures, settings), false);
    }

    #endregion Sub-additive closure

    #region Max-plus operators
    
    /// <summary>
    /// Computes the max-plus convolution of the two curves, $f \overline{\otimes} g$.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the max-plus convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public virtual Curve MaxPlusConvolution(Curve curve, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Computing max-plus convolution");
        #endif
        return -Convolution(-this, -curve, settings);
    }

    /// <summary>
    /// Computes the max-plus convolution of the two curves, $f \overline{\otimes} g$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus convolution</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static Curve MaxPlusConvolution(Curve a, Curve b, ComputationSettings? settings = null)
        => a.MaxPlusConvolution(b, settings);

    /// <summary>
    /// Computes the max-plus convolution of a set of curves.
    /// </summary>
    /// <param name="curves">The set of curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall max-plus convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static Curve MaxPlusConvolution(IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        const int ParallelizationThreshold = 8;

        #if DO_LOG
        logger.Debug($"Generic list max-plus convolution, {curves.Count} curves");
        #endif

        if (settings.UseParallelListConvolution && curves.Count > ParallelizationThreshold)
        {
            return curves
                .AsParallel()
                .Aggregate((a , b) => MaxPlusConvolution(a, b, settings));
        }
        else
        {
            return curves
                .Aggregate((a , b) => MaxPlusConvolution(a, b, settings));
        }
    }

    /// <summary>
    /// Computes the max-plus convolution of a set of curves, $f \overline{\otimes} g$.
    /// </summary>
    /// <param name="curves">The set of curves to be convolved.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the overall max-plus convolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    /// <remarks>Optimized for minimal allocations</remarks>
    public static Curve MaxPlusConvolution(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        #if DO_LOG
        logger.Debug($"Generic list max-plus convolution, IEnumerable");
        #endif

        if (settings.UseParallelListConvolution)
        {
            return curves
                .AsParallel()
                .Aggregate((a , b) => MaxPlusConvolution(a, b, settings));
        }
        else
        {
            return curves
                .Aggregate((a , b) => MaxPlusConvolution(a, b, settings));
        }
    }

    /// <summary>
    /// Computes the max-plus deconvolution of two curves.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus deconvolution.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public virtual Curve MaxPlusDeconvolution(Curve curve, ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Computing max-plus deconvolution");
        #endif
        return -Deconvolution(-this, -curve, settings);
    }

    /// <summary>
    /// Computes the max-plus deconvolution of the two curves
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus deconvolution</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static Curve MaxPlusDeconvolution(Curve a, Curve b, ComputationSettings? settings = null)
        => a.MaxPlusDeconvolution(b, settings);

    /// <summary>
    /// Computes the super-additive closure of the curve.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The result of the super-additive closure.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public static SuperAdditiveCurve SuperAdditiveClosure(Curve curve, ComputationSettings? settings = null)
        => curve.SuperAdditiveClosure(settings);

    /// <summary>
    /// Computes the super-additive closure of the curve.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>The result of the super-additive closure.</returns>
    /// <remarks>Max-plus operators are defined through min-plus operators, see [DNC18] Section 2.4</remarks>
    public virtual SuperAdditiveCurve SuperAdditiveClosure(ComputationSettings? settings = null)
    {
        #if DO_LOG
        logger.Trace("Computing super-additive closure");
        #endif
        var result = -((-this).SubAdditiveClosure(settings));
        return new SuperAdditiveCurve(result, false);
    }

    #endregion Max-plus operators

    #region Composition

    /// <summary>
    /// Compute the composition of this curve, $f$, and $g$, i.e. $f(g(t))$.
    /// </summary>
    /// <param name="g">Inner function, must be non-negative and non-decreasing.</param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException">If the operands are not defined as expected.</exception>
    /// <returns>The result of the composition.</returns>
    /// <remarks>
    /// Algorithmic properties discussed in [ZNS22].
    /// </remarks>
    public virtual Curve Composition(Curve g, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        var f = this;
        
        if (!g.IsNonNegative)
            throw new ArgumentException("g must be non-negative");
        if (!g.IsNonDecreasing)
            throw new ArgumentException("g must be non-decreasing");

        var T_g = g.PseudoPeriodStart;
        var T_f = g.LowerPseudoInverse()
            .ValueAt(f.PseudoPeriodStart);
        
        // initialized with non-optimal values
        var T = Rational.Max(T_g, T_f);
        var d = f.PseudoPeriodLength.Numerator * g.PseudoPeriodLength * g.PseudoPeriodHeight.Denominator;
        var c = f.PseudoPeriodLength.Denominator * g.PseudoPeriodHeight.Numerator * f.PseudoPeriodHeight;
        
        if (settings.UseCompositionOptimizations)
        {
            if (f.IsUltimatelyConstant || g.IsUltimatelyConstant)
            {
                // composition will also be U.C.
                // the following expression for T summarise Proposition 19, 20 and 21 from [ZNS22]
                T = Rational.Min(
                    g.IsUltimatelyConstant ? T_g : Rational.PlusInfinity, 
                    f.IsUltimatelyConstant ? T_f : Rational.PlusInfinity
                );
                d = 1;
                c = 0;
            }
            else if (f.IsUltimatelyAffine || g.IsUltimatelyAffine)
            {
                if (g.IsUltimatelyAffine && f.IsUltimatelyAffine)
                {
                    // composition will also be U.A. with rho = rho_f * rho_g
                    d = 1;
                    c = f.PseudoPeriodAverageSlope * g.PseudoPeriodAverageSlope;
                }
                else if (f.IsUltimatelyAffine)
                {
                    d = g.PseudoPeriodLength;
                    c = g.PseudoPeriodHeight * f.PseudoPeriodAverageSlope;
                }
                else if (g.IsUltimatelyAffine)
                {
                    d = f.PseudoPeriodLength / g.PseudoPeriodAverageSlope;
                    c = f.PseudoPeriodHeight;
                }
            }
        }

        #if DO_LOG
        var sw = Stopwatch.StartNew();
        #endif
        var gCut = g.Cut(0, T + d);
        var fCut = f.Cut(g.ValueAt(0), g.LeftLimitAt(T + d), isEndInclusive: true);
        var sequence = Sequence.Composition(fCut, gCut);
        #if DO_LOG
        sw.Stop();
        logger.Trace($"{settings.UseCompositionOptimizations} {gCut.Count} {fCut.Count}");
        logger.Trace(sw.Elapsed);
        #endif
        
        var result = new Curve(
            baseSequence: sequence,
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );

        return settings.AutoOptimize ? result.Optimize() : result;
    }

    /// <summary>
    /// Compute the composition $f(g(t))$.
    /// </summary>
    /// <param name="f">Outer function.</param>
    /// <param name="g">Inner function, must be non-negative and non-decreasing.</param>
    /// <param name="settings"></param>
    /// <exception cref="ArgumentException">If the operands are not defined as expected.</exception>
    /// <returns>The result of the composition.</returns>
    /// <remarks>
    /// Algorithmic properties discussed in [ZNS22].
    /// </remarks>
    public static Curve Composition(Curve f, Curve g, ComputationSettings? settings = null)
        => f.Composition(g, settings);

    #endregion Composition
}

/// <summary>
/// Provides LINQ extensions methods for <see cref="Curve"/>, 
/// which are mostly shorthands to methods such as <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings?)"/>.
/// </summary>
public static class CurveExtensions
{
    /// <inheritdoc cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Addition(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Addition(curves, settings);

    /// <inheritdoc cref="Curve.Addition(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Addition(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Addition(curves, settings);

    /// <inheritdoc cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Minimum(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Minimum(curves, settings);

    /// <inheritdoc cref="Curve.Minimum(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Minimum(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Minimum(curves, settings);

    /// <inheritdoc cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve Convolution(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.Convolution(curves, settings);

    /// <inheritdoc cref="Curve.Convolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve Convolution(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.Convolution(curves, settings);

    /// <inheritdoc cref="Curve.MaxPlusConvolution(IEnumerable{Curve}, ComputationSettings?)"/>
    public static Curve MaxPlusConvolution(this IEnumerable<Curve> curves, ComputationSettings? settings = null)
        => Curve.MaxPlusConvolution(curves, settings);

    /// <inheritdoc cref="Curve.MaxPlusConvolution(IReadOnlyCollection{Curve}, ComputationSettings?)"/>
    public static Curve MaxPlusConvolution(this IReadOnlyCollection<Curve> curves, ComputationSettings? settings = null)
        => Curve.MaxPlusConvolution(curves, settings);
}