using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class CutEndpoints
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void CurveCutUsesTheRequestedEndpoints(bool includeStart, bool includeEnd)
    {
        var curve = new RateLatencyServiceCurve(2, 1);
        var result = curve.ToExpression().Cut(new Interval(2, 4, includeStart, includeEnd)).Value;

        AssertEndpoints(result, includeStart, includeEnd);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SequenceCutUsesTheRequestedEndpoints(bool includeStart, bool includeEnd)
    {
        var sequence = new Sequence([new Point(1, 0), new Segment(1, 5, 0, 2), new Point(5, 8)]);
        var result = sequence.ToExpression().Cut(new Interval(2, 4, includeStart, includeEnd)).Value;

        AssertEndpoints(result, includeStart, includeEnd);
    }

    private static void AssertEndpoints(Sequence result, bool includeStart, bool includeEnd)
    {
        Assert.Equal((Rational)2, result.DefinedFrom);
        Assert.Equal((Rational)4, result.DefinedUntil);
        Assert.Equal(includeStart, result.IsLeftClosed);
        Assert.Equal(includeEnd, result.IsRightClosed);
        Assert.Equal((Rational)4, result.ValueAt(3));
        if (includeStart)
            Assert.Equal((Rational)2, result.ValueAt(2));
        if (includeEnd)
            Assert.Equal((Rational)6, result.ValueAt(4));
    }
}
