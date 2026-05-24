using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalArithmeticExpressions
{
    public static List<(Rational a, Rational b, Rational c)> AdditionCases =
    [
        (1, 2, 3),
        (new Rational(1, 2), new Rational(2, 3), new Rational(-1, 6)),
        (new Rational(-5), new Rational(7), new Rational(3, 4)),
    ];

    public static IEnumerable<object[]> AdditionTestCases
        => AdditionCases.ToXUnitTestCases();

    public static List<Rational> InvertCases =
    [
        2,
        new Rational(3, 4),
        new Rational(-5, 7),
    ];

    public static IEnumerable<object[]> InvertTestCases
        => InvertCases.ToXUnitTestCases();

    public static List<(Rational a, Rational b, Rational c)> ProductCases =
    [
        (2, 3, 5),
        (new Rational(1, 2), new Rational(-2, 3), new Rational(9, 4)),
        (new Rational(-5), new Rational(-7), new Rational(3, 8)),
    ];

    public static IEnumerable<object[]> ProductTestCases
        => ProductCases.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(AdditionTestCases))]
    public void RationalAdditionExpressionComputesSum(Rational a, Rational b, Rational c)
    {
        var expression = Expressions.RationalAddition(a.ToExpression(), b.ToExpression())
            .Addition(c);

        Assert.IsType<RationalAdditionExpression>(expression);
        Assert.Equal(a + b + c, expression.Compute());
    }

    [Theory]
    [MemberData(nameof(AdditionTestCases))]
    public void RationalAdditionConcreteOverloadsComputeSum(Rational a, Rational b, Rational c)
    {
        var concreteExpression = Expressions.RationalAddition(a, b);
        var mixedExpression = Expressions.RationalAddition(c, concreteExpression);

        Assert.IsType<RationalAdditionExpression>(concreteExpression);
        Assert.IsType<RationalAdditionExpression>(mixedExpression);
        Assert.Equal(a + b, concreteExpression.Compute());
        Assert.Equal(a + b + c, mixedExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(AdditionTestCases))]
    public void RationalAdditionCollectionComputesSum(Rational a, Rational b, Rational c)
    {
        var numbers = new List<Rational> { a, b, c };
        var names = Enumerable.Range(0, numbers.Count)
            .Select(index => $"r{index}")
            .ToList();

        var expression = Expressions.RationalAddition(numbers, names);

        Assert.IsType<RationalAdditionExpression>(expression);
        Assert.Equal(numbers.Aggregate(Rational.Zero, (sum, number) => sum + number), expression.Compute());
    }

    [Theory]
    [MemberData(nameof(InvertTestCases))]
    public void InvertExpressionComputesReciprocal(Rational value)
    {
        var expression = Expressions.Invert(value.ToExpression());

        Assert.IsType<InvertRationalExpression>(expression);
        Assert.Equal(Rational.Invert(value), expression.Compute());
    }

    [Theory]
    [MemberData(nameof(InvertTestCases))]
    public void InvertConcreteAndInstanceOverloadsComputeReciprocal(Rational value)
    {
        var concreteExpression = Expressions.Invert(value);
        var instanceExpression = value.ToExpression().Invert();

        Assert.IsType<InvertRationalExpression>(concreteExpression);
        Assert.IsType<InvertRationalExpression>(instanceExpression);
        Assert.Equal(Rational.Invert(value), concreteExpression.Compute());
        Assert.Equal(Rational.Invert(value), instanceExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(InvertTestCases))]
    public void NegateExpressionComputesOpposite(Rational value)
    {
        var expression = Expressions.Negate(value.ToExpression());
        var concreteExpression = Expressions.Negate(value);
        var instanceExpression = value.ToExpression().Negate();

        Assert.IsType<NegateRationalExpression>(expression);
        Assert.IsType<NegateRationalExpression>(concreteExpression);
        Assert.IsType<NegateRationalExpression>(instanceExpression);
        Assert.Equal(Rational.Negate(value), expression.Compute());
        Assert.Equal(Rational.Negate(value), concreteExpression.Compute());
        Assert.Equal(Rational.Negate(value), instanceExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(ProductTestCases))]
    public void RationalProductExpressionComputesProduct(Rational a, Rational b, Rational c)
    {
        var expression = Expressions.Product(a.ToExpression(), b.ToExpression())
            .Product(c);

        Assert.IsType<RationalProductExpression>(expression);
        Assert.Equal(a * b * c, expression.Compute());
    }

    [Theory]
    [MemberData(nameof(ProductTestCases))]
    public void RationalProductConcreteOverloadsComputeProduct(Rational a, Rational b, Rational c)
    {
        var concreteExpression = Expressions.Product(a, b);
        var mixedLeftExpression = Expressions.Product(c, concreteExpression);
        var mixedRightExpression = Expressions.Product(concreteExpression, c);

        Assert.IsType<RationalProductExpression>(concreteExpression);
        Assert.IsType<RationalProductExpression>(mixedLeftExpression);
        Assert.IsType<RationalProductExpression>(mixedRightExpression);
        Assert.Equal(a * b, concreteExpression.Compute());
        Assert.Equal(a * b * c, mixedLeftExpression.Compute());
        Assert.Equal(a * b * c, mixedRightExpression.Compute());
    }

    [Theory]
    [MemberData(nameof(ProductTestCases))]
    public void RationalProductCollectionComputesProduct(Rational a, Rational b, Rational c)
    {
        var numbers = new List<Rational> { a, b, c };
        var names = Enumerable.Range(0, numbers.Count)
            .Select(index => $"r{index}")
            .ToList();
        var expression = Expressions.RationalProduct(numbers, names);

        Assert.IsType<RationalProductExpression>(expression);
        Assert.Equal(numbers.Aggregate(Rational.One, (product, number) => product * number), expression.Compute());
    }
}
