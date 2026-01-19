using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unipi.Nancy.NetworkCalculus;
using Xunit;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

public class Renaming
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Renaming(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static List<(CurveExpression expression, string newName)> CurveRenamingExamples =
    [
        (
            Expressions.Convolution(
                Expressions.FromCurve(new SigmaRhoArrivalCurve(2, 3), "a"),
                Expressions.FromCurve(new RateLatencyServiceCurve(3, 4), "b")
            ),
            "conv"
        ),
#pragma warning disable CS0618 // Type or member is obsolete
        (
            Expressions.Subtraction(
                Expressions.FromCurve(new SigmaRhoArrivalCurve(2, 3), "a"),
                Expressions.FromCurve(new RateLatencyServiceCurve(3, 4), "b"),
                false
            ),
            "diff"
        ),
        (
            Expressions.Subtraction(
                Expressions.FromCurve(new SigmaRhoArrivalCurve(2, 3), "a"),
                Expressions.FromCurve(new RateLatencyServiceCurve(3, 4), "b"),
                true
            ),
            "diff_nn"
        )
#pragma warning restore CS0618 // Type or member is obsolete
    ];

    public static IEnumerable<object[]> CurveRenamingTestCases
        => CurveRenamingExamples.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(CurveRenamingTestCases))]
    public void CurveRenaming(CurveExpression expression, string newName)
    {
        _testOutputHelper.WriteLine(expression.ToUnicodeString());
        var rn1 = expression with { Name = newName };
        _testOutputHelper.WriteLine(rn1.ToUnicodeString(0));
        _testOutputHelper.WriteLine(rn1.ToUnicodeString(2));
        var rn2 = expression.WithName(newName);
        _testOutputHelper.WriteLine(rn2.ToUnicodeString(0));
        _testOutputHelper.WriteLine(rn2.ToUnicodeString(2));
    }
    
    public static List<(RationalExpression expression, string newName)> RationalRenamingExamples =
    [
        (
            Expressions.Product(
                Expressions.FromRational(1.5m, "a"),
                Expressions.FromRational(0.5m, "b")
            ),
            "product"
        ),
        (
            Expressions.RationalSubtraction(
                Expressions.FromRational(1.5m, "a"),
                Expressions.FromRational(0.5m, "b")
            ),
            "diff"
        )
    ];

    public static IEnumerable<object[]> RationalRenamingTestCases
        => RationalRenamingExamples.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(RationalRenamingTestCases))]
    public void RationalRenaming(RationalExpression expression, string newName)
    {
        _testOutputHelper.WriteLine(expression.ToUnicodeString());
        var rn1 = expression with { Name = newName };
        _testOutputHelper.WriteLine(rn1.ToUnicodeString(0));
        _testOutputHelper.WriteLine(rn1.ToUnicodeString(2));
        var rn2 = expression.WithName(newName);
        _testOutputHelper.WriteLine(rn2.ToUnicodeString(0));
        _testOutputHelper.WriteLine(rn2.ToUnicodeString(2));
    }
}