using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences;

public class Constructors
{
    public static List<List<Element>> UninterruptedSequences =
    [
        [
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
        ],
        // just a segment
        [
            new Segment(
                startTime: 0,
                endTime: 4,
                rightLimitAtStartTime: 20,
                slope: new Rational(-5, 4)
            ),
        ],
        // just a point
        [
            new Point(
                time: 4,
                value: 15
            )
        ]
    ];
    
    public static IEnumerable<object[]> UninterruptedSequenceTestCases =>
        UninterruptedSequences.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(UninterruptedSequenceTestCases))]
    public void UninterruptedSequence_New(List<Element> elements)
    {
        var sequence = new Sequence(elements);
        // todo: add sampling asserts
    }
    
    [Theory]
    [MemberData(nameof(UninterruptedSequenceTestCases))]
    public void UninterruptedSequence_ToSequence(List<Element> elements)
    {
        var sequence = elements.ToSequence();
        // todo: add sampling asserts
    }
    
    public static List<List<Element>> InterruptedSequences =
    [
        // missing segment
        [
            new Point(
                time: 0,
                value: 20
            ),
            // missing here
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
        ],
        // missing point
        [
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
            // missing here
            new Segment(
                startTime: 4,
                endTime: 6,
                rightLimitAtStartTime: 15,
                slope: 3
            )
        ]
    ];

    public static IEnumerable<object[]> InterruptedSequenceTestCases =>
        InterruptedSequences.ToXUnitTestCases();
    
    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_New(List<Element> elements)
    {
        Assert.Throws<ArgumentException>(() => new Sequence(elements));
    }
    
    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_ToSequence(List<Element> elements)
    {
        Assert.Throws<ArgumentException>(elements.ToSequence);
    }

    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_New_Fill(List<Element> elements)
    {
        var sequence = new Sequence(
            elements,
            fillFrom: elements.First().StartTime,
            fillTo: elements.Last().EndTime
        );
    }
    
    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_ToSequence_Fill(List<Element> elements)
    {
        var sequence = elements.ToSequence(
            fillFrom: elements.First().StartTime,
            fillTo: elements.Last().EndTime
        );
    }

    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_New_Fill_OverEnd(List<Element> elements)
    {
        var sequence = new Sequence(
            elements,
            fillFrom: elements.First().StartTime,
            fillTo: elements.Last().EndTime + 5
        );
    }
    
    [Theory]
    [MemberData(nameof(InterruptedSequenceTestCases))]
    public void InterruptedSequence_ToSequence_Fill_OverEnd(List<Element> elements)
    {
        var sequence = elements.ToSequence(
            fillFrom: elements.First().StartTime,
            fillTo: elements.Last().EndTime + 5
        );
    }

    public static List<(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)> ConstructorIntervals =
    [
        // normal case, start < end
        (1, 10, false, false),
        (1, 10, false, true),
        (1, 10, true, false),
        (1, 10, true, true),
        // start = end
        (5, 5, false, false),
        (5, 5, false, true),
        (5, 5, true, false),
        (5, 5, true, true),
        // start > end
        (10, 5, false, false),
        (10, 5, false, true),
        (10, 5, true, false),
        (10, 5, true, true),
    ];
    
    public static IEnumerable<object[]> ConstructorIntervalsTestCases()
        => ConstructorIntervals.ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(ConstructorIntervalsTestCases))]
    public void ZeroConstructor(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        if (start > end)
        {
            // start must be <= end
            Assert.Throws<ArgumentException>(() => Sequence.Zero(start, end, isStartIncluded, isEndIncluded));
        }
        else if (start == end && !(isStartIncluded && isEndIncluded))
        {
            // if start == end, both must be included
            Assert.Throws<ArgumentException>(() => Sequence.Zero(start, end, isStartIncluded, isEndIncluded));
        }
        else
        {
            var sequence = Sequence.Zero(start, end, isStartIncluded, isEndIncluded);
            if (start < end)
                Assert.Equal(Rational.Zero, sequence.RightLimitAt(start));
            if (isStartIncluded)
                Assert.Equal(Rational.Zero, sequence.ValueAt(start));
            if (start < end)
                Assert.Equal(Rational.Zero, sequence.LeftLimitAt(end));
            if (isEndIncluded)
                Assert.Equal(Rational.Zero, sequence.ValueAt(end));
        }
    }
    
    [Theory]
    [MemberData(nameof(ConstructorIntervalsTestCases))]
    public void ConstantConstructor(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        var value = 5;
        if (start > end)
        {
            // start must be <= end
            Assert.Throws<ArgumentException>(() => Sequence.Constant(value, start, end, isStartIncluded, isEndIncluded));
        }
        else if (start == end && !(isStartIncluded && isEndIncluded))
        {
            // if start == end, both must be included
            Assert.Throws<ArgumentException>(() => Sequence.Constant(value, start, end, isStartIncluded, isEndIncluded));
        }
        else
        {
            var sequence = Sequence.Constant(value, start, end, isStartIncluded, isEndIncluded);
            if (start < end)
                Assert.Equal(value, sequence.RightLimitAt(start));
            if (isStartIncluded)
                Assert.Equal(value, sequence.ValueAt(start));
            if (start < end)
                Assert.Equal(value, sequence.LeftLimitAt(end));
            if (isEndIncluded)
                Assert.Equal(value, sequence.ValueAt(end));
        }
    }
    
    [Theory]
    [MemberData(nameof(ConstructorIntervalsTestCases))]
    public void PlusInfiniteConstructor(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        if (start > end)
        {
            // start must be <= end
            Assert.Throws<ArgumentException>(() => Sequence.PlusInfinite(start, end, isStartIncluded, isEndIncluded));
        }
        else if (start == end && !(isStartIncluded && isEndIncluded))
        {
            // if start == end, both must be included
            Assert.Throws<ArgumentException>(() => Sequence.PlusInfinite(start, end, isStartIncluded, isEndIncluded));
        }
        else
        {
            var sequence = Sequence.PlusInfinite(start, end, isStartIncluded, isEndIncluded);
            if (start < end)    
                Assert.Equal(Rational.PlusInfinity, sequence.RightLimitAt(start));
            if (isStartIncluded)
                Assert.Equal(Rational.PlusInfinity, sequence.ValueAt(start));
            if (start < end)    
                Assert.Equal(Rational.PlusInfinity, sequence.LeftLimitAt(end));
            if (isEndIncluded)
                Assert.Equal(Rational.PlusInfinity, sequence.ValueAt(end));
        }
    }
    
    [Theory]
    [MemberData(nameof(ConstructorIntervalsTestCases))]
    public void MinusInfiniteConstructor(Rational start, Rational end, bool isStartIncluded, bool isEndIncluded)
    {
        if (start > end)
        {
            // start must be <= end
            Assert.Throws<ArgumentException>(() => Sequence.MinusInfinite(start, end, isStartIncluded, isEndIncluded));
        }
        else if (start == end && !(isStartIncluded && isEndIncluded))
        {
            // if start == end, both must be included
            Assert.Throws<ArgumentException>(() => Sequence.MinusInfinite(start, end, isStartIncluded, isEndIncluded));
        }
        else
        {
            var sequence = Sequence.MinusInfinite(start, end, isStartIncluded, isEndIncluded);
            if (start < end)
                Assert.Equal(Rational.MinusInfinity, sequence.RightLimitAt(start));
            if (isStartIncluded)
                Assert.Equal(Rational.MinusInfinity, sequence.ValueAt(start));
            if (start < end)
                Assert.Equal(Rational.MinusInfinity, sequence.LeftLimitAt(end));
            if (isEndIncluded)
                Assert.Equal(Rational.MinusInfinity, sequence.ValueAt(end));
        }
    }
}