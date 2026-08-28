using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ToLowerNonIncreasing
{
    public static List<(Curve operand, Curve expected)> KnownPairs =
    [
        (
            // f rises, then falls below the origin, then rises again:
            // the running infimum follows the decreases and is pinned at the running minimum while f rises.
            // f(0) = 5 is not the global minimum, so the closure is not constant.
            operand: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 5),
                    new Segment(0, 2, 5, -2),
                    new Point(2, 1),
                    new Segment(2, 3, 1, 1),
                    new Point(3, 2),
                    new Segment(3, 6, 2, -1),
                    new Point(6, -1),
                    new Segment(6, 8, -1, 0.5)
                ]),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 1
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 5),
                    new Segment(0, 2, 5, -2),
                    new Point(2, 1),
                    Segment.Constant(2, 3, 1),
                    new Point(3, 1),
                    Segment.Constant(3, 4, 1),
                    new Point(4, 1),
                    new Segment(4, 6, 1, -1),
                    new Point(6, -1),
                    Segment.Constant(6, 8, -1)
                ]),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 0
            )
        ),
        (
            // a non-decreasing curve: the running infimum is the constant f(0)
            operand: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 3),
                    new Segment(0, 2, 3, 1),
                    new Point(2, 5),
                    new Segment(2, 4, 5, 1)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 2
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 3),
                    Segment.Constant(0, 2, 3),
                    new Point(2, 3),
                    Segment.Constant(2, 4, 3)
                ]),
                pseudoPeriodStart: 2,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: 0
            )
        )
    ];

    public static IEnumerable<object[]> GetKnownTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void ToLowerNonIncreasingTest(Curve operand, Curve expected)
    {
        var result = operand.ToLowerNonIncreasing();

        Assert.True((-result).IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));

        // the lower non-increasing closure is a minorant: the operand dominates it
        var (dominance, lower, _) = Curve.Dominance(operand, result);
        Assert.True(dominance);
        Assert.True(Curve.Equivalent(lower, result));
    }

    /// <summary>
    /// The algebraic method, f ⊗ 0 = Convolution(f, 0), must give the same result as the construction.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void AlgebraicImplementation(Curve operand, Curve expected)
    {
        var settings = ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false };

        var result = operand.ToLowerNonIncreasing(settings);

        Assert.True((-result).IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }

    /// <summary>
    /// The lower non-increasing closure of $f$ equals the negation of the upper non-decreasing closure of $-f$: $\text{LNI}(f) = -(\text{UND}(-f))$.
    /// The algebraic path (Convolution) is compared against the independently implemented ToUpperNonDecreasing, so the check is not circular.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void DualityWithNonDecreasingClosure(Curve operand, Curve expected)
    {
        var settings = ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false };

        var byAlgebraic = operand.ToLowerNonIncreasing(settings);
        var byDuality = (-operand).ToUpperNonDecreasing().Negate();

        Assert.True(Curve.Equivalent(byAlgebraic, byDuality));
        Assert.True(Curve.Equivalent(byAlgebraic, expected));
    }

    /// <summary>
    /// The fast path (duality with the existing upper non-decreasing closure) and the algebraic path
    /// (Convolution with the zero curve) must agree on a broad battery of curves.
    /// </summary>
    public static IEnumerable<object[]> DecreasingCases()
        => ToUpperNonDecreasing.GetDecreasingTestCases();

    [Theory]
    [MemberData(nameof(DecreasingCases))]
    public void FastAndAlgebraicAgree(Curve operand, Curve _)
    {
        var fast = operand.ToLowerNonIncreasing();
        var algebraic = operand.ToLowerNonIncreasing(
            ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false });

        Assert.True(Curve.Equivalent(fast, algebraic));
    }
}
