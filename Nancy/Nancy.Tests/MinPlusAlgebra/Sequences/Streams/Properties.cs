using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

/// <summary>
/// Checks bounded-state helpers against materialized definitions, including unattained extrema and endpoint projections.
/// </summary>
public class Properties
{
    public static IEnumerable<Element[]> ElementCases()
    {
        foreach (var startIncluded in new[] { false, true })
        foreach (var endIncluded in new[] { false, true })
        foreach (var slope in new[] { -2, 0, 2 })
        foreach (var jump in new[] { -3, 0, 3 })
        {
            var elements = new List<Element>();
            if (startIncluded) elements.Add(new Point(0, 1));
            elements.Add(new Segment(0, 2, 1, slope));
            elements.Add(new Point(2, 1 + 2 * slope + jump));
            elements.Add(new Segment(2, 4, 1 + 2 * slope + jump, slope));
            if (endIncluded) elements.Add(new Point(4, 1 + 4 * slope + jump));
            yield return elements.ToArray();
        }
        yield return new Element[] { new Point(2, -1) };
        yield return new Element[] { new Segment(0, 2, 0, 1) };
        yield return new Element[] { Segment.Constant(0, 2, 3) };
        yield return new Element[] { Point.PlusInfinite(0), Segment.PlusInfinite(0, 2), Point.PlusInfinite(2) };
        yield return new Element[] { Point.MinusInfinite(0), Segment.MinusInfinite(0, 2) };
        yield return new Element[] { new Point(0, 0), Segment.PlusInfinite(0, 1), new Point(1, 2), Segment.MinusInfinite(1, 2), new Point(2, 0) };
        yield return new Element[] { new Segment(0, 1, 1, -1), new Point(1, 0), Segment.Constant(1, 2, 0) };
    }

    public static IEnumerable<object[]> Inputs() => ElementCases().ToXUnitTestCases();

    private static IEnumerable<Element> SingleUse(Element[] elements)
    {
        var used = false;
        return Enumerate();
        IEnumerable<Element> Enumerate()
        {
            if (used) throw new InvalidOperationException("The source was enumerated twice.");
            used = true;
            foreach (var element in elements) yield return element;
        }
    }

    private static bool ReferenceNonDecreasing(Element[] elements)
    {
        if (elements.OfType<Segment>()
            .Any(s => s.Slope < 0))
            return false;
        return elements.EnumerateBreakpoints()
            .All(bp =>
                (bp.left is null || bp.left.LeftLimitAtEndTime <= bp.center.Value)
                && (bp.right is null || bp.center.Value <= bp.right.RightLimitAtStartTime));
    }

    private static bool ReferenceIncreasing(Element[] elements)
    {
        if (elements.OfType<Segment>()
            .Any(s => s.Slope <= 0))
            return false;
        return elements.EnumerateBreakpoints()
            .All(bp =>
                (bp.left is null || bp.left.LeftLimitAtEndTime <= bp.center.Value)
                && (bp.right is null || bp.center.Value <= bp.right.RightLimitAtStartTime));
    }

    private static IEnumerable<Element> ReferenceLeftContinuity(Element[] elements)
    {
        if (elements is [Segment lone]) return [lone];
        var result = new List<Element>();
        var first = true;
        foreach (var (before, point, after) in elements.EnumerateBreakpoints())
        {
            if (first && before is not null) result.Add(before);
            var value = before?.LeftLimitAtEndTime;
            result.Add(value is { } v ? new Point(point.Time, v) : point);
            if (after is not null) result.Add(after);
            first = false;
        }
        return result;
    }

