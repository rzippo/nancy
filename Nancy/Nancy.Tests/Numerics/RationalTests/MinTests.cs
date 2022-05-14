using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class MinTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FiniteMin()
    {
        Assert.Equal(b, Rational.Min(a, b));
    }

    [Fact]
    public void InfiniteMin_1()
    {
        Assert.Equal(Rational.MinusInfinity, Rational.Min(a, Rational.MinusInfinity));
        Assert.Equal(Rational.MinusInfinity, Rational.Min(Rational.MinusInfinity, a));
        Assert.Equal(Rational.MinusInfinity, Rational.Min(b, Rational.MinusInfinity));
        Assert.Equal(Rational.MinusInfinity, Rational.Min(Rational.MinusInfinity, b));
    }

    [Fact]
    public void InfiniteMin_2()
    {
        Assert.Equal(Rational.MinusInfinity, Rational.Min(Rational.MinusInfinity, Rational.PlusInfinity));
    }
}