namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvolutionDistributivityMin : Equivalence
{
    public ConvolutionDistributivityMin() : base(Expressions.Convolution(
            Expressions.Placeholder("f"),
            Expressions.Minimum(
                Expressions.Placeholder("g"),
                Expressions.Placeholder("h"))
        ),
        Expressions.Minimum(
            Expressions.Convolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.Convolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("h"))
        ))
    {
    }
}