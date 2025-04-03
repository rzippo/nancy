using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class RationalDivision
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RationalDivision(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(Rational a, Rational b)> RationalPairs = [
        (
            new Rational(4),
            new Rational(4)
        ),
        (
            new Rational(2),
            new Rational(3)
        ),
        (
            new Rational(-2),
            new Rational(5)
        ),
        (
            new Rational(2),
            new Rational(-5)
        )
    ];

    public static IEnumerable<object[]> DivisionTestCases
        => RationalPairs.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DivisionTestCases))]
    public void Division(Rational a, Rational b)
    {
        var aExp = a.ToExpression();
        var bExp = b.ToExpression();
        var divisionExp = Expressions.Division(aExp, bExp);
        _testOutputHelper.WriteLine(divisionExp.ToUnicodeString());
        var divisionValue = a / b;
        var divisionExpResult = divisionExp.Compute();
        _testOutputHelper.WriteLine(divisionValue.ToCodeString());
        _testOutputHelper.WriteLine(divisionExpResult.ToCodeString());
        Assert.Equivalent(divisionValue, divisionExpResult);
    }
}