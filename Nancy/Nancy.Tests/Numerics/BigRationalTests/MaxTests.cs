using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.BigRationalTests;

public class MaxTests
{
    BigRational a = new BigRational(4, 5);
    BigRational b = new BigRational(3, 4);

    [Fact]
    public void FiniteMax()
    {
        Assert.Equal(a, BigRational.Max(a, b));
    }

    [Fact]
    public void InfiniteMax_1()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.Max(a, BigRational.PlusInfinity));
        Assert.Equal(BigRational.PlusInfinity, BigRational.Max(b, BigRational.PlusInfinity));
    }

    [Fact]
    public void InfiniteMax_2()
    {
        Assert.Equal(BigRational.PlusInfinity, BigRational.Max(BigRational.MinusInfinity, BigRational.PlusInfinity));
    }
}