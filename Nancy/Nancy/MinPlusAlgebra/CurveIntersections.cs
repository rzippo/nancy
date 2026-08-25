using System;
using System.Collections.Generic;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// A time at which one curve passes the other, in the sense that the sign of their difference flips across it.
/// </summary>
/// <param name="Time">The time of the crossing.</param>
/// <param name="IsUpward">True if the receiver passes from below the other curve to above it.</param>
/// <param name="IsContact">True if the two curves are equal at <paramref name="Time"/>; false when one jumps over the other without ever meeting it.</param>
public readonly record struct CurveCrossing(
    Rational Time,
    bool IsUpward,
    bool IsContact
);

/// <summary>
/// The read-out operations shared by <see cref="IntersectionPattern"/> and <see cref="CrossingPattern"/>.
/// </summary>
/// <remarks>
/// Both describe an infinite set as a transient part followed by a block that repeats every period, and are read the same way.
/// Keeping the walk in one place means the guards that make these operations terminate on an infinite pattern exist once, rather than in two copies that have to be kept in step.
/// Two occurrences are merged while their combination stays a single item, so a stretch of equality that runs across a period boundary is reported whole.
/// Crossings never merge, since two crossings at distinct times stay distinct.
/// </remarks>
internal static class PeriodicPattern
{
    /// <summary>
    /// True if the repeating block actually recurs, rather than being empty or standing still.
    /// </summary>
    private static bool Repeats<T>(IReadOnlyList<T> repeating, Rational periodLength)
        => repeating.Count > 0 && periodLength > 0;

    /// <summary>
    /// The occurrences in increasing order, merging each with the following ones while their combination stays a single item.
    /// </summary>
    /// <remarks>
    /// Terminates even on an infinite pattern as soon as the caller stops pulling, since the occurrences are ordered and their times grow without bound.
    /// A merge chain always ends: merging forever would mean the occurrences cover a whole period and then some, which the construction rules out.
    /// </remarks>
    private static IEnumerable<T> EnumerateMerged<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge)
        where T : struct
    {
        T? current = null;
        foreach (var item in Enumerate(transient, repeating, periodLength, shift))
        {
            if (current is null)
            {
                current = item;
                continue;
            }

            if (tryMerge(current.Value, item) is { } merged)
            {
                current = merged;
            }
            else
            {
                yield return current.Value;
                current = item;
            }
        }

        if (current is not null)
            yield return current.Value;
    }

    /// <summary>
    /// The first item in the overall order, or null if there is none.
    /// </summary>
    internal static T? First<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge)
        where T : struct
    {
        foreach (var item in EnumerateMerged(transient, repeating, periodLength, shift, tryMerge))
            return item;

        return null;
    }

    /// <summary>
    /// The first <paramref name="count"/> items, in increasing order.
    /// </summary>
    internal static IReadOnlyList<T> Take<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge,
        int count)
        where T : struct
    {
        if (count <= 0)
            return Array.Empty<T>();

        var result = new List<T>();
        foreach (var item in EnumerateMerged(transient, repeating, periodLength, shift, tryMerge))
        {
            if (result.Count >= count)
                break;
            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// The items that start before <paramref name="time"/>, in increasing order.
    /// </summary>
    /// <remarks>
    /// Terminates even on an infinite pattern, since the items are ordered and their times grow without bound.
    /// </remarks>
    internal static IReadOnlyList<T> Before<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge,
        Func<T, Rational> startOf,
        Rational time)
        where T : struct
    {
        if (time.IsMinusInfinite)
            return Array.Empty<T>();

        var result = new List<T>();
        foreach (var item in EnumerateMerged(transient, repeating, periodLength, shift, tryMerge))
        {
            if (startOf(item) >= time)
                break;
            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// The item at the given position in the overall order, or null if there are fewer.
    /// </summary>
    internal static T? ElementAt<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge,
        int index)
        where T : struct
    {
        if (index < 0)
            return null;

        var position = 0;
        foreach (var item in EnumerateMerged(transient, repeating, periodLength, shift, tryMerge))
        {
            if (position == index)
                return item;
            position++;
        }

        return null;
    }

    /// <summary>
    /// Walks the items in increasing order, without end when the pattern repeats.
    /// </summary>
    internal static IEnumerable<T> Enumerate<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift,
        Func<T, T, T?> tryMerge)
        where T : struct
        => EnumerateMerged(transient, repeating, periodLength, shift, tryMerge);

    /// <summary>
    /// Walks the occurrences in increasing order, without end when the pattern repeats.
    /// </summary>
    private static IEnumerable<T> Enumerate<T>(
        IReadOnlyList<T> transient,
        IReadOnlyList<T> repeating,
        Rational periodLength,
        Func<T, Rational, T> shift)
    {
        foreach (var item in transient)
            yield return item;

        if (!Repeats(repeating, periodLength))
            yield break;

        for (var period = 0; ; period++)
            foreach (var item in repeating)
                yield return shift(item, periodLength * period);
    }
}

