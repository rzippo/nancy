using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Nodes;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Covers <c>ReplaceByPosition</c> over every node shape.
/// <see cref="ReplaceByPositionTests"/> covers the four binary operand-type combinations at depth one.
/// This covers unary, n-ary, the root, operands whose type differs from the replacement's, and the paths that should be rejected.
/// </summary>
public class ReplaceByPositionShapes
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(1, 1);
    private static readonly Curve B = new SigmaRhoArrivalCurve(2, 2);
    private static readonly Curve C = new SigmaRhoArrivalCurve(3, 3);
    private static readonly Sequence SequenceA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SequenceB = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);
    private static readonly Sequence SequenceC = new([Point.Origin(), new Segment(0, 6, 0, 3), new Point(6, 18)]);
    private static readonly Interval CutWindow = new(0, 5, true, true);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression Ce => Expressions.FromCurve(C, "c");
    private static RationalExpression Two => Expressions.FromRational(new Rational(2), "two");
    private static RationalExpression Three => Expressions.FromRational(new Rational(3), "three");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");
    private static SequenceExpression Sb => SequenceB.ToExpression("b");
    private static SequenceExpression Sc => SequenceC.ToExpression("c");

    public static IEnumerable<object[]> Replacements()
    {
        yield return Case("curve unary operand",
            () => Expressions.SubAdditiveClosure(Ae).ReplaceByPosition(
                new ExpressionPosition().InnerOperand(), Be),
            () => Expressions.SubAdditiveClosure(Be));

        yield return Case("rational unary over a rational",
            () => Expressions.Negate(Expressions.RationalSubtraction(Two, Three)).ReplaceByPosition(
                new ExpressionPosition().InnerOperand(), Three),
            () => Expressions.Negate(Three));

        yield return Case("rational unary over a curve",
            () => Expressions.MaxValue(Ae).ReplaceByPosition(
                new ExpressionPosition().InnerOperand(), Be),
            () => Expressions.MaxValue(Be));

        yield return Case("curve n-ary indexed operand",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"]).ReplaceByPosition(
                new ExpressionPosition().IndexedOperand(1), Ce),
            () => Expressions.Convolution([A, C, C], ["a", "c", "c"]));

        yield return Case("rational n-ary indexed operand",
            () => Expressions.RationalMinimum([new Rational(1), new Rational(2), new Rational(3)],
                    ["one", "two", "three"])
                .ReplaceByPosition(new ExpressionPosition().IndexedOperand(1), Three),
            () => Expressions.RationalMinimum([new Rational(1), new Rational(3), new Rational(3)],
                ["one", "three", "three"]));

        yield return Case("the root",
            () => Expressions.Convolution(A, B).ReplaceByPosition(new ExpressionPosition(), Ce),
            () => Ce);

        yield return Case("curve operand under a unary under an n-ary",
            () => Expressions.Convolution([Expressions.SubAdditiveClosure(Ae), Be]).ReplaceByPosition(
                new ExpressionPosition().IndexedOperand(0).InnerOperand(), Ce),
            () => Expressions.Convolution([Expressions.SubAdditiveClosure(Ce), Be]));

        // The replacement is a Rational while the operand it is nested under is a Curve.
        yield return Case("rational under the left operand of a curve binary",
            () => Expressions.Subtraction(Expressions.Scale(Ae, Two), Be).ReplaceByPosition(
                new ExpressionPosition().LeftOperand().RightOperand(), Three),
            () => Expressions.Subtraction(Expressions.Scale(Ae, Three), Be));

        yield return Case("rational under the right operand of a curve binary",
            () => Expressions.Subtraction(Ae, Expressions.Scale(Be, Two)).ReplaceByPosition(
                new ExpressionPosition().RightOperand().RightOperand(), Three),
            () => Expressions.Subtraction(Ae, Expressions.Scale(Be, Three)));

        // The replacement is a Curve while the node being rebuilt is a Rational.
        yield return Case("curve under the left operand of a rational binary",
            () => Expressions.HorizontalDeviation(Expressions.Convolution([A, B], ["a", "b"]), Ce)
                .ReplaceByPosition(new ExpressionPosition().LeftOperand().IndexedOperand(1), Ce),
            () => Expressions.HorizontalDeviation(Expressions.Convolution([A, C], ["a", "c"]), Ce));

        // ReplaceByPosition currently throws "Invalid position" for a sequence expression.

        yield return Case("sequence unary operand",
            () => Sa.Negate().ReplaceByPosition(new ExpressionPosition().InnerOperand(), Sb),
            () => Sb.Negate());

        yield return Case("sequence binary left operand",
            () => Sa.Subtraction(Sb).ReplaceByPosition(new ExpressionPosition().LeftOperand(), Sc),
            () => Sc.Subtraction(Sb));

        yield return Case("sequence binary right operand",
            () => Sa.Subtraction(Sb).ReplaceByPosition(new ExpressionPosition().RightOperand(), Sc),
            () => Sa.Subtraction(Sc));

        yield return Case("sequence n-ary indexed operand",
            () => Sa.Addition(Sb).Addition(Sc).ReplaceByPosition(new ExpressionPosition().IndexedOperand(1), Sa),
            () => Sa.Addition(Sa).Addition(Sc));

        yield return Case("the sequence root",
            () => Sa.Addition(Sb).ReplaceByPosition(new ExpressionPosition(), Sc),
            () => Sc);

        // A sequence expression reached from inside a rational one, which SequenceHorizontalDeviationExpression allows.

        yield return Case("sequence operand of a rational binary",
            () => Expressions.HorizontalDeviation(Sa.Addition(Sb), Sc)
                .ReplaceByPosition(new ExpressionPosition().LeftOperand().IndexedOperand(1), Sa),
            () => Expressions.HorizontalDeviation(Sa.Addition(Sa), Sc));

        // A curve expression reached from inside a sequence one, which CurveCutExpression allows.

        yield return Case("curve operand of a sequence cut",
            () => Ae.Cut(CutWindow).ReplaceByPosition(new ExpressionPosition().InnerOperand(), Be),
            () => Be.Cut(CutWindow));

        static object[] Case(string name, Func<IExpression> replace, Func<IExpression> expected)
            => [name, replace, expected];
    }

    [Theory]
    [MemberData(nameof(Replacements))]
    public void TheSubExpressionAtThePositionIsReplaced(
        string name,
        Func<IExpression> replace,
        Func<IExpression> expected)
    {
        _ = name;

        Assert.Equal(expected().ToUnicodeString(), replace().ToUnicodeString());
    }

    public static IEnumerable<object[]> RejectedPositions()
    {
        yield return Case("an index past the last operand",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"]).ReplaceByPosition(
                new ExpressionPosition().IndexedOperand(5), Ce));

        yield return Case("a step the node it lands on cannot take",
            () => Expressions.Convolution([Expressions.SubAdditiveClosure(Ae), Be]).ReplaceByPosition(
                new ExpressionPosition().IndexedOperand(0).InnerOperand().InnerOperand(), Ce));

        yield return Case("a binary step on a unary node",
            () => Expressions.SubAdditiveClosure(Ae).ReplaceByPosition(
                new ExpressionPosition().LeftOperand(), Ce));

        yield return Case("a replacement of another value type at an operand",
            () => Expressions.Deconvolution(Ae, Be).ReplaceByPosition(
                new ExpressionPosition().LeftOperand(), Two));

        yield return Case("a replacement of another value type at the root",
            () => Ae.ReplaceByPosition(new ExpressionPosition(), Two));

        static object[] Case(string name, Func<IExpression> replace) => [name, replace];
    }

    [Theory]
    [MemberData(nameof(RejectedPositions))]
    public void AnInvalidPositionIsRejected(string name, Func<IExpression> replace)
    {
        _ = name;

        Assert.Throws<ArgumentException>(() => replace());
    }

    /// <summary>
    /// The n-ary constructors flatten a nested operand of the same operation.
    /// The shape of the result is therefore not the shape that was handed in.
    /// The value it computes is what the replacement has to preserve.
    /// </summary>
    [Fact]
    public void ReplacingAnNAryOperandWithTheSameOperationKeepsTheValue()
    {
        var e = Expressions.Convolution([A, B], ["a", "b"]);

        var replaced = e.ReplaceByPosition(
            new ExpressionPosition().IndexedOperand(1),
            Expressions.Convolution([C, A], ["c", "a"]));

        var expected = Expressions.Convolution([A, C, A], ["a", "c", "a"]);
        Assert.True(expected.Compute().Equivalent(replaced.Compute()));
    }
}
