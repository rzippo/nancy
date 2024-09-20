using System;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.Equivalences;

[TestSubject(typeof(Nancy.Expressions.Equivalences.Equivalence))]
public class Equivalence
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Equivalence(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [EmbeddedResourceData("Unipi.Nancy.Expressions.Tests/Equivalences/equivalences.eq")]
    public void ReadEquivalences_AntlrGrammar(string equivalenceCatalog)
    {
        var list = Nancy.Expressions.Equivalences.Equivalence.ReadEquivalences(equivalenceCatalog);

        Curve a = new RateLatencyServiceCurve(1, 2).SubAdditiveClosure();
        Curve b = new RateLatencyServiceCurve(2, 4).SubAdditiveClosure();

        var expression = Expressions.Convolution(a, b);
        var eq = expression.ApplyEquivalence(list.ToArray()[0]);
        Assert.Equal("a", eq.ToUnicodeString());
        Assert.True(expression.Equivalent(eq));
        Assert.False(expression == eq);

        var beta1 = new RateLatencyServiceCurve(10,20);
        var beta2 = new RateLatencyServiceCurve(3,4);

        var sacBeta1 = Expressions.SubAdditiveClosure(beta1);
        var expr = Expressions.Convolution(sacBeta1, beta2);

        _testOutputHelper.WriteLine("Original Expression: " + expr);
        _testOutputHelper.WriteLine("Equivalent Expression: " + expr.ApplyEquivalence(list.ToArray()[1]));
        _testOutputHelper.WriteLine("Test Expression: " + expr.ApplyEquivalence(list.ToArray()[0]));
    }
}