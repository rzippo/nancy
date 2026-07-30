using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class CurveExpressionVisitors
{
    private static Curve AffineCurve(int slope = 1) =>
        new(
            new Sequence([
                Point.Origin(),
                new Segment(0, 1, 0, slope)
            ]),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: slope
        );

    private static Curve BothInfiniteCurve() =>
        new(
            new Sequence([
                Point.PlusInfinite(0),
                Segment.PlusInfinite(0, 1),
                Point.MinusInfinite(1),
                Segment.MinusInfinite(1, 2),
            ]),
            pseudoPeriodStart: 0,
            pseudoPeriodLength: 2,
            pseudoPeriodHeight: 0
        );

    private static List<CurveExpression> CurveExpressions()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");
        var fasterService = new RateLatencyServiceCurve(rate: 4, latency: 1).ToExpression("t");
        var affine = AffineCurve().ToExpression("f");

#pragma warning disable CS0618 // Type or member is obsolete
        return
        [
            service,
            arrival.Negate(),
            arrival.Negate().ToNonNegative(),
            service.SubAdditiveClosure(),
            service.SuperAdditiveClosure(),
            arrival.ToUpperNonDecreasing(),
            arrival.ToLowerNonDecreasing(),
            arrival.ToLeftContinuous(),
            arrival.ToRightContinuous(),
            arrival.WithZeroOrigin(),
            arrival.LowerPseudoInverse(),
            arrival.UpperPseudoInverse(),
            Expressions.Addition(arrival, service),
            Expressions.Subtraction(arrival, service),
            Expressions.Minimum(arrival, service),
            Expressions.Maximum(arrival, service),
            Expressions.Convolution(arrival, service),
            Expressions.Deconvolution(arrival, service),
            Expressions.MaxPlusConvolution(service, fasterService),
            Expressions.MaxPlusDeconvolution(service, fasterService),
            Expressions.Composition(affine, service),
            arrival.DelayBy(new Rational(1)),
            arrival.ForwardBy(new Rational(1)),
            arrival.HorizontalShift(new Rational(-1)),
            arrival.VerticalShift(new Rational(2)),
            arrival.Scale(new Rational(2)),
            arrival.Scale(Rational.Zero),
            arrival.Floor(),
            arrival.Ceil(),
        ];
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static IEnumerable<object[]> CurveExpressionTestCases
        => CurveExpressions().ToXUnitTestCases();

    private static List<CurveExpression> WellDefinedExpressions()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");

#pragma warning disable CS0618 // Type or member is obsolete
        return
        [
            service,
            arrival.Negate(),
            arrival.ToNonNegative(),
            service.SubAdditiveClosure(),
            arrival.ToUpperNonDecreasing(),
            arrival.ToLowerNonDecreasing(),
            arrival.ToLeftContinuous(),
            arrival.ToRightContinuous(),
            arrival.WithZeroOrigin(),
            Expressions.Addition(arrival, service),
            Expressions.Subtraction(arrival, service),
            Expressions.Minimum(arrival, service),
            Expressions.Maximum(arrival, service),
            Expressions.Convolution(arrival, service),
            Expressions.Deconvolution(arrival, service),
            arrival.Floor(),
            arrival.Ceil(),
        ];
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public static IEnumerable<object[]> WellDefinedExpressionTestCases
        => WellDefinedExpressions().ToXUnitTestCases();

    private static List<CurveExpression> UnsupportedWellDefinedExpressions()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");
        var affine = AffineCurve().ToExpression("f");

        return
        [
            service.SuperAdditiveClosure(),
            arrival.LowerPseudoInverse(),
            arrival.UpperPseudoInverse(),
            Expressions.MaxPlusConvolution(service, service),
            Expressions.MaxPlusDeconvolution(service, service),
            Expressions.Composition(affine, service),
            arrival.DelayBy(new Rational(1)),
            arrival.ForwardBy(new Rational(1)),
            arrival.HorizontalShift(new Rational(1)),
            arrival.VerticalShift(new Rational(1)),
            arrival.Scale(new Rational(2)),
        ];
    }

    public static IEnumerable<object[]> UnsupportedWellDefinedExpressionTestCases
        => UnsupportedWellDefinedExpressions().ToXUnitTestCases();

    private static List<CurveExpression> SubscriptOrSuperscriptExpressions()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");

        return
        [
            arrival.LowerPseudoInverse(),
            arrival.UpperPseudoInverse(),
            arrival.ToLowerNonDecreasing(),
            arrival.ToUpperNonDecreasing(),
            arrival.ToNonNegative(),
            arrival.ToLeftContinuous(),
            arrival.ToRightContinuous(),
        ];
    }

    public static IEnumerable<object[]> SubscriptOrSuperscriptExpressionTestCases
        => SubscriptOrSuperscriptExpressions().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(CurveExpressionTestCases))]
    public void CurveAnalysisVisitorsReturnStableValuesForRepresentativeExpressions(CurveExpression expression)
    {
        var firstRead = (
            expression.IsSubAdditive,
            expression.IsLeftContinuous,
            expression.IsRightContinuous,
            expression.IsNonNegative,
            expression.IsNonDecreasing,
            expression.IsConcave,
            expression.IsConvex,
            expression.IsPassingThroughOrigin
        );

        var secondRead = (
            expression.IsSubAdditive,
            expression.IsLeftContinuous,
            expression.IsRightContinuous,
            expression.IsNonNegative,
            expression.IsNonDecreasing,
            expression.IsConcave,
            expression.IsConvex,
            expression.IsPassingThroughOrigin
        );

        Assert.Equal(firstRead, secondRead);
    }

    [Fact]
    public void CurveAnalysisVisitorsMatchConcreteCurveProperties()
    {
        var curve = new RateLatencyServiceCurve(rate: 3, latency: 1);
        var expression = curve.ToExpression("s");

        Assert.Equal(curve.IsSubAdditive, expression.IsSubAdditive);
        Assert.Equal(curve.IsLeftContinuous, expression.IsLeftContinuous);
        Assert.Equal(curve.IsRightContinuous, expression.IsRightContinuous);
        Assert.Equal(curve.IsNonNegative, expression.IsNonNegative);
        Assert.Equal(curve.IsNonDecreasing, expression.IsNonDecreasing);
        Assert.Equal(curve.IsConcave, expression.IsConcave);
        Assert.Equal(curve.IsConvex, expression.IsConvex);
        Assert.Equal(curve.ValueAt(Rational.Zero) == Rational.Zero, expression.IsPassingThroughOrigin);
    }

    [Fact]
    public void CurveAnalysisVisitorsReportProjectionContracts()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");

        Assert.True(arrival.Negate().ToNonNegative().IsNonNegative);
        Assert.True(arrival.ToUpperNonDecreasing().IsNonDecreasing);
        Assert.True(arrival.ToLowerNonDecreasing().IsNonDecreasing);
        Assert.True(arrival.ToLeftContinuous().IsLeftContinuous);
        Assert.True(arrival.ToRightContinuous().IsRightContinuous);
        Assert.True(arrival.WithZeroOrigin().IsPassingThroughOrigin);
        Assert.True(service.SubAdditiveClosure().IsSubAdditive);
    }

    [Theory]
    [MemberData(nameof(CurveExpressionTestCases))]
    public void CurveAnalysisVisitorsReturnStableValuesForNewProperties(CurveExpression expression)
    {
        var firstRead = (
            expression.IsSuperAdditive,
            expression.IsIncreasing,
            expression.IsContinuous,
            expression.IsUltimatelyFinite,
            expression.IsPlain,
            expression.IsUltimatelyPlain,
            expression.IsUltimatelyAffine,
            expression.IsUltimatelyConstant,
            expression.IsRegularSubAdditive,
            expression.IsRegularSuperAdditive,
            expression.IsRegularConcave,
            expression.IsRegularConvex
        );

        var secondRead = (
            expression.IsSuperAdditive,
            expression.IsIncreasing,
            expression.IsContinuous,
            expression.IsUltimatelyFinite,
            expression.IsPlain,
            expression.IsUltimatelyPlain,
            expression.IsUltimatelyAffine,
            expression.IsUltimatelyConstant,
            expression.IsRegularSubAdditive,
            expression.IsRegularSuperAdditive,
            expression.IsRegularConcave,
            expression.IsRegularConvex
        );

        Assert.Equal(firstRead, secondRead);
    }

    [Fact]
    public void CurveAnalysisVisitorsMatchConcreteCurveNewProperties()
    {
        var curve = new RateLatencyServiceCurve(rate: 3, latency: 1);
        var expression = curve.ToExpression("s");

        Assert.Equal(curve.IsSuperAdditive, expression.IsSuperAdditive);
        Assert.Equal(curve.IsIncreasing, expression.IsIncreasing);
        Assert.Equal(curve.IsContinuous, expression.IsContinuous);
        Assert.Equal(curve.IsUltimatelyFinite, expression.IsUltimatelyFinite);
        Assert.Equal(curve.IsPlain, expression.IsPlain);
        Assert.Equal(curve.IsUltimatelyPlain, expression.IsUltimatelyPlain);
        Assert.Equal(curve.IsUltimatelyAffine, expression.IsUltimatelyAffine);
        Assert.Equal(curve.IsUltimatelyConstant, expression.IsUltimatelyConstant);
        Assert.Equal(curve.IsRegularSubAdditive, expression.IsRegularSubAdditive);
        Assert.Equal(curve.IsRegularSuperAdditive, expression.IsRegularSuperAdditive);
        Assert.Equal(curve.IsRegularConcave, expression.IsRegularConcave);
        Assert.Equal(curve.IsRegularConvex, expression.IsRegularConvex);
    }

    [Fact]
    public void IsSuperAdditiveVisitorAppliesKnownClosureShortcuts()
    {
        var rl = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var rl2 = new RateLatencyServiceCurve(rate: 1, latency: 1);
        var arrivalCurve = new SigmaRhoArrivalCurve(sigma: 2, rho: 1);
        var rlExpr = rl.ToExpression("rl");
        var rl2Expr = rl2.ToExpression("rl2");
        var arrivalExpr = arrivalCurve.ToExpression("a");

        // Negation swaps sub-/super-additivity.
        Assert.Equal(arrivalCurve.Negate().IsSuperAdditive, (-arrivalExpr).IsSuperAdditive);

        // The super-additive closure is always super-additive.
        Assert.True(rlExpr.SuperAdditiveClosure().IsSuperAdditive);

        // The sum of super-additive curves is super-additive.
        var sum = rlExpr.Addition(rl2Expr);
        Assert.Equal(Curve.Addition(rl, rl2).IsSuperAdditive, sum.IsSuperAdditive);

        // The (max,+) convolution of super-additive curves is super-additive.
        var maxPlusConv = rlExpr.MaxPlusConvolution(rl2Expr);
        Assert.Equal(Curve.MaxPlusConvolution(rl, rl2).IsSuperAdditive, maxPlusConv.IsSuperAdditive);
    }

    [Fact]
    public void IsIncreasingVisitorMatchesGroundTruthAcrossShortcutPaths()
    {
        var rl = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var rlExpr = rl.ToExpression("rl");

        // Flat-then-increasing curve is not (strictly) increasing.
        Assert.Equal(rl.IsIncreasing, rlExpr.IsIncreasing);
        Assert.False(rlExpr.IsIncreasing);

        // Negation of a non-increasing curve requires falling back to computation, still correct.
        Assert.Equal(rl.Negate().IsIncreasing, (-rlExpr).IsIncreasing);

        // A pure time shift does not change whether a curve is increasing.
        var delayed = rlExpr.DelayBy(new Rational(2));
        Assert.Equal(rl.DelayBy(new Rational(2)).IsIncreasing, delayed.IsIncreasing);
    }

    [Fact]
    public void UltimateShapePropertiesArePreservedUnderShiftsAndScaling()
    {
        var constantCurve = new ConstantCurve(7);
        var constantExpr = constantCurve.ToExpression("c");

        var delayed = constantExpr.DelayBy(new Rational(4));
        var shifted = constantExpr.VerticalShift(new Rational(-3));
        var scaled = constantExpr.Scale(new Rational(5));
        var negated = -constantExpr;
        var withZeroOrigin = constantExpr.WithZeroOrigin();

        Assert.Equal(constantCurve.DelayBy(new Rational(4)).IsUltimatelyConstant, delayed.IsUltimatelyConstant);
        Assert.Equal(constantCurve.VerticalShift(new Rational(-3)).IsUltimatelyConstant, shifted.IsUltimatelyConstant);
        Assert.Equal(constantCurve.Scale(new Rational(5)).IsUltimatelyConstant, scaled.IsUltimatelyConstant);
        Assert.Equal(constantCurve.Negate().IsUltimatelyConstant, negated.IsUltimatelyConstant);
        Assert.Equal(constantCurve.WithZeroOrigin().IsUltimatelyConstant, withZeroOrigin.IsUltimatelyConstant);

        Assert.True(delayed.IsUltimatelyFinite);
        Assert.True(delayed.IsPlain);
        Assert.True(delayed.IsUltimatelyPlain);
        Assert.True(delayed.IsUltimatelyAffine);

        // Floor/Ceil of an ultimately-constant tail is still ultimately constant.
        var floored = constantExpr.Floor();
        var ceiled = constantExpr.Ceil();
        Assert.Equal(constantCurve.Floor().IsUltimatelyConstant, floored.IsUltimatelyConstant);
        Assert.Equal(constantCurve.Ceil().IsUltimatelyConstant, ceiled.IsUltimatelyConstant);
    }

    [Fact]
    public void FloorAndCeilPreserveNonNegativeAndNonDecreasingButFallBackForContinuity()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1);
        var arrivalExpr = arrival.ToExpression("a");
        var nonNegativeNonDecreasing = arrivalExpr.ToNonNegative().ToUpperNonDecreasing();
        var nonNegativeNonDecreasingCurve = arrival.ToNonNegative().ToUpperNonDecreasing();

        var floored = nonNegativeNonDecreasing.Floor();
        var ceiled = nonNegativeNonDecreasing.Ceil();

        Assert.True(floored.IsNonNegative);
        Assert.True(floored.IsNonDecreasing);
        Assert.True(ceiled.IsNonNegative);
        Assert.True(ceiled.IsNonDecreasing);

        // No shortcut is applied for continuity; the visitor must fall back to computation, and still be correct.
        Assert.Equal(nonNegativeNonDecreasingCurve.Floor().IsLeftContinuous, floored.IsLeftContinuous);
        Assert.Equal(nonNegativeNonDecreasingCurve.Floor().IsRightContinuous, floored.IsRightContinuous);
        Assert.Equal(nonNegativeNonDecreasingCurve.Ceil().IsLeftContinuous, ceiled.IsLeftContinuous);
        Assert.Equal(nonNegativeNonDecreasingCurve.Ceil().IsRightContinuous, ceiled.IsRightContinuous);
    }

    [Theory]
    [MemberData(nameof(WellDefinedExpressionTestCases))]
    public void IsWellDefinedVisitorHandlesImplementedFiniteExpressions(CurveExpression expression)
    {
        Assert.True(expression.IsWellDefined);
    }

    [Theory]
    [MemberData(nameof(UnsupportedWellDefinedExpressionTestCases))]
    public void IsWellDefinedVisitorDocumentsUnsupportedExpressionKinds(CurveExpression expression)
    {
        Assert.Throws<NotImplementedException>(() => expression.IsWellDefined);
    }

    [Fact]
    public void IsWellDefinedVisitorHandlesImplementedInfiniteValueBranches()
    {
        var plusInfinity = Curve.PlusInfinite().ToExpression("p");
        var minusInfinity = Curve.MinusInfinite().ToExpression("m");
        var bothInfinite = BothInfiniteCurve().ToExpression("b");

        Assert.False(Expressions.Addition(plusInfinity, minusInfinity).IsWellDefined);
        Assert.False(Expressions.Convolution(plusInfinity, minusInfinity).IsWellDefined);
        Assert.False(Expressions.Subtraction(plusInfinity, plusInfinity).IsWellDefined);
        Assert.False(bothInfinite.SubAdditiveClosure().IsWellDefined);
        Assert.Throws<NotImplementedException>(() => Expressions.Deconvolution(plusInfinity, plusInfinity).IsWellDefined);
    }

    [Theory]
    [MemberData(nameof(CurveExpressionTestCases))]
    public void RenameCurveVisitorRenamesEveryCurveExpressionKind(CurveExpression expression)
    {
        var renamed = expression.WithName("renamed");

        Assert.Equal(expression.GetType(), renamed.GetType());
        Assert.Equal("renamed", renamed.Name);
    }

    [Theory]
    [MemberData(nameof(CurveExpressionTestCases))]
    public void FormatterVisitorsHandleEveryCurveExpressionKind(CurveExpression expression)
    {
        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToUnicodeString(depth: 0));
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToLatexString(depth: 0));
    }

    [Fact]
    public void FormatterVisitorsRespectNamesAtDepthLimitsAndSignedArguments()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");

