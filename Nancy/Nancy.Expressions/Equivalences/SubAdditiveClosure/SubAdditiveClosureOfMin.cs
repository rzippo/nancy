namespace Unipi.Nancy.Expressions.Equivalences;

public class SubAdditiveClosureOfMin() : Equivalence(
    Expressions.SubAdditiveClosure(
        Expressions.Minimum(
            Expressions.Placeholder("f"),
            Expressions.Placeholder("g"))
    ),
    Expressions.Convolution(
        Expressions.SubAdditiveClosure(
            Expressions.Placeholder("f")),
        Expressions.SubAdditiveClosure(
            Expressions.Placeholder("g"))
    )
);