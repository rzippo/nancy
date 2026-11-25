using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// CommonVisit itself, the path every AdditionExpression rename goes through, used to copy only nine of CurveExpression's sixteen real predicate cache fields.
// RenameCurveVisitorCachePreservation pins the ten-operator-bypass half of that bug, forcing IsComputed/_value only.
// This pins the other half: every predicate cache individually survives a rename through CommonVisit.
public class RenameCurveVisitorPredicateCachePreservation
{
    private static CurveExpression Expression()
        => Expressions.FromCurve(new ConstantCurve(1), "a").Addition(Expressions.FromCurve(new ConstantCurve(2), "b"));

    public static IEnumerable<object[]> Predicates()
    {
        yield return Case(e => e.IsSubAdditive, e => e.SubAdditiveIfKnown);
        yield return Case(e => e.IsSuperAdditive, e => e.SuperAdditiveIfKnown);
        yield return Case(e => e.IsLeftContinuous, e => e.LeftContinuousIfKnown);
        yield return Case(e => e.IsRightContinuous, e => e.RightContinuousIfKnown);
        yield return Case(e => e.IsNonNegative, e => e.NonNegativeIfKnown);
        yield return Case(e => e.IsNonDecreasing, e => e.NonDecreasingIfKnown);
        yield return Case(e => e.IsIncreasing, e => e.IncreasingIfKnown);
        yield return Case(e => e.IsConcave, e => e.ConcaveIfKnown);
        yield return Case(e => e.IsConvex, e => e.ConvexIfKnown);
        yield return Case(e => e.IsPassingThroughOrigin, e => e.PassingThroughOriginIfKnown);
        yield return Case(e => e.IsUltimatelyFinite, e => e.UltimatelyFiniteIfKnown);
        yield return Case(e => e.IsPlain, e => e.PlainIfKnown);
        yield return Case(e => e.IsUltimatelyPlain, e => e.UltimatelyPlainIfKnown);
        yield return Case(e => e.IsUltimatelyAffine, e => e.UltimatelyAffineIfKnown);
        yield return Case(e => e.IsUltimatelyConstant, e => e.UltimatelyConstantIfKnown);
        yield return Case(e => e.IsWellDefined, e => e.WellDefinedIfKnown);
        yield return Case(e => e.IsNonIncreasing, e => e.NonIncreasingIfKnown);
        yield return Case(e => e.IsDecreasing, e => e.DecreasingIfKnown);
        yield return Case(e => e.IsNonPositive, e => e.NonPositiveIfKnown);
        yield return Case(e => e.IsPositive, e => e.PositiveIfKnown);
        yield return Case(e => e.IsNegative, e => e.NegativeIfKnown);
        yield return Case(e => e.IsConstant, e => e.ConstantIfKnown);
        yield return Case(e => e.IsAffine, e => e.AffineIfKnown);
        yield return Case(e => e.IsZero, e => e.ZeroIfKnown);

        static object[] Case(Func<CurveExpression, bool> force, Func<CurveExpression, bool?> ifKnown)
            => new object[] { force, ifKnown };
    }

    [Theory]
    [MemberData(nameof(Predicates))]
    public void RenamingThroughCommonVisitPreservesTheForcedPredicate(
        Func<CurveExpression, bool> force, Func<CurveExpression, bool?> ifKnown)
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(ifKnown(expression));

        force(expression);
        Assert.NotNull(ifKnown(expression));

        var renamed = expression.WithName("renamed");

        Assert.NotNull(ifKnown(renamed));
        Assert.Equal(ifKnown(expression), ifKnown(renamed));
    }
}
