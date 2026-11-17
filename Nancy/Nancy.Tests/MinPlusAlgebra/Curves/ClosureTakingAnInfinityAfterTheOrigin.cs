using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// The subadditive closure of a curve that takes $-\infty$ after the origin, and no $+\infty$, is the closure of its (min,+) restriction before its first $-\infty$, then $-\infty$;
/// the superadditive closure mirrors it with $+\infty$, nancy#182, [TBP-CLOS-INF] Theorem 9.3 and Theorem 10.1.
/// </summary>
public class ClosureTakingAnInfinityAfterTheOrigin
{
    private static Curve C(Element[] elements, Rational pseudoPeriodStart, Rational pseudoPeriodLength)
        => new(new Sequence(elements), pseudoPeriodStart, pseudoPeriodLength, 0);

    /// <summary>The examples of [TBP-CLOS-INF] Example 9.4: each curve and its subadditive closure.</summary>
    public static IEnumerable<object[]> Cases()
    {
        yield return ["A, -inf on [1, 2[ in the period",
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)], 0, 2),
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)], 1, 1)];
        yield return ["B, -inf on ]1, 2[ in the period",
            C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2)], 0, 2),
            C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1)];
        yield return ["C, -inf on ]1, 2[ in the transient",
            C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2), new Point(2, 5), Segment.Constant(2, 3, 5)], 2, 1),
            C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1)];
        // computed by public 1.5, and rejected by the gates on this branch
        yield return ["D, -inf at 1 in the transient",
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.Zero(1, 2), Point.Zero(2), Segment.Zero(2, 3)], 2, 1),
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)], 1, 1)];
        yield return ["E, t, then -inf from 2",
            C([Point.Origin(), new Segment(0, 2, 0, 1), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1),
            C([Point.Origin(), new Segment(0, 2, 0, 1), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1)];
        yield return ["F, t, then -inf after 2",
            C([Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.MinusInfinite(2, 3), Point.MinusInfinite(3), Segment.MinusInfinite(3, 4)], 3, 1),
            C([Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, 2), Segment.MinusInfinite(2, 3), Point.MinusInfinite(3), Segment.MinusInfinite(3, 4)], 3, 1)];
        yield return ["G, -inf at 1 in the period",
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.Zero(1, 2)], 0, 2),
            C([Point.Origin(), Segment.Zero(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)], 1, 1)];
        // the closure before the first -inf differs from f
        yield return ["H, 1, then 3, then -inf from 2",
            C([Point.Origin(), Segment.Constant(0, 1, 1), new Point(1, 1), Segment.Constant(1, 2, 3), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1),
            C([Point.Origin(), Segment.Constant(0, 1, 1), new Point(1, 1), Segment.Constant(1, 2, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1)];
        // -inf after 3 only, and the closure at 3 below f(3)
        yield return ["I, -inf after 3",
            C([Point.Origin(), Segment.Constant(0, 1, 3), new Point(1, 1), Segment.Constant(1, 3, 4), new Point(3, 4), Segment.MinusInfinite(3, 4), Point.MinusInfinite(4), Segment.MinusInfinite(4, 5)], 4, 1),
            C([Point.Origin(), Segment.Constant(0, 1, 3), new Point(1, 1), Segment.Constant(1, 2, 4), new Point(2, 2), Segment.Constant(2, 3, 4), new Point(3, 3), Segment.MinusInfinite(3, 4), Point.MinusInfinite(4), Segment.MinusInfinite(4, 5)], 4, 1)];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SubAdditiveClosureIsTheClosureBeforeTheFirstMinusInfinityThenMinusInfinity(string id, Curve f, Curve expected)
    {
        var closure = f.SubAdditiveClosure();
        Assert.True(closure.Equivalent(expected), $"{id}: {closure}");
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void SuperAdditiveClosureMirrorsIt(string id, Curve f, Curve expected)
    {
        var closure = f.Negate().SuperAdditiveClosure();
        Assert.True(closure.Equivalent(expected.Negate()), $"{id}: {closure}");
    }

    /// <summary>
    /// Case C was computed only thanks to the order of the specialized convolution of subadditive curves.
    /// </summary>
    [Fact]
    public void SubAdditiveClosureDoesNotDependOnTheConvolutionOptimizations()
    {
        var c = C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2), new Point(2, 5), Segment.Constant(2, 3, 5)], 2, 1);
        var expected = C([Point.Origin(), Segment.Zero(0, 1), Point.Zero(1), Segment.MinusInfinite(1, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1);
        var settings = new ComputationSettings { UseSubAdditiveConvolutionOptimizations = false };
        Assert.True(c.SubAdditiveClosure(settings).Equivalent(expected));
    }

    /// <summary>
    /// A curve finite right after the origin that takes both infinities has, in the convention the closures adopt, the closure of its restriction before its first $-\infty$, then $-\infty$.
    /// </summary>
    [Fact]
    public void ClosuresOfACurveTakingBothInfinitiesAndFiniteRightAfterTheOrigin()
    {
        // 0 on [0, 1[, +inf on [1, 2[, then -inf: 0 on [0, 2[ and -inf from 2
        var f = C([Point.Origin(), Segment.Zero(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1);
        var expected = C([Point.Origin(), Segment.Zero(0, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1);
        Assert.True(f.SubAdditiveClosure().Equivalent(expected));
        Assert.True(f.Negate().SuperAdditiveClosure().Equivalent(expected.Negate()));
    }

    /// <summary>
    /// A curve $+\infty$ right after the origin that takes $-\infty$ has no short path, and its closures are not supported.
    /// </summary>
    [Fact]
    public void ClosuresOfACurvePlusInfiniteRightAfterTheOriginAndTakingMinusInfinityAreNotSupported()
    {
        // 0 at 0, +inf on ]0, 1[, then -inf
        var f = C([Point.Origin(), Segment.PlusInfinite(0, 1), Point.MinusInfinite(1), Segment.MinusInfinite(1, 2)], 1, 1);
        Assert.Throws<UndeterminedResultException>(() => f.SubAdditiveClosure());
        Assert.Throws<UndeterminedResultException>(() => f.Negate().SuperAdditiveClosure());
    }

    /// <summary>
    /// A curve negative at the origin and finite right after it has a closure $-\infty$ everywhere, in the convention the closures adopt, whether or not it takes both infinities.
    /// </summary>
    [Fact]
    public void GeneralClosuresOfACurveNegativeAtTheOriginTakingBothInfinities()
    {
        // -1 at 0, 0 on ]0, 1[, +inf on [1, 2[, then -inf
        var f = C([new Point(0, -1), Segment.Zero(0, 1), Point.PlusInfinite(1), Segment.PlusInfinite(1, 2), Point.MinusInfinite(2), Segment.MinusInfinite(2, 3)], 2, 1);
        Assert.True(f.GeneralSubAdditiveClosure().Equivalent(Curve.MinusInfinite()));
        Assert.True(f.Negate().GeneralSuperAdditiveClosure().Equivalent(Curve.PlusInfinite()));
    }
}
