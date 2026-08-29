using System.Collections.Generic;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.NetworkCalculus;
using Unipi.Nancy.Numerics;
using Xunit;

namespace Unipi.Nancy.Expressions.Tests.Operands;

public class RenameCurveVisitorCachePreservation
{
    private static readonly CurveExpression A = Expressions.FromCurve(new ConstantCurve(1), "a");
    private static readonly CurveExpression B = Expressions.FromCurve(new ConstantCurve(2), "b");

    // The ten operators whose Visit overload used to bypass CommonVisit entirely, doing a bare `expression with { Name = NewName }` that drops every cache field, _value included.
    public static IEnumerable<object[]> PreviouslyBypassingOperators()
    {
        yield return new object[] { Expressions.Deconvolution(A, B) };
        yield return new object[] { Expressions.MaxPlusConvolution(A, B) };
        yield return new object[] { Expressions.MaxPlusDeconvolution(A, B) };
        yield return new object[] { Expressions.Composition(A, B) };
        yield return new object[] { Expressions.DelayBy(A, new Rational(1)) };
        yield return new object[] { Expressions.ForwardBy(A, new Rational(1)) };
        yield return new object[] { Expressions.HorizontalShift(A, new Rational(1)) };
        yield return new object[] { Expressions.VerticalShift(A, new Rational(1)) };
        yield return new object[] { Expressions.Scale(A, new Rational(2)) };
    }

    [Theory]
    [MemberData(nameof(PreviouslyBypassingOperators))]
    public void RenamingPreservesAnAlreadyComputedValue(CurveExpression expression)
    {
        expression.ComputeWithoutResult();
        Assert.True(expression.IsComputed);

        var renamed = expression.WithName("renamed");

        Assert.True(renamed.IsComputed);
        Assert.Equal(expression.Value, renamed.Value);
    }
}
