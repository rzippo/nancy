using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

// A node returns a copy of itself with new operands.
// The copy carries the name, generation and settings the caller set, keeps any state beyond the operands, and leaves the computed-value caches behind.
public class ExpressionRebuild
{
    private static readonly ExpressionSettings Settings =
        new() { ComputationSettings = new ComputationSettings { UseParallelism = false } };

    private static CurveExpression Leaf(int value, string name)
        => new ConcreteCurveExpression(new ConstantCurve(value), name);

    private static Sequence SequenceOf(int slope)
        => new([Point.Origin(), new Segment(0, 6, 0, slope), new Point(6, 6 * slope)]);

    [Fact]
    public void ABinaryNodeKeepsItsStateBeyondTheOperands()
    {
        var original = new SubtractionExpression(Leaf(1, "a"), Leaf(2, "b"), "d", Settings)
        {
            NonNegative = true
        };
        original.ComputeWithoutResult();

        var rebuilt = (SubtractionExpression)original.WithOperands(Leaf(3, "c"), Leaf(4, "e"));

        Assert.True(rebuilt.NonNegative);
        Assert.Equal("d", rebuilt.Name);
        Assert.Same(Settings, rebuilt.Settings);
        Assert.False(rebuilt.IsComputed);
        Assert.True(original.IsComputed);
    }

    [Fact]
    public void ASequenceNodeTakesACurveOperandAndKeepsItsInterval()
    {
        var interval = new Interval(0, 5, true, true);
        var original = new CurveCutExpression(Leaf(1, "a"), interval, "cut", Settings);
        original.ComputeWithoutResult();

        var rebuilt = (CurveCutExpression)original.WithOperand(Leaf(2, "b"));

        Assert.Equal(interval, rebuilt.Interval);
        Assert.Equal("cut", rebuilt.Name);
        Assert.Same(Settings, rebuilt.Settings);
        Assert.False(rebuilt.IsComputed);
    }

    [Fact]
    public void ARationalNodeTakesTwoSequenceOperands()
    {
        var original = new SequenceHorizontalDeviationExpression(
            SequenceOf(1), "a", SequenceOf(2), "b", "hdev", Settings);
        original.ComputeWithoutResult();

        var rebuilt = (SequenceHorizontalDeviationExpression)original.WithOperands(
            new ConcreteSequenceExpression(SequenceOf(3), "c"),
            new ConcreteSequenceExpression(SequenceOf(4), "d"));

        Assert.Equal("hdev", rebuilt.Name);
        Assert.Same(Settings, rebuilt.Settings);
        Assert.False(rebuilt.IsComputed);
    }

    [Fact]
    public void AnNAryNodeKeepsItsNameAndSettings()
    {
        var original = new AdditionExpression([Leaf(1, "a"), Leaf(2, "b")], "s", Settings);
        original.ComputeWithoutResult();

        var rebuilt = (AdditionExpression)original.WithOperands([Leaf(3, "c"), Leaf(4, "d")]);

        Assert.Equal("s", rebuilt.Name);
        Assert.Same(Settings, rebuilt.Settings);
        Assert.False(rebuilt.IsComputed);
        Assert.True(original.IsComputed);
    }
}
