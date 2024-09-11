namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvAdditionByAConstant : Equivalence
{
    public ConvAdditionByAConstant() : base(
        Expressions.Addition(
            Expressions.Convolution(
                Expressions.Placeholder("f"),
                Expressions.Placeholder("g")),
            Expressions.Placeholder("K")
        ),
        Expressions.Convolution(
            Expressions.Placeholder("f"),
            Expressions.Addition(
                Expressions.Placeholder("g"),
                Expressions.Placeholder("K")
            )
        )
    )
    {
        AddHypothesis("K", curveExpression => curveExpression.IsUltimatelyConstant());
    }
}