    private static IEnumerable<Element> ReferenceRightContinuity(Element[] elements)
    {
        if (elements is [Segment lone]) return [lone];
        var result = new List<Element>();
        var first = true;
        foreach (var (before, point, after) in elements.EnumerateBreakpoints())
        {
            if (first && before is not null) result.Add(before);
            var value = after?.RightLimitAtStartTime;
            result.Add(value is { } v ? new Point(point.Time, v) : point);
            if (after is not null) result.Add(after);
            first = false;
        }
        return result;
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void NonDecreasingMatchesTheMaterializedDefinition(Element[] elements)
    {
        Assert.Equal(ReferenceNonDecreasing(elements), SingleUse(elements).IsNonDecreasing());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void IncreasingMatchesTheMaterializedDefinition(Element[] elements)
    {
        Assert.Equal(ReferenceIncreasing(elements), SingleUse(elements).IsIncreasing());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void NonNegativityMatchesTheMaterializedDefinition(Element[] elements)
    {
        Assert.Equal(elements.GetElementsBoundaryValues()
            .Min() >= 0, SingleUse(elements).IsNonNegative());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void LeftContinuityMatchesTheMaterializedDefinition(Element[] elements)
    {
        Assert.Equal(ReferenceLeftContinuity(elements), SingleUse(elements).ToLeftContinuous());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void RightContinuityMatchesTheMaterializedDefinition(Element[] elements)
    {
        Assert.Equal(ReferenceRightContinuity(elements), SingleUse(elements).ToRightContinuous());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void MaxExtremumMatchesTheMaterializedDefinition(Element[] elements)
    {
        var value = elements.GetElementsBoundaryValues()
            .Max();
        var attaining = elements.FirstOrDefault(e => e.AttainsValue(value));
        Rational? expectedValue = attaining is null ? null : value;
        Assert.Equal(expectedValue, SingleUse(elements).MaxValue());
        AssertAttainingArgument(elements, expectedValue, SingleUse(elements).MaxArg());
        IReadOnlyCollection<Element> collection = elements;
        Assert.Equal(expectedValue, collection.MaxValue());
        AssertAttainingArgument(elements, expectedValue, collection.MaxArg());
    }

    [Theory]
    [MemberData(nameof(Inputs))]
    public void MinExtremumMatchesTheMaterializedDefinition(Element[] elements)
    {
        var value = elements.GetElementsBoundaryValues()
            .Min();
        var attaining = elements.FirstOrDefault(e => e.AttainsValue(value));
        Rational? expectedValue = attaining is null ? null : value;
        Assert.Equal(expectedValue, SingleUse(elements).MinValue());
        AssertAttainingArgument(elements, expectedValue, SingleUse(elements).MinArg());
        IReadOnlyCollection<Element> collection = elements;
        Assert.Equal(expectedValue, collection.MinValue());
        AssertAttainingArgument(elements, expectedValue, collection.MinArg());
    }

    private static void AssertAttainingArgument(Element[] elements, Rational? value, Rational? argument)
    {
        if (value is null)
        {
            Assert.Null(argument);
            return;
        }

        Assert.NotNull(argument);
        Assert.Contains(elements, element => element.IsDefinedFor(argument.Value)
            && element.ValueAt(argument.Value) == value.Value);
    }

    [Fact]
    public void EmptyInputsThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().IsNonDecreasing());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().IsIncreasing());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().ToLeftContinuous()
            .ToList());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().ToRightContinuous()
            .ToList());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().IsNonNegative());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().MaxValue());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().MaxArg());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().MinValue());
        Assert.Throws<ArgumentException>(() => Enumerable.Empty<Element>().MinArg());
    }

    [Fact]
    public void NonNegativityStopsAtTheFirstViolation()
    {
        var disposed = false;
        IEnumerable<Element> Source()
        {
            try
            {
                yield return new Point(0, -1);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed = true; }
        }
        var result = Source().IsNonNegative();
        Assert.False(result);
        Assert.True(disposed);
    }

    [Fact]
    public void NonDecreasingStopsAtTheFirstViolation()
    {
        var disposed = false;
        IEnumerable<Element> Source()
        {
            try
            {
                yield return new Segment(0, 1, 0, -1);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed = true; }
        }
        var result = Source().IsNonDecreasing();
        Assert.False(result);
        Assert.True(disposed);
    }

    [Fact]
    public void IncreasingStopsAtTheFirstViolation()
    {
        var disposed = false;
        IEnumerable<Element> Source()
        {
            try
            {
                yield return new Segment(0, 1, 0, -1);
                throw new InvalidOperationException("The tail must not be read.");
            }
            finally { disposed = true; }
        }
        var result = Source().IsIncreasing();
        Assert.False(result);
        Assert.True(disposed);
    }

    [Fact]
    public void LeftContinuityProjectionConsumesOnlyAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Source()
        {
            for (var i = 0; i < 10; i++)
            {
                pulls++; yield return new Point(i, i + 1);
                pulls++; yield return Segment.Constant(i, i + 1, i);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        var result = (Source().ToLeftContinuous())
            .Take(5)
            .ToList();
        Assert.Equal(5, result.Count);
        Assert.InRange(pulls, 5, 7);
    }

    [Fact]
    public void RightContinuityProjectionConsumesOnlyAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Source()
        {
            for (var i = 0; i < 10; i++)
            {
                pulls++; yield return new Point(i, i + 1);
                pulls++; yield return Segment.Constant(i, i + 1, i);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        var result = (Source().ToRightContinuous())
            .Take(5)
            .ToList();
        Assert.Equal(5, result.Count);
        Assert.InRange(pulls, 5, 7);
    }
}
