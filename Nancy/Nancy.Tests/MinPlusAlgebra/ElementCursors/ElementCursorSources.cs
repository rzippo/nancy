using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.ElementCursors;

/// <summary>
/// Checks that an element cursor behaves alike over every kind of source.
/// </summary>
/// <remarks>
/// The cursor reads arrays and the elements of a sequence by index, lists through their own enumerator, and other collections through their enumerator.
/// The sources cover all three: an array and the elements of a sequence built from a stream, a list and a derived list, then a read-only collection and an iterator.
/// A sequence cannot hold empty or malformed elements, so the tests of those take the other sources.
/// </remarks>
public class ElementCursorSources
{
    /// <summary>A way of holding the same elements as the input of a cursor.</summary>
    public sealed record Source(string Name, Func<Element[], IEnumerable<Element>> Hold)
    {
        /// <inheritdoc />
        public override string ToString() => Name;
    }

    private sealed class DerivedList(IEnumerable<Element> elements) : List<Element>(elements);

    private static List<Source> SourceCatalogue() =>
    [
        new("array", elements => elements),
        new("list", elements => elements.ToList()),
        new("derived list", elements => new DerivedList(elements)),
        new("read-only collection", elements => new ReadOnlyCollection<Element>(elements)),
        new("iterator", Iterate)
    ];

    private static List<Source> ValidSourceCatalogue() =>
    [
        ..SourceCatalogue(),
        new("elements of a sequence built from a stream", elements => new Sequence(Iterate(elements)).Elements)
    ];

    public static IEnumerable<object[]> Sources() => SourceCatalogue().ToXUnitTestCases();

    public static IEnumerable<object[]> ValidSources() => ValidSourceCatalogue().ToXUnitTestCases();

    private static IEnumerable<Element> Iterate(Element[] elements)
    {
        foreach (var element in elements)
            yield return element;
    }

    private static readonly Segment FirstSegment = new(0, 10, 1, 2);

    private static Element[] Elements() =>
        [new Point(0, 1), FirstSegment, new Point(10, 25), new Segment(10, 20, 25, -1)];

    [Theory]
    [MemberData(nameof(ValidSources))]
    public void MoveNextEnumeratesTheSourceElements(Source source)
    {
        var elements = Elements();
        using var cursor = new ElementCursor(source.Hold(elements));
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        foreach (var element in elements)
        {
            Assert.True(cursor.MoveNext());
            Assert.Same(element, cursor.Current);
            Assert.Same(element, ((IEnumerator) cursor).Current);
        }
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
    }

    [Theory]
    [MemberData(nameof(ValidSources))]
    public void LookupsSampleTheSourceElements(Source source)
    {
        var elements = Elements();
        using var cursor = new ElementCursor(source.Hold(elements));
        Assert.Same(elements[0], cursor.PointAt(0));
        Assert.Same(FirstSegment, cursor.SegmentAfter(0));
        Assert.Equal(new Point(3, 7), cursor.PointAt(3));
        Assert.Same(FirstSegment, cursor.Current);
        Assert.Same(elements[2], cursor.PointAt(10));
        Assert.Equal(new Point(15, 20), cursor.PointAt(15));
        Assert.False(cursor.TryPointAt(20, out _));
        Assert.False(cursor.TrySegmentAfter(20, out _));
    }

    [Theory]
    [MemberData(nameof(ValidSources))]
    public void ExhaustionIsTerminal(Source source)
    {
        using var cursor = new ElementCursor(source.Hold(Elements()));
        while (cursor.MoveNext()) { }
        for (var i = 0; i < 3; i++)
        {
            Assert.False(cursor.MoveNext());
            Assert.Throws<InvalidOperationException>(() => cursor.Current);
        }
        Assert.False(cursor.TryPointAt(20, out _));
        Assert.Throws<ArgumentException>(() => cursor.PointAt(19));
    }

    [Theory]
    [MemberData(nameof(Sources))]
    public void EmptyInputEndsOnTheFirstMoveNext(Source source)
    {
        using var cursor = new ElementCursor(source.Hold([]));
        Assert.False(cursor.MoveNext());
        Assert.False(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.PointAt(0));
        Assert.Throws<ArgumentException>(() => cursor.SegmentAfter(0));
    }

    [Theory]
    [MemberData(nameof(Sources))]
    public void MalformedInputClosesTraversal(Source source)
    {
        using var cursor = new ElementCursor(source.Hold([new Point(0, 1), new Point(0, 2), new Segment(0, 1, 2, 0)]));
        Assert.True(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.MoveNext());
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(2, out _));
    }

    [Theory]
    [MemberData(nameof(ValidSources))]
    public void DisposeClosesTraversal(Source source)
    {
        var cursor = new ElementCursor(source.Hold(Elements()));
        Assert.True(cursor.MoveNext());
        cursor.Dispose();
        cursor.Dispose();
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.Throws<ArgumentException>(() => cursor.PointAt(5));
    }

    [Fact]
    public void ACovariantPointArrayIsReadAsItsElements()
    {
        Point[] points = [new Point(0, 1)];
        using var cursor = new ElementCursor(points);
        Assert.True(cursor.MoveNext());
        Assert.Same(points[0], cursor.Current);
        Assert.False(cursor.MoveNext());
    }

    [Fact]
    public void ACovariantPointArrayOfSeveralPointsIsMalformed()
    {
        Point[] points = [new Point(0, 1), new Point(1, 1)];
        using var cursor = new ElementCursor(points);
        Assert.True(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.MoveNext());
    }

    [Fact]
    public void ANullSourceIsRejected()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ElementCursor(null!));
        Assert.Equal("elements", exception.ParamName);
    }



    public static IEnumerable<object[]> ListSources() => SourceCatalogue()
        .Where(source => source.Hold([]) is List<Element>)
        .ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ListSources))]
    public void AChangeToAListDuringTraversalIsReported(Source source)
    {
        var list = (List<Element>) source.Hold(Elements());
        using var cursor = new ElementCursor(list);
        Assert.True(cursor.MoveNext());
        list[2] = new Point(10, 26);
        Assert.Throws<InvalidOperationException>(() => cursor.MoveNext());
    }

    [Theory]
    [MemberData(nameof(ListSources))]
    public void AChangeToAListDuringALazyTransformIsReported(Source source)
    {
        var list = (List<Element>) source.Hold(Elements());
        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var _ in list.Scale(2))
                list.Add(new Point(20, 15));
        });
    }

    [Theory]
    [MemberData(nameof(ValidSources))]
    public void StreamedTransformsReadTheSourceElements(Source source)
    {
        var scaled = source.Hold(Elements())
            .Scale(2)
            .ToSequence();
        Assert.Equal(new Rational(50), scaled.SupValue());
        Assert.Equal(4, scaled.Elements.Count);
    }
}
