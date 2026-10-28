using System;
using System.Collections;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.ElementCursors;

public class ElementCursorLookups
{
    private static ElementCursor Open(params Element[] elements)
    {
        var cursor = new ElementCursor(elements);
        Assert.True(cursor.MoveNext());
        return cursor;
    }

    private static Element[] Ramp() => [new Point(0, 9), new Segment(0, 2, 1, 1), new Point(2, 8)];

    [Fact]
    public void BeforeClosedSupportHasItsOwnException()
    {
        using var c = Open(Ramp());
        Assert.Throws<BeforeLeftEndpointException>(() => c.PointAt(-1));
        Assert.Equal(9, c.PointAt(0).Value);
    }

    [Fact]
    public void BeforeOpenSupportHasItsOwnException()
    {
        using var c = Open(new Segment(0, 2, 1, 1));
        Assert.Throws<BeforeLeftEndpointException>(() => c.PointAt(0));
        Assert.Equal(1, c.SegmentAfter(0).RightLimitAtStartTime);
    }

    [Fact]
    public void RightSegmentBeforeSupportHasItsOwnException()
    {
        using var c = Open(Ramp());
        Assert.Throws<BeforeLeftEndpointException>(() => c.SegmentAfter(-1));
        Assert.Equal(9, c.PointAt(0).Value);
    }

