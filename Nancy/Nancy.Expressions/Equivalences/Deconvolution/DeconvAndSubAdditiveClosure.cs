namespace Unipi.Nancy.Expressions.Equivalences;

public class DeconvAndSubAdditiveClosure() : Equivalence(
    Expressions.Deconvolution(
        Expressions.Convolution(
            Expressions.Placeholder("g"),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))),
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
    ),
    Expressions.Convolution(
        Expressions.Placeholder("g"),
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
    )
);