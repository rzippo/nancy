using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Nodes;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Covers <c>ReplaceByValue</c> over every node shape.
/// </summary>
public class ReplaceByValueShapes
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
        yield return Case("the left operand of a curve binary",
            () => Expressions.Subtraction(Ae, Be).ReplaceByValue(Ae, Ce),
            () => Expressions.Subtraction(Ce, Be));

        yield return Case("the right operand of a curve binary",
            () => Expressions.Subtraction(Ae, Be).ReplaceByValue(Be, Ce),
            () => Expressions.Subtraction(Ae, Ce));

        yield return Case("both operands of a curve binary, when both match",
            () => Expressions.Subtraction(Ae, Ae).ReplaceByValue(Ae, Ce),
            () => Expressions.Subtraction(Ce, Ce));

        yield return Case("the operand of a curve unary",
            () => Expressions.SubAdditiveClosure(Ae).ReplaceByValue(Ae, Be),
            () => Expressions.SubAdditiveClosure(Be));

        yield return Case("the root",
            () => Ae.ReplaceByValue(Ae, Be),
            () => Be);

        yield return Case("an operand of a curve n-ary",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"]).ReplaceByValue(Be, Ce),
            () => Expressions.Convolution([A, C, C], ["a", "c", "c"]));

        yield return Case("a pattern that matches nothing",
            () => Expressions.Subtraction(Ae, Be).ReplaceByValue(Ce, Ce),
            () => Expressions.Subtraction(Ae, Be));

        yield return Case("the rational operand of a curve binary",
            () => Expressions.Scale(Ae, Two).ReplaceByValue(Two, Three, false),
            () => Expressions.Scale(Ae, Three));

        yield return Case("an operand of a rational binary",
            () => Expressions.RationalSubtraction(Two, Three).ReplaceByValue(Three, Two, false),
            () => Expressions.RationalSubtraction(Two, Two));

        yield return Case("the operand of a rational unary",
            () => Expressions.Negate(Three).ReplaceByValue(Three, Two, false),
            () => Expressions.Negate(Two));

        yield return Case("an operand of a rational n-ary",
            () => Expressions.RationalMinimum([new Rational(1), new Rational(2), new Rational(3)],
                    ["one", "two", "three"])
                .ReplaceByValue(Two, Three, false),
            () => Expressions.RationalMinimum([new Rational(1), new Rational(3), new Rational(3)],
                ["one", "three", "three"]));

        // A curve operand of a rational-valued node: the shape of hDev, vDev and zDev.
        yield return Case("a curve operand of a rational binary",
            () => Expressions.HorizontalDeviation(Ae, Be).ReplaceByValue(Be, Ce, false),
            () => Expressions.HorizontalDeviation(Ae, Ce));

        yield return Case("a curve operand of a rational unary",
            () => Expressions.MaxValue(Ae).ReplaceByValue(Ae, Be, false),
            () => Expressions.MaxValue(Be));

        yield return Case("a partial n-ary match, keeping the unmatched operands",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"])
                .ReplaceByValue(Expressions.Convolution([B, C], ["b", "c"]), Ce),
            () => Expressions.Convolution([A, C], ["a", "c"]));

        yield return Case("a partial n-ary match, dropping the unmatched operands",
            () => Expressions.Convolution([A, B, C], ["a", "b", "c"])
                .ReplaceByValue(Expressions.Convolution([B, C], ["b", "c"]), Ce, true),
            () => Ce);

        // The unmatched operands to carry over are the ones the right operand reported, not the left.
        yield return Case("a partial n-ary match under the right operand of a binary",
            () => Expressions.Subtraction(Ae, Expressions.Convolution([A, B, C], ["a", "b", "c"]))
                .ReplaceByValue(Expressions.Convolution([B, C], ["b", "c"]), Ce),
            () => Expressions.Subtraction(Ae, Expressions.Convolution([A, C], ["a", "c"])));

        yield return Case("a partial rational n-ary match",
            () => Expressions.RationalMinimum([new Rational(1), new Rational(2), new Rational(3)],
                    ["one", "two", "three"])
                .ReplaceByValue(
                    Expressions.RationalMinimum([new Rational(2), new Rational(3)], ["two", "three"]),
                    Three,
                    false),
            () => Expressions.RationalMinimum([new Rational(1), new Rational(3)], ["one", "three"]));

        // ReplaceByValue currently returns a sequence expression unchanged, without an exception.
        // A case is therefore only caught by asserting on the result.

        yield return Case("the operand of a sequence unary",
            () => Sa.Negate().ReplaceByValue(Sa, Sb),
            () => Sb.Negate());

        yield return Case("the left operand of a sequence binary",
            () => Sa.Subtraction(Sb).ReplaceByValue(Sa, Sc),
            () => Sc.Subtraction(Sb));

        yield return Case("the right operand of a sequence binary",
            () => Sa.Subtraction(Sb).ReplaceByValue(Sb, Sc),
            () => Sa.Subtraction(Sc));

        yield return Case("an operand of a sequence n-ary",
            () => Sa.Addition(Sb).Addition(Sc).ReplaceByValue(Sb, Sc),
            () => Sa.Addition(Sc).Addition(Sc));

        yield return Case("the root of the sequence tree",
            () => Sa.ReplaceByValue(Sa, Sb),
            () => Sb);

        // A sequence expression reached from inside a rational one, which SequenceHorizontalDeviationExpression allows.

        yield return Case("a sequence operand of a rational binary",
            () => Expressions.HorizontalDeviation(Sa.Addition(Sb), Sc)
                .ReplaceByValue(Sb, Sa, false),
            () => Expressions.HorizontalDeviation(Sa.Addition(Sa), Sc));

        // A curve expression reached from inside a sequence one, which CurveCutExpression allows.

        yield return Case("a curve operand of a sequence cut",
            () => Ae.Cut(CutWindow).ReplaceByValue(Ae, Be),
            () => Be.Cut(CutWindow));

        static object[] Case(string name, Func<IExpression> replace, Func<IExpression> expected)
            => [name, replace, expected];
    }

    [Theory]
    [MemberData(nameof(Replacements))]
    public void TheMatchedSubExpressionIsReplaced(
        string name,
        Func<IExpression> replace,
        Func<IExpression> expected)
    {
        _ = name;

        Assert.Equal(expected().ToUnicodeString(), replace().ToUnicodeString());
    }

    /// <summary>
    /// A replacement that carries a name is an operand of its own, so the operands a partial match leaves over are not merged into it.
    /// </summary>
    [Fact]
    public void ANamedReplacementOfAPartialMatchStaysOneOperand()
    {
        var d = Expressions.FromCurve(new SigmaRhoArrivalCurve(4, 4), "d");
        var e = Expressions.FromCurve(new SigmaRhoArrivalCurve(5, 5), "e");
        var p = Ce.Convolution(d, "p");

        var result = (CurveNAryExpression)Expressions.Convolution([Ae, Be, e]).ReplaceByValue(Ae.Convolution(Be), p);

        Assert.Equal(2, result.Operands.Count);
        Assert.Contains(result.Operands, operand => ReferenceEquals(operand, p));
        Assert.Equal("", result.Name);
    }
}
