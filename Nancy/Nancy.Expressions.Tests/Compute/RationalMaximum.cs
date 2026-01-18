using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalMaximum
{
    private readonly ITestOutputHelper output;

    public RationalMaximum(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<List<Rational>> MaximumArgs =
    [
        [1, 1],
        [2, 3],
        [2, 3, 5],
        [new (2, 4), new (3, 4), new (5, 4)],
        [new (1, 4), new (7, 4), new (9, 4)],
        [new (1, 4), new (2, 4), new (3, 4), new (5, 4)],
    ];

    public static IEnumerable<object[]> MaximumArgsTestCases
        => MaximumArgs.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(MaximumArgsTestCases))]
    public void Iterative_Rationals(List<Rational> maximumArgs)
    {
        // first compute through the normal Rational method
        var expected = maximumArgs[0];
        foreach (var maxArg in maximumArgs)
            expected = Rational.Max(expected, maxArg);
        
        // then compute through expression
        var expression = RationalExpression.Max(maximumArgs[0], maximumArgs[1]);
        foreach (var re in maximumArgs.Skip(2))
        {
            expression = expression.Max(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MaximumArgsTestCases))]
    public void Iterative_Expressions(List<Rational> maximumArgs)
    {
        // first compute through the normal Rational method
        var expected = maximumArgs[0];
        foreach (var maxArg in maximumArgs)
            expected = Rational.Max(expected, maxArg);
        
        // then compute through expression
        var maxArgsExpr = maximumArgs
            .Select(r => Expressions.FromRational(r))
            .ToList();
        var expression = RationalExpression.Max(maxArgsExpr[0], maxArgsExpr[1]);
        foreach (var re in maxArgsExpr.Skip(2))
        {
            expression = expression.Max(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MaximumArgsTestCases))]
    public void Collection_Rationals(List<Rational> maximumArgs)
    {
        // Test Expressions.RationalMaximum with collection of Rational
        var expected = maximumArgs[0];
        foreach (var maxArg in maximumArgs)
            expected = Rational.Max(expected, maxArg);
        
        var names = Enumerable.Range(0, maximumArgs.Count)
            .Select(i => $"r{i}")
            .ToList();
        
        var expression = Expressions.RationalMaximum(maximumArgs, names);
        
        output.WriteLine($"Collection Rational: {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MaximumArgsTestCases))]
    public void Collection_Expressions(List<Rational> maximumArgs)
    {
        // Test Expressions.RationalMaximum with collection of RationalExpression
        var expected = maximumArgs[0];
        foreach (var maxArg in maximumArgs)
            expected = Rational.Max(expected, maxArg);
        
        var maxArgsExpr = maximumArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        
        var expression = Expressions.RationalMaximum(maxArgsExpr);
        
        output.WriteLine($"Collection Expression: {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Fact]
    public void StaticMethodMixedTypes_Test()
    {
        // Test Expressions.RationalMaximum with mixed Rational and RationalExpression
        var r1 = new Rational(3);
        var r2 = new Rational(5);
        var expected = Rational.Max(r1, r2);
        
        var expr2 = Expressions.FromRational(r2);
        var expression = Expressions.RationalMaximum(r1, expr2);
        
        output.WriteLine($"Mixed types (Rational, Expression): {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

}