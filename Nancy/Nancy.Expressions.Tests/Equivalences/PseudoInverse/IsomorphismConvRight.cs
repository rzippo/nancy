using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.PseudoInverse;


[TestSubject(typeof(Nancy.Expressions.Equivalences.IsomorphismConvRight))]
public class IsomorphismConvRight
{

    [Fact]
    public void ApplyEquivalence_IsomorphismConvRight()
    {
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(1, 2);

        var e = Expressions.LowerPseudoInverse(Expressions.MaxPlusConvolution(f, g));
        
        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.IsomorphismConvRight());
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "f↓⁻¹ ⊗ g↓⁻¹";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString());
    }
}