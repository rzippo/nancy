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
    public void KnownResultsTest(List<Rational> maximumArgs)
    {
        // first compute through the normal Rational method
        var expected = maximumArgs[0];
        foreach (var maxArg in maximumArgs)
            expected = Rational.Max(expected, maxArg);
        
        // then compute through expression
        var maxArgsExpr = maximumArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        // todo: add and test collection version of maximum
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

}