using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.MinPlusAlgebra;

/// <summary>
/// Enumerates elements and locates points and segments by time during a merge-style sweep.
/// </summary>
/// <remarks>
/// An enumerator advances by whole elements; a cursor also samples successive times while retaining the segment that contains them.
/// This supports sweeps through several functions whose breakpoints need not coincide.
/// Each consumed element is checked for uninterrupted time order as described by <see cref="Sequence.Elements"/>.
/// Unread elements are not validated.
/// Construction opens the enumerator without reading an element; either <see cref="MoveNext"/> or a lookup starts traversal.
/// Requests are ordered by time, then by side: the point at a time precedes the segment immediately to its right.
/// Repeated requests are allowed, and sampling inside a segment does not advance to its end.
/// A manual advance moves the position to the next element's start and discards the preceding element.
/// The order requirement also applies after exhaustion; rejected backward requests leave the position unchanged.
/// Try lookups return false only past the right endpoint; before-left, empty-input and ordering errors throw.
/// A before-left failure is possible only while the cursor holds its first element and has not served a lookup.
/// After leaving that state, requests behind the cursor position are ordering errors.
/// Disposing the cursor or encountering malformed input closes traversal; subsequent lookups throw <see cref="ArgumentException"/>.
/// </remarks>
public sealed class ElementCursor : IEnumerator<Element>
{
    private enum Side : byte
    {
        // No request since the last advance: the position is the start of the current element.
        // With no current element, the cursor has not started.
        Unset,
        Point,
        Right,
        // No element is available: the source was empty, malformed or disposed.
        Closed
    }

    private IEnumerator<Element>? enumerator;
    private Element? current;
    private bool isFirst;
    private Rational lastTime;
    private Side lastSide;

    /// <summary>
    /// Opens a forward cursor before the first input element.
    /// </summary>
    /// <param name="elements">Elements in uninterrupted time order, as described by <see cref="Sequence.Elements"/>.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null.</exception>
    public ElementCursor(IEnumerable<Element> elements)
        => enumerator = (elements ?? throw new ArgumentNullException(nameof(elements))).GetEnumerator();

    /// <summary>The current input element, retained when sampling within a segment.</summary>
    /// <exception cref="InvalidOperationException">If traversal has not started, has ended or is closed.</exception>
    public Element Current => current ?? throw new InvalidOperationException("The cursor is not positioned on an element.");

    object IEnumerator.Current => Current;

    /// <summary>Advances to the next input element and validates its adjacency to the preceding element.</summary>
    /// <returns>True if an element is available; false on empty input and after exhaustion or disposal.</returns>
    /// <exception cref="ArgumentException">If the next element violates uninterrupted time order; traversal is then closed.</exception>
    public bool MoveNext()
    {
        if (current is null && lastSide != Side.Unset)
            return false;
        if (enumerator!.MoveNext())
        {
            var next = enumerator.Current;
            if (current is not null &&
                (next.StartTime != current.EndTime || (current is Point) == (next is Point)))
            {
                Close();
                throw new ArgumentException("Elements must alternate points and segments at adjoining times.");
            }
            isFirst = current is null;
            current = next;
            lastSide = Side.Unset;
            return true;
        }
        if (current is null)
        {
            lastSide = Side.Closed;
        }
        else
        {
            // Past the end: a point at the end time is passed only if the last element defined it.
            lastTime = current.EndTime;
            lastSide = current is Point ? Side.Right : Side.Point;
            current = null;
        }
        return false;
    }

    /// <summary>Resetting a forward cursor is unsupported.</summary>
    /// <exception cref="NotSupportedException">On every call.</exception>
    public void Reset() => throw new NotSupportedException("The cursor only supports forward traversal.");

    /// <summary>Closes traversal and disposes the source enumerator once.</summary>
    public void Dispose() => Close();

    private void Close()
    {
        enumerator?.Dispose();
        enumerator = null;
        current = null;
        lastSide = Side.Closed;
    }

