namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvSubAdditiveAsSelfConvMinimum : Equivalence
{
    public ConvSubAdditiveAsSelfConvMinimum() : base(
        Expressions.Convolution(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
        Expressions.Convolution(
            Expressions.Minimum(Expressions.Placeholder("f"), Expressions.Placeholder("g")),
            Expressions.Minimum(Expressions.Placeholder("f"), Expressions.Placeholder("g"))))
    {
        AddHypothesis("f", f => f.IsSubAdditive);
        AddHypothesis("g", g => g.IsSubAdditive);
        AddHypothesis("f", f => f.IsZeroAtZero);
        AddHypothesis("g", g => g.IsZeroAtZero);
        AddHypothesis("f", "g", (f, g) => f.Convolution(g).IsWellDefined);
    }
}