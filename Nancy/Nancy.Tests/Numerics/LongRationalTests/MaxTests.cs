using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class MaxTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FiniteMax()
    {
        Assert.Equal(a, LongRational.Max(a, b));
    }

    [Fact]
    public void InfiniteMax_1()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.Max(a, LongRational.PlusInfinity));
        Assert.Equal(LongRational.PlusInfinity, LongRational.Max(b, LongRational.PlusInfinity));
    }

    [Fact]
    public void InfiniteMax_2()
    {
        Assert.Equal(LongRational.PlusInfinity, LongRational.Max(LongRational.MinusInfinity, LongRational.PlusInfinity));
    }
}