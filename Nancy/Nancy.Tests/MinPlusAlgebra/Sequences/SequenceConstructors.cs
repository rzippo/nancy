using System;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceConstructors
{
    private Element[] missingSegment = new Element[]
    {
        new Point(
            time: 0,
            value: 20
        ),
        new Point(
            time: 4,
            value: 15
        ),
        new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 15,
            slope: 3
        )
    };

    private Element[] missingPoint = new Element[]
    {
        new Point(
            time: 0,
            value: 20
        ),
        new Segment(
            startTime: 0,
            endTime: 4,
            rightLimitAtStartTime: 20,
            slope: new Rational(-5, 4)
        ),
        new Segment(
            startTime: 4,
            endTime: 6,
            rightLimitAtStartTime: 15,
            slope: 3
        )
    };

    [Fact]
    public void InterruptedSequenceSegment()
    {
        Assert.Throws<ArgumentException>(() => new Sequence(missingSegment));
    }

    [Fact]
    public void InterruptedSequencePoint()
    {
        Assert.Throws<ArgumentException>(() => new Sequence(missingPoint));
    }

    [Fact]
    public void FillSegment()
    {
        Sequence filledSequence = new Sequence(
            missingSegment, 
            fillFrom: 0, 
            fillTo: missingSegment.Last().EndTime
        );

        //note: add sampling tests
    }

    [Fact]
    public void FillPoint()
    {
        Sequence filledSequence = new Sequence(
            missingPoint,
            fillFrom: 0,
            fillTo: missingSegment.Last().EndTime
        );

        //note: add sampling tests
    }

    [Fact]
    public void FillOverEnd()
    {
        Sequence sequence = TestFunctions.SequenceA;
        var elements = sequence.Elements;
        Sequence extendendSequence = new Sequence(
            elements,
            fillFrom: 0,
            fillTo: sequence.DefinedUntil + 5
        );

        //note: add sampling tests
    }
}