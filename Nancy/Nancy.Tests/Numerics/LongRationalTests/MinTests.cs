using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class MinTests
{
    LongRational a = new LongRational(4, 5);
    LongRational b = new LongRational(3, 4);

    [Fact]
    public void FiniteMin()
    {
        Assert.Equal(b, LongRational.Min(a, b));
    }

    [Fact]
    public void InfiniteMin_1()
    {
        Assert.Equal(LongRational.MinusInfinity, LongRational.Min(a, LongRational.MinusInfinity));
        Assert.Equal(LongRational.MinusInfinity, LongRational.Min(LongRational.MinusInfinity, a));
        Assert.Equal(LongRational.MinusInfinity, LongRational.Min(b, LongRational.MinusInfinity));
        Assert.Equal(LongRational.MinusInfinity, LongRational.Min(LongRational.MinusInfinity, b));
    }

    [Fact]
    public void InfiniteMin_2()
    {
        Assert.Equal(LongRational.MinusInfinity, LongRational.Min(LongRational.MinusInfinity, LongRational.PlusInfinity));
    }
}