using System.Collections.Generic;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Every operation the sequence expression tree offers evaluates to what the same operation on <see cref="Sequence"/> gives.
/// The expression layer is a way of writing the operation down, so its value is the operand type's own answer and nothing else.
/// </summary>
public class SequenceExpressionOperations
{
    private static readonly Sequence A = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence B = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);
    private static readonly Sequence C = new([Point.Origin(), new Segment(0, 6, 0, 3), new Point(6, 18)]);
    /// <summary>Maps $[0, 3]$ onto $[0, 6]$, which is the domain of <see cref="A"/>, so composing the two is well defined.</summary>
    private static readonly Sequence Inner = new([Point.Origin(), new Segment(0, 3, 0, 2), new Point(3, 6)]);
    /// <summary>Jumps at $t = 3$, where its left limit, value and right limit are 3, 4 and 5, so the three sampling operations disagree there.</summary>
    private static readonly Sequence Jumpy = new([
        Point.Origin(), new Segment(0, 3, 0, 1), new Point(3, 4), new Segment(3, 6, 5, 1), new Point(6, 8)]);
    /// <summary>Decreasing, discontinuous and negative, so the property queries have a false answer to give.</summary>
    private static readonly Sequence Awkward = new([
        new Point(0, 2), new Segment(0, 3, 2, -1), new Point(3, -5), new Segment(3, 6, -1, -1), new Point(6, -4)]);

    [Fact]
    public void AdditionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Addition(A, B).Equivalent(A.ToExpression("s").Addition(B.ToExpression("b")).Value));
    }

    [Fact]
    public void SubtractionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Subtraction(A, B).Equivalent(A.ToExpression("s").Subtraction(B.ToExpression("b")).Value));
    }

    [Fact]
    public void MinimumAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Minimum(A, B).Equivalent(A.ToExpression("s").Minimum(B.ToExpression("b")).Value));
    }

    [Fact]
    public void MaximumAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Maximum(A, B).Equivalent(A.ToExpression("s").Maximum(B.ToExpression("b")).Value));
    }

    [Fact]
    public void ConvolutionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Convolution(A, B).Equivalent(A.ToExpression("s").Convolution(B.ToExpression("b")).Value));
    }

    [Fact]
    public void DeconvolutionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.Deconvolution(A, B).Equivalent(A.ToExpression("s").Deconvolution(B.ToExpression("b")).Value));
    }

    [Fact]
    public void MaxPlusConvolutionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.MaxPlusConvolution(A, B).Equivalent(A.ToExpression("s").MaxPlusConvolution(B.ToExpression("b")).Value));
    }

    [Fact]
    public void MaxPlusDeconvolutionAgreesWithTheSequenceOperation()
    {
        Assert.True(Sequence.MaxPlusDeconvolution(A, B).Equivalent(A.ToExpression("s").MaxPlusDeconvolution(B.ToExpression("b")).Value));
    }

    /// <summary>
    /// The commutative and associative operations flatten, so three operands are one n-ary node rather than two nested binary ones.
    /// </summary>
    [Fact]
    public void AdditionFlattensIntoOneNode()
    {
        var a = A.ToExpression("a");
        var b = B.ToExpression("b");
        var c = C.ToExpression("c");

        var twice = a.Addition(b).Addition(c);

        var nAry = Assert.IsAssignableFrom<SequenceNAryExpression>(twice);
        Assert.Equal(3, nAry.Operands.Count);
    }

    [Fact]
    public void MinimumFlattensIntoOneNode()
    {
        var a = A.ToExpression("a");
        var b = B.ToExpression("b");
        var c = C.ToExpression("c");

        var twice = a.Minimum(b).Minimum(c);

        var nAry = Assert.IsAssignableFrom<SequenceNAryExpression>(twice);
        Assert.Equal(3, nAry.Operands.Count);
    }

    [Fact]
    public void MaximumFlattensIntoOneNode()
    {
        var a = A.ToExpression("a");
        var b = B.ToExpression("b");
        var c = C.ToExpression("c");

        var twice = a.Maximum(b).Maximum(c);

        var nAry = Assert.IsAssignableFrom<SequenceNAryExpression>(twice);
        Assert.Equal(3, nAry.Operands.Count);
    }

    [Fact]
    public void ConvolutionFlattensIntoOneNode()
    {
        var a = A.ToExpression("a");
        var b = B.ToExpression("b");
        var c = C.ToExpression("c");

        var twice = a.Convolution(b).Convolution(c);

        var nAry = Assert.IsAssignableFrom<SequenceNAryExpression>(twice);
        Assert.Equal(3, nAry.Operands.Count);
    }

    /// <summary>
    /// The cut is the only crossing from a curve expression to a sequence one, a curve being defined over $[0, +\infty[$ and a sequence over a bounded interval.
    /// </summary>
    [Fact]
    public void CuttingACurveExpressionGivesASequenceExpression()
    {
        var beta = new RateLatencyServiceCurve(2, 1);
        var interval = new Interval(0, 5, true, true);

        var cut = Expressions.FromCurve(beta, "beta").Cut(interval);

        Assert.IsType<CurveCutExpression>(cut);
        Assert.True(beta.Cut(0, 5, true, true).Equivalent(cut.Value));
    }

    /// <summary>
    /// The neighbourhood cut crosses the same way, and carries the right limit at the end that the plain cut cannot answer for.
    /// </summary>
    [Fact]
    public void CuttingACurveExpressionToANeighbourhoodGivesASequenceExpression()
    {
        var f = new StairCurve(30, 100).DelayBy(0);

        var cut = Expressions.FromCurve(f, "f").CutToNeighbourhood(0, 100);

        Assert.IsType<CurveCutToNeighbourhoodExpression>(cut);
        Assert.True(f.CutToNeighbourhood(0, 100).Equivalent(cut.Value));
        Assert.Equal(f.RightLimitAt(100), cut.Value.RightLimitAt(100));
    }

    /// <summary>
    /// A sequence expression cuts to a sequence expression, the two cut nodes differing only in the operand they take.
    /// </summary>
    [Fact]
    public void CuttingASequenceExpressionStaysASequenceExpression()
    {
        var cut = A.ToExpression("a").Cut(new Interval(1, 4, true, true));

        Assert.IsType<SequenceCutExpression>(cut);
        Assert.True(A.Cut(1, 4, true, true).Equivalent(cut.Value));
    }

    [Fact]
    public void NegateAgreesWithTheSequenceOperation()
    {
        Assert.True(A.Negate().Equivalent(A.ToExpression("s").Negate().Value));
    }

    [Fact]
    public void FloorAgreesWithTheSequenceOperation()
    {
        Assert.True(Awkward.Floor().Equivalent(Awkward.ToExpression("s").Floor().Value));
    }

    [Fact]
    public void CeilAgreesWithTheSequenceOperation()
    {
        Assert.True(Awkward.Ceil().Equivalent(Awkward.ToExpression("s").Ceil().Value));
    }

    [Fact]
    public void ToNonNegativeAgreesWithTheSequenceOperation()
    {
        Assert.True(A.ToNonNegative().Equivalent(A.ToExpression("s").ToNonNegative().Value));
    }

    [Fact]
    public void ToLeftContinuousAgreesWithTheSequenceOperation()
    {
        Assert.True(A.ToLeftContinuous().Equivalent(A.ToExpression("s").ToLeftContinuous().Value));
    }

    [Fact]
    public void ToRightContinuousAgreesWithTheSequenceOperation()
    {
        Assert.True(A.ToRightContinuous().Equivalent(A.ToExpression("s").ToRightContinuous().Value));
    }

    [Fact]
    public void LowerPseudoInverseAgreesWithTheSequenceOperation()
    {
        Assert.True(A.LowerPseudoInverse().Equivalent(A.ToExpression("s").LowerPseudoInverse().Value));
    }

    [Fact]
    public void UpperPseudoInverseAgreesWithTheSequenceOperation()
    {
        Assert.True(A.UpperPseudoInverse().Equivalent(A.ToExpression("s").UpperPseudoInverse().Value));
    }

    [Fact]
    public void ScaleAgreesWithTheSequenceOperation()
    {
        Assert.True(A.Scale(3).Equivalent(A.ToExpression("s").Scale(3).Value));
    }

    [Fact]
    public void DelayAgreesWithTheSequenceOperation()
    {
        Assert.True(A.Delay(2).Equivalent(A.ToExpression("s").Delay(2).Value));
    }

    [Fact]
    public void ForwardAgreesWithTheSequenceOperation()
    {
        Assert.True(A.Forward(2).Equivalent(A.ToExpression("s").Forward(2).Value));
    }

    [Fact]
    public void HorizontalShiftAgreesWithTheSequenceOperation()
    {
        Assert.True(A.HorizontalShift(2).Equivalent(A.ToExpression("s").HorizontalShift(2).Value));
    }

    [Fact]
    public void VerticalShiftAgreesWithTheSequenceOperation()
    {
        Assert.True(A.VerticalShift(3).Equivalent(A.ToExpression("s").VerticalShift(3).Value));
    }

    /// <summary>
    /// Composition asks that the inner operand's image lie within the outer operand's domain, so the operands are chosen to satisfy it.
    /// </summary>
    [Fact]
    public void CompositionAgreesWithTheSequenceOperation()
    {
        var result = A.ToExpression("a").Composition(Inner.ToExpression("g")).Value;

        Assert.True(Sequence.Composition(A, Inner).Equivalent(result));
    }

    [Fact]
    public void ValueAtAgreesWithTheSequenceOperation()
    {
        Assert.Equal(Jumpy.ValueAt(3), Jumpy.ToExpression("s").ValueAt(3).Value);
    }

    [Fact]
    public void LeftLimitAtAgreesWithTheSequenceOperation()
    {
        Assert.Equal(Jumpy.LeftLimitAt(3), Jumpy.ToExpression("s").LeftLimitAt(3).Value);
    }

    [Fact]
    public void RightLimitAtAgreesWithTheSequenceOperation()
    {
        Assert.Equal(Jumpy.RightLimitAt(3), Jumpy.ToExpression("s").RightLimitAt(3).Value);
    }

    /// <summary>
    /// The deviations between sequences also cross into the rational tree.
    /// </summary>
    [Fact]
    public void HorizontalDeviationAgreesWithTheSequenceOperation()
    {
        Assert.Equal(Sequence.HorizontalDeviation(B, A), Expressions.HorizontalDeviation(B, A, "b", "a").Value);
    }

    [Fact]
    public void VerticalDeviationAgreesWithTheSequenceOperation()
    {
        Assert.Equal(Sequence.VerticalDeviation(B, A), Expressions.VerticalDeviation(B, A, "b", "a").Value);
    }

    /// <summary>
    /// Renaming reaches through the tree and leaves the value alone.
    /// </summary>
    [Fact]
    public void RenamingKeepsTheValue()
    {
        var sum = A.ToExpression("a").Addition(B.ToExpression("b"));

        var renamed = sum.WithName("s");

        Assert.Equal("s", renamed.Name);
        Assert.True(sum.Value.Equivalent(renamed.Value));
    }

    /// <summary>
    /// A placeholder stands for an expression yet to be supplied, so it has no value to compute.
    /// </summary>
    [Fact]
    public void APlaceholderHasNoValue()
    {
        var placeholder = new SequencePlaceholderExpression("x");

        Assert.Throws<System.InvalidOperationException>(() => placeholder.Value);
    }

    /// <summary>
    /// A sequence expression renders in both notations, and the cut nodes say over what window they cut.
    /// </summary>
    [Fact]
    public void SequenceExpressionsRender()
    {
        var sum = A.ToExpression("a").Addition(B.ToExpression("b"));

        Assert.Equal("a + b", sum.ToUnicodeString());
        Assert.Equal("a + b", sum.ToLatexString());

        var cut = A.ToExpression("a").Cut(new Interval(1, 4, true, true));
        Assert.Contains("cut(a", cut.ToUnicodeString());
        Assert.Contains("[1, 4]", cut.ToUnicodeString());
    }

    /// <summary>
    /// An equivalence applies at every matching subtree and the host's value type takes no part, so a curve equivalence reaches the curve operand of a cut under a sequence expression.
    /// </summary>
    [Fact]
    public void ACurveEquivalenceAppliesAtTheCurveOperandOfACut()
    {
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(2, 4);
        var interval = new Interval(0, 5, true, true);
        var cut = Expressions.SubAdditiveClosure(Expressions.Minimum(f, g)).Cut(interval);

        var applied = cut.ApplyEquivalence(new SubAdditiveClosureOfMin());

        Assert.NotEqual(cut.ToUnicodeString(), applied.ToUnicodeString());
        Assert.True(cut.Compute().Equivalent(applied.Compute()));
    }

    /// <summary>
    /// Where an equivalence matches nothing, a sequence expression is returned unchanged, as it is on the other two trees.
    /// </summary>
    [Fact]
    public void AnEquivalenceThatMatchesNothingLeavesTheSequenceExpressionUnchanged()
    {
        var sum = A.ToExpression("a").Addition(B.ToExpression("b"));

        var applied = sum.ApplyEquivalence(new SubAdditiveClosureOfMin());

        Assert.Equal(sum.ToUnicodeString(), applied.ToUnicodeString());
    }

    /// <summary>
    /// A curve equivalence rewrites the same curve subtree the same way whether it sits under a curve host, a sequence host, or a rational-over-sequence host.
    /// </summary>
    [Fact]
    public void ACurveEquivalenceGivesTheSameCurveRewriteUnderEveryHost()
    {
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(2, 4);
        var interval = new Interval(0, 5, true, true);
        var closureOfMin = Expressions.SubAdditiveClosure(Expressions.Minimum(f, g));
        var equivalence = new SubAdditiveClosureOfMin();

        var underCurveHost = closureOfMin.ApplyEquivalence(equivalence).ToUnicodeString();

        var underSequenceHost = closureOfMin.Cut(interval).ApplyEquivalence(equivalence);
        Assert.Equal(
            underCurveHost,
            Assert.IsType<CurveCutExpression>(underSequenceHost).Operand.ToUnicodeString());

        var underRationalOverSequenceHost = Expressions
            .HorizontalDeviation(closureOfMin.Cut(interval), closureOfMin.Cut(interval))
            .ApplyEquivalence(equivalence);
        var deviation = Assert.IsType<SequenceHorizontalDeviationExpression>(underRationalOverSequenceHost);
        Assert.Equal(
            underCurveHost,
            Assert.IsType<CurveCutExpression>(deviation.LeftOperand).Operand.ToUnicodeString());
        Assert.Equal(
            underCurveHost,
            Assert.IsType<CurveCutExpression>(deviation.RightOperand).Operand.ToUnicodeString());
    }

    /// <summary>
    /// The mppg language describes curves and rationals, so a sequence expression has nothing to be written as.
    /// </summary>
    [Fact]
    public void RenderingASequenceExpressionAsMppgIsRefused()
    {
        var sum = A.ToExpression("a").Addition(B.ToExpression("b"));

        Assert.Throws<System.NotSupportedException>(() => sum.ToMppgString());
    }

    public static TheoryData<Sequence> PropertyOperands => new(A, Awkward);

    [Theory]
    [MemberData(nameof(PropertyOperands))]
    public void IsNonNegativeAgreesWithTheSequence(Sequence sequence)
    {
        Assert.Equal(sequence.IsNonNegative, sequence.ToExpression("s").IsNonNegative);
    }

    [Theory]
    [MemberData(nameof(PropertyOperands))]
    public void IsNonDecreasingAgreesWithTheSequence(Sequence sequence)
    {
        Assert.Equal(sequence.IsNonDecreasing, sequence.ToExpression("s").IsNonDecreasing);
    }

    [Theory]
    [MemberData(nameof(PropertyOperands))]
    public void IsLeftContinuousAgreesWithTheSequence(Sequence sequence)
    {
        Assert.Equal(sequence.IsLeftContinuous, sequence.ToExpression("s").IsLeftContinuous);
    }

    [Theory]
    [MemberData(nameof(PropertyOperands))]
    public void IsRightContinuousAgreesWithTheSequence(Sequence sequence)
    {
        Assert.Equal(sequence.IsRightContinuous, sequence.ToExpression("s").IsRightContinuous);
    }

    /// <summary>
    /// The operations that cross into the rational tree must render too.
    /// <see cref="RationalExpression.ToString"/> is the Unicode rendering, so a node no formatter handles cannot be printed, interpolated, or shown in a failure message.
    /// </summary>
    [Fact]
    public void ValueAtRenders()
    {
        var expression = Jumpy.ToExpression("j").ValueAt(3);

        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToString());
    }

    [Fact]
    public void LeftLimitAtRenders()
    {
        var expression = Jumpy.ToExpression("j").LeftLimitAt(3);

        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToString());
    }

    [Fact]
    public void RightLimitAtRenders()
    {
        var expression = Jumpy.ToExpression("j").RightLimitAt(3);

        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToString());
    }

    [Fact]
    public void HorizontalDeviationRenders()
    {
        var expression = Expressions.HorizontalDeviation(B, A, "b", "a");

        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToString());
    }

    [Fact]
    public void VerticalDeviationRenders()
    {
        var expression = Expressions.VerticalDeviation(B, A, "b", "a");

        Assert.NotEmpty(expression.ToUnicodeString());
        Assert.NotEmpty(expression.ToLatexString());
        Assert.NotEmpty(expression.ToString());
    }

    /// <summary>
    /// <see cref="SequenceExpression.ToString"/> renders the expression, as it does on the other two trees, rather than naming its type.
    /// </summary>
    [Fact]
    public void ToStringRendersTheExpression()
    {
        var sum = A.ToExpression("a").Addition(B.ToExpression("b"));

        Assert.Equal(sum.ToUnicodeString(), sum.ToString());
        Assert.DoesNotContain("Expression", sum.ToString());
    }

    /// <summary>
    /// Chaining onto an existing n-ary node must keep the name and settings given for the result, not attach them to the operand being appended.
    /// </summary>
    [Fact]
    public void AdditionChainingKeepsTheGivenName()
    {
        var start = A.ToExpression("a");
        var chained = start.Addition(B).Addition(C, expressionName: "s");

        Assert.Equal("s", chained.Name);
    }

    [Fact]
    public void MinimumChainingKeepsTheGivenName()
    {
        var start = A.ToExpression("a");
        var chained = start.Minimum(B).Minimum(C, expressionName: "s");

        Assert.Equal("s", chained.Name);
    }

    [Fact]
    public void MaximumChainingKeepsTheGivenName()
    {
        var start = A.ToExpression("a");
        var chained = start.Maximum(B).Maximum(C, expressionName: "s");

        Assert.Equal("s", chained.Name);
    }

    [Fact]
    public void ConvolutionChainingKeepsTheGivenName()
    {
        var start = A.ToExpression("a");
        var chained = start.Convolution(B).Convolution(C, expressionName: "s");

        Assert.Equal("s", chained.Name);
    }

    /// <summary>
    /// A tree more than one level deep evaluates to the same nesting of the operand type's own operations.
    /// </summary>
    [Fact]
    public void NestedExpressionsEvaluate()
    {
        var nested = A.ToExpression("a").Addition(B.ToExpression("b"))
            .Convolution(A.ToExpression("a").Minimum(C.ToExpression("c")));

        var expected = Sequence.Convolution(Sequence.Addition(A, B), Sequence.Minimum(A, C));

        Assert.True(expected.Equivalent(nested.Value));
    }

    /// <summary>
    /// Appending one n-ary node to another of the same operator merges them, rather than nesting one inside the other.
    /// </summary>
    [Fact]
    public void MergingTwoNAryNodesFlattensThem()
    {
        var left = A.ToExpression("a").Addition(B.ToExpression("b"));
        var right = C.ToExpression("c").Addition(A.ToExpression("d"));

        var merged = left.Addition(right);

        var nAry = Assert.IsAssignableFrom<SequenceNAryExpression>(merged);
        Assert.Equal(4, nAry.Operands.Count);
        Assert.True(Sequence.Addition(Sequence.Addition(A, B), Sequence.Addition(C, A)).Equivalent(merged.Value));
    }

    /// <summary>
    /// The Sum extension is the n-ary addition over a collection.
    /// </summary>
    [Fact]
    public void SumOverACollectionIsTheAddition()
    {
        var sum = new[] { A.ToExpression("a"), B.ToExpression("b"), C.ToExpression("c") }.Sum();

        Assert.Equal(3, sum.Operands.Count);
        Assert.True(Sequence.Addition(Sequence.Addition(A, B), C).Equivalent(sum.Value));
    }

    /// <summary>
    /// The neighbourhood cut of a sequence expression carries the right limit at the end, as the operator does.
    /// </summary>
    [Fact]
    public void CuttingASequenceExpressionToANeighbourhoodCarriesTheRightLimit()
    {
        var cut = Jumpy.ToExpression("j").CutToNeighbourhood(0, 3);

        Assert.IsType<SequenceCutToNeighbourhoodExpression>(cut);
        Assert.True(Jumpy.CutToNeighbourhood(0, 3).Equivalent(cut.Value));
        Assert.Equal(Jumpy.RightLimitAt(3), cut.Value.RightLimitAt(3));
    }
}
