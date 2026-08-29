using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class TryGetFinite
{
    [Fact]
    public void FiniteValueReturnsTrueWithItsOwnNumeratorAndDenominator()
    {
        var value = new BigRational(3, 4);

        var result = value.TryGetFinite(out var numerator, out var denominator);

        Assert.True(result);
        Assert.Equal(value.Numerator, numerator);
        Assert.Equal(value.Denominator, denominator);
    }

    [Fact]
    public void PlusInfinityReturnsFalseWithDefaultOutParams()
    {
        var result = BigRational.PlusInfinity.TryGetFinite(out var numerator, out var denominator);

        Assert.False(result);
        Assert.Equal(default, numerator);
        Assert.Equal(default, denominator);
    }

    [Fact]
    public void MinusInfinityReturnsFalseWithDefaultOutParams()
    {
        var result = BigRational.MinusInfinity.TryGetFinite(out var numerator, out var denominator);

        Assert.False(result);
        Assert.Equal(default, numerator);
        Assert.Equal(default, denominator);
    }
}
