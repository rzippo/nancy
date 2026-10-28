using System;
using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Tests.MinPlusAlgebra.Sequences.Streams;

public class HelperValidation
{
    public static IEnumerable<object[]> InvalidInputs() => ValidationCases.InvalidInputs().ToXUnitTestCases();

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void OrderDependentHelpersRejectMalformedInputs(Element[] elements)
    {
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsNonDecreasing());
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).IsIncreasing());
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).ToLeftContinuous()
            .ToList());
        Assert.Throws<ArgumentException>(() => ValidationCases.SingleUse(elements).ToRightContinuous()
            .ToList());
    }

    [Fact]
    public void HelpersEnumerateTheirInputOnce()
    {
        var elements = ValidationCases.ValidInput();
        Assert.True(ValidationCases.SingleUse(elements).IsIncreasing());
        Assert.True(ValidationCases.SingleUse(elements).IsNonDecreasing());
        Assert.True(ValidationCases.SingleUse(elements).IsNonNegative());
        Assert.NotEmpty(ValidationCases.SingleUse(elements).ToLeftContinuous()
            .ToList());
        Assert.NotEmpty(ValidationCases.SingleUse(elements).ToRightContinuous()
            .ToList());
        Assert.Equal(2, ValidationCases.SingleUse(elements).MaxValue());
        Assert.Equal(0, ValidationCases.SingleUse(elements).MinValue());
    }

    [Fact]
    public void NonNegativityAndExtremeValuesDoNotRequireTimeOrder()
    {
        Element[] elements = [new Point(2, 3), new Segment(0, 1, 1, 1), new Point(1, 0)];
        Assert.True(ValidationCases.SingleUse(elements).IsNonNegative());
        Assert.Equal(3, ValidationCases.SingleUse(elements).MaxValue());
        Assert.Equal(0, ValidationCases.SingleUse(elements).MinValue());
    }

    [Fact]
    public void DecisivePropertiesDoNotReadAnInvalidTail()
    {
        Element[] elements = [new Segment(0, 1, -1, -1), new Point(-2, 0)];
        Assert.False(ValidationCases.SingleUse(elements).IsNonNegative());
        Assert.False(ValidationCases.SingleUse(elements).IsNonDecreasing());
        Assert.False(ValidationCases.SingleUse(elements).IsIncreasing());
        Assert.Single(ValidationCases.SingleUse(elements).ToLeftContinuous()
            .Take(1));
        Assert.Single(ValidationCases.SingleUse(elements).ToRightContinuous()
            .Take(1));
    }
}