    [Fact]
    public void PastClosedSupportHasItsOwnException()
    {
        using var c = Open(Ramp());
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(3));
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(3));
        Assert.False(c.MoveNext());
    }

    [Fact]
    public void OpenRightEndpointHasItsOwnException()
    {
        using var c = Open(new Segment(0, 2, 1, 1));
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(2));
    }

    [Fact]
    public void ClosedRightPointPrecedesRightEndFailure()
    {
        using var c = Open(Ramp());
        Assert.Equal(8, c.PointAt(2).Value);
        Assert.Throws<AfterRightEndpointException>(() => c.SegmentAfter(2));
    }

    [Fact]
    public void PointCannotFollowRightAtTheSameTime()
    {
        using var c = Open(Ramp());
        c.SegmentAfter(1);
        Assert.Throws<ArgumentException>(() => c.PointAt(1));
    }

    [Fact]
    public void PointCannotMoveBackwardAfterPoint()
    {
        using var c = Open(Ramp());
        c.PointAt(1);
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
    }

    [Fact]
    public void RightCannotMoveBackwardAfterPoint()
    {
        using var c = Open(Ramp());
        c.PointAt(1);
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(0));
    }

    [Fact]
    public void PointCannotMoveBackwardAfterRight()
    {
        using var c = Open(Ramp());
        c.SegmentAfter(1);
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
    }

    [Fact]
    public void PointCannotMoveBackwardInsideSegment()
    {
        using var c = Open(Ramp());
        c.PointAt(new Rational(3, 2));
        Assert.Throws<ArgumentException>(() => c.PointAt(1));
    }

    [Fact]
    public void RightCannotMoveBackwardInsideSegment()
    {
        using var c = Open(Ramp());
        c.SegmentAfter(new Rational(3, 2));
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(1));
    }

    [Fact]
    public void RightCannotMoveBackwardAfterRight()
    {
        using var c = Open(Ramp());
        c.SegmentAfter(1);
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(0));
    }

    [Fact]
    public void EqualPositionsAndArbitrarilyCloseLaterTimesWork()
    {
        using var c = Open(Ramp());
        Assert.Equal(9, c.PointAt(0).Value);
        Assert.Equal(9, c.PointAt(0).Value);
        var segment = c.SegmentAfter(0);
        Assert.Same(segment, c.SegmentAfter(0));
        var near = new Rational(1, 1_000_000);
        Assert.Equal(1+near, c.PointAt(near).Value);
        Assert.Same(segment, c.SegmentAfter(near));
    }

    [Fact]
    public void SingletonPointHasBothBoundaries()
    {
        using var c = Open(new Point(1, 7));
        Assert.Throws<BeforeLeftEndpointException>(() => c.PointAt(0));
        Assert.Equal(7, c.PointAt(1).Value);
        Assert.Throws<AfterRightEndpointException>(() => c.SegmentAfter(1));
    }

    [Fact]
    public void InfiniteValuesRemainValues()
    {
        using var c = Open(new Point(0, Rational.PlusInfinity), Segment.MinusInfinite(0, 2));
        Assert.True(c.PointAt(0).Value.IsPlusInfinite);
        Assert.True(c.PointAt(1).Value.IsMinusInfinite);
    }

    [Fact]
    public void InfiniteOpenRightEndpointIsExcluded()
    {
        using var c = Open(new Segment(0, Rational.PlusInfinity, 1, 1));
        Assert.Equal(1001, c.PointAt(1000).Value);
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(Rational.PlusInfinity));
    }

    [Fact]
    public void InfiniteOpenLeftEndpointIsExcluded()
    {
        using var c = Open(Segment.Constant(Rational.MinusInfinity, 0, 3));
        Assert.Throws<BeforeLeftEndpointException>(() => c.PointAt(Rational.MinusInfinity));
        Assert.Equal(3, c.PointAt(-1).Value);
    }

    [Fact]
    public void LookupStartsLazily()
    {
        using var c = new ElementCursor(Ramp());
        Assert.Equal(9, c.PointAt(0).Value);
        Assert.Equal(1, c.SegmentAfter(0).RightLimitAtStartTime);
        Assert.Equal(8, c.PointAt(2).Value);
    }

    [Fact]
    public void LazyLookupClassifiesBeforeLeft()
    {
        using var c = new ElementCursor(Ramp());
        Assert.Throws<BeforeLeftEndpointException>(() => c.PointAt(-1));
        Assert.Equal(9, c.PointAt(0).Value);
    }

    [Fact]
    public void LazyLookupOnEmptySourceIsInvalidInput()
    {
        using var c = new ElementCursor(Array.Empty<Element>());
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(0));
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
        Assert.False(c.MoveNext());
    }

    [Fact]
    public void AdvancedFirstElementMakesEarlierRequestsOrderingViolations()
    {
        using var c = Open(Ramp());
        Assert.True(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
        Assert.Throws<ArgumentException>(() => c.PointAt(-1));
        Assert.Equal(1, c.SegmentAfter(0).RightLimitAtStartTime);
    }

    [Fact]
    public void PassedOpenStartPointIsAnOrderingViolation()
    {
        using var c = Open(Ramp());
        Assert.True(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
    }

    [Fact]
    public void EmptyInputIsNotARegularSupportBoundary()
    {
        using var c = new ElementCursor(Array.Empty<Element>());
        Assert.False(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.PointAt(0));
    }

    [Fact]
    public void NullSourceIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new ElementCursor(null!));
    }

    [Fact]
    public void DisposedCursorHasNoElements()
    {
        var c = Open(Ramp());
        c.Dispose();
        Assert.False(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.PointAt(3));
    }

    [Fact]
    public void CurrentAndResetKeepEnumeratorSemantics()
    {
        using var c = new ElementCursor(Ramp());
        Assert.Throws<InvalidOperationException>(() => c.Current);
        while (c.MoveNext())
        {
        }
        Assert.Throws<InvalidOperationException>(() => c.Current);
        Assert.Throws<NotSupportedException>(() => c.Reset());
    }

    [Fact]
    public void ManualAdvanceCannotRestoreDiscardedHistory()
    {
        using var c = Open(Ramp());
        c.PointAt(1);
        Assert.True(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.PointAt(1));
    }

    [Fact]
    public void ExhaustionDoesNotMakeInteriorRequestsPastSupport()
    {
        using var c = Open(Ramp());
        while (c.MoveNext())
        {
        }
        Assert.Throws<ArgumentException>(() => c.PointAt(1));
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(3));
    }

    [Fact]
    public void MalformedInputCannotBeSkippedByRetrying()
    {
        using var c = Open(new Point(0, 1), new Point(0, 2), Segment.Constant(0, 1, 1));
        Assert.Throws<ArgumentException>(() => c.MoveNext());
        Assert.False(c.MoveNext());
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(0));
    }

    [Fact]
    public void MalformedInputFoundByLookupClosesTheCursor()
    {
        using var c = Open(new Point(0, 1), new Point(0, 2), Segment.Constant(0, 1, 1));
        Assert.Throws<ArgumentException>(() => c.SegmentAfter(0));
        Assert.False(c.MoveNext());
    }

    [Fact]
    public void UnreadInvalidTailIsNotValidated()
    {
        using var c = Open(new Point(0, 1), new Point(0, 2));
        Assert.Equal(1, c.PointAt(0).Value);
    }

    [Fact]
    public void SourceIsOpenedOnceAndDisposedOnce()
    {
        var source = new Source();
        var c = new ElementCursor(source);
        Assert.True(c.MoveNext());
        Assert.Equal(1, c.PointAt(0).Value);
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(1));
        var pulls = source.Pulls;
        Assert.Throws<AfterRightEndpointException>(() => c.PointAt(2));
        Assert.Equal(pulls, source.Pulls);
        c.Dispose();
        c.Dispose();
        Assert.Equal(1, source.Opens);
        Assert.Equal(1, source.Disposals);
    }

    private sealed class Source : IEnumerable<Element>, IEnumerator<Element>
    {
        public int Opens;
        public int Pulls;
        public int Disposals;
        public Element Current => new Point(0, 1);
        object IEnumerator.Current => Current;
        public IEnumerator<Element> GetEnumerator()
        {
            if (++Opens > 1)
                throw new Exception("re-enumerated");
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public bool MoveNext() => ++Pulls == 1;
        public void Dispose() => Disposals++;
        public void Reset() => throw new NotSupportedException();
    }
}
