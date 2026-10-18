using System.Collections.Generic;
using Eq = Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.PseudoInverse;

/// <summary>
/// The equivalences on pseudo-inverses keep the value of an expression whose operands are negative at first.
/// </summary>
public class OperandsNegativeAtFirst
{
    // Non-decreasing and continuous, so it meets every other hypothesis of the four equivalences, and -1 at 0.
    private static readonly Curve NegativeAtFirst = new RateLatencyServiceCurve(1, 2).VerticalShift(-1, false);

    private static readonly Curve Constant = new ConstantCurve(2);

    public static IEnumerable<object[]> Cases()
    {
        yield return [new Eq.PseudoInversesOfLeftContinuous(),
            Expressions.LowerPseudoInverse(Expressions.UpperPseudoInverse(NegativeAtFirst))];
        yield return [new Eq.PseudoInversesOfRightContinuous(),
            Expressions.UpperPseudoInverse(Expressions.LowerPseudoInverse(NegativeAtFirst))];
        yield return [new Eq.IsomorphismConvLeft(),
            Expressions.UpperPseudoInverse(Expressions.Convolution(NegativeAtFirst, Constant))];
        yield return [new Eq.IsomorphismConvLeft(),
            Expressions.UpperPseudoInverse(Expressions.Convolution(Constant, NegativeAtFirst))];
        yield return [new Eq.IsomorphismConvRight(),
            Expressions.LowerPseudoInverse(Expressions.MaxPlusConvolution(NegativeAtFirst, NegativeAtFirst))];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void TheRewriteKeepsTheValue(Eq.Equivalence equivalence, CurveExpression expression)
    {
        var rewritten = expression.ApplyEquivalence(equivalence);
        Assert.True(expression.Equivalent(rewritten));
    }
}
