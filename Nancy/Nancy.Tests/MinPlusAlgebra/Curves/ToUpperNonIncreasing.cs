using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ToUpperNonIncreasing
{
    public static List<(Curve operand, Curve expected)> KnownPairs =
    [
        (
            // f falls, rises back to the origin value, then falls again and keeps falling:
            // the least non-increasing majorant is pinned at the running maximum over the future.
            operand: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 2),
                    new Segment(0, 2, 2, -1),
                    new Point(2, 0),
                    new Segment(2, 4, 0, 1),
                    new Point(4, 2),
                    new Segment(4, 6, 2, -1),
                    new Point(6, 0),
                    new Segment(6, 8, 0, -0.5)
                ]),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: -1
            ),
            expected: new Curve(
                baseSequence: new Sequence([
                    new Point(0, 2),
                    Segment.Constant(0, 4, 2),
                    new Point(4, 2),
                    new Segment(4, 6, 2, -1),
                    new Point(6, 0),
                    new Segment(6, 8, 0, -0.5)
                ]),
                pseudoPeriodStart: 6,
                pseudoPeriodLength: 2,
                pseudoPeriodHeight: -1
            )
        )
    ];

    public static IEnumerable<object[]> GetKnownTestCases()
        => KnownPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void ToUpperNonIncreasingTest(Curve operand, Curve expected)
    {
        var result = operand.ToUpperNonIncreasing();

        Assert.True((-result).IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));

        // the upper non-increasing closure is a majorant: it dominates the operand
        var (dominance, _, upper) = Curve.Dominance(operand, result);
        Assert.True(dominance);
        Assert.True(Curve.Equivalent(upper, result));
    }

    /// <summary>
    /// The algebraic method, f ⊘ 0 = Deconvolution(f, 0), must give the same result as the construction.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void AlgebraicImplementation(Curve operand, Curve expected)
    {
        var settings = ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false };

        var result = operand.ToUpperNonIncreasing(settings);

        Assert.True((-result).IsNonDecreasing);
        Assert.True(Curve.Equivalent(result, expected));
    }

    /// <summary>
    /// The upper non-increasing closure of $f$ equals the negation of the lower non-decreasing closure of $-f$: $\text{UNI}(f) = -(\text{LND}(-f))$.
    /// The algebraic path (Deconvolution) is compared against the independently implemented ToLowerNonDecreasing, so the check is not circular.
    /// </summary>
    [Theory]
    [MemberData(nameof(GetKnownTestCases))]
    public void DualityWithNonDecreasingClosure(Curve operand, Curve expected)
    {
        var settings = ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false };

        var byAlgebraic = operand.ToUpperNonIncreasing(settings);
        var byDuality = (-operand).ToLowerNonDecreasing().Negate();

        Assert.True(Curve.Equivalent(byAlgebraic, byDuality));
        Assert.True(Curve.Equivalent(byAlgebraic, expected));
    }

    /// <summary>
    /// A curve that is already non-increasing is its own upper non-increasing closure.
    /// </summary>
    [Fact]
    public void AlreadyNonIncreasingIsUnchanged()
    {
        var operand = new Curve(
            baseSequence: new Sequence([
                new Point(0, 2),
                new Segment(0, 2, 2, -1),
                new Point(2, 0),
                new Segment(2, 4, 0, -1)
            ]),
            pseudoPeriodStart: 2,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: -2
        );

        Assert.True((-operand).IsNonDecreasing);
        Assert.True(Curve.Equivalent(operand.ToUpperNonIncreasing(), operand));
    }

    /// <summary>
    /// The fast path (duality with the existing lower non-decreasing closure) and the algebraic path
    /// (Deconvolution with the zero curve) must agree on a broad battery of curves.
    /// </summary>
    public static IEnumerable<object[]> DecreasingCases()
        => ToUpperNonDecreasing.GetDecreasingTestCases();

    [Theory]
    [MemberData(nameof(DecreasingCases))]
    public void FastAndAlgebraicAgree(Curve operand, Curve _)
    {
        var fast = operand.ToUpperNonIncreasing();
        var algebraic = operand.ToUpperNonIncreasing(
            ComputationSettings.Default() with { UseNonDecreasingClosureOptimizations = false });

        Assert.True(Curve.Equivalent(fast, algebraic));
    }
}
