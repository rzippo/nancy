using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks that restricting the inverse to nonnegative heights preserves qualifying times in a negative source prefix.
/// </summary>
public class UpperPseudoInverseNegativePrefix
{
    public static IEnumerable<(string Name, Sequence Source, Sequence Expected)> Examples()
    {
        yield return ("jump over zero", new Sequence([
            new Point(0, -1), Segment.Constant(0, 1, -1),
            new Point(1, 1), Segment.Constant(1, 2, 1)
        ]), new Sequence([new Point(0, 1), Segment.Constant(0, 1, 1), new Point(1, 2)]));
        yield return ("excluded source start", new Sequence([
            Segment.Constant(0, 1, -1), new Point(1, 1), new Segment(1, 2, 1, 1)
        ]), new Sequence([new Point(0, 1), Segment.Constant(0, 1, 1),
            new Point(1, 1), new Segment(1, 2, 1, 1)]));
        yield return ("negative point then positive right limit", new Sequence([
            new Point(3, -1), new Segment(3, 4, 1, 1)
        ]), new Sequence([new Point(0, 3), Segment.Constant(0, 1, 3),
            new Point(1, 3), new Segment(1, 2, 3, 1)]));
        yield return ("zero point then positive right limit", new Sequence([
            new Point(0, -1), Segment.Constant(0, 1, -1),
            new Point(1, 0), new Segment(1, 2, 1, 1)
        ]), new Sequence([new Point(0, 1), Segment.Constant(0, 1, 1),
            new Point(1, 1), new Segment(1, 2, 1, 1)]));
        yield return ("negative point then zero right limit", new Sequence([
            new Point(0, -1), Segment.Constant(0, 1, -1),
            new Point(1, -1), new Segment(1, 2, 0, 1)
        ]), new Sequence([new Point(0, 1), new Segment(0, 1, 1, 1)]));
        yield return ("continuous crossing", new Sequence([
            new Point(0, -1), new Segment(0, 2, -1, 1), new Point(2, 1)
        ]), new Sequence([new Point(0, 1), new Segment(0, 1, 1, 1), new Point(1, 2)]));
        yield return ("plateau at zero", new Sequence([
            new Point(0, -1), new Segment(0, 1, -1, 1), new Point(1, 0),
            Segment.Zero(1, 2), Point.Zero(2), new Segment(2, 3, 0, 1)
        ]), new Sequence([new Point(0, 2), new Segment(0, 1, 2, 1)]));
        yield return ("rising to zero excluded", new Sequence([
            new Segment(0, 1, -1, 1)
        ]), new Sequence([new Point(0, 1)]));
        yield return ("negative through excluded end", new Sequence([
            new Point(0, -2), new Segment(0, 1, -2, 1)
        ]), new Sequence([new Point(0, 1)]));
        yield return ("negative through included end", new Sequence([
            new Point(0, -2), new Segment(0, 1, -2, 1), new Point(1, -1)
        ]), new Sequence([new Point(0, 1)]));
        yield return ("minus infinite prefix", new Sequence([
            Point.MinusInfinite(0), Segment.MinusInfinite(0, 1),
            new Point(1, 1), Segment.Constant(1, 2, 1)
        ]), new Sequence([new Point(0, 1), Segment.Constant(0, 1, 1), new Point(1, 2)]));
    }

    public static IEnumerable<object[]> Cases() => Examples().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Cases))]
    public void MaterializedInversePreservesNegativePrefix(string name, Sequence source, Sequence expected)
    {
        var actual = source.UpperPseudoInverse(startFromZero: true);
        Assert.True(Sequence.Equivalent(expected, actual), name);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamedInversePreservesNegativePrefixInOneTraversal(string name, Sequence source, Sequence expected)
    {
        var traversals = 0;
        IEnumerable<Element> Once()
        {
            if (++traversals != 1)
                throw new InvalidOperationException("The input was enumerated twice.");
            foreach (var element in source.Elements)
                yield return element;
        }

        var stream = Once().UpperPseudoInverse(startFromZero: true);
        Assert.Equal(0, traversals);
        var actual = stream.ToSequence();
        Assert.Equal(1, traversals);
        Assert.True(Sequence.Equivalent(expected, actual), name);
    }

    public static IEnumerable<Element[]> DecreasingPrefixes()
    {
        yield return [new Point(0, -1), Segment.Constant(0, 1, -2), new Point(1, 1)];
        yield return [new Point(0, -1), new Segment(0, 1, -1, -1), new Point(1, 1)];
        yield return [new Point(0, -1), Segment.Constant(0, 1, -1), new Point(1, -2),
            Segment.Constant(1, 2, -2), new Point(2, 1)];
    }

    public static IEnumerable<object[]> DecreasingCases() => DecreasingPrefixes().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(DecreasingCases))]
    public void RejectsDecreasesBeforeReplacingNegativeValues(Element[] source)
        => Assert.Throws<ArgumentException>(() => source
            .UpperPseudoInverse(startFromZero: true)
            .ToList());

    [Fact]
    public void StopsReadingAfterTheRequestedPrefix()
    {
        IEnumerable<Element> Source()
        {
            yield return new Point(0, -1);
            yield return Segment.Constant(0, 1, -1);
            yield return new Point(1, 1);
            yield return Segment.Constant(1, 2, 1);
            yield return new Point(2, 2);
            yield return new Segment(2, 3, 2, 1);
            yield return new Point(3, 3);
            throw new InvalidOperationException("The unrequested tail was consumed.");
        }

        var first = Source().UpperPseudoInverse(startFromZero: true)
            .Take(1)
            .Single();
        Assert.Equal(new Point(0, 1), first);
    }

}