#pragma warning disable CS0618 // Type or member is obsolete
        var namedChild = Expressions.Addition(arrival.Negate("alpha2"), service);
        var namedChildLatex = namedChild.ToLatexString(depth: 0);
        Assert.Contains(@"\alpha", namedChildLatex);
        Assert.Contains("2", namedChildLatex);
        Assert.Contains("\u03B12", namedChild.ToUnicodeString(depth: 0));

        var rightNested = Expressions.Subtraction(arrival, Expressions.Addition(arrival, service));
        Assert.Contains(@"\left( ", rightNested.ToLatexString());

        var curveArgument = Expressions.Addition(arrival, service);
        Assert.Contains(@"\left(", curveArgument.ValueAt(new Rational(1)).ToLatexString());
        Assert.Contains("(", curveArgument.ValueAt(new Rational(1)).ToUnicodeString());

        var timeArgument = Expressions.RationalSubtraction(new Rational(3), new Rational(1));
        Assert.Contains("^-", arrival.LeftLimitAt(timeArgument).ToLatexString());
        Assert.Contains("^+", arrival.RightLimitAt(timeArgument).ToLatexString());
        Assert.Contains("^-", arrival.LeftLimitAt(timeArgument).ToUnicodeString());
        Assert.Contains("^+", arrival.RightLimitAt(timeArgument).ToUnicodeString());

        var negativeConstantShiftLatex = arrival.VerticalShift(new Rational(-2)).ToLatexString();
        var negativeConstantShiftUnicode = arrival.VerticalShift(new Rational(-2)).ToUnicodeString();
        var negativeExpressionShiftLatex = arrival.VerticalShift(Expressions.Negate(new Rational(2).ToExpression("k"))).ToLatexString();
        var negativeExpressionShiftUnicode = arrival.VerticalShift(Expressions.Negate(new Rational(2).ToExpression("k"))).ToUnicodeString();

        Assert.Contains(" - ", negativeConstantShiftLatex);
        Assert.DoesNotContain("+ -", negativeConstantShiftLatex);
        Assert.Contains(" - ", negativeConstantShiftUnicode);
        Assert.DoesNotContain("+ -", negativeConstantShiftUnicode);
        Assert.Contains(" - ", negativeExpressionShiftLatex);
        Assert.DoesNotContain("+ -", negativeExpressionShiftLatex);
        Assert.Contains(" - ", negativeExpressionShiftUnicode);
        Assert.DoesNotContain("+ -", negativeExpressionShiftUnicode);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Theory]
    [MemberData(nameof(SubscriptOrSuperscriptExpressionTestCases))]
    public void LatexSubscriptDetectorRecognizesPostfixSensitiveExpressions(CurveExpression expression)
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");

        Assert.True(LatexFormatterVisitor.ContainsSubscriptOrSuperscript(expression));
        Assert.False(LatexFormatterVisitor.ContainsSubscriptOrSuperscript(arrival));
    }

    [Fact]
    public void CurveAnalysisVisitorsRejectPlaceholders()
    {
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsSubAdditive);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsLeftContinuous);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsRightContinuous);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsNonNegative);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsNonDecreasing);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsConcave);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsConvex);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsPassingThroughOrigin);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsWellDefined);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsSuperAdditive);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsIncreasing);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsUltimatelyFinite);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsPlain);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsUltimatelyPlain);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsUltimatelyAffine);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsUltimatelyConstant);
    }

    [Fact]
    public void RenameAndFormatterVisitorsHandleCurvePlaceholders()
    {
        var placeholder = Expressions.Placeholder("p");
        var renamed = placeholder.WithName("q");

        Assert.IsType<CurvePlaceholderExpression>(renamed);
        Assert.Equal("q", renamed.Name);
        Assert.Equal("p", placeholder.ToUnicodeString());
        Assert.Equal("p", placeholder.ToLatexString());
    }
}
