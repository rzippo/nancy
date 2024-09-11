using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Deconvolution;


[TestSubject(typeof(Nancy.Expressions.Equivalences.DeconvolutionWeakCommutativity))]
public class DeconvolutionWeakCommutativity
{

    [Fact]
    public void ApplyEquivalence_DeconvolutionWeakCommutativity()
    {
        Curve f = new RateLatencyServiceCurve(1, 2);
        Curve g = new RateLatencyServiceCurve(2, 4);
        Curve h = new RateLatencyServiceCurve(3, 5);

        var e = Expressions.Deconvolution(
            Expressions.Deconvolution(f, h),
            g
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.DeconvolutionWeakCommutativity());
        
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        Assert.Equal("(f ⊘ g) ⊘ h", eq.ToUnicodeString());
    }
}