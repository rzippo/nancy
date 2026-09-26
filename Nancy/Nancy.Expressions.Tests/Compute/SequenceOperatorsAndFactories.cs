using System.Collections.Generic;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

/// <summary>
/// Every operator and static factory the sequence tree offers is the same expression as the instance method it stands for:
/// structurally equal, and computing to equivalent sequences.
/// </summary>
public class SequenceOperatorsAndFactories
{
    private static readonly Sequence SeqA = new([Point.Origin(), new Segment(0, 6, 0, 1), new Point(6, 6)]);
    private static readonly Sequence SeqB = new([Point.Origin(), new Segment(0, 6, 0, 2), new Point(6, 12)]);
    /// <summary>Maps $[0, 3]$ onto $[0, 6]$, the domain of <see cref="SeqA"/>, so composing the two is well defined.</summary>
    private static readonly Sequence SeqInner = new([Point.Origin(), new Segment(0, 3, 0, 2), new Point(3, 6)]);
    /// <summary>Right-open, so the sequence following it can be left-closed at the join, as concatenation needs.</summary>
    private static readonly Sequence SeqConcatL = new([Point.Origin(), new Segment(0, 2, 0, 1)]);
    private static readonly Sequence SeqConcatR = new([new Point(1, 10), new Segment(1, 3, 10, 2)]);

    private static readonly SequenceExpression A = SeqA.ToExpression("a");
    private static readonly SequenceExpression B = SeqB.ToExpression("b");
    private static readonly SequenceExpression Inner = SeqInner.ToExpression("inner");
    private static readonly SequenceExpression ConcatL = SeqConcatL.ToExpression("l");
    private static readonly SequenceExpression ConcatR = SeqConcatR.ToExpression("r");

    private static readonly Rational K = new(3);
    private static readonly RationalExpression Ke = K.ToExpression("k");

    private static readonly Curve Beta = new RateLatencyServiceCurve(2, 1);
    private static readonly CurveExpression BetaE = Beta.ToExpression("beta");
    private static readonly Interval CurveWindow = new(0, 5, true, true);
    private static readonly Interval SeqWindow = new(1, 4, true, true);

