using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.ConvAndSubadditiveClosure))]
public class ConvAndSubadditiveClosure
{
    [Fact]
    public void ApplyEquivalence_ConvAndSubadditiveClosure()
    {
        Curve a = new RateLatencyServiceCurve(1, 2);
        Curve b = new RateLatencyServiceCurve(2, 4);

        var e = Expressions.Convolution(
            Expressions.Deconvolution(
                a,
                Expressions.SubAdditiveClosure(b)),
            Expressions.SubAdditiveClosure(b)
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.ConvAndSubadditiveClosure());
        
        Assert.False(e == eq);
        Assert.True(e.Equivalent(eq));
        Assert.Equal("a ⊘ subadditiveClosure{b}", eq.ToUnicodeString());
    }
}