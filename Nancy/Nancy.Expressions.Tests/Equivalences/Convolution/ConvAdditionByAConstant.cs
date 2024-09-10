using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.ConvAdditionByAConstant))]
public class ConvAdditionByAConstant
{
    [Fact]
    public void ApplyEquivalence_ConvAdditionByAConstant()
    {
        var k = new ConstantCurve(1);
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(3, 4);

        var e = Expressions.Addition(
            Expressions.Convolution(f, g),
            k);

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.ConvAdditionByAConstant());
        
        Assert.True(k.IsUltimatelyConstant);
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        Assert.Equal("f \u2297 (g + k)", eq.ToUnicodeString());
    }
}