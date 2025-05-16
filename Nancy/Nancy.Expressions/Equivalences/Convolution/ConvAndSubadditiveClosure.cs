namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $(g \oslash \overline{f}) \otimes \overline{f} = g \oslash \overline{f}$.
/// </summary>
/// <remarks>
/// Proposition 2.9, point 3, in [DNC18].
/// </remarks>
public class ConvAndSubadditiveClosure : Equivalence
{
    /// <inheritdoc cref="ConvAndSubadditiveClosure"/>
    public ConvAndSubadditiveClosure() : base(Expressions.Convolution(
            Expressions.Deconvolution(
                Expressions.Placeholder("g"),
                Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
        ),
        Expressions.Deconvolution(
            Expressions.Placeholder("g"),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
        ))
    {
    }
}