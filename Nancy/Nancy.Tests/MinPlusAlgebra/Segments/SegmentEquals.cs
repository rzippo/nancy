using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Segments;

public class SegmentEquals
{
    [Fact]
    public void SameSegments()
    {
        Segment p1 = new Segment(3, 5, 3, 7);
        Segment p2 = new Segment(3, 5, 3, 7);

        Assert.True(p1.Equals(p2));
    }

    [Fact]
    public void DifferentSegments()
    {
        Segment p1 = new Segment(3, 5, 3, 7);
        Segment p2 = new Segment(2, 7, 1, 8);

        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void NullObject()
    {
#nullable disable
        Segment p1 = new Segment(3, 5, 3, 7);
        object o = null;

        Assert.False(p1.Equals(o));
#nullable restore
    }

    [Fact]
    public void TypeMismatch()
    {
        Segment s = new Segment(3, 5, 3, 7);
        Point p = new Point(5, 3);

        Assert.False(s.Equals(p));
    }
}