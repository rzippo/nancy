using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.PseudoInverse;


[TestSubject(typeof(Nancy.Expressions.Equivalences.PseudoInversesOfLeftContinuous))]
public class PseudoInverseOfLeftContinuous
{

    [Fact]
    public void ApplyEquivalence_PseudoInverseOfLeftContinuous()
    {
        var f = new RateLatencyServiceCurve(1, 2);

        var e = Expressions.LowerPseudoInverse(Expressions.UpperPseudoInverse(f));
        
        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.PseudoInversesOfLeftContinuous());
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "f";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString());
    }
}