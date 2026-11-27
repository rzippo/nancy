using System;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class NAryOperandNames
{
    public static TheoryData<string[]> MismatchedNames => new()
    {
        Array.Empty<string>(),
        new[] { "a" },
        new[] { "a", "b", "c" }
    };

    [Theory]
    [MemberData(nameof(MismatchedNames))]
    public void CurveOperandsRequireOneNameEach(string[] names)
    {
        var operands = new Curve[] { new ConstantCurve(1), new ConstantCurve(2) };

        var exception = Assert.Throws<ArgumentException>("names", () => new AdditionExpression(operands, names));
        Assert.StartsWith("The number of names must equal the number of operands.", exception.Message);
    }

    [Fact]
    public void CurveOperandsKeepTheirNamesAndOrder()
    {
        var operands = new Curve[] { new ConstantCurve(1), new ConstantCurve(2) };
        var expression = new AdditionExpression(operands, new[] { "a", "b" });

        Assert.Collection(expression.Operands,
            first => { Assert.Equal("a", first.Name); Assert.Equal(operands[0], first.Value); },
            second => { Assert.Equal("b", second.Name); Assert.Equal(operands[1], second.Value); });
    }

    [Theory]
    [MemberData(nameof(MismatchedNames))]
    public void SequenceOperandsRequireOneNameEach(string[] names)
    {
        var operands = new Sequence[] { Sequence.Constant(1, 0, 1), Sequence.Constant(2, 0, 1) };

        var exception = Assert.Throws<ArgumentException>("names", () => new SequenceAdditionExpression(operands, names));
        Assert.StartsWith("The number of names must equal the number of operands.", exception.Message);
    }

    [Fact]
    public void SequenceOperandsKeepTheirNamesAndOrder()
    {
        var operands = new Sequence[] { Sequence.Constant(1, 0, 1), Sequence.Constant(2, 0, 1) };
        var expression = new SequenceAdditionExpression(operands, new[] { "a", "b" });

        Assert.Collection(expression.Operands,
            first => { Assert.Equal("a", first.Name); Assert.Equal(operands[0], first.Value); },
            second => { Assert.Equal("b", second.Name); Assert.Equal(operands[1], second.Value); });
    }

    [Theory]
    [MemberData(nameof(MismatchedNames))]
    public void RationalOperandsRequireOneNameEach(string[] names)
    {
        var operands = new Rational[] { (Rational)1, (Rational)2 };

        var exception = Assert.Throws<ArgumentException>("names", () => new RationalAdditionExpression(operands, names));
        Assert.StartsWith("The number of names must equal the number of operands.", exception.Message);
    }

    [Fact]
    public void RationalOperandsKeepTheirNamesAndOrder()
    {
        var operands = new Rational[] { (Rational)1, (Rational)2 };
        var expression = new RationalAdditionExpression(operands, new[] { "a", "b" });

        Assert.Collection(expression.Operands,
            first => { Assert.Equal("a", first.Name); Assert.Equal(operands[0], first.Value); },
            second => { Assert.Equal("b", second.Name); Assert.Equal(operands[1], second.Value); });
    }

}
