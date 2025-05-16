namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $f \oslash (g \wedge h) = (f \oslash g) \vee (f \oslash h)$.
/// </summary>
/// <remarks>
/// Proposition 2.8, point 2, in [DNC18].
/// </remarks>
public class DeconvDistributivityWithMin : Equivalence
{
    /// <inheritdoc cref="DeconvDistributivityWithMin"/>
    public DeconvDistributivityWithMin() : base(Expressions.Deconvolution(
            Expressions.Placeholder("f"),
            Expressions.Minimum(
                Expressions.Placeholder("g"),
                Expressions.Placeholder("h"))
        ),
        Expressions.Maximum(
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.Deconvolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("h"))
        ))
    {
    }
}