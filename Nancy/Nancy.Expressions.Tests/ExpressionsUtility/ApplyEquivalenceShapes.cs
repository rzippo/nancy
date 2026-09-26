using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Nodes;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Covers the two equivalence operations over the shapes that can host the curve node they rewrite.
/// The tests under <c>Equivalences/</c> apply each equivalence at the root of a curve expression.
/// This covers the equivalence reached through a traversal, and reached inside a rational expression.
/// An equivalence applies wherever it matches, so the host's value type takes no part.
/// </summary>
public class ApplyEquivalenceShapes
{
    private static readonly Curve A = new RateLatencyServiceCurve(1, 2);
    private static readonly Curve B = new RateLatencyServiceCurve(2, 4);
    private static readonly Sequence SequenceA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Interval CutWindow = new(0, 5, true, true);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static SequenceExpression SequenceAe => SequenceA.ToExpression("a");

    /// <summary>The left side of <see cref="SubAdditiveClosureOfMin"/>, as an expression to plant.</summary>
    private static CurveExpression ClosureOfMin => Expressions.SubAdditiveClosure(Expressions.Minimum(A, B));

    /// <summary>The right side of <see cref="SubAdditiveClosureOfMin"/>, as an expression to plant.</summary>
    private static CurveExpression ConvolutionOfClosures => Expressions.Convolution(
        Expressions.SubAdditiveClosure(Ae),
        Expressions.SubAdditiveClosure(Be));

    public static IEnumerable<object[]> Applications()
    {
        yield return Case("by value, at the root",
            () => ClosureOfMin.ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => ClosureOfMin);

        yield return Case("by value, under an operand of a curve n-ary",
            () => Expressions.Convolution([ClosureOfMin, Ae]).ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => Expressions.Convolution([ClosureOfMin, Ae]));

        yield return Case("by value, under the left operand of a curve binary over a rational",
            () => Expressions.DelayBy(ClosureOfMin, Expressions.FromRational(new Rational(2), "two"))
                .ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => Expressions.DelayBy(ClosureOfMin, Expressions.FromRational(new Rational(2), "two")));

        // A curve equivalence rewriting one operand of a rational-valued node.
        yield return Case("by value, under an operand of a rational binary",
            () => Expressions.HorizontalDeviation(ClosureOfMin, Be)
                .ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => Expressions.HorizontalDeviation(ClosureOfMin, Be));

        yield return Case("by value, under the operand of a rational unary",
            () => Expressions.MaxValue(ClosureOfMin).ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => Expressions.MaxValue(ClosureOfMin));

        yield return Case("by position, at the root",
            () => ClosureOfMin.ApplyEquivalenceByPosition(
                new ExpressionPosition(), new SubAdditiveClosureOfMin()),
            () => ClosureOfMin);

        yield return Case("by position, at an operand of a curve n-ary",
            () => Expressions.Convolution([ClosureOfMin, Ae]).ApplyEquivalenceByPosition(
                new ExpressionPosition().IndexedOperand(0), new SubAdditiveClosureOfMin()),
            () => Expressions.Convolution([ClosureOfMin, Ae]));

        // CheckRightOnly matches the right side and substitutes the left, running the equivalence backwards.
        yield return Case("by value, right to left",
            () => ConvolutionOfClosures.ApplyEquivalence(
                new SubAdditiveClosureOfMin(), CheckType.CheckRightOnly),
            () => ConvolutionOfClosures);

        // An equivalence applies wherever it matches, so a curve site under a sequence host is reached the same way.

        yield return Case("by value, at a curve cut under a sequence root",
            () => ClosureOfMin.Cut(CutWindow).ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => ClosureOfMin.Cut(CutWindow));

        yield return Case("by value, at a curve node under a sequence node's rational operand",
            () => SequenceAe.Scale(Expressions.HorizontalDeviation(ClosureOfMin, Be))
                .ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => SequenceAe.Scale(Expressions.HorizontalDeviation(ClosureOfMin, Be)));

        yield return Case("by value, at a curve cut under a rational-over-sequence root",
            () => Expressions.HorizontalDeviation(ClosureOfMin.Cut(CutWindow), ClosureOfMin.Cut(CutWindow))
                .ApplyEquivalence(new SubAdditiveClosureOfMin()),
            () => Expressions.HorizontalDeviation(ClosureOfMin.Cut(CutWindow), ClosureOfMin.Cut(CutWindow)));

        static object[] Case(string name, Func<IExpression> apply, Func<IExpression> original)
            => [name, apply, original];
    }

    [Theory]
    [MemberData(nameof(Applications))]
    public void TheEquivalenceRewritesTheExpressionAndKeepsItsValue(
        string name,
        Func<IExpression> apply,
        Func<IExpression> original)
    {
        _ = name;

        var before = original();
        var after = apply();

        Assert.NotEqual(before.ToUnicodeString(), after.ToUnicodeString());
        AssertSameValue(before, after);
    }

    private static void AssertSameValue(IExpression expected, IExpression actual)
    {
        switch (expected, actual)
        {
            case (CurveExpression e, CurveExpression a):
                Assert.True(e.Compute().Equivalent(a.Compute()));
                break;
            case (RationalExpression e, RationalExpression a):
                Assert.Equal(e.Compute(), a.Compute());
                break;
            case (SequenceExpression e, SequenceExpression a):
                Assert.True(e.Compute().Equivalent(a.Compute()));
                break;
            default:
                Assert.Fail($"{expected.GetType().Name} and {actual.GetType().Name} are not the same kind");
                break;
        }
    }

    [Fact]
    public void AnEquivalenceThatMatchesNothingLeavesTheExpressionUnchanged()
    {
        var e = Expressions.Convolution(A, B);

        var applied = e.ApplyEquivalence(new SubAdditiveClosureOfMin());

        Assert.Equal(e.ToUnicodeString(), applied.ToUnicodeString());
    }

    [Fact]
    public void AnEquivalenceThatMatchesNothingAtThePositionLeavesTheExpressionUnchanged()
    {
        var e = Expressions.Convolution(A, B);

        var applied = e.ApplyEquivalenceByPosition(new ExpressionPosition(), new SubAdditiveClosureOfMin());

        Assert.Equal(e.ToUnicodeString(), applied.ToUnicodeString());
    }

}
