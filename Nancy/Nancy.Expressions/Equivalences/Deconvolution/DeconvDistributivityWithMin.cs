namespace Unipi.Nancy.Expressions.Equivalences;

public class DeconvDistributivityWithMin() : Equivalence(
    Expressions.Deconvolution(
        Expressions.Placeholder("f"),
        Expressions.Minimum(
            Expressions.Placeholder("g"),
            Expressions.Placeholder("h"))
    ),
    Expressions.Maximum(
        Expressions.Deconvolution(
            Expressions.Placeholder("f"),
            Expressions.Placeholder("g")),
        Expressions.Deconvolution(
            Expressions.Placeholder("f"),
            Expressions.Placeholder("h"))
    )
);