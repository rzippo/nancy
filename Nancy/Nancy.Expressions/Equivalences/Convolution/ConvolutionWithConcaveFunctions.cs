namespace Unipi.Nancy.Expressions.Equivalences;

// todo: add reference
/// <summary>
/// If $f$ and $g$ are concave and 0 at 0, then $f \otimes g = f \wedge g$.
/// </summary>
public class ConvolutionWithConcaveFunctions : Equivalence
{
    /// <inheritdoc cref="ConvolutionWithConcaveFunctions"/>
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