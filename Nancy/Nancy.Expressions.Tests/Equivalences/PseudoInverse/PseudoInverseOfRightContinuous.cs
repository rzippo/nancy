using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.PseudoInverse;


[TestSubject(typeof(Nancy.Expressions.Equivalences.PseudoInverseOfRightContinuous))]
public class PseudoInverseOfRightContinuous
{

    [Fact]
    public void ApplyEquivalence_PseudoInverseOfRightContinuous()
    {
        var f = new RateLatencyServiceCurve(1, 2);

        var e = Expressions.UpperPseudoInverse(
            Expressions.LowerPseudoInverse(f));

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.PseudoInverseOfRightContinuous());
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        Assert.Equal("f", eq.ToUnicodeString());
    }
}