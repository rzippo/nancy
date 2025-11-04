namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The upper pseudoinverse of a (min,+) convolution of two non-decreasing left-continuous function is equal
/// to the (max,+) convolution of their upper pseudoinverses.
/// I.e., $(f \otimes g)^{\overline{-1}} = f^{\overline{-1}} \overline{\otimes} g^{\overline{-1}}$. 
/// </summary>
/// <remarks>
/// Theorem 4.16 in [Zippo23].
/// </remarks>
public class IsomorphismConvLeft : Equivalence
{
    /// <inheritdoc cref="IsomorphismConvLeft"/>
    public IsomorphismConvLeft() :
        base(Expressions.UpperPseudoInverse(
                Expressions.Convolution(
                    Expressions.Placeholder("f"),
                    Expressions.Placeholder("g"))),
            Expressions.Convolution(
                Expressions.UpperPseudoInverse(
                    Expressions.Placeholder("f")),
                Expressions.UpperPseudoInverse(
                    Expressions.Placeholder("g"))
            ))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsLeftContinuous);
        AddHypothesis("g", g => g.IsNonDecreasing);
        AddHypothesis("g", g => g.IsLeftContinuous);
    }
}