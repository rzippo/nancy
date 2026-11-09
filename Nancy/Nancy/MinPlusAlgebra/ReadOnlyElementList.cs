using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// An immutable, non-empty list of elements in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.
/// </summary>
internal sealed class ReadOnlyElementList : IList<Element>, IReadOnlyList<Element>
{
    private readonly Element[] storage;
    private readonly int start;
    private readonly int count;

    private ReadOnlyElementList(Element[] storage) : this(storage, 0, storage.Length)
    {
    }

    private ReadOnlyElementList(Element[] storage, int start, int count)
    {
        this.storage = storage;
        this.start = start;
        this.count = count;
    }

    /// <summary>Copies the elements into a new list.</summary>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    /// <exception cref="ArgumentException">If there are no elements, or they are not in uninterrupted time order.</exception>
    public static ReadOnlyElementList CopyOf(IEnumerable<Element> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        var storage = elements.ToArray();
        if (storage.Length == 0)
            throw new ArgumentException("A list of elements cannot be empty.", nameof(elements));
        if (!((ReadOnlySpan<Element>) storage).AreUninterruptedSequence())
            throw new ArgumentException("Elements must alternate points and segments at adjoining times.", nameof(elements));
        return new ReadOnlyElementList(storage);
    }

    /// <summary>The elements as a span, without copying.</summary>
    public ReadOnlySpan<Element> AsSpan() => new(storage, start, count);

    /// <summary>The elements as a read-only memory region, without copying.</summary>
    public ReadOnlyMemory<Element> AsMemory() => new(storage, start, count);

    /// <inheritdoc cref="List{T}.Count"/>
    public int Count => count;

    /// <inheritdoc cref="List{T}.this"/>
    public Element this[int index] => index >= 0 && index < count
        ? storage[start + index]
        : throw new ArgumentOutOfRangeException(nameof(index));

    Element IList<Element>.this[int index]
    {
        get => this[index];
        set => throw new NotSupportedException("The elements of a sequence are read-only.");
    }

    bool ICollection<Element>.IsReadOnly => true;

    /// <inheritdoc cref="List{T}.Contains"/>
    public bool Contains(Element item) => IndexOf(item) >= 0;

    /// <inheritdoc cref="List{T}.IndexOf(T)"/>
    public int IndexOf(Element item)
    {
        var index = Array.IndexOf(storage, item, start, count);
        return index < 0 ? -1 : index - start;
    }

    /// <inheritdoc cref="List{T}.CopyTo(T[], int)"/>
    public void CopyTo(Element[] array, int arrayIndex) => Array.Copy(storage, start, array, arrayIndex, count);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public IEnumerator<Element> GetEnumerator() => new Enumerator(storage, start, start + count);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection<Element>.Add(Element item) => throw new NotSupportedException("The elements of a sequence are read-only.");

    void ICollection<Element>.Clear() => throw new NotSupportedException("The elements of a sequence are read-only.");

    bool ICollection<Element>.Remove(Element item) => throw new NotSupportedException("The elements of a sequence are read-only.");

    void IList<Element>.Insert(int index, Element item) => throw new NotSupportedException("The elements of a sequence are read-only.");

    void IList<Element>.RemoveAt(int index) => throw new NotSupportedException("The elements of a sequence are read-only.");

    /// <summary>The <paramref name="length"/> elements from <paramref name="index"/> on, sharing this list's array without copying it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If the range is empty or not within the list.</exception>
    public ReadOnlyElementList Slice(int index, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, count - index);
        return new ReadOnlyElementList(storage, start + index, length);
    }

    /// <summary>Returns the suffix starting at <paramref name="index"/>, sharing this list's array without copying it.</summary>
    /// <param name="index">Inclusive zero-based index of the first element.</param>
    /// <returns>The nonempty suffix.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> does not identify an element of this list.</exception>
    public ReadOnlyElementList Slice(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, count);
        return new ReadOnlyElementList(storage, start + index, count - index);
    }

    /// <summary>Copies the elements into a new list, which does not share this list's array.</summary>
    public ReadOnlyElementList Copy() => new(AsSpan().ToArray());

    /// <summary>Copies the elements of two lists, one after the other, into a new list.</summary>
    /// <exception cref="ArgumentException">If the second list does not start where the first ends, alternating a point and a segment.</exception>
    public static ReadOnlyElementList Concat(ReadOnlyElementList first, ReadOnlyElementList second)
    {
        ThrowIfNotAdjoining(first, second);
        var storage = new Element[first.count + second.count];
        first.AsSpan().CopyTo(storage);
        second.AsSpan().CopyTo(storage.AsSpan(first.count));
        return new ReadOnlyElementList(storage);
    }

    /// <summary>Copies the elements of three lists, one after the other, into a new list.</summary>
    /// <exception cref="ArgumentException">If a list does not start where the one before it ends, alternating a point and a segment.</exception>
    public static ReadOnlyElementList Concat(ReadOnlyElementList first, ReadOnlyElementList second, ReadOnlyElementList third)
    {
        ThrowIfNotAdjoining(first, second);
        ThrowIfNotAdjoining(second, third);
        var storage = new Element[first.count + second.count + third.count];
        first.AsSpan().CopyTo(storage);
        second.AsSpan().CopyTo(storage.AsSpan(first.count));
        third.AsSpan().CopyTo(storage.AsSpan(first.count + second.count));
        return new ReadOnlyElementList(storage);
    }

    private static void ThrowIfNotAdjoining(ReadOnlyElementList before, ReadOnlyElementList after)
    {
        var last = before[before.count - 1];
        var next = after[0];
        if (next.StartTime != last.EndTime || (last is Point) == (next is Point))
            throw new ArgumentException("The lists must adjoin, alternating a point and a segment.");
    }

    /// <summary>Reads the range of the array forward by index.</summary>
    private sealed class Enumerator : IEnumerator<Element>
    {
        private readonly Element[] storage;
        private readonly int start;
        private readonly int end;
        private int index;

        public Enumerator(Element[] storage, int start, int end)
        {
            this.storage = storage;
            this.start = start;
            this.end = end;
            index = start - 1;
        }

        public Element Current => index >= start && index < end
            ? storage[index]
            : throw new InvalidOperationException("The enumerator is not positioned on an element.");

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (index < end)
                index++;
            return index < end;
        }

        public void Reset() => index = start - 1;

        public void Dispose()
        {
        }
    }
}
