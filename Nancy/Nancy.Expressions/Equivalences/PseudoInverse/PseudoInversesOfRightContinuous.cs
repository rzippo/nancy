namespace Unipi.Nancy.Expressions.Equivalences;

public class PseudoInversesOfRightContinuous : Equivalence // Lemma 4.13 PhD Thesis Zippo
{
    public PseudoInversesOfRightContinuous() :
        base(Expressions.UpperPseudoInverse(Expressions.LowerPseudoInverse(Expressions.Placeholder("f"))),
            Expressions.Placeholder("f"))
    {
        AddHypothesis("f", f => f.IsNonDecreasing);
        AddHypothesis("f", f => f.IsRightContinuous);
    }
}