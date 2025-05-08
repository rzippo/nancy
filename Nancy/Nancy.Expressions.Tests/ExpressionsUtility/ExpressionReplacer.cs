using System;
using Unipi.Nancy.Expressions.ExpressionsUtility;
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
    public void ReplaceByValueTest_1()
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
    public void ReplaceByPositionTest_1()
    {
        var e = Expressions.Convolution(
            Expressions.DelayBy(Curve.Zero(), Rational.Zero, "alpha1"),
            Expressions.FromCurve(Curve.Zero(), "a"));

        var replaced = e.ReplaceByPosition(
            e.RootPosition().IndexedOperand(0).RightOperand(),
            Expressions.FromRational(Rational.One)
        );
        
        _testOutputHelper.WriteLine(e.ToString());
        _testOutputHelper.WriteLine(replaced.ToString());
    }
    
    /// <summary>
    /// This is the same as <see cref="ReplaceByPositionTest_3"/>, but with different order of evaluation.
    /// Together, they test that value caching does not interfere with correct computation.
    /// </summary>
    [Fact]
    public void ReplaceByPositionTest_2()
    {
        var e1 = Expressions.HorizontalDeviation(
            new SigmaRhoArrivalCurve(6, 2),
            new RateLatencyServiceCurve(3, 10)
        );
        // expected value = 10 + 6 / 3 = 12 
        var v1 = e1.Compute();

        var e2 = e1.ReplaceByPosition(
            e1.RootPosition().RightOperand(),
            new RateLatencyServiceCurve(2, 5)
        );
        // expected value = 5 + 6 / 2 = 8
        var v2 = e2.Compute();

        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(v1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(v2.ToString());
        
        Assert.Equal(12, v1);
        Assert.Equal(8, v2);
    }
    
    /// <summary>
    /// This is the same as <see cref="ReplaceByPositionTest_2"/>, but with different order of evaluation.
    /// Together, they test that value caching does not interfere with correct computation.
    /// </summary>
    [Fact]
    public void ReplaceByPositionTest_3()
    {
        var e1 = Expressions.HorizontalDeviation(
            new SigmaRhoArrivalCurve(6, 2),
            new RateLatencyServiceCurve( 3, 10)
        );

        var e2 = e1.ReplaceByPosition(
            e1.RootPosition().RightOperand(),
            new RateLatencyServiceCurve( 2, 5)
        );

        // expected value = 10 + 6 / 3 = 12 
        var v1 = e1.Compute();

        // expected value = 5 + 6 / 2 = 8
        var v2 = e2.Compute();

        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(v1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(v2.ToString());
        
        Assert.Equal(12, v1);
        Assert.Equal(8, v2);
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