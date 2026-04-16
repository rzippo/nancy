using System;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.Numerics.LongRationalTests;

public class DivisionEdgeCases
{
    [Fact]
    public void InfiniteOverFinitePositive()
    {
        var result = LongRational.PlusInfinity / new LongRational(2);
        Assert.Equal(LongRational.PlusInfinity, result);
    }

    [Fact]
    public void InfiniteOverFiniteNegative()
    {
        var result = LongRational.PlusInfinity / new LongRational(-2);
        Assert.Equal(LongRational.MinusInfinity, result);
    }

    [Fact]
    public void InfiniteOverFinite_FiniteSignDeterminesResult()
    {
        var posResult = LongRational.PlusInfinity / new LongRational(2);
        var negResult = LongRational.PlusInfinity / new LongRational(-2);
        var posNegResult = LongRational.MinusInfinity / new LongRational(2);
        var negNegResult = LongRational.MinusInfinity / new LongRational(-2);

        Assert.Equal(LongRational.PlusInfinity, posResult);
        Assert.Equal(LongRational.MinusInfinity, negResult);
        Assert.Equal(LongRational.MinusInfinity, posNegResult);
        Assert.Equal(LongRational.PlusInfinity, negNegResult);
    }

    [Fact]
    public void MinusInfiniteOverFinite()
    {
        var result = LongRational.MinusInfinity / new LongRational(3);
        Assert.Equal(LongRational.MinusInfinity, result);
    }

    [Fact]
    public void InfiniteOverRational()
    {
        var result = LongRational.PlusInfinity / new LongRational(3, 2);
        Assert.Equal(LongRational.PlusInfinity, result);
    }

    [Fact]
    public void MinusInfiniteOverRational()
    {
        var result = LongRational.MinusInfinity / new LongRational(3, 2);
        Assert.Equal(LongRational.MinusInfinity, result);
    }
}
