using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

/// <summary>
/// Checks the sequence shape predicates through their enumerable, span and array overloads.
/// </summary>
/// <remarks>
/// Each shape states its expected answers independently of any overload.
/// Enumerable callers holding an array or a list are read as spans, and other collections through their enumerator, so the enumerable overloads are checked over both kinds of source.
/// </remarks>
public class SequenceShapeSpans
{
    /// <summary>Elements with the answers of the three shape predicates.</summary>
    public sealed record Shape(string Name, Element[] Elements, bool InTimeOrder, bool InTimeSequence, bool Uninterrupted)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>A way of holding the same elements as an enumerable.</summary>
    public sealed record Source(string Name, Func<Element[], IEnumerable<Element>> Hold)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    private static List<Shape> ShapeCatalogue() =>
    [
        new("single point", [new Point(0, 1)], true, true, true),
        new("single segment", [new Segment(0, 1, 2, 3)], true, true, true),
        new("point, segment, point",
            [Point.Origin(), Segment.Constant(0, 1, 0), new Point(1, 0)],
            true, true, true),
        new("segment, point, segment",
            [new Segment(0, 1, 2, 1), new Point(1, 4), new Segment(1, 3, 4, -1)],
            true, true, true),
        new("points out of order", [new Point(1, 1), Point.Origin()], false, false, false),
        new("point, then a segment after a gap",
            [Point.Origin(), Segment.Constant(1, 2, 0)],
            true, true, false),
        new("two points at different times", [Point.Origin(), new Point(1, 0)], true, true, false),
        new("two points at the same time", [Point.Origin(), new Point(0, 1)], true, false, false),
        new("two adjoining segments",
            [Segment.Constant(0, 1, 0), Segment.Constant(1, 2, 0)],
            true, true, false),
        new("overlapping segments",
            [Segment.Constant(0, 2, 0), Segment.Constant(1, 3, 0)],
            true, false, false),
        new("segments sharing a start, longer first",
            [Segment.Constant(0, 2, 0), Segment.Constant(0, 1, 0)],
            false, false, false)
    ];

    private static List<Source> SourceCatalogue() =>
    [
        new("array", elements => elements),
        new("list", elements => elements.ToList()),
        new("read-only collection", elements => new ReadOnlyCollection<Element>(elements)),
        new("iterator", Iterate)
    ];

    private static IEnumerable<Element> Iterate(Element[] elements)
    {
        foreach (var element in elements)
            yield return element;
    }

    public static IEnumerable<object[]> Shapes() => ShapeCatalogue().ToXUnitTestCases();

    public static IEnumerable<object[]> ShapesAndSources()
        => ShapeCatalogue()
            .SelectMany(shape => SourceCatalogue().Select(source => (shape, source)))
            .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ShapesAndSources))]
    public void EnumerableOverloadsAnswerTheShape(Shape shape, Source source)
    {
        Assert.Equal(shape.InTimeOrder, source.Hold(shape.Elements).AreInTimeOrder());
        Assert.Equal(shape.InTimeSequence, source.Hold(shape.Elements).AreInTimeSequence());
        Assert.Equal(shape.Uninterrupted, source.Hold(shape.Elements).AreUninterruptedSequence());
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void SpanOverloadsAnswerTheShape(Shape shape)
    {
        ReadOnlySpan<Element> span = shape.Elements;
        Assert.Equal(shape.InTimeOrder, span.AreInTimeOrder());
        Assert.Equal(shape.InTimeSequence, span.AreInTimeSequence());
        Assert.Equal(shape.Uninterrupted, span.AreUninterruptedSequence());
    }

    [Theory]
    [MemberData(nameof(Shapes))]
    public void ArrayOverloadsAnswerTheShape(Shape shape)
    {
        Assert.Equal(shape.InTimeOrder, shape.Elements.AreInTimeOrder());
        Assert.Equal(shape.InTimeSequence, shape.Elements.AreInTimeSequence());
        Assert.Equal(shape.Uninterrupted, shape.Elements.AreUninterruptedSequence());
    }

    [Fact]
    public void ASpanSliceIsReadAsItsOwnElements()
    {
        // Uninterrupted as a whole; the slice drops the leading point and keeps its uninterrupted remainder.
        Element[] elements = [Point.Origin(), Segment.Constant(0, 1, 0), new Point(1, 0), Segment.Constant(1, 2, 0)];
        var slice = new ReadOnlySpan<Element>(elements, 1, 3);
        Assert.True(slice.AreUninterruptedSequence());

        // Dropping the middle point leaves two adjoining segments.
        Element[] gapped = [elements[1], elements[3]];
        Assert.False(new ReadOnlySpan<Element>(gapped).AreUninterruptedSequence());
    }

    [Fact]
    public void EmptySpansAreRejected()
    {
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.AreInTimeOrder());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.AreInTimeSequence());
        Assert.Throws<ArgumentException>(() => ReadOnlySpan<Element>.Empty.AreUninterruptedSequence());
    }

    [Fact]
    public void EmptyArraysAreRejected()
    {
        Element[] elements = [];
        Assert.Throws<ArgumentException>(() => elements.AreInTimeOrder());
        Assert.Throws<ArgumentException>(() => elements.AreInTimeSequence());
        Assert.Throws<ArgumentException>(() => elements.AreUninterruptedSequence());
    }

    [Fact]
    public void NullArraysAreRejected()
    {
        Element[] elements = null!;
        Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => elements.AreInTimeOrder()).ParamName);
        Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => elements.AreInTimeSequence()).ParamName);
        Assert.Equal("elements", Assert.Throws<ArgumentNullException>(() => elements.AreUninterruptedSequence()).ParamName);
    }

    [Fact]
    public void ACovariantPointArrayIsReadAsItsElements()
    {
        Point[] points = [Point.Origin(), new Point(1, 0)];
        Assert.True(points.AreInTimeOrder());
        Assert.True(points.AreInTimeSequence());
        Assert.False(points.AreUninterruptedSequence());
    }
}
