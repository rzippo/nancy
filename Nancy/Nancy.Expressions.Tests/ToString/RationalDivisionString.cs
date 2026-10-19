using System.Collections.Generic;
using Xunit;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

/// <summary>
/// The string forms of a division between numbers: a negation of a number reads as the negative number, and a negative integer needs no parentheses before the division.
/// </summary>
public class RationalDivisionString
{
    private static RationalExpression Number(Rational value, string name = "")
        => Expressions.FromRational(value, name);

    public static List<(string id, RationalExpression expression, string unicode, string latex)> Examples =
    [
        ("negation of 3 over 2", -Number(3) / Number(2), "-3/2", @"\frac{-3}{2}"),
        ("literal -3 over 2", Number(-3) / Number(2), "-3/2", @"\frac{-3}{2}"),
        ("3 over negation of 2", Number(3) / -Number(2), "3/(-2)", @"\frac{3}{-2}"),
        ("negation of 3 over negation of 2", -Number(3) / -Number(2), "-3/(-2)", @"\frac{-3}{-2}"),
        ("negation of the number 3/2", -Number(new Rational(3, 2)), "-3/2", @"-\frac32"),
        ("negation of a negative number", -Number(-3), "3", "3"),
        ("negative integer over 2, plus 1", -Number(3) / Number(2) + Number(1), "(-3/2) + 1", @"\frac{-3}{2} + 1"),

        // what does not change
        ("3 over 2", Number(3) / Number(2), "3/2", @"\frac{3}{2}"),
        ("1/2 over 3", Number(1) / Number(2) / Number(3), "(1/2)/3", @"\frac{\frac{1}{2}}{3}"),
        ("negation of a division", -(Number(3) / Number(2)), "-(3/2)", @"-\left( \frac{3}{2} \right)"),
        ("negation of a named number, by name", -Number(3, "x"), "-(x)", @"-\left( x \right)"),
    ];

    public static IEnumerable<object[]> ExampleTestCases()
        => Examples.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ExampleTestCases))]
    public void DivisionBetweenNumbersIsWrittenSimply(string id, RationalExpression expression, string unicode, string latex)
    {
        var byName = id.EndsWith("by name");
        Assert.Equal(unicode, expression.ToUnicodeString(showRationalsAsName: byName));
        if (!byName)
            Assert.Equal(unicode, expression.ToString());
        Assert.Equal(latex, expression.ToLatexString(showRationalsAsName: byName));
    }
}
