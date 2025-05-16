namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ and $g$ are subadditive and 0 at 0, then $f \otimes g = (f \wedge g) \otimes (f \wedge g)$.
/// </summary>
/// <remarks>
/// Theorem 3 in [ZS23].
/// </remarks>
public class ConvSubAdditiveAsSelfConvMinimum : Equivalence
{
    /// <inheritdoc cref="ConvSubAdditiveAsSelfConvMinimum"/>
    public ConvSubAdditiveAsSelfConvMinimum() : base(
        Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
        Expressions.Convolution(
            Expressions.Minimum(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
            Expressions.Minimum(Expressions.Placeholder("f"), Expressions.Placeholder("g"))))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("g", g => g.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
        AddHypothesis("g", g => g.IsZeroAtZero);
        AddHypothesis("f", "g", (f, g) => f.Convolution(g).IsWellDefined);
    }
}