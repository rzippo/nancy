using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceStorage
{
    private static List<Element> Elements() =>
    [
        new Point(0, 1),
        new Segment(0, 2, 1, 1),
        new Point(2, 3)
    ];

    [Fact]
    public void ChangesToTheSourceListDoNotReachTheSequence()
    {
        var list = Elements();
        var sequence = new Sequence(list);
        list[0] = new Point(0, 7);
        list.Add(new Segment(2, 3, 3, 0));

        Assert.Equal(Elements(), sequence.Elements);
    }

    [Fact]
    public void ElementsCannotBeChangedThroughACast()
    {
        var sequence = new Sequence(Elements());
        var cut = sequence.Cut(0, 2);

        foreach (var elements in new[] { sequence.Elements, cut.Elements })
        {
            Assert.IsNotType<List<Element>>(elements);
            Assert.IsNotType<Element[]>(elements);
            if (elements is IList<Element> list)
                Assert.Throws<NotSupportedException>(() => list[0] = new Point(0, 7));
        }
    }

    [Fact]
    public void EveryWriteThroughTheListInterfaceThrows()
    {
        var sequence = new Sequence(Elements());
        var list = Assert.IsAssignableFrom<IList<Element>>(sequence.Elements);

        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list.Add(new Point(2, 3)));
        Assert.Throws<NotSupportedException>(() => list.Insert(0, new Point(0, 1)));
        Assert.Throws<NotSupportedException>(() => list.Remove(list[0]));
        Assert.Throws<NotSupportedException>(() => list.RemoveAt(0));
        Assert.Throws<NotSupportedException>(() => list.Clear());
        Assert.Equal(Elements(), sequence.Elements);
    }

    [Fact]
    public void ListQueriesReadTheElements()
    {
        var sequence = new Sequence(Elements());
        var list = Assert.IsAssignableFrom<IList<Element>>(sequence.Elements);
        var copy = new Element[5];
        list.CopyTo(copy, 1);

        Assert.True(list.Contains(new Point(2, 3)));
        Assert.False(list.Contains(new Point(2, 4)));
        Assert.Equal(1, list.IndexOf(new Segment(0, 2, 1, 1)));
        Assert.Equal(-1, list.IndexOf(new Point(2, 4)));
        Assert.Equal(new Element?[] { null, new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3), null }, copy);
    }

    [Fact]
    public void EnumeratorReadsTheElementsOnlyBetweenItsEnds()
    {
        var sequence = new Sequence(Elements());
        using var enumerator = sequence.Elements.GetEnumerator();

        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        var read = new List<Element>();
        while (enumerator.MoveNext())
            read.Add(enumerator.Current);
        Assert.Equal(Elements(), read);
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(new Point(0, 1), enumerator.Current);
    }

    [Fact]
    public void AsSpanReadsTheElements()
    {
        var sequence = new Sequence(Elements());

        Assert.Equal(sequence.Elements, sequence.AsSpan().ToArray());
    }

    [Fact]
    public void AsMemoryReadsTheElements()
    {
        var sequence = new Sequence(Elements());

        Assert.Equal(sequence.Elements, sequence.AsMemory().ToArray());
    }
}
