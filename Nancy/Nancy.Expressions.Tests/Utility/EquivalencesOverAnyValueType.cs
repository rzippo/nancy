using System;
using Unipi.Nancy.Expressions.Equivalences;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Utility;

/// <summary>
/// An equivalence is stated over expressions of one value type, and nothing in the machinery is written for a particular one.
/// The sides, the result and the hypotheses all speak of expressions.
/// The equivalences here are deliberately false, since what is under test is what the API admits rather than any mathematics.
/// </summary>
public class EquivalencesOverAnyValueType
{
    private static readonly Curve A = new RateLatencyServiceCurve(1, 2);
    private static readonly Sequence SequenceA =
        new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);

    private static CurveExpression Ae => Expressions.FromCurve(A, "a");
    private static SequenceExpression Sa => SequenceA.ToExpression("a");

    // Not a theorem: negating a sequence is not the identity.
    // Its purpose is to be stated over sequences at all.
    private static Equivalence NegateASequence()
        => new(Expressions.SequencePlaceholder("f"),
            Expressions.SequencePlaceholder("f").Negate());

    // Not a theorem: the same shape over the rational tree.
    private static Equivalence NegateARational()
        => new(Expressions.RationalPlaceholder("f"),
            Expressions.Negate(Expressions.RationalPlaceholder("f")));

    [Fact]
    public void AnEquivalenceOverSequenceExpressionsCanBeStated()
    {
        var equivalence = NegateASequence();

        Assert.IsAssignableFrom<SequenceExpression>(equivalence.LeftSideExpression);
        Assert.IsAssignableFrom<SequenceExpression>(equivalence.RightSideExpression);
    }

    [Fact]
    public void AnEquivalenceOverSequenceExpressionsApplies()
    {
        var result = NegateASequence().Apply(Sa);

        Assert.True(result.IsMatch);
        Assert.IsAssignableFrom<SequenceExpression>(result.NewExpression);
        Assert.Equal(Sa.Negate().ToUnicodeString(), result.NewExpression!.ToUnicodeString());
    }

    [Fact]
    public void AnEquivalenceOverRationalExpressionsApplies()
    {
        var result = NegateARational().Apply(Expressions.FromRational(2, "two"));

        Assert.True(result.IsMatch);
        Assert.IsAssignableFrom<RationalExpression>(result.NewExpression);
    }

    /// <summary>
    /// A hypothesis is written for one value type, and a placeholder bound to another cannot satisfy it.
    /// The equivalence matches by shape, so the hypothesis is the only thing standing in the way.
    /// </summary>
    [Fact]
    public void AHypothesisWrittenForCurvesIsNotSatisfiedByASequenceBinding()
    {
        var equivalence = NegateASequence();
        // The placeholder binds to a sequence expression, so a condition on curve expressions cannot hold of it, whatever the condition says.
        // This one is true of every curve, to make the point rest on the type alone.
        equivalence.AddHypothesis<CurveExpression>("f", _ => true);

        var result = equivalence.Apply(Sa);

        Assert.False(result.IsMatch);
        Assert.Null(result.NewExpression);
    }

    /// <summary>
    /// The counterpart: the same shape of condition, written for the value type that is actually bound, holds.
    /// </summary>
    [Fact]
    public void AHypothesisWrittenForSequencesIsSatisfiedByASequenceBinding()
    {
        var equivalence = NegateASequence();
        equivalence.AddHypothesis<SequenceExpression>("f", _ => true);

        var result = equivalence.Apply(Sa);

        Assert.True(result.IsMatch);
    }

    /// <summary>
    /// A hypothesis that names a placeholder the equivalence never binds is never satisfied, so the equivalence cannot be applied.
    /// This is what tells an unbound name apart from one bound to the wrong value type: both refuse the match, and neither is quietly ignored.
    /// </summary>
    [Fact]
    public void AHypothesisOnAnUnboundPlaceholderIsNeverSatisfied()
    {
        var equivalence = NegateASequence();
        equivalence.AddHypothesis<SequenceExpression>("nowhere", _ => true);

        Assert.False(equivalence.Apply(Sa).IsMatch);
    }

    /// <summary>
    /// A curve equivalence still works as it did, hypotheses included, the store having lost the value type rather than the condition.
    /// </summary>
    [Fact]
    public void ACurveHypothesisStillPrunesAMatch()
    {
        var refused = new Equivalence(Expressions.Placeholder("f"), Expressions.Negate(Expressions.Placeholder("f")));
        refused.AddHypothesis<CurveExpression>("f", _ => false);

        var admitted = new Equivalence(Expressions.Placeholder("f"), Expressions.Negate(Expressions.Placeholder("f")));
        admitted.AddHypothesis<CurveExpression>("f", _ => true);

        Assert.False(refused.Apply(Ae).IsMatch);
        Assert.True(admitted.Apply(Ae).IsMatch);
    }

    /// <summary>
    /// The two sides must be written over the same value type, whatever placeholders they use.
    /// </summary>
    [Fact]
    public void AnEquivalenceWhoseSidesDifferInValueTypeIsRejected()
    {
        var equivalence = () => new Equivalence(
            Expressions.Placeholder("f"),
            Expressions.Negate(Expressions.RationalPlaceholder("g")));

        Assert.Throws<ArgumentException>(equivalence);
    }

    /// <summary>
    /// A placeholder name must stand for one value type everywhere it occurs across the two sides.
    /// </summary>
    [Fact]
    public void APlaceholderNameUsedForTwoValueTypesIsRejected()
    {
        var equivalence = () => new Equivalence(
            Expressions.VerticalShift(Expressions.Placeholder("f"), Expressions.RationalPlaceholder("K")),
            Expressions.Addition(Expressions.Placeholder("f"), Expressions.Placeholder("K")));

        Assert.Throws<ArgumentException>(equivalence);
    }

    /// <summary>
    /// A side may legitimately hold placeholders of several value types, so long as each name keeps one.
    /// </summary>
    [Fact]
    public void APlaceholderOfAnotherValueTypeInsideASideIsAccepted()
    {
        var exception = Record.Exception(() => new ConvAdditionByAConstant());

        Assert.Null(exception);
    }
}
