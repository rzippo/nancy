namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Lemma 4.12 PhD Thesis Zippo
/// </summary>
public class PseudoInversesOfLeftContinuous : Equivalence
{
    public PseudoInversesOfLeftContinuous() :
        base(Expressions.LowerPseudoInverse(Expressions.UpperPseudoInverse(Expressions.Placeholder("f"))),
            Expressions.Placeholder("f"))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsLeftContinuous);
    }
}