    public static IEnumerable<object[]> SequenceCases()
    {
        // Operators
        yield return ["operator + (expr, expr)", A + B, A.Addition(B)];
        yield return ["operator - (expr, expr)", A - B, A.Subtraction(B)];
        yield return ["operator - (expr, sequence)", A - SeqB, A.Subtraction(SeqB)];
        yield return ["operator - (expr, rational expr)", A - Ke, A.VerticalShift(Ke.Negate())];
        yield return ["operator - (expr, rational)", A - K, A.VerticalShift(-K)];
        yield return ["operator * (expr, rational expr)", A * Ke, A.Scale(Ke)];
        yield return ["operator * (expr, rational)", A * K, A.Scale(K)];
        yield return ["operator / (expr, rational expr)", A / Ke, A.Scale(Ke.Invert())];
        yield return ["operator / (expr, rational)", A / K, A.Scale(1 / K)];
        yield return ["operator - (unary)", -A, A.Negate()];
        yield return ["operator + (rational expr, expr)", Ke + A, A.VerticalShift(Ke)];
        yield return ["operator + (rational, expr)", K + A, A.VerticalShift(K)];
        yield return ["operator * (rational expr, expr)", Ke * A, A.Scale(Ke)];
        yield return ["operator * (rational, expr)", K * A, A.Scale(K)];

        // Unary factories
        yield return ["Negate (expr)", Expressions.Negate(A), A.Negate()];
        yield return ["Negate (sequence)", Expressions.Negate(SeqA), SeqA.ToExpression().Negate()];
        yield return ["ToNonNegative (expr)", Expressions.ToNonNegative(A), A.ToNonNegative()];
        yield return ["ToNonNegative (sequence)", Expressions.ToNonNegative(SeqA), SeqA.ToExpression().ToNonNegative()];
        yield return ["Floor (expr)", Expressions.Floor(A), A.Floor()];
        yield return ["Floor (sequence)", Expressions.Floor(SeqA), SeqA.ToExpression().Floor()];
        yield return ["Ceil (expr)", Expressions.Ceil(A), A.Ceil()];
        yield return ["Ceil (sequence)", Expressions.Ceil(SeqA), SeqA.ToExpression().Ceil()];
        yield return ["ToLeftContinuous (expr)", Expressions.ToLeftContinuous(A), A.ToLeftContinuous()];
        yield return ["ToLeftContinuous (sequence)", Expressions.ToLeftContinuous(SeqA), SeqA.ToExpression().ToLeftContinuous()];
        yield return ["ToRightContinuous (expr)", Expressions.ToRightContinuous(A), A.ToRightContinuous()];
        yield return ["ToRightContinuous (sequence)", Expressions.ToRightContinuous(SeqA), SeqA.ToExpression().ToRightContinuous()];
        yield return ["LowerPseudoInverse (expr)", Expressions.LowerPseudoInverse(A), A.LowerPseudoInverse()];
        yield return ["LowerPseudoInverse (sequence)", Expressions.LowerPseudoInverse(SeqA), SeqA.ToExpression().LowerPseudoInverse()];
        yield return ["UpperPseudoInverse (expr)", Expressions.UpperPseudoInverse(A), A.UpperPseudoInverse()];
        yield return ["UpperPseudoInverse (sequence)", Expressions.UpperPseudoInverse(SeqA), SeqA.ToExpression().UpperPseudoInverse()];

        // Addition
        yield return ["Addition (expr, expr)", Expressions.Addition(A, B), A.Addition(B)];
        yield return ["Addition (expr, sequence)", Expressions.Addition(A, SeqB), A.Addition(SeqB)];
        yield return ["Addition (sequence, sequence)", Expressions.Addition(SeqA, SeqB), SeqA.ToExpression().Addition(SeqB)];
        yield return ["Addition (sequence, expr)", Expressions.Addition(SeqA, B), SeqA.ToExpression().Addition(B)];
        yield return ["Addition (sequences, names)", Expressions.Addition([SeqA, SeqB], ["a", "b"]), SeqA.ToExpression().Addition(SeqB)];
        yield return ["Addition (expressions)", Expressions.Addition([A, B]), A.Addition(B)];

        // Subtraction
        yield return ["Subtraction (expr, expr)", Expressions.Subtraction(A, B), A.Subtraction(B)];
        yield return ["Subtraction (expr, sequence)", Expressions.Subtraction(A, SeqB), A.Subtraction(SeqB)];
        yield return ["Subtraction (sequence, sequence)", Expressions.Subtraction(SeqA, SeqB), SeqA.ToExpression().Subtraction(SeqB)];
        yield return ["Subtraction (sequence, expr)", Expressions.Subtraction(SeqA, B), SeqA.ToExpression().Subtraction(B)];

        // Minimum
        yield return ["Minimum (expr, expr)", Expressions.Minimum(A, B), A.Minimum(B)];
        yield return ["Minimum (expr, sequence)", Expressions.Minimum(A, SeqB), A.Minimum(SeqB)];
        yield return ["Minimum (sequence, sequence)", Expressions.Minimum(SeqA, SeqB), SeqA.ToExpression().Minimum(SeqB)];
        yield return ["Minimum (sequence, expr)", Expressions.Minimum(SeqA, B), SeqA.ToExpression().Minimum(B)];
        yield return ["Minimum (sequences, names)", Expressions.Minimum([SeqA, SeqB], ["a", "b"]), SeqA.ToExpression().Minimum(SeqB)];
        yield return ["Minimum (expressions)", Expressions.Minimum([A, B]), A.Minimum(B)];

        // Maximum
        yield return ["Maximum (expr, expr)", Expressions.Maximum(A, B), A.Maximum(B)];
        yield return ["Maximum (expr, sequence)", Expressions.Maximum(A, SeqB), A.Maximum(SeqB)];
        yield return ["Maximum (sequence, sequence)", Expressions.Maximum(SeqA, SeqB), SeqA.ToExpression().Maximum(SeqB)];
        yield return ["Maximum (sequence, expr)", Expressions.Maximum(SeqA, B), SeqA.ToExpression().Maximum(B)];
        yield return ["Maximum (sequences, names)", Expressions.Maximum([SeqA, SeqB], ["a", "b"]), SeqA.ToExpression().Maximum(SeqB)];
        yield return ["Maximum (expressions)", Expressions.Maximum([A, B]), A.Maximum(B)];

        // Convolution
        yield return ["Convolution (expr, expr)", Expressions.Convolution(A, B), A.Convolution(B)];
        yield return ["Convolution (expr, sequence)", Expressions.Convolution(A, SeqB), A.Convolution(SeqB)];
        yield return ["Convolution (sequence, sequence)", Expressions.Convolution(SeqA, SeqB), SeqA.ToExpression().Convolution(SeqB)];
        yield return ["Convolution (sequence, expr)", Expressions.Convolution(SeqA, B), SeqA.ToExpression().Convolution(B)];
        yield return ["Convolution (sequences, names)", Expressions.Convolution([SeqA, SeqB], ["a", "b"]), SeqA.ToExpression().Convolution(SeqB)];
        yield return ["Convolution (expressions)", Expressions.Convolution([A, B]), A.Convolution(B)];

        // Deconvolution
        yield return ["Deconvolution (expr, expr)", Expressions.Deconvolution(A, B), A.Deconvolution(B)];
        yield return ["Deconvolution (expr, sequence)", Expressions.Deconvolution(A, SeqB), A.Deconvolution(SeqB)];
        yield return ["Deconvolution (sequence, sequence)", Expressions.Deconvolution(SeqA, SeqB), SeqA.ToExpression().Deconvolution(SeqB)];
        yield return ["Deconvolution (sequence, expr)", Expressions.Deconvolution(SeqA, B), SeqA.ToExpression().Deconvolution(B)];

        // MaxPlusConvolution
        yield return ["MaxPlusConvolution (expr, expr)", Expressions.MaxPlusConvolution(A, B), A.MaxPlusConvolution(B)];
        yield return ["MaxPlusConvolution (expr, sequence)", Expressions.MaxPlusConvolution(A, SeqB), A.MaxPlusConvolution(SeqB)];
        yield return ["MaxPlusConvolution (sequence, sequence)", Expressions.MaxPlusConvolution(SeqA, SeqB), SeqA.ToExpression().MaxPlusConvolution(SeqB)];
        yield return ["MaxPlusConvolution (sequence, expr)", Expressions.MaxPlusConvolution(SeqA, B), SeqA.ToExpression().MaxPlusConvolution(B)];
        yield return ["MaxPlusConvolution (sequences, names)", Expressions.MaxPlusConvolution([SeqA, SeqB], ["a", "b"]), SeqA.ToExpression().MaxPlusConvolution(SeqB)];
        yield return ["MaxPlusConvolution (expressions)", Expressions.MaxPlusConvolution([A, B]), A.MaxPlusConvolution(B)];

        // MaxPlusDeconvolution
        yield return ["MaxPlusDeconvolution (expr, expr)", Expressions.MaxPlusDeconvolution(A, B), A.MaxPlusDeconvolution(B)];
        yield return ["MaxPlusDeconvolution (expr, sequence)", Expressions.MaxPlusDeconvolution(A, SeqB), A.MaxPlusDeconvolution(SeqB)];
        yield return ["MaxPlusDeconvolution (sequence, sequence)", Expressions.MaxPlusDeconvolution(SeqA, SeqB), SeqA.ToExpression().MaxPlusDeconvolution(SeqB)];
        yield return ["MaxPlusDeconvolution (sequence, expr)", Expressions.MaxPlusDeconvolution(SeqA, B), SeqA.ToExpression().MaxPlusDeconvolution(B)];

        // Composition
        yield return ["Composition (expr, expr)", Expressions.Composition(A, Inner), A.Composition(Inner)];
        yield return ["Composition (expr, sequence)", Expressions.Composition(A, SeqInner), A.Composition(SeqInner)];
        yield return ["Composition (sequence, sequence)", Expressions.Composition(SeqA, SeqInner), SeqA.ToExpression().Composition(SeqInner)];
        yield return ["Composition (sequence, expr)", Expressions.Composition(SeqA, Inner), SeqA.ToExpression().Composition(Inner)];

        // Concat
        yield return ["Concat (expr, expr)", Expressions.Concat(ConcatL, ConcatR), ConcatL.Concat(ConcatR)];
        yield return ["Concat (expr, sequence)", Expressions.Concat(ConcatL, SeqConcatR), ConcatL.Concat(SeqConcatR)];
        yield return ["Concat (sequence, sequence)", Expressions.Concat(SeqConcatL, SeqConcatR), SeqConcatL.ToExpression().Concat(SeqConcatR)];
        yield return ["Concat (sequence, expr)", Expressions.Concat(SeqConcatL, ConcatR), SeqConcatL.ToExpression().Concat(ConcatR)];

        // Delay
        yield return ["Delay (expr, rational expr)", Expressions.Delay(A, Ke), A.Delay(Ke)];
        yield return ["Delay (expr, rational)", Expressions.Delay(A, K), A.Delay(K)];
        yield return ["Delay (sequence, rational)", Expressions.Delay(SeqA, K), SeqA.ToExpression().Delay(K)];
        yield return ["Delay (sequence, rational expr)", Expressions.Delay(SeqA, Ke), SeqA.ToExpression().Delay(Ke)];

        // Forward
        yield return ["Forward (expr, rational expr)", Expressions.Forward(A, Ke), A.Forward(Ke)];
        yield return ["Forward (expr, rational)", Expressions.Forward(A, K), A.Forward(K)];
        yield return ["Forward (sequence, rational)", Expressions.Forward(SeqA, K), SeqA.ToExpression().Forward(K)];
        yield return ["Forward (sequence, rational expr)", Expressions.Forward(SeqA, Ke), SeqA.ToExpression().Forward(Ke)];

        // HorizontalShift
        yield return ["HorizontalShift (expr, rational expr)", Expressions.HorizontalShift(A, Ke), A.HorizontalShift(Ke)];
        yield return ["HorizontalShift (expr, rational)", Expressions.HorizontalShift(A, K), A.HorizontalShift(K)];
        yield return ["HorizontalShift (sequence, rational)", Expressions.HorizontalShift(SeqA, K), SeqA.ToExpression().HorizontalShift(K)];
        yield return ["HorizontalShift (sequence, rational expr)", Expressions.HorizontalShift(SeqA, Ke), SeqA.ToExpression().HorizontalShift(Ke)];

        // VerticalShift
        yield return ["VerticalShift (expr, rational expr)", Expressions.VerticalShift(A, Ke), A.VerticalShift(Ke)];
        yield return ["VerticalShift (expr, rational)", Expressions.VerticalShift(A, K), A.VerticalShift(K)];
        yield return ["VerticalShift (sequence, rational)", Expressions.VerticalShift(SeqA, K), SeqA.ToExpression().VerticalShift(K)];
        yield return ["VerticalShift (sequence, rational expr)", Expressions.VerticalShift(SeqA, Ke), SeqA.ToExpression().VerticalShift(Ke)];

        // Scale
        yield return ["Scale (expr, rational expr)", Expressions.Scale(A, Ke), A.Scale(Ke)];
        yield return ["Scale (expr, rational)", Expressions.Scale(A, K), A.Scale(K)];
        yield return ["Scale (sequence, rational)", Expressions.Scale(SeqA, K), SeqA.ToExpression().Scale(K)];
        yield return ["Scale (sequence, rational expr)", Expressions.Scale(SeqA, Ke), SeqA.ToExpression().Scale(Ke)];

        // Cut
        yield return ["Cut (curve expr)", Expressions.Cut(BetaE, CurveWindow), BetaE.Cut(CurveWindow)];
        yield return ["Cut (curve)", Expressions.Cut(Beta, CurveWindow), BetaE.Cut(CurveWindow)];
        yield return ["Cut (sequence expr)", Expressions.Cut(A, SeqWindow), A.Cut(SeqWindow)];
        yield return ["Cut (sequence)", Expressions.Cut(SeqA, SeqWindow), SeqA.ToExpression().Cut(SeqWindow)];

        // CutToNeighbourhood
        yield return ["CutToNeighbourhood (curve expr)", Expressions.CutToNeighbourhood(BetaE, 0, 5), BetaE.CutToNeighbourhood(0, 5)];
        yield return ["CutToNeighbourhood (curve)", Expressions.CutToNeighbourhood(Beta, 0, 5), BetaE.CutToNeighbourhood(0, 5)];
        yield return ["CutToNeighbourhood (sequence expr)", Expressions.CutToNeighbourhood(A, 1, 4), A.CutToNeighbourhood(1, 4)];
        yield return ["CutToNeighbourhood (sequence)", Expressions.CutToNeighbourhood(SeqA, 1, 4), SeqA.ToExpression().CutToNeighbourhood(1, 4)];
    }

