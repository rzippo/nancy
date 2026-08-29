using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class WithGenerationTests
{
    [Fact]
    public void WithGenerationSetsGenerationLeavingNameAndOperandsUnchanged()
    {
        var x = Expressions.FromCurve(new ConstantCurve(1)).WithName("x").Addition(
            Expressions.FromCurve(new ConstantCurve(2)));
        var nAry = (CurveNAryExpression)x;

        var x2 = x.WithGeneration(2);
        var nAry2 = (CurveNAryExpression)x2;

        Assert.Equal(2, x2.Generation);
        Assert.Equal(x.Name, x2.Name);
        Assert.Equal(nAry.Operands.Count, nAry2.Operands.Count);
        Assert.Equal(x.Value, x2.Value);
    }

    [Fact]
    public void WithGenerationDoesNotMutateTheReceiver()
    {
        var x = Expressions.FromCurve(new ConstantCurve(1)).WithGeneration(1);

        var x2 = x.WithGeneration(2);

        Assert.Equal(1, x.Generation);
        Assert.Equal(2, x2.Generation);
    }

    [Fact]
    public void WithGenerationPreservesAnAlreadyComputedValue()
    {
        var x = Expressions.FromCurve(new ConstantCurve(1)).Addition(Expressions.FromCurve(new ConstantCurve(2)));
        x.ComputeWithoutResult();

        var x2 = x.WithGeneration(1);

        Assert.True(x2.IsComputed);
        Assert.Equal(x.Value, x2.Value);
    }

    [Fact]
    public void RationalWithGenerationSetsGenerationLeavingNameAndOperandsUnchanged()
    {
        var x = Expressions.FromRational(1).WithName("x").Addition(Expressions.FromRational(2));
        var nAry = (RationalNAryExpression)x;

        var x2 = x.WithGeneration(2);
        var nAry2 = (RationalNAryExpression)x2;

        Assert.Equal(2, x2.Generation);
        Assert.Equal(x.Name, x2.Name);
        Assert.Equal(nAry.Operands.Count, nAry2.Operands.Count);
        Assert.Equal(x.Value, x2.Value);
    }

    // IGenericExpression<T> carries its own WithGeneration, implemented explicitly on each base.

    [Fact]
    public void CurveWithGenerationThroughTheInterfaceSetsGeneration()
    {
        IGenericExpression<Curve> x = Expressions.FromCurve(new ConstantCurve(1), "x");

        var x2 = x.WithGeneration(3);

        Assert.Equal(3, x2.Generation);
        Assert.Equal(x.Name, x2.Name);
    }

    [Fact]
    public void RationalWithGenerationThroughTheInterfaceSetsGeneration()
    {
        IGenericExpression<Rational> x = Expressions.FromRational(new Rational(1, 2), "x");

        var x2 = x.WithGeneration(3);

        Assert.Equal(3, x2.Generation);
        Assert.Equal(x.Name, x2.Name);
    }
}
