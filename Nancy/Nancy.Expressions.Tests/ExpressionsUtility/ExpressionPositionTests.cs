using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Covers the path step value and the position built from it: construction, rendering, parsing and equality.
/// It also covers the rejection of a step that does not fit the node it lands on.
/// </summary>
public class ExpressionPositionTests
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(1, 1);
    private static readonly Curve B = new SigmaRhoArrivalCurve(2, 2);
    private static readonly Curve C = new SigmaRhoArrivalCurve(3, 3);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression Ce => Expressions.FromCurve(C, "c");

    public static IEnumerable<object[]> Positions()
    {
        yield return [new ExpressionPosition()];
        yield return [new ExpressionPosition().InnerOperand()];
        yield return [new ExpressionPosition().LeftOperand()];
        yield return [new ExpressionPosition().RightOperand()];
        yield return [new ExpressionPosition().IndexedOperand(0)];
        yield return [new ExpressionPosition().IndexedOperand(42)];
        yield return [new ExpressionPosition().LeftOperand().IndexedOperand(1).InnerOperand()];
    }

    [Theory]
    [MemberData(nameof(Positions))]
    public void APositionSurvivesARoundTrip(ExpressionPosition position)
    {
        var rendered = position.ToString();
        var parsed = ExpressionPosition.Parse(rendered);

        Assert.Equal(position, parsed);
        Assert.Equal(position.GetPositionPath(), parsed.GetPositionPath());
        Assert.Equal(position.Steps, parsed.Steps);
    }

    [Fact]
    public void TwoPositionsNamingTheSameSiteAreEqual()
    {
        var built = new ExpressionPosition().LeftOperand().IndexedOperand(1);
        var fromSteps = new ExpressionPosition([PathStep.LeftOperand, PathStep.IndexedOperand(1)]);

        Assert.Equal(built, fromSteps);
        Assert.Equal(built.GetHashCode(), fromSteps.GetHashCode());
        Assert.NotEqual(built, new ExpressionPosition().LeftOperand().IndexedOperand(2));
    }

    [Fact]
    public void ANegativeOperandIndexCannotBeBuilt()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PathStep.IndexedOperand(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpressionPosition().IndexedOperand(-1));
        Assert.Throws<ArgumentException>(() => new ExpressionPosition(["-1"]));
    }

    [Theory]
    [InlineData("Nope")]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("")]
    public void AnInvalidStepSpellingIsRejected(string spelling)
    {
        Assert.Throws<ArgumentException>(() => new ExpressionPosition([spelling]));
        Assert.False(ExpressionPosition.ValidateExpressionPosition([spelling]));
    }

    [Theory]
    [InlineData("Operand")]
    [InlineData("LeftOperand")]
    [InlineData("RightOperand")]
    [InlineData("0")]
    [InlineData("7")]
    [InlineData("-1")]
    [InlineData("Operand(1)")]
    [InlineData("")]
    [InlineData(" 1")]
    public void TryParseAndValidationAgree(string spelling)
    {
        var parsed = PathStep.TryParse(spelling, out var result);
        var validated = ExpressionPosition.ValidateExpressionPosition([spelling]);

        Assert.Equal(validated, parsed);

        if (parsed)
        {
            Assert.Equal(result, PathStep.Parse(spelling));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => PathStep.Parse(spelling));
        }
    }

    public static IEnumerable<object[]> RejectedSteps()
    {
        yield return Case("Operand", "binary",
            () => Expressions.Subtraction(Ae, Be)
                .ReplaceByPosition(new ExpressionPosition().InnerOperand(), Ce));

        yield return Case("LeftOperand", "unary",
            () => Expressions.SubAdditiveClosure(Ae)
                .ReplaceByPosition(new ExpressionPosition().LeftOperand(), Ce));

        yield return Case("5", "n-ary",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"])
                .ReplaceByPosition(new ExpressionPosition().IndexedOperand(5), Ce));

        static object[] Case(string step, string shape, Func<IExpression> replace) => [step, shape, replace];
    }

    [Theory]
    [MemberData(nameof(RejectedSteps))]
    public void AStepThatDoesNotFitItsNodeNamesTheStepAndTheNodeShape(
        string step,
        string shape,
        Func<IExpression> replace)
    {
        var exception = Assert.Throws<ArgumentException>(() => replace());

        Assert.Contains(step, exception.Message);
        Assert.Contains(shape, exception.Message);
    }
}
