using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.NetworkCalculus.Json;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Utility;

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
/// Implementation of data structure described in [BT08] Section 4.1
/// </remarks>
/// <docs position="1"/>
[JsonObject(MemberSerialization.OptIn)]
[System.Text.Json.Serialization.JsonConverter(typeof(GenericCurveSystemJsonConverter))]
public class Curve : IToCodeString, IStableHashCode
{
    #if DO_LOG
    private static Logger logger = LogManager.GetCurrentClassLogger();
    #endif

    /// <summary>
    /// Type identification constant for JSON (de)serialization.
    /// </summary>
    /// <exclude />
    public const string TypeCode = "curve";

    #region Properties

    /// <summary>
    /// Point in time after which the curve has a pseudo-periodic behavior.
    /// </summary>
    /// <remarks>
    /// Referred to as $T$ or Rank in [BT08] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "pseudoPeriodStart")]
    [JsonPropertyName("pseudoPeriodStart")]
    public Rational PseudoPeriodStart { get; init; }

    /// <summary>
    /// Time length of each pseudo-period.
    /// </summary>
    /// <remarks>
    /// Referred to as $d$ in [BT08] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "pseudoPeriodLength")]
    [JsonPropertyName("pseudoPeriodLength")]
    public Rational PseudoPeriodLength { get; init; }

    /// <summary>
    /// Static value gain applied after each pseudo-period.
    /// If it's 0, the curve is truly periodic.
    /// </summary>
    /// <remarks>
    /// Referred to as $c$ in [BT08] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "pseudoPeriodHeight")]
    [JsonPropertyName("pseudoPeriodHeight")]
    public Rational PseudoPeriodHeight { get; init; }

    /// <summary>
    /// Average slope of curve in pseudo-periodic behavior.
    /// If it's 0, the curve is truly periodic.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational PseudoPeriodSlope =>
        PseudoPeriodicSequence.IsInfinite
            ? (PseudoPeriodicElements.First().IsPlusInfinite ? Rational.PlusInfinity : Rational.MinusInfinity)
            : PseudoPeriodHeight / PseudoPeriodLength;

    /// <summary>
    /// End time of the first pseudo period.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational FirstPseudoPeriodEnd =>
        PseudoPeriodStart + PseudoPeriodLength;

    /// <summary>
    /// End time of the second pseudo period.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational SecondPseudoPeriodEnd =>
        FirstPseudoPeriodEnd + PseudoPeriodLength;

    /// <summary>
    /// <see cref="Sequence"/> describing behavior of the curve in $[0, T + d[$.
    /// Combined with the UPP property, this is also allows to derive $f(t)$ for any $t \ge T + d$.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_1, ..., t_k]$ in [BT08] Section 4.1
    /// </remarks>
    [JsonProperty(PropertyName = "baseSequence")]
    [JsonPropertyName("baseSequence")]
    public Sequence BaseSequence { get; init; }

    /// <summary>
    /// True if the curve has finite value for any $t$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsFinite => BaseSequence.IsFinite;

    /// <summary>
    /// The first instant around which the curve is not infinite.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is finite.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational FirstFiniteTime => BaseSequence.FirstFiniteTime;

    /// <summary>
    /// The first instant around which the curve is not infinite, excluding the origin point.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t)$ is finite.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational FirstNonZeroTime =>
        IsZero
            ? Rational.PlusInfinity
            : Rational.Min(BaseSequence.FirstNonZeroTime, PseudoPeriodStart + PseudoPeriodLength);

    /// <summary>
    /// Returns the minimum $T_L$ such that $f(t + d) = f(t) + c$ for all $t > T_L$.
    /// It is the infimum of all valid <see cref="PseudoPeriodStart"/>, i.e. $T_L = \inf\{ T | f(t + d) = f(t) + c \forall t \ge T \}$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational PseudoPeriodStartInfimum
    {
        get
        {
            var opt = this.Optimize();
            if (!opt.HasTransient)
                return 0;
            if (!opt.IsLeftContinuousAt(opt.PseudoPeriodStart))
                return opt.PseudoPeriodStart;

            var lastTransientSegment = (Segment) opt.TransientElements.Last();
            var lastPeriodSegment = (Segment) opt.PseudoPeriodicElements.Last();
            if (lastTransientSegment.Slope == lastPeriodSegment.Slope)
                // since it is already a minimal curve, we can infer right-discontinuity in $T_L$
                return lastTransientSegment.StartTime;
            else
                return opt.PseudoPeriodStart;
        }
    }

    /// <summary>
    /// True if the curve is 0 for all $t$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsZero =>
        BaseSequence.IsZero && PseudoPeriodHeight.IsZero;

    /// <summary>
    /// True if the curve has $+\infty$ value for any $t$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsPlusInfinite =>
        this.Equivalent(PlusInfinite());

    /// <summary>
    /// True if the curve has $-\infty$ value for any $t$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsMinusInfinite =>
        this.Equivalent(MinusInfinite());

    /// <summary>
    /// True if $f(0) = 0$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsPassingThroughOrigin
        => _isPassingThroughOrigin ??= ValueAt(0) == 0;

    internal bool? _isPassingThroughOrigin;

    /// <summary>
    /// True if there is no infinite value or discontinuity within the curve.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
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
    /// True if there is no discontinuity within the curve, except at most in origin.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsContinuousExceptOrigin
    {
        get
        {
            if (!BaseSequence
                    .Cut(0, BaseSequence.DefinedUntil, isStartIncluded: false)
                    .IsContinuous)
                return false;

            Rational pseudoPeriodStartValue = ((Point)PseudoPeriodicElements.First()).Value;
            Rational pseudoPeriodLastValue = ((Segment)PseudoPeriodicElements.Last()).LeftLimitAtEndTime;

            bool isPeriodicContinuous = pseudoPeriodStartValue + PseudoPeriodHeight == pseudoPeriodLastValue;

            return isPeriodicContinuous;
        }
    }

    /// <summary>
    /// True if there is no left-discontinuity within the curve.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsLeftContinuous
        => _isLeftContinuous ??= Cut(0, SecondPseudoPeriodEnd).IsLeftContinuous;

    /// <summary>
    /// Private cache field for <see cref="IsLeftContinuous"/>
    /// </summary>
    internal bool? _isLeftContinuous;

    /// <summary>
    /// True if there is no right-discontinuity within the curve.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRightContinuous
        => _isRightContinuous ??= Cut(0, SecondPseudoPeriodEnd).IsRightContinuous;

    /// <summary>
    /// Private cache field for <see cref="IsRightContinuous"/>
    /// </summary>
    internal bool? _isRightContinuous;

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
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsNonNegative
        => _isNonNegative ??= InfValue() >= 0;

    /// <summary>
    /// Private cache field for <see cref="IsNonNegative"/>
    /// </summary>
    internal bool? _isNonNegative;

    /// <summary>
    /// True if the curve is non-negative over the given interval, i.e. i.e. $f(t) \ge 0$ for any $t$ in the given interval.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If an invalid interval is given.</exception>
    public bool IsNonNegativeOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        if (_isNonNegative is true)
            return true;

        var _end = end ?? Rational.PlusInfinity;

