using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

public class SubAdditiveClosureOfSubAdd : Equivalence
{
    public SubAdditiveClosureOfSubAdd() : base(
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
    }
}