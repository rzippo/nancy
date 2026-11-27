using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is subadditive and $f(0) = 0$, its self-convolution is equal to $f$ itself, i.e. $f \otimes f = f$.
/// </summary>
/// <remarks>
/// This follows directly from [DNC18], Definitions 2.7 and 2.10, with equality at the convolution endpoint since $f(0) = 0$.
/// </remarks>
public class SelfConvolutionSubAdditive : Equivalence
{
    /// <inheritdoc cref="SelfConvolutionSubAdditive"/>
    public SelfConvolutionSubAdditive() : base(
        Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis<CurveExpression>("f", f => f.IsSubAdditive);
        AddHypothesis<CurveExpression>("f", f => f.IsPassingThroughOrigin);
    }
}
