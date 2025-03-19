namespace Unipi.Nancy.Expressions.Equivalences;

public class DeconvolutionWithConvolution : Equivalence
{
    public DeconvolutionWithConvolution() : base(Expressions.Deconvolution(
            Expressions.Placeholder("f"),
            Expressions.Convolution(
                Expressions.Placeholder("g"),
                Expressions.Placeholder("h"))
        ),
        Expressions.Deconvolution(
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("h")),
            Expressions.Placeholder("g")
        ))
    {
    }
}