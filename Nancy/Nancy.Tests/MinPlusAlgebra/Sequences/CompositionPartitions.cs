using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class CompositionPartitions
{
    public static IReadOnlyList<Sequence> OuterFunctions { get; } =
    [
        new Sequence([
            new Point(0, -2), new Segment(0, 1, -2, 3),
            new Point(1, 1), new Segment(1, 2, 1, -2),
            new Point(2, -1), new Segment(2, 3, -1, 1), new Point(3, 0)
        ]),
        new Sequence([
            new Point(0, 4), new Segment(0, 1, -2, -1),
            new Point(1, 7), new Segment(1, 2, 9, 2),
            new Point(2, -3), new Segment(2, 3, 6, -2), new Point(3, 5)
        ]),
        new Sequence([
            Point.MinusInfinite(0), new Segment(0, 1, 2, -1),
            Point.PlusInfinite(1), Segment.MinusInfinite(1, 2),
            new Point(2, 3), Segment.PlusInfinite(2, 3), new Point(3, -1)
        ]),
        new Sequence([new Point(0, 7), Segment.Constant(0, 3, 7), new Point(3, 7)])
    ];

    public static IEnumerable<(Sequence Outer, Sequence Inner)> GeneratedPairs()
    {
        Rational[] levels = [0, new Rational(1, 2), new Rational(3, 2), 3];
        var monotoneValues =
            from startPoint in levels
            from firstStart in levels.Where(value => value >= startPoint)
            from firstEnd in levels.Where(value => value >= firstStart)
            from middlePoint in levels.Where(value => value >= firstEnd)
            from secondStart in levels.Where(value => value >= middlePoint)
            from secondEnd in levels.Where(value => value >= secondStart)
            from endPoint in levels.Where(value => value >= secondEnd)
            select (startPoint, firstStart, firstEnd, middlePoint, secondStart, secondEnd, endPoint);

        foreach (var outer in OuterFunctions)
        foreach (var values in monotoneValues)
        {
            var inner = new Sequence([
                new Point(-1, values.startPoint),
                new Segment(-1, 1, values.firstStart, (values.firstEnd - values.firstStart) / 2),
                new Point(1, values.middlePoint),
                new Segment(1, 4, values.secondStart, (values.secondEnd - values.secondStart) / 3),
                new Point(4, values.endPoint)
            ]);
            foreach (var restricted in EndpointVariants(inner))
            {
                // An excluded endpoint discards its value; keep one representative for each retained element sequence.
                if (!restricted.IsLeftClosed && values.startPoint != values.firstStart)
                    continue;
                if (!restricted.IsRightClosed && values.endPoint != values.secondEnd)
                    continue;
                yield return (outer, restricted);
            }
        }
    }

    public static IEnumerable<(Sequence Outer, Sequence Inner)> OperandPairs() => GeneratedPairs()
        .Concat(BoundaryPairs())
        .Concat(Composition.WellDefinedPairs.Select(pair => (pair.f, pair.g)))
        .Concat(CompositionUnboundedSupport.KnownCompositions.Select(pair => (pair.Outer, pair.Inner)));

    public static IEnumerable<object[]> Cases() => OperandPairs().ToXUnitTestCases();

    public static IEnumerable<object[]> BoundedCases() => OperandPairs()
        .Where(pair => pair.Outer.DefinedUntil.IsFinite && pair.Inner.DefinedUntil.IsFinite)
        .ToXUnitTestCases();

    public static IEnumerable<(Sequence Outer, Sequence Inner)> BoundaryPairs()
    {
        var openOuter = new Sequence([new Segment(0, 2, 3, -1)]);
        yield return (openOuter, new Sequence([new Segment(0, 2, 0, 1)]));
        yield return (openOuter, new Sequence([new Point(0, 1), new Segment(0, 1, 1, 1)]));
        yield return (openOuter, new Sequence([new Segment(0, 1, 0, 1), new Point(1, 1)]));

        var plateau = new Sequence([new Point(0, 1), Segment.Constant(0, 2, 1), new Point(2, 1)]);
        foreach (var inner in EndpointVariants(plateau))
            yield return (new Sequence([new Point(1, Rational.PlusInfinity)]), inner);

        yield return (
            new Sequence([new Point(0, 7), new Segment(0, 2, 9, 1)]),
            new Sequence([Segment.Constant(0, 1, 0), new Point(1, 0), new Segment(1, 2, 0, 1)])
        );
        yield return (
            new Sequence([new Segment(0, 2, 9, 1), new Point(2, 7)]),
            new Sequence([new Segment(0, 1, 1, 1), new Point(1, 2), Segment.Constant(1, 2, 2)])
        );
    }

    public static IEnumerable<object[]> KnownCases() => Composition.KnownCompositions
        .Distinct()
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(KnownCases))]
    public void StreamCompositionMatchesKnownResults(Sequence outer, Sequence inner, Sequence expected)
    {
        var actual = outer.Elements.Composition(inner.Elements)
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual));
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void StreamCompositionAgreesOnEveryAffinePiece(Sequence outer, Sequence inner)
    {
        var actual = ValidationCases.SingleUse(outer.Elements)
            .Composition(ValidationCases.SingleUse(inner.Elements))
            .ToSequence();
        AssertPointwiseComposition(outer, inner, actual);
    }

    [Theory]
    [MemberData(nameof(BoundedCases))]
    public void StreamCompositionMatchesInverseImagePartitioning(Sequence outer, Sequence inner)
    {
        var expected = MaterializedCompositionReference.Composition(outer, inner);
        var actual = outer.Elements.Composition(inner.Elements)
            .ToSequence();
        Assert.True(Sequence.Equivalent(expected, actual));
    }

    private static IEnumerable<Sequence> EndpointVariants(Sequence sequence)
    {
        yield return sequence;
        yield return new Sequence(sequence.Elements.Where(element =>
            element is not Point point || point.Time != sequence.DefinedFrom));
        yield return new Sequence(sequence.Elements.Where(element =>
            element is not Point point || point.Time != sequence.DefinedUntil));
        yield return new Sequence(sequence.Elements.Where(element =>
            element is not Point point || point.Time != sequence.DefinedFrom && point.Time != sequence.DefinedUntil));
    }

    /// <summary>
    /// Checks every point and two interior values per affine interval of an independent common partition.
    /// </summary>
    private static void AssertPointwiseComposition(Sequence outer, Sequence inner, Sequence actual)
    {
        Assert.Equal(inner.DefinedFrom, actual.DefinedFrom);
        Assert.Equal(inner.DefinedUntil, actual.DefinedUntil);
        Assert.Equal(inner.IsLeftClosed, actual.IsLeftClosed);
        Assert.Equal(inner.IsRightClosed, actual.IsRightClosed);

        var boundaries = inner.Elements.Concat(actual.Elements)
            .SelectMany(element => new[] { element.StartTime, element.EndTime })
            .ToList();
        var outerBoundaries = outer.Elements
            .SelectMany(element => new[] { element.StartTime, element.EndTime })
            .Where(time => time.IsFinite)
            .Distinct()
            .ToList();

        foreach (var segment in inner.Elements.OfType<Segment>()
            .Where(segment => segment.Slope > 0))
        foreach (var level in outerBoundaries.Where(level =>
                     level > segment.RightLimitAtStartTime && level < segment.LeftLimitAtEndTime))
        {
            boundaries.Add(segment.StartTime + (level - segment.RightLimitAtStartTime) / segment.Slope);
        }

        var times = boundaries.Distinct()
            .OrderBy(time => time)
            .ToArray();
        foreach (var time in times.Where(time => time.IsFinite))
        {
            Assert.Equal(inner.IsDefinedAt(time), actual.IsDefinedAt(time));
            if (inner.IsDefinedAt(time))
                Assert.Equal(outer.ValueAt(inner.ValueAt(time)), actual.ValueAt(time));
        }

        foreach (var (start, end) in times.Zip(times.Skip(1)))
        {
            // Two distinct values determine a finite affine piece; infinite pieces are constant.
            var first = end.IsPlusInfinite ? start + 1 : start + (end - start) / 3;
            var second = end.IsPlusInfinite ? start + 2 : start + 2 * (end - start) / 3;
            Assert.Equal(outer.ValueAt(inner.ValueAt(first)), actual.ValueAt(first));
            Assert.Equal(outer.ValueAt(inner.ValueAt(second)), actual.ValueAt(second));
        }
    }
}
