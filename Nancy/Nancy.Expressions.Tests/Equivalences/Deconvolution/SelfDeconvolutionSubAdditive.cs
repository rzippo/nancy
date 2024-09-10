using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Deconvolution;


[TestSubject(typeof(Nancy.Expressions.Equivalences.SelfDeconvolutionSubAdditive))]
public class SelfDeconvolutionSubAdditive
{

    [Fact]
    public void ApplyEquivalence_SelfDeconvolutionSubAdditive()
    {
        var f = new RateLatencyServiceCurve(1, 2).SubAdditiveClosure();

        var e = Expressions.Deconvolution(f,f);
        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.SelfDeconvolutionSubAdditive());
        
        Assert.False(e == eq);
        Assert.True(e.Equivalent(eq));
        Assert.Equal("f", eq.ToUnicodeString());
    }
}