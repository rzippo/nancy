using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.Expressions.Utility;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Each tree carries <c>ReplaceByPosition</c> overloads taking a bare <see cref="Curve"/> or <see cref="Rational"/> rather than an expression.
/// Each is reachable: the value is planted where a node of that value type sits.
/// </summary>
public class ReplaceByPositionValueOverloads
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(1, 1);
    private static readonly Curve B = new SigmaRhoArrivalCurve(2, 2);
    private static readonly Curve C = new SigmaRhoArrivalCurve(3, 3);
    private static readonly Sequence SequenceA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Interval CutWindow = new(0, 5, true, true);
    private static readonly Rational RThree = new(3);
    private static readonly Rational RFour = new(4);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression Ce => Expressions.FromCurve(C, "c");
    private static RationalExpression Two => Expressions.FromRational(new Rational(2), "two");
    private static RationalExpression Three => Expressions.FromRational(RThree, "three");
    private static RationalExpression Four => Expressions.FromRational(RFour, "four");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");

    public static IEnumerable<object[]> Replacements()
    {
        // Where a Curve sits in each tree.

        yield return Case("a curve under a curve unary",
            () => Expressions.SubAdditiveClosure(Ae).ReplaceByPosition(
                new ExpressionPosition().InnerOperand(), B, "b"),
            () => Expressions.SubAdditiveClosure(Be));

        yield return Case("a curve under a rational binary",
            () => Expressions.HorizontalDeviation(Ae, Be).ReplaceByPosition(
                new ExpressionPosition().LeftOperand(), C, "c"),
            () => Expressions.HorizontalDeviation(Ce, Be));

        yield return Case("a curve under a sequence cut",
            () => Ae.Cut(CutWindow).ReplaceByPosition(
                new ExpressionPosition().InnerOperand(), B, "b"),
            () => Be.Cut(CutWindow));

        // Where a Rational sits in each tree.

        yield return Case("a rational at the scale factor of a curve scale",
            () => Expressions.Scale(Ae, Two).ReplaceByPosition(
                new ExpressionPosition().RightOperand(), RThree, "three"),
            () => Expressions.Scale(Ae, Three));

        yield return Case("a rational under a rational binary",
            () => Expressions.RationalSubtraction(Three, Two).ReplaceByPosition(
                new ExpressionPosition().LeftOperand(), RFour, "four"),
            () => Expressions.RationalSubtraction(Four, Two));

        yield return Case("a rational at the scale factor of a sequence scale",
            () => Sa.Scale(Two).ReplaceByPosition(
                new ExpressionPosition().RightOperand(), RThree, "three"),
            () => Sa.Scale(Three));

        static object[] Case(string name, Func<IExpression> replace, Func<IExpression> expected)
            => [name, replace, expected];
    }

    [Theory]
    [MemberData(nameof(Replacements))]
    public void TheValueIsPlantedAtThePosition(
        string name,
        Func<IExpression> replace,
        Func<IExpression> expected)
    {
        _ = name;

        Assert.Equal(expected().ToUnicodeString(), replace().ToUnicodeString());
    }
}
