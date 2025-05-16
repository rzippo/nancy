using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Unipi.Nancy.Expressions.Internals;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Equivalences.Convolution;

[TestSubject(typeof(Nancy.Expressions.Equivalences.ConvAdditionByAConstant))]
public class ConvAdditionByAConstant
{
    [Fact]
    public void ApplyEquivalence_ConvAdditionByAConstant()
    {
        var k = new RationalNumberExpression(1, "k");
        var f = new RateLatencyServiceCurve(1, 2);
        var g = new RateLatencyServiceCurve(3, 4);

        var e = Expressions.VerticalShift(
            Expressions.Convolution(f, g),
            k
        );

        var eq = e.ApplyEquivalence(new Nancy.Expressions.Equivalences.ConvAdditionByAConstant());
        
        Assert.True(e.Equivalent(eq));
        Assert.False(e == eq);
        var expected = "f \u2297 (g + k)";
        var regex = $"\\(?{Regex.Escape(expected)}\\)?";
        Assert.Matches(regex, eq.ToUnicodeString(showRationalsAsName: true));
    }
}