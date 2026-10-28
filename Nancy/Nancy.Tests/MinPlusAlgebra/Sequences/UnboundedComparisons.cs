using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Affine bounds and first differences with unbounded operands, including finite overlaps and isolated boundary points.
/// </summary>
public class UnboundedComparisons
{
    private static readonly Rational Infinity = Rational.PlusInfinity;

    public static IEnumerable<(string Name, Element[] Left, Element[] Right, bool Bound, bool Strict, Rational? Difference)> FiniteCases()
    {
        yield return ("larger positive slope", [new Segment(0, Infinity, 0, 2)], [new Segment(0, Infinity, 0, 1)], false, false, 0);
        yield return ("smaller positive slope", [new Segment(0, Infinity, 0, 1)], [new Segment(0, Infinity, 0, 2)], true, true, 0);
        yield return ("larger negative slope", [new Segment(0, Infinity, 0, -1)], [new Segment(0, Infinity, 0, -2)], false, false, 0);
        yield return ("smaller negative slope", [new Segment(0, Infinity, 0, -2)], [new Segment(0, Infinity, 0, -1)], true, true, 0);
        yield return ("parallel below", [new Segment(0, Infinity, 0, 1)], [new Segment(0, Infinity, 1, 1)], true, true, 0);
        yield return ("parallel above", [new Segment(0, Infinity, 1, 1)], [new Segment(0, Infinity, 0, 1)], false, false, 0);
        yield return ("equal ramps", [new Segment(0, Infinity, 0, 1)], [new Segment(0, Infinity, 0, 1)], true, false, null);
        yield return ("equal constants", [Segment.Constant(0, Infinity, 1)], [Segment.Constant(0, Infinity, 1)], true, false, null);
        yield return ("crossing from below", [new Segment(0, Infinity, 0, 2)], [new Segment(0, Infinity, 1, 1)], false, false, 0);
        yield return ("crossing from above", [new Segment(0, Infinity, 1, 1)], [new Segment(0, Infinity, 0, 2)], false, false, 0);
        yield return ("equal limits at the common start", [new Segment(0, Infinity, 0, 1)], [new Segment(2, Infinity, 2, 2)], true, true, 2);
        yield return ("left value grows before the overlap", [new Segment(0, Infinity, 0, 1)], [new Segment(2, Infinity, 1, 1)], false, false, 2);
        yield return ("right value grows before the overlap", [new Segment(2, Infinity, 1, 1)], [new Segment(0, Infinity, 0, 1)], true, true, 2);
        yield return ("equal functions with different starts", [new Segment(0, Infinity, 0, 1)], [new Segment(2, Infinity, 2, 1)], true, false, null);
        yield return ("bounded overlap ends at the crossing", [new Segment(0, Infinity, 0, 2)], [new Segment(0, 1, 1, 1)], true, true, 0);
        yield return ("bounded overlap starts at the crossing", [new Segment(0, 1, 0, 1)], [new Segment(0, Infinity, 0, 2)], true, true, 0);
        yield return ("included equal start", [new Point(0, 0), new Segment(0, Infinity, 0, 1)], [new Point(0, 0), new Segment(0, Infinity, 0, 2)], true, false, 0);
        yield return ("violating start point", [new Point(0, 3), new Segment(0, Infinity, 0, 1)], [new Point(0, 0), new Segment(0, Infinity, 0, 2)], false, false, 0);
        yield return ("first difference after a shared prefix",
            [new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, Infinity, 1, 1)],
            [new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 1), new Segment(1, Infinity, 1, 2)], true, false, 1);
    }

    public static IEnumerable<(string Name, Element[] Left, Element[] Right, bool Bound, Rational? Difference)> InfiniteCases()
    {
        yield return ("ramp below plus infinity", [new Segment(0, Infinity, 0, 1)], [Segment.PlusInfinite(0, Infinity)], true, 0);
        yield return ("plus infinity above ramp", [Segment.PlusInfinite(0, Infinity)], [new Segment(0, Infinity, 0, 1)], false, 0);
        yield return ("minus infinity below falling ramp", [Segment.MinusInfinite(0, Infinity)], [new Segment(0, Infinity, 0, -1)], true, 0);
        yield return ("falling ramp above minus infinity", [new Segment(0, Infinity, 0, -1)], [Segment.MinusInfinite(0, Infinity)], false, 0);
        yield return ("equal plus infinities", [Segment.PlusInfinite(0, Infinity)], [Segment.PlusInfinite(0, Infinity)], true, null);
        yield return ("equal minus infinities", [Segment.MinusInfinite(0, Infinity)], [Segment.MinusInfinite(0, Infinity)], true, null);
        yield return ("opposite infinities ordered", [Segment.MinusInfinite(0, Infinity)], [Segment.PlusInfinite(0, Infinity)], true, 0);
        yield return ("opposite infinities reversed", [Segment.PlusInfinite(0, Infinity)], [Segment.MinusInfinite(0, Infinity)], false, 0);
    }

    public static IEnumerable<object[]> Bounds()
        => FiniteCases()
            .Select(c => (c.Name, c.Left, c.Right, c.Bound))
            .Concat(InfiniteCases().Select(c => (c.Name, c.Left, c.Right, c.Bound)))
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Bounds))]
    public void BoundsMatchExpectedOrder(string name, Element[] left, Element[] right, bool expected)
    {
        Assert.NotEmpty(name);
        var a = new Sequence(left);
        var b = new Sequence(right);
        Assert.Equal(expected, Sequence.LessOrEqual(a, b));
        Assert.Equal(expected, Sequence.GreaterOrEqual(b, a));
        Assert.Equal(expected, a <= b);
        Assert.Equal(expected, b >= a);
        Assert.Equal(expected, SequenceExtensions.LessOrEqualOnOverlap(left, right));
    }

}
