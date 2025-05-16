namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $(f \oslash h) \oslash g = (f \oslash g) \oslash h$.
/// </summary>
/// <remarks>
/// Proposition 2.7, point 8, [DNC18].
/// </remarks>
public class DeconvolutionWeakCommutativity : Equivalence
{
    /// <inheritdoc cref="DeconvolutionWeakCommutativity"/>
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