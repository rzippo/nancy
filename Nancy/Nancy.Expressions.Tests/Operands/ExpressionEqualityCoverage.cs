using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// Coverage sweep: every concrete CurveExpression/RationalExpression/SequenceExpression subclass gets one direct Equals/GetHashCode case here.
// A reflection count confirms the list is complete, so a class added later without a matching case fails loudly.
public class ExpressionEqualityCoverage
{
    private static readonly Curve Curve1 = new RateLatencyServiceCurve(rate: 2, latency: 1);
    private static readonly Curve Curve2 = new RateLatencyServiceCurve(rate: 3, latency: 1);
    private static readonly Curve Curve3 = new RateLatencyServiceCurve(rate: 5, latency: 1);
    private static readonly Rational Rational1 = new(1, 2);
    private static readonly Rational Rational2 = new(1, 3);
    private static readonly Rational Rational3 = new(2, 5);
    private static readonly Sequence Sequence1 = new Sequence([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence Sequence2 = new Sequence([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);
    private static readonly Sequence Sequence3 = new Sequence([Point.Origin(), new Segment(0, 6, 0, 3), new Point(6, 18)]);
    private static readonly Interval Interval1 = new Interval(1, 4, true, true);
    private static readonly Interval Interval2 = new Interval(2, 5, true, true);
    // maps [0,3] onto [0,6], which is the domain of Sequence1, so the composition is well defined
    private static readonly Sequence Inner1 = new Sequence([Point.Origin(), new Segment(0, 3, 0, 2), new Point(3, 6)]);
    private static readonly Sequence Inner2 = new Sequence([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);

    public static IEnumerable<object[]> OneOfEachConcreteType()
    {
        // Curve, unary (operand: Curve)
        yield return Case(() => new CeilExpression(Curve1, "a"), () => new CeilExpression(Curve2, "a"));
        yield return Case(() => new FloorExpression(Curve1, "a"), () => new FloorExpression(Curve2, "a"));
        yield return Case(() => new NegateExpression(Curve1, "a"), () => new NegateExpression(Curve2, "a"));
        yield return Case(() => new ToLeftContinuousExpression(Curve1, "a"), () => new ToLeftContinuousExpression(Curve2, "a"));
        yield return Case(() => new ToLowerNonDecreasingExpression(Curve1, "a"), () => new ToLowerNonDecreasingExpression(Curve2, "a"));
        yield return Case(() => new ToLowerNonIncreasingExpression(Curve1, "a"), () => new ToLowerNonIncreasingExpression(Curve2, "a"));
        yield return Case(() => new ToNonNegativeExpression(Curve1, "a"), () => new ToNonNegativeExpression(Curve2, "a"));
        yield return Case(() => new ToRightContinuousExpression(Curve1, "a"), () => new ToRightContinuousExpression(Curve2, "a"));
        yield return Case(() => new ToUpperNonDecreasingExpression(Curve1, "a"), () => new ToUpperNonDecreasingExpression(Curve2, "a"));
        yield return Case(() => new ToUpperNonIncreasingExpression(Curve1, "a"), () => new ToUpperNonIncreasingExpression(Curve2, "a"));
        yield return Case(() => new UpperPseudoInverseExpression(Curve1, "a"), () => new UpperPseudoInverseExpression(Curve2, "a"));
        yield return Case(() => new LowerPseudoInverseExpression(Curve1, "a"), () => new LowerPseudoInverseExpression(Curve2, "a"));
        yield return Case(() => new WithZeroOriginExpression(Curve1, "a"), () => new WithZeroOriginExpression(Curve2, "a"));
        yield return Case(() => new SubAdditiveClosureExpression(Curve1, "a"), () => new SubAdditiveClosureExpression(Curve2, "a"));
        yield return Case(() => new SuperAdditiveClosureExpression(Curve1, "a"), () => new SuperAdditiveClosureExpression(Curve2, "a"));
        yield return Case(() => new WithOriginAtExpression(Curve1, "a", Rational1), () => new WithOriginAtExpression(Curve1, "a", Rational2));

        // Curve, binary (Curve, Curve)
        yield return Case(
            () => new CompositionExpression(Curve1, "a", Curve2, "b"),
            () => new CompositionExpression(Curve1, "a", Curve3, "b"));
        yield return Case(
            () => new DeconvolutionExpression(Curve1, "a", Curve2, "b"),
            () => new DeconvolutionExpression(Curve1, "a", Curve3, "b"));
        yield return Case(
            () => new MaxPlusDeconvolutionExpression(Curve1, "a", Curve2, "b"),
            () => new MaxPlusDeconvolutionExpression(Curve1, "a", Curve3, "b"));
        yield return Case(
            () => new SubtractionExpression(Curve1, "a", Curve2, "b"),
            () => new SubtractionExpression(Curve1, "a", Curve3, "b"));

        // Curve, binary (Curve, Rational)
        yield return Case(() => new DelayByExpression(Curve1, "a", Rational1), () => new DelayByExpression(Curve1, "a", Rational2));
        yield return Case(() => new ForwardByExpression(Curve1, "a", Rational1), () => new ForwardByExpression(Curve1, "a", Rational2));
        yield return Case(() => new HorizontalShiftExpression(Curve1, "a", Rational1), () => new HorizontalShiftExpression(Curve1, "a", Rational2));
        yield return Case(() => new ScaleExpression(Curve1, "a", Rational1), () => new ScaleExpression(Curve1, "a", Rational2));
        yield return Case(() => new VerticalShiftExpression(Curve1, "a", Rational1), () => new VerticalShiftExpression(Curve1, "a", Rational2));

        // Curve, n-ary
        yield return Case(
            () => new AdditionExpression([Curve1.ToExpression("a"), Curve2.ToExpression("b")], "s"),
            () => new AdditionExpression([Curve1.ToExpression("a"), Curve3.ToExpression("b")], "s"));
        yield return Case(
            () => new ConvolutionExpression([Curve1.ToExpression("a"), Curve2.ToExpression("b")], "s"),
            () => new ConvolutionExpression([Curve1.ToExpression("a"), Curve3.ToExpression("b")], "s"));
        yield return Case(
            () => new MaxPlusConvolutionExpression([Curve1.ToExpression("a"), Curve2.ToExpression("b")], "s"),
            () => new MaxPlusConvolutionExpression([Curve1.ToExpression("a"), Curve3.ToExpression("b")], "s"));
        yield return Case(
            () => new MaximumExpression([Curve1.ToExpression("a"), Curve2.ToExpression("b")], "s"),
            () => new MaximumExpression([Curve1.ToExpression("a"), Curve3.ToExpression("b")], "s"));
        yield return Case(
            () => new MinimumExpression([Curve1.ToExpression("a"), Curve2.ToExpression("b")], "s"),
            () => new MinimumExpression([Curve1.ToExpression("a"), Curve3.ToExpression("b")], "s"));

        // Curve, leaf / placeholder
        yield return Case(() => new ConcreteCurveExpression(Curve1, "a"), () => new ConcreteCurveExpression(Curve2, "a"));
        yield return Case(() => new CurvePlaceholderExpression("a"), () => new CurvePlaceholderExpression("b"));

        // Rational, unary (operand: Rational)
        yield return Case(() => new InvertRationalExpression(Rational1), () => new InvertRationalExpression(Rational2));
        yield return Case(() => new NegateRationalExpression(Rational1), () => new NegateRationalExpression(Rational2));
        yield return Case(() => new RationalAbsoluteValueExpression(Rational1), () => new RationalAbsoluteValueExpression(Rational2));
        yield return Case(() => new RationalCeilExpression(Rational1), () => new RationalCeilExpression(Rational2));
        yield return Case(() => new RationalFloorExpression(Rational1), () => new RationalFloorExpression(Rational2));

        // Rational, unary (operand: Curve)
        yield return Case(() => new InfValueExpression(Curve1, "a"), () => new InfValueExpression(Curve2, "a"));
        yield return Case(() => new SupValueExpression(Curve1, "a"), () => new SupValueExpression(Curve2, "a"));
        yield return Case(() => new MinValueExpression(Curve1, "a"), () => new MinValueExpression(Curve2, "a"));
        yield return Case(() => new MaxValueExpression(Curve1, "a"), () => new MaxValueExpression(Curve2, "a"));

        // Rational, binary (Rational, Rational)
        yield return Case(
            () => new RationalDivisionExpression(Rational1.ToExpression("a"), Rational2.ToExpression("b")),
            () => new RationalDivisionExpression(Rational1.ToExpression("a"), Rational3.ToExpression("b")));
        yield return Case(
            () => new RationalModuloExpression(Rational1.ToExpression("a"), Rational2.ToExpression("b")),
            () => new RationalModuloExpression(Rational1.ToExpression("a"), Rational3.ToExpression("b")));
        yield return Case(
            () => new RationalPowerExpression(Rational1.ToExpression("a"), Rational2.ToExpression("b")),
            () => new RationalPowerExpression(Rational1.ToExpression("a"), Rational3.ToExpression("b")));
        yield return Case(
            () => new RationalSubtractionExpression(Rational1.ToExpression("a"), Rational2.ToExpression("b")),
            () => new RationalSubtractionExpression(Rational1.ToExpression("a"), Rational3.ToExpression("b")));

        // Rational, binary (Curve, Rational)
        yield return Case(() => new LeftLimitAtExpression(Curve1, "a", Rational1), () => new LeftLimitAtExpression(Curve1, "a", Rational2));
        yield return Case(() => new RightLimitAtExpression(Curve1, "a", Rational1), () => new RightLimitAtExpression(Curve1, "a", Rational2));
        yield return Case(() => new ValueAtExpression(Curve1, "a", Rational1), () => new ValueAtExpression(Curve1, "a", Rational2));

        // Rational, binary (Curve, Curve)
        yield return Case(
            () => new HorizontalDeviationExpression(Curve1, "a", Curve2, "b"),
            () => new HorizontalDeviationExpression(Curve1, "a", Curve3, "b"));
        yield return Case(
            () => new VerticalDeviationExpression(Curve1, "a", Curve2, "b"),
            () => new VerticalDeviationExpression(Curve1, "a", Curve3, "b"));
        yield return Case(
            () => new ZDeviationExpression(Curve1, "a", Curve2, "b"),
            () => new ZDeviationExpression(Curve1, "a", Curve3, "b"));

        // Rational, n-ary
        yield return Case(
            () => new RationalAdditionExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalAdditionExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));
        yield return Case(
            () => new RationalProductExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalProductExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));
        yield return Case(
            () => new RationalMaximumExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalMaximumExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));
        yield return Case(
            () => new RationalMinimumExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalMinimumExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));
        yield return Case(
            () => new RationalGreatestCommonDivisorExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalGreatestCommonDivisorExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));
        yield return Case(
            () => new RationalLeastCommonMultipleExpression([Rational1.ToExpression("a"), Rational2.ToExpression("b")], "s"),
            () => new RationalLeastCommonMultipleExpression([Rational1.ToExpression("a"), Rational3.ToExpression("b")], "s"));

