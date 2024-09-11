﻿namespace Unipi.Nancy.Expressions.Equivalences;

public class ConvAndSubadditiveClosure() : Equivalence(
    Expressions.Convolution(
        Expressions.Deconvolution(
            Expressions.Placeholder("g"),
            Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))),
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
    ),
    Expressions.Deconvolution(
        Expressions.Placeholder("g"),
        Expressions.SubAdditiveClosure(Expressions.Placeholder("f"))
    )
);