/// <summary>
/// The finite description of a possibly infinite set of intervals over which two curves take the same value.
/// </summary>
/// <remarks>
/// The read-outs merge occurrences whose union is itself an interval, so a stretch of equality is reported whole even when it runs across a period boundary or the edge of the repeating part.
/// </remarks>
/// <param name="Transient">The intervals before the repeating part, in increasing order.</param>
/// <param name="Repeating">One pseudo-period worth of intervals, repeated forever; empty when there are finitely many.</param>
/// <param name="PeriodStart">Time at which the repeating part begins.</param>
/// <param name="PeriodLength">Length of the pseudo-period over which <paramref name="Repeating"/> repeats.</param>
public readonly record struct IntersectionPattern(
    IReadOnlyList<Interval> Transient,
    IReadOnlyList<Interval> Repeating,
    Rational PeriodStart,
    Rational PeriodLength
)
{
    private static readonly Func<Interval, Rational, Interval> ShiftBy =
        (interval, delta) => new Interval(
            interval.Lower + delta, interval.Upper + delta, interval.IsLowerIncluded, interval.IsUpperIncluded);

    private static readonly Func<Interval, Rational> StartOf = interval => interval.Lower;

    private static readonly Func<Interval, Interval, Interval?> TryMerge = Interval.Union;

    /// <summary>
    /// True if there are infinitely many intersections, which <see cref="Repeating"/> being non-empty answers by inspection.
    /// </summary>
    /// <remarks>
    /// Curves that meet over a stretch that never ends give one unbounded interval, not infinitely many, so this is false for them.
    /// </remarks>
    public bool IsInfinite => Repeating.Count > 0;

    /// <summary>
    /// True if the two curves never take the same value within the requested window.
    /// </summary>
    public bool IsEmpty => Transient.Count == 0 && Repeating.Count == 0;

    /// <summary>
    /// The first intersection, or null if there is none.
    /// </summary>
    public Interval? First =>
        PeriodicPattern.First(Transient, Repeating, PeriodLength, ShiftBy, TryMerge);

    /// <summary>
    /// The last intersection, or null if there is none.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there are infinitely many intersections, so that there is no last one.
    /// </exception>
    public Interval? Last
    {
        get
        {
            if (IsInfinite)
                throw new InvalidOperationException("The curves meet infinitely often, so there is no last intersection.");

            return Transient.Count > 0 ? Transient[^1] : null;
        }
    }

    /// <summary>
    /// The number of intersections, or null when there are infinitely many.
    /// </summary>
    public int? Count => IsInfinite ? null : Transient.Count;

    /// <summary>
    /// The first <paramref name="count"/> intersections, in increasing order.
    /// </summary>
    /// <param name="count">How many intersections to take at most.</param>
    public IReadOnlyList<Interval> Take(int count)
        => PeriodicPattern.Take(Transient, Repeating, PeriodLength, ShiftBy, TryMerge, count);

    /// <summary>
    /// The intersections that start before <paramref name="time"/>, in increasing order.
    /// </summary>
    /// <param name="time">The time bound, exclusive.</param>
    /// <remarks>
    /// Terminates even on an infinite pattern when <paramref name="time"/> is finite, since the intersections are ordered in time and their times grow without bound.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pattern is infinite and <paramref name="time"/> is $+\infty$, so every intersection is before the bound.
    /// </exception>
    public IReadOnlyList<Interval> Before(Rational time)
    {
        if (IsInfinite && time.IsPlusInfinite)
            throw new InvalidOperationException("The curves meet infinitely often, so there are infinitely many intersections before +infinity.");

        return PeriodicPattern.Before(Transient, Repeating, PeriodLength, ShiftBy, TryMerge, StartOf, time);
    }

    /// <summary>
    /// The intersection at the given position in the overall order, or null if there are fewer.
    /// </summary>
    /// <param name="index">The zero-based position.</param>
    public Interval? ElementAt(int index)
        => PeriodicPattern.ElementAt(Transient, Repeating, PeriodLength, ShiftBy, TryMerge, index);

    /// <summary>
    /// Enumerates the intersections in increasing order.
    /// </summary>
    /// <param name="throwIfInfinite">
    /// If true, which is the default, throws when the pattern is infinite rather than enumerating forever.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pattern is infinite and <paramref name="throwIfInfinite"/> is true.
    /// </exception>
    public IEnumerable<Interval> Enumerate(bool throwIfInfinite = true)
    {
        if (throwIfInfinite && IsInfinite)
            throw new InvalidOperationException("The pattern is infinite; enumerate it with throwIfInfinite: false to accept a non-terminating loop.");

        return PeriodicPattern.Enumerate(Transient, Repeating, PeriodLength, ShiftBy, TryMerge);
    }
}

