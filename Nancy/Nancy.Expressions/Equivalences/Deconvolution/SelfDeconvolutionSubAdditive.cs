using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

public class SelfDeconvolutionSubAdditive : Equivalence
{
    public SelfDeconvolutionSubAdditive() : base(
        Expressions.Deconvolution(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
    }
}