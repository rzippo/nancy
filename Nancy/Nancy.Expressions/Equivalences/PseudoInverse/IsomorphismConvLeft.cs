namespace Unipi.Nancy.Expressions.Equivalences;

public class IsomorphismConvLeft : Equivalence // Theorem 4.16 PhD Thesis Zippo
{
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