        if(start == 0 && _end == Rational.PlusInfinity)
            return IsNonNegative;
        else if(start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");
            return true;
        }
        else if(_end < Rational.PlusInfinity)
        {
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            return cut.IsNonNegative();
        }
        else
        {
            var cut = CutAsEnumerable(start, start + PseudoPeriodLength, isStartIncluded, true);
            return cut.IsNonNegative();
        }
    }

    /// <summary>
    /// The first instant $t^*$ around which the curve is non-negative.
    /// Does not specify whether it's inclusive or not, i.e. if $f(t^*) \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computes $t^* = \inf\{ t \ge 0 \mid f(t) \ge 0 \}$.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public Rational FirstNonNegativeTime
    {
        get
        {
            if (IsNonNegative)
                return 0;

            var t = FindFirstNonNegativeInSequence(BaseSequence.Elements);
            if (t != Rational.PlusInfinity)
                return t;

            if (PseudoPeriodSlope <= 0)
                return Rational.PlusInfinity;
            else
            {
                var k = (-ValueAt(PseudoPeriodStart) / PseudoPeriodHeight).FastFloor();
                return FindFirstNonNegativeInSequence(
                    CutAsEnumerable(PseudoPeriodStart + k * PseudoPeriodLength, PseudoPeriodStart + (k + 1) * PseudoPeriodLength, isEndIncluded: true)
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
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsNonDecreasing
        => _isNonDecreasing ??= CutAsEnumerable(0, SecondPseudoPeriodEnd).IsNonDecreasing();

    /// <summary>
    /// Private cache field for <see cref="IsNonDecreasing"/>
    /// </summary>
    internal bool? _isNonDecreasing;

    /// <summary>
    /// True if for any $t > s$, $f(t) > f(s)$.
    /// </summary>
    public bool IsIncreasing
        => _isIncreasing ??= CutAsEnumerable(0, SecondPseudoPeriodEnd).IsIncreasing();

    /// <summary>
    /// Private cache field for <see cref="IsIncreasing"/>
    /// </summary>
    internal bool? _isIncreasing;

    /// <summary>
    /// True if for any pair $t,s$ in the given interval, $t > s$, $f(t) \ge f(s)$.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If an invalid interval is given.</exception>
    public bool IsNonDecreasingOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        if (_isNonDecreasing is true)
            return true;

        var _end = end ?? Rational.PlusInfinity;

        if(start == 0 && _end == Rational.PlusInfinity)
            return IsNonDecreasing;
        else if(start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");
            return true;
        }
        else if(_end < Rational.PlusInfinity)
        {
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            return cut.IsNonDecreasing();
        }
        else
        {
            var cut = CutAsEnumerable(start, start + PseudoPeriodLength, isStartIncluded, true);
            return cut.IsNonDecreasing();
        }
    }

    /// <summary>
    /// True if for any $t_0$ interior (from the left) to the given interval, $lim_{t \to t_0^-}{f(t)} = f(t_0)$.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If an invalid interval is given.</exception>
    /// <remarks>See [Zippo23] Definition 14.4</remarks>
    public bool IsLeftContinuousOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        if (_isLeftContinuous is true)
            return true;

        var _end = end ?? Rational.PlusInfinity;

        if(start == 0 && _end == Rational.PlusInfinity)
            return IsLeftContinuous;
        else if(start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");
            return true;
        }
        else if(_end < Rational.PlusInfinity)
        {
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            return cut.IsLeftContinuous();
        }
        else
        {
            var cut = CutAsEnumerable(start, start + PseudoPeriodLength, isStartIncluded, true);
            return cut.IsLeftContinuous();
        }
    }

    /// <summary>
    /// True if for any $t_0$ interior (from the right) to the given interval, $lim_{t \to t_0^+}{f(t)} = f(t_0)$.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If an invalid interval is given.</exception>
    /// <remarks>See [Zippo23] Definition 14.5</remarks>
    public bool IsRightContinuousOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        if (_isRightContinuous is true)
            return true;

        var _end = end ?? Rational.PlusInfinity;

        if(start == 0 && _end == Rational.PlusInfinity)
            return IsRightContinuous;
        else if(start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");
            return true;
        }
        else if(_end < Rational.PlusInfinity)
        {
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            return cut.IsRightContinuous();
        }
        else
        {
            var cut = CutAsEnumerable(start, start + PseudoPeriodLength, isStartIncluded, true);
            return cut.IsRightContinuous();
        }
    }

    /// <summary>
    /// True if for all $t \ge$ <see cref="PseudoPeriodStart"/> the curve is finite.
    /// </summary>
    /// <remarks>
    /// This property does not check if $f(t), t &lt;$ <see cref="PseudoPeriodStart"/> is either infinite, finite or both.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyFinite =>
        PseudoPeriodicElements.All(elem => elem.IsFinite);

    /// <summary>
    /// True if, for some $T$, $f(t) = +\infty$ for all $t \ge T$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyPlusInfinite
        => CutAsEnumerable(PseudoPeriodStart, SecondPseudoPeriodEnd).IsPlusInfinite();

    /// <summary>
    /// True if, for some $T$, $f(t) = -\infty$ for all $t \ge T$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyMinusInfinite
        => CutAsEnumerable(PseudoPeriodStart, SecondPseudoPeriodEnd).IsMinusInfinite();

    /// <summary>
    /// True if, for $b \in \{-\infty, +\infty\}$ and some $T$, $f(t) = b$ for all $t \ge T$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyInfinite
        => IsUltimatelyPlusInfinite || IsUltimatelyMinusInfinite;

    /// <summary>
    /// True if for all $t >$ <see cref="PseudoPeriodStart"/> the curve is either always finite or always infinite.
    /// </summary>
    /// <remarks>
    /// Defined in [BT08], Definition 1.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyPlain =>
        IsUltimatelyFinite || IsUltimatelyPlusInfinite || IsUltimatelyMinusInfinite;

    /// <summary>
    /// True if $f$ is plain, i.e., it is either
    /// a) always finite,
    /// b) always plus or minus infinite (without changing sign),
    /// or c) finite up to a $T$, then always plus or minus infinite (without changing sign)
    /// </summary>
    /// <remarks>
    /// Formally defined in [BT08], Definition 1.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsPlain
    {
        get
        {
            if (IsFinite)
                return true;

            var ti = PseudoPeriodStartInfimum;
            var transientIsFinite =  ti > 0 && CutAsEnumerable(0, ti).IsFinite() || true;
            var periodIsPlainlyInfinite =
                ValueAt(ti).IsFinite
                    ? (CutAsEnumerable(ti, SecondPseudoPeriodEnd, false).IsMinusInfinite() ||
                       CutAsEnumerable(ti, SecondPseudoPeriodEnd, false).IsPlusInfinite())
                    : (CutAsEnumerable(ti, SecondPseudoPeriodEnd).IsMinusInfinite() ||
                       CutAsEnumerable(ti, SecondPseudoPeriodEnd).IsPlusInfinite());

            return transientIsFinite && periodIsPlainlyInfinite;
        }
    }

    /// <summary>
    /// True if for all $t \ge$ <see cref="PseudoPeriodStart"/> the curve is affine.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyAffine =>
        IsUltimatelyFinite
        && PseudoPeriodicElements.Count() == 2
        && ((Segment)PseudoPeriodicElements.Last()).Slope == PseudoPeriodSlope
        && IsContinuousAt(FirstPseudoPeriodEnd);

    /// <summary>
    /// True if for $t \ge$ <see cref="PseudoPeriodStart"/> the curve is constant.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsUltimatelyConstant =>
        IsUltimatelyAffine && PseudoPeriodSlope == 0;

    /// <summary>
    /// True if the curve is sub-additive, i.e. $f(t+s) \le f(t) + f(s)$.
    /// </summary>
    /// <remarks>
    /// Based on [Zippo23] Lemma 9.3: $f(0) \ge 0, f$ is sub-additive $\iff f^\circ = f^\circ \otimes f^\circ$,
    /// where $f^\circ$ is defined in <see cref="Curve.WithZeroOrigin"/>.
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
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
    internal bool? _IsSubAdditive;

    /// <summary>
    /// True if the curve is sub-additive with $f(0) = 0$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRegularSubAdditive
        => IsSubAdditive && IsPassingThroughOrigin;

    /// <summary>
    /// True if the curve is super-additive, i.e. $f(t+s) \ge f(t) + f(s)$.
    /// </summary>
    /// <remarks>
    /// Based on [Zippo23] Lemma 9.4: $f$ is super-additive $\iff f^\circ = f^\circ \overline{\otimes} f^\circ$,
    /// where $f^\circ$ is defined in <see cref="Curve.WithZeroOrigin"/>.
    /// Can be computationally expensive the first time it is invoked, the result is cached afterwards.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
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
    internal bool? _IsSuperAdditive;

    /// <summary>
    /// True if the curve is super-additive with $f(0) = 0$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRegularSuperAdditive
        => IsSuperAdditive && IsPassingThroughOrigin;


    /// <summary>
    /// Tests if the curve is concave,
    /// i.e. for any two points $(t, f(t))$ the straight line joining them is below $f$.
    /// </summary>
    /// <remarks>
    /// The property is checked via the following property: $f$ is concave $\iff$
    /// a) $f$ is continuous, or it is continuous for $t > 0$ and $f(0) \le f(0^+)$, and
    /// b) $f$ is composed of segments with decreasing slopes.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRegularConcave
        => IsConcave && IsPassingThroughOrigin;

    /// <summary>
    /// Tests if the curve is convex,
    /// i.e. for any two points $(t, f(t))$ the straight line joining them is above $f$.
    /// </summary>
    /// <remarks>
    /// The property is checked via the following property: $f$ is convex $\iff$
    /// a) $f$ is continuous, or it is continuous for $t > 0$ and $f(0) \ge f(0^+)$, and
    /// b) $f$ is composed of segments with increasing slopes.
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRegularConvex
        => IsConvex && IsPassingThroughOrigin;

    /// <summary>
    /// True if pseudo-periodic behavior starts at $T > 0$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool HasTransient =>
        PseudoPeriodStart > 0;

    /// <summary>
    /// Sequence describing the curve in $[0, T[$, before pseudo-periodic behavior.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Sequence? TransientSequence
        => HasTransient ? BaseSequence.Cut(0, PseudoPeriodStart) : null;

    /// <summary>
    /// Elements describing the curve from $[0, T[$, before pseudo-periodic behavior.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_1, ..., t_{i_0 - 1}]$ in [BT08] Section 4.1
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
    public IEnumerable<Element> TransientElements
        => TransientSequence?.Elements ?? Enumerable.Empty<Element>();

    /// <summary>
    /// Sequence describing the pseudo-periodic behavior of the curve in $[T, T + d[$.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Sequence PseudoPeriodicSequence =>
        BaseSequence.Cut(PseudoPeriodStart, FirstPseudoPeriodEnd);

    /// <summary>
    /// Elements describing the pseudo-periodic behavior of the curve in $[T, T + d[$.
    /// </summary>
    /// <remarks>
    /// Referred to as $[t_{i_0}, ..., t_k]$ in [BT08] Section 4.1
    /// </remarks>
    [System.Text.Json.Serialization.JsonIgnore]
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
    [System.Text.Json.Serialization.JsonConstructor]
    public Curve(
        Sequence baseSequence,
        Rational pseudoPeriodStart,
        Rational pseudoPeriodLength,
        Rational pseudoPeriodHeight
    )
    {
        if (baseSequence.DefinedFrom != 0 || baseSequence.DefinedUntil != pseudoPeriodStart + pseudoPeriodLength)
        {
            throw new ArgumentException("Base sequence must start at t = 0 and end at T + d");
        }
        else if (
            pseudoPeriodHeight.IsInfinite &&
            baseSequence
                .CutAsEnumerable(pseudoPeriodStart, pseudoPeriodStart + pseudoPeriodLength)
                .Any(e => e.IsFinite)
        )
        {
            // non-conforming infinite curve
            pseudoPeriodStart = baseSequence.DefinedUntil;
            baseSequence = baseSequence.Elements
                .Fill(0, pseudoPeriodStart + pseudoPeriodLength, fillWith: pseudoPeriodHeight)
                .ToSequence();
        }

        BaseSequence = baseSequence
            .Optimize()
            .Cut(cutStart: 0, cutEnd: pseudoPeriodStart + pseudoPeriodLength, isStartIncluded: true, isEndIncluded: false)
            .EnforceSplitAt(pseudoPeriodStart);
        PseudoPeriodStart = pseudoPeriodStart;
        PseudoPeriodLength = pseudoPeriodLength;
        PseudoPeriodHeight = pseudoPeriodHeight;
    }

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
        bool isPartialCurve
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
        else if (
            pseudoPeriodHeight.IsInfinite &&
            baseSequence
                .CutAsEnumerable(pseudoPeriodStart, pseudoPeriodStart + pseudoPeriodLength)
                .Any(e => e.IsFinite)
        )
        {
            // non-conforming infinite curve
            pseudoPeriodStart = baseSequence.DefinedUntil;
            baseSequence = baseSequence.Elements
                .Fill(0, pseudoPeriodStart + pseudoPeriodLength, fillWith: pseudoPeriodHeight)
                .ToSequence();
        }

        BaseSequence = baseSequence
            .Optimize()
            .Cut(cutStart: 0, cutEnd: pseudoPeriodStart + pseudoPeriodLength, isStartIncluded: true, isEndIncluded: false)
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

        return (BaseSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight) ==
               (curve.BaseSequence, curve.PseudoPeriodStart, curve.PseudoPeriodLength, curve.PseudoPeriodHeight);
    }

    /// <inheritdoc cref="object.GetHashCode"/>
    public override int GetHashCode()
        => (BaseSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight).GetHashCode();

    /// <summary>
    /// A stable hashcode.
    /// </summary>
    public int GetStableHashCode()
        => (BaseSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight).GetStableHashCode();

    /// <summary>
    /// Returns <c>true</c> if its operands are equal, <c>false</c> otherwise
    /// </summary>
    public static bool operator ==(Curve? a, Curve? b) =>
        Equals(a, b);

    /// <summary>
    /// Returns <c>false</c> if its operands are equal, <c>true</c> otherwise
    /// </summary>
    public static bool operator !=(Curve? a, Curve? b) =>
        !Equals(a, b);

    /// <summary>
    /// True if the curves represent the same function.
    /// </summary>
    public bool Equivalent(Curve curve, ComputationSettings? settings = null)
        => Equivalent(this, curve, settings);

    /// <summary>
    /// True if the curves represent the same function.
    /// </summary>
    public static bool Equivalent(Curve a, Curve b, ComputationSettings? settings = null)
    {
        var cutEnd = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart)
                            + Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);

        var seqA = a.CutAsEnumerable(0, cutEnd, true, true, settings);
        var seqB = b.CutAsEnumerable(0, cutEnd, true, true, settings);

        return Sequence.Equivalent(seqA, seqB);
    }

    /// <summary>
    /// True if all the curves in the set represent the same function.
    /// </summary>
    /// <param name="curves"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool Equivalent(IEnumerable<Curve> curves, ComputationSettings? settings = null)
    {
        using var enumerator = curves.GetEnumerator();
        if (!enumerator.MoveNext())
            throw new ArgumentException("The set of curves is empty");

        var reference = enumerator.Current;
        while (enumerator.MoveNext())
        {
            var result = Curve.Equivalent(reference, enumerator.Current, settings);
            if (!result)
                return false;
        }

        return true;
    }

    /// <summary>
    /// True if, starting from <paramref name="time"/>, the curves represent the same function.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="time"></param>
    /// <param name="isStartIncluded">If true, <paramref name="time"/> is included in the comparison.</param>
    /// <param name="settings"></param>
    public static bool EquivalentAfter(Curve a, Curve b, Rational time, bool isStartIncluded = true, ComputationSettings? settings = null)
    {
        var cutEnd = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart, time)
                            + Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);

        var seqA = a.CutAsEnumerable(time, cutEnd, isStartIncluded, true, settings);
        var seqB = b.CutAsEnumerable(time, cutEnd, isStartIncluded, true, settings);

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
    /// <param name="settings"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static Rational? FindFirstInequivalence(Curve a, Curve b, ComputationSettings? settings = null)
        => FindFirstInequivalenceAfter(a, b, 0, true, settings);

    //todo: write tests
    /// <summary>
    /// Returns the first time around which the functions represented by the curves differ.
    /// Returns null if the two curves represent the same function.
    /// Mostly useful to debug curves that *should* be equivalent.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="time"></param>
    /// <param name="isStartIncluded">If true, <paramref name="time"/> is included in the comparison.</param>
    /// <param name="settings"></param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static Rational? FindFirstInequivalenceAfter(Curve a, Curve b, Rational time, bool isStartIncluded = true, ComputationSettings? settings = null)
    {
        var cutEnd = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart, time)
                            + Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);

        var seqA = a.CutAsEnumerable(time, cutEnd, isStartIncluded, true, settings);
        var seqB = b.CutAsEnumerable(time, cutEnd, isStartIncluded, true, settings);

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
    public bool EquivalentExceptOrigin(Curve curve, ComputationSettings? settings = null)
        => EquivalentExceptOrigin(this, curve, settings);

    /// <summary>
    /// True if the curves represent the same function, except for origin.
    /// </summary>
    public static bool EquivalentExceptOrigin(Curve a, Curve b, ComputationSettings? settings = null)
    {
        var cutEnd = Rational.Max(a.PseudoPeriodStart, b.PseudoPeriodStart)
                          + Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);

        var seqA = a.CutAsEnumerable(0, cutEnd, false, true, settings);
        var seqB = b.CutAsEnumerable(0, cutEnd, false, true, settings);

        return Sequence.Equivalent(seqA, seqB);
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
        if (a.PseudoPeriodSlope < b.PseudoPeriodSlope)
            return (true, a, b);
        else if (b.PseudoPeriodSlope < a.PseudoPeriodSlope)
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
        if (IsZero)
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
        if (time < 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        Element element = GetElementAt(time, settings);
        return element.ValueAt(time);
    }

    /// <summary>
    /// Computes the right limit of the curve at given time $t$.
    /// </summary>
    /// <param name="time">The target time of the limit.</param>
    /// <returns>The value of $f(t^+)$.</returns>
    public Rational RightLimitAt(Rational time)
    {
        if (time < 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        var segment = GetSegmentAfter(time, false);
        return (segment.StartTime == time) ?
            segment.RightLimitAtStartTime
            : segment.ValueAt(time);
    }

    /// <summary>
    /// Computes the left limit of the curve at given time $t$.
    /// </summary>
    /// <param name="time">The target time of the limit.</param>
    /// <returns>The value of $f(t^-)$.</returns>
    /// <exception cref="ArgumentException">The argument is 0, as a curve is not defined for $t &lt; 0$.</exception>
    public Rational LeftLimitAt(Rational time)
    {
        if (time == 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        Segment segment = GetSegmentBefore(time);
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
    public Element GetElementAt(Rational time, ComputationSettings? settings = null)
    {
        if (time < 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        if (time < FirstPseudoPeriodEnd)
            return BaseSequence.GetElementAt(time);

        //otherwise
        return GetExtensionSequenceAt(time, settings).GetElementAt(time);
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the curve before time t.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <exception cref="ArgumentException">Thrown if time 0 is given, as a curve is not defined before 0.</exception>
    /// <returns>The <see cref="Segment"/> describing the curve before time t, or null if there is none.</returns>
    public Segment GetSegmentBefore(Rational time)
    {
        if (time == 0)
            throw new ArgumentException("A curve is not defined for t < 0");

        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        if (time <= FirstPseudoPeriodEnd)
            return BaseSequence.GetSegmentBefore(time);

        //otherwise
        return GetExtensionSequenceBefore(time).GetSegmentBefore(time);
    }

    /// <summary>
    /// Returns the <see cref="Segment"/> that describes the curve after time $t$.
    /// </summary>
    /// <param name="time">Time t of the sample.</param>
    /// <param name="autoMerge">
    /// If true, it seeks for possible merges to return the longest finite-length segment, i.e.,
    /// such that either $f$ is not differentiable at its end time or the segment length extends to $+\infty$.
    /// </param>
    /// <returns>The <see cref="Segment"/> describing the curve after time t.</returns>
    public Segment GetSegmentAfter(Rational time, bool autoMerge = true)
    {
        if (time.IsPlusInfinite)
            throw new ArgumentException("Cannot sample a curve at t = +infinity");

        if (!autoMerge)
        {
            if (time < FirstPseudoPeriodEnd)
                return BaseSequence.GetSegmentAfter(time);
            else
                return GetExtensionSequenceAt(time).GetSegmentAfter(time);
        }
        else
        {
            var segment = (time < FirstPseudoPeriodEnd)
                ? BaseSequence.GetSegmentAfter(time)
                : GetExtensionSequenceAt(time).GetSegmentAfter(time);
            var t = segment.EndTime;

            // avoid infinite extensions
            if (t >= PseudoPeriodStart && (IsUltimatelyAffine || IsUltimatelyInfinite))
                return segment;

            // try merging with the next segment, until a non-differentiable time is reached
            bool try_merge;
            do
            {
                try_merge = false;
                if (IsContinuousAt(t))
                {
                    var nextSegment = GetSegmentAfter(t, false);
                    if (nextSegment.Slope == segment.Slope)
                    {
                        t = nextSegment.EndTime;
                        try_merge = true;
                    }
                }
            } while (try_merge);

            if (t > segment.EndTime)
                // reconstruct and return the merged segment
                return new Segment(segment.StartTime, t, segment.RightLimitAtStartTime, segment.Slope);
            else
                return segment;
        }
    }

    /// <summary>
    /// True if the given element matches, in its support, with the curve.
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
    /// True if the given sequence matches, in its interval, with the curve.
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
    /// Returns a cut of the curve limited to the given interval.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the interval.</param>
    /// <param name="cutEnd">Right endpoint of the interval.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed.</param>
    /// <param name="settings"></param>
    /// <returns>A list of elements equivalently defined within the given interval.</returns>
    /// <remarks>Optimized for minimal allocations.</remarks>
    public IEnumerable<Element> CutAsEnumerable(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
        ComputationSettings? settings = null
    )
    {
        settings ??= ComputationSettings.Default();

        if (cutEnd > BaseSequence.DefinedUntil || (isEndIncluded && cutEnd == BaseSequence.DefinedUntil))
        {
            if (cutStart > BaseSequence.DefinedUntil)
            {
                var startingPseudoPeriodIndex = ((cutStart - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                if (isEndIncluded)
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

                return elements.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
            }
            else
            {
                var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                if (isEndIncluded)
                    endingPseudoPeriodIndex++;

                var indexes = settings.UseParallelExtend
                    ? Enumerable.Range(1, endingPseudoPeriodIndex).AsParallel()
                    : Enumerable.Range(1, endingPseudoPeriodIndex);

                var extensionElements = indexes
                    .Select(i => ComputeExtensionSequenceAsEnumerable(i, settings))
                    .SelectMany(en => en);

                var elements = BaseSequence.Elements.Concat(extensionElements);

                return elements.Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
            }
        }
        else
        {
            return BaseSequence
                .CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        }
    }

    /// <summary>
    /// Returns a cut of the curve limited to the given interval.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the interval.</param>
    /// <param name="cutEnd">Right endpoint of the interval.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed.</param>
    /// <param name="settings"></param>
    /// <returns>A sequence equivalently defined within the given interval.</returns>
    public Sequence Cut(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
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

        Sequence result;
        if (cutEnd > BaseSequence.DefinedUntil || (isEndIncluded && cutEnd == BaseSequence.DefinedUntil))
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
                    if(isStartIncluded)
                        elements.Add(new Point(cutStart, valueAtStart));
                    elements.Add(new Segment(cutStart, cutEnd, valueAtStart, bsLastSegment.Slope));
                    if (isEndIncluded)
                    {
                        var valueAtEnd = pseudoPeriodStartValue +
                                       (cutEnd - bsLastSegment.StartTime) * bsLastSegment.Slope;
                        elements.Add(new Point(cutEnd, valueAtEnd));
                    }

                    result = elements
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

                    if (isEndIncluded)
                    {
                        var lastPoint = new Point(cutEnd, lastSegment.LeftLimitAtEndTime);
                        elements = elements.Append(lastPoint);
                    }

                    result = elements
                        .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded)
                        .ToSequence();
                }
            }
            else
            {
                if (cutStart > BaseSequence.DefinedUntil)
                {
                    var startingPseudoPeriodIndex = ((cutStart - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    if (isEndIncluded)
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

                    result = elements
                        .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded)
                        .ToSequence();
                }
                else
                {
                    var endingPseudoPeriodIndex = ((cutEnd - PseudoPeriodStart) / PseudoPeriodLength).FastFloor();
                    if (isEndIncluded)
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

                    result = elements
                        .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded)
                        .ToSequence();
                }
            }
        }
        else
        {
            result = BaseSequence
                .Cut(cutStart, cutEnd, isStartIncluded, isEndIncluded);
        }

        if(_isNonDecreasing is not null)
            result._isNonDecreasing = _isNonDecreasing;
        if(_isLeftContinuous is not null)
            result._isLeftContinuous = _isLeftContinuous;
        if(_isRightContinuous is not null)
            result._isRightContinuous = _isRightContinuous;

        return result;
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
    /// which follows, minus the restrictions, the definition in [BT08] Section 4.1
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
    /// Returns the number of elements of a cut of the curve limited to the given interval.
    /// </summary>
    /// <param name="cutStart">Left endpoint of the interval.</param>
    /// <param name="cutEnd">Right endpoint of the interval.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed.</param>
    /// <returns>The number of elements of the sequence equivalently defined within the given interval.</returns>
    public int Count(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        return CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded).Count();
    }

    /// <summary>
    /// Computes the first time the curve is at or above given <paramref name="value"/>,
    /// i.e., $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right \}$
    /// </summary>
    /// <param name="value">The value to reach.</param>
    /// <returns>The first time t at which $f(t)$ = value, or $+\infty$ if it is never reached.</returns>
    /// <remarks>
    /// The current implementation uses <see cref="ToUpperNonDecreasing"/> and <see cref="LowerPseudoInverse"/>.
    /// Thus it is useful as a shortcut but not to optimize computation of $f^{-1}_\downarrow(x)$ for a single point.
    /// </remarks>
    public Rational TimeAt(Rational value)
    {
        return this
            .ToUpperNonDecreasing()
            .LowerPseudoInverse()
            .ValueAt(value);
    }

    /// <summary>
    /// Splits the curve into two, $f_t$ and $f_p$,
    /// so that $f = f_t \wedge f_p$ (if <paramref name="minDecomposition"/> is true)
    /// or $f = f_t \vee f_p$ (if <paramref name="minDecomposition"/> is false).
    /// </summary>
    /// <param name="splitTime">Time at which to split the curve. Defaults to <see cref="PseudoPeriodStart"/>.</param>
    /// <param name="leftIncludesEndPoint">
    /// If true, and <paramref name="splitTime"/> is $T > 0$, the support of $f_t$ will be $[0, T]$.
    /// If false, it will be $[0, T[$.
    /// </param>
    /// <param name="minDecomposition">
    /// If true (default), the parts have value $+\infty$ outside their support, i.e., they can be recomposed by computing their minimum.
    /// Conversely, if false the parts have value $-\infty$ outside their support, i.e., they can be recomposed by computing their maximum.
    /// </param>
    /// <exception cref="ArgumentException">If <paramref name="splitTime"/> is $\le 0$.</exception>
    /// <returns>
    /// A tuple containing the two parts.
    /// If <paramref name="splitTime"/> is 0, the left part will be null.
    /// </returns>
    public (Curve? left, Curve right) Decompose(Rational? splitTime = null, bool leftIncludesEndPoint = false, bool minDecomposition = true)
    {
        var _splitTime = splitTime ?? PseudoPeriodStart;

        Curve? left = null;
        if (_splitTime > 0)
        {
            var cut = Cut(0, _splitTime, isEndIncluded: leftIncludesEndPoint);
            left = minDecomposition ? Minimum(PlusInfinite(), cut) : Maximum(MinusInfinite(), cut);
        }

        if (_splitTime >= PseudoPeriodStart)
        {
            var start_seq = _splitTime == 0 ?
                    new List<Element> {}
                    : minDecomposition
                    ? new List<Element> {Point.PlusInfinite(0), Segment.PlusInfinite(0, _splitTime)}
                    : new List<Element> {Point.MinusInfinite(0), Segment.MinusInfinite(0, _splitTime)};
            var right = new Curve(
                baseSequence: start_seq
                    .Concat(CutAsEnumerable(_splitTime, _splitTime + PseudoPeriodLength))
                    .ToSequence(),
                pseudoPeriodStart: _splitTime,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
            return (left, right);
        }
        else
        {
            var start_seq = _splitTime == 0 ?
                new List<Element> {}
                : minDecomposition
                ? new List<Element> {Point.PlusInfinite(0), Segment.PlusInfinite(0, _splitTime)}
                : new List<Element> {Point.MinusInfinite(0), Segment.MinusInfinite(0, _splitTime)};
            var right = new Curve(
                baseSequence: start_seq
                    .Concat(CutAsEnumerable(_splitTime, PseudoPeriodStart + PseudoPeriodLength))
                    .ToSequence(),
                pseudoPeriodStart: PseudoPeriodStart,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
            return (left, right);
        }
    }

    #endregion Methods

    #region Json Methods

    /// <summary>
    /// Returns string serialization in Json format.
    /// </summary>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, new GenericCurveNewtonsoftJsonConverter());
    }

    /// <summary>
    /// Deserializes a Curve.
    /// </summary>
    public static Curve FromJson(string json)
    {
        var curve = JsonConvert.DeserializeObject<Curve>(json, new GenericCurveNewtonsoftJsonConverter());
        if (curve == null)
            throw new InvalidOperationException("Invalid JSON format.");
        return curve;
    }

    /// <summary>
    /// Returns a string containing C# code to create this Curve.
    /// Useful to copy and paste from a debugger into another test or notebook for further investigation.
    /// </summary>
    public virtual string ToCodeString(bool formatted = false, int indentation = 0)
    {
        var newline = formatted ? "\n" : "";
        var space = formatted ? "\n" : " ";

        var sb = new StringBuilder();
        sb.Append($"{tabs(0)}new Curve({newline}");
        sb.Append($"{tabs(1)}baseSequence: {BaseSequence.ToCodeString(formatted, 1)},{space}");
        sb.Append($"{tabs(1)}pseudoPeriodStart: {PseudoPeriodStart.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}pseudoPeriodLength: {PseudoPeriodLength.ToCodeString()},{space}");
        sb.Append($"{tabs(1)}pseudoPeriodHeight: {PseudoPeriodHeight.ToCodeString()}{newline}");
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
    /// Scales the curve by a multiplicative factor, i.e. $g(t) = k \cdot f(t)$.
    /// </summary>
    public virtual Curve Scale(Rational scaling)
    {
        return new Curve(
            baseSequence: BaseSequence * scaling,
            pseudoPeriodStart: PseudoPeriodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodLength * PseudoPeriodSlope * scaling
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
    /// Computes $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="delay"/> is either negative or infinite.</exception>
    /// <seealso cref="ForwardBy(Rational)"/>
    /// <seealso cref="HorizontalShift(Rational)"/>
    public virtual Curve DelayBy(Rational delay)
    {
        if (delay == 0)
            return this;

        if (delay < 0)
            throw new ArgumentException("Delay must be >= 0");

        if (delay.IsInfinite)
            throw new ArgumentException("Delay must be finite.");

        return new Curve(
            baseSequence: BaseSequence.Delay(delay),
            pseudoPeriodStart: PseudoPeriodStart + delay,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodHeight
        );
    }

    /// <summary>
    /// Brings forward the curve, removing the parts from 0 to the given time.
    /// Computes $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="time"/> is either negative or infinite.</exception>
    /// <seealso cref="DelayBy(Rational)"/>
    /// <seealso cref="HorizontalShift(Rational)"/>
    public virtual Curve ForwardBy(Rational time)
    {
        if (time == 0)
            return this;

        if (time < 0)
            throw new ArgumentException("Time must be >= 0");

        if (time.IsInfinite)
            throw new ArgumentException("Time to forward by must be finite.");

        if (time <= PseudoPeriodStart)
        {
            return new Curve(
                baseSequence: BaseSequence.Forward(time),
                pseudoPeriodStart: PseudoPeriodStart - time,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
        else
        {
            return new Curve(
                baseSequence: Cut(0, time + PseudoPeriodLength).Forward(time),
                pseudoPeriodStart: 0,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
    }

    /// <summary>
    /// Shifts the curve horizontally to the right, i.e. $g(t) = f(t - T)$.
    /// If $T \ge 0$, it behaves like <see cref="DelayBy(Rational)"/>.
    /// If $T &lt; 0$, it behaves like <see cref="ForwardBy(Rational)"/>.
    /// </summary>
    /// <exception cref="ArgumentException">If <paramref name="shift"/> is infinite.</exception>
    /// <seealso cref="DelayBy(Rational)"/>
    /// <seealso cref="ForwardBy(Rational)"/>
    public virtual Curve HorizontalShift(Rational shift)
    {
        if (shift == 0)
            return this;
        else if (shift >= 0)
            return DelayBy(shift);
        else
            return ForwardBy(-shift);
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

        if (exceptOrigin && IsZero)
            return new ConstantCurve(shift);

        if (exceptOrigin && PseudoPeriodStart == 0)
        {
            // must skip first period
            return new Curve(
                baseSequence: Cut(0, SecondPseudoPeriodEnd)
                    .VerticalShift(shift, exceptOrigin),
                pseudoPeriodStart: FirstPseudoPeriodEnd,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
        else
        {
            return new Curve(
                baseSequence: BaseSequence.VerticalShift(shift, exceptOrigin),
                pseudoPeriodStart: PseudoPeriodStart,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
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
    /// Computes the _upper non-decreasing closure_ of this curve,
    /// i.e., the lowest curve $g(t) \ge f(t)$ so that $g(t + s) \ge g(t)$ for any $t, s \ge 0$.
    /// </summary>
    /// <remarks>
    /// This implements the _upper non-decreasing closure_ defined in [DNC18] p. 45, although the implementation differs.
    /// </remarks>
    public Curve ToUpperNonDecreasing()
    {
        if (IsNonDecreasing)
            return this;

        // the following implementation is more efficient than the definition in [DNC18] p. 45, which uses the max-plus convolution,
        // since here we add terms to the global maximum only if there actually is a decrease.

        // this list will contain the curve to transform plus,
        // for each breakpoint at which a decrease happens, a constant segment with the sup value at the breakpoint and $-\infty$ before it.
        List<Curve> curves = new (){ this };

        if (HasTransient)
        {
            // all elements in [0, T]
            var transient = CutAsEnumerable(0, PseudoPeriodStart, isEndIncluded: true);
            foreach (var (left, center, right) in transient.EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointSupValue();
                    curves.Add(GetLowerboundCurve(time, value));
                }
            }
        }

        if(PseudoPeriodSlope > 0)
        {
            foreach (var (left, center, right) in
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndIncluded: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointSupValue();
                    curves.Add(GetPeriodicLowerboundCurve(time, value));
                }
            }
        }
        else
        {
            foreach (var (left, center, right) in
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndIncluded: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    right is not null && right.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointSupValue();
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
    /// Computes the _lower non-decreasing closure_ of this curve,
    /// i.e., the highest curve $g(t) \le f(t)$ so that $g(t + s) \ge g(t)$ for any $t, s \ge 0$.
    /// </summary>
    public Curve ToLowerNonDecreasing()
    {
        if (IsNonDecreasing)
            return this;

        // this list will contain the curve to transform plus,
        // for each breakpoint at which a decrease ends, a constant segment with the inf value at the breakpoint and $+\infty$ after it.
        List<Curve> curves = new (){ this };

        if (HasTransient)
        {
            // all elements in [0, T]
            var transient = CutAsEnumerable(0, PseudoPeriodStart, isEndIncluded: true);
            foreach (var (left, center, right) in transient.EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    left is not null && left.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointInfValue();
                    curves.Add(GetUpperboundCurve(time, value));
                }
            }
        }

        if(PseudoPeriodSlope > 0)
        {
            foreach (var (left, center, right) in
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndIncluded: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    left is not null && left.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointInfValue();
                    curves.Add(GetPeriodicUpperboundCurve(time, value));
                }
            }
        }
        else
        {
            foreach (var (left, center, right) in
                     Cut(PseudoPeriodStart, FirstPseudoPeriodEnd, isEndIncluded: true).EnumerateBreakpoints())
            {
                if (
                    left is not null && left.LeftLimitAtEndTime > center.Value ||
                    right is not null && center.Value > right.RightLimitAtStartTime ||
                    left is not null && left.Slope < 0
                )
                {
                    var time = center.Time;
                    var value = (left, center, right).BreakpointInfValue();
                    curves.Add(GetUpperboundCurve(time, value));
                }
            }
        }

        return Minimum(curves);

        Curve GetUpperboundCurve(Rational time, Rational value)
        {
            List<Element> elements;
            if (time > 0)
            {
                elements = new()
                {
                    new Point(0, value),
                    Segment.Constant(0, time, value),
                    new Point(time, value),
                    Segment.PlusInfinite(time, time + 2)
                };
            }
            else
            {
                elements = new()
                {
                    new Point(time, value),
                    Segment.PlusInfinite(time, time + 2)
                };
            }

            return new Curve(
                baseSequence: elements.ToSequence(),
                pseudoPeriodStart: time + 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }

        Curve GetPeriodicUpperboundCurve(Rational time, Rational value)
        {
            List<Element> elements;
            if (time > 0)
            {
                elements = new()
                {
                    new Point(0, value),
                    Segment.Constant(0, time, value),
                    new Point(time, value),
                    new Segment(time, time + PseudoPeriodLength, value + PseudoPeriodHeight, 0)
                };
            }
            else
            {
                elements = new()
                {
                    new Point(time, value),
                    new Segment(time, time + PseudoPeriodLength, value + PseudoPeriodHeight, 0)
                };
            }

            return new Curve(
                baseSequence: elements.ToSequence(),
                pseudoPeriodStart: time,
                pseudoPeriodLength: PseudoPeriodLength,
                pseudoPeriodHeight: PseudoPeriodHeight
            );
        }
    }

    /// <summary>
    /// Computes a left-continuous version of this curve.
    /// </summary>
    public Curve ToLeftContinuous()
    {
        var sequence = CutAsEnumerable(0, SecondPseudoPeriodEnd)
            .ToLeftContinuous()
            .ToSequence();

        return new Curve(
            sequence,
            FirstPseudoPeriodEnd,
            PseudoPeriodLength,
            PseudoPeriodHeight
        ).TransientReduction();
    }

    /// <summary>
    /// Computes a right-continuous version of this curve.
    /// </summary>
    public Curve ToRightContinuous()
    {
        var sequence = CutAsEnumerable(0, SecondPseudoPeriodEnd)
            .ToRightContinuous()
            .ToSequence();

        return new Curve(
            sequence,
            FirstPseudoPeriodEnd,
            PseudoPeriodLength,
            PseudoPeriodHeight
        ).TransientReduction();
    }

    /// <summary>
    /// Computes $f^\circ = \min \left( f, \delta_0 \right)$.
    /// If $f(0) > 0$, this enforces $f(0) = 0$.
    /// If $f(0) \le 0$, this does nothing.
    /// </summary>
    public Curve WithZeroOrigin()
    {
        if (ValueAt(0) <= 0)
        {
            return this;
        }
        else
        {
            return Minimum(this, new DelayServiceCurve(0));
        }
    }

    /// <summary>
    /// Enforces $f(0) = v$.
    /// </summary>
    /// <param name="value">The value $v$ to be enforced.</param>
    public Curve WithOriginAt(Rational value)
    {
        if (ValueAt(0) == value)
            return this;
        else
        {
            if (PseudoPeriodStart == 0)
            {
                var newSequence = CutAsEnumerable(0, SecondPseudoPeriodEnd)
                    .Skip(1)
                    .Prepend(new Point(0, value))
                    .ToSequence();
                return new Curve(newSequence, FirstPseudoPeriodEnd, PseudoPeriodLength, PseudoPeriodHeight);
            }
            else
            {
                var newSequence = BaseSequence.Elements
                    .Skip(1)
                    .Prepend(new Point(0, value))
                    .ToSequence();
                return new Curve(newSequence, PseudoPeriodStart, PseudoPeriodLength, PseudoPeriodHeight);
            }
        }
    }

    /// <summary>
    /// Enforces $f(0) = f(0^+)$, i.e. right-continuity at $0$.
    /// </summary>
    public Curve WithOriginRightContinuous()
    {
        return WithOriginAt(RightLimitAt(0));
    }

    /// <summary>
    /// Computes the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is left-continuous, thus is revertible, i.e. $\left(f^{-1}_\downarrow\right)^{-1}_\downarrow = f$, only if $f$ is left-continuous, see [DNC18] § 3.2.1 .
    /// Algorithmic properties discussed in [ZNS23b].
    /// </remarks>
    public Curve LowerPseudoInverse()
    {
        if (!IsNonDecreasing)
            throw new ArgumentException("The lower pseudo-inverse is defined only for non-decreasing functions");

        if (IsUltimatelyConstant)
        {
            var constant_start = PseudoPeriodStartInfimum;
            var constant_value = ValueAt(PseudoPeriodStart);
            var transient_lpi = CutAsEnumerable(0, constant_start, isEndIncluded: true).LowerPseudoInverse();
            var lpi = IsRightContinuousAt(constant_start)
                ? transient_lpi
                    .Append(Segment.PlusInfinite(constant_value, constant_value + 2))
                : transient_lpi
                    .Append(Segment.Constant(ValueAt(constant_start), constant_value, constant_start))
                    .Append(new Point(constant_value, constant_start))
                    .Append(Segment.PlusInfinite(constant_value, constant_value + 2));

            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: constant_value + 1,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (IsUltimatelyPlusInfinite)
        {
            var lastFiniteTime = PseudoPeriodStartInfimum; // T_I
            if (lastFiniteTime == 0)
                return Zero();
            var valueAtLastFiniteTime = ValueAt(lastFiniteTime); // f(T_I)
            var lastFiniteValue = valueAtLastFiniteTime.IsFinite ? valueAtLastFiniteTime :
                lastFiniteTime > 0 ? LeftLimitAt(lastFiniteTime) : 0; // L
            var isLastFiniteConstant = GetSegmentBefore(lastFiniteTime).IsConstant;
            var transient_lpi = BaseSequence.CutAsEnumerable(0, lastFiniteTime).LowerPseudoInverse();
            var lpi = isLastFiniteConstant
                ? transient_lpi
                    .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 2, lastFiniteTime))
                : transient_lpi
                    .Append(new Point(lastFiniteValue, lastFiniteTime))
                    .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

            return new Curve(
                baseSequence: lpi.ToSequence(),
                pseudoPeriodStart: isLastFiniteConstant ? lastFiniteValue + 1 : lastFiniteValue,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (!IsNonNegative)
        {
            var T = Rational.Max(FirstPseudoPeriodEnd, FirstNonNegativeTime);
            if (ValueAt(T) < 0)
                T += PseudoPeriodLength;
            var sequence = CutAsEnumerable(0, T + PseudoPeriodLength, isEndIncluded: true)
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
            // the point at the right endpoint is included in case there is a left-discontinuity at the end of the pseudo-period
            // the pseudo-inverse of said point is then removed from the result
            var sequence = CutAsEnumerable(0, SecondPseudoPeriodEnd, isEndIncluded: true)
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

    // todo: add reference about $f(t) > 0$ for $t > 0$

    /// <summary>
    /// Computes the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing.</exception>
    /// <remarks>
    /// The result of this operation is right-continuous.
    /// If $f$ is right-continuous and $f(t) > 0$ for $t > 0$, then the operation is revertible,
    /// i.e., $\left(f^{-1}_\uparrow\right)^{-1}_\uparrow = f$, see [DNC18] § 3.2.1.
    /// Algorithmic properties discussed in [ZNS23b].
    /// </remarks>
    public Curve UpperPseudoInverse()
    {
        if (!IsNonDecreasing)
            throw new ArgumentException("The upper pseudo-inverse is defined only for non-decreasing functions");

        if (IsUltimatelyConstant)
        {
            var constant_start = PseudoPeriodStartInfimum;
            var constant_value = ValueAt(PseudoPeriodStart);
            var transient_upi = constant_start > 0
                ? CutAsEnumerable(0, constant_start).UpperPseudoInverse()
                : Enumerable.Empty<Element>();
            var upi = IsRightContinuousAt(constant_start)
                ? transient_upi
                    .Append(Point.PlusInfinite(constant_value))
                    .Append(Segment.PlusInfinite(constant_value, constant_value + 1))
                : transient_upi
                    .Append(new Point(ValueAt(constant_start), constant_start))
                    .Append(Segment.Constant(ValueAt(constant_start), constant_value, constant_start))
                    .Append(Point.PlusInfinite(constant_value))
                    .Append(Segment.PlusInfinite(constant_value, constant_value + 1));

            var valueAtZero = ValueAt(0);
            var sequence = valueAtZero > 0
                ? Sequence.MinusInfinite(0, valueAtZero).Elements
                    .Concat(upi)
                    .ToSequence()
                : upi.ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: constant_value,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (IsUltimatelyPlusInfinite)
        {
            var lastFiniteTime = PseudoPeriodStartInfimum; // T_I
            if (lastFiniteTime == 0)
                return Zero();
            var valueAtLastFiniteTime = ValueAt(lastFiniteTime); // f(T_I)
            var lastFiniteValue = valueAtLastFiniteTime.IsFinite ? valueAtLastFiniteTime :
                lastFiniteTime > 0 ? LeftLimitAt(lastFiniteTime) : 0; // L
            var isLastFiniteConstant = GetSegmentBefore(lastFiniteTime).IsConstant;
            var transient_upi = BaseSequence.CutAsEnumerable(0, lastFiniteTime).UpperPseudoInverse();
            var upi = isLastFiniteConstant
                ? transient_upi
                    .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime))
                : transient_upi
                    .Append(new Point(lastFiniteValue, lastFiniteTime))
                    .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

            var valueAtZero = ValueAt(0);
            var sequence = valueAtZero > 0
                ? Sequence.MinusInfinite(0, valueAtZero).Elements
                    .Concat(upi)
                    .ToSequence()
                : upi.ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: lastFiniteValue,
                pseudoPeriodLength: 1,
                pseudoPeriodHeight: 0
            );
        }
        else if (!IsNonNegative)
        {
            if (FirstNonNegativeTime == Rational.PlusInfinity)
                // for any y in Q+, f(x) <= y for any x in Q+
                // hence sup{ Q+ } = +infty for all y
                return PlusInfinite();

            var T = Rational.Max(FirstPseudoPeriodEnd, FirstNonNegativeTime);
            if (ValueAt(T) < 0)
                T += PseudoPeriodLength;
            var sequence = CutAsEnumerable(0, T + PseudoPeriodLength, isEndIncluded: true)
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
            // the point at the right endpoint is included in case there is a left-discontinuity at the end of the pseudo-period
            // the pseudo-inverse of said point is then removed from the result
            var upi = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true)
                .UpperPseudoInverse()
                .SkipLast(1);

            var valueAtZero = ValueAt(0);
            var sequence = valueAtZero > 0
                ? Sequence.MinusInfinite(0, valueAtZero).Elements
                    .Concat(upi)
                    .ToSequence()
                : upi.ToSequence();

            return new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: ValueAt(PseudoPeriodStart),
                pseudoPeriodLength: PseudoPeriodHeight,
                pseudoPeriodHeight: PseudoPeriodLength
            );
        }
    }

    /// <summary>
    /// Computes the lower pseudo-inverse function over interval $I$, $f^{-1}_{\downarrow,I}(x)$.
    /// The support of the result will be the interval $f(I)$, defined as the smallest interval containing all $f(x)$ for $x \in I$.
    /// If $0 \in I$, the support is extended to start from 0.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If an invalid interval is given.</exception>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing or non-negative.</exception>
    /// <remarks>
    /// Defined and discussed in [ZNS23a].
    /// </remarks>
    public Curve LowerPseudoInverseOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (!IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded))
            throw new ArgumentException("The lower pseudo-inverse over an interval is defined only for functions that are non-decreasing over the same interval");
        if (!IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded))
            throw new ArgumentException("The lower pseudo-inverse over an interval is defined only for non-negative functions");

        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        var _end = end ?? Rational.PlusInfinity;
        var startFromZero = start == 0;

        if (start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");

            // point-interval, the inverse is just a point, if f(x) is finite
            var value = ValueAt(start);
            if (value.IsInfinite)
                return PlusInfinite();
            else
            {
                var point = new Point(ValueAt(start), start);
                return Minimum(PlusInfinite(), point);
            }
        }
        else if (_end < Rational.PlusInfinity)
        {
            // limited interval, no pseudo-periodic behavior
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            var lpi_raw = cut
                .LowerPseudoInverse(startFromZero)
                .ToList();
            var lpiShouldEndWithPoint = isEndIncluded || GetSegmentBefore(_end).IsConstant;
            var lpi = lpi_raw.Last() is Point && !lpiShouldEndWithPoint
                ? lpi_raw.SkipLast(1).ToSequence()
                : lpi_raw.ToSequence();
            return Minimum(PlusInfinite(), lpi);
        }
        else
        {
            if (IsUltimatelyConstant)
            {
                var constant_start = Rational.Max(PseudoPeriodStartInfimum, start);
                var constant_value = ValueAt(PseudoPeriodStart);
                var transient_lpi = CutAsEnumerable(start, constant_start, isEndIncluded: true)
                    .LowerPseudoInverse(startFromZero);
                var lpi = IsRightContinuousAt(constant_start)
                    ? transient_lpi
                        .Append(Segment.PlusInfinite(constant_value, constant_value + 2))
                    : transient_lpi
                        .Append(Segment.Constant(ValueAt(constant_start), constant_value, constant_start))
                        .Append(new Point(constant_value, constant_start))
                        .Append(Segment.PlusInfinite(constant_value, constant_value + 2));

                var valueAtStart = ValueAt(start);
                var sequence = !startFromZero && valueAtStart > 0
                    ? Sequence.PlusInfinite(0, valueAtStart).Elements
                        .Concat(lpi)
                        .ToSequence()
                    : lpi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: constant_value + 1,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                );
            }
            else if (IsUltimatelyPlusInfinite)
            {
                var lastFiniteTime = Rational.Max(PseudoPeriodStartInfimum, start); // T_I
                if (lastFiniteTime == 0)
                    return Zero();
                var valueAtLastFiniteTime = ValueAt(lastFiniteTime); // f(T_I)
                var lastFiniteValue = valueAtLastFiniteTime.IsFinite ? valueAtLastFiniteTime :
                    lastFiniteTime > 0 ? LeftLimitAt(lastFiniteTime) : 0; // L
                var isLastFiniteConstant = GetSegmentBefore(lastFiniteTime).IsConstant;
                var transient_lpi = start < lastFiniteTime
                    ? BaseSequence.CutAsEnumerable(start, lastFiniteTime)
                        .LowerPseudoInverse(startFromZero)
                    : Enumerable.Empty<Element>();
                var lpi = isLastFiniteConstant
                    ? transient_lpi
                        .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 2, lastFiniteTime))
                    : transient_lpi
                        .Append(new Point(lastFiniteValue, lastFiniteTime))
                        .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

                var valueAtStart = ValueAt(start).IsFinite ? ValueAt(start) : LeftLimitAt(start);
                var sequence = !startFromZero && valueAtStart > 0
                    ? Sequence.PlusInfinite(0, valueAtStart).Elements
                        .Concat(lpi)
                        .ToSequence()
                    : lpi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: isLastFiniteConstant ? lastFiniteValue + 1 : lastFiniteValue,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                );
            }
            else
            {
                var periodStart = (start < PseudoPeriodStart ? PseudoPeriodStart : start) + PseudoPeriodLength;
                // the point at the right endpoint is included in case there is a left-discontinuity at the end of the pseudo-period
                // the pseudo-inverse of said point is then removed from the result
                var lpi = CutAsEnumerable(start, periodStart + PseudoPeriodLength, isEndIncluded: true)
                    .LowerPseudoInverse(startFromZero)
                    .SkipLast(1);

                var valueAtStart = ValueAt(start);
                var sequence = !startFromZero && valueAtStart > 0
                    ? Sequence.PlusInfinite(0, valueAtStart).Elements
                        .Concat(lpi)
                        .ToSequence()
                    : lpi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: ValueAt(periodStart),
                    pseudoPeriodLength: PseudoPeriodHeight,
                    pseudoPeriodHeight: PseudoPeriodLength
                ).TransientReduction();
            }
        }
    }

    /// <summary>
    /// Computes the upper pseudo-inverse function over interval $D$, $f^{-1}_{\uparrow,D}(x)$.
    /// The support of the result will be the interval $f(I)$, defined as the smallest interval containing all $f(x)$ for $x \in I$.
    /// </summary>
    /// <param name="start">Start of the interval.</param>
    /// <param name="end">End of the interval. If not specified, it is assumed $+\infty$.</param>
    /// <param name="isStartIncluded">If true, the interval is left-closed.</param>
    /// <param name="isEndIncluded">If true, the interval is right-closed. If <paramref name="end"/> is $+\infty$, it has no effect.</param>
    /// <exception cref="ArgumentException">If the curve is not non-decreasing or non-negative.</exception>
    /// <remarks>
    /// Defined and discussed in [ZNS23a].
    /// </remarks>
    public Curve UpperPseudoInverseOverInterval(
        Rational start,
        Rational? end = null,
        bool isStartIncluded = true,
        bool isEndIncluded = false
    )
    {
        if (!IsNonDecreasingOverInterval(start, end, isStartIncluded, isEndIncluded))
            throw new ArgumentException("The upper pseudo-inverse over an interval is defined only for functions that are non-decreasing over the same interval");
        if (!IsNonNegativeOverInterval(start, end, isStartIncluded, isEndIncluded))
            throw new ArgumentException("The upper pseudo-inverse over an interval is defined only for non-negative functions");

        if (start > end)
            throw new ArgumentException("Interval start cannot be after its end.");
        if (start < 0 || start == Rational.PlusInfinity)
            throw new ArgumentException($"Invalid start: {start}");

        var _end = end ?? Rational.PlusInfinity;

        if (start == _end)
        {
            if (!(isStartIncluded && isEndIncluded))
                throw new ArgumentException("Interval endpoints, if equal, must be both inclusive.");

            // point-interval, the inverse is just a point, if f(x) is finite
            var value = ValueAt(start);
            if (value.IsInfinite)
                return MinusInfinite();
            else
            {
                var point = new Point(ValueAt(start), start);
                return Maximum(MinusInfinite(), point);
            }
        }
        else if (_end < Rational.PlusInfinity)
        {
            // limited interval, no pseudo-periodic behavior
            var cut = CutAsEnumerable(start, _end, isStartIncluded, isEndIncluded);
            var upi_raw = cut
                .UpperPseudoInverse()
                .ToList();
            var upiShouldEndWithPoint = isEndIncluded || GetSegmentBefore(_end).IsConstant;
            var upi = upi_raw.Last() is Point && !upiShouldEndWithPoint
                ? upi_raw.SkipLast(1).ToSequence()
                : upi_raw.ToSequence();
            return Maximum(MinusInfinite(), upi);
        }
        else
        {
            if (IsUltimatelyConstant)
            {
                var constant_start = Rational.Max(PseudoPeriodStartInfimum, start);
                var constant_value = ValueAt(PseudoPeriodStart);
                var transient_upi = start < constant_start
                    ? CutAsEnumerable(start, constant_start)
                        .UpperPseudoInverse()
                    : Enumerable.Empty<Element>();
                var upi = IsRightContinuousAt(constant_start)
                    ? transient_upi
                        .Append(Point.PlusInfinite(constant_value))
                        .Append(Segment.PlusInfinite(constant_value, constant_value + 1))
                    : transient_upi
                        .Append(new Point(ValueAt(constant_start), constant_start))
                        .Append(Segment.Constant(ValueAt(constant_start), constant_value, constant_start))
                        .Append(Point.PlusInfinite(constant_value))
                        .Append(Segment.PlusInfinite(constant_value, constant_value + 1));

                var valueAtStart = ValueAt(start);
                var sequence = valueAtStart > 0
                    ? Sequence.MinusInfinite(0, valueAtStart).Elements
                        .Concat(upi)
                        .ToSequence()
                    : upi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: constant_value,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                );
            }
            else if (IsUltimatelyPlusInfinite)
            {
                var lastFiniteTime = Rational.Max(PseudoPeriodStartInfimum, start); // T_I
                if (lastFiniteTime == 0)
                    return Zero();
                var valueAtLastFiniteTime = ValueAt(lastFiniteTime); // f(T_I)
                var lastFiniteValue = valueAtLastFiniteTime.IsFinite ? valueAtLastFiniteTime :
                    lastFiniteTime > 0 ? LeftLimitAt(lastFiniteTime) : 0; // L
                var isLastFiniteConstant = GetSegmentBefore(lastFiniteTime).IsConstant;
                var transient_upi = start < lastFiniteTime
                    ? BaseSequence.CutAsEnumerable(start, lastFiniteTime)
                        .UpperPseudoInverse()
                    : Enumerable.Empty<Element>();
                var upi = isLastFiniteConstant
                    ? transient_upi
                        .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime))
                    : transient_upi
                        .Append(new Point(lastFiniteValue, lastFiniteTime))
                        .Append(Segment.Constant(lastFiniteValue, lastFiniteValue + 1, lastFiniteTime));

                var valueAtStart = ValueAt(start).IsFinite ? ValueAt(start) : LeftLimitAt(start);
                var sequence = valueAtStart > 0
                    ? Sequence.MinusInfinite(0, valueAtStart).Elements
                        .Concat(upi)
                        .ToSequence()
                    : upi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: lastFiniteValue,
                    pseudoPeriodLength: 1,
                    pseudoPeriodHeight: 0
                );
            }
            else
            {
                var periodStart = (start < PseudoPeriodStart ? PseudoPeriodStart : start) + PseudoPeriodLength;
                // the point at the right endpoint is included in case there is a left-discontinuity at the end of the pseudo-period
                // the pseudo-inverse of said point is then removed from the result
                var upi = CutAsEnumerable(start, periodStart + PseudoPeriodLength, isEndIncluded: true)
                    .UpperPseudoInverse()
                    .SkipLast(1);

                var valueAtStart = ValueAt(start);
                var sequence = valueAtStart > 0
                    ? Sequence.MinusInfinite(0, valueAtStart).Elements
                        .Concat(upi)
                        .ToSequence()
                    : upi.ToSequence();

                return new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: ValueAt(periodStart),
                    pseudoPeriodLength: PseudoPeriodHeight,
                    pseudoPeriodHeight: PseudoPeriodLength
                );
            }
        }
    }

    /// <summary>
    /// Computes the horizontal deviation between the two curves, $hDev(f, g)$.
    /// If <paramref name="f"/> is an arrival curve and <paramref name="g"/> a service curve, the result will be the worst-case delay.
    /// </summary>
    /// <param name="f">Must be non-decreasing.</param>
    /// <param name="g">Must be non-decreasing.</param>
    /// <param name="settings"></param>
    /// <returns>A non-negative horizontal deviation.</returns>
    public static Rational HorizontalDeviation(Curve f, Curve g, ComputationSettings? settings = null)
    {
        if (!f.IsNonDecreasing || !g.IsNonDecreasing)
            throw new ArgumentException("The arguments must be non-decreasing.");

        if (f is SigmaRhoArrivalCurve sr && g is RateLatencyServiceCurve rl)
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
            // todo: document source for this result
            var a_upi = a.UpperPseudoInverse();
            var b_upi = b.UpperPseudoInverse();
            var hDev = MaxPlusDeconvolution(a_upi, b_upi, settings)
                .Negate()
                .ToNonNegative()
                .ValueAt(0);
            #elif false
            // [DNC18] Proposition 5.14
            var hDev = b.LowerPseudoInverse()
                .Composition(a, settings)
                .Deconvolution(new RateLatencyServiceCurve(1, 0), settings)
                .ValueAt(0);
            #elif true
            // Derived from [DNC18] Lemma 5.2 and similar, in principle, to [DNC18] Proposition 5.14
            var hDev = g.LowerPseudoInverse()
                .Composition(f, settings)
                .Subtraction(new RateLatencyServiceCurve(1, 0), settings)
                .SupValue();
            #endif
            return hDev;
        }
    }

    /// <summary>
    /// Given $hDev(f, g, t) = \inf\{ d \ge 0 \mid f(t) \le g(t+d) \}$,
    /// computes the first time around which $hDev(f, g, t)$ gets close to the horizontal deviation between the two curves, $hDev(f, g)$
    /// (i.e., either it attains the value or has it as a limit).
    /// </summary>
    /// <param name="f">Must be non-decreasing.</param>
    /// <param name="g">Must be non-decreasing.</param>
    /// <param name="settings"></param>
    /// <remarks>
    /// The definition of $hDev(f, g, t)$ is based on [DNC18] Lemma 5.1.
    /// </remarks>
    public static Rational HorizontalDeviationMeasuredAt(Curve f, Curve g, ComputationSettings? settings = null)
    {
        if (!f.IsNonDecreasing || !g.IsNonDecreasing)
            throw new ArgumentException("The arguments must be non-decreasing.");

        if (f is SigmaRhoArrivalCurve sr && g is RateLatencyServiceCurve rl)
        {
            if(rl.Rate >= sr.Rho)
                return 0;
            else
                return Rational.PlusInfinity;
        }
        else
        {
            var hDevArg = g.LowerPseudoInverse()
                .Composition(f, settings)
                .Subtraction(new RateLatencyServiceCurve(1, 0), settings)
                .SupArg();
            return hDevArg;
        }
    }

    /// <summary>
    /// Computes the vertical deviation between the two curves, $vDev(f, g) = \sup_{u \ge 0}\{ f(u) - g(u) \}$.
    /// If <paramref name="f"/> is an arrival curve and <paramref name="g"/> a service curve, the result will be the worst-case backlog.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <returns>A vertical deviation.</returns>
    /// <remarks>
    /// Following from the definition in [DNC18] p.100, the result may be negative.
    /// </remarks>
    public static Rational VerticalDeviation(Curve f, Curve g)
    {
        if (f is SigmaRhoArrivalCurve sr && g is RateLatencyServiceCurve dr)
        {
            if(dr.Rate >= sr.Rho)
                return sr.Sigma + dr.Latency * sr.Rho;
            else
                return Rational.PlusInfinity;
        }
        else
        {
            var diff = f - g;
            return diff.SupValue();
        }
    }

    /// <summary>
    /// Computes the first time around which the difference between the two curves $f(t) - g(t)$ gets close to their vertical deviation 
    /// $vDev(f, g)$.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    public static Rational VerticalDeviationMeasuredAt(Curve f, Curve g)
    {
        if (f is SigmaRhoArrivalCurve sr && g is RateLatencyServiceCurve dr)
        {
            if (dr.Rate >= sr.Rho)
                if (sr.Rho > 0)
                    return dr.Latency;
                else
                    return 0;
            else
                return Rational.PlusInfinity;
        }
        else
        {
            var diff = f - g;
            return diff.SupArg();
        }
    }

    /// <summary>
    /// Computes the deviation $z(f, g) = \inf\{t \ge 0 \mid f \otimes g (t) \ge 0 \}$.
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <param name="settings"></param>
    /// <returns>The z-deviation between the two curves.</returns>
    /// <remarks>
    /// Used in [HCS24] Theorem 19, generalizing the computation of delay bounds for negative service curves.
    /// For that result, <paramref name="f"/> should be a minimal arrival curve and <paramref name="g"/> should be a (lower) non-decreasing service curve.
    /// </remarks>
    public static Rational ZDeviation(Curve f, Curve g, ComputationSettings? settings = null)
    {
        var conv = Curve.Convolution(f, g, settings);
        return conv.FirstNonNegativeTime;
    }

    /// <summary>
    /// If the curve is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public Rational SupValue(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope <= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.SupValue();
        }
        else
        {
            return Rational.PlusInfinity;
        }
    }

    /// <summary>
    /// If the curve is upper-bounded, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$,
    /// returns the first time around which $f(t)$ gets close to $x$ (i.e., either it attains the value or has it as a limit).
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public Rational SupArg(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope <= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.SupArg();
        }
        else
        {
            return Rational.PlusInfinity;
        }
    }

    /// <summary>
    /// If the curve has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for any $t \ge 0$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MaxValue(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope <= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.MaxValue();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// If the curve has a maximum, i.e., exist $x$ and $t^*$ such that $f(t) \le x$ for any $t \ge 0$ and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MaxArg(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope <= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.MaxArg();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// If the curve is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for any $t \ge 0$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public Rational InfValue(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope >= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.InfValue();
        }
        else
        {
            return Rational.MinusInfinity;
        }
    }

    /// <summary>
    /// If the curve is lower-bounded, i.e., exists $x$ such that $f(t) \ge x$ for any $t \ge 0$,
    /// returns the first time around which $f(t)$ gets close to $x$ (i.e., either it attains the value or has it as a limit).
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public Rational InfArg(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope >= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.InfArg();
        }
        else
        {
            return Rational.MinusInfinity;
        }
    }

    /// <summary>
    /// If the curve has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for any $t \ge 0$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MinValue(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope >= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.MinValue();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// If the curve has a minimum, i.e., exist $x$ and $t^*$ such that $f(t) \ge x$ for any $t \ge 0$ and $f(t^*) = x$, returns $t^*$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MinArg(ComputationSettings? settings = null)
    {
        if (PseudoPeriodSlope <= 0)
        {
            var cut = CutAsEnumerable(0, FirstPseudoPeriodEnd, isEndIncluded: true, settings: settings);
            return cut.MinArg();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// If the curve is upper-bounded in interval $I$, i.e., exists $x$ such that $f(t) \le x$ for any $t \ge 0$, returns $\inf x$.
    /// Otherwise, returns $+\infty$.
    /// </summary>
    public Rational SupValueOverInterval(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
        ComputationSettings? settings = null
    )
    {
        var cut = CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded, settings);
        return cut.SupValue();
    }

    /// <summary>
    /// If the curve has a maximum in interval $I$, i.e., exist $x$ and $t^* \in I$ such that $f(t) \le x$ for any $t \in I$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MaxValueOverInterval(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
        ComputationSettings? settings = null
    )
    {
        var cut = CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded, settings);
        return cut.MaxValue();
    }

    /// <summary>
    /// If the curve is lower-bounded in interval $I$, i.e., exists $x$ such that $f(t) \ge x$ for any $t \in I$, returns $\sup x$.
    /// Otherwise, returns $-\infty$.
    /// </summary>
    public Rational InfValueOverInterval(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
        ComputationSettings? settings = null
    )
    {
        var cut = CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded, settings);
        return cut.InfValue();
    }

    /// <summary>
    /// If the curve has a minimum in interval $I$, i.e., exist $x$ and $t^* \in I$ such that $f(t) \ge x$ for any $t \in I$ and $f(t^*) = x$, returns $x$.
    /// Otherwise, returns null.
    /// </summary>
    public Rational? MinValueOverInterval(
        Rational cutStart,
        Rational cutEnd,
        bool isStartIncluded = true,
        bool isEndIncluded = false,
        ComputationSettings? settings = null
    )
    {
        var cut = CutAsEnumerable(cutStart, cutEnd, isStartIncluded, isEndIncluded, settings);
        return cut.MinValue();
    }

    /// <summary>
    /// Computes the floor function, $\lfloor f(t) \rfloor$.
    /// </summary>
    public Curve Floor()
    {
        if (PseudoPeriodHeight == 0)
        {
            return new Curve(
                BaseSequence.Floor(),
                PseudoPeriodStart,
                PseudoPeriodLength,
                PseudoPeriodHeight
            ).Optimize();
        }
        else
        {
            var d = PseudoPeriodLength * PseudoPeriodHeight.Denominator;
            var c = PseudoPeriodHeight.Numerator;
            var T = PseudoPeriodStart;
            return new Curve(
                Cut(0, T + d).Floor(),
                T,
                d,
                c
            ).Optimize();
        }
    }

    /// <summary>
    /// Computes the ceiling function, $\lceil f(t) \rceil$.
    /// </summary>
    public Curve Ceil()
    {
        if (PseudoPeriodHeight == 0)
        {
            return new Curve(
                BaseSequence.Ceil(),
                PseudoPeriodStart,
                PseudoPeriodLength,
                PseudoPeriodHeight
            ).Optimize();
        }
        else
        {
            var d = PseudoPeriodLength * PseudoPeriodHeight.Denominator;
            var c = PseudoPeriodHeight.Numerator;
            var T = PseudoPeriodStart;
            return new Curve(
                Cut(0, T + d).Ceil(),
                T,
                d,
                c
            ).Optimize();
        }
    }

    #endregion

    #region Optimization

    /// <summary>
    /// Optimizes Curve representation by anticipating periodic start and reducing period length.
    /// </summary>
    /// <returns>An equivalent minimal representation for the same curve.</returns>
    /// <remarks>This method implements representation minimization, which is discussed in [ZS23].</remarks>
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
                pseudoPeriodHeight: PseudoPeriodSlope
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
                var d = this.PseudoPeriodLength;
                var c = this.PseudoPeriodHeight;
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
                    pseudoPeriodHeight: periodLength * PseudoPeriodSlope
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
                bool tryShorten = periodStart - periodLength >= 0;
                // int iteration = 0;
                while (tryShorten)
                {
                    // iteration++;
                    // logger.Trace($"TransientReduction by period, iteration #{iteration}");

                    var candidatePeriod = sequence.Cut(periodStart - periodLength, periodStart).Optimize();
                    var shiftedCandidateSequence = candidatePeriod
                        .VerticalShift(PseudoPeriodHeight, exceptOrigin: false)
                        .Delay(periodLength, prependWithZero: false);
                    if (shiftedCandidateSequence == pseudoPeriodicSequence)
                    {
                        periodStart -= periodLength;
                        pseudoPeriodicSequence = candidatePeriod;

                        optimized = true;
                        tryShorten = periodStart - periodLength >= 0;
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
                if (periodStart == 0)
                    return;

                Sequence transientTail, periodTail;
                Rational transientTailSlope, periodTailSlope;
                bool tryShorten;
                updateTails();

                while (tryShorten)
                {
                    var length = Rational.Min(transientTail.Length, periodTail.Length);
                    if (transientTail.Length > length)
                        transientTail = transientTail.Cut(transientTail.DefinedUntil - length,
                            transientTail.DefinedUntil);
                    if (periodTail.Length > length)
                        periodTail = periodTail.Cut(periodTail.DefinedUntil - length,
                            periodTail.DefinedUntil);

                    var shiftedTransientSequence = transientTail
                        .VerticalShift(PseudoPeriodHeight, exceptOrigin: false)
                        .Delay(periodLength, prependWithZero: false);
                    if (shiftedTransientSequence == periodTail)
                    {
                        periodStart -= length;
                        sequence = sequence.Cut(0, periodStart + periodLength);
                        optimized = true;
                        if(periodStart == 0)
                            return;
                        updateTails();
                    }
                    else
                    {
                        tryShorten = false;
                    }
                }

                void updateTails()
                {
                    transientTail = sequence
                        .CutAsEnumerable(0, periodStart)
                        .TakeLast(2).ToSequence();
                    transientTailSlope = ((Segment)transientTail.Elements.Last()).Slope;
                    periodTail = sequence
                        .CutAsEnumerable(periodStart, periodStart + periodLength)
                        .TakeLast(2).ToSequence();
                    periodTailSlope = ((Segment)periodTail.Elements.Last()).Slope;
                    tryShorten = transientTailSlope == periodTailSlope;
                }
            }
        }
    }

    #endregion Optimization

    #region Addition and Subtraction operators

    /// <summary>
    /// Computes the sum of the two curves.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Algorithm defined in [BT08] Section 4.2 </remarks>
    public virtual Curve Addition(Curve b, ComputationSettings? settings = null)
    {
        settings ??= new ComputationSettings();
        
        Rational T = Rational.Max(PseudoPeriodStart, b.PseudoPeriodStart);
        Rational d = Rational.LeastCommonMultiple(PseudoPeriodLength, b.PseudoPeriodLength);
        Rational c = d * (PseudoPeriodSlope + b.PseudoPeriodSlope);

        Sequence extendedSequence1 = Extend(T + d, settings);
        Sequence extendedSequence2 = b.Extend(T + d, settings);

        Sequence baseSequence = extendedSequence1 + extendedSequence2;
        var result = new Curve(
            baseSequence: baseSequence,
            pseudoPeriodStart: T,
            pseudoPeriodLength: d,
            pseudoPeriodHeight: c
        );

        return settings.UseRepresentationMinimization ? result.Optimize() : result;
    }

    /// <summary>
    /// Computes the sum of the two curves.
    /// </summary>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Algorithm defined in [BT08] Section 4.2 </remarks>
    public static Curve Addition(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Addition(b);

    /// <summary>
    /// Computes the sum of a set of curves.
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
                .Aggregate((f, g) => Addition(f, g, settings));
        }
        else
        {
            result = curves
                .Aggregate((f, g) => Addition(f, g, settings));
        }

        return result;
    }

    /// <summary>
    /// Computes the sum of a set of curves.
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
                .Aggregate((f, g) => Addition(f, g, settings));
        }
        else
        {
            return curves
                .Aggregate((f, g) => Addition(f, g, settings));
        }
    }

    /// <summary>
    /// Computes the sum of the two curves.
    /// </summary>
    /// <returns>The curve resulting from the sum.</returns>
    /// <remarks> Algorithm defined in [BT08] Section 4.2 </remarks>
    public static Curve operator +(Curve a, Curve b)
        => a.Addition(b);

    /// <summary>
    /// Computes the subtraction between two curves.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the subtraction.</returns>
    /// <remarks>
    /// The result may contain negative values. 
    /// Use <see cref="ToNonNegative"/> for a non-negative closure. 
    /// </remarks>
    public virtual Curve Subtraction(Curve b, ComputationSettings? settings = null)
        => Addition(-b, settings);

    /// <summary>
    /// Computes the subtraction between two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the subtraction.</returns>
    /// <remarks>
    /// The result may contain negative values. 
    /// Use <see cref="ToNonNegative"/> for a non-negative closure. 
    /// </remarks>
    public static Curve Subtraction(Curve a, Curve b, ComputationSettings? settings = null)
        => a.Subtraction(b, settings);

    /// <summary>
    /// Computes the subtraction between two curves.
    /// </summary>
    /// <param name="b">Second operand.</param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the subtraction.</returns>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public virtual Curve Subtraction(Curve b, bool nonNegative, ComputationSettings? settings = null)
        => nonNegative ?
            Addition(-b, settings).ToNonNegative() :
            Addition(-b, settings);

    /// <summary>
    /// Computes the subtraction between two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="nonNegative">If true, the result is non-negative.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the subtraction.</returns>
    [Obsolete("Subtraction with implicit handling of negative values is going to be removed in a later version.")]
    public static Curve Subtraction(Curve a, Curve b, bool nonNegative, ComputationSettings? settings = null)
        => a.Subtraction(b, nonNegative, settings);

    /// <summary>
    /// Computes the subtraction between two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>The non-negative curve resulting from the subtraction.</returns>
    /// <remarks>
    /// The result may contain negative values. 
    /// Use <see cref="ToNonNegative"/> for a non-negative closure. 
    /// </remarks>
    public static Curve operator -(Curve a, Curve b)
        => a.Subtraction(b);

    #endregion Addition and Subtraction operators

    #region Minimum and maximum operators

    /// <summary>
    /// True if $f \wedge g$ is known to be ultimately pseudo-periodic.
    /// Tests the sufficient (but not necessary) conditions from [BT08].
    /// </summary>
    /// <remarks>
    /// If false, the result <see cref="Minimum(Curve, ComputationSettings?)"/> may be invalid.
    /// </remarks>
    public static bool IsMinimumUltimatelyPseudoPeriodic(Curve f, Curve g)
    {
        return f.IsUltimatelyPlain && g.IsUltimatelyPlain;
    }

    /// <summary>
    /// Computes the minimum of two curves.
    /// </summary>
    /// <param name="curve">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the minimum.</returns>
    /// <remarks>
    /// Algorithm defined in [BT08] Section 4.3
    /// </remarks>
    public virtual Curve Minimum(Curve curve, ComputationSettings? settings = null)
    {
        // Renaming for symmetry
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

        if (a.PseudoPeriodSlope == b.PseudoPeriodSlope)
        {
            d = EarliestValidLength();
            c = d * a.PseudoPeriodSlope;
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
                if (Sequence.LessOrEqual(aBaseCut, bBaseCut, settings) && a.PseudoPeriodSlope <= b.PseudoPeriodSlope)
                    return a.PseudoPeriodLength;
                if (Sequence.LessOrEqual(bBaseCut, aBaseCut, settings) && b.PseudoPeriodSlope <= a.PseudoPeriodSlope)
                    return b.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            }
        }
        else
        {
            Curve ultimatelyLower, ultimatelyHigher;

            if (a.PseudoPeriodSlope < b.PseudoPeriodSlope)
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

            if (!ultimatelyHigher.IsUltimatelyInfinite && !ultimatelyLower.IsUltimatelyInfinite)
            {
                Rational boundsIntersection = BoundsIntersection(ultimatelyLower: ultimatelyLower, ultimatelyHigher: ultimatelyHigher);
                T = Rational.Max(boundsIntersection, a.PseudoPeriodStart, b.PseudoPeriodStart);
                #if DO_LOG && TRACE_MIN_MAX_EXTENSIONS
                logger.Trace($"Minimum, different slopes: {a.PseudoPeriodSlope} ~ {(decimal)a.PseudoPeriodSlope}, {b.PseudoPeriodSlope} ~ {(decimal)b.PseudoPeriodSlope}");
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

        var retVal = settings.UseRepresentationMinimization ? result.Optimize() : result;

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Minimum: took {timer.Elapsed}; a {a.BaseSequence.Count} b {b.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {retVal}");
        #endif
        return retVal;
    }

    //Bounds intersection is proved in [BT08] proposition 4, proof 1
    private static Rational BoundsIntersection(Curve ultimatelyLower, Curve ultimatelyHigher)
    {
        //Bounds are computed relative to origin-passing lines with slope
        //equal to the functions' periodic slopes.

        //fl(t) <= M + fl.PseudoPeriodSlope * t; t >= fl.PseudoPeriodStartTime
        Rational M = DeviationsFromAverageSlopeLine(ultimatelyLower)
            .Max();

        //fh(t) >= m + fh.PseudoPeriodSlope * t; t >= fh.PseudoPeriodStartTime
        Rational m = DeviationsFromAverageSlopeLine(ultimatelyHigher)
            .Min();

        //Intersection points between the two bounds
        var boundIntersection = (M - m) / (ultimatelyHigher.PseudoPeriodSlope - ultimatelyLower.PseudoPeriodSlope);
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
                        deviations.Add(p.Value - f.PseudoPeriodSlope * p.Time);
                        break;

                    case Segment s:
                        deviations.Add(s.RightLimitAtStartTime - f.PseudoPeriodSlope * s.StartTime);
                        deviations.Add(s.LeftLimitAtEndTime - f.PseudoPeriodSlope * s.EndTime);
                        break;

                    default:
                        throw new InvalidCastException();
                }
            }
            return deviations;
        }
    }

    /// <summary>
    /// Computes the minimum of two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the minimum.</returns>
    /// <remarks>
    /// Algorithm defined in [BT08] Section 4.3
    /// </remarks>
    public static Curve Minimum(Curve? a, Curve? b, ComputationSettings? settings = null)
    {
        a ??= PlusInfinite();
        b ??= PlusInfinite();
        return a.Minimum(b, settings);
    }

    /// <summary>
    /// Computes the minimum of a set of curves.
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
                .Aggregate((f, g) => Minimum(f, g, settings));
        }
        else
        {
            #if DO_LOG
            Curve current = curves.First();
            int i = 1;
            foreach (var curve in curves.Skip(1))
            {
                logger.Trace($"Minimum {i} of {curves.Count - 1}");
                i++;
                current = Minimum(current, curve, settings);
            }
            return current;
            #else
            return curves.Aggregate((f, g) => Minimum(f, g, settings));
            #endif
        }
    }

    /// <summary>
    /// Computes the minimum of a set of curves.
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
    /// True if $f \vee g$ is known to be ultimately pseudo-periodic.
    /// Tests the sufficient (but not necessary) conditions from [BT08].
    /// </summary>
    /// <remarks>
    /// If false, the result <see cref="Maximum(Unipi.Nancy.MinPlusAlgebra.Curve,Unipi.Nancy.MinPlusAlgebra.ComputationSettings?)"/> may be invalid.
    /// </remarks>
    public static bool IsMaximumUltimatelyPseudoPeriodic(Curve f, Curve g)
    {
        return f.IsUltimatelyPlain && g.IsUltimatelyPlain;
    }

    /// <summary>
    /// Computes the maximum of two curves.
    /// </summary>
    /// <param name="curve">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the maximum.</returns>
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

        if (a.PseudoPeriodSlope == b.PseudoPeriodSlope)
        {
            d = EarliestValidLength();
            c = d * a.PseudoPeriodSlope;
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
                if (Sequence.GreaterOrEqual(aBaseCut, bBaseCut, settings) && a.PseudoPeriodSlope >= b.PseudoPeriodSlope)
                    return a.PseudoPeriodLength;
                if (Sequence.GreaterOrEqual(bBaseCut, aBaseCut, settings) && b.PseudoPeriodSlope >= a.PseudoPeriodSlope)
                    return b.PseudoPeriodLength;

                return Rational.LeastCommonMultiple(a.PseudoPeriodLength, b.PseudoPeriodLength);
            }
        }
        else
        {
            Curve ultimatelyLower, ultimatelyHigher;

            if (a.PseudoPeriodSlope < b.PseudoPeriodSlope)
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

            if (!ultimatelyHigher.IsUltimatelyInfinite && !ultimatelyLower.IsUltimatelyInfinite)
            {
                Rational boundsIntersection = BoundsIntersection(ultimatelyLower: ultimatelyLower, ultimatelyHigher: ultimatelyHigher);
                T = Rational.Max(boundsIntersection, a.PseudoPeriodStart, b.PseudoPeriodStart);
#if TRACE_MIN_MAX_EXTENSIONS
                #if DO_LOG
                logger.Trace($"Maximum, different slopes: {a.PseudoPeriodSlope} ~ {(decimal)a.PseudoPeriodSlope}, {b.PseudoPeriodSlope} ~ {(decimal)b.PseudoPeriodSlope}");
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

        var retVal = settings.UseRepresentationMinimization ? result.Optimize() : result;

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
    /// Computes the maximum of two curves.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the maximum.</returns>
    public static Curve Maximum(Curve? a, Curve? b, ComputationSettings? settings = null)
    {
        a ??= MinusInfinite();
        b ??= MinusInfinite();
        return a.Maximum(b, settings);
    }

    /// <summary>
    /// Computes the maximum of a set of curves.
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
                .Aggregate((f, g) => Maximum(f, g, settings));
        }
        else
        {
            #if DO_LOG
            Curve current = curves.First();
            int i = 1;
            foreach (var curve in curves.Skip(1))
            {
                logger.Trace($"Maximum {i} of {curves.Count - 1}");
                i++;
                current = Maximum(current, curve, settings);
            }
            return current;
            #else
            return curves.Aggregate((f, g) => Maximum(f, g, settings));
            #endif
        }
    }

    /// <summary>
    /// Computes the maximum of a set of curves.
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
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its support.
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
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public static Curve Minimum(Curve c, Element e, ComputationSettings? settings = null)
        => c.Minimum(e, settings);

    /// <summary>
    /// Computes the minimum between the curve and sequence.
    /// The sequence is considered to have $s(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public Curve Minimum(Sequence s, ComputationSettings? settings = null)
    {
        var periodStart = s.DefinedUntil < PseudoPeriodStart ? PseudoPeriodStart : s.DefinedUntil + PseudoPeriodLength;
        var cut = Cut(0, periodStart + PseudoPeriodLength);
        var minS = Sequence.Minimum(cut, s, false);

        var result = new Curve(
            baseSequence: minS,
            pseudoPeriodStart: periodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodHeight
        );
        return result.Optimize();
    }

    /// <summary>
    /// Computes the minimum between the curve and sequence.
    /// The sequence is considered to have $s(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public static Curve Minimum(Curve c, Sequence s, ComputationSettings? settings = null)
        => c.Minimum(s, settings);

    /// <summary>
    /// Computes the maximum between the curve and element.
    /// The element is considered to have $e(t) = -\infty$ for any $t$ outside its support.
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
    /// The element is considered to have $e(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public static Curve Maximum(Curve c, Element e, ComputationSettings? settings = null)
        => c.Maximum(e, settings);

    /// <summary>
    /// Computes the maximum between the curve and sequence.
    /// The sequence is considered to have $s(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public Curve Maximum(Sequence s, ComputationSettings? settings = null)
    {
        var periodStart = s.DefinedUntil < PseudoPeriodStart ? PseudoPeriodStart : s.DefinedUntil + PseudoPeriodLength;
        var cut = Cut(0, periodStart + PseudoPeriodLength);
        var maxS = Sequence.Maximum(cut, s, false);

        var result = new Curve(
            baseSequence: maxS,
            pseudoPeriodStart: periodStart,
            pseudoPeriodLength: PseudoPeriodLength,
            pseudoPeriodHeight: PseudoPeriodHeight
        );
        return result.Optimize();
    }

    /// <summary>
    /// Computes the maximum between the curve and sequence.
    /// The sequence is considered to have $s(t) = +\infty$ for any $t$ outside its support.
    /// </summary>
    public static Curve Maximum(Curve c, Sequence s, ComputationSettings? settings = null)
        => c.Maximum(s, settings);

    #endregion Minimum and maximum operators

    #region Convolution operator

    /// <summary>
    /// Computes the convolution of two curves, $f \otimes g$.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the convolution.</returns>
    /// <remarks>
    /// Base algorithm derived from [BT08] Section 4.4, with isospeed optimizations described in [ZNS23a] and [TBP]
    /// </remarks>
    public virtual Curve Convolution(Curve curve, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var f = this;
        var g = curve;

        //Checks for convolution with infinite curves
        if (f.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return g.VerticalShift(f.ValueAt(0), false);
        if (g.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return f.VerticalShift(g.ValueAt(0), false);

        //Checks for convolution of positive curve with zero
        if (f.IsZero || g.IsZero)
        {
            if (f.IsZero && g.IsNonNegative)
                return f;
            if (g.IsZero && f.IsNonNegative)
                return g;
        }

        #if DO_LOG
        logger.Trace($"Computing convolution of f ({f.BaseSequence.Count} elements, T: {f.PseudoPeriodStart} d: {f.PseudoPeriodLength})" +
                     $" and g ({g.BaseSequence.Count} elements, T: {g.PseudoPeriodStart} d: {g.PseudoPeriodLength})");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"f:\n {f} \n g:\n {g}");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        Curve result;
        if (settings.SinglePassConvolution && f.PseudoPeriodSlope == g.PseudoPeriodSlope)
        {
            result = SinglePassConvolution();
        }
        else
        {
            var terms = new List<Curve>();
            if (
                !settings.SinglePassConvolution &&  // if true, checking for equivalence is useless due to the check above
                Equivalent(f, g, settings)
            )
            {
                // self convolution: skip duplicate middle term
                if (f.HasTransient)
                    terms.Add(ConvolutionTransientTransient(f, f));

                if (f.HasTransient && !f.IsUltimatelyInfinite)
                    terms.Add(ConvolutionTransientPeriodic(f, f));

                if (!f.IsUltimatelyInfinite)
                    terms.Add(ConvolutionPeriodicPeriodic(f, f));
            }
            else
            {
                if (f.HasTransient)
                {
                    if (g.HasTransient)
                        terms.Add(ConvolutionTransientTransient(f, g));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(ConvolutionTransientPeriodic(f, g));
                }

                if (!f.IsUltimatelyInfinite)
                {
                    if (g.HasTransient)
                        terms.Add(ConvolutionTransientPeriodic(g, f));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(ConvolutionPeriodicPeriodic(f, g));
                }
            }
            result = Minimum(terms, settings);
        }

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Convolution: took {timer.Elapsed}; f {f.BaseSequence.Count} g {g.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {result}");
        #endif
        return result;

        // Computes the convolution in a single operation,
        // since all UPP parameters (in particular, T) can be determined a priori
        Curve SinglePassConvolution()
        {
            #if DO_LOG
            logger.Trace("Convolution: same slope, single pass");
            #endif
            // From rho_f = rho_g it follows that k_d_f = k_c_f.
            // Hence, there is no improvement on the UPP parameters to be gained using isomorphisms.
            // The optimization lies instead in the use of vertical filtering, and the by-sequence heuristic

            var d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var T = f.PseudoPeriodStart + g.PseudoPeriodStart + d;
            var c = f.PseudoPeriodSlope * d;

            var cutEnd = T + d;
            bool useIsomorphism = settings.UseConvolutionIsospeedOptimization &&
                                  f.IsLeftContinuous && g.IsLeftContinuous &&
                                  f.IsNonDecreasing && g.IsNonDecreasing &&
                                  !f.IsUltimatelyConstant && !g.IsUltimatelyConstant &&
                                  // compared to the normal path, we don't check this before entering SinglePassConvolution()
                                  !f.IsUltimatelyInfinite && !g.IsUltimatelyInfinite;
            Rational cutCeiling;
            if (useIsomorphism)
            {
                // Vertical filtering optimization, discussed in [ZNS23a]
                var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
                cutCeiling = f.ValueAt(f.PseudoPeriodStart) + g.ValueAt(g.PseudoPeriodStart) + 2 * lcm_c;
            }
            else
            {
                cutCeiling = Rational.PlusInfinity;
            }

            var fCut = f.Cut(0, cutEnd, isEndIncluded: false, settings: settings);
            var gCut = g.Cut(0, cutEnd, isEndIncluded: false, settings: settings);
            var sequence = Sequence.Convolution(fCut, gCut, settings, cutEnd, cutCeiling, useIsomorphism: useIsomorphism);

            var result = new Curve(
                baseSequence: sequence.Cut(0, cutEnd),
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );
            if (settings.UseRepresentationMinimization)
                result = result.Optimize();
            return result;
        }

        // Computes a partial convolution term, that is the convolution of two transient parts.
        // Described in [BT08] Section 4.4, point 3
        Curve ConvolutionTransientTransient(
            Curve firstTransientCurve,
            Curve secondTransientCurve)
        {
            var firstTransientSequence = firstTransientCurve.TransientElements.ToSequence();
            var secondTransientSequence = secondTransientCurve.TransientElements.ToSequence();

            #if DO_LOG
            logger.Trace("Convolution: transient x transient");
            #endif
            var convolution = Sequence.Convolution(firstTransientSequence, secondTransientSequence, settings);

            //Has no actual meaning
            Rational d = 1;
            var T = f.PseudoPeriodStart + g.PseudoPeriodStart;

            var extendedConvolution = new Sequence(
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

            return settings.UseRepresentationMinimization ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of a transient part and a pseudo-periodic one.
        // Described in [BT08] Section 4.4, points 4 and 5
        Curve ConvolutionTransientPeriodic(
            Curve transientCurve,
            Curve periodicCurve)
        {
            Rational T = transientCurve.PseudoPeriodStart + periodicCurve.PseudoPeriodStart;
            Rational d = periodicCurve.PseudoPeriodLength;
            Rational c = periodicCurve.PseudoPeriodHeight;

            var transientSequence = transientCurve.TransientElements.ToSequence();
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

            return settings.UseRepresentationMinimization ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of two pseudo-periodic parts.
        // Described in [BT08] Section 4.4, point 6
        Curve ConvolutionPeriodicPeriodic(
            Curve f,
            Curve g)
        {
            if (f.IsUltimatelyAffine || g.IsUltimatelyAffine)
            {
                var d = f.IsUltimatelyAffine ? g.PseudoPeriodLength : f.PseudoPeriodLength;
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                var fCut = f.Cut(tf, tf + 2*d, settings: settings);
                var gCut = g.Cut(tg, tg + 2*d, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                logger.Trace(
                    $"Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                logger.Trace("Convolution: periodic x periodic UA");
                #endif

                var cutEnd = T + d;
                Sequence limitedConvolution = Sequence.Convolution(fCut, gCut, settings, cutEnd);

                var result = new Curve(
                    baseSequence: limitedConvolution.Cut(tf + tg, cutEnd),
                    pseudoPeriodStart: T,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c,
                    isPartialCurve: true
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
            else if (settings.UseConvolutionIsospeedOptimization &&
                 f.IsLeftContinuousOverInterval(f.PseudoPeriodStart) && g.IsLeftContinuousOverInterval(g.PseudoPeriodStart) &&
                 f.IsNonDecreasingOverInterval(f.PseudoPeriodStart) && g.IsNonDecreasingOverInterval(g.PseudoPeriodStart)
            )
            {
                // super-isospeed algorithm, discussed in [TBP]
                var d_f = f.PseudoPeriodLength;
                var d_g = g.PseudoPeriodLength;
                var lcm_d = Rational.LeastCommonMultiple(d_f, d_g);
                var k_d_f = lcm_d / d_f;
                var k_d_g = lcm_d / d_g;

                var c_f = f.PseudoPeriodHeight;
                var c_g = g.PseudoPeriodHeight;
                var lcm_c = Rational.LeastCommonMultiple(c_f, c_g);
                var k_c_f = lcm_c / c_f;
                var k_c_g = lcm_c / c_g;

                var d = settings.UseConvolutionSuperIsospeedOptimization ?
                    (k_c_f * d_f > k_c_g * d_g ?
                        Rational.GreatestCommonDivisor(k_d_f, k_c_f) * d_f :
                        Rational.GreatestCommonDivisor(k_d_g, k_c_g) * d_g) :
                    Rational.Min(lcm_d, Rational.Max(k_c_f * d_f, k_c_g * d_g));
                var c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + lcm_d;

                #if DO_LOG
                logger.Trace(
                    $"Convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fSegmentAfterTf = f.GetSegmentAfter(tf);
                var tf_prime = (f.IsRightContinuousAt(tf) && fSegmentAfterTf.IsConstant) ? fSegmentAfterTf.EndTime : tf;
                var fCutEnd_minp = tf + lcm_d + d;
                var fCutEnd_iso = tf_prime + 2 * k_c_f * d_f;
                var fCut = fCutEnd_minp <= fCutEnd_iso
                    ? f.Cut(tf, fCutEnd_minp, isEndIncluded: false, settings: settings)
                    : f.Cut(tf, fCutEnd_iso, isEndIncluded: true, settings: settings);

                var gSegmentAfterTg = g.GetSegmentAfter(tg);
                var tg_prime = (g.IsRightContinuousAt(tg) && gSegmentAfterTg.IsConstant) ? gSegmentAfterTg.EndTime : tg;
                var gCutEnd_minp = tg + lcm_d + d;
                var gCutEnd_iso = tg_prime + 2 * k_c_g * d_g;
                var gCut = gCutEnd_minp <= gCutEnd_iso
                   ? g.Cut(tg, gCutEnd_minp, isEndIncluded: false, settings: settings)
                   : g.Cut(tg, gCutEnd_iso, isEndIncluded: true, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Convolution: periodic x periodic isom.");
                #endif
                var cutEnd = tf + tg + lcm_d + d;
                var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;
                var sequence = Sequence.Convolution(fCut, gCut, settings, cutEnd, cutCeiling, useIsomorphism: true);

                var resultEnd = sequence.Elements.Last(e => e.IsFinite).EndTime;
                var resultT = resultEnd - d;
                var minT = Rational.Min(T, resultT);

                var baseSequence = sequence.Elements
                    .Cut(tf + tg, minT + d)
                    .Fill(0, minT + d, fillWith: Rational.PlusInfinity)
                    .ToSequence();

                var result = new Curve(
                    baseSequence: baseSequence,
                    pseudoPeriodStart: minT,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
            else
            {
                // Base algorithm described in [BT08] Section 4.4, point 6
                Rational d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                #if DO_LOG
                logger.Trace(
                    $"Convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCutEnd = tf + 2*d;
                var gCutEnd = tg + 2*d;
                var fCut = f.Cut(tf, fCutEnd, isEndIncluded: false, settings: settings);
                var gCut = g.Cut(tg, gCutEnd, isEndIncluded: false, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Convolution: periodic x periodic");
                #endif
                var cutEnd = T + d;
                var limitedConvolution = Sequence.Convolution(fCut, gCut, settings, cutEnd);

                var result = new Curve(
                    baseSequence: limitedConvolution.Cut(tf + tg, cutEnd),
                    pseudoPeriodStart: T,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c,
                    isPartialCurve: true
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
        }
    }

    /// <summary>
    /// Computes the convolution of two curves, $a \otimes b$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the convolution.</returns>
    /// <remarks>Described in [BT08] Section 4.4</remarks>
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

    /// <exclude />
    /// <summary>
    /// Returns all element pairs whose convolution contribute to the value of $f \otimes g$ at the given time.
    /// Useful for debugging purposes.
    /// </summary>
    /// <param name="f">First operand of the (min,+) convolution.</param>
    /// <param name="g">Second operand of the (min,+) convolution.</param>
    /// <param name="time">The time of sampling of $f \otimes g$.</param>
    /// <param name="settings"></param>
    /// <remarks>
    /// The convolution is computed using the given <paramref name="settings"/>, and may exploit any specialized algorithm provided.
    /// The element pairs, however, are searched using all elements of $f$ and $g$ between 0 and time, i.e., the search does not use smart cuts.
    ///
    /// If the result of the convolution is mathematically wrong, this method will provide the pairs that are considered for that result and,
    /// more importantly, will not include the pairs that should have instead been considered for the correct result.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<(Element, Element)> TraceConvolution(Curve f, Curve g, Rational time, ComputationSettings? settings = null)
    {
        if (time < 0 || time.IsPlusInfinite)
            throw new ArgumentException("Time must be non-negative and finite.");

        var h = Convolution(f, g, settings);
        var h_t = h.ValueAt(time);

        // for simplicity, just take everything
        var fCut = f.CutAsEnumerable(0, f.GetSegmentAfter(time).EndTime, isEndIncluded: true);
        var gCut = g.CutAsEnumerable(0, g.GetSegmentAfter(time).EndTime, isEndIncluded: true);

        var convolutionPairs = fCut.SelectMany(e_f =>
                gCut.Select(e_g => (e_f, e_g))
        );
        var relevantPairs = convolutionPairs
            .Where(pair =>
            {
                if (pair.e_f is Point pf && pair.e_g is Point pg)
                {
                    return pf.Time + pg.Time == time;
                }
                else
                {
                    return pair.e_f.StartTime + pair.e_g.StartTime < time &&
                        pair.e_f.EndTime + pair.e_g.EndTime > time;
                }
            });
        var exactValuePairs = relevantPairs
            .Where(pair =>
            {
                var c = pair.e_f.Convolution(pair.e_g).ToSequence();
                return c.ValueAt(time) == h_t;
            });

        return exactValuePairs;
    }

    #endregion Convolution operator

    #region EstimateConvolution

    // todo: implement horizontal and vertical filtering in EstimateConvolution

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the (min,+) convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the (min,+) convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public virtual long EstimateConvolution(Curve curve, bool countElements = false, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var f = this;
        var g = curve;

        //Checks for convolution with infinite curves
        if (f.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return 0;
        if (g.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return 0;

        //Checks for convolution with null curves
        if (f.IsZero || g.IsZero)
            return 0;

        #if DO_LOG
        logger.Trace($"Estimating convolution of f ({f.BaseSequence.Count} elements, T: {f.PseudoPeriodStart} d: {f.PseudoPeriodLength})" +
                     $" and g ({g.BaseSequence.Count} elements, T: {g.PseudoPeriodStart} d: {g.PseudoPeriodLength})");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"f1:\n {a} \n f2:\n {b}");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        long result;
        if (settings.SinglePassConvolution && f.PseudoPeriodSlope == g.PseudoPeriodSlope)
        {
            result = EstimateSinglePassConvolution();
        }
        else
        {
            var terms = new List<long>();
            if (
                !settings.SinglePassConvolution &&  // if true, checking for equivalence is useless due to the check above
                Equivalent(f, g, settings)
            )
            {
                // self convolution: skip duplicate middle term
                if (f.HasTransient)
                    terms.Add(EstimateConvolutionTransientTransient(f, f));

                if (f.HasTransient && !f.IsUltimatelyInfinite)
                    terms.Add(EstimateConvolutionTransientPeriodic(f, f));

                if (!f.IsUltimatelyInfinite)
                    terms.Add(EstimateConvolutionPeriodicPeriodic(f, f));
            }
            else
            {
                if (f.HasTransient)
                {
                    if (g.HasTransient)
                        terms.Add(EstimateConvolutionTransientTransient(f, g));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(EstimateConvolutionTransientPeriodic(f, g));
                }

                if (!f.IsUltimatelyInfinite)
                {
                    if (g.HasTransient)
                        terms.Add(EstimateConvolutionTransientPeriodic(g, f));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(EstimateConvolutionPeriodicPeriodic(f, g));
                }
            }
            result = terms.Sum();
        }

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Estimate convolution: took {timer.Elapsed}; f {f.BaseSequence.Count} g {g.BaseSequence.Count} => [{countElements}] {result}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n f: {f} \n g: {g} \n result: {result}");
        #endif
        return result;

        // Computes the convolution in a single operation,
        // since all UPP parameters (in particular, T) can be determined a priori
        long EstimateSinglePassConvolution()
        {
            #if DO_LOG
            logger.Trace("Convolution: same slope, single pass");
            #endif
            // As discussed in [TBP], there is no improvement on the UPP parameters to be gained using isomorphisms.
            // The optimization lies instead in the use of a vertical filter (cutCeiling), in addition to the horizontal one (cutEnd)

            var d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var T = f.PseudoPeriodStart + g.PseudoPeriodStart + d;
            var c = f.PseudoPeriodSlope * d;

            var cutEnd = T + d;
            Rational cutCeiling;
            if (
                settings.UseConvolutionIsospeedOptimization &&
                f.IsLeftContinuousOverInterval(f.FirstFiniteTime) && g.IsLeftContinuousOverInterval(g.FirstFiniteTime) &&
                f.IsNonDecreasingOverInterval(f.FirstFiniteTime) && g.IsNonDecreasingOverInterval(g.FirstFiniteTime) &&
                !f.IsUltimatelyConstant && !g.IsUltimatelyConstant
            )
            {
                var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
                cutCeiling = f.ValueAt(f.PseudoPeriodStart) + g.ValueAt(g.PseudoPeriodStart) + 2 * lcm_c;
            }
            else
            {
                cutCeiling = Rational.PlusInfinity;
            }

            var aCut = f.Cut(0, cutEnd, settings: settings);
            var bCut = g.Cut(0, cutEnd, settings: settings);
            var result = Sequence.EstimateConvolution(aCut, bCut, settings, cutEnd, cutCeiling, countElements);
            return result;
        }

        // Computes a partial convolution term, that is the convolution of two transient parts.
        // Described in [BT08] Section 4.4, point 3
        long EstimateConvolutionTransientTransient(
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
        // Described in [BT08] Section 4.4, points 4 and 5
        long EstimateConvolutionTransientPeriodic(
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
        // Described in [BT08] Section 4.4, point 6
        long EstimateConvolutionPeriodicPeriodic(
            Curve f,
            Curve g)
        {
            if (f.IsUltimatelyAffine || g.IsUltimatelyAffine)
            {
                var d = f.IsUltimatelyAffine ? g.PseudoPeriodLength : f.PseudoPeriodLength;
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                #if DO_LOG
                logger.Trace($"Estimate convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCut = f.Cut(tf, tf + 2*d, settings: settings);
                var gCut = g.Cut(tg, tg + 2*d, settings: settings);

                #if DO_LOG
                logger.Trace($"Estimate convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                #if DO_LOG
                logger.Trace("Estimate convolution: periodic x periodic UA");
                #endif
                var cutEnd = T + d;
                var result = Sequence.EstimateConvolution(fCut, gCut, settings, cutEnd, countElements: countElements);

                return result;
            }
            else if (settings.UseConvolutionIsospeedOptimization &&
                 f.IsLeftContinuousOverInterval(f.PseudoPeriodStart) && g.IsLeftContinuousOverInterval(g.PseudoPeriodStart) &&
                 f.IsNonDecreasingOverInterval(f.PseudoPeriodStart) && g.IsNonDecreasingOverInterval(g.PseudoPeriodStart)
            )
            {
                // todo: update this estimate with super-isospeed [TBP]

                // Optimized algorithm discussed in [ZNS23a]
                var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
                var k_c_f = lcm_c / f.PseudoPeriodHeight;
                var k_c_g = lcm_c / g.PseudoPeriodHeight;
                var lcm_d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
                var d = Rational.Min(lcm_d, Rational.Max(k_c_f * f.PseudoPeriodLength, k_c_g * g.PseudoPeriodLength));
                var c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + lcm_d;

                #if DO_LOG
                logger.Trace(
                    $"Estimate convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fSegmentAfterTf = f.GetSegmentAfter(tf);
                var tf_prime = (f.IsRightContinuousAt(tf) && fSegmentAfterTf.IsConstant) ? fSegmentAfterTf.EndTime : tf;
                var fCutEnd_minp = tf + 2 * lcm_d;
                var fCutEnd_iso = tf_prime + 2 * k_c_f * f.PseudoPeriodLength;
                var fCut = fCutEnd_minp <= fCutEnd_iso
                    ? f.Cut(tf, fCutEnd_minp, isEndIncluded: false, settings: settings)
                    : f.Cut(tf, fCutEnd_iso, isEndIncluded: true, settings: settings);

                var gSegmentAfterTg = g.GetSegmentAfter(tg);
                var tg_prime = (g.IsRightContinuousAt(tg) && gSegmentAfterTg.IsConstant) ? gSegmentAfterTg.EndTime : tg;
                var gCutEnd_minp = tg + 2 * lcm_d;
                var gCutEnd_iso = tg_prime + 2 * k_c_g * g.PseudoPeriodLength;
                var gCut = gCutEnd_minp <= gCutEnd_iso
                   ? g.Cut(tg, gCutEnd_minp, isEndIncluded: false, settings: settings)
                   : g.Cut(tg, gCutEnd_iso, isEndIncluded: true, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Estimate convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Estimate convolution: periodic x periodic isom.");
                #endif
                var cutEnd = tf + tg + lcm_d + d;
                var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;
                var result = Sequence.EstimateConvolution(fCut, gCut, settings, cutEnd, cutCeiling, countElements: countElements);

                return result;
            }
            else
            {
                // Base algorithm described in [BT08]
                Rational d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Min(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                #if DO_LOG
                logger.Trace(
                    $"Estimate convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCutEnd = tf + 2*d;
                var gCutEnd = tg + 2*d;
                var fCut = f.Cut(tf, fCutEnd, isEndIncluded: false, settings: settings);
                var gCut = g.Cut(tg, gCutEnd, isEndIncluded: false, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Estimate convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Estimate convolution: periodic x periodic");
                #endif
                var cutEnd = T + d;
                var result = Sequence.EstimateConvolution(fCut, gCut, settings, cutEnd, countElements: countElements);

                return result;
            }
        }
    }

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the (min,+) convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="f">First operand.</param>
    /// <param name="g">Second operand.</param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the (min,+) convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public static long EstimateConvolution(Curve f, Curve g, bool countElements = false, ComputationSettings? settings = null)
        => f.EstimateConvolution(g, countElements, settings);

    // todo: support isospeed in EstimateMaxPlusConvolution

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the (max,+) convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="curve">Second operand.</param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the (max,+) convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public virtual long EstimateMaxPlusConvolution(
        Curve curve,
        bool countElements = false,
        ComputationSettings? settings = null
    )
        => Curve.EstimateMaxPlusConvolution(this, curve, countElements, settings);

    /// <summary>
    /// Computes the number of elementary convolutions involved in computing the (max,+) convolution of the two curves,
    /// avoiding allocations as much as possible.
    /// </summary>
    /// <param name="f">First operand.</param>
    /// <param name="g">Second operand.</param>
    /// <param name="countElements">
    /// If true, instead of counting only how many convolutions are done, it counts how many convolution elements are produced.
    /// </param>
    /// <param name="settings"></param>
    /// <returns>
    /// The number of elementary convolutions involved in computing the result of the (max,+) convolution,
    /// or the number of elements resulting from these convolutions if <paramref name="countElements"/> is true.
    /// </returns>
    public static long EstimateMaxPlusConvolution(Curve f, Curve g, bool countElements = false, ComputationSettings? settings = null)
        => Curve.EstimateConvolution(-f, -g, countElements, settings);

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
    /// Described in [BT08] Section 4.5 .
    /// </remarks>
    public virtual Curve Deconvolution(Curve curve, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();

        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var f = this;
        var g = curve;

        // Checks for deconvolution with infinite operands or result
        if (f.PseudoPeriodSlope > g.PseudoPeriodSlope)
        {
            return PlusInfinite();
        }
        if(f.IsFinite)
        {
            if(g.IsPlusInfinite)
                return MinusInfinite();
            if(g.IsMinusInfinite)
                return PlusInfinite();
        }

        Rational T = Rational.Max(f.PseudoPeriodStart, g.PseudoPeriodStart) + Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);

        Sequence fCut = f.Cut(0, T + FirstPseudoPeriodEnd, settings: settings);
        Sequence gCut = g.Cut(0, T, settings: settings);

        var cutEnd = f.FirstPseudoPeriodEnd;
        var cutDeconvolution = Sequence.Deconvolution(fCut, gCut, 0, cutEnd, settings).Optimize();

        return new Curve(
            baseSequence: cutDeconvolution,
            pseudoPeriodStart: f.PseudoPeriodStart,
            pseudoPeriodLength: f.PseudoPeriodLength,
            pseudoPeriodHeight: f.PseudoPeriodHeight
        );
    }

    /// <summary>
    /// Computes the deconvolution of two curves, $a \oslash b$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the deconvolution</returns>
    /// <remarks>
    /// The result is not forced to have $f(0) = 0$, see <see cref="WithZeroOrigin"/> to have this property.
    /// Described in [BT08] Section 4.5 .
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
    /// <remarks>Described in [BT08] Section 4.6 as algorithm 5</remarks>
    public static SubAdditiveCurve SubAdditiveClosure(Curve curve, ComputationSettings? settings = null)
        => curve.SubAdditiveClosure(settings);

    /// <summary>
    /// Computes the sub-additive closure of the curve.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>The result of the sub-additive closure.</returns>
    /// <remarks>Described in [BT08] Section 4.6 as algorithm 5</remarks>
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
    /// Computes the max-plus convolution of two curves, $f \overline{\otimes} g$.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="settings"></param>
    /// <returns>The curve resulting from the max-plus convolution.</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT08] Section 4.4</remarks>
    public virtual Curve MaxPlusConvolution(Curve curve, ComputationSettings? settings = null)
    {
        #if MAX_CONV_AS_NEGATIVE_MIN_CONV
        return -Convolution(-this, -curve, settings);
        #else

        settings ??= ComputationSettings.Default();

        //The instance method is implemented to allow overriding
        //Renaming for symmetry
        var f = this;
        var g = curve;

        //Checks for convolution with infinite curves
        if (f.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return g.VerticalShift(f.ValueAt(0), false);
        if (g.FirstFiniteTimeExceptOrigin == Rational.PlusInfinity)
            return f.VerticalShift(g.ValueAt(0), false);

        #if DO_LOG
        logger.Trace($"Computing max-plus convolution of f1 ({f.BaseSequence.Count} elements, T: {f.PseudoPeriodStart} d: {f.PseudoPeriodLength})" +
                     $" and f2 ({g.BaseSequence.Count} elements, T: {g.PseudoPeriodStart} d: {g.PseudoPeriodLength})");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"f1:\n {a} \n f2:\n {b}");
        #endif

        #if DO_LOG
        var timer = Stopwatch.StartNew();
        #endif

        Curve result;
        if (settings.SinglePassConvolution && f.PseudoPeriodSlope == g.PseudoPeriodSlope)
        {
            result = SinglePassMaxPlusConvolution();
        }
        else
        {
            var terms = new List<Curve>();
            if (
                !settings.SinglePassConvolution &&  // if true, checking for equivalence is useless due to the check above
                Equivalent(f, g, settings)
            )
            {
                // self convolution: skip duplicate middle term
                if (f.HasTransient)
                    terms.Add(MaxPlusConvolutionTransientTransient(f, f));

                if (f.HasTransient && !f.IsUltimatelyInfinite)
                    terms.Add(MaxPlusConvolutionTransientPeriodic(f, f));

                if (!f.IsUltimatelyInfinite)
                    terms.Add(MaxPlusConvolutionPeriodicPeriodic(f, f));
            }
            else
            {
                if (f.HasTransient)
                {
                    if (g.HasTransient)
                        terms.Add(MaxPlusConvolutionTransientTransient(f, g));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(MaxPlusConvolutionTransientPeriodic(f, g));
                }

                if (!f.IsUltimatelyInfinite)
                {
                    if (g.HasTransient)
                        terms.Add(MaxPlusConvolutionTransientPeriodic(g, f));
                    if (!g.IsUltimatelyInfinite)
                        terms.Add(MaxPlusConvolutionPeriodicPeriodic(f, g));
                }
            }
            result = Maximum(terms, settings);
        }

        #if DO_LOG
        timer.Stop();
        logger.Debug($"Max-plus Convolution: took {timer.Elapsed}; a {f.BaseSequence.Count} b {g.BaseSequence.Count} => {result.BaseSequence.Count}");
        #endif
        #if DO_LOG && DO_COSTLY_LOGS
        logger.Trace($"Json\n a: {a} \n b: {b} \n result: {result}");
        #endif
        return result;

        // Computes the convolution in a single operation,
        // since all UPP parameters (in particular, T) can be determined a priori
        Curve SinglePassMaxPlusConvolution()
        {
            #if DO_LOG
            logger.Trace("Max-plus Convolution: same slope, single pass");
            #endif
            // From rho_f = rho_g it follows that k_d_f = k_c_f.
            // Hence, there is no improvement on the UPP parameters to be gained using isomorphisms.
            // The optimization lies instead in the use of vertical filtering, and the by-sequence heuristic

            var d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
            var T = f.PseudoPeriodStart + g.PseudoPeriodStart + d;
            var c = f.PseudoPeriodSlope * d;

            var cutEnd = T + d;
            bool useIsomorphism = settings.UseConvolutionIsospeedOptimization &&
                                  f.IsRightContinuous && g.IsRightContinuous &&
                                  f.IsNonDecreasing && g.IsNonDecreasing &&
                                  !f.IsUltimatelyConstant && !g.IsUltimatelyConstant;
            Rational cutCeiling;
            if (useIsomorphism)
            {
                // Vertical filtering optimization, discussed in [ZNS23a]
                var lcm_c = Rational.LeastCommonMultiple(f.PseudoPeriodHeight, g.PseudoPeriodHeight);
                #if false
                // expression as in theory
                var tstar_f = f.LowerPseudoInverseOverInterval(f.PseudoPeriodStart)
                    .ValueAt(f.ValueAt(f.PseudoPeriodStart) + f.PseudoPeriodHeight);
                var tstar_g = g.LowerPseudoInverseOverInterval(g.PseudoPeriodStart)
                    .ValueAt(g.ValueAt(g.PseudoPeriodStart) + g.PseudoPeriodHeight);
                #else
                // more efficient expression
                var tstar_f = f
                    .Cut(f.PseudoPeriodStart, f.FirstPseudoPeriodEnd, isEndIncluded: true)
                    .LastPlateauStart;
                var tstar_g = g
                    .Cut(g.PseudoPeriodStart, g.FirstPseudoPeriodEnd, isEndIncluded: true)
                    .LastPlateauStart;
                #endif
                var T_f_lpi = tstar_f < f.FirstPseudoPeriodEnd ? f.ValueAt(f.FirstPseudoPeriodEnd) : f.ValueAt(f.PseudoPeriodStart);
                var T_g_lpi = tstar_g < g.FirstPseudoPeriodEnd ? g.ValueAt(g.FirstPseudoPeriodEnd) : g.ValueAt(g.PseudoPeriodStart);
                cutCeiling = T_f_lpi + T_g_lpi + 2 * lcm_c;
            }
            else
            {
                cutCeiling = Rational.PlusInfinity;
            }

            var fCut = f.Cut(0, cutEnd, isEndIncluded: false, settings: settings);
            var gCut = g.Cut(0, cutEnd, isEndIncluded: false, settings: settings);
            var sequence = Sequence.MaxPlusConvolution(fCut, gCut, settings, cutEnd, cutCeiling, useIsomorphism: useIsomorphism);

            var resultEnd = sequence.Elements.Last(e => e.IsFinite).EndTime;
            var resultT = resultEnd - d;
            var minT = Rational.Min(T, resultT);

            var result = new Curve(
                baseSequence: sequence.Cut(0, minT + d),
                pseudoPeriodStart: minT,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );
            if (settings.UseRepresentationMinimization)
                result = result.Optimize();
            return result;
        }

        // Computes a partial convolution term, that is the convolution of two transient parts.
        // Described in [BT08] Section 4.4, point 3
        Curve MaxPlusConvolutionTransientTransient(
            Curve firstTransientCurve,
            Curve secondTransientCurve)
        {
            var firstTransientSequence = firstTransientCurve.TransientElements.ToSequence();
            var secondTransientSequence = secondTransientCurve.TransientElements.ToSequence();

            #if DO_LOG
            logger.Trace("Max-plus Convolution: transient x transient");
            #endif
            var convolution = Sequence.MaxPlusConvolution(firstTransientSequence, secondTransientSequence, settings);

            //Has no actual meaning
            Rational d = 1;
            var T = f.PseudoPeriodStart + g.PseudoPeriodStart;

            var extendedConvolution = new Sequence(
                elements: convolution.Elements,
                fillFrom: 0,
                fillTo: T + d,
                fillWith: Rational.MinusInfinity
            );

            var result = new Curve(
                baseSequence: extendedConvolution,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: Rational.MinusInfinity
            );

            return settings.UseRepresentationMinimization ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of a transient part and a pseudo-periodic one.
        // Described in [BT08] Section 4.4, points 4 and 5
        Curve MaxPlusConvolutionTransientPeriodic(
            Curve transientCurve,
            Curve periodicCurve)
        {
            Rational T = transientCurve.PseudoPeriodStart + periodicCurve.PseudoPeriodStart;
            Rational d = periodicCurve.PseudoPeriodLength;
            Rational c = periodicCurve.PseudoPeriodHeight;

            var transientSequence = transientCurve.TransientElements.ToSequence();
            var periodicSequence = periodicCurve.Cut(periodicCurve.PseudoPeriodStart, T + d, settings: settings);

            #if DO_LOG
            logger.Trace("Max-plus Convolution: transient x periodic");
            #endif
            var limitedConvolution = Sequence.MaxPlusConvolution(transientSequence, periodicSequence, settings);

            var sequence = limitedConvolution
                .Elements
                .Cut(periodicCurve.PseudoPeriodStart, T + d)
                .Fill(0, T + d, fillWith: Rational.MinusInfinity)
                .ToSequence();

            var result = new Curve(
                baseSequence: sequence,
                pseudoPeriodStart: T,
                pseudoPeriodLength: d,
                pseudoPeriodHeight: c
            );

            return settings.UseRepresentationMinimization ? result.Optimize() : result;
        }

        // Computes a partial convolution term, that is the convolution of two pseudo-periodic parts.
        // Described in [BT08] Section 4.4, point 6
        Curve MaxPlusConvolutionPeriodicPeriodic(
            Curve f,
            Curve g)
        {
            if (f.IsUltimatelyAffine || g.IsUltimatelyAffine)
            {
                var d = f.IsUltimatelyAffine ? g.PseudoPeriodLength : f.PseudoPeriodLength;
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Max(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCut = f.Cut(tf, tf + 2*d, settings: settings);
                var gCut = g.Cut(tg, tg + 2*d, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Max-plus Convolution: periodic x periodic UA");
                #endif
                var cutEnd = T + d;
                Sequence limitedConvolution = Sequence.MaxPlusConvolution(fCut, gCut, settings, cutEnd);
                var sequence = limitedConvolution.Elements
                    .Cut(tf + tg, cutEnd)
                    .Fill(0, cutEnd, fillWith: Rational.MinusInfinity)
                    .ToSequence();

                var result = new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: T,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
            else if (settings.UseConvolutionIsospeedOptimization &&
                 f.IsRightContinuousOverInterval(f.PseudoPeriodStart) && g.IsRightContinuousOverInterval(g.PseudoPeriodStart) &&
                 f.IsNonDecreasingOverInterval(f.PseudoPeriodStart) && g.IsNonDecreasingOverInterval(g.PseudoPeriodStart)
            )
            {
                // todo: fill in references
                // super-isospeed algorithm discussed in [TBP]

                // Check for Lemma X in [TBP]
                #if false
                // expression as in theory
                var tstar_f = f.LowerPseudoInverseOverInterval(f.PseudoPeriodStart)
                    .ValueAt(f.ValueAt(f.PseudoPeriodStart) + f.PseudoPeriodHeight);
                var tstar_g = g.LowerPseudoInverseOverInterval(g.PseudoPeriodStart)
                    .ValueAt(g.ValueAt(g.PseudoPeriodStart) + g.PseudoPeriodHeight);
                #else
                // more efficient expression
                var tstar_f = f
                    .Cut(f.PseudoPeriodStart, f.FirstPseudoPeriodEnd, isEndIncluded: true)
                    .LastPlateauStart;
                var tstar_g = g
                    .Cut(g.PseudoPeriodStart, g.FirstPseudoPeriodEnd, isEndIncluded: true)
                    .LastPlateauStart;
                #endif

                if (tstar_f < f.FirstPseudoPeriodEnd ||
                    tstar_g < g.FirstPseudoPeriodEnd)
                {
                    // todo: fill in reference
                    // If Lemma X does not apply, workaround according to Remark Y in [TBP]
                    var fpstarCut = f
                        .CutAsEnumerable(f.PseudoPeriodStart, tstar_f + f.PseudoPeriodLength)
                        .Fill(0, f.PseudoPeriodStart, fillWith: Rational.MinusInfinity)
                        .ToSequence();
                    var fpstar = new Curve(
                        fpstarCut,
                        tstar_f,
                        f.PseudoPeriodLength,
                        f.PseudoPeriodHeight
                    );

                    var gpstarCut = g
                        .CutAsEnumerable(g.PseudoPeriodStart, tstar_g + g.PseudoPeriodLength)
                        .Fill(0, g.PseudoPeriodStart, fillWith: Rational.MinusInfinity)
                        .ToSequence();
                    var gpstar = new Curve(
                        gpstarCut,
                        tstar_g,
                        g.PseudoPeriodLength,
                        g.PseudoPeriodHeight
                    );
                    return MaxPlusConvolution(fpstar, gpstar, settings);
                }

                var d_f = f.PseudoPeriodLength;
                var d_g = g.PseudoPeriodLength;
                var lcm_d = Rational.LeastCommonMultiple(d_f, d_g);
                var k_d_f = lcm_d / d_f;
                var k_d_g = lcm_d / d_g;

                var c_f = f.PseudoPeriodHeight;
                var c_g = g.PseudoPeriodHeight;
                var lcm_c = Rational.LeastCommonMultiple(c_f, c_g);
                var k_c_f = lcm_c / c_f;
                var k_c_g = lcm_c / c_g;

                var d = settings.UseConvolutionSuperIsospeedOptimization ?
                    (k_c_f * d_f < k_c_g * d_g ?
                        Rational.GreatestCommonDivisor(k_d_f, k_c_f) * d_f :
                        Rational.GreatestCommonDivisor(k_d_g, k_c_g) * d_g) :
                    Rational.Min(lcm_d, Rational.Min(k_c_f * d_f, k_c_g * d_g));
                var c = d * Rational.Max(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + lcm_d;

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from Tf {tf} df {f.PseudoPeriodLength}  Tg {tg} dg {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCutEnd_direct = tf + lcm_d + d;
                var fCutEnd_iso = tf + 2 * k_c_f * d_f;
                var fCut = fCutEnd_direct <= fCutEnd_iso
                    ? f.Cut(tf, fCutEnd_direct, isEndIncluded: false, settings: settings)
                    : f.Cut(tf, fCutEnd_iso, isEndIncluded: true, settings: settings);

                var gCutEnd_direct = tg + lcm_d + d;
                var gCutEnd_iso = tg + 2 * k_c_g * d_g;
                var gCut = gCutEnd_direct <= gCutEnd_iso
                    ? g.Cut(tg, gCutEnd_direct, isEndIncluded: false, settings: settings)
                    : g.Cut(tg, gCutEnd_iso, isEndIncluded: true, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Max-plus Convolution: periodic x periodic isom.");
                #endif
                var cutEnd = tf + tg + lcm_d + d;
                var cutCeiling = f.ValueAt(tf) + g.ValueAt(tg) + 2 * lcm_c;
                var sequence = Sequence.MaxPlusConvolution(fCut, gCut, settings, cutEnd, cutCeiling, useIsomorphism: true);

                var resultEnd = sequence.Elements.Last(e => e.IsFinite).EndTime;
                var resultT = resultEnd - d;
                var minT = Rational.Min(T, resultT);

                var baseSequence = sequence.Elements
                    .Cut(tf + tg, minT + d)
                    .Fill(0, minT + d, fillWith: Rational.MinusInfinity)
                    .ToSequence();

                var result = new Curve(
                    baseSequence: baseSequence,
                    pseudoPeriodStart: minT,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
            else
            {
                // Base algorithm adapted from [BT08] Section 4.4, point 6
                Rational d = Rational.LeastCommonMultiple(f.PseudoPeriodLength, g.PseudoPeriodLength);
                var tf = f.PseudoPeriodStart;
                var tg = g.PseudoPeriodStart;
                var T = tf + tg + d;
                Rational c = d * Rational.Max(f.PseudoPeriodSlope, g.PseudoPeriodSlope);

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from T1 {tf} d1 {f.PseudoPeriodLength}  T2 {tg} d2 {g.PseudoPeriodLength} to T {T} d {d}");
                #endif

                var fCutEnd = tf + 2*d;
                var gCutEnd = tg + 2*d;
                var fCut = f.Cut(tf, fCutEnd, isEndIncluded: false, settings: settings);
                var gCut = g.Cut(tg, gCutEnd, isEndIncluded: false, settings: settings);

                #if DO_LOG
                logger.Trace(
                    $"Max-plus Convolution: extending from {f.PseudoPeriodicSequence.Count} and {g.PseudoPeriodicSequence.Count} to {fCut.Count} and {gCut.Count}");
                #endif

                #if DO_LOG
                logger.Trace("Max-plus Convolution: periodic x periodic");
                #endif
                var cutEnd = T + d;
                var limitedConvolution = Sequence.MaxPlusConvolution(fCut, gCut, settings, cutEnd);
                var sequence = limitedConvolution.Elements
                    .Cut(tf + tg, cutEnd)
                    .Fill(0, cutEnd, fillWith: Rational.MinusInfinity)
                    .ToSequence();

                var result = new Curve(
                    baseSequence: sequence,
                    pseudoPeriodStart: T,
                    pseudoPeriodLength: d,
                    pseudoPeriodHeight: c
                );

                return settings.UseRepresentationMinimization ? result.Optimize() : result;
            }
        }
        #endif
    }

    /// <summary>
    /// Computes the max-plus convolution of two curves, $f \overline{\otimes} g$.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <param name="settings"></param>
    /// <returns>The result of the max-plus convolution</returns>
    /// <remarks>Adapted from the min-plus convolution algorithm described in [BT08] Section 4.4</remarks>
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

    /// <exclude />
    /// <summary>
    /// Returns all element pairs whose convolution contribute to the value of $f \overline{\otimes} g$ at the given time.
    /// Useful for debugging purposes.
    /// </summary>
    /// <param name="f">First operand of the (max,+) convolution.</param>
    /// <param name="g">Second operand of the (max,+) convolution.</param>
    /// <param name="time">The time of sampling of $f \overline{\otimes} g$.</param>
    /// <param name="settings"></param>
    /// <remarks>
    /// The convolution is computed using the given <paramref name="settings"/>, and may exploit any specialized algorithm provided.
    /// The element pairs, however, are searched using all elements of $f$ and $g$ between 0 and time, i.e., the search does not use smart cuts.
    ///
    /// If the result of the convolution is mathematically wrong, this method will provide the pairs that are considered for that result and,
    /// more importantly, will not include the pairs that should have instead been considered for the correct result.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<(Element, Element)> TraceMaxPlusConvolution(Curve f, Curve g, Rational time, ComputationSettings? settings = null)
    {
        if (time < 0 || time.IsPlusInfinite)
            throw new ArgumentException("Time must be non-negative and finite.");

        var h = MaxPlusConvolution(f, g, settings);
        var h_t = h.ValueAt(time);

        // for simplicity, just take everything
        var fCut = f.CutAsEnumerable(0, f.GetSegmentAfter(time).EndTime, isEndIncluded: true);
        var gCut = g.CutAsEnumerable(0, g.GetSegmentAfter(time).EndTime, isEndIncluded: true);

        var convolutionPairs = fCut.SelectMany(e_f =>
            gCut.Select(e_g => (e_f, e_g))
        );
        var relevantPairs = convolutionPairs
            .Where(pair =>
            {
                if (pair.e_f is Point pf && pair.e_g is Point pg)
                {
                    return pf.Time + pg.Time == time;
                }
                else
                {
                    return pair.e_f.StartTime + pair.e_g.StartTime < time &&
                           pair.e_f.EndTime + pair.e_g.EndTime > time;
                }
            });
        var exactValuePairs = relevantPairs
            .Where(pair =>
            {
                var c = Element.MaxPlusConvolution(pair.e_f, pair.e_g).ToSequence();
                return c.ValueAt(time) == h_t;
            });

        return exactValuePairs;
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
    /// Computes the max-plus deconvolution of two curves
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
    /// Algorithmic properties discussed in [ZNS23b].
    /// </remarks>
    public virtual Curve Composition(Curve g, ComputationSettings? settings = null)
    {
        settings ??= ComputationSettings.Default();
        var f = this;

        if (!g.IsNonNegative)
            throw new ArgumentException("g must be non-negative");
        if (!g.IsNonDecreasing)
            throw new ArgumentException("g must be non-decreasing");

        var T_due_g = g.PseudoPeriodStart;
        var T_due_f = g.LowerPseudoInverse()
            .ValueAt(f.PseudoPeriodStart);
        if (T_due_f.IsFinite && !g.IsRightContinuousAt(T_due_f))
            // it suffices to use \lpi{g}(T_f) + epsilon, for any epsilon > 0
            T_due_f = g.GetSegmentAfter(T_due_f).EndTime;

        // initialized with non-optimal values
        var T = Rational.Max(T_due_g, T_due_f);
        var d = f.PseudoPeriodLength.Numerator * g.PseudoPeriodLength * g.PseudoPeriodHeight.Denominator;
        var c = f.PseudoPeriodLength.Denominator * g.PseudoPeriodHeight.Numerator * f.PseudoPeriodHeight;

        if (
            T_due_f.IsPlusInfinite // This indicates that the 'standard' logic cannot be applied
            || settings.UseCompositionOptimizations
        )
        {
            if (f.IsUltimatelyConstant || g.IsUltimatelyConstant)
            {
                // composition will also be U.C.
                // the following expression for T summarise Proposition 19, 20 and 21 from [ZNS23b]
                T = Rational.Min(
                    g.IsUltimatelyConstant ? T_due_g : Rational.PlusInfinity,
                    f.IsUltimatelyConstant ? T_due_f : Rational.PlusInfinity
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
                    c = f.PseudoPeriodSlope * g.PseudoPeriodSlope;
                }
                else if (f.IsUltimatelyAffine)
                {
                    d = g.PseudoPeriodLength;
                    c = g.PseudoPeriodHeight * f.PseudoPeriodSlope;
                }
                else if (g.IsUltimatelyAffine)
                {
                    d = f.PseudoPeriodLength / g.PseudoPeriodSlope;
                    c = f.PseudoPeriodHeight;
                }
            }
        }

        #if DO_LOG
        var sw = Stopwatch.StartNew();
        #endif
        var gCut = g.Cut(0, T + d);
        var fCut = f.Cut(g.ValueAt(0), g.LeftLimitAt(T + d), isEndIncluded: true);
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

        return settings.UseRepresentationMinimization ? result.Optimize() : result;
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
    /// Algorithmic properties discussed in [ZNS23b].
    /// </remarks>
    public static Curve Composition(Curve f, Curve g, ComputationSettings? settings = null)
        => f.Composition(g, settings);

    #endregion Composition
}