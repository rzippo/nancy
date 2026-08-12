using JetBrains.Annotations;
using Xunit;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.NetworkCalculus;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

[TestSubject(typeof(MppgFormattingException))]
public class ToMppgStringErrors
{
    private static CurveExpression A => Expressions.FromCurve(new SigmaRhoArrivalCurve(1, 2), "a");

    private static CurveExpression B => Expressions.FromCurve(new RateLatencyServiceCurve(2, 5), "b");

    [Fact]
    public void ToMppgString_OriginOperations_Throw()
    {
        Assert.Throws<MppgFormattingException>(() => A.WithZeroOrigin().ToMppgString());
        Assert.Throws<MppgFormattingException>(() => A.WithOriginAt(3).ToMppgString());
    }

    [Fact]
    public void ToMppgString_CurveExtrema_Throw()
    {
        Assert.Throws<MppgFormattingException>(() => A.SupValue().ToMppgString());
        Assert.Throws<MppgFormattingException>(() => A.InfValue().ToMppgString());
        Assert.Throws<MppgFormattingException>(() => A.MaxValue().ToMppgString());
        Assert.Throws<MppgFormattingException>(() => A.MinValue().ToMppgString());
    }

    [Fact]
    public void ToMppgString_Placeholders_Throw()
    {
        Assert.Throws<MppgFormattingException>(() => new CurvePlaceholderExpression("p").ToMppgString());
        Assert.Throws<MppgFormattingException>(() => new RationalPlaceholderExpression("q").ToMppgString());
    }

    [Fact]
    public void ToMppgString_SamplingOfACompoundCurve_Throws()
    {
        // MPPG samples a function only through the name of a variable
        Assert.Throws<MppgFormattingException>(() => Expressions.Addition(A, B).ValueAt(3).ToMppgString());
        Assert.Throws<MppgFormattingException>(() => Expressions.Addition(A, B).LeftLimitAt(3).ToMppgString());
        Assert.Throws<MppgFormattingException>(() => Expressions.Addition(A, B).RightLimitAt(3).ToMppgString());
    }

    [Fact]
    public void ToMppgString_SamplingOfANamedExpression_DoesNotThrow()
    {
        var named = Expressions.Addition(A, B, "k");

        Assert.Equal("k(3)", named.ValueAt(3).ToMppgString());
    }

    [Fact]
    public void MppgFormattingException_ReportsTheOffendingExpression()
    {
        var exception = Assert.Throws<MppgFormattingException>(() => A.WithZeroOrigin("z").ToMppgString());

        Assert.Equal(typeof(WithZeroOriginExpression), exception.ExpressionType);
        Assert.Equal("z", exception.ExpressionName);
        Assert.Contains("z", exception.Message);
    }

    [Fact]
    public void ToMppgString_DoesNotComputeTheExpression()
    {
        var expr = Expressions.Convolution(A, B);

        var mppg = expr.ToMppgString();

        Assert.False(expr.IsComputed);
        Assert.Equal(mppg, expr.ToMppgString());
    }

    [Fact]
    public void ToMppgString_OfAnUnsupportedOperation_DoesNotComputeTheExpression()
    {
        var expr = Expressions.Convolution(A, B).WithZeroOrigin();

        Assert.Throws<MppgFormattingException>(() => expr.ToMppgString());
        Assert.False(expr.IsComputed);
    }
}
