using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class SequenceEquals
{
    [Fact]
    public void SameSequences()
    {
        Sequence s1 = TestFunctions.SequenceA;
        var elements = new List<Element>(TestFunctions.SequenceA.Elements);
        Sequence s2 = new Sequence(elements);

        Assert.True(s1.Equals(s2));
    }

    [Fact]
    public void SameSequences_DeepClone()
    {
        Sequence s1 = TestFunctions.SequenceA;
        var elements = TestFunctions.SequenceA.Elements
            .Select(Clone);
        Sequence s2 = new Sequence(elements);

        Assert.True(s1.Equals(s2));

        Element Clone(Element e)
        {
            switch (e)
            {
                case Point p:
                    return new Point(
                        time: p.Time,
                        value: p.Value
                    );

                case Segment s:
                    return new Segment(
                        startTime: s.StartTime,
                        endTime: s.EndTime,
                        rightLimitAtStartTime: s.RightLimitAtStartTime,
                        slope: s.Slope
                    );

                default:
                    throw new InvalidCastException();
            }
        }
    }

    [Fact]
    public void DifferentSequences()
    {
        Sequence s1 = TestFunctions.SequenceA;
        Sequence s2 = TestFunctions.SequenceB;

        Assert.False(s1.Equals(s2));
    }

    [Fact]
    public void NullObject()
    {
#nullable disable
        Sequence s1 = TestFunctions.SequenceA;
        object o = null;

        Assert.False(s1.Equals(o));
#nullable restore
    }

    [Fact]
    public void TypeMismatch()
    {
        Sequence s = TestFunctions.SequenceA;
        Point p = new Point(5, 3);

        Assert.False(s.Equals(p));
    }
}