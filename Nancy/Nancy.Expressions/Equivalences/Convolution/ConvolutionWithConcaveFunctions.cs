namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvolutionWithConcaveFunctions : Equivalence
{
    public ConvolutionWithConcaveFunctions() : base(
        Expressions.Convolution(
            Expressions.Placeholder("f"), 
            Expressions.Placeholder("g")),
        Expressions.Minimum(
                Expressions.Placeholder("f"), 
                Expressions.Placeholder("g")))
    {
        AddHypothesis("f", f => f.IsConcave);
        AddHypothesis("g", g => g.IsConcave);
        AddHypothesis("f", f => f.IsZeroAtZero);
        AddHypothesis("g", g => g.IsZeroAtZero);
    }
}