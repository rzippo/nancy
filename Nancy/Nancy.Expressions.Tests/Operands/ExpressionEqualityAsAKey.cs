using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// The defect Equals/GetHashCode was added to fix: the record-generated hash included the mutable cache fields.
// An expression's hash therefore changed the first time its Value was computed, and it went missing from the bucket it had been filed under.
public class ExpressionEqualityAsAKey
{
    private static readonly Curve CurveA = new RateLatencyServiceCurve(rate: 2, latency: 1);
    private static readonly Curve CurveB = new SigmaRhoArrivalCurve(sigma: 3, rho: 1);

    private static CurveExpression Sum()
        => new AdditionExpression([CurveA.ToExpression("a"), CurveB.ToExpression("b")], "");

    [Fact]
    public void ComputingAValueLeavesTheHashUnchanged()
    {
        var expression = Sum();
        var before = expression.GetHashCode();

        expression.ComputeWithoutResult();

        Assert.True(expression.IsComputed);
        Assert.Equal(before, expression.GetHashCode());
    }

    [Fact]
    public void ClearingTheValueCacheLeavesTheHashUnchanged()
    {
        var expression = Sum();
        var before = expression.GetHashCode();
        expression.ComputeWithoutResult();

        expression.ClearValueCache();

        Assert.Equal(before, expression.GetHashCode());
    }

    [Fact]
    public void AKeyStaysFindableAcrossComputingAndClearingItsValue()
    {
        var key = Sum();
        var dictionary = new Dictionary<CurveExpression, string> { [key] = "value" };

        key.ComputeWithoutResult();
        Assert.True(dictionary.TryGetValue(key, out var afterCompute));
        Assert.Equal("value", afterCompute);

        key.ClearValueCache();
        Assert.True(dictionary.TryGetValue(key, out var afterClear));
        Assert.Equal("value", afterClear);
    }

    [Fact]
    public void AnEqualButSeparatelyBuiltExpressionFindsTheSameEntry()
    {
        var dictionary = new Dictionary<CurveExpression, string> { [Sum()] = "value" };

        Assert.True(dictionary.TryGetValue(Sum(), out var found));
        Assert.Equal("value", found);
    }

    [Fact]
    public void ASetKeepsOneEntryForTwoEqualExpressions()
    {
        var set = new HashSet<CurveExpression> { Sum(), Sum() };

        Assert.Single(set);
    }
}
