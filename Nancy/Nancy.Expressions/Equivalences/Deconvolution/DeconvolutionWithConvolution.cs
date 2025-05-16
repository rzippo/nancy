namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $f \oslash (g \otimes h) = (f \oslash h) \oslash g$.
/// </summary>
/// <remarks>
/// Proposition 2.7, point 8, [DNC18].
/// </remarks>
public class DeconvolutionWithConvolution : Equivalence
{
    /// <inheritdoc cref="DeconvolutionWithConvolution"/>
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