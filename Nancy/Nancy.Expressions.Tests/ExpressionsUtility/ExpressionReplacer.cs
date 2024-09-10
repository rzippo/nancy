using System;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;


public class ExpressionReplacer
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ExpressionReplacer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ReplaceByValueTest()
    {
        var e = Expressions.SubAdditiveClosure(
            Expressions.DelayBy(Curve.Zero(), Rational.Zero, "alpha"));

        var replaced = e.ReplaceByValue(
            Expressions.FromCurve(Curve.Zero(), "alpha"),
            Expressions.DelayBy(Curve.Zero(), Rational.Zero, "alpha1"));
        
        // Assert.True(replaced.RootPosition().LeftOperand().RightOperand());
        _testOutputHelper.WriteLine(replaced.ToString());
        _testOutputHelper.WriteLine(e.ToString());
    }

    [Fact]
    public void ReplaceByPositionTest()
    {
        var e = Expressions.Convolution(
            Expressions.DelayBy(Curve.Zero(), Rational.Zero, "alpha1"),
            Expressions.FromCurve(Curve.Zero(), "a"));

        _testOutputHelper.WriteLine(e.ReplaceByPosition(e.RootPosition().Operand(0).RightOperand(),
            Expressions.FromRational(Rational.One)).ToString());
        _testOutputHelper.WriteLine(e.ToString());
    }
    
    [Fact]
    public void ReplaceByValueTest2()
    {
        var a = new RateLatencyServiceCurve(1,1);
        var b = new RateLatencyServiceCurve(1,2);
        var c = new RateLatencyServiceCurve(1,3);

        var abc = Expressions.Addition(Expressions.Convolution([a, b, c], ["a", "b", "c"]), Expressions.Convolution([b, c], ["b", "c"]));

        _testOutputHelper.WriteLine(abc.ToString());

        var ac = abc.ReplaceByValue(Expressions.Convolution(b,c), Expressions.FromCurve(c));

        _testOutputHelper.WriteLine(ac.ToString());
    }
}