namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvAdditionByAConstant : Equivalence
{
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