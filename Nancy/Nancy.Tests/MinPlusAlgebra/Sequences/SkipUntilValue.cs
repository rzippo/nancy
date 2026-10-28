using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Threshold skipping validates monotonicity as each source element is consumed.
/// </summary>
public class SkipUntilValue
{
    public static IEnumerable<(string Name, Sequence Source, Rational Threshold, Element[] Expected)> ValidCases()
    {
        var ramp = new Sequence([new Point(0, 0), new Segment(0, 2, 0, 2), new Point(2, 4)]);
        yield return ("at the first point", ramp, 0, ramp.Elements.ToArray());
        yield return ("inside a segment", ramp, 1, [new Point(new Rational(1, 2), 1), new Segment(new Rational(1, 2), 2, 1, 2), new Point(2, 4)]);
        yield return ("at the final point", ramp, 4, [new Point(2, 4)]);
        yield return ("above every value", ramp, 5, []);
        yield return ("open initial limit", new Sequence([new Segment(0, 2, 0, 1)]), 0, [new Segment(0, 2, 0, 1)]);
        yield return ("unattained final limit", new Sequence([new Segment(0, 2, 0, 1)]), 2, []);
        yield return ("jump across the threshold",
            new Sequence([new Point(0, 0), new Segment(0, 1, 0, 1), new Point(1, 3), Segment.Constant(1, 2, 3)]), 2,
            [new Point(1, 3), Segment.Constant(1, 2, 3)]);
        yield return ("constant at the threshold", new Sequence([new Point(0, 2), Segment.Constant(0, 2, 2)]), 2,
            [new Point(0, 2), Segment.Constant(0, 2, 2)]);
        yield return ("negative threshold", new Sequence([new Segment(0, 2, -2, 1), new Point(2, 0)]), -1,
            [new Point(1, -1), new Segment(1, 2, -1, 1), new Point(2, 0)]);
        yield return ("plus infinity attained", new Sequence([Segment.PlusInfinite(0, 2)]), Rational.PlusInfinity,
            [Segment.PlusInfinite(0, 2)]);
        yield return ("minus infinity retained", new Sequence([Segment.MinusInfinite(0, 2)]), Rational.MinusInfinity,
            [Segment.MinusInfinite(0, 2)]);
        yield return ("a single point below the threshold", new Sequence([new Point(0, -1)]), 0, []);
        var plateau = new Sequence([new Point(0, 0), Segment.Zero(0, 4), new Point(4, 0), new Segment(4, 5, 0, 2), new Point(5, 2)]);
        yield return ("initial plateau at the threshold", plateau, 0, plateau.Elements.ToArray());
    }

    public static IEnumerable<object[]> ValidInputs() => ValidCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ValidInputs))]
    public void StreamedResultsMatchExpectedElements(string name, Sequence source, Rational threshold, Element[] expected)
    {
        Assert.NotEmpty(name);
        Assert.Equal(expected, source.Elements.SkipUntilValue(threshold));
    }

    public static IEnumerable<(string Name, Sequence Source, Element[] ValidPrefix)> DecreasingTailCases()
    {
        var falling = new Sequence([new Point(0, 1), new Segment(0, 1, 1, -2), new Point(1, -1)]);
        yield return ("falling segment below the threshold", falling, [new Point(0, 1)]);
        var jump = new Sequence([new Point(0, 2), Segment.Constant(0, 1, 2), new Point(1, 1)]);
        yield return ("downward jump above the threshold", jump, [new Point(0, 2), Segment.Constant(0, 1, 2)]);
        yield return ("downward jump after a crossing segment",
            new Sequence([new Segment(0, 2, -1, 1), new Point(2, -1)]),
            [new Point(1, 0), new Segment(1, 2, 0, 1)]);
        yield return ("segment starts below the preceding point",
            new Sequence([new Point(0, 2), Segment.Constant(0, 1, 1)]), [new Point(0, 2)]);
        yield return ("finite value after plus infinity",
            new Sequence([new Point(0, Rational.PlusInfinity), Segment.Constant(0, 1, 1)]),
            [new Point(0, Rational.PlusInfinity)]);
    }

    public static IEnumerable<object[]> DecreasingTails()
        => DecreasingTailCases()
            .Select(c => (c.Name, c.Source))
            .ToXUnitTestCases();

    public static IEnumerable<object[]> RetainedTails() => DecreasingTailCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(RetainedTails))]
    public void StreamedResultsRejectDecreasesBeforeYieldingThem(string name, Sequence source, Element[] validPrefix)
    {
        Assert.NotEmpty(name);
        using var result = source.Elements.SkipUntilValue(0).GetEnumerator();
        foreach (var expected in validPrefix)
        {
            Assert.True(result.MoveNext());
            Assert.Equal(expected, result.Current);
        }
        Assert.Throws<ArgumentException>(() => result.MoveNext());
    }

    [Theory]
    [MemberData(nameof(DecreasingTails))]
    public void LowerInverseRejectsDecreasesInTheRetainedTail(string name, Sequence source)
    {
        Assert.NotEmpty(name);
        Assert.Throws<ArgumentException>(() => source.Elements.LowerPseudoInverse(startFromZero: true)
            .ToList());
    }

    public static IEnumerable<(string Name, Element[] Elements, Rational Threshold)> DecreasingPrefixCases()
    {
        yield return ("falling segment below the threshold", [new Segment(0, 1, -1, -1)], 0);
        yield return ("falling first qualifying segment", [new Segment(0, 1, 2, -1)], 0);
        yield return ("downward jump in the skipped prefix", [new Point(0, -1), Segment.Constant(0, 1, -1), new Point(1, -2)], 0);
    }

    public static IEnumerable<object[]> DecreasingPrefixes() => DecreasingPrefixCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecreasingPrefixes))]
    public void StreamedResultsRejectDecreasesWhileSearching(string name, Element[] elements, Rational threshold)
    {
        Assert.NotEmpty(name);
        Assert.Throws<ArgumentException>(() => elements.SkipUntilValue(threshold)
            .ToList());
    }

    [Fact]
    public void StreamingReadsTheSourceOnceAndStopsWithItsConsumer()
    {
        var enumerations = 0;
        var disposed = false;
        IEnumerable<Element> Source()
        {
            Assert.Equal(1, ++enumerations);
            try
            {
                yield return new Point(0, -1);
                yield return new Segment(0, 1, -1, 1);
                yield return new Point(1, 0);
                throw new InvalidOperationException("The consumer did not request the tail.");
            }
            finally
            {
                disposed = true;
            }
        }

        var result = Source().SkipUntilValue(0);
        Assert.Equal(0, enumerations);
        Assert.Equal(new Point(1, 0), result.First());
        Assert.True(disposed);
    }

    [Fact]
    public void AnEmptyStreamHasAnEmptyResult()
    {
        Assert.Empty(Enumerable.Empty<Element>().SkipUntilValue(0));
    }

}
