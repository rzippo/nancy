using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class MinTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FiniteMin()
    {
        Assert.Equal(b, BigRational.Min(a, b));
    }

    [Fact]
    public void InfiniteMin_1()
    {
        Assert.Equal(BigRational.MinusInfinity, BigRational.Min(a, BigRational.MinusInfinity));
        Assert.Equal(BigRational.MinusInfinity, BigRational.Min(BigRational.MinusInfinity, a));
        Assert.Equal(BigRational.MinusInfinity, BigRational.Min(b, BigRational.MinusInfinity));
        Assert.Equal(BigRational.MinusInfinity, BigRational.Min(BigRational.MinusInfinity, b));
    }

    [Fact]
    public void InfiniteMin_2()
    {
        Assert.Equal(BigRational.MinusInfinity, BigRational.Min(BigRational.MinusInfinity, BigRational.PlusInfinity));
    }
}