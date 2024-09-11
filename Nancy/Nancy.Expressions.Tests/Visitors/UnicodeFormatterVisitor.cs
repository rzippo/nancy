using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Visitors;


[TestSubject(typeof(Nancy.Expressions.Visitors.UnicodeFormatterVisitor))]
public class UnicodeFormatterVisitor
{

    [Fact]
    public void ToUnicodeString_UnicodeFormatting()
    {
        var a = new SigmaRhoArrivalCurve(1, 2);
        var b = new RateLatencyServiceCurve(2, 5);
        var e = Expressions.Convolution(a, b);
        
        Assert.Equal("a \u2297 b", e.ToUnicodeString());
    }
}