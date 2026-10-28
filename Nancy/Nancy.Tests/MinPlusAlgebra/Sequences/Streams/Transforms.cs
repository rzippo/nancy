using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class Transforms
{
    public static IEnumerable<object[]> Inputs() => Properties.ElementCases().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(Inputs))]
    public void TransformationsMatchTheSequenceOperations(Element[] elements)
    {
        var sequence = new Sequence(elements);
        void Check(Sequence expected, IEnumerable<Element> actual)
            => Assert.True(Sequence.Equivalent(expected, actual.MergeAsEnumerable()
                .ToSequence()));
        Check(sequence.Negate(), elements.Negate());
        Check(sequence.Scale(-2), elements.Scale(-2));
        Check(sequence.VerticalShift(3), elements.VerticalShift(3));
        Check(sequence.VerticalShift(3, true), elements.VerticalShift(3, true));
        Check(sequence.Delay(2), elements.Delay(2));
        Check(sequence.HorizontalShift(2), elements.HorizontalShift(2));
        Check(sequence.Delay(2, false), elements.Delay(2, false));
        if (sequence.DefinedUntil > 1)
        {
            Check(sequence.Forward(1), elements.Forward(1));
            Check(sequence.HorizontalShift(-1), elements.HorizontalShift(-1));
        }
        if (sequence.IsFinite)
        {
            Check(sequence.Floor(), elements.Floor());
            Check(sequence.Ceil(), elements.Ceil());
        }
        Check(sequence.ToNonNegative(), elements.ToNonNegative());
    }

    [Fact]
    public void UnaryPipelinesConsumeAFinitePrefix()
    {
        var pulls = 0;
        IEnumerable<Element> Source()
        {
            for (var i = 0; i < 10; i++)
            {
                pulls++; yield return new Point(i, i);
                pulls++; yield return new Segment(i, i + 1, i, 1);
            }
            throw new InvalidOperationException("The stream was read past the prefix the operation needs.");
        }
        var actual = Source().Scale(2)
            .VerticalShift(-1)
            .ToNonNegative()
            .Floor()
            .Take(5)
            .ToList();
        Assert.Equal(5, actual.Count);
        Assert.InRange(pulls, 1, 6);
    }

    [Fact]
    public void InvalidTimeParametersAreRejected()
    {
        Element[] elements = [new Point(0, 0)];
        Assert.Throws<ArgumentException>(() => elements.Delay(-1));
        Assert.Throws<ArgumentException>(() => elements.Forward(Rational.PlusInfinity));
        Assert.Throws<ArgumentException>(() => elements.HorizontalShift(Rational.MinusInfinity));
    }

    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void ZeroPrefixedShiftsRejectMalformedInputs(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Delay(1)
            .ToList());
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).HorizontalShift(1)
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void NegationRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Negate()
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void ScalingRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Scale(2)
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void VerticalShiftRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).VerticalShift(1)
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void FloorRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Floor()
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void CeilingRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Ceil()
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void NonNegativeProjectionRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).ToNonNegative()
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void AdvanceRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Forward(1)
            .ToList());
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void DelayWithoutZeroPrefixRejectsMalformedInput(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).Delay(1, false)
            .ToList());
    }

    [Fact]
    public void DelayTraversesOnceAndStopsWithItsConsumer()
    {
        Assert.NotEmpty(ValidationCases.SingleUse(ValidationCases.ValidInput()).Delay(1)
            .ToList());
        Element[] elements = [new Point(0, 0), new Point(-1, 1)];
        Assert.Single(ValidationCases.SingleUse(elements).Delay(1)
            .Take(1));
    }
}
