using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointClosure
{
    [Fact]
    public void SimplePoint()
    {
        Point point = new Point(time: 3, value: 5);

        Curve pointClosure = point.SubAdditiveClosure();

        Assert.False(pointClosure.IsContinuous);
        Assert.False(pointClosure.IsContinuousExceptOrigin);
        Assert.False(pointClosure.IsFinite);
        Assert.False(pointClosure.IsZero);
        Assert.False(pointClosure.IsUltimatelyPlain);

        Assert.Equal(Rational.Zero, pointClosure.ValueAt(0));
        Assert.Equal(5, pointClosure.ValueAt(3));
        Assert.Equal(10, pointClosure.ValueAt(6));
        Assert.Equal(15, pointClosure.ValueAt(9));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    public void PointInZero_Positive(decimal value)
    {
        Point point = new Point(time: 0, value: value);

        var closure = point.SubAdditiveClosure();
        Assert.Equal(Rational.Zero, closure.ValueAt(0));
        Assert.Equal(Rational.PlusInfinity, closure.ValueAt(1));

        Assert.False(closure.IsContinuous);
        Assert.False(closure.IsFinite);
        Assert.False(closure.IsZero);
        Assert.True(closure.IsUltimatelyPlain);
    }

    [Fact]
    public void PointInZero_Negative()
    {
        Point point = new Point(time: 0, value: -1);
        Assert.Throws<NotImplementedException>(() => point.SubAdditiveClosure());
    }
}