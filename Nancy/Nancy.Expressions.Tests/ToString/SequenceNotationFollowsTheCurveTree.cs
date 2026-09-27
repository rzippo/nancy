using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

/// <summary>
/// Sequence operations must render with the same notation as their curve or rational counterpart.
/// </summary>
public class SequenceNotationFollowsTheCurveTree
{
    private static readonly SequenceExpression A = Expressions.SequencePlaceholder("a");
    private static readonly SequenceExpression B = Expressions.SequencePlaceholder("b");
    private static readonly CurveExpression CurveA = Expressions.Placeholder("a");
    private static readonly CurveExpression CurveB = Expressions.Placeholder("b");
    private static readonly RationalExpression Two = Expressions.RationalPlaceholder("two");

    // Concrete leaves, since how a leaf is bracketed depends on its name.
    private static readonly SequenceExpression ConcreteA =
        Expressions.FromSequence(new Sequence([Point.Origin()]), "a");
    private static readonly SequenceExpression ConcreteF1 =
        Expressions.FromSequence(new Sequence([Point.Origin()]), "f_1");
    private static readonly CurveExpression ConcreteCurveA = Expressions.FromCurve(Curve.Zero(), "a");
    private static readonly CurveExpression ConcreteCurveF1 = Expressions.FromCurve(Curve.Zero(), "f_1");

    public record Case(IExpression Sequence, IExpression Curve, params (string From, string To)[] Substitutions);

    public static IEnumerable<object[]> Cases =>
    [
        new object[] { new Case(A.Negate(), CurveA.Negate()) },
        new object[] { new Case(A.ToNonNegative(), CurveA.ToNonNegative()) },
        new object[] { new Case(ConcreteA.ToNonNegative(), ConcreteCurveA.ToNonNegative()) },
        new object[] { new Case(ConcreteF1.ToNonNegative(), ConcreteCurveF1.ToNonNegative()) },
        new object[] { new Case(A.Floor(), CurveA.Floor()) },
        new object[] { new Case(A.Ceil(), CurveA.Ceil()) },
        new object[] { new Case(A.ToLeftContinuous(), CurveA.ToLeftContinuous()) },
        new object[] { new Case(A.ToRightContinuous(), CurveA.ToRightContinuous()) },
        new object[] { new Case(A.LowerPseudoInverse(), CurveA.LowerPseudoInverse()) },
        new object[] { new Case(A.UpperPseudoInverse(), CurveA.UpperPseudoInverse()) },

        new object[] { new Case(new SequenceAdditionExpression([A, B]), CurveA.Addition(CurveB)) },
        new object[] { new Case(new SequenceSubtractionExpression(A, B), CurveA.Subtraction(CurveB)) },
        new object[] { new Case(new SequenceMinimumExpression([A, B]), CurveA.Minimum(CurveB)) },
        new object[] { new Case(new SequenceMaximumExpression([A, B]), CurveA.Maximum(CurveB)) },
        new object[] { new Case(new SequenceConvolutionExpression([A, B]), CurveA.Convolution(CurveB)) },
        new object[] { new Case(new SequenceDeconvolutionExpression(A, B), CurveA.Deconvolution(CurveB)) },
        new object[] { new Case(new SequenceMaxPlusConvolutionExpression([A, B]), CurveA.MaxPlusConvolution(CurveB)) },
        new object[] { new Case(new SequenceMaxPlusDeconvolutionExpression(A, B), CurveA.MaxPlusDeconvolution(CurveB)) },
        new object[] { new Case(new SequenceCompositionExpression(A, B), CurveA.Composition(CurveB)) },

        new object[] { new Case(A.Delay(Two), CurveA.DelayBy(Two), ("delayBy", "delay")) },
        new object[] { new Case(A.Forward(Two), CurveA.ForwardBy(Two), ("forwardBy", "forward")) },
        new object[] { new Case(A.HorizontalShift(Two), CurveA.HorizontalShift(Two)) },
        new object[] { new Case(A.VerticalShift(Two), CurveA.VerticalShift(Two)) },
        new object[] { new Case(A.Scale(Two), CurveA.Scale(Two)) },

        new object[] { new Case(A.ValueAt(Two), CurveA.ValueAt(Two)) },
        new object[] { new Case(A.LeftLimitAt(Two), CurveA.LeftLimitAt(Two)) },
        new object[] { new Case(A.RightLimitAt(Two), CurveA.RightLimitAt(Two)) },

        new object[] { new Case(new SequenceHorizontalDeviationExpression(A, B), Expressions.HorizontalDeviation(CurveA, CurveB)) },
        new object[] { new Case(new SequenceVerticalDeviationExpression(A, B), Expressions.VerticalDeviation(CurveA, CurveB)) },
    ];

