using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// Checks the pseudo-inverses against their definitions at a finite set of heights.
/// At each height, the expected value is computed by a direct search for the infimum or supremum over the elements of the curve,
/// which is independent of the algorithms under test.
/// </summary>
public class PseudoInverseDefinitions
{
    private static readonly Rational Inf = Rational.PlusInfinity;

    private static Rational Limit(Element e)
        => e switch
        {
            Point p => p.Value,
            Segment { IsPlusInfinite: true } => Inf,
            Segment s => s.RightLimitAtStartTime + s.Slope * s.Length,
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Whether <paramref name="y"/> is a value of an element: the value of a point, that of a plateau, and those strictly between the limits of a slope.
    /// </summary>
    private static bool IsAttained(Element e, Rational y)
        => e switch
        {
            Point p => p.Value == y,
            Segment { IsPlusInfinite: true } => false,
            Segment s when s.Slope == 0 => s.RightLimitAtStartTime == y,
            Segment s => s.RightLimitAtStartTime < y && y < s.RightLimitAtStartTime + s.Slope * s.Length,
            _ => throw new InvalidOperationException()
        };

    /// <summary>
    /// Returns the first instant at which the elements are at or above <paramref name="y"/>, $+\infty$ if there is none.
    /// </summary>
    internal static Rational Infimum(IEnumerable<Element> elements, Rational y)
    {
        var best = Inf;
        foreach (var e in elements)
        {
            switch (e)
            {
                case Point p:
                    if (p.Value >= y && p.Time < best)
                        best = p.Time;
                    break;
                case Segment s:
                    var right = s.RightLimitAtStartTime;
                    if (right >= y || s.IsPlusInfinite)
                    {
                        // every instant of the open segment qualifies
                        if (s.StartTime < best)
                            best = s.StartTime;
                    }
                    else if (s.Slope > 0 && right + s.Slope * s.Length > y)
                    {
                        var t = s.StartTime + (y - right) / s.Slope;
                        if (t < best)
                            best = t;
                    }

                    break;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns the last instant at which the elements are at or below <paramref name="y"/>, $-\infty$ if there is none.
    /// </summary>
    internal static Rational Supremum(IEnumerable<Element> elements, Rational y)
    {
        var best = Rational.MinusInfinity;
        foreach (var e in elements)
        {
            switch (e)
            {
                case Point p:
                    if (p.Value <= y && p.Time > best)
                        best = p.Time;
                    break;
                case Segment s:
                    var right = s.RightLimitAtStartTime;
                    // an open start does not qualify, whatever the values after it
                    if (s.IsPlusInfinite || right > y || (right == y && s.Slope > 0))
                        break;
                    var t = s.Slope == 0 || right + s.Slope * s.Length <= y
                        ? s.EndTime
                        : s.StartTime + (y - right) / s.Slope;
                    if (t > best)
                        best = t;
                    break;
            }
        }

        return best;
    }

    /// <summary>
    /// Cuts <paramref name="f"/> over the interval.
    /// An unbounded interval is cut two pseudo-periods past where its values start to repeat.
    /// </summary>
    private static List<Element> Restriction(Curve f, Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        var cutEnd = end == Inf
            ? Rational.Max(start, f.PseudoPeriodStart) + 2 * f.PseudoPeriodLength + 1
            : end;
        return f.Cut(start, cutEnd, isStartIncluded, end == Inf || isEndIncluded).Elements.ToList();
    }

    /// <summary>
    /// The heights at which the pseudo-inverses over an interval are defined, [ZNS23a] Definition 14: the support $f(I)$, extended down to 0 if $0 \in I$.
    /// Above, the support ends at the top of a bounded restriction, and at where an unbounded one was cut.
    /// With <paramref name="includeUnattainedLow"/>, the lowest height is included also where no instant attains it, as at an excluded start the curve rises from:
    /// the infimum of the definition is still that start there.
    /// </summary>
    /// <param name="restriction">The elements of the curve over the interval.</param>
    /// <param name="extendToZero">Starts from 0 instead of the lowest value.</param>
    /// <param name="hasTop">Stops at the highest value.</param>
    /// <param name="isTopCutOff">Excludes the highest value, where the restriction was cut off.</param>
    /// <param name="includeUnattainedLow">Includes the lowest value even if no instant attains it.</param>
    private static IEnumerable<Rational> Heights(List<Element> restriction, bool extendToZero, bool hasTop, bool isTopCutOff, bool includeUnattainedLow = false)
    {
        var first = restriction[0];
        var low = first is Point p ? p.Value : ((Segment)first).RightLimitAtStartTime;
        var isLowAttained = restriction.Any(e => IsAttained(e, low));
        if (extendToZero)
        {
            low = 0;
            isLowAttained = true;
        }

        Rational? high = null;
        var isHighAttained = true;
        if (hasTop)
        {
            var top = restriction.Max(Limit);
            high = top;
            isHighAttained = !isTopCutOff && restriction.Any(e => IsAttained(e, top));
        }

        for (var k = 0; k <= 72; k++)
        foreach (var y in new[] { new Rational(k, 6), new Rational(k, 7) })
        {
            if (y < low || (y == low && !isLowAttained && !includeUnattainedLow))
                continue;
            if (high is { } h && (y > h || (y == h && !isHighAttained)))
                continue;
            yield return y;
        }
    }

    private static IEnumerable<Rational> Heights(
        Curve f, List<Element> restriction, Rational start, Rational end, bool isStartIncluded, bool isOverInterval, bool includeUnattainedLow
    )
        => Heights(
            restriction,
            // the whole pseudo-inverses are defined from 0, those over an interval from f(a), [ZNS26] Definition 19
            extendToZero: start == 0 && isStartIncluded && !isOverInterval,
            // an unbounded pseudo-period, followed up to where the restriction ends
            hasTop: end != Inf || (!f.IsUltimatelyConstant && !f.IsUltimatelyPlusInfinite),
            isTopCutOff: end == Inf,
            includeUnattainedLow: includeUnattainedLow);

    /// <summary>
    /// Whether a restriction is non-negative, which the pseudo-inverses over an interval require.
    /// </summary>
    private static bool IsNonNegative(IEnumerable<Element> restriction)
        => restriction.All(e => e is Point p ? p.Value >= 0 : ((Segment)e).RightLimitAtStartTime >= 0);

    /// <summary>
    /// Returns the curves checked over their whole domain and over the intervals of <see cref="Intervals"/>.
    /// </summary>
    public static IEnumerable<(string id, Curve f)> NamedCurves()
    {
        // $\lfloor t \rfloor$ up to 3, taking 3 at 3, where its left limit is 2, and $+\infty$ after
        yield return ("floor, jumping into +inf at 3", Curve.Maximum(new RateLatencyServiceCurve(1, 0).Floor(), new DelayServiceCurve(3)));

        // $t$ up to 2, taking 3 at 2, where its left limit is 2, and $+\infty$ after
        yield return ("slope, above its left limit into +inf at 2", new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 3), Segment.PlusInfinite(2, 3),
                Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)
            ]),
            3, 1, 0));

