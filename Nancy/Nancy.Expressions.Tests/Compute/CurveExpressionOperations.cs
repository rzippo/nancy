using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CurveExpressionOperations
{
    public static List<Curve> UnaryCurveCases =
    [
        new RateLatencyServiceCurve(rate: 2, latency: 3),
        new Curve(
            baseSequence: new Sequence([
                Point.Origin(),
                new Segment(0, 2, 0, 1),
                new Point(2, 2),
                new Segment(2, 3, 2, -1),
                new Point(3, 1),
                new Segment(3, 4, 1, 1)
            ]),
            pseudoPeriodStart: 3,
            pseudoPeriodLength: 1,
            pseudoPeriodHeight: 1
        ),
    ];

    public static IEnumerable<object[]> UnaryCurveTestCases
        => UnaryCurveCases.ToXUnitTestCases();

    public static List<(Curve a, Curve b)> CurvePairs =
    [
        (
            new SigmaRhoArrivalCurve(sigma: 4, rho: 3),
            new RateLatencyServiceCurve(rate: 4, latency: 3)
        ),
        (
            new RateLatencyServiceCurve(rate: 3, latency: 1),
            new RateLatencyServiceCurve(rate: 2, latency: 0)
        ),
    ];

    public static IEnumerable<object[]> CurvePairTestCases
        => CurvePairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(UnaryCurveTestCases))]
    public void NegateExpressionComputesConcreteNegation(Curve curve)
    {
        var expression = Expressions.Negate(curve.ToExpression());
        var concreteExpression = Expressions.Negate(curve);
        var instanceExpression = curve.ToExpression().Negate();
        var expected = curve.Negate();

        Assert.IsType<NegateExpression>(expression);
        Assert.IsType<NegateExpression>(concreteExpression);
        Assert.IsType<NegateExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Theory]
    [MemberData(nameof(UnaryCurveTestCases))]
    public void SuperAdditiveClosureExpressionComputesConcreteClosure(Curve curve)
    {
        var expression = Expressions.SuperAdditiveClosure(curve.ToExpression());
        var concreteExpression = Expressions.SuperAdditiveClosure(curve);
        var instanceExpression = curve.ToExpression().SuperAdditiveClosure();
        var expected = curve.SuperAdditiveClosure();

        Assert.IsType<SuperAdditiveClosureExpression>(expression);
        Assert.IsType<SuperAdditiveClosureExpression>(concreteExpression);
        Assert.IsType<SuperAdditiveClosureExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Theory]
    [MemberData(nameof(CurvePairTestCases))]
    public void MaxPlusDeconvolutionExpressionComputesConcreteOperation(Curve a, Curve b)
    {
        var expression = Expressions.MaxPlusDeconvolution(a.ToExpression(), b.ToExpression());
        var concreteExpression = Expressions.MaxPlusDeconvolution(a, b);
        var mixedExpression = Expressions.MaxPlusDeconvolution(a, b.ToExpression());
        var instanceExpression = a.ToExpression().MaxPlusDeconvolution(b);
        var expected = Curve.MaxPlusDeconvolution(a, b);

        Assert.IsType<MaxPlusDeconvolutionExpression>(expression);
        Assert.IsType<MaxPlusDeconvolutionExpression>(concreteExpression);
        Assert.IsType<MaxPlusDeconvolutionExpression>(mixedExpression);
        Assert.IsType<MaxPlusDeconvolutionExpression>(instanceExpression);
        Assert.True(Curve.Equivalent(expected, expression.Compute()));
        Assert.True(Curve.Equivalent(expected, concreteExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, mixedExpression.Compute()));
        Assert.True(Curve.Equivalent(expected, instanceExpression.Compute()));
    }

    [Theory]
    [MemberData(nameof(CurvePairTestCases))]
    public void VerticalDeviationExpressionComputesConcreteDeviation(Curve a, Curve b)
    {
        var expression = Expressions.VerticalDeviation(a.ToExpression(), b.ToExpression());
        var concreteExpression = Expressions.VerticalDeviation(a, b);
        var leftConcreteExpression = Expressions.VerticalDeviation(a, b.ToExpression());
        var rightConcreteExpression = Expressions.VerticalDeviation(a.ToExpression(), b);
        var expected = Curve.VerticalDeviation(a, b);

        Assert.IsType<VerticalDeviationExpression>(expression);
        Assert.IsType<VerticalDeviationExpression>(concreteExpression);
        Assert.IsType<VerticalDeviationExpression>(leftConcreteExpression);
        Assert.IsType<VerticalDeviationExpression>(rightConcreteExpression);
        Assert.Equal(expected, expression.Compute());
        Assert.Equal(expected, concreteExpression.Compute());
        Assert.Equal(expected, leftConcreteExpression.Compute());
        Assert.Equal(expected, rightConcreteExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(CurvePairTestCases))]
    public void HorizontalDeviationExpressionComputesConcreteDeviation(Curve a, Curve b)
    {
        var expression = Expressions.HorizontalDeviation(a.ToExpression(), b.ToExpression());
        var concreteExpression = Expressions.HorizontalDeviation(a, b);
        var leftConcreteExpression = Expressions.HorizontalDeviation(a, b.ToExpression());
        var rightConcreteExpression = Expressions.HorizontalDeviation(a.ToExpression(), b);
        var expected = Curve.HorizontalDeviation(a, b);

        Assert.IsType<HorizontalDeviationExpression>(expression);
        Assert.IsType<HorizontalDeviationExpression>(concreteExpression);
        Assert.IsType<HorizontalDeviationExpression>(leftConcreteExpression);
        Assert.IsType<HorizontalDeviationExpression>(rightConcreteExpression);
        Assert.Equal(expected, expression.Compute());
        Assert.Equal(expected, concreteExpression.Compute());
        Assert.Equal(expected, leftConcreteExpression.Compute());
        Assert.Equal(expected, rightConcreteExpression.Compute());
    }

    [Fact]
    public void ConvexAndNonNegativeVisitorsEvaluateRepresentativePaths()
    {
        var service = new RateLatencyServiceCurve(rate: 2, latency: 3);
        var fasterService = new RateLatencyServiceCurve(rate: 4, latency: 1);
        var serviceExpression = service.ToExpression();
        var fasterServiceExpression = fasterService.ToExpression();

        Assert.Equal(service.IsConvex, serviceExpression.IsConvex);
        Assert.Equal(service.IsNonNegative, serviceExpression.IsNonNegative);

        var negatedService = serviceExpression.Negate();
        Assert.Equal(negatedService.Compute().IsConvex, negatedService.IsConvex);
        Assert.Equal(negatedService.Compute().IsNonNegative, negatedService.IsNonNegative);

        var nonNegativeProjection = negatedService.ToNonNegative();
        Assert.True(nonNegativeProjection.IsNonNegative);
        Assert.Equal(nonNegativeProjection.Compute().IsConvex, nonNegativeProjection.IsConvex);

        var sum = Expressions.Addition(serviceExpression, fasterServiceExpression);
        Assert.Equal(sum.Compute().IsConvex, sum.IsConvex);
        Assert.True(sum.IsNonNegative);

        var minimum = Expressions.Minimum(serviceExpression, fasterServiceExpression);
        Assert.Equal(minimum.Compute().IsConvex, minimum.IsConvex);
        Assert.True(minimum.IsNonNegative);

        var maximum = Expressions.Maximum(serviceExpression, fasterServiceExpression);
        Assert.Equal(maximum.Compute().IsConvex, maximum.IsConvex);
        Assert.True(maximum.IsNonNegative);

        var shifted = serviceExpression.HorizontalShift(new Rational(-1));
        Assert.Equal(shifted.Compute().IsConvex, shifted.IsConvex);
        Assert.Equal(shifted.Compute().IsNonNegative, shifted.IsNonNegative);
    }
}
