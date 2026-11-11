using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// The materialized cut of a sequence, its streamed cut and the cut of its element stream reject the same windows, with the same exception.
/// </summary>
public class CutBounds
{
    /// <summary>
    /// Support $]1, 5]$, so that both an excluded and an included endpoint can be requested.
    /// </summary>
    private static Sequence LeftOpen() => new Sequence([
        new Segment(1, 3, 0, 1), new Point(3, 2), new Segment(3, 5, 2, 0), new Point(5, 2)
    ]);

    public static IEnumerable<object[]> InvalidWindows() =>
    [
        [new Rational(4), new Rational(2), false, false],
        [new Rational(0), new Rational(2), false, false],
        [new Rational(2), new Rational(6), false, false],
        [new Rational(1), new Rational(4), true, false],
        [new Rational(4), new Rational(4), true, false],
        [new Rational(1), new Rational(1), true, true],
    ];

    [Theory]
    [MemberData(nameof(InvalidWindows))]
    public void EveryCutRejectsAsMaterialized(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        var sequence = LeftOpen();
        var materialized = Assert.Throws<ArgumentException>(() => sequence.Cut(start, end, isStartIncluded, isEndIncluded));
        // The streamed contract fixes what complete consumption throws, not when.
        var streamed = Assert.Throws<ArgumentException>(() => sequence.CutAsEnumerable(start, end, isStartIncluded, isEndIncluded).ToArray());
        var elements = Assert.Throws<ArgumentException>(() => sequence.Elements.Cut(start, end, isStartIncluded, isEndIncluded).ToArray());
        Assert.Equal(materialized.Message, streamed.Message);
        Assert.Equal(materialized.Message, elements.Message);
    }
}
