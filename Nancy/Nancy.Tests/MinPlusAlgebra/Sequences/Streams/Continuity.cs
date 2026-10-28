using System;
using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class Continuity
{
    [Fact]
    public void LeftContinuityStopsAtTheFirstMismatchingPoint()
    {
        Assert.False(Input().IsLeftContinuous());

        IEnumerable<Element> Input()
        {
            yield return Segment.Zero(0, 1);
            yield return new Point(1, 1);
            throw new InvalidOperationException("The answer is already determined.");
        }
    }

    [Fact]
    public void RightContinuityStopsAtTheFirstMismatchingSegment()
    {
        Assert.False(Input().IsRightContinuous());

        IEnumerable<Element> Input()
        {
            yield return new Point(0, 1);
            yield return Segment.Zero(0, 1);
            throw new InvalidOperationException("The answer is already determined.");
        }
    }
}
