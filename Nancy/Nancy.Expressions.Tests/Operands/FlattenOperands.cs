using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class FlattenOperandsTests
{
    [Fact]
    public void FlattenOperandsCrossesNamedBoundariesThatOperandsStopsAt()
    {
        CurveExpression x = Expressions.FromCurve(new ConstantCurve(1)).WithName("x");
        x = x.Addition(Expressions.FromCurve(new ConstantCurve(2))).WithName("x");
        x = x.Addition(Expressions.FromCurve(new ConstantCurve(3))).WithName("x");
        var nAry = (CurveNAryExpression)x;

        Assert.Equal(2, nAry.Operands.Count);
        Assert.Equal(3, nAry.FlattenOperands().Count);
    }

    [Fact]
    public void FlattenOperandsCrossesNamedBoundariesOnRationalExpressionsToo()
    {
        RationalExpression x = Expressions.FromRational(1).WithName("x");
        x = x.Addition(Expressions.FromRational(2)).WithName("x");
        x = x.Addition(Expressions.FromRational(3)).WithName("x");
        var nAry = (RationalNAryExpression)x;

        Assert.Equal(2, nAry.Operands.Count);
        Assert.Equal(3, nAry.FlattenOperands().Count);
    }

    [Fact]
    public void FlattenOperandsRecursesThroughEveryLevelOfNesting()
    {
        var a = Expressions.FromCurve(new ConstantCurve(1), "a");
        var b = Expressions.FromCurve(new ConstantCurve(2), "b");
        var level1 = (CurveExpression)new AdditionExpression([a, b], "level1");
        var level2 = (CurveExpression)new AdditionExpression([level1, a], "level2");
        var level3 = (CurveNAryExpression)new AdditionExpression([level2, b], "level3");

        Assert.Equal(2, level3.Operands.Count);
        Assert.Equal(4, level3.FlattenOperands().Count);
    }

    [Fact]
    public void FlattenOperandsStopsAtAnOperandOfADifferentOperator()
    {
        var a = Expressions.FromCurve(new ConstantCurve(1), "a");
        var b = Expressions.FromCurve(new ConstantCurve(2), "b");
        var minimum = (CurveExpression)new MinimumExpression([a, b], "m");
        var addition = (CurveNAryExpression)new AdditionExpression([minimum, a], "s");

        var flattened = addition.FlattenOperands();

        Assert.Equal(2, flattened.Count);
        Assert.Contains(minimum, flattened);
    }
}
