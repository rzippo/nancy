using System;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

public class ZeroPseudoPeriodLength
{
    [Fact]
    public void CurveConstructor_RejectsZeroLength()
    {
        var seq = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Constant(0, 5, 0)
        });
        // DefinedUntil = 5, pseudoPeriodStart + pseudoPeriodLength = 5 + 0 = 5
        // Sequence check would pass, but the guard must fire first.
        Assert.Throws<ArgumentException>(() => new Curve(seq, 5, 0, 1));
    }

    [Fact]
    public void CurveConstructor_RejectsNegativeLength()
    {
        var seq = new Sequence(new Element[]
        {
            Point.Origin(),
            Segment.Constant(0, 5, 0)
        });
        Assert.Throws<ArgumentException>(() => new Curve(seq, 3, -1, 1));
    }
}
