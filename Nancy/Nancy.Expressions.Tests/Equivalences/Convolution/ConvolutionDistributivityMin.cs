using JetBrains.Annotations;
using Xunit;using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.ConvolutionDistributivityMin))]
public class ConvolutionDistributivityMin
{
    [Fact]
    public void ApplyEquivalence_ConvolutionDistributivityMin()
    {
        Curve a = new RateLatencyServiceCurve(1, 2).SubAdditiveClosure();
        Curve b = new RateLatencyServiceCurve(2, 4).SubAdditiveClosure();
        Curve c = new RateLatencyServiceCurve(4, 6).SubAdditiveClosure();

        var e = Expressions.Convolution(a,
            Expressions.Minimum(b, c));

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.ConvolutionDistributivityMin());
        var eq2 = eq.ApplyEquivalence(new Nancy.Expressions.Equivalences.ConvolutionDistributivityMin(), CheckType.CheckRightOnly);
        
        Assert.False(e == eq);
        Assert.True(e.Equivalent(eq));
        Assert.Equal("(a \u2297 b) \u2227 (a \u2297 c)", eq.ToUnicodeString());
        Assert.Equal(e.ToUnicodeString(), eq2.ToUnicodeString());
    }
}