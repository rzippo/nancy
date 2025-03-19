namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Equivalence defined in Prop. 2.8 in "Deterministic Network Calculus - From Theory to Practical Implementation",
/// Bouillard, Boyer, Le Corronc. In the book there is a typo: the right side of the equivalence is wrong both in the
/// statement and the end of the proof (although the steps in the proof are correct). The correct right side of the
/// equivalence is $(f \oslash h) \vee (g \oslash h)$.
/// </summary>
public class DeconvDistributivityWithMax : Equivalence
{
    public DeconvDistributivityWithMax() : base(Expressions.Deconvolution(
            Expressions.Maximum(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.Placeholder("h")
        ),
        Expressions.Maximum(
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("h")),
            Expressions.Deconvolution(
                Expressions.Placeholder("g"),
                Expressions.Placeholder("h"))
        ))
    {
    }
}