        // Rational, leaf / placeholder
        yield return Case(() => new RationalNumberExpression(Rational1), () => new RationalNumberExpression(Rational2));
        yield return Case(() => new RationalPlaceholderExpression("a"), () => new RationalPlaceholderExpression("b"));

        static object[] Case(Func<object> make, Func<object> makeDifferent)
            => new object[] { make, makeDifferent };

        // Sequence, unary (operand: Sequence)
        yield return Case(() => new SequenceToNonNegativeExpression(Sequence1, "a"), () => new SequenceToNonNegativeExpression(Sequence2, "a"));
        yield return Case(
            () => new SequenceCutExpression(Sequence1, "a", Interval1),
            () => new SequenceCutExpression(Sequence1, "a", Interval2));
        yield return Case(
            () => new SequenceCutToNeighbourhoodExpression(Sequence1, "a", 1, 4),
            () => new SequenceCutToNeighbourhoodExpression(Sequence1, "a", 2, 5));

        // Sequence, unary (operand: Curve)
        yield return Case(
            () => new CurveCutExpression(Curve1, "a", Interval1),
            () => new CurveCutExpression(Curve1, "a", Interval2));
        yield return Case(
            () => new CurveCutToNeighbourhoodExpression(Curve1, "a", 1, 4),
            () => new CurveCutToNeighbourhoodExpression(Curve1, "a", 2, 5));