    [Theory]
    [MemberData(nameof(SequenceCases))]
    public void SequenceOperatorOrFactoryAgreesWithTheInstanceMethod(string name, SequenceExpression built, SequenceExpression expected)
    {
        Assert.Equal(expected, built);
        Assert.True(Sequence.Equivalent(expected.Compute(), built.Compute()), name);
    }

    public static IEnumerable<object[]> RationalCases()
    {
        // ValueAt
        yield return ["ValueAt (expr, rational)", Expressions.ValueAt(A, K), A.ValueAt(K)];
        yield return ["ValueAt (expr, rational expr)", Expressions.ValueAt(A, Ke), A.ValueAt(Ke)];
        yield return ["ValueAt (sequence, rational)", Expressions.ValueAt(SeqA, K), SeqA.ToExpression().ValueAt(K)];
        yield return ["ValueAt (sequence, rational expr)", Expressions.ValueAt(SeqA, Ke), SeqA.ToExpression().ValueAt(Ke)];

        // LeftLimitAt
        yield return ["LeftLimitAt (expr, rational)", Expressions.LeftLimitAt(A, K), A.LeftLimitAt(K)];
        yield return ["LeftLimitAt (expr, rational expr)", Expressions.LeftLimitAt(A, Ke), A.LeftLimitAt(Ke)];
        yield return ["LeftLimitAt (sequence, rational)", Expressions.LeftLimitAt(SeqA, K), SeqA.ToExpression().LeftLimitAt(K)];
        yield return ["LeftLimitAt (sequence, rational expr)", Expressions.LeftLimitAt(SeqA, Ke), SeqA.ToExpression().LeftLimitAt(Ke)];

        // RightLimitAt
        yield return ["RightLimitAt (expr, rational)", Expressions.RightLimitAt(A, K), A.RightLimitAt(K)];
        yield return ["RightLimitAt (expr, rational expr)", Expressions.RightLimitAt(A, Ke), A.RightLimitAt(Ke)];
        yield return ["RightLimitAt (sequence, rational)", Expressions.RightLimitAt(SeqA, K), SeqA.ToExpression().RightLimitAt(K)];
        yield return ["RightLimitAt (sequence, rational expr)", Expressions.RightLimitAt(SeqA, Ke), SeqA.ToExpression().RightLimitAt(Ke)];
    }

    [Theory]
    [MemberData(nameof(RationalCases))]
    public void RationalOperatorOrFactoryAgreesWithTheInstanceMethod(string name, RationalExpression built, RationalExpression expected)
    {
        Assert.True(expected.Equals(built), name);
        Assert.True(expected.Compute() == built.Compute(), name);
    }
}
