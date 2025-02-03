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
        _testOutputHelper.WriteLine(e.ToString());
        
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(d));
        var r1 = Expressions.Subtraction(d, b);
        _testOutputHelper.WriteLine(e1.ToString());
        Assert.Equal(e1.ToString(), r1.ToString());
        
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(a));
        var r2 = Expressions.Subtraction(Expressions.Subtraction(f, a), a);
        _testOutputHelper.WriteLine(e2.ToString());
        Assert.Equal(e2.ToString(), r2.ToString());
        
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(d));
        var r3 = Expressions.Subtraction(Expressions.Subtraction(f, d), b);
        _testOutputHelper.WriteLine(e3.ToString());
        Assert.Equal(e3.ToString(), r3.ToString());

        var e4 = e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(c));
        var r4 =   Expressions.Subtraction(c, b);
        _testOutputHelper.WriteLine(e4.ToString());
        Assert.Equal(e4.ToString(), r4.ToString());
        
        var e5 = e2.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(b));
        var r5 = Expressions.Subtraction(Expressions.Subtraction(f, a), b);
        _testOutputHelper.WriteLine(e5.ToString());
        Assert.Equal(e5.ToString(), r5.ToString());
        
        var e6 = e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(f));
        var r6 = Expressions.Subtraction(Expressions.Subtraction(f, f), b);
        _testOutputHelper.WriteLine(e6.ToString());
        Assert.Equal(e6.ToString(), r6.ToString());
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
        _testOutputHelper.WriteLine(e.ToString());
        
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(c));
        var r1 = Expressions.HorizontalDeviation(c, d);
        _testOutputHelper.WriteLine(e1.ToString());
        Assert.Equal(e1.ToString(), r1.ToString());
        
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(f));
        var r2 = Expressions.HorizontalDeviation(Expressions.Subtraction(c, a), f);
        _testOutputHelper.WriteLine(e2.ToString());
        Assert.Equal(e2.ToString(), r2.ToString());
        
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(b));
        var r3 = Expressions.HorizontalDeviation(Expressions.Subtraction(c, b), d);
        _testOutputHelper.WriteLine(e3.ToString());
        Assert.Equal(e3.ToString(), r3.ToString());

        var e4 = e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(a));
        var r4 = Expressions.HorizontalDeviation(a, d);
        _testOutputHelper.WriteLine(e4.ToString());
        Assert.Equal(e4.ToString(), r4.ToString());

        var e5 = e2.ReplaceByPosition(e.RootPosition().RightOperand(), new ConcreteCurveExpression(d));
        var r5 = Expressions.HorizontalDeviation(Expressions.Subtraction(c, a), d);
        _testOutputHelper.WriteLine(e5.ToString());
        Assert.Equal(e5.ToString(), r5.ToString());

        var e6 = e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new ConcreteCurveExpression(a));
        var r6 = Expressions.HorizontalDeviation(Expressions.Subtraction(c, a), d);
        _testOutputHelper.WriteLine(e6.ToString());
        Assert.Equal(e6.ToString(), r6.ToString());

        _testOutputHelper.WriteLine(e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(a)).ToString());
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
        _testOutputHelper.WriteLine(e.ToString());
        
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(a));
        var r1 = Expressions.Scale(a, Expressions.RationalSubtraction(f, c));
        _testOutputHelper.WriteLine(e1.ToString());
        Assert.Equal(e1.ToString(), r1.ToString());
        
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(d));
        var r2 = Expressions.Scale(b, d);
        _testOutputHelper.WriteLine(e2.ToString());
        Assert.Equal(e2.ToString(), r2.ToString());
        
        var e3 = e.ReplaceByPosition(e.RootPosition().RightOperand().LeftOperand(), new RationalNumberExpression(d));
        var r3 = Expressions.Scale(b, Expressions.RationalSubtraction(d, c));
        _testOutputHelper.WriteLine(e3.ToString());
        Assert.Equal(e3.ToString(), r3.ToString());
        
        var e4 = e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new ConcreteCurveExpression(b));
        var r4 = Expressions.Scale(b, Expressions.RationalSubtraction(f, c));
        _testOutputHelper.WriteLine(e4.ToString());
        Assert.Equal(e4.ToString(), r4.ToString());

        var e5 = e2.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(c));
        var r5 = Expressions.Scale(b, c);
        _testOutputHelper.WriteLine(e5.ToString());
        Assert.Equal(e5.ToString(), r5.ToString());
        
        var e6 = e3.ReplaceByPosition(e.RootPosition().RightOperand().LeftOperand(), new RationalNumberExpression(c));
        var r6 = Expressions.Scale(b, Expressions.RationalSubtraction(c, c));
        _testOutputHelper.WriteLine(e6.ToString());
        Assert.Equal(e6.ToString(), r6.ToString());
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
        _testOutputHelper.WriteLine(e.ToString());
        
        var e1 = e.ReplaceByPosition(e.RootPosition().LeftOperand(), new RationalNumberExpression(a));
        var r1 = Expressions.RationalSubtraction(a, b);
        _testOutputHelper.WriteLine(e1.ToString());
        Assert.Equal(e1.ToString(), r1.ToString());
        
        var e2 = e.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(d));
        var r2 = Expressions.RationalSubtraction(Expressions.RationalSubtraction(f, c), d);
        _testOutputHelper.WriteLine(e2.ToString());
        Assert.Equal(e2.ToString(), r2.ToString());
        
        var e3 = e.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new RationalNumberExpression(d));
        var r3 = Expressions.RationalSubtraction(Expressions.RationalSubtraction(f, d), b);
        _testOutputHelper.WriteLine(e3.ToString());
        Assert.Equal(e3.ToString(), r3.ToString());

        var e4 = e1.ReplaceByPosition(e.RootPosition().LeftOperand(), new RationalNumberExpression(b));
        var r4 = Expressions.RationalSubtraction(b, b);
        _testOutputHelper.WriteLine(e4.ToString());
        Assert.Equal(e4.ToString(), r4.ToString());

        var e5 = e2.ReplaceByPosition(e.RootPosition().RightOperand(), new RationalNumberExpression(c));
        var r5 = Expressions.RationalSubtraction(Expressions.RationalSubtraction(f, c), c);
        _testOutputHelper.WriteLine(e5.ToString());
        Assert.Equal(e5.ToString(), r5.ToString());

        var e6 = e3.ReplaceByPosition(e.RootPosition().LeftOperand().RightOperand(), new RationalNumberExpression(c));
        var r6 = Expressions.RationalSubtraction(Expressions.RationalSubtraction(f, c), b);
        _testOutputHelper.WriteLine(e6.ToString());
        Assert.Equal(e6.ToString(), r6.ToString());
    }
}