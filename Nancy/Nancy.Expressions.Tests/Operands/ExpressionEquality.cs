using System.Collections.Generic;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class ExpressionEquality
{
    private static readonly Curve CurveA = new RateLatencyServiceCurve(rate: 2, latency: 1);
    private static readonly Curve CurveB = new RateLatencyServiceCurve(rate: 3, latency: 1);

    // Name/Generation/Settings do not participate in identity, at each arity.

    [Fact]
    public void UnaryExpressionsDifferingOnlyInNameAreEqual()
    {
        CurveExpression a = new NegateExpression(CurveA, "a", "x");
        CurveExpression b = new NegateExpression(CurveA, "a", "y");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void BinaryExpressionsDifferingOnlyInGenerationAreEqual()
    {
        CurveExpression a = new SubtractionExpression(CurveA, "a", CurveB, "b").WithGeneration(1);
        CurveExpression b = new SubtractionExpression(CurveA, "a", CurveB, "b").WithGeneration(2);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void NAryExpressionsDifferingOnlyInSettingsAreEqual()
    {
        var settingsA = new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheSegmentThreshold = 5 } };
        var settingsB = new ExpressionSettings { CacheSettings = new CacheSettings { CheapCacheSegmentThreshold = 50 } };
        CurveExpression a = new AdditionExpression([CurveA.ToExpression("a"), CurveB.ToExpression("b")], "s", settingsA);
        CurveExpression b = new AdditionExpression([CurveA.ToExpression("a"), CurveB.ToExpression("b")], "s", settingsB);

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // Every associative-commutative operator: operand order does not matter.

    public static IEnumerable<object[]> AssociativeCommutativeOperators()
    {
        var a = CurveA.ToExpression("a");
        var b = CurveB.ToExpression("b");
        yield return new object[] { new AdditionExpression([a, b]), new AdditionExpression([b, a]) };
        yield return new object[] { new ConvolutionExpression([a, b]), new ConvolutionExpression([b, a]) };
        yield return new object[] { new MaxPlusConvolutionExpression([a, b]), new MaxPlusConvolutionExpression([b, a]) };
        yield return new object[] { new MaximumExpression([a, b]), new MaximumExpression([b, a]) };
        yield return new object[] { new MinimumExpression([a, b]), new MinimumExpression([b, a]) };

        var ra = new Rational(1, 2).ToExpression("ra");
        var rb = new Rational(1, 3).ToExpression("rb");
        yield return new object[] { new RationalAdditionExpression([ra, rb]), new RationalAdditionExpression([rb, ra]) };
        yield return new object[] { new RationalProductExpression([ra, rb]), new RationalProductExpression([rb, ra]) };
        yield return new object[] { new RationalMaximumExpression([ra, rb]), new RationalMaximumExpression([rb, ra]) };
        yield return new object[] { new RationalMinimumExpression([ra, rb]), new RationalMinimumExpression([rb, ra]) };
        yield return new object[] { new RationalGreatestCommonDivisorExpression([ra, rb]), new RationalGreatestCommonDivisorExpression([rb, ra]) };
        yield return new object[] { new RationalLeastCommonMultipleExpression([ra, rb]), new RationalLeastCommonMultipleExpression([rb, ra]) };
    }

    [Theory]
    [MemberData(nameof(AssociativeCommutativeOperators))]
    public void SwappingOperandOrderOfAnAssociativeCommutativeOperatorIsEqual(object forward, object reversed)
    {
        Assert.Equal(forward, reversed);
        Assert.Equal(forward.GetHashCode(), reversed.GetHashCode());
    }

    // Binary operators are never commutative here: swapping operands must not be equal.

    public static IEnumerable<object[]> BinaryOperators()
    {
        yield return new object[] { new SubtractionExpression(CurveA, "a", CurveB, "b"), new SubtractionExpression(CurveB, "b", CurveA, "a") };
        yield return new object[] { new DeconvolutionExpression(CurveA, "a", CurveB, "b"), new DeconvolutionExpression(CurveB, "b", CurveA, "a") };
        yield return new object[] { new MaxPlusDeconvolutionExpression(CurveA, "a", CurveB, "b"), new MaxPlusDeconvolutionExpression(CurveB, "b", CurveA, "a") };
        yield return new object[] { new CompositionExpression(CurveA, "a", CurveB, "b"), new CompositionExpression(CurveB, "b", CurveA, "a") };
    }

    [Theory]
    [MemberData(nameof(BinaryOperators))]
    public void SwappingOperandOrderOfABinaryOperatorIsNotEqual(object forward, object reversed)
    {
        Assert.NotEqual(forward, reversed);
    }

    // A concrete leaf's hash must not collide with the bare value it wraps.

    [Fact]
    public void ConcreteCurveExpressionHashDiffersFromTheBareCurvesHash()
        => Assert.NotEqual(CurveA.GetHashCode(), new ConcreteCurveExpression(CurveA, "a").GetHashCode());

    [Fact]
    public void RationalNumberExpressionHashDiffersFromTheBareRationalsHash()
    {
        var value = new Rational(1, 2);
        Assert.NotEqual(value.GetHashCode(), new RationalNumberExpression(value).GetHashCode());
    }

    // Placeholders are the one exception: name is their entire identity, they having no operands or computable value.

    [Fact]
    public void CurvePlaceholdersWithDifferentNamesAreNotEqual()
    {
        var a = new CurvePlaceholderExpression("a");
        var b = new CurvePlaceholderExpression("b");

        Assert.NotEqual(a, b);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void CurvePlaceholdersWithTheSameNameAreEqual()
    {
        var a = new CurvePlaceholderExpression("a");
        var b = new CurvePlaceholderExpression("a");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void RationalPlaceholdersWithDifferentNamesAreNotEqual()
    {
        var a = new RationalPlaceholderExpression("a");
        var b = new RationalPlaceholderExpression("b");

        Assert.NotEqual(a, b);
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    // Regression guard for the multiset-vs-positional-comparison defect.
    // A hash-sorted positional comparison gets this wrong when two distinct operands collide.
    // CollidingLeaf forces a collision, which is cheaper than searching for one under the real hash function.
    private record CollidingLeaf : ConcreteCurveExpression
    {
        public CollidingLeaf(Curve curve, string name) : base(curve, name) { }
        public override int GetHashCode() => 42;
    }

    [Fact]
    public void OperandsWithACollidingHashAreStillComparedCorrectly()
    {
        var x = new CollidingLeaf(CurveA, "x");
        var y = new CollidingLeaf(CurveB, "y");
        Assert.Equal(42, x.GetHashCode());
        Assert.Equal(42, y.GetHashCode());
        Assert.NotEqual(x, y); // sanity: they really are different despite the forced collision

        var forward = new AdditionExpression([x, y], "s");
        var reversed = new AdditionExpression([y, x], "s");

        // Same multiset {x, y} in a different insertion order, so they must be equal despite the collision.
        // A "sort both lists by hash, compare positionally" implementation cannot reliably guarantee that here.
        Assert.Equal(forward, reversed);
        Assert.Equal(forward.GetHashCode(), reversed.GetHashCode());
    }

    [Fact]
    public void OperandsWithACollidingHashDoNotProduceAFalsePositive()
    {
        var x = new CollidingLeaf(CurveA, "x");
        var y = new CollidingLeaf(CurveB, "y");
        var z = new CollidingLeaf(new RateLatencyServiceCurve(rate: 7, latency: 1), "z");

        var withY = new AdditionExpression([x, y], "s");
        var withZ = new AdditionExpression([x, z], "s");

        Assert.NotEqual(withY, withZ);
    }

    // A multiset counts each operand, where a set would collapse the repeats and call these equal.

    [Fact]
    public void RepeatedOperandsAreCountedNotCollapsed()
    {
        var a = CurveA.ToExpression("a");
        var b = CurveB.ToExpression("b");

        var twoAs = new AdditionExpression([a, a, b], "s");
        var twoBs = new AdditionExpression([a, b, b], "s");

        Assert.NotEqual(twoAs, twoBs);
    }

    [Fact]
    public void RepeatedOperandsInADifferentOrderAreEqual()
    {
        var a = CurveA.ToExpression("a");
        var b = CurveB.ToExpression("b");

        var forward = new AdditionExpression([a, a, b], "s");
        var reordered = new AdditionExpression([b, a, a], "s");

        Assert.Equal(forward, reordered);
        Assert.Equal(forward.GetHashCode(), reordered.GetHashCode());
    }

    // Identity is operator plus operands, so the operator has to tell these apart on its own.
    // The record system supplies that through EqualityContract, with no explicit type check here.

    [Fact]
    public void DifferentNAryOperatorsOverTheSameOperandsAreNotEqual()
    {
        var a = CurveA.ToExpression("a");
        var b = CurveB.ToExpression("b");

        CurveExpression addition = new AdditionExpression([a, b], "s");
        CurveExpression minimum = new MinimumExpression([a, b], "s");

        Assert.NotEqual(addition, minimum);
        Assert.NotEqual(addition.GetHashCode(), minimum.GetHashCode());
    }

    [Fact]
    public void DifferentBinaryOperatorsOverTheSameOperandsAreNotEqual()
    {
        CurveExpression deconvolution = new DeconvolutionExpression(CurveA, "a", CurveB, "b");
        CurveExpression subtraction = new SubtractionExpression(CurveA, "a", CurveB, "b");

        Assert.NotEqual(deconvolution, subtraction);
        Assert.NotEqual(deconvolution.GetHashCode(), subtraction.GetHashCode());
    }

    [Fact]
    public void DifferentUnaryOperatorsOverTheSameOperandAreNotEqual()
    {
        CurveExpression floor = new FloorExpression(CurveA, "a");
        CurveExpression ceil = new CeilExpression(CurveA, "a");

        Assert.NotEqual(floor, ceil);
        Assert.NotEqual(floor.GetHashCode(), ceil.GetHashCode());
    }
}
