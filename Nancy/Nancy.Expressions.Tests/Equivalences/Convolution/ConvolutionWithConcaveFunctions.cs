﻿using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.ConvolutionWithConcaveFunctions))]
public class ConvolutionWithConcaveFunctions
{
    [Fact]
    public void ApplyEquivalence_ConvolutionWithConcaveFunctions_Test()
    {
        Curve a = new SigmaRhoArrivalCurve(1, 2);
        Curve b = new SigmaRhoArrivalCurve(2, 4);
        Curve c = new RateLatencyServiceCurve(3, 6);

        var equivalence = new Nancy.Expressions.Equivalences.ConvolutionWithConcaveFunctions();
        var e1 = Expressions.Convolution(a, b);
        var e2 = Expressions.Convolution(a, c);
        var e3 = e1.Convolution(c);
        var eq1 = e1.ApplyEquivalence(equivalence);
        var eq2 = e2.ApplyEquivalence(equivalence);
        var eq3 = e3.ApplyEquivalence(equivalence);

        Assert.False(e1 == eq1);
        Assert.True(e2 == eq2);
        Assert.False(e3 == eq3);
        Assert.True(e1.Equivalent(eq1));
        Assert.True(e3.Equivalent(eq3));
        Assert.Equal("a \u2227 b", eq1.ToUnicodeString());
        Assert.Equal("c \u2297 (a \u2227 b)", eq3.ToUnicodeString());
    }
}