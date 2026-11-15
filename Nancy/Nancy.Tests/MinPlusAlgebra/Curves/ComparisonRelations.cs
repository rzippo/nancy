using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ComparisonRelations
{
    // Every curve built through NetworkCalculus passes through the origin, so a pair drawn from those meets at t = 0 and the strict relations are false whatever the implementation does.
    // AboveOrigin starts at 1 and rises, giving the strict relations a case they can be true for.
    private static Curve AboveOrigin(Rational start, Rational slope)
        => new Curve(
            baseSequence: new Sequence([
                new Point(0, start),
                new Segment(0, 1, start, slope)
            ]),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: slope);

    private static Curve Ramp(Rational slope) => new RateLatencyServiceCurve(rate: slope, latency: 0);

    // pairs where the lower one bounds the upper one, with how they touch
    public static IEnumerable<object[]> OrderedPairs()
    {
        // apart everywhere, origin included
        yield return Case(AboveOrigin(1, 1), AboveOrigin(3, 3), touchesAnywhere: false, touchesAfterOrigin: false);
        // both through the origin, apart after it
        yield return Case(Ramp(2), Ramp(5), touchesAnywhere: true, touchesAfterOrigin: false);
        // meeting again at an interior stretch: a delay curve sits on zero, the ramp joins it there
        yield return Case(Ramp(1), new RateLatencyServiceCurve(rate: 1, latency: 0), touchesAnywhere: true, touchesAfterOrigin: true);
        // k on [k, k + 1[ against t: equal at the start of each unit interval, apart before its end, with the same slope
        yield return Case(new Curve(new Sequence([Point.Origin(), Segment.Zero(0, 1)]), 0, 1, 1), Ramp(1), touchesAnywhere: true, touchesAfterOrigin: true);

        static object[] Case(Curve lower, Curve upper, bool touchesAnywhere, bool touchesAfterOrigin)
            => [lower, upper, touchesAnywhere, touchesAfterOrigin];
    }

    [Theory]
    [MemberData(nameof(OrderedPairs))]
    public void AnOrderedPairIsBoundedInOneDirection(Curve lower, Curve upper, bool touchesAnywhere, bool touchesAfterOrigin)
    {
        AssertEverySurfaceAgrees(lower, upper);
        AssertEverySurfaceAgrees(upper, lower);

        Assert.True(lower.IsLowerBoundOf(upper));
        Assert.True(upper.IsUpperBoundOf(lower));

        var equivalent = lower.Equivalent(upper);
        Assert.Equal(!equivalent, lower.IsProperLowerBoundOf(upper));
        Assert.Equal(!equivalent, upper.IsProperUpperBoundOf(lower));

        Assert.Equal(!touchesAnywhere, lower.IsStrictLowerBoundOf(upper));
        Assert.Equal(!touchesAnywhere, upper.IsStrictUpperBoundOf(lower));

        Assert.Equal(!touchesAfterOrigin, lower.IsStrictLowerBoundOfExceptOrigin(upper));
        Assert.Equal(!touchesAfterOrigin, upper.IsStrictUpperBoundOfExceptOrigin(lower));
    }

    // the same function encoded two ways, and a curve against itself
    public static IEnumerable<object[]> EquivalentPairs()
    {
        var rateLatency = new RateLatencyServiceCurve(rate: 1, latency: 1);
        var reEncoded = new Curve(
            rateLatency.Extend(rateLatency.PseudoPeriodStart + 2 * rateLatency.PseudoPeriodLength),
            rateLatency.PseudoPeriodStart,
            2 * rateLatency.PseudoPeriodLength,
            2 * rateLatency.PseudoPeriodHeight);

        yield return [rateLatency, reEncoded];
        yield return [rateLatency, rateLatency];
    }

    [Theory]
    [MemberData(nameof(EquivalentPairs))]
    public void EquivalentCurvesBoundEachOtherAndNeitherIsProper(Curve a, Curve b)
    {
        AssertEverySurfaceAgrees(a, b);

        Assert.True(a.IsLowerBoundOf(b));
        Assert.True(a.IsUpperBoundOf(b));
        Assert.True(a.Equivalent(b));

        Assert.False(a.IsProperLowerBoundOf(b));
        Assert.False(a.IsProperUpperBoundOf(b));
        Assert.False(a.IsStrictLowerBoundOf(b));
        Assert.False(a.IsStrictUpperBoundOf(b));
        Assert.False(a.IsStrictLowerBoundOfExceptOrigin(b));
        Assert.False(a.IsStrictUpperBoundOfExceptOrigin(b));
    }

    // A re-encoding is not a proper bound of its original, even though == reports the two as different.
    [Fact]
    public void ARepresentationChangeIsNotAProperBound()
    {
        var pair = EquivalentPairs().First();
        var (original, reEncoded) = ((Curve)pair[0], (Curve)pair[1]);

        Assert.False(original == reEncoded);
        Assert.True(original.Equivalent(reEncoded));
        Assert.False(original.IsProperLowerBoundOf(reEncoded));
    }

    public static IEnumerable<object[]> CrossingPairs()
    {
        yield return [Ramp(1), new ConstantCurve(2)];
        yield return [new ConstantCurve(1), Ramp(1)];
    }

    [Theory]
    [MemberData(nameof(CrossingPairs))]
    public void CrossingCurvesBoundEachOtherInNeitherDirection(Curve a, Curve b)
    {
        AssertEverySurfaceAgrees(a, b);

        foreach (var (x, y) in new[] { (a, b), (b, a) })
        {
            Assert.False(x.IsLowerBoundOf(y));
            Assert.False(x.IsUpperBoundOf(y));
            Assert.False(x.IsProperLowerBoundOf(y));
            Assert.False(x.IsProperUpperBoundOf(y));
            Assert.False(x.IsStrictLowerBoundOf(y));
            Assert.False(x.IsStrictUpperBoundOf(y));
            Assert.False(x.IsStrictLowerBoundOfExceptOrigin(y));
            Assert.False(x.IsStrictUpperBoundOfExceptOrigin(y));
        }
    }

    [Theory]
    [MemberData(nameof(CrossingPairs))]
    public void NotBeingBelowDoesNotMakeACurveAnUpperBound(Curve a, Curve b)
    {
        Assert.False(a < b);
        Assert.False(a >= b);
        Assert.False(a > b);
        Assert.False(a <= b);
    }

    // The operators read the proper relation, so a re-encoding of one function is neither below nor above itself.
    [Theory]
    [MemberData(nameof(EquivalentPairs))]
    public void EquivalentCurvesAreNeitherStrictlyBelowNorAbove(Curve a, Curve b)
    {
        Assert.True(a <= b);
        Assert.True(a >= b);
        Assert.False(a < b);
        Assert.False(a > b);
    }

    [Theory]
    [MemberData(nameof(OrderedPairs))]
    public void SettingsReachTheComputation(Curve lower, Curve upper, bool touchesAnywhere, bool touchesAfterOrigin)
    {
        _ = touchesAnywhere;
        _ = touchesAfterOrigin;

        var settings = new ComputationSettings { UseParallelism = false };

        Assert.Equal(lower.IsLowerBoundOf(upper), Curve.IsLowerBoundOf(lower, upper, settings));
        Assert.Equal(lower.IsProperLowerBoundOf(upper), Curve.IsProperLowerBoundOf(lower, upper, settings));
        Assert.Equal(lower.IsStrictLowerBoundOf(upper), Curve.IsStrictLowerBoundOf(lower, upper, settings));
        Assert.Equal(
            lower.IsStrictLowerBoundOfExceptOrigin(upper),
            Curve.IsStrictLowerBoundOfExceptOrigin(lower, upper, settings));
    }

    // A curve of unit slope between jumps, given by its pieces: the value at the start of each, and the right limit there.
    private static Curve Pieces(
        Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight,
        params (Rational Start, Rational End, Rational Value, Rational Right)[] pieces)
        => new Curve(
            baseSequence: new Sequence(pieces.SelectMany(p => new Element[]
            {
                new Point(p.Start, p.Value),
                new Segment(p.Start, p.End, p.Right, 1)
            })),
            pseudoPeriodStart: pseudoPeriodStart,
            pseudoPeriodLength: pseudoPeriodLength,
            pseudoPeriodHeight: pseudoPeriodHeight);

    // f(t) = t + g(t), with g = 0 on [0,2), then 1 on [2,3), 0 on [3,4), 1 on [4,5) and so on: period 2 from T = 1.
    private static Curve Steps(Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight)
        => Pieces(pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight,
            (0, 2, 0, 0), (2, 3, 3, 3));

    // The same function written over the base sequence [0,5), so that it can carry period 4 from T = 1 or period 2 from T = 3.
    private static Curve StepsOverFiveUnits(Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight, Rational valueAtFour)
        => Pieces(pseudoPeriodStart, pseudoPeriodLength, pseudoPeriodHeight,
            (0, 2, 0, 0), (2, 3, 3, 3), (3, 4, 3, 3), (4, 5, valueAtFour, 5));

    // In each pair below, what tells the two curves apart happens only after both transients end.
    // Cases: the first curve, the second, whether the first is below the second, whether the second is below the first, whether they are the same function.
    public static IEnumerable<object[]> LatePairs()
    {
        // Period 3 from T = 2: t + 3 on [0,2), then t + 2, t + 1, t + 2 on the three unit stretches of a period.
        var upper = Pieces(2, 3, 3, (0, 2, 3, 3), (2, 3, 4, 4), (3, 4, 4, 4), (4, 5, 6, 6));
        // As upper, but t + 1/2 on the middle stretch of a period.
        var crossing = Pieces(2, 3, 3, (0, 2, 3, 3), (2, 3, 4, 4), (3, 4, new Rational(7, 2), new Rational(7, 2)), (4, 5, 6, 6));

        // Steps (period 2 from T = 1) sits below upper everywhere and meets it only on [6,7], in the third period of upper
        yield return Case(Steps(1, 2, 2), upper, true, false, false);
        // Steps is above crossing only on [6,7], so the two cross at t = 6, after both transients and before t = 8
        yield return Case(Steps(1, 2, 2), crossing, false, false, false);
        // the same function over period 2 from T = 1 and over period 4 from T = 1
        yield return Case(Steps(1, 2, 2), StepsOverFiveUnits(1, 4, 4, 5), true, true, true);
        // the same function with the first period unrolled into the transient, period 2 from T = 3
        yield return Case(Steps(1, 2, 2), StepsOverFiveUnits(3, 2, 2, 5), true, true, true);
        // equal except at t = 4, where the period-4 encoding takes 4 instead of 5, and so again at t = 8, 12, ...
        yield return Case(StepsOverFiveUnits(1, 4, 4, 4), Steps(1, 2, 2), true, false, false);

        static object[] Case(Curve a, Curve b, bool aBelowB, bool bBelowA, bool equivalent)
            => [a, b, aBelowB, bBelowA, equivalent];
    }

    [Theory]
    [MemberData(nameof(LatePairs))]
    public void CurvesDifferingOnlyAfterTheirTransientsAreOrderedAccordingly(
        Curve a, Curve b, bool aBelowB, bool bBelowA, bool equivalent)
    {
        AssertEveryRelation(a, b, aBelowB, bBelowA, equivalent);
        AssertEveryRelation(b, a, bBelowA, aBelowB, equivalent);
    }

    // A curve constant on each unit interval, with the given values from 0 on, then repeating from pseudoPeriodStart.
    private static Curve UnitSteps(Rational pseudoPeriodStart, Rational pseudoPeriodLength, Rational pseudoPeriodHeight, params Rational[] values)
        => new Curve(
            baseSequence: new Sequence(values.SelectMany((value, i) => new Element[]
            {
                new Point(i, value),
                Segment.Constant(i, i + 1, value)
            })),
            pseudoPeriodStart: pseudoPeriodStart,
            pseudoPeriodLength: pseudoPeriodLength,
            pseudoPeriodHeight: pseudoPeriodHeight);

    // 2k on [2k, 2k + 1[ and -inf on [2k + 1, 2k + 2[.
    private static readonly Curve ZeroThenMinusInfinite = UnitSteps(0, 2, 2, 0, Rational.MinusInfinity);

    // +inf at each integer k, and k times the height on ]k, k + 1[.
    private static Curve PlusInfiniteAtIntegers(Rational pseudoPeriodHeight)
        => new Curve(new Sequence([Point.PlusInfinite(0), Segment.Zero(0, 1)]), 0, 1, pseudoPeriodHeight);

    // -inf at each integer k, and k on ]k, k + 1[.
    private static readonly Curve MinusInfiniteAtIntegers
        = new Curve(new Sequence([Point.MinusInfinite(0), Segment.Zero(0, 1)]), 0, 1, 1);

    // k times the height at each integer k, and +inf between.
    private static Curve FiniteAtIntegers(Rational pseudoPeriodHeight)
        => new Curve(new Sequence([Point.Origin(), Segment.PlusInfinite(0, 1)]), 0, 1, pseudoPeriodHeight);

    // In each pair below, one curve or both take +inf or -inf; in the last two, the minimum of the two is not ultimately pseudo-periodic.
    // Cases: the first curve, the second, then whether the first is below the second and the other way round, strictly below, and strictly below except at the origin.
    public static IEnumerable<object[]> PairsTakingInfinities()
    {
        // 2k on [2k, 2k + 1[ passes 10 at t = 12, after the first common period
        yield return Case("2k or -inf, against 10", ZeroThenMinusInfinite, new ConstantCurve(10),
            false, false, false, false, false, false);
        yield return Case("2k or -inf, against 10 or +inf", ZeroThenMinusInfinite, UnitSteps(0, 2, 0, 10, Rational.PlusInfinity),
            false, false, false, false, false, false);
        // the two are +inf at the integers, and differ only from ]1, 2[ on
        yield return Case("+inf at the integers, k or 0 between", PlusInfiniteAtIntegers(1), PlusInfiniteAtIntegers(0),
            false, true, false, false, false, false);
        // the first is -inf wherever the second is finite, and the second is +inf wherever the first is finite
        yield return Case("-inf or k, against -k or +inf", MinusInfiniteAtIntegers, FiniteAtIntegers(-1),
            true, false, true, false, true, false);
        yield return Case("-inf or k, against 0 or +inf", MinusInfiniteAtIntegers, FiniteAtIntegers(0),
            true, false, true, false, true, false);
        // the second is 1, 5, 3, 5 on [0, 4[, then 2 and 5; the first is 4 > 2 at t = 4
        yield return Case("2k or -inf, against a transient", ZeroThenMinusInfinite, UnitSteps(4, 2, 0, 1, 5, 3, 5, 2, 5),
            false, false, false, false, false, false);
        // the first is 5 on [5, 6[, where the second is 1: past one period of either, within their least common multiple
        yield return Case("periods 3 and 2, crossing at t = 5", UnitSteps(0, 3, 0, 0, 0, 5), UnitSteps(0, 2, 0, Rational.PlusInfinity, 1),
            false, false, false, false, false, false);

        // the subadditive closures of a point: k at each integer k, and 3k at each 2k, +inf elsewhere
        var closureOfOne = new Point(1, 1).SubAdditiveClosure(pseudoPeriodLength: 1, pseudoPeriodHeight: 1);
        var closureOfTwo = new Point(2, 3).SubAdditiveClosure(pseudoPeriodLength: 2, pseudoPeriodHeight: 3);
        // both +inf between the integers
        yield return Case("closures of two points", closureOfOne, closureOfTwo,
            true, false, false, false, false, false);
        // equal at the integers
        yield return Case("t, against the closure of a point", Ramp(1), closureOfOne,
            true, false, false, false, false, false);
        yield return Case("t / 2, against the closure of a point", Ramp(new Rational(1, 2)), closureOfOne,
            true, false, false, false, true, false);
        // both are finite only at the integers, where the closure grows faster: k passes 100 + k / 2 at k = 201
        var slowerAtIntegers = new Curve(
            new Sequence([Point.Origin(), Segment.PlusInfinite(0, 1), new Point(1, new Rational(201, 2)), Segment.PlusInfinite(1, 2)]),
            1, 1, new Rational(1, 2));
        yield return Case("the closure of a point, against 100 + k / 2 at the integers", closureOfOne, slowerAtIntegers,
            false, false, false, false, false, false);

        // t passes 100 at t = 101, where the first is finite
        yield return Case("100 or +inf, against t",
            new Curve(new Sequence([new Point(0, 100), Segment.PlusInfinite(0, 1)]), 0, 1, 0), Ramp(1),
            false, false, false, false, false, false);
        // neither is ultimately finite, so whether both are finite somewhere in the period takes a scan: k passes 100 on ]102, 103[
        yield return Case("100 or +inf, against -inf or k",
            UnitSteps(0, 2, 0, 100, Rational.PlusInfinity), MinusInfiniteAtIntegers,
            false, false, false, false, false, false);

        static object[] Case(string id, Curve a, Curve b,
            bool aBelowB, bool bBelowA, bool aStrictlyBelowB, bool bStrictlyBelowA, bool aStrictlyBelowBExceptOrigin, bool bStrictlyBelowAExceptOrigin)
            => [id, a, b, aBelowB, bBelowA, aStrictlyBelowB, bStrictlyBelowA, aStrictlyBelowBExceptOrigin, bStrictlyBelowAExceptOrigin];
    }

    [Theory]
    [MemberData(nameof(PairsTakingInfinities))]
    public void CurvesTakingInfinitiesAreOrderedByTheirValues(string id,
        Curve a, Curve b, bool aBelowB, bool bBelowA, bool aStrictlyBelowB, bool bStrictlyBelowA, bool aStrictlyBelowBExceptOrigin, bool bStrictlyBelowAExceptOrigin)
    {
        _ = id;
        var equivalent = aBelowB && bBelowA;
        AssertEveryRelation(a, b, aBelowB, bBelowA, equivalent);
        AssertEveryRelation(b, a, bBelowA, aBelowB, equivalent);

        var settings = new ComputationSettings { UseParallelism = false };
        Assert.Equal(aStrictlyBelowB, Curve.IsStrictLowerBoundOf(a, b, settings));
        Assert.Equal(bStrictlyBelowA, Curve.IsStrictLowerBoundOf(b, a, settings));
        Assert.Equal(aStrictlyBelowBExceptOrigin, Curve.IsStrictLowerBoundOfExceptOrigin(a, b, settings));
        Assert.Equal(bStrictlyBelowAExceptOrigin, Curve.IsStrictLowerBoundOfExceptOrigin(b, a, settings));

        var (verified, lower, upper) = Curve.Dominance(a, b, settings);
        Assert.Equal(aBelowB || bBelowA, verified);
        if (aBelowB)
        {
            Assert.Same(a, lower);
            Assert.Same(b, upper);
        }
        else if (bBelowA)
        {
            Assert.Same(b, lower);
            Assert.Same(a, upper);
        }
    }

    private static void AssertEveryRelation(Curve x, Curve y, bool xBelowY, bool yBelowX, bool equivalent)
    {
        var settings = new ComputationSettings { UseParallelism = false };

        Assert.False(x == y);
        Assert.True(x != y);

        Assert.Equal(xBelowY, x <= y);
        Assert.Equal(yBelowX, x >= y);
        Assert.Equal(xBelowY && !equivalent, x < y);
        Assert.Equal(yBelowX && !equivalent, x > y);

        Assert.Equal(equivalent, x.Equivalent(y, settings));
        Assert.Equal(equivalent, Curve.Equivalent(x, y, settings));

        Assert.Equal(xBelowY, x.IsLowerBoundOf(y, settings));
        Assert.Equal(xBelowY, Curve.IsLowerBoundOf(x, y, settings));
        Assert.Equal(yBelowX, x.IsUpperBoundOf(y, settings));
        Assert.Equal(yBelowX, Curve.IsUpperBoundOf(x, y, settings));
        Assert.Equal(xBelowY && !equivalent, x.IsProperLowerBoundOf(y, settings));
        Assert.Equal(xBelowY && !equivalent, Curve.IsProperLowerBoundOf(x, y, settings));
        Assert.Equal(yBelowX && !equivalent, x.IsProperUpperBoundOf(y, settings));
        Assert.Equal(yBelowX && !equivalent, Curve.IsProperUpperBoundOf(x, y, settings));
    }

    // The three surfaces of each relation must agree, so a call through one can be read as the others.
    private static void AssertEverySurfaceAgrees(Curve a, Curve b)
    {
        Assert.Equal(a.IsLowerBoundOf(b), Curve.IsLowerBoundOf(a, b));
        Assert.Equal(a.IsLowerBoundOf(b), a <= b);

        Assert.Equal(a.IsUpperBoundOf(b), Curve.IsUpperBoundOf(a, b));
        Assert.Equal(a.IsUpperBoundOf(b), a >= b);

        Assert.Equal(a.IsProperLowerBoundOf(b), Curve.IsProperLowerBoundOf(a, b));
        Assert.Equal(a.IsProperLowerBoundOf(b), a < b);

        Assert.Equal(a.IsProperUpperBoundOf(b), Curve.IsProperUpperBoundOf(a, b));
        Assert.Equal(a.IsProperUpperBoundOf(b), a > b);
        Assert.Equal(a.IsStrictLowerBoundOf(b), Curve.IsStrictLowerBoundOf(a, b));
        Assert.Equal(a.IsStrictUpperBoundOf(b), Curve.IsStrictUpperBoundOf(a, b));
        Assert.Equal(a.IsStrictLowerBoundOfExceptOrigin(b), Curve.IsStrictLowerBoundOfExceptOrigin(a, b));
        Assert.Equal(a.IsStrictUpperBoundOfExceptOrigin(b), Curve.IsStrictUpperBoundOfExceptOrigin(a, b));
    }

    // Curve against Point: one value against one value, so there is a single strict reading.

    public static IEnumerable<object[]> PointCases()
    {
        var curve = Ramp(1);   // f(t) = t
        yield return [curve, new Point(2, 5), true, false];    // curve below the point
        yield return [curve, new Point(2, 1), false, true];    // curve above the point
        yield return [curve, new Point(2, 2), false, false];   // curve exactly at the point
    }

    [Theory]
    [MemberData(nameof(PointCases))]
    public void ACurveComparesAgainstAPointAtThatTime(Curve curve, Point point, bool below, bool above)
    {
        Assert.Equal(below, curve.IsStrictlyBelow(point));
        Assert.Equal(above, curve.IsStrictlyAbove(point));
        Assert.Equal(below || !above, curve.IsBelow(point));
        Assert.Equal(above || !below, curve.IsAbove(point));

        Assert.Equal(curve.IsBelow(point), Curve.IsBelow(curve, point));
        Assert.Equal(curve.IsAbove(point), Curve.IsAbove(curve, point));
        Assert.Equal(curve.IsStrictlyBelow(point), Curve.IsStrictlyBelow(curve, point));
        Assert.Equal(curve.IsStrictlyAbove(point), Curve.IsStrictlyAbove(curve, point));

        Assert.Equal(curve.IsBelow(point), curve <= point);
        Assert.Equal(curve.IsAbove(point), curve >= point);
        Assert.Equal(curve.IsStrictlyBelow(point), curve < point);
        Assert.Equal(curve.IsStrictlyAbove(point), curve > point);
    }

    // A relation added later without a case here fails this rather than going unexercised.
    [Fact]
    public void EveryComparisonMemberIsExercised()
    {
        var covered = new[]
        {
            nameof(Curve.IsLowerBoundOf), nameof(Curve.IsUpperBoundOf),
            nameof(Curve.IsProperLowerBoundOf), nameof(Curve.IsProperUpperBoundOf),
            nameof(Curve.IsStrictLowerBoundOf), nameof(Curve.IsStrictUpperBoundOf),
            nameof(Curve.IsStrictLowerBoundOfExceptOrigin), nameof(Curve.IsStrictUpperBoundOfExceptOrigin),
            nameof(Curve.IsBelow), nameof(Curve.IsAbove),
            nameof(Curve.IsStrictlyBelow), nameof(Curve.IsStrictlyAbove),
        };

        // every comparison operator is exercised through AssertEverySurfaceAgrees or the point theory
        var operators = new[] { "op_LessThan", "op_GreaterThan", "op_LessThanOrEqual", "op_GreaterThanOrEqual" };
        var declaredOperators = typeof(Curve)
            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .Where(n => n.StartsWith("op_") && n.Contains("Than"))
            .Distinct();
        Assert.Empty(declaredOperators.Except(operators));

        var declared = typeof(Curve)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .Where(n => n.StartsWith("IsLowerBound") || n.StartsWith("IsUpperBound")
                        || n.StartsWith("IsProper") || n.StartsWith("IsStrict")
                        || n == nameof(Curve.IsBelow) || n == nameof(Curve.IsAbove))
            .Distinct();

        Assert.Empty(declared.Except(covered));
    }
}
