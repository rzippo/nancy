using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class ReadOnlyElementListCreation
{
    private static readonly Element[] Elements =
    [
        new Point(0, 1),
        new Segment(0, 2, 1, 1),
        new Point(2, 3),
        new Segment(2, 4, 3, 1)
    ];

    private static ReadOnlyElementList Of(params Element[] elements) => ReadOnlyElementList.CopyOf(elements);

    [Fact]
    public void CopyOfDoesNotFollowTheSource()
    {
        var source = new List<Element>(Elements);
        var list = ReadOnlyElementList.CopyOf(source);
        source[0] = new Point(0, 7);

        Assert.Equal(Elements, list);
    }

    public static IEnumerable<object[]> InvalidElements()
    {
        yield return [Array.Empty<Element>()];
        // two points at one time
        yield return [new Element[] { new Point(0, 1), new Point(0, 2) }];
        // a gap between a segment and the next point
        yield return [new Element[] { new Point(0, 1), new Segment(0, 1, 1, 1), new Point(2, 3) }];
        // out of order
        yield return [new Element[] { new Point(2, 3), new Segment(0, 2, 1, 1) }];
    }

    [Theory]
    [MemberData(nameof(InvalidElements))]
    public void CopyOfRejectsElementsOutOfUninterruptedOrder(Element[] elements)
        => Assert.Throws<ArgumentException>(() => ReadOnlyElementList.CopyOf(elements));

    [Fact]
    public void ConcatOfTwoKeepsTheListsInOrder()
    {
        var list = ReadOnlyElementList.Concat(Of(Elements[..1]), Of(Elements[1..]));

        Assert.Equal(Elements, list);
    }

    [Fact]
    public void ConcatOfThreeKeepsTheListsInOrder()
    {
        var list = ReadOnlyElementList.Concat(Of(Elements[..1]), Of(Elements[1..3]), Of(Elements[3..]));

        Assert.Equal(Elements, list);
    }

    [Fact]
    public void ConcatRejectsListsThatDoNotAdjoin()
    {
        // on the line 1 + t, but with nothing defined between 1 and 2
        var before = Of(new Point(0, 1), new Segment(0, 1, 1, 1));
        var after = Of(new Point(2, 3), new Segment(2, 3, 3, 1));
        var point = Of(new Point(1, 2));

        Assert.Throws<ArgumentException>(() => ReadOnlyElementList.Concat(before, after));
        Assert.Throws<ArgumentException>(() => ReadOnlyElementList.Concat(point, point));
        Assert.Throws<ArgumentException>(() => ReadOnlyElementList.Concat(before, point, after));
    }

    [Fact]
    public void SliceKeepsTheElementsFromTheIndex()
    {
        var slice = Of(Elements).Slice(2, 2);

        Assert.Equal(Elements[2..], slice);
        Assert.Equal(2, slice.Count);
        Assert.Same(Elements[2], slice[0]);
        Assert.Equal(Elements[2..], slice.AsSpan().ToArray());
        Assert.Equal(Elements[2..], slice.AsMemory().ToArray());
        Assert.Equal(1, slice.IndexOf(Elements[3]));
        Assert.Equal(-1, slice.IndexOf(Elements[0]));
        Assert.DoesNotContain(Elements[1], slice);
        var copy = new Element[3];
        slice.CopyTo(copy, 1);
        Assert.Equal([null, Elements[2], Elements[3]], copy);
    }

    [Fact]
    public void ASliceOfASliceCountsFromItsOwnStart()
    {
        var slice = Of(Elements).Slice(1, 3).Slice(2, 1);

        Assert.Equal(Elements[3..], slice);
    }

    [Fact]
    public void SliceRangesAreChecked()
    {
        var list = Of(Elements);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(Elements.Length, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(2, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(2, 2)[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.Slice(2, 2)[-1]);
    }

    [Fact]
    public void ACopyDoesNotShareTheArray()
    {
        var source = Of(Elements);
        var copy = source.Slice(1, 2).Copy();

        Assert.Equal(Elements[1..3], copy);
        Assert.Equal(2, copy.AsMemory().Length);
        Assert.False(System.Runtime.InteropServices.MemoryMarshal.TryGetArray(source.AsMemory(), out var a)
                     && System.Runtime.InteropServices.MemoryMarshal.TryGetArray(copy.AsMemory(), out var b)
                     && ReferenceEquals(a.Array, b.Array));
    }
}
