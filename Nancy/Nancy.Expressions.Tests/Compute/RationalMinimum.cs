using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalMinimum
{
    private readonly ITestOutputHelper output;

    public RationalMinimum(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<List<Rational>> MinimumArgs =
    [
        [1, 1],
        [2, 3],
        [2, 3, 5],
        [new (2, 4), new (3, 4), new (5, 4)],
        [new (1, 4), new (7, 4), new (9, 4)],
        [new (1, 4), new (2, 4), new (3, 4), new (5, 4)],
    ];

    public static IEnumerable<object[]> MinimumArgsTestCases
        => MinimumArgs.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(MinimumArgsTestCases))]
    public void Iterative_Rationals(List<Rational> minimumArgs)
    {
        // first compute through the normal Rational method
        var expected = minimumArgs[0];
        foreach (var minArg in minimumArgs)
            expected = Rational.Min(expected, minArg);
        
        // then compute through expression
        var expression = RationalExpression.Min(minimumArgs[0], minimumArgs[1]);
        foreach (var re in minimumArgs.Skip(2))
        {
            expression = expression.Min(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MinimumArgsTestCases))]
    public void Iterative_Expressions(List<Rational> minimumArgs)
    {
        // first compute through the normal Rational method
        var expected = minimumArgs[0];
        foreach (var minArg in minimumArgs)
            expected = Rational.Min(expected, minArg);
        
        // then compute through expression
        var minArgsExpr = minimumArgs
            .Select(r => Expressions.FromRational(r))
            .ToList();
        var expression = RationalExpression.Min(minArgsExpr[0], minArgsExpr[1]);
        foreach (var re in minArgsExpr.Skip(2))
        {
            expression = expression.Min(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MinimumArgsTestCases))]
    public void Collection_Rationals(List<Rational> minimumArgs)
    {
        // Test Expressions.RationalMinimum with collection of Rational
        var expected = minimumArgs[0];
        foreach (var minArg in minimumArgs)
            expected = Rational.Min(expected, minArg);
        
        var names = Enumerable.Range(0, minimumArgs.Count)
            .Select(i => $"r{i}")
            .ToList();
        
        var expression = Expressions.RationalMinimum(minimumArgs, names);
        
        output.WriteLine($"Collection Rational: {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Theory]
    [MemberData(nameof(MinimumArgsTestCases))]
    public void Collection_Expressions(List<Rational> minimumArgs)
    {
        // Test Expressions.RationalMinimum with collection of RationalExpression
        var expected = minimumArgs[0];
        foreach (var minArg in minimumArgs)
            expected = Rational.Min(expected, minArg);
        
        var minArgsExpr = minimumArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        
        var expression = Expressions.RationalMinimum(minArgsExpr);
        
        output.WriteLine($"Collection Expression: {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

    [Fact]
    public void StaticMethodMixedTypes_Test()
    {
        // Test Expressions.RationalMinimum with mixed Rational and RationalExpression
        var r1 = new Rational(3);
        var r2 = new Rational(5);
        var expected = Rational.Min(r1, r2);
        
        var expr2 = Expressions.FromRational(r2);
        var expression = Expressions.RationalMinimum(r1, expr2);
        
        output.WriteLine($"Mixed types (Rational, Expression): {expected}");
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

}