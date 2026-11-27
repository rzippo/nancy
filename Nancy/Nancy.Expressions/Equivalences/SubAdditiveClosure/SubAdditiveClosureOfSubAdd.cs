using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ is subadditive and $f(0) = 0$, its subadditive closure is equal to $f$ itself, i.e. $\overline{f} = f$.
/// </summary>
/// <remarks>
/// See [DNC18], Lemma 2.6.
/// </remarks>
public class SubAdditiveClosureOfSubAdd : Equivalence
{
    /// <inheritdoc cref="SubAdditiveClosureOfSubAdd"/>
    public SubAdditiveClosureOfSubAdd() : base(
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis<CurveExpression>("f", f => f.IsSubAdditive);
        AddHypothesis<CurveExpression>("f", f => f.IsPassingThroughOrigin);
    }
}