using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Deconvolution;


[TestSubject(typeof(Nancy.Expressions.Equivalences.DeconvAndSubAdditiveClosure))]
public class DeconvAndSubAdditiveClosure
{

    [Fact]
    public void ApplyEquivalence_DeconvAndSubadditiveClosure()
    {
        Curve a = new RateLatencyServiceCurve(1, 2);
        Curve b = new RateLatencyServiceCurve(2, 4);
        
        var e = Expressions.Deconvolution(
            Expressions.Convolution(a,
                Expressions.SubAdditiveClosure(b)),
            Expressions.SubAdditiveClosure(b)
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.DeconvAndSubAdditiveClosure());
        
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        Assert.Equal("a ⊗ subadditiveClosure(b)", eq.ToUnicodeString());
    }
}