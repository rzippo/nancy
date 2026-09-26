using System;
using System.Collections.Generic;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Utility;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Each rewriting operation comes in two forms: the value-returning one hands back the expression alone, and the result-returning one also says whether anything matched, how many sites, where, and what an equivalence bound.
/// The result is what lets a caller tell a rewrite that changed something from one that did not.
/// </summary>
public class RewriteResults
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
    private static RationalExpression Five => Expressions.FromRational(new Rational(5), "five");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");
    private static SequenceExpression Sb => SequenceB.ToExpression("b");
    private static SequenceExpression Sc => SequenceC.ToExpression("c");

    public static IEnumerable<object[]> NoMatchCases()
    {
        yield return Case("curve tree, replace by value",
            () => Expressions.Subtraction(Ae, Be),
            () => Expressions.Subtraction(Ae, Be).ReplaceByValueWithResult(Ce, Ce));

        yield return Case("rational tree, replace by value",
            () => Expressions.RationalSubtraction(Two, Three),
            () => Expressions.RationalSubtraction(Two, Three).ReplaceByValueWithResult(Five, Five));

        yield return Case("sequence tree, replace by value",
            () => Sa.Addition(Sb),
            () => Sa.Addition(Sb).ReplaceByValueWithResult(Sc, Sc));

        yield return Case("curve tree, apply an equivalence",
            () => Expressions.Convolution(A, B),
            () => Expressions.Convolution(A, B).ApplyEquivalenceWithResult(new SubAdditiveClosureOfMin()));

        yield return Case("rational tree, apply an equivalence",
            () => Expressions.HorizontalDeviation(A, B),
            () => Expressions.HorizontalDeviation(A, B).ApplyEquivalenceWithResult(new SubAdditiveClosureOfMin()));

        yield return Case("sequence tree, apply an equivalence",
            () => Sa.Addition(Sb),
            () => Sa.Addition(Sb).ApplyEquivalenceWithResult(new SubAdditiveClosureOfMin()));

        yield return Case("curve tree, apply an equivalence by position",
            () => Expressions.Convolution(A, B),
            () => Expressions.Convolution(A, B).ApplyEquivalenceByPositionWithResult(
                new ExpressionPosition(), new SubAdditiveClosureOfMin()));

        yield return Case("sequence tree, apply an equivalence by position",
            () => Sa.Addition(Sb),
            () => Sa.Addition(Sb).ApplyEquivalenceByPositionWithResult(
                new ExpressionPosition(), new SubAdditiveClosureOfMin()));

        static object[] Case(string name, Func<IExpression> original, Func<ExpressionRewriteResult> apply)
            => [name, original, apply];
    }

    [Theory]
    [MemberData(nameof(NoMatchCases))]
    public void ARewriteThatMatchesNothingReportsNoMatch(
        string name,
        Func<IExpression> original,
        Func<ExpressionRewriteResult> apply)
    {
        _ = name;

        var result = apply();

        Assert.False(result.Matched);
        Assert.Equal(0, result.ReplacementCount);
        Assert.Empty(result.Positions);
        Assert.Equal(original().ToUnicodeString(), result.Expression.ToUnicodeString());
    }

    [Fact]
    public void AReplacementAtAPositionReportsTheSite()
    {
        var expression = Expressions.Subtraction(Ae, Be);

        var result = expression.ReplaceByPositionWithResult(new ExpressionPosition().RightOperand(), Ce);

        Assert.True(result.Matched);
        Assert.Equal(1, result.ReplacementCount);
        Assert.Equal(new ExpressionPosition().RightOperand(), Assert.Single(result.Positions));
        Assert.Equal(Expressions.Subtraction(Ae, Ce).ToUnicodeString(), result.Expression.ToUnicodeString());
    }

    [Fact]
    public void ACurveEquivalenceUnderASequenceHostReportsTheSiteAndTheBindings()
    {
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(2, 4);
        var cut = Expressions.SubAdditiveClosure(Expressions.Minimum(f, g)).Cut(CutWindow);

        var result = cut.ApplyEquivalenceWithResult(new SubAdditiveClosureOfMin());

        Assert.True(result.Matched);
        Assert.Equal(1, result.ReplacementCount);
        Assert.Equal(new ExpressionPosition().InnerOperand(), Assert.Single(result.Positions));
        Assert.NotNull(result.Bindings);
        // one map whatever the placeholders stand for, so the caller casts where it knows what it asked for
        Assert.IsAssignableFrom<CurveExpression>(result.Bindings!["f"]);
        Assert.IsAssignableFrom<CurveExpression>(result.Bindings!["g"]);
    }
}
