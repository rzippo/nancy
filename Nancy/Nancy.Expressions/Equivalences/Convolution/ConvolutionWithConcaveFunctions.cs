namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// If $f$ and $g$ are concave and 0 at 0, then $f \otimes g = f \wedge g$.
/// </summary>
/// <remarks>
/// See [DNC18], Proposition 3.12(4).
/// </remarks>
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
        AddHypothesis<CurveExpression>("f", f => f.IsConcave);
        AddHypothesis<CurveExpression>("g", g => g.IsConcave);
        AddHypothesis<CurveExpression>("f", f => f.IsPassingThroughOrigin);
        AddHypothesis<CurveExpression>("g", g => g.IsPassingThroughOrigin);
    }
}