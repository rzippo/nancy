namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// $(f \otimes g) + K = (f \otimes (g + K))$.
/// </summary>
/// <remarks>
/// Proposition 2.1, point 4, in [DNC18].
/// </remarks>
public class ConvAdditionByAConstant : Equivalence
{
    /// <inheritdoc cref="ConvAdditionByAConstant"/>
    public ConvAdditionByAConstant() : base(
        Expressions.VerticalShift(
            Expressions.Convolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.RationalPlaceholder("K")
        ),
        Expressions.Convolution(
            Expressions.Placeholder("f"),
            Expressions.VerticalShift(
                Expressions.Placeholder("g"),
                Expressions.RationalPlaceholder("K")
            )
        )
    )
    {
    }
}