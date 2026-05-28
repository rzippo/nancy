using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra;

public class ElementTests
{
    [Fact]
    public void Point_Length_IsZero()
    {
        var p = new Point(3, 5);
        Assert.Equal(Rational.Zero, p.Length);
    }

    [Fact]
    public void Segment_Length_IsPositive()
    {
        var s = new Segment(0, 5, 2, 1);
        Assert.Equal(new Rational(5), s.Length);
    }

    [Fact]
    public void Point_IsFinite_True()
    {
        var p = new Point(3, 5);
        Assert.True(p.IsFinite);
    }

    [Fact]
    public void Segment_IsFinite_True()
    {
        var s = new Segment(0, 5, 2, 1);
        Assert.True(s.IsFinite);
    }

    [Fact]
    public void Point_GetStableHashCode_Stable()
    {
        var p1 = new Point(3, 5);
        var p2 = new Point(3, 5);
        Assert.Equal(p1.GetStableHashCode(), p2.GetStableHashCode());
    }

    [Fact]
    public void Segment_GetStableHashCode_Stable()
    {
        var s1 = new Segment(0, 5, 2, 1);
        var s2 = new Segment(0, 5, 2, 1);
        Assert.Equal(s1.GetStableHashCode(), s2.GetStableHashCode());
    }

    [Fact]
    public void Point_ToMppgString_FormatsCorrectly()
    {
        var p = new Point(3, 5);
        var str = p.ToMppgString();
        Assert.Contains("3", str);
        Assert.Contains("5", str);
    }

    [Fact]
    public void Segment_ToMppgString_FormatsCorrectly()
    {
        var s = new Segment(0, 5, 2, 1);
        var str = s.ToMppgString();
        Assert.Contains("0", str);
        Assert.Contains("5", str);
    }

    [Fact]
    public void Point_AttainsValue_AtItsTime()
    {
        var p = new Point(3, 5);
        Assert.True(p.AttainsValue(5));
        Assert.False(p.AttainsValue(7));
    }

    [Fact]
    public void Segment_AttainsValue_WithinRange()
    {
        var s = new Segment(0, 5, 2, 1);
        Assert.True(s.AttainsValue(3));
        Assert.False(s.AttainsValue(10));
    }

    [Fact]
    public void Point_HorizontalShift_ShiftsTime()
    {
        var p = new Point(3, 5);
        var shifted = p.HorizontalShift(2);
        Assert.Equal(new Point(5, 5), shifted);
    }

    [Fact]
    public void Segment_HorizontalShift_ShiftsTime()
    {
        var s = new Segment(0, 5, 2, 1);
        var shifted = s.HorizontalShift(2);
        Assert.Equal(new Segment(2, 7, 2, 1), shifted);
    }
}
