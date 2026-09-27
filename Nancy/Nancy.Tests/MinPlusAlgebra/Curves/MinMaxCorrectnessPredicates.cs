using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// <see cref="Curve.IsMinimumUltimatelyPseudoPeriodic"/> and <see cref="Curve.IsMaximumUltimatelyPseudoPeriodic"/> say when the computed minimum and maximum are correct.
/// Over every pair of the operand kinds below, a predicate that holds implies a result equal to the definition, pointwise, over several periods.
/// </summary>
public class MinMaxCorrectnessPredicates
{
    private static Curve Finite(Rational p, Rational s)
        => new(new Sequence([Point.Origin(), new Segment(0, p, 1, s)]), 0, p, s * p);

    /// <summary>Finite at the multiples of <paramref name="p"/>, $+\infty$ between them.</summary>
    private static Curve PlusInfiniteBetweenPoints(Rational p, Rational s)
        => new(new Sequence([new Point(0, 1), Segment.PlusInfinite(0, p)]), 0, p, s * p);

    /// <summary>Finite at the multiples of <paramref name="p"/>, $-\infty$ between them.</summary>
    private static Curve MinusInfiniteBetweenPoints(Rational p, Rational s)
        => new(new Sequence([new Point(0, 1), Segment.MinusInfinite(0, p)]), 0, p, s * p);

    private static Curve UltimatelyPlusInfinite(Rational p, Rational s)
        => new(new Sequence([Point.Origin(), new Segment(0, 2, 0, s), Point.PlusInfinite(2), Segment.PlusInfinite(2, 2 + p)]), 2, p, 0);

    private static Curve UltimatelyMinusInfinite(Rational p, Rational s)
        => new(new Sequence([Point.Origin(), new Segment(0, 2, 0, s), Point.MinusInfinite(2), Segment.MinusInfinite(2, 2 + p)]), 2, p, 0);

    private static readonly Func<Rational, Rational, Curve>[] Kinds =
        [Finite, PlusInfiniteBetweenPoints, MinusInfiniteBetweenPoints, UltimatelyPlusInfinite, UltimatelyMinusInfinite];

    private static readonly (Rational p, Rational s)[] Shapes = [(1, 0), (2, 1), (3, 2), (2, -1)];

    public static List<(Curve a, Curve b)> OperandPairs =
        (from fa in Kinds
         from fb in Kinds
         from sa in Shapes
         from sb in Shapes
         select (fa(sa.p, sa.s), fb(sb.p, sb.s))).ToList();

    public static IEnumerable<object[]> OperandPairsTestCases()
        => OperandPairs.ToXUnitTestCases();

    private static bool AgreesWithTheDefinition(Curve result, Curve a, Curve b, Func<Rational, Rational, Rational> op)
        => Enumerable.Range(0, 121)
            .Select(q => new Rational(q, 4))
            .All(t => result.ValueAt(t) == op(a.ValueAt(t), b.ValueAt(t)));

    [Theory]
    [MemberData(nameof(OperandPairsTestCases))]
    public void AMinimumThePredicateVouchesForIsCorrect(Curve a, Curve b)
    {
        if (Curve.IsMinimumUltimatelyPseudoPeriodic(a, b))
            Assert.True(AgreesWithTheDefinition(Curve.Minimum(a, b), a, b, Rational.Min));
    }

    [Theory]
    [MemberData(nameof(OperandPairsTestCases))]
    public void AMaximumThePredicateVouchesForIsCorrect(Curve a, Curve b)
    {
        if (Curve.IsMaximumUltimatelyPseudoPeriodic(a, b))
            Assert.True(AgreesWithTheDefinition(Curve.Maximum(a, b), a, b, Rational.Max));
    }

    /// <summary>
    /// The pairs the predicates are decided on, each with the answer the conditions give for the minimum and the maximum.
    /// </summary>
    public static List<(Curve a, Curve b, bool minimum, bool maximum)> TruthTable =
    [
        // both ultimately plain
        (Finite(1, 0), Finite(2, 1), true, true),
        (Finite(2, 1), UltimatelyPlusInfinite(1, 0), true, true),
        // equal slopes, whatever the infinities between points
        (PlusInfiniteBetweenPoints(2, 1), PlusInfiniteBetweenPoints(3, 1), true, true),
        (MinusInfiniteBetweenPoints(2, 1), Finite(1, 1), true, true),
        // different slopes: the dominated operand decides
        // lower slope +inf between points
        (PlusInfiniteBetweenPoints(2, 0), Finite(1, 1), false, true),
        // higher slope +inf between points
        (Finite(1, 0), PlusInfiniteBetweenPoints(2, 1), true, true),
        // lower slope -inf between points
        (MinusInfiniteBetweenPoints(2, 0), Finite(1, 1), true, true),
        // higher slope -inf between points
        (Finite(1, 0), MinusInfiniteBetweenPoints(2, 1), true, false),
    ];

    public static IEnumerable<object[]> TruthTableTestCases()
        => TruthTable.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(TruthTableTestCases))]
    public void ThePredicatesFollowTheConditions(Curve a, Curve b, bool minimum, bool maximum)
    {
        Assert.Equal(minimum, Curve.IsMinimumUltimatelyPseudoPeriodic(a, b));
        Assert.Equal(maximum, Curve.IsMaximumUltimatelyPseudoPeriodic(a, b));
    }

    /// <summary>
    /// Non-negative closures of sparse curves, each with whether the predicate vouches for it.
    /// </summary>
    public static List<(Curve operand, bool isVouched)> NonNegativeClosures =
    [
        // 1 at 0, 1 - 7k at 5k and +inf elsewhere: the closure is representable, vouched for, and correct
        (
            new Curve(new Sequence([new Point(0, 1), Segment.PlusInfinite(0, 5), new Point(5, 4), Segment.PlusInfinite(5, 10)]), 5, 5, 3)
                - new Curve(new Sequence([Point.Origin(), new Segment(0, 1, 0, 2)]), 0, 1, 2),
            true
        ),
        // g(2k) = 3k, -inf elsewhere: the closure is 3k at the even integers and 0 between them, which is not ultimately pseudo-periodic;
        // no curve can represent it, the predicate does not vouch for it, and the computed result differs from it
        (
            new Curve(new Sequence([Point.Origin(), Segment.MinusInfinite(0, 2)]), 0, 2, 3),
            false
        ),
    ];

    public static IEnumerable<object[]> NonNegativeClosuresTestCases()
        => NonNegativeClosures.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(NonNegativeClosuresTestCases))]
    public void ANonNegativeClosureIsCorrectWhereThePredicateVouchesForIt(Curve operand, bool isVouched)
    {
        Assert.Equal(isVouched, Curve.IsMaximumUltimatelyPseudoPeriodic(operand, Curve.Zero()));
        Assert.Equal(isVouched, AgreesWithTheDefinition(operand.ToNonNegative(), operand, Curve.Zero(), Rational.Max));
    }
}