        // Sequence, binary (Sequence, Sequence)
        yield return Case(
            () => new SequenceSubtractionExpression(Sequence1, "a", Sequence2, "b"),
            () => new SequenceSubtractionExpression(Sequence1, "a", Sequence3, "b"));
        yield return Case(
            () => new SequenceDeconvolutionExpression(Sequence1, "a", Sequence2, "b"),
            () => new SequenceDeconvolutionExpression(Sequence1, "a", Sequence3, "b"));
        yield return Case(
            () => new SequenceMaxPlusDeconvolutionExpression(Sequence1, "a", Sequence2, "b"),
            () => new SequenceMaxPlusDeconvolutionExpression(Sequence1, "a", Sequence3, "b"));

        // Sequence, n-ary
        yield return Case(
            () => new SequenceAdditionExpression([Sequence1.ToExpression("a"), Sequence2.ToExpression("b")], "s"),
            () => new SequenceAdditionExpression([Sequence1.ToExpression("a"), Sequence3.ToExpression("b")], "s"));
        yield return Case(
            () => new SequenceMinimumExpression([Sequence1.ToExpression("a"), Sequence2.ToExpression("b")], "s"),
            () => new SequenceMinimumExpression([Sequence1.ToExpression("a"), Sequence3.ToExpression("b")], "s"));
        yield return Case(
            () => new SequenceMaximumExpression([Sequence1.ToExpression("a"), Sequence2.ToExpression("b")], "s"),
            () => new SequenceMaximumExpression([Sequence1.ToExpression("a"), Sequence3.ToExpression("b")], "s"));
        yield return Case(
            () => new SequenceConvolutionExpression([Sequence1.ToExpression("a"), Sequence2.ToExpression("b")], "s"),
            () => new SequenceConvolutionExpression([Sequence1.ToExpression("a"), Sequence3.ToExpression("b")], "s"));
        yield return Case(
            () => new SequenceMaxPlusConvolutionExpression([Sequence1.ToExpression("a"), Sequence2.ToExpression("b")], "s"),
            () => new SequenceMaxPlusConvolutionExpression([Sequence1.ToExpression("a"), Sequence3.ToExpression("b")], "s"));

