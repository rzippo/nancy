using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class RationalToString
{
    private readonly ITestOutputHelper output;

    public RationalToString(ITestOutputHelper output)
    {
        this.output = output;
    }

    public static List<Rational> TestRationals = new List<Rational>()
    {
        Rational.Zero,
        Rational.One,
        Rational.MinusOne,
        Rational.PlusInfinity,
        Rational.MinusInfinity,
        new Rational(10, 5),
        new Rational(5, 10),
        new Rational(13, 11),
    };

    public static IEnumerable<object[]> ToStringTestCases()
    {
        foreach (var rational in TestRationals)
        {
            yield return new object[] { rational };
        }
    }

    [Theory]
    [MemberData(nameof(ToStringTestCases))]
    public void ToStringTest(Rational rational)
    {
        var str = rational.ToString();
        output.WriteLine(str);
    }

    [Theory]
    [MemberData(nameof(ToStringTestCases))]
    public void ToCodeStringTest(Rational rational)
    {
        var str = rational.ToCodeString();
        output.WriteLine(str);
    }

    public static IEnumerable<object[]> ListToStringTestCases()
    {
        yield return new object[] { TestRationals };
    }


    [Theory]
    [MemberData(nameof(ListToStringTestCases))]
    public void ListToCodeStringTest(IReadOnlyList<Rational> list)
    {
        var str = list.ToCodeString();
        output.WriteLine(str);
    }
}