namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The lower pseudoinverse of a (max,+) convolution of two non-decreasing right-continuous function is equal
/// to the (min,+) convolution of their lower pseudoinverses.
/// I.e., $(f \overline{\otimes} g)^{\underline{-1}} = f^\underline{-1} \otimes g^\underline{-1}$. 
/// </summary>
/// <remarks>
/// Theorem 4.18 in [Zippo23].
/// </remarks>
public class IsomorphismConvRight : Equivalence
{
    /// <inheritdoc cref="IsomorphismConvRight"/>
    public IsomorphismConvRight() :
        base(Expressions.LowerPseudoInverse(
                Expressions.MaxPlusConvolution(
                    Expressions.Placeholder("f"),
                    Expressions.Placeholder("g"))),
            Expressions.Convolution(
                Expressions.LowerPseudoInverse(
                    Expressions.Placeholder("f")),
                Expressions.LowerPseudoInverse(
                    Expressions.Placeholder("g"))
            ))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsRightContinuous);
        AddHypothesis("g", g => g.IsNonDecreasing);
        AddHypothesis("g", g => g.IsRightContinuous);
    }
}