    [Theory]
    [MemberData(nameof(Cases))]
    public void UnicodeFollowsTheCurveTree(Case testCase)
    {
        var expected = ApplySubstitutions(testCase.Curve.ToUnicodeString(), testCase.Substitutions);
        Assert.Equal(expected, testCase.Sequence.ToUnicodeString());
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void LatexFollowsTheCurveTree(Case testCase)
    {
        var expected = ApplySubstitutions(testCase.Curve.ToLatexString(), testCase.Substitutions);
        Assert.Equal(expected, testCase.Sequence.ToLatexString());
    }

    /// <summary>
    /// The curve renderings the sequence tree follows, written out, since comparing the two trees cannot notice a change both share.
    /// </summary>
    public static IEnumerable<object[]> CurveRenderings()
    {
        var a = Expressions.FromCurve(Curve.Zero(), "a");
        var b = Expressions.FromCurve(Curve.Zero(), "b");
        var f1 = Expressions.FromCurve(Curve.Zero(), "f_1");
        var two = Expressions.FromRational(2, "two");
        yield return [a.ToNonNegative(), "[a]⁺", "a^{+}"];
        yield return [f1.ToNonNegative(), "f_1⁺", "f__{{1}}^{+}"];
        yield return [a.LowerPseudoInverse(), "a↓⁻¹", @"a^{\underline{-1}}"];
        yield return [a.UpperPseudoInverse(), "a↑⁻¹", @"a^{\overline{-1}}"];
        yield return [a.Floor(), "⌊a⌋", @"\lfloor a \rfloor"];
        yield return [a.Ceil(), "⌈a⌉", @"\lceil a \rceil"];
        yield return [a.Negate(), "-(a)", @"-\left( a \right)"];
        yield return [a.ToLeftContinuous(), "toLeftContinuous(a)", "a_{l}"];
        yield return [a.ToRightContinuous(), "toRightContinuous(a)", "a_{r}"];
        yield return [a.ValueAt(two), "a(2)", @"a\left(2\right)"];
        yield return [a.LeftLimitAt(two), "a(2⁻)", @"a\left(2^{-}\right)"];
        yield return [a.RightLimitAt(two), "a(2⁺)", @"a\left(2^{+}\right)"];
        yield return [a.LeftLimitAt(two.Addition(two)), "a((2 + 2)⁻)", @"a\left(\left(2 + 2\right)^{-}\right)"];
        yield return [a.DelayBy(two), "delayBy(a, 2)", @"delayBy\left( a, 2 \right)"];
        yield return [a.Scale(two), "a·2", @"a \cdot 2"];
        yield return [Expressions.HorizontalDeviation(a, b), "hdev(a, b)", @"hdev\left( a, b \right)"];
    }

    [Theory]
    [MemberData(nameof(CurveRenderings))]
    public void TheCurveTreeKeepsItsNotation(IExpression expression, string unicode, string latex)
    {
        Assert.Equal(unicode, expression.ToUnicodeString());
        Assert.Equal(latex, expression.ToLatexString());
    }

    private static string ApplySubstitutions(string value, (string From, string To)[] substitutions)
    {
        return substitutions.Aggregate(value, (current, substitution) => current.Replace(substitution.From, substitution.To));
    }
}
