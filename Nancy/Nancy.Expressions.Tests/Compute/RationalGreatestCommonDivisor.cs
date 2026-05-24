using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalGreatestCommonDivisor
{
    private readonly ITestOutputHelper output;

    public RationalGreatestCommonDivisor(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<List<Rational>> GcdArgs =
    [
        [1, 1],
        [2, 3],
        [2, 3, 5],
        [new (2, 4), new (3, 4), new (5, 4)],
        [new (1, 4), new (7, 4), new (9, 4)],
        [new (1, 4), new (2, 4), new (3, 4), new (5, 4)],
    ];

    public static IEnumerable<object[]> GcdArgsTestCases
        => GcdArgs.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(GcdArgsTestCases))]
    public void KnownResultsTest(List<Rational> gcdArgs)
    {
        // first compute through the normal Rational method
        var expected = gcdArgs[0];
        foreach (var gcdArg in gcdArgs.Skip(1))
            expected = Rational.GreatestCommonDivisor(expected, gcdArg);
        
        // then compute through expression
        var gcdArgsExpr = gcdArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        var expression = RationalExpression.GreatestCommonDivisor(gcdArgsExpr[0], gcdArgsExpr[1]);
        foreach (var re in gcdArgsExpr.Skip(2))
        {
            expression = expression.GreatestCommonDivisor(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(GcdArgsTestCases))]
    public void ConcreteCollectionComputesKnownResult(List<Rational> gcdArgs)
    {
        var expected = gcdArgs[0];
        foreach (var gcdArg in gcdArgs.Skip(1))
            expected = Rational.GreatestCommonDivisor(expected, gcdArg);

        var names = Enumerable.Range(0, gcdArgs.Count)
            .Select(index => $"r{index}")
            .ToList();
        var expression = Expressions.GreatestCommonDivisor(gcdArgs, names);

        Assert.IsType<RationalGreatestCommonDivisorExpression>(expression);
        Assert.Equal(expected, expression.Compute());
    }

    [Fact]
    public void ConcreteAndMixedOverloadsComputeKnownResult()
    {
        var a = new Rational(6);
        var b = new Rational(9);
        var c = new Rational(15);
        var expected = Rational.GreatestCommonDivisor(Rational.GreatestCommonDivisor(a, b), c);

        var concreteExpression = Expressions.GreatestCommonDivisor(a, b);
        var mixedLeftExpression = Expressions.GreatestCommonDivisor(c, concreteExpression);
        var mixedRightExpression = Expressions.GreatestCommonDivisor(concreteExpression, c);

        Assert.IsType<RationalGreatestCommonDivisorExpression>(concreteExpression);
        Assert.IsType<RationalGreatestCommonDivisorExpression>(mixedLeftExpression);
        Assert.IsType<RationalGreatestCommonDivisorExpression>(mixedRightExpression);
        Assert.Equal(Rational.GreatestCommonDivisor(a, b), concreteExpression.Compute());
        Assert.Equal(expected, mixedLeftExpression.Compute());
        Assert.Equal(expected, mixedRightExpression.Compute());
    }

}
