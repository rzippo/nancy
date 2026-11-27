using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is subadditive and $f(0) = 0$, its self-deconvolution is equal to $f$ itself, i.e. $f \oslash f = f$.
/// </summary>
/// <remarks>
/// See [DNC18], Lemma 2.6 and Proposition 2.9(1).
/// </remarks>
public class SelfDeconvolutionSubAdditive : Equivalence
{
    /// <inheritdoc cref="SelfDeconvolutionSubAdditive"/>
    public SelfDeconvolutionSubAdditive() : base(
        Expressions.Deconvolution(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis<CurveExpression>("f", f => f.IsSubAdditive);
        AddHypothesis<CurveExpression>("f", f => f.IsPassingThroughOrigin);
    }
}