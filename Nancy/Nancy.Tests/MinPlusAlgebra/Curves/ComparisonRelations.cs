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
