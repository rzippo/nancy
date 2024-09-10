using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

public class SelfConvolutionSubAdditive : Equivalence
{
    public SelfConvolutionSubAdditive() : base(
        Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("f")),
        new CurvePlaceholderExpression("f"))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
    }
}