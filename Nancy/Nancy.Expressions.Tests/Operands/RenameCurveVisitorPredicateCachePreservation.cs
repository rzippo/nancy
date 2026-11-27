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

    [Fact]
    public void RenamingPreservesIsSubAdditive()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.SubAdditiveIfKnown);

        _ = expression.IsSubAdditive;
        Assert.NotNull(expression.SubAdditiveIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.SubAdditiveIfKnown);
        Assert.Equal(expression.SubAdditiveIfKnown, renamed.SubAdditiveIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsSuperAdditive()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.SuperAdditiveIfKnown);

        _ = expression.IsSuperAdditive;
        Assert.NotNull(expression.SuperAdditiveIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.SuperAdditiveIfKnown);
        Assert.Equal(expression.SuperAdditiveIfKnown, renamed.SuperAdditiveIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsLeftContinuous()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.LeftContinuousIfKnown);

        _ = expression.IsLeftContinuous;
        Assert.NotNull(expression.LeftContinuousIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.LeftContinuousIfKnown);
        Assert.Equal(expression.LeftContinuousIfKnown, renamed.LeftContinuousIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsRightContinuous()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.RightContinuousIfKnown);

        _ = expression.IsRightContinuous;
        Assert.NotNull(expression.RightContinuousIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.RightContinuousIfKnown);
        Assert.Equal(expression.RightContinuousIfKnown, renamed.RightContinuousIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsNonNegative()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.NonNegativeIfKnown);

        _ = expression.IsNonNegative;
        Assert.NotNull(expression.NonNegativeIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.NonNegativeIfKnown);
        Assert.Equal(expression.NonNegativeIfKnown, renamed.NonNegativeIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsNonDecreasing()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.NonDecreasingIfKnown);

        _ = expression.IsNonDecreasing;
        Assert.NotNull(expression.NonDecreasingIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.NonDecreasingIfKnown);
        Assert.Equal(expression.NonDecreasingIfKnown, renamed.NonDecreasingIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsIncreasing()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.IncreasingIfKnown);

        _ = expression.IsIncreasing;
        Assert.NotNull(expression.IncreasingIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.IncreasingIfKnown);
        Assert.Equal(expression.IncreasingIfKnown, renamed.IncreasingIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsConcave()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.ConcaveIfKnown);

        _ = expression.IsConcave;
        Assert.NotNull(expression.ConcaveIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.ConcaveIfKnown);
        Assert.Equal(expression.ConcaveIfKnown, renamed.ConcaveIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsConvex()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.ConvexIfKnown);

        _ = expression.IsConvex;
        Assert.NotNull(expression.ConvexIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.ConvexIfKnown);
        Assert.Equal(expression.ConvexIfKnown, renamed.ConvexIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsPassingThroughOrigin()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.PassingThroughOriginIfKnown);

        _ = expression.IsPassingThroughOrigin;
        Assert.NotNull(expression.PassingThroughOriginIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.PassingThroughOriginIfKnown);
        Assert.Equal(expression.PassingThroughOriginIfKnown, renamed.PassingThroughOriginIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsUltimatelyFinite()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.UltimatelyFiniteIfKnown);

        _ = expression.IsUltimatelyFinite;
        Assert.NotNull(expression.UltimatelyFiniteIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.UltimatelyFiniteIfKnown);
        Assert.Equal(expression.UltimatelyFiniteIfKnown, renamed.UltimatelyFiniteIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsPlain()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.PlainIfKnown);

        _ = expression.IsPlain;
        Assert.NotNull(expression.PlainIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.PlainIfKnown);
        Assert.Equal(expression.PlainIfKnown, renamed.PlainIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsUltimatelyPlain()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.UltimatelyPlainIfKnown);

        _ = expression.IsUltimatelyPlain;
        Assert.NotNull(expression.UltimatelyPlainIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.UltimatelyPlainIfKnown);
        Assert.Equal(expression.UltimatelyPlainIfKnown, renamed.UltimatelyPlainIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsUltimatelyAffine()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.UltimatelyAffineIfKnown);

        _ = expression.IsUltimatelyAffine;
        Assert.NotNull(expression.UltimatelyAffineIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.UltimatelyAffineIfKnown);
        Assert.Equal(expression.UltimatelyAffineIfKnown, renamed.UltimatelyAffineIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsUltimatelyConstant()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.UltimatelyConstantIfKnown);

        _ = expression.IsUltimatelyConstant;
        Assert.NotNull(expression.UltimatelyConstantIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.UltimatelyConstantIfKnown);
        Assert.Equal(expression.UltimatelyConstantIfKnown, renamed.UltimatelyConstantIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsWellDefined()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.WellDefinedIfKnown);

        _ = expression.IsWellDefined;
        Assert.NotNull(expression.WellDefinedIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.WellDefinedIfKnown);
        Assert.Equal(expression.WellDefinedIfKnown, renamed.WellDefinedIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsNonIncreasing()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.NonIncreasingIfKnown);

        _ = expression.IsNonIncreasing;
        Assert.NotNull(expression.NonIncreasingIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.NonIncreasingIfKnown);
        Assert.Equal(expression.NonIncreasingIfKnown, renamed.NonIncreasingIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsDecreasing()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.DecreasingIfKnown);

        _ = expression.IsDecreasing;
        Assert.NotNull(expression.DecreasingIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.DecreasingIfKnown);
        Assert.Equal(expression.DecreasingIfKnown, renamed.DecreasingIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsNonPositive()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.NonPositiveIfKnown);

        _ = expression.IsNonPositive;
        Assert.NotNull(expression.NonPositiveIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.NonPositiveIfKnown);
        Assert.Equal(expression.NonPositiveIfKnown, renamed.NonPositiveIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsPositive()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.PositiveIfKnown);

        _ = expression.IsPositive;
        Assert.NotNull(expression.PositiveIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.PositiveIfKnown);
        Assert.Equal(expression.PositiveIfKnown, renamed.PositiveIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsNegative()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.NegativeIfKnown);

        _ = expression.IsNegative;
        Assert.NotNull(expression.NegativeIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.NegativeIfKnown);
        Assert.Equal(expression.NegativeIfKnown, renamed.NegativeIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsConstant()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.ConstantIfKnown);

        _ = expression.IsConstant;
        Assert.NotNull(expression.ConstantIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.ConstantIfKnown);
        Assert.Equal(expression.ConstantIfKnown, renamed.ConstantIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsAffine()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.AffineIfKnown);

        _ = expression.IsAffine;
        Assert.NotNull(expression.AffineIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.AffineIfKnown);
        Assert.Equal(expression.AffineIfKnown, renamed.AffineIfKnown);
    }

    [Fact]
    public void RenamingPreservesIsZero()
    {
        var expression = Expression();
        Assert.IsType<AdditionExpression>(expression);
        Assert.Null(expression.ZeroIfKnown);

        _ = expression.IsZero;
        Assert.NotNull(expression.ZeroIfKnown);

        var renamed = expression.WithName("renamed");

        Assert.NotNull(renamed.ZeroIfKnown);
        Assert.Equal(expression.ZeroIfKnown, renamed.ZeroIfKnown);
    }

}
