namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is non-decreasing and left-continuous, the lower pseudoinverse of its upper pseudoinverse is equal to $f$ again.
/// I.e., $(f^{\overline{-1}})^{\underline{-1}} = f$.
/// </summary>
/// <remarks>
/// Lemma 4.12 in [Zippo23].
/// </remarks>
public class PseudoInversesOfLeftContinuous : Equivalence
{
    /// <inheritdoc cref="PseudoInversesOfLeftContinuous"/>
    public PseudoInversesOfLeftContinuous() :
        base(Expressions.LowerPseudoInverse(Expressions.UpperPseudoInverse(Expressions.Placeholder("f"))),
            Expressions.Placeholder("f"))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsLeftContinuous);
    }
}