        // $t$ up to 1, then 1 up to 2 excluded, taking 2 at 2, where its left limit is 1, and $+\infty$ after
        yield return ("plateau, above its left limit into +inf at 2", new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), Segment.Constant(1, 2, 1), new Point(2, 2),
                Segment.PlusInfinite(2, 3), Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)
            ]),
            3, 1, 0));

        // $t$ up to 2 included, then $+\infty$
        yield return ("slope, equal to its left limit into +inf at 2", new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.PlusInfinite(2, 3),
                Point.PlusInfinite(3), Segment.PlusInfinite(3, 4)
            ]),
            3, 1, 0));

        // $t$ up to 2 excluded, taking $+\infty$ at 2
        yield return ("slope, +inf at 2", new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 2, 0, 1), Point.PlusInfinite(2), Segment.PlusInfinite(2, 3)
            ]),
            2, 1, 0));

        // $+\infty$ everywhere
        yield return ("+inf from 0", Curve.PlusInfinite());

        // 1 at 0, $+\infty$ after
        yield return ("1 at 0, then +inf", new Curve(
            new Sequence([
                new Point(0, 1), Segment.PlusInfinite(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
            ]),
            1, 1, 0));

        // 0 at 0, $+\infty$ after
        yield return ("0 at 0, then +inf", new Curve(
            new Sequence([
                Point.Origin(), Segment.PlusInfinite(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
            ]),
            1, 1, 0));

        // negative at 0, then non-negative from the next instant, and $+\infty$ at 1
        yield return ("-1 at 0, then 0 and +inf", new Curve(
            new Sequence([
                new Point(0, -1), new Segment(0, 1, 0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
            ]),
            1, 1, 0));

        // negative up to 1, then $+\infty$
        yield return ("negative, then +inf", new Curve(
            new Sequence([
                new Point(0, -2), Segment.Constant(0, 1, -1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2)
            ]),
            1, 1, 0));

        // negative up to 1, taking 2 at 1, and $+\infty$ after
        yield return ("negative, then 2 and +inf", new Curve(
            new Sequence([
                new Point(0, -1), new Segment(0, 1, -1, 1), new Point(1, 2), Segment.PlusInfinite(1, 2),
                Point.PlusInfinite(2), Segment.PlusInfinite(2, 3)
            ]),
            2, 1, 0));

        // negative at 0, then 0 from the next instant on
        yield return ("-1 at 0, then 0 and a slope", new Curve(
            new Sequence([
                new Point(0, -1), new Segment(0, 3, 0, 2), new Point(3, 6), new Segment(3, 4, 6, 2)
            ]),
            3, 1, 2));

        // negative up to 1, then reaching 0 at 1
        yield return ("negative, then 0 and a slope", new Curve(
            new Sequence([
                new Point(0, -2), new Segment(0, 1, -2, 1), new Point(1, -1), new Segment(1, 2, 0, 2),
                new Point(2, 2), new Segment(2, 3, 2, 2)
            ]),
            2, 1, 2));

        // negative at 0, then a plateau at 0, a jump, and a constant
        yield return ("-1 at 0, then a plateau and a constant", new Curve(
            new Sequence([
                new Point(0, -1), Segment.Constant(0, 1, 0), new Point(1, 2), Segment.Constant(1, 2, 3),
                new Point(2, 3), Segment.Constant(2, 3, 3)
            ]),
            2, 1, 0));

        // negative at 2, where an interval can start, and non-negative right after
        yield return ("negative at 2, then a slope", new Curve(
            new Sequence([
                new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, -1), new Segment(2, 3, 0, 1),
                new Point(3, 1), new Segment(3, 4, 1, 1)
            ]),
            3, 1, 1));

        // negative at 2, and non-negative right after, up to a constant
        yield return ("negative at 2, then a constant", new Curve(
            new Sequence([
                new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, -1), new Segment(2, 3, 0, 1),
                new Point(3, 1), Segment.Constant(3, 4, 1)
            ]),
            3, 1, 0));

        // negative at 2, and non-negative right after, up to $+\infty$
        yield return ("negative at 2, then a slope and +inf", new Curve(
            new Sequence([
                new Point(0, -1), Segment.Constant(0, 2, -1), new Point(2, -1), new Segment(2, 3, 0, 1),
                new Point(3, 1), Segment.PlusInfinite(3, 4), Point.PlusInfinite(4), Segment.PlusInfinite(4, 5)
            ]),
            4, 1, 0));

        // jumping at 1 and 2, where the value is below the right limit
        yield return ("jumps with the value below the right limit", new Curve(
            new Sequence([
                Point.Origin(), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, 2, 2, 1),
                new Point(2, 3), new Segment(2, 3, 4, 1), new Point(3, 5), new Segment(3, 4, 5, 1)
            ]),
            3, 1, 1));

        // negative forever
        yield return ("negative forever", new Curve(
            new Sequence([
                new Point(0, -1), Segment.Constant(0, 1, -1)
            ]),
            0, 1, 0));
    }

    public static IEnumerable<object[]> NamedCurveCases()
        => NamedCurves().Select(c => new object[] { c.id, c.f });

    /// <summary>
    /// The intervals a curve is checked over: those starting at 0, 1 and 2, closed or open on the left, unbounded or ending 1 or 3 after, closed or open on the right.
    /// Only those over which the curve is non-negative are used.
    /// </summary>
    private static IEnumerable<(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)> Intervals(Curve f)
    {
        foreach (var start in new[] { 0, 1, 2 })
        foreach (var isStartIncluded in new[] { true, false })
        foreach (var length in new[] { -1, 1, 3 })
        foreach (var isEndIncluded in new[] { true, false })
        {
            if (length == -1 && !isEndIncluded)
                continue;
            var end = length == -1 ? Inf : start + length;
            if (!IsNonNegative(Restriction(f, start, end, isStartIncluded, isEndIncluded)))
                continue;
            yield return (start, end, isStartIncluded, isEndIncluded);
        }
    }

    public static IEnumerable<object[]> NamedCurveIntervalCases()
        => NamedCurves().SelectMany(c => Intervals(c.f).Select(i => new object[] { c.id, c.f, i.start, i.end, i.isStartIncluded, i.isEndIncluded }));

    private static void AssertInverse(
        string id, Curve f, Curve inverse, Func<IEnumerable<Element>, Rational, Rational> definition,
        Rational start, Rational end, bool isStartIncluded, bool isEndIncluded, bool isOverInterval = false
    )
    {
        var restriction = Restriction(f, start, end, isStartIncluded, isEndIncluded);
        if (isOverInterval && isStartIncluded)
        {
            // [ZNS26] Definition 19: below f(a) the lower form is +inf and the upper form -inf, also if a = 0
            var first = restriction[0];
            var low = first is Point p ? p.Value : ((Segment)first).RightLimitAtStartTime;
            var below = definition == Infimum ? Inf : Rational.MinusInfinity;
            for (var k = 0; k <= 72; k++)
            foreach (var y in new[] { new Rational(k, 6), new Rational(k, 7) })
            {
                if (y >= low)
                    continue;
                var belowActual = inverse.ValueAt(y);
                Assert.True(below == belowActual, $"{id}: at y = {y}, below f(a) = {low}, expected {below}, got {belowActual}");
            }
        }

        // the lower forms are also checked at a lowest height no instant attains, where the infimum is still defined
        foreach (var y in Heights(f, restriction, start, end, isStartIncluded, isOverInterval, includeUnattainedLow: definition == Infimum))
        {
            var expected = end == Inf && f.IsUltimatelyConstant && definition == Supremum && y >= f.ValueAt(f.PseudoPeriodStart)
                // f never exceeds y
                ? Inf
                : definition(restriction, y);
            var actual = inverse.ValueAt(y);
            Assert.True(expected == actual, $"{id}: at y = {y}, expected {expected}, got {actual}");
        }
    }

    [Theory]
    [MemberData(nameof(NamedCurveCases))]
    public void LowerPseudoInverseIsTheInfimum(string id, Curve f)
        => AssertInverse(id, f, f.LowerPseudoInverse(), Infimum, 0, Inf, true, true);

    [Theory]
    [MemberData(nameof(NamedCurveCases))]
    public void UpperPseudoInverseIsTheSupremum(string id, Curve f)
        => AssertInverse(id, f, f.UpperPseudoInverse(), Supremum, 0, Inf, true, true);

    [Theory]
    [MemberData(nameof(NamedCurveIntervalCases))]
    public void LowerPseudoInverseOverIntervalIsTheInfimum(
        string id, Curve f, Rational start, Rational end, bool isStartIncluded, bool isEndIncluded
    )
        => AssertInverse(
            id, f,
            f.LowerPseudoInverseOverInterval(start, end == Inf ? null : end, isStartIncluded, isEndIncluded),
            Infimum, start, end, isStartIncluded, isEndIncluded, isOverInterval: true);

    [Theory]
    [MemberData(nameof(NamedCurveIntervalCases))]
    public void UpperPseudoInverseOverIntervalIsTheSupremum(
        string id, Curve f, Rational start, Rational end, bool isStartIncluded, bool isEndIncluded
    )
        => AssertInverse(
            id, f,
            f.UpperPseudoInverseOverInterval(start, end == Inf ? null : end, isStartIncluded, isEndIncluded),
            Supremum, start, end, isStartIncluded, isEndIncluded, isOverInterval: true);

    private static void AssertSequenceInverse(
        string id, List<Element> elements, Sequence inverse, Func<IEnumerable<Element>, Rational, Rational> definition, bool extendToZero
    )
    {
        foreach (var y in Heights(elements, extendToZero, hasTop: true, isTopCutOff: false))
        {
            var expected = definition(elements, y);
            Rational actual;
            try
            {
                actual = inverse.ValueAt(y);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException($"{id}: at y = {y}, {e.Message} Cut: {new Sequence(elements).ToCodeString()} Inverse: {inverse.ToCodeString()}", e);
            }

            Assert.True(expected == actual, $"{id}: at y = {y}, expected {expected}, got {actual}");
        }
    }

    /// <summary>
    /// Small curves that together have each kind of tail, a start at, below and above 0, jumps, plateaus at both ends of the period, and fractional values.
    /// </summary>
    private static readonly Curve[] SelectedCurves =
    [
        Curve.PlusInfinite(),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,-2), new Segment(0,new Rational(5, 2),-2,0) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: 0),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(5, 4),0,0), new Point(new Rational(5, 4),1), new Segment(new Rational(5, 4),new Rational(5, 2),1,new Rational(1, 3)) }), pseudoPeriodStart: 0, pseudoPeriodLength: new Rational(5, 2), pseudoPeriodHeight: new Rational(17, 12)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,2), new Segment(0,1,3,2), new Point(1,5), new Segment(1,2,5,2) }), pseudoPeriodStart: 1, pseudoPeriodLength: 1, pseudoPeriodHeight: 2),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,new Rational(3, 2),0,1), new Point(new Rational(3, 2),new Rational(3, 2)), new Segment(new Rational(3, 2),new Rational(5, 2),new Rational(3, 2),0) }), pseudoPeriodStart: new Rational(3, 2), pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,-2), new Segment(0,1,-2,new Rational(3, 2)), new Point(1,new Rational(1, 2)), new Segment(1,2,new Rational(1, 2),1), new Point(2,new Rational(3, 2)), new Segment(2,3,new Rational(3, 2),1) }), pseudoPeriodStart: 2, pseudoPeriodLength: 1, pseudoPeriodHeight: 1),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,0), new Segment(0,2,1,new Rational(3, 2)), new Point(2,4), new Segment(2,3,4,new Rational(1, 2)), new Point(3,new Rational(9, 2)), new Segment(3,4,new Rational(9, 2),0) }), pseudoPeriodStart: 2, pseudoPeriodLength: 2, pseudoPeriodHeight: new Rational(1, 2)),
        new Curve(baseSequence: new Sequence(new List<Element>{ new Point(0,1), new Segment(0,2,1,0), new Point(2,2), new Segment(2,3,3,1), new Point(3,4), new Segment(3,new Rational(9, 2),5,new Rational(1, 3)), new Point(new Rational(9, 2),new Rational(11, 2)), new Segment(new Rational(9, 2),5,new Rational(11, 2),0) }), pseudoPeriodStart: new Rational(9, 2), pseudoPeriodLength: new Rational(1, 2), pseudoPeriodHeight: 1)
    ];

    public static IEnumerable<object[]> SelectedCurveCases()
        => SelectedCurves.Select((f, i) => new object[] { $"selected {i}", f });

    /// <summary>
    /// Checks the selected curves over their whole domain, over the intervals of <see cref="Intervals"/>, and as sequences cut over the bounded ones.
    /// </summary>
    [Theory]
    [MemberData(nameof(SelectedCurveCases))]
    public void SelectedCurvesMatchTheDefinitions(string id, Curve f)
    {
        AssertInverse(id, f, f.LowerPseudoInverse(), Infimum, 0, Inf, true, true);
        AssertInverse(id, f, f.UpperPseudoInverse(), Supremum, 0, Inf, true, true);

        foreach (var (start, end, isStartIncluded, isEndIncluded) in Intervals(f))
        {
            var where = $"{id}, over {(isStartIncluded ? "[" : "]")}{start}, {end}{(isEndIncluded && end != Inf ? "]" : "[")}";
            var upperBound = end == Inf ? (Rational?)null : end;
            AssertInverse(where, f, f.LowerPseudoInverseOverInterval(start, upperBound, isStartIncluded, isEndIncluded),
                Infimum, start, end, isStartIncluded, isEndIncluded, isOverInterval: true);
            AssertInverse(where, f, f.UpperPseudoInverseOverInterval(start, upperBound, isStartIncluded, isEndIncluded),
                Supremum, start, end, isStartIncluded, isEndIncluded, isOverInterval: true);

            if (end == Inf)
                continue;
            var cut = f.Cut(start, end, isStartIncluded, isEndIncluded);
            var elements = cut.Elements.ToList();
            if (elements.Any(e => e.IsInfinite))
                continue;
            foreach (var fromZero in new[] { false, true })
            {
                AssertSequenceInverse($"{where}, cut, from 0: {fromZero}", elements, cut.LowerPseudoInverse(fromZero), Infimum, extendToZero: fromZero);
                AssertSequenceInverse($"{where}, cut, from 0: {fromZero}", elements, cut.UpperPseudoInverse(fromZero), Supremum, extendToZero: fromZero);
            }
        }
    }
}
