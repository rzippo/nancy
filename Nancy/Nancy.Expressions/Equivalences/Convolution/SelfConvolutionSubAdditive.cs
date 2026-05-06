using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

// todo: add reference
/// <summary>
/// If $f$ is subadditive and $f(0) = 0$, its self-convolution is equal to $f$ itself, i.e. $f \otimes f = f$.
/// </summary>
public class SelfConvolutionSubAdditive : Equivalence
{
    /// <inheritdoc cref="SelfConvolutionSubAdditive"/>
    public SelfConvolutionSubAdditive() : base(
        Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
    }
}
