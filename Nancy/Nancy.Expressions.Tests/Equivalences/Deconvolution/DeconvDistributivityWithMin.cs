using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Deconvolution;


[TestSubject(typeof(Nancy.Expressions.Equivalences.DeconvDistributivityWithMin))]
public class DeconvDistributivityWithMin
{

    [Fact]
    public void ApplyEquivalence_DeconvDistributivityWithMin()
    {
        Curve f = new RateLatencyServiceCurve(1, 20);
        Curve g = new RateLatencyServiceCurve(2, 4);
        Curve h = new RateLatencyServiceCurve(3, 5);

        var e = Expressions.Deconvolution(
            f,
            Expressions.Minimum(g,h)
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.DeconvDistributivityWithMin());
        
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "(f ⊘ g) ∨ (f ⊘ h)";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString());
    }
}