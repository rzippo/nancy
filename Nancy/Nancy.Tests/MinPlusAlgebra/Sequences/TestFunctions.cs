using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public static class TestFunctions
{
    public static readonly Sequence SequenceA = new Sequence(new Element[]
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
    });

    public static readonly Sequence SequenceB = new Sequence(new Element[]
    {
        new Point(
            time: 0,
            value: 5
        ), 
        new Segment(
            startTime: 0,
            endTime: 3,
            rightLimitAtStartTime: 5,
            slope: 5
        ),
        new Point(
            time: 3,
            value: 20
        ), 
        new Segment(
            startTime: 3,
            endTime: 7,
            rightLimitAtStartTime: 20,
            slope: -5
        ),
        Point.Zero(7),
        Segment.Zero(
            startTime:7,
            endTime: 8
        ) 
    });
}