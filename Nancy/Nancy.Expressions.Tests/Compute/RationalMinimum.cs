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
    public void KnownResultsTest(List<Rational> minimumArgs)
    {
        // first compute through the normal Rational method
        var expected = minimumArgs[0];
        foreach (var minArg in minimumArgs)
            expected = Rational.Min(expected, minArg);
        
        // then compute through expression
        var minArgsExpr = minimumArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        // todo: add and test collection version of minimum
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

}