    /// <summary>
    /// Checks a request against the position and, if it is valid, prepares the current element.
    /// </summary>
    /// <returns>False if the request is after the right endpoint of an exhausted source.</returns>
    private bool Admit(Rational t, Side side)
    {
        if (current is null)
        {
            if (lastSide == Side.Unset)
            {
                if (!MoveNext())
                    throw new ArgumentException("The element stream is empty.");
            }
            else if (lastSide == Side.Closed)
                throw new ArgumentException("No element is available: the stream is empty, malformed or disposed.");
            else
            {
                CheckOrder(t, side);
                lastTime = t;
                lastSide = side;
                return false;
            }
        }

        if (lastSide == Side.Unset)
        {
            var c = t.CompareTo(current!.StartTime);
            if (c < 0 || (c == 0 && side == Side.Point && current is Segment))
            {
                if (isFirst)
                    throw new BeforeLeftEndpointException();
                throw new ArgumentException("The request precedes the cursor position.", nameof(t));
            }
        }
        else
            CheckOrder(t, side);
        return true;
    }

    private void CheckOrder(Rational t, Side side)
    {
        var c = t.CompareTo(lastTime);
        if (c < 0 || (c == 0 && lastSide == Side.Right && side == Side.Point))
            throw new ArgumentException("The request precedes the previous request.", nameof(t));
    }

    /// <summary>Finds the point at the requested time, sampling a segment when necessary.</summary>
    /// <param name="t">The requested time, in the sequential order described by this cursor.</param>
    /// <param name="point">The found point, or null when the request is past the right endpoint.</param>
    /// <returns>True when the requested position belongs to the support; false past its right endpoint.</returns>
    /// <exception cref="BeforeLeftEndpointException">If an initial lookup precedes the first supported position.</exception>
    /// <exception cref="ArgumentException">If input is empty or malformed, the cursor is closed, or the request precedes the cursor position.</exception>
    public bool TryPointAt(Rational t, [NotNullWhen(true)] out Point? point)
    {
        point = null;
        if (!Admit(t, Side.Point))
            return false;
        while (current is Point p ? p.Time < t : current!.EndTime <= t)
        {
            if (!MoveNext())
            {
                lastTime = t;
                lastSide = Side.Point;
                return false;
            }
        }
        lastTime = t;
        lastSide = Side.Point;
        point = current as Point ?? ((Segment) current!).Sample(t);
        return true;
    }

    /// <summary>Finds the segment covering times immediately to the right of the requested time.</summary>
    /// <param name="t">The requested time, in the sequential order described by this cursor.</param>
    /// <param name="segment">The found segment, or null when the request is past the right endpoint.</param>
    /// <returns>True when the requested position belongs to the support; false past its right endpoint.</returns>
    /// <exception cref="BeforeLeftEndpointException">If an initial lookup precedes the first supported position.</exception>
    /// <exception cref="ArgumentException">If input is empty or malformed, the cursor is closed, or the request precedes the cursor position.</exception>
    public bool TrySegmentAfter(Rational t, [NotNullWhen(true)] out Segment? segment)
    {
        segment = null;
        if (!Admit(t, Side.Right))
            return false;
        while (current is not Segment s || s.EndTime <= t)
        {
            if (!MoveNext())
            {
                lastTime = t;
                lastSide = Side.Right;
                return false;
            }
        }
        lastTime = t;
        lastSide = Side.Right;
        segment = (Segment) current;
        return true;
    }

    /// <summary>Finds the point at the requested time, sampling a segment when necessary.</summary>
    /// <param name="t">The requested time, in the sequential order described by this cursor.</param>
    /// <returns>The existing or sampled point.</returns>
    /// <exception cref="BeforeLeftEndpointException">If an initial lookup precedes the first supported position.</exception>
    /// <exception cref="AfterRightEndpointException">If the request is past the right endpoint.</exception>
    /// <exception cref="ArgumentException">If input is empty or malformed, the cursor is closed, or the request precedes the cursor position.</exception>
    public Point PointAt(Rational t)
        => TryPointAt(t, out var point) ? point : throw new AfterRightEndpointException();

    /// <summary>Finds the segment covering times immediately to the right of the requested time.</summary>
    /// <param name="t">The requested time, in the sequential order described by this cursor.</param>
    /// <returns>The existing or sampled segment.</returns>
    /// <exception cref="BeforeLeftEndpointException">If an initial lookup precedes the first supported position.</exception>
    /// <exception cref="AfterRightEndpointException">If the request is past the right endpoint.</exception>
    /// <exception cref="ArgumentException">If input is empty or malformed, the cursor is closed, or the request precedes the cursor position.</exception>
    public Segment SegmentAfter(Rational t)
        => TrySegmentAfter(t, out var segment) ? segment : throw new AfterRightEndpointException();
}
