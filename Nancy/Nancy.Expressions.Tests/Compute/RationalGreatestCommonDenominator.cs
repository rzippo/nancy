using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalGreatestCommonDivisor
{
    private readonly ITestOutputHelper output;

    public RationalGreatestCommonDivisor(ITestOutputHelper output)
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
        foreach (var lcmArg in lcmArgs)
            expected = Rational.GreatestCommonDivisor(expected, lcmArg);
        
        // then compute through expression
        // todo: add and use Rational.ToExpression()
        var lcmArgsExpr = lcmArgs.Select(r => Expressions.FromRational(r))
            .ToList();
        // todo: add and test collection version of LCM
        var expression = RationalExpression.GreatestCommonDivisor(lcmArgsExpr[0], lcmArgsExpr[1]);
        foreach (var re in lcmArgsExpr.Skip(2))
        {
            expression = expression.GreatestCommonDivisor(re);
        }
        
        output.WriteLine(expected.ToString());
        output.WriteLine(expression.ToString());
        expression.Compute();
        output.WriteLine(expression.Value.ToString());
        
        Assert.Equal(expected, expression.Value);
    }

}