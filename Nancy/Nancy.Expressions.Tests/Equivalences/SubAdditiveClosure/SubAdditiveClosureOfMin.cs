using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.SubAdditiveClosure;

[TestSubject(typeof(Nancy.Expressions.Equivalences.SubAdditiveClosureOfMin))]
public class SubAdditiveClosureOfMin
{

    [Fact]
    public void ApplyEquivalence_SubAdditiveClosureOfMin()
    {
        Curve a = new RateLatencyServiceCurve(1, 2);
        Curve b = new RateLatencyServiceCurve(2, 4);

        var e = Expressions.SubAdditiveClosure(
            Expressions.Minimum(a,b));

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.SubAdditiveClosureOfMin());
        
        Assert.False(e == eq);
        var expected = "subadditiveClosure(a) ⊗ subadditiveClosure(b)";
var regex = $"\\(?{Regex.Escape(expected)}\\)?";
Assert.Matches(regex, eq.ToUnicodeString());
        Assert.True(e.Equivalent(eq));
    }
}