/// <summary>
/// The finite description of a possibly infinite set of crossings between two curves.
/// </summary>
/// <param name="Transient">The crossings before the repeating part, in increasing order.</param>
/// <param name="Repeating">One pseudo-period worth of crossings, repeated forever; empty when there are finitely many.</param>
/// <param name="PeriodStart">Time at which the repeating part begins.</param>
/// <param name="PeriodLength">Length of the pseudo-period over which <paramref name="Repeating"/> repeats.</param>
public readonly record struct CrossingPattern(
    IReadOnlyList<CurveCrossing> Transient,
    IReadOnlyList<CurveCrossing> Repeating,
    Rational PeriodStart,
    Rational PeriodLength
)
{
    private static readonly Func<CurveCrossing, Rational, CurveCrossing> ShiftBy =
        (crossing, delta) => crossing with { Time = crossing.Time + delta };

    private static readonly Func<CurveCrossing, Rational> StartOf = crossing => crossing.Time;

    // two crossings at distinct times stay distinct, so there is never anything to merge
    private static readonly Func<CurveCrossing, CurveCrossing, CurveCrossing?> NeverMerge = (_, _) => null;

    /// <summary>
    /// True if there are infinitely many crossings, which <see cref="Repeating"/> being non-empty answers by inspection.
    /// </summary>
    public bool IsInfinite => Repeating.Count > 0;

    /// <summary>
    /// True if the two curves never cross within the requested window.
    /// </summary>
    public bool IsEmpty => Transient.Count == 0 && Repeating.Count == 0;

    /// <summary>
    /// The first crossing, or null if there is none.
    /// </summary>
    public CurveCrossing? First =>
        PeriodicPattern.First(Transient, Repeating, PeriodLength, ShiftBy, NeverMerge);

    /// <summary>
    /// The last crossing, or null if there is none.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when there are infinitely many crossings, so that there is no last one.
    /// </exception>
    public CurveCrossing? Last
    {
        get
        {
            if (IsInfinite)
                throw new InvalidOperationException("The curves cross infinitely often, so there is no last crossing.");

            return Transient.Count > 0 ? Transient[^1] : null;
        }
    }

    /// <summary>
    /// The number of crossings, or null when there are infinitely many.
    /// </summary>
    public int? Count => IsInfinite ? null : Transient.Count;

    /// <summary>
    /// The first <paramref name="count"/> crossings, in increasing order.
    /// </summary>
    /// <param name="count">How many crossings to take at most.</param>
    public IReadOnlyList<CurveCrossing> Take(int count)
        => PeriodicPattern.Take(Transient, Repeating, PeriodLength, ShiftBy, NeverMerge, count);

    /// <summary>
    /// The crossings that occur before <paramref name="time"/>, in increasing order.
    /// </summary>
    /// <param name="time">The time bound, exclusive.</param>
    /// <remarks>
    /// Terminates even on an infinite pattern when <paramref name="time"/> is finite, since the crossings are ordered in time and their times grow without bound.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pattern is infinite and <paramref name="time"/> is $+\infty$, so every crossing is before the bound.
    /// </exception>
    public IReadOnlyList<CurveCrossing> Before(Rational time)
    {
        if (IsInfinite && time.IsPlusInfinite)
            throw new InvalidOperationException("The curves cross infinitely often, so there are infinitely many crossings before +infinity.");

        return PeriodicPattern.Before(Transient, Repeating, PeriodLength, ShiftBy, NeverMerge, StartOf, time);
    }

    /// <summary>
    /// The crossing at the given position in the overall order, or null if there are fewer.
    /// </summary>
    /// <param name="index">The zero-based position.</param>
    public CurveCrossing? ElementAt(int index)
        => PeriodicPattern.ElementAt(Transient, Repeating, PeriodLength, ShiftBy, NeverMerge, index);

    /// <summary>
    /// Enumerates the crossings in increasing order.
    /// </summary>
    /// <param name="throwIfInfinite">
    /// If true, which is the default, throws when the pattern is infinite rather than enumerating forever.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the pattern is infinite and <paramref name="throwIfInfinite"/> is true.
    /// </exception>
    public IEnumerable<CurveCrossing> Enumerate(bool throwIfInfinite = true)
    {
        if (throwIfInfinite && IsInfinite)
            throw new InvalidOperationException("The pattern is infinite; enumerate it with throwIfInfinite: false to accept a non-terminating loop.");

        return PeriodicPattern.Enumerate(Transient, Repeating, PeriodLength, ShiftBy, NeverMerge);
    }
}
