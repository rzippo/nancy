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
            expression.IsZeroAtZero
        );

        var secondRead = (
            expression.IsSubAdditive,
            expression.IsLeftContinuous,
            expression.IsRightContinuous,
            expression.IsNonNegative,
            expression.IsNonDecreasing,
            expression.IsConcave,
            expression.IsConvex,
            expression.IsZeroAtZero
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
        Assert.Equal(curve.ValueAt(Rational.Zero) == Rational.Zero, expression.IsZeroAtZero);
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
        Assert.True(arrival.WithZeroOrigin().IsZeroAtZero);
        Assert.True(service.SubAdditiveClosure().IsSubAdditive);
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
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsZeroAtZero);
        Assert.Throws<InvalidOperationException>(() => Expressions.Placeholder("p").IsWellDefined);
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
