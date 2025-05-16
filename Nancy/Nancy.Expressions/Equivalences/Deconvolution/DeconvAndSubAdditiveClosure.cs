namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $(g \otimes \overline{f}) \oslash \overline{f} = g \otimes \overline{f}$.
/// </summary>
/// <remarks>
/// Proposition 2.9, point 2, in [DNC18].
/// </remarks>
public class DeconvAndSubAdditiveClosure : Equivalence
{
    /// <inheritdoc cref="DeconvAndSubAdditiveClosure"/>
    public DeconvAndSubAdditiveClosure() : base(Expressions.Deconvolution(
            Expressions.Convolution(
                Expressions.Placeholder("g"),
                Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
        ),
        Expressions.Convolution(
            Expressions.Placeholder("g"),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
        ))
    {
    }
}