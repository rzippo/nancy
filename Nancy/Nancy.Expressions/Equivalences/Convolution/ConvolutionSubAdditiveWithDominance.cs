using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvolutionSubAdditiveWithDominance : Equivalence
{
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