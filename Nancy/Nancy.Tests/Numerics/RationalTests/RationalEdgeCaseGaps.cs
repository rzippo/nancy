using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class RationalEdgeCaseGaps
{
    [Fact]
    public void IncrementDecrement_FiniteValues()
    {
        var r = new Rational(3, 2);
        var incremented = r;
        incremented++;
        Assert.Equal(new Rational(5, 2), incremented);

        var decremented = r;
        decremented--;
        Assert.Equal(new Rational(1, 2), decremented);
    }

    [Fact]
    public void Invert_Zero_ReturnsPlusInfinity()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.Invert(Rational.Zero));
    }

    [Fact]
    public void Max_IEnumerable_ReturnsMaximum()
    {
        var values = new List<Rational> { new(1, 2), new(3, 4), new(5, 6) };
        var result = Rational.Max(values.AsEnumerable());
        Assert.Equal(new Rational(5, 6), result);
    }

    [Fact]
    public void Min_IEnumerable_ReturnsMinimum()
    {
        var values = new List<Rational> { new(1, 2), new(3, 4), new(5, 6) };
        var result = Rational.Min(values.AsEnumerable());
        Assert.Equal(new Rational(1, 2), result);
    }

    [Fact]
    public void GreatestCommonDivisor_LongOverload()
    {
        Assert.Equal(6, Rational.GreatestCommonDivisor(12, 18));
        Assert.Equal(1, Rational.GreatestCommonDivisor(7, 13));
        Assert.Equal(5, Rational.GreatestCommonDivisor(-10, 15));
        Assert.Equal(3, Rational.GreatestCommonDivisor(0, 3));
        Assert.Equal(0, Rational.GreatestCommonDivisor(0, 0));
    }
}
