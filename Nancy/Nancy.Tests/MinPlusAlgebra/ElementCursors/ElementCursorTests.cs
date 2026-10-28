using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.ElementCursors;

/// <summary>
/// Checks the enumerator states and time lookups of an element cursor, including terminal exhaustion.
/// </summary>
public class ElementCursorTests
{
    [Fact]
    public void ConstructionLeavesAnEmptyCursorBeforeEnumeration()
    {
        var input = new Probe([]);
        using var cursor = new ElementCursor(input);
        Assert.Equal(0, input.MoveNextCalls);
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator) cursor).Current);
        Assert.Throws<ArgumentException>(() => cursor.PointAt(0));
        Assert.Throws<ArgumentException>(() => cursor.SegmentAfter(0));
        Assert.False(cursor.MoveNext());
    }

    [Fact]
    public void ConstructionLeavesANonemptyCursorBeforeItsFirstElement()
    {
        var input = new Probe([new Point(0, 1)]);
        using var cursor = new ElementCursor(input);
        Assert.Equal(0, input.MoveNextCalls);
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator) cursor).Current);
        Assert.True(cursor.MoveNext());
        Assert.Same(input.Elements[0], cursor.Current);
        Assert.Same(cursor.Current, ((IEnumerator) cursor).Current);
    }

    [Fact]
    public void MoveNextEnumeratesTheInputElements()
    {
        Element[] elements = [new Point(0, 1), new Segment(0, 10, 1, 2), new Point(10, 21)];
        using IEnumerator<Element> cursor = new ElementCursor(elements);
        foreach (var element in elements)
        {
            Assert.True(cursor.MoveNext());
            Assert.Same(element, cursor.Current);
        }
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
    }

    [Fact]
    public void TimeLookupsRetainTheCurrentSegmentAcrossOtherBreakpoints()
    {
        var segment = new Segment(0, 10, 1, 2);
        Element[] elements = [new Point(0, 1), segment, new Point(10, 21)];
        using var cursor = new ElementCursor(elements);
        Assert.True(cursor.MoveNext());
        Assert.Same(elements[0], cursor.PointAt(0));
        Assert.Same(segment, cursor.SegmentAfter(0));
        foreach (var time in new[] { 3, 7 })
        {
            Assert.Equal(new Point(time, 1 + 2 * time), cursor.PointAt(time));
            Assert.Same(segment, cursor.SegmentAfter(time));
            Assert.Same(segment, cursor.Current);
        }
        Assert.True(cursor.MoveNext());
        Assert.Same(elements[2], cursor.Current);
        Assert.Same(elements[2], cursor.PointAt(10));
        Assert.False(cursor.TrySegmentAfter(10, out _));
    }

    [Fact]
    public void MoveNextExhaustionIsTerminal()
    {
        var input = new Probe([new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3)]);
        using var cursor = new ElementCursor(input);
        Assert.True(cursor.MoveNext());
        var end = 3;
        while (cursor.MoveNext()) { }
        var calls = input.MoveNextCalls;
        for (var i = 0; i < 3; i++)
        {
            Assert.False(cursor.MoveNext());
            Assert.False(cursor.TryPointAt(end, out _));
            Assert.False(cursor.TrySegmentAfter(end, out _));
            Assert.Throws<InvalidOperationException>(() => cursor.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator) cursor).Current);
            end++;
        }
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void PointLookupExhaustionIsTerminal()
    {
        var input = new Probe([new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3)]);
        using var cursor = new ElementCursor(input);
        Assert.True(cursor.MoveNext());
        var end = 3;
        Assert.False(cursor.TryPointAt(end, out _));
        var calls = input.MoveNextCalls;
        for (var i = 0; i < 3; i++)
        {
            Assert.False(cursor.MoveNext());
            Assert.False(cursor.TryPointAt(end, out _));
            Assert.False(cursor.TrySegmentAfter(end, out _));
            Assert.Throws<InvalidOperationException>(() => cursor.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator) cursor).Current);
            end++;
        }
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void SegmentLookupExhaustionIsTerminal()
    {
        var input = new Probe([new Point(0, 1), new Segment(0, 2, 1, 1), new Point(2, 3)]);
        using var cursor = new ElementCursor(input);
        Assert.True(cursor.MoveNext());
        var end = 3;
        Assert.False(cursor.TrySegmentAfter(end, out _));
        end++;
        var calls = input.MoveNextCalls;
        for (var i = 0; i < 3; i++)
        {
            Assert.False(cursor.MoveNext());
            Assert.False(cursor.TryPointAt(end, out _));
            Assert.False(cursor.TrySegmentAfter(end, out _));
            Assert.Throws<InvalidOperationException>(() => cursor.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator) cursor).Current);
            end++;
        }
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void EmptyInputEndsOnTheFirstMoveNext()
    {
        var input = new Probe([]);
        using var cursor = new ElementCursor(input);
        Assert.False(cursor.MoveNext());
        Assert.False(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.PointAt(0));
        Assert.Throws<ArgumentException>(() => cursor.SegmentAfter(0));
        Assert.Equal(1, input.MoveNextCalls);
    }

    [Fact]
    public void ResetIsUnsupported()
    {
        using IEnumerator<Element> cursor = new ElementCursor(new Element[] { new Point(0, 1) });
        Assert.Throws<NotSupportedException>(() => cursor.Reset());
    }

    [Fact]
    public void DisposeReleasesTheUnderlyingEnumeratorOnce()
    {
        var input = new Probe([new Point(0, 1)]);
        var cursor = new ElementCursor(input);
        Assert.True(cursor.MoveNext());
        cursor.Dispose();
        cursor.Dispose();
        Assert.Equal(1, input.DisposeCalls);
        Assert.False(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.PointAt(0));
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
    }

    [Fact]
    public void TerminalPointRequestsAdvanceTheRecordedTime()
    {
        var input = new Probe([new Point(0, 1), Segment.Constant(0, 2, 1), new Point(2, 1)]);
        using var cursor = new ElementCursor(input);
        Assert.False(cursor.TryPointAt(3, out _));
        var calls = input.MoveNextCalls;
        Assert.False(cursor.TryPointAt(5, out _));
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(4, out _));
        Assert.Throws<ArgumentException>(() => cursor.TrySegmentAfter(4, out _));
        Assert.False(cursor.TryPointAt(5, out _));
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void TerminalRightRequestsAdvanceTheRecordedTimeAndSide()
    {
        var input = new Probe([new Point(0, 1)]);
        using var cursor = new ElementCursor(input);
        Assert.False(cursor.TryPointAt(3, out _));
        var calls = input.MoveNextCalls;
        Assert.False(cursor.TrySegmentAfter(3, out _));
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(3, out _));
        Assert.False(cursor.TrySegmentAfter(5, out _));
        Assert.Throws<ArgumentException>(() => cursor.TrySegmentAfter(4, out _));
        Assert.False(cursor.TrySegmentAfter(5, out _));
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void ThrowingTerminalLookupsRecordEveryRequest()
    {
        var input = new Probe([new Point(0, 1)]);
        using var cursor = new ElementCursor(input);
        Assert.Throws<AfterRightEndpointException>(() => cursor.PointAt(3));
        var calls = input.MoveNextCalls;
        Assert.Throws<AfterRightEndpointException>(() => cursor.PointAt(5));
        Assert.Throws<ArgumentException>(() => cursor.PointAt(4));
        Assert.Throws<AfterRightEndpointException>(() => cursor.SegmentAfter(5));
        Assert.Throws<ArgumentException>(() => cursor.PointAt(5));
        Assert.Throws<AfterRightEndpointException>(() => cursor.SegmentAfter(6));
        Assert.Throws<ArgumentException>(() => cursor.SegmentAfter(5));
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void ManualExhaustionStartsAtTheSideAfterTheFinalPoint()
    {
        var input = new Probe([new Point(2, 1)]);
        using var cursor = new ElementCursor(input);
        while (cursor.MoveNext()) { }
        var calls = input.MoveNextCalls;
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(2, out _));
        Assert.False(cursor.TrySegmentAfter(2, out _));
        Assert.False(cursor.TryPointAt(3, out _));
        Assert.False(cursor.TryPointAt(5, out _));
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(4, out _));
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void ManualExhaustionStartsAtThePointAtAnOpenEnd()
    {
        var input = new Probe([Segment.Constant(0, 2, 1)]);
        using var cursor = new ElementCursor(input);
        while (cursor.MoveNext()) { }
        var calls = input.MoveNextCalls;
        Assert.False(cursor.TryPointAt(2, out _));
        Assert.False(cursor.TrySegmentAfter(2, out _));
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(2, out _));
        Assert.False(cursor.TrySegmentAfter(3, out _));
        Assert.False(cursor.TrySegmentAfter(5, out _));
        Assert.Throws<ArgumentException>(() => cursor.TrySegmentAfter(4, out _));
        Assert.Equal(calls, input.MoveNextCalls);
    }

    [Fact]
    public void MalformedInputDisposesTheSourceOnceAndClosesTraversal()
    {
        var input = new Probe([new Point(0, 1), new Point(0, 2)]);
        var cursor = new ElementCursor(input);
        Assert.True(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.MoveNext());
        Assert.Equal(1, input.DisposeCalls);
        var calls = input.MoveNextCalls;
        Assert.False(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.TryPointAt(2, out _));
        Assert.Equal(calls, input.MoveNextCalls);
        cursor.Dispose();
        Assert.Equal(1, input.DisposeCalls);
    }

    private sealed class Probe(Element[] elements) : IEnumerable<Element>, IEnumerator<Element>
    {
        public Element[] Elements { get; } = elements;
        public int MoveNextCalls { get; private set; }
        public int DisposeCalls { get; private set; }
        private int index = -1;
        public Element Current => Elements[index];
        object IEnumerator.Current => Current;
        public IEnumerator<Element> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool MoveNext()
        {
            MoveNextCalls++;
            return index < Elements.Length && ++index < Elements.Length;
        }
        public void Reset() => throw new NotSupportedException();
        public void Dispose() => DisposeCalls++;
    }

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void TheCursorRejectsAnInvalidTransitionWhenReached(Element[] elements)
    {
        using var cursor = new ElementCursor(ValidationCases.SingleUse(elements));
        Assert.True(cursor.MoveNext());
        Assert.Throws<ArgumentException>(() => cursor.MoveNext());
    }
}
