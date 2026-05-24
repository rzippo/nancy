using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalLeastCommonMultiple
{
    private readonly ITestOutputHelper output;

    public RationalLeastCommonMultiple(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<List<Rational>> LcmArgs =
    [
        [1, 1],
        [2, 3],
        [2, 3, 5],
        [new (2, 4), new (3, 4), new (5, 4)],
        [new (1, 4), new (7, 4), new (9, 4)],
        [new (1, 4), new (2, 4), new (3, 4), new (5, 4)],
    ];

    public static IEnumerable<object[]> LcmArgsTestCases
        => LcmArgs.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(LcmArgsTestCases))]
    public void KnownResultsTest(List<Rational> lcmArgs)
    {
        // first compute through the normal Rational method
        var expected = lcmArgs[0];
        foreach (var lcmArg in lcmArgs.Skip(1))
            expected = Rational.LeastCommonMultiple(expected, lcmArg);
        
        // then compute through expression
        var lcmArgsExpr = lcmArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        // todo: add and test collection version of LCM
        var expression = RationalExpression.LeastCommonMultiple(lcmArgsExpr[0], lcmArgsExpr[1]);
        foreach (var re in lcmArgsExpr.Skip(2))
        {
            expression = expression.LeastCommonMultiple(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(LcmArgsTestCases))]
    public void ConcreteCollectionComputesKnownResult(List<Rational> lcmArgs)
    {
        var expected = lcmArgs[0];
        foreach (var lcmArg in lcmArgs.Skip(1))
            expected = Rational.LeastCommonMultiple(expected, lcmArg);

        var names = Enumerable.Range(0, lcmArgs.Count)
            .Select(index => $"r{index}")
            .ToList();
        var expression = Expressions.LeastCommonMultiple(lcmArgs, names);

        Assert.IsType<RationalLeastCommonMultipleExpression>(expression);
        Assert.Equal(expected, expression.Compute());
    }

    [Fact]
    public void ConcreteAndMixedOverloadsComputeKnownResult()
    {
        var a = new Rational(6);
        var b = new Rational(9);
        var c = new Rational(15);
        var expected = Rational.LeastCommonMultiple(Rational.LeastCommonMultiple(a, b), c);

        var concreteExpression = Expressions.LeastCommonMultiple(a, b);
        var mixedLeftExpression = Expressions.LeastCommonMultiple(c, concreteExpression);
        var mixedRightExpression = Expressions.LeastCommonMultiple(concreteExpression, c);

        Assert.IsType<RationalLeastCommonMultipleExpression>(concreteExpression);
        Assert.IsType<RationalLeastCommonMultipleExpression>(mixedLeftExpression);
        Assert.IsType<RationalLeastCommonMultipleExpression>(mixedRightExpression);
        Assert.Equal(Rational.LeastCommonMultiple(a, b), concreteExpression.Compute());
        Assert.Equal(expected, mixedLeftExpression.Compute());
        Assert.Equal(expected, mixedRightExpression.Compute());
    }

}
