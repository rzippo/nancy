namespace Unipi.Nancy.Expressions.Equivalences;

public class IsomorphismConvRight : Equivalence // Theorem 4.18 PhD Thesis Zippo
{
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
