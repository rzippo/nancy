using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.RationalTests;

public class MaxTests
{
    Rational a = new Rational(4, 5);
    Rational b = new Rational(3, 4);

    [Fact]
    public void FiniteMax()
    {
        Assert.Equal(a, Rational.Max(a, b));
    }

    [Fact]
    public void InfiniteMax_1()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.Max(a, Rational.PlusInfinity));
        Assert.Equal(Rational.PlusInfinity, Rational.Max(b, Rational.PlusInfinity));
    }

    [Fact]
    public void InfiniteMax_2()
    {
        Assert.Equal(Rational.PlusInfinity, Rational.Max(Rational.MinusInfinity, Rational.PlusInfinity));
    }
}