using System;
using Unipi.Nancy.Expressions.Internals;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Compute;

public class PlaceholderExpressions
{
    [Fact]
    public void CurvePlaceholderCarriesNameAndCannotBeComputed()
    {
        var expression = Expressions.Placeholder("alpha");

        Assert.IsType<CurvePlaceholderExpression>(expression);
        Assert.Equal("alpha", ((CurvePlaceholderExpression)expression).CurveName);
        Assert.Equal("alpha", expression.Name);
        Assert.Contains("alpha", expression.ToUnicodeString());
        Assert.Throws<InvalidOperationException>(() => expression.Compute());
    }

    [Fact]
    public void RationalPlaceholderCarriesNameAndCannotBeComputed()
    {
        var expression = Expressions.RationalPlaceholder("theta");

        Assert.IsType<RationalPlaceholderExpression>(expression);
        Assert.Equal("theta", ((RationalPlaceholderExpression)expression).RationalName);
        Assert.Equal("theta", expression.Name);
        Assert.Contains("theta", expression.ToUnicodeString());
        Assert.Throws<InvalidOperationException>(() => expression.Compute());
    }
}
