namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $f \otimes (g \wedge h) = (f \otimes g) \wedge (f \otimes h)$.
/// </summary>
/// <remarks>
/// Lemma 2.1, point 3, in [DNC18].
/// </remarks>
public class ConvolutionDistributivityMin : Equivalence
{
    /// <inheritdoc cref="ConvolutionDistributivityMin"/>
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