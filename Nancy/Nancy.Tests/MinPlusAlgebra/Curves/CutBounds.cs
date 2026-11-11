using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Curves;

/// <summary>
/// The materialized and the streamed cut of a curve reject the same windows, with the same exception.
/// </summary>
public class CutBounds
{
    private static readonly (string name, Curve curve)[] Curves =
    [
        ("ultimately pseudo-periodic", new Curve(new Sequence([
            Point.Origin(), new Segment(0, 1, 0, 2), new Point(1, 3), new Segment(1, 2, 3, 1),
            new Point(2, 4), new Segment(2, 3, 4, 0)
        ]), 1, 2, 2)),
        ("ultimately affine", new RateLatencyServiceCurve(2, 3)),
        ("ultimately infinite", new Curve(new Sequence([
            Point.Origin(), new Segment(0, 2, 0, 1), new Point(2, Rational.PlusInfinity),
            new Segment(2, 3, Rational.PlusInfinity, 0)
        ]), 2, 1, 0)),
    ];

    private static readonly (Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)[] InvalidWindows =
    [
        (-1, 5, true, false),
        (-3, -1, true, false),
        (5, 4, true, false),
        (1, Rational.PlusInfinity, true, false),
        (0, Rational.PlusInfinity, true, false),
        (Rational.PlusInfinity, Rational.PlusInfinity, true, true),
        (4, 4, true, false),
    ];

    public static IEnumerable<object[]> Cases()
        => Curves.SelectMany(c => InvalidWindows.Select(w =>
            new object[] { c.name, w.start, w.end, w.isStartIncluded, w.isEndIncluded }));

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamedCutRejectsAsMaterialized(string curveName, Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        var curve = Curves.Single(c => c.name == curveName).curve;
        var materialized = Assert.Throws<ArgumentException>(() => curve.Cut(start, end, isStartIncluded, isEndIncluded));
        // The streamed contract fixes what complete consumption throws, not when.
        var streamed = Assert.Throws<ArgumentException>(() => curve.CutAsEnumerable(start, end, isStartIncluded, isEndIncluded).ToArray());
        Assert.Equal(materialized.Message, streamed.Message);
    }
}
