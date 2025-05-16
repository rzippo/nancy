using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is subadditive, $g(0) = 0$, and $f(t) \le g(t) \forall~t$, then $f \otimes g = f$
/// </summary>
/// <remarks>
/// Theorem 1 in [ZS23].
/// </remarks>
public class ConvolutionSubAdditiveWithDominance : Equivalence
{
    /// <inheritdoc cref="ConvolutionSubAdditiveWithDominance"/>
    public ConvolutionSubAdditiveWithDominance() :
        base(Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
            Expressions.Placeholder("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("g", g => g.IsZeroAtZero);
        AddHypothesis("f", "g", (CurveExpression f, CurveExpression g) => f <= g);
        AddHypothesis("f", "g", (f, g) => f.Convolution(g).IsWellDefined);
    }
}