namespace Unipi.Nancy.Expressions.Equivalences;

// todo: add reference
/// <summary>
/// The subadditive clusore of the minimum of two function is equal to the convolution between their subadditive closures, i.e., $\overline{f \wedge g} = \overline{f} \otimes \overline{g}$.
/// </summary>
public class SubAdditiveClosureOfMin : Equivalence
{
    /// <inheritdoc cref="SubAdditiveClosureOfMin"/>
    public SubAdditiveClosureOfMin() : base(Expressions.SubAdditiveClosure(
            Expressions.Minimum(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g"))
        ),
        Expressions.Convolution(
            Expressions.SubAdditiveClosure(
                Expressions.Placeholder("f")),
            Expressions.SubAdditiveClosure(
                Expressions.Placeholder("g"))
        ))
    {
    }
}