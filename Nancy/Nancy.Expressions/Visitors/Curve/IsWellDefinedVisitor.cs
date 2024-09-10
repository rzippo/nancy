using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check the operations of a curve expression are well-defined (according to the definitions in [BT08],
/// Section 2.1). Implemented minimizing the amount of computations.
/// </summary>
public class IsWellDefinedVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsWellDefined;

    // A curve is always well-defined
    public virtual void Visit(ConcreteCurveExpression expression) => IsWellDefined = true;

    public virtual void Visit(NegateExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(ToNonNegativeExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        if (expression.Expression.Value.IsFinite) IsWellDefined = true;
        else if (expression.Expression.Value.SupValue() == Rational.PlusInfinity &&
                 expression.Expression.Value.InfValue() == Rational.MinusInfinity) IsWellDefined = false;
    }

    public virtual void Visit(SuperAdditiveClosureExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(ToUpperNonDecreasingExpression expression) => IsWellDefined = true;

    public virtual void Visit(ToLowerNonDecreasingExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(ToLeftContinuousExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(ToRightContinuousExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(WithZeroOriginExpression expression) => expression.Expression.Accept(this);

    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(AdditionExpression expression)
    {
        var plusInfinity = 0;
        var minusInfinity = 0;
        foreach (var operand in expression.Expressions)
        {
            if (operand.Value.IsFinite) continue;
            if (operand.Value.SupValue() == Rational.PlusInfinity) plusInfinity++;
            if (operand.Value.InfValue() == Rational.MinusInfinity) minusInfinity++;
        }

        // If there are operands with infinite values but opposite signs --> Convolution is NOT well-defined
        IsWellDefined = plusInfinity == 0 || minusInfinity == 0;
    }

    public virtual void Visit(SubtractionExpression expression)
    {
        // If at least one of the operands is finite --> Subtraction is finite
        if (expression.LeftExpression.Value.IsFinite || expression.RightExpression.Value.IsFinite) IsWellDefined = true;

        // f - g is undefined if it exists t: f(t) = g(t) = +infinity (-infinity)
        else if (expression.LeftExpression.Value.InfValue() == expression.RightExpression.Value.InfValue() &&
                 expression.LeftExpression.Value.InfValue() == Rational.MinusInfinity ||
                 expression.LeftExpression.Value.SupValue() == expression.RightExpression.Value.SupValue() &&
                 expression.LeftExpression.Value.SupValue() == Rational.PlusInfinity) IsWellDefined = false;
    }

    public virtual void Visit(MinimumExpression expression) => IsWellDefined = true;

    public virtual void Visit(MaximumExpression expression) => IsWellDefined = true;

    public virtual void Visit(ConvolutionExpression expression)
    {
        var plusInfinity = 0;
        var minusInfinity = 0;
        foreach (var operand in expression.Expressions)
        {
            if (operand.Value.IsFinite) continue;
            if (operand.Value.SupValue() == Rational.PlusInfinity) plusInfinity++;
            if (operand.Value.InfValue() == Rational.MinusInfinity) minusInfinity++;
        }

        // If there are operands with infinite values but opposite signs --> Convolution is NOT well-defined
        IsWellDefined = plusInfinity == 0 || minusInfinity == 0;
    }

    public virtual void Visit(DeconvolutionExpression expression)
    {
        // f deconv g is undefined if ∃ t1 ≤ t2, f (t2) = g(t1) = +∞ (or−∞)
        if (expression.LeftExpression.Value.IsFinite || expression.RightExpression.Value.IsFinite) IsWellDefined = true;
        else
        {
            throw new NotImplementedException("f deconv g is undefined if ∃ t1 ≤ t2, f (t2) = g(t1) = +∞ (or−∞)");
        }
    }

    public virtual void Visit(MaxPlusConvolutionExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(CompositionExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(DelayByExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(AnticipateByExpression expression)
    {
        throw new NotImplementedException();
    }

    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    public virtual void Visit(ScaleExpression expression)
    {
        throw new NotImplementedException();
    }
}