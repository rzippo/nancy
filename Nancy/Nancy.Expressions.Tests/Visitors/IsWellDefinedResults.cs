using System.Collections.Generic;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Visitors;

public class IsWellDefinedResults
{
    public static TheoryData<Curve, bool> ClosureOperands => new()
    {
        { Curve.Zero(), true },
        { Curve.PlusInfinite(), true },
        { Curve.MinusInfinite(), true },
        { Curve.PlusInfinite().WithOriginAt(Unipi.Nancy.Numerics.Rational.MinusInfinity), false }
    };

    [Theory]
    [MemberData(nameof(ClosureOperands))]
    public void SubAdditiveClosureAssignsItsResultOnEveryVisit(Curve curve, bool expected)
    {
        var expression = curve.ToExpression().SubAdditiveClosure();
        var visitor = new IsWellDefinedVisitor { IsWellDefined = !expected };

        expression.Accept(visitor);
        Assert.Equal(expected, visitor.IsWellDefined);
        Assert.Equal(expected, expression.IsWellDefined);
        Assert.Equal(expected, expression.IsWellDefined);
    }

    public static TheoryData<Curve, Curve, bool> SubtractionOperands => new()
    {
        { Curve.Zero(), Curve.PlusInfinite(), true },
        { Curve.PlusInfinite(), Curve.Zero(), true },
        { Curve.PlusInfinite(), Curve.MinusInfinite(), true },
        { Curve.MinusInfinite(), Curve.PlusInfinite(), true },
        { Curve.PlusInfinite(), Curve.PlusInfinite(), false },
        { Curve.MinusInfinite(), Curve.MinusInfinite(), false }
    };

    [Theory]
    [MemberData(nameof(SubtractionOperands))]
    public void SubtractionAssignsItsResultOnEveryVisit(Curve left, Curve right, bool expected)
    {
        var expression = Expressions.Subtraction(left, right);
        var visitor = new IsWellDefinedVisitor { IsWellDefined = !expected };

        expression.Accept(visitor);
        Assert.Equal(expected, visitor.IsWellDefined);
        Assert.Equal(expected, expression.IsWellDefined);
        Assert.Equal(expected, expression.IsWellDefined);
    }
}
