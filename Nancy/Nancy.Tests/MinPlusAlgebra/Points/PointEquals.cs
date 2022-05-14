using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Points;

public class PointEquals
{
    [Fact]
    public void SamePoints()
    {
        Point p1 = new Point(5, 3);
        Point p2 = new Point(5, 3);

        Assert.True(p1.Equals(p2));
    }

    [Fact]
    public void DifferentPoints()
    {
        Point p1 = new Point(5, 3);
        Point p2 = new Point(7, 2);

        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void NullObject()
    {
#nullable disable
        Point p1 = new Point(5, 3);
        object o = null;

        Assert.False(p1.Equals(o));
#nullable restore
    }

    [Fact]
    public void TypeMismatch()
    {
        Point p = new Point(5, 3);
        Segment s = Segment.Constant(8, 10, 2);

        Assert.False(p.Equals(s));
    }
}