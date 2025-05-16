namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $(f \vee g) \oslash h = (f \oslash h) \vee (g \oslash h)$.
/// </summary>
/// <remarks>
/// Proposition 2.8, point 1, in [DNC18].
/// 
/// In the book there is a typo: the right side of the equivalence is wrong both in the
/// statement and the end of the proof (although the steps in the proof are correct).
/// The correct right side of the equivalence is $(f \oslash h) \vee (g \oslash h)$.
/// </remarks>
public class DeconvDistributivityWithMax : Equivalence
{
    /// <inheritdoc cref="DeconvDistributivityWithMax"/>
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