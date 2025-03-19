namespace Unipi.Nancy.Expressions.Equivalences;

public class DeconvolutionWeakCommutativity : Equivalence
{
    public DeconvolutionWeakCommutativity() : base(Expressions.Deconvolution(
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("h")),
            Expressions.Placeholder("g")
        ),
        Expressions.Deconvolution(
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.Placeholder("h")
        ))
    {
    }
}