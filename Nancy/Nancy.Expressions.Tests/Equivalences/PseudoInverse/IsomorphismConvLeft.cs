using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.PseudoInverse;


[TestSubject(typeof(Nancy.Expressions.Equivalences.IsomorphismConvLeft))]
public class IsomorphismConvLeft
{

    [Fact]
    public void ApplyEquivalence_IsomorphismConvLeft()
    {
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(1, 2);

        var e = Expressions.UpperPseudoInverse(Expressions.Convolution(f, g));
        
        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.IsomorphismConvLeft());
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "f↑⁻¹ ⊗ g↑⁻¹";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString());
    }
}