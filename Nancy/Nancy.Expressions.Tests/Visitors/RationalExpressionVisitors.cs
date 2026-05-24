using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class RationalExpressionVisitors
{
    private static List<RationalExpression> RationalExpressions()
    {
        var arrival = new SigmaRhoArrivalCurve(sigma: 2, rho: 1).ToExpression("a");
        var service = new RateLatencyServiceCurve(rate: 3, latency: 0).ToExpression("s");
        var x = new Rational(2).ToExpression("x");
        var y = new Rational(3).ToExpression("y");

        return
        [
            Expressions.HorizontalDeviation(arrival, service),
            Expressions.VerticalDeviation(arrival, service),
            arrival.ValueAt(new Rational(1)),
            arrival.LeftLimitAt(new Rational(1)),
            arrival.RightLimitAt(new Rational(1)),
            Expressions.RationalAddition(x, y),
            Expressions.RationalSubtraction(x, y),
            Expressions.Product(x, y),
            Expressions.Division(y, x),
            Expressions.LeastCommonMultiple(new Rational(6), new Rational(9)),
            Expressions.GreatestCommonDivisor(new Rational(6), new Rational(9)),
            Expressions.RationalMinimum(x, y),
            Expressions.RationalMaximum(x, y),
            Expressions.FromRational(new Rational(5), "five"),
            Expressions.Negate(x),
            Expressions.Invert(y),
        ];
    }

    public static IEnumerable<object[]> RationalExpressionTestCases
        => RationalExpressions().ToXUnitTestCases();

    private static List<(RationalExpression expression, Rational expected)> RationalExpressionComputations()
    {
        var arrivalCurve = new SigmaRhoArrivalCurve(sigma: 2, rho: 1);
        var serviceCurve = new RateLatencyServiceCurve(rate: 3, latency: 0);
        var arrival = arrivalCurve.ToExpression("a");
        var service = serviceCurve.ToExpression("s");
        var xValue = new Rational(2);
        var yValue = new Rational(3);
        var x = xValue.ToExpression("x");
        var y = yValue.ToExpression("y");

        return
        [
            (Expressions.HorizontalDeviation(arrival, service), Curve.HorizontalDeviation(arrivalCurve, serviceCurve)),
            (Expressions.VerticalDeviation(arrival, service), Curve.VerticalDeviation(arrivalCurve, serviceCurve)),
            (arrival.ValueAt(new Rational(1)), arrivalCurve.ValueAt(new Rational(1))),
            (arrival.LeftLimitAt(new Rational(1)), arrivalCurve.LeftLimitAt(new Rational(1))),
            (arrival.RightLimitAt(new Rational(1)), arrivalCurve.RightLimitAt(new Rational(1))),
            (Expressions.RationalAddition(x, y), xValue + yValue),
            (Expressions.RationalSubtraction(x, y), xValue - yValue),
            (Expressions.Product(x, y), xValue * yValue),
            (Expressions.Division(y, x), yValue / xValue),
            (Expressions.LeastCommonMultiple(new Rational(6), new Rational(9)), Rational.LeastCommonMultiple(6, 9)),
            (Expressions.GreatestCommonDivisor(new Rational(6), new Rational(9)), Rational.GreatestCommonDivisor(6, 9)),
            (Expressions.RationalMinimum(x, y), Rational.Min(xValue, yValue)),
            (Expressions.RationalMaximum(x, y), Rational.Max(xValue, yValue)),
            (Expressions.FromRational(new Rational(5), "five"), new Rational(5)),
            (Expressions.Negate(x), Rational.Negate(xValue)),
            (Expressions.Invert(y), Rational.Invert(yValue)),
        ];
    }

    public static IEnumerable<object[]> RationalExpressionComputationTestCases
        => RationalExpressionComputations().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RationalExpressionTestCases))]
    public void RenameRationalVisitorRenamesEveryRationalExpressionKind(RationalExpression expression)
    {
        var renamed = expression.WithName("renamed");

        Assert.Equal(expression.GetType(), renamed.GetType());
        Assert.Equal("renamed", renamed.Name);
    }

    [Theory]
    [MemberData(nameof(RationalExpressionTestCases))]
    public void FormatterVisitorsHandleEveryRationalExpressionKind(RationalExpression expression)
    {
        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToUnicodeString(depth: 0));
        Assert.NotEmpty(expression.ToUnicodeString(showRationalsAsName: true));
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToLatexString(depth: 0));
        Assert.NotEmpty(expression.ToLatexString(showRationalsAsName: true));
    }

    [Theory]
    [MemberData(nameof(RationalExpressionComputationTestCases))]
    public void RationalExpressionEvaluatorComputesEveryRationalExpressionKind(
        RationalExpression expression,
        Rational expected)
    {
        Assert.Equal(expected, expression.Compute());
        Assert.Equal(expected, expression.Value);
    }

    [Fact]
    public void FormatterVisitorsRespectRationalNamesAndValues()
    {
        var x = new Rational(2).ToExpression("x");
        var y = new Rational(3).ToExpression("y");
        var fraction = Expressions.FromRational(new Rational(3, 2), "alpha2");
        var integer = Expressions.FromRational(new Rational(4), "n");
        var namedChild = Expressions.Product(Expressions.Negate(x, "alpha2"), y);

        Assert.Contains(@"\frac", fraction.ToLatexString(showRationalsAsName: false));
        Assert.Equal("4", integer.ToLatexString(showRationalsAsName: false));
        var namedFractionLatex = fraction.ToLatexString(showRationalsAsName: true);
        Assert.Contains(@"\alpha", namedFractionLatex);
        Assert.Contains("2", namedFractionLatex);
        Assert.DoesNotContain(@"\frac", namedFractionLatex);
        Assert.Contains("3/2", fraction.ToUnicodeString(showRationalsAsName: false));
        Assert.Contains("\u03B12", fraction.ToUnicodeString(showRationalsAsName: true));

        Assert.Contains(@"\alpha", namedChild.ToLatexString(depth: 0));
        Assert.Contains("\u03B12", namedChild.ToUnicodeString(depth: 0));
    }

    [Fact]
    public void RenameAndFormatterVisitorsHandleRationalPlaceholders()
    {
        var placeholder = Expressions.RationalPlaceholder("r");
        var renamed = placeholder.WithName("q");

        Assert.IsType<RationalPlaceholderExpression>(renamed);
        Assert.Equal("q", renamed.Name);
        Assert.Equal("r", placeholder.ToUnicodeString());
        Assert.Equal("r", placeholder.ToLatexString());
    }
}
