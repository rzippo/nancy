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
    private static Rational Infimum(IEnumerable<Element> elements, Rational y)
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
    private static Rational Supremum(IEnumerable<Element> elements, Rational y)
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
    /// The heights at which the pseudo-inverses over the interval are defined, [ZNS23a] Definition 14: the support $f(I)$, extended down to 0 if $0 \in I$.
    /// </summary>
    private static IEnumerable<Rational> Heights(
        Curve f, List<Element> restriction, Rational start, Rational end, bool isStartIncluded
    )
    {
        var first = restriction[0];
        var low = first is Point p ? p.Value : ((Segment)first).RightLimitAtStartTime;
        var isLowAttained = restriction.Any(e => IsAttained(e, low));
        if (start == 0 && isStartIncluded)
        {
            low = 0;
            isLowAttained = true;
        }

        Rational? high = null;
        var isHighAttained = true;
        if (end != Inf)
        {
            var top = restriction.Max(Limit);
            high = top;
            isHighAttained = restriction.Any(e => IsAttained(e, top));
        }
        else if (!f.IsUltimatelyConstant && !f.IsUltimatelyPlusInfinite)
        {
            // an unbounded pseudo-period, followed up to where the restriction ends
            high = restriction.Max(Limit);
            isHighAttained = false;
        }

        for (var k = 0; k <= 72; k++)
        foreach (var y in new[] { new Rational(k, 6), new Rational(k, 7) })
        {
            if (y < low || (y == low && !isLowAttained))
                continue;
            if (high is { } h && (y > h || (y == h && !isHighAttained)))
                continue;
            yield return y;
        }
    }

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
    /// The named curves over a set of intervals, those starting at 0, 1 and 2, closed or open on the left, unbounded or ending 1 or 3 after, closed or open on the right.
    /// Only the intervals over which the curve is non-negative are used.
    /// </summary>
    public static IEnumerable<object[]> NamedCurveIntervalCases()
    {
        foreach (var (id, f) in NamedCurves())
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
            yield return [id, f, new Rational(start), end, isStartIncluded, isEndIncluded];
        }
    }

    private static void AssertInverse(
        string id, Curve f, Curve inverse, Func<IEnumerable<Element>, Rational, Rational> definition,
        Rational start, Rational end, bool isStartIncluded, bool isEndIncluded
    )
    {
        var restriction = Restriction(f, start, end, isStartIncluded, isEndIncluded);
        foreach (var y in Heights(f, restriction, start, end, isStartIncluded))
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
            Infimum, start, end, isStartIncluded, isEndIncluded);

    [Theory]
    [MemberData(nameof(NamedCurveIntervalCases))]
    public void UpperPseudoInverseOverIntervalIsTheSupremum(
        string id, Curve f, Rational start, Rational end, bool isStartIncluded, bool isEndIncluded
    )
        => AssertInverse(
            id, f,
            f.UpperPseudoInverseOverInterval(start, end == Inf ? null : end, isStartIncluded, isEndIncluded),
            Supremum, start, end, isStartIncluded, isEndIncluded);
}
