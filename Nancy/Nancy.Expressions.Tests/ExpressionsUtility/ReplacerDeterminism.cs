using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;

/// <summary>
/// Replacement is a pure function of the expression, what to find and what to put there.
/// The same inputs give the same answer however many times the operation is called, and the original is left untouched.
/// </summary>
public class ReplacerDeterminism
{
    private static readonly Curve A = new SigmaRhoArrivalCurve(1, 1);
    private static readonly Curve B = new SigmaRhoArrivalCurve(2, 2);
    private static readonly Curve C = new SigmaRhoArrivalCurve(3, 3);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static CurveExpression Be => Expressions.FromCurve(B, "b");
    private static CurveExpression Ce => Expressions.FromCurve(C, "c");

    [Fact]
    public void ReplacingByValueTwiceGivesTheSameAnswer()
    {
        var expression = Expressions.Subtraction(Ae, Be);
        var before = expression.ToUnicodeString();

        var first = expression.ReplaceByValue(Ae, Ce).ToUnicodeString();
        var second = expression.ReplaceByValue(Ae, Ce).ToUnicodeString();

        Assert.NotEqual(before, first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ApplyingAnEquivalenceTwiceGivesTheSameAnswer()
    {
        var expression = Expressions.SubAdditiveClosure(Expressions.Minimum(A, B));
        var equivalence = new SubAdditiveClosureOfMin();
        var before = expression.ToUnicodeString();

        var first = expression.ApplyEquivalence(equivalence).ToUnicodeString();
        var second = expression.ApplyEquivalence(equivalence).ToUnicodeString();

        Assert.NotEqual(before, first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void ARewriteLeavesTheOriginalExpressionUnchanged()
    {
        var expression = Expressions.Subtraction(Ae, Be);
        var before = expression.ToUnicodeString();

        _ = expression.ReplaceByValue(Ae, Ce);
        _ = expression.ApplyEquivalence(new SubAdditiveClosureOfMin());

        Assert.Equal(before, expression.ToUnicodeString());
    }
}
