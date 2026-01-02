using System.Collections.Generic;
using Unipi.Nancy.Plots;
using Xunit;

namespace Unipi.Nancy.Tests.Plots;

public class NamesParsing
{
    public static List<(string expression, int n, List<string> names)> KnownParseTuples =
    [
        (
            "[b1, b2, b3]",
            3,
            ["b1", "b2", "b3"]
        ),
        (
            "new []{b1, b2, b3}",
            3,
            ["b1", "b2", "b3"]
        ),
        (
            "new Curve[]{b1, b2, b3}",
            3,
            ["b1", "b2", "b3"]
        ),
        (
            "new List<Curve>{b1, b2, b3}",
            3,
            ["b1", "b2", "b3"]
        ),
        (
            "listName",
            3,
            ["f", "g", "h"]
        ),
        (
            "[f, f + 1, f - 1]",
            3,
            ["f", "f + 1", "f - 1"]
        ),
    ];

    public static IEnumerable<object[]> KnownParseTestCases
        => KnownParseTuples.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownParseTestCases))]
    public void ParsingMatch(string expression, int n, List<string> names)
    {
        var parsedNames = ParametersNameParsing.ParseNames(expression, n);
        Assert.Equal(names, parsedNames);
    }
}