        // Sequence, leaf
        yield return Case(() => new ConcreteSequenceExpression(Sequence1, "a"), () => new ConcreteSequenceExpression(Sequence2, "a"));

        // Rational, from sequences
        yield return Case(
            () => new SequenceHorizontalDeviationExpression(Sequence1, "a", Sequence2, "b"),
            () => new SequenceHorizontalDeviationExpression(Sequence1, "a", Sequence3, "b"));
        yield return Case(
            () => new SequenceVerticalDeviationExpression(Sequence1, "a", Sequence2, "b"),
            () => new SequenceVerticalDeviationExpression(Sequence1, "a", Sequence3, "b"));

        // Sequence, unary (operand: Sequence), no parameter
        yield return Case(() => new SequenceNegateExpression(Sequence1, "a"), () => new SequenceNegateExpression(Sequence2, "a"));
        yield return Case(() => new SequenceFloorExpression(Sequence1, "a"), () => new SequenceFloorExpression(Sequence2, "a"));
        yield return Case(() => new SequenceCeilExpression(Sequence1, "a"), () => new SequenceCeilExpression(Sequence2, "a"));
        yield return Case(() => new SequenceToLeftContinuousExpression(Sequence1, "a"), () => new SequenceToLeftContinuousExpression(Sequence2, "a"));
        yield return Case(() => new SequenceToRightContinuousExpression(Sequence1, "a"), () => new SequenceToRightContinuousExpression(Sequence2, "a"));
        yield return Case(() => new SequenceLowerPseudoInverseExpression(Sequence1, "a"), () => new SequenceLowerPseudoInverseExpression(Sequence2, "a"));
        yield return Case(() => new SequenceUpperPseudoInverseExpression(Sequence1, "a"), () => new SequenceUpperPseudoInverseExpression(Sequence2, "a"));

        // Sequence, binary (Sequence, Rational)
        yield return Case(
            () => new SequenceScaleExpression(Sequence1, "a", Rational1),
            () => new SequenceScaleExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceDelayExpression(Sequence1, "a", Rational1),
            () => new SequenceDelayExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceForwardExpression(Sequence1, "a", Rational1),
            () => new SequenceForwardExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceHorizontalShiftExpression(Sequence1, "a", Rational1),
            () => new SequenceHorizontalShiftExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceVerticalShiftExpression(Sequence1, "a", Rational1),
            () => new SequenceVerticalShiftExpression(Sequence1, "a", Rational2));

        // Sequence, binary (Sequence, Sequence)
        yield return Case(
            () => new SequenceCompositionExpression(Sequence1, "a", Inner1, "g"),
            () => new SequenceCompositionExpression(Sequence1, "a", Inner2, "g"));

        // Sequence, leaf placeholder
        yield return Case(() => new SequencePlaceholderExpression("a"), () => new SequencePlaceholderExpression("b"));

        // Rational, sampling a sequence
        yield return Case(
            () => new SequenceValueAtExpression(Sequence1, "a", Rational1),
            () => new SequenceValueAtExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceLeftLimitAtExpression(Sequence1, "a", Rational1),
            () => new SequenceLeftLimitAtExpression(Sequence1, "a", Rational2));
        yield return Case(
            () => new SequenceRightLimitAtExpression(Sequence1, "a", Rational1),
            () => new SequenceRightLimitAtExpression(Sequence1, "a", Rational2));
    }

    [Theory]
    [MemberData(nameof(OneOfEachConcreteType))]
    public void EachConcreteTypeIsReflexiveAndDistinguishesADifferentInstance(Func<object> make, Func<object> makeDifferent)
    {
        var a = make();
        var b = make();
        var different = makeDifferent();

        Assert.True(a.Equals(a));
        Assert.True(a.Equals(b));
        Assert.True(b.Equals(a));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        Assert.False(a.Equals(different));
    }

    [Fact]
    public void EveryConcreteExpressionTypeHasACoverageCase()
    {
        var caseCount = OneOfEachConcreteType().Count();

        var concreteTypeCount = typeof(Unipi.Nancy.Expressions.CurveExpression).Assembly
            .GetTypes()
            .Count(t => !t.IsAbstract && t.IsClass &&
                        (typeof(Unipi.Nancy.Expressions.CurveExpression).IsAssignableFrom(t) ||
                         typeof(Unipi.Nancy.Expressions.RationalExpression).IsAssignableFrom(t) ||
                         typeof(Unipi.Nancy.Expressions.SequenceExpression).IsAssignableFrom(t)));

        Assert.Equal(concreteTypeCount, caseCount);
    }
}
