using System;
using Unipi.Nancy.Expressions.Internals;
using Xunit;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit.Abstractions;

namespace Unipi.Nancy.Expressions.Tests.ExpressionsUtility;


public class ReplaceByPositionTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ReplaceByPositionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void BinaryReturnCurveLeftCurveRightCurveTest()
    {
        var a = new SigmaRhoArrivalCurve(1, 1);
        var b = new SigmaRhoArrivalCurve(2, 2);
        var c = new SigmaRhoArrivalCurve(3, 3);
        var d = new SigmaRhoArrivalCurve(4, 4);
        var f = new SigmaRhoArrivalCurve(5, 5);
        
        var e = Expressions.Subtraction(Expressions.Subtraction(f, a), b);
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(d));
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(a));
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(d));

        _testOutputHelper.WriteLine(e.ToString());
        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(e3.ToString());
        _testOutputHelper.WriteLine(e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(c)).ToString());
        _testOutputHelper.WriteLine(e2.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(b)).ToString());
        _testOutputHelper.WriteLine(e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(f)).ToString());
    }
    
    [Fact]
    public void BinaryReturnRationalLeftCurveRightCurveTest()
    {
        var a = new SigmaRhoArrivalCurve(1, 1);
        var b = new SigmaRhoArrivalCurve(2, 2);
        var c = new SigmaRhoArrivalCurve(3, 3);
        var d = new RateLatencyServiceCurve(4, 4);
        var f = new RateLatencyServiceCurve(5, 5);
        
        var e = Expressions.HorizontalDeviation(Expressions.Subtraction(c, a), d);
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(c));
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(f));
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(b));

        _testOutputHelper.WriteLine(e.ToString());
        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(e3.ToString());
        _testOutputHelper.WriteLine(e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(a)).ToString());
        _testOutputHelper.WriteLine(e2.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(d)).ToString());
        _testOutputHelper.WriteLine(e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(a)).ToString());
    }
    
    [Fact]
    public void BinaryReturnCurveLeftCurveRightRationalTest()
    {
        var a = new SigmaRhoArrivalCurve(1, 1);
        var b = new SigmaRhoArrivalCurve(2, 2);
        var c = 3;
        var d = 4;
        var f = 5;
        
        var e = Expressions.Scale(b, Expressions.RationalSubtraction(f, c));
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(a));
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(d));
        var e3 = e.ReplaceByPosition(e.RootPosition().RightOperand().LeftOperand(), new RationalNumberExpression(d));
        
        _testOutputHelper.WriteLine(e.ToString());
        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(e3.ToString());
        _testOutputHelper.WriteLine(e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(b)).ToString());
        _testOutputHelper.WriteLine(e2.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(c)).ToString());
        _testOutputHelper.WriteLine(e3.ReplaceByPosition(e.RootPosition().RightOperand().LeftOperand(), new RationalNumberExpression(c)).ToString());
    }
    
    [Fact]
    public void BinaryReturnRationalLeftRationalRightRationalTest()
    {
        var a = 1;
        var b = 2;
        var c = 3;
        var d = 4;
        var f = 5;

        var e = Expressions.RationalSubtraction(Expressions.RationalSubtraction(f, c), b);
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new RationalNumberExpression(a));
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(d));
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new RationalNumberExpression(d));

        _testOutputHelper.WriteLine(e.ToString());
        _testOutputHelper.WriteLine(e1.ToString());
        _testOutputHelper.WriteLine(e2.ToString());
        _testOutputHelper.WriteLine(e3.ToString());
        _testOutputHelper.WriteLine(e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new RationalNumberExpression(b)).ToString());
        _testOutputHelper.WriteLine(e2.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(c)).ToString());
        _testOutputHelper.WriteLine(e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new RationalNumberExpression(c)).ToString());
    }
}