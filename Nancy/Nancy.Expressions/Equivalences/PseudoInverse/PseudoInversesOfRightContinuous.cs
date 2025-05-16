namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is non-decreasing and right-continuous, the upper pseudoinverse of its lower pseudoinverse is equal to $f$ again.
/// I.e., $(f^{\underline{-1}})^{\overline{-1}} = f$.
/// </summary>
/// <remarks>
/// Lemma 4.13 in [Zippo23].
/// </remarks>
public class PseudoInversesOfRightContinuous : Equivalence 
{
    /// <inheritdoc cref="PseudoInversesOfRightContinuous"/>
    public PseudoInversesOfRightContinuous() :
        base(Expressions.UpperPseudoInverse(Expressions.LowerPseudoInverse(Expressions.Placeholder("f"))),
            Expressions.Placeholder("f"))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsRightContinuous);
    }
}