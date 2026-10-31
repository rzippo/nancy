using System;
using System.Collections.Generic;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Reads elements forward, checking each one for uninterrupted time order as <see cref="ElementCursor.MoveNext"/> does.
/// </summary>
/// <remarks>
/// Spans, arrays and lists are read by index; other sources are read through their enumerator.
/// Unread elements are not validated.
/// </remarks>
internal ref struct ElementReader
{
    private readonly ReadOnlySpan<Element> span;
    private readonly IEnumerator<Element>? enumerator;
    private int index;
    private Element? current;

    /// <summary>Opens a reader before the first element of a stream.</summary>
    /// <param name="elements">Elements in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    public ElementReader(IEnumerable<Element> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        if (!SequenceExtensions.TryGetSpan(elements, out span))
            enumerator = elements.GetEnumerator();
        index = -1;
    }

    /// <summary>Opens a reader before the first element of a span.</summary>
    /// <param name="elements">Elements in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    public ElementReader(ReadOnlySpan<Element> elements)
    {
        span = elements;
        index = -1;
    }

    /// <summary>Throws unless the reader is still before its first element.</summary>
    /// <exception cref="InvalidOperationException">If an element has already been read.</exception>
    public readonly void ThrowIfStarted()
    {
        if (current is not null || index >= 0)
            throw new InvalidOperationException("The reader has already been read from.");
    }

    /// <summary>The element read last.</summary>
    public readonly Element Current => current!;

    /// <summary>Advances to the next element and validates its adjacency to the preceding one.</summary>
    /// <returns>True if an element is available; false after the last one.</returns>
    /// <exception cref="ArgumentException">If the next element violates uninterrupted time order.</exception>
    public bool MoveNext()
    {
        Element next;
        if (enumerator is not null)
        {
            if (!enumerator.MoveNext())
                return false;
            next = enumerator.Current;
        }
        else
        {
            if (index + 1 >= span.Length)
            {
                index = span.Length;
                return false;
            }
            next = span[++index];
        }
        if (current is not null &&
            (next.StartTime != current.EndTime || (current is Point) == (next is Point)))
            throw new ArgumentException("Elements must alternate points and segments at adjoining times.");
        current = next;
        return true;
    }

    /// <summary>Disposes the enumerator of a stream, if any.</summary>
    public void Dispose() => enumerator?.Dispose();
}
