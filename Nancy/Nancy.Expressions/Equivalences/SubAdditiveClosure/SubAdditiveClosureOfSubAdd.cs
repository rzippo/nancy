using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

// todo: add reference
/// <summary>
/// If $f$ is subadditive and $f(0) = 0$, its subadditive closure is equal to $f$ itself, i.e. $\overline{f} = f$.
/// </summary>
public class SubAdditiveClosureOfSubAdd : Equivalence
{
    /// <summary>
    /// If $f$ is subadditive and $f(0) = 0$, its subadditive closure is equal to $f$ itself, i.e. $\overline{f} = f$.
    /// </summary>
    public SubAdditiveClosureOfSubAdd() : base(
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
    }
}