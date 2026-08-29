using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check the value of a curve expression is non-decreasing. Implemented minimizing the amount of
/// computations.
/// </summary>
public class IsNonDecreasingVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsNonDecreasing;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) 
        => IsNonDecreasing = expression.Value.IsNonDecreasing;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsNonDecreasing = expression.Compute().IsNonDecreasing;

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
    { 
        expression.Operand.Accept(this);
        if (IsNonDecreasing) IsNonDecreasing = false;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        if (((CurveExpression)expression.Operand).IsNonNegative)
        {
            expression.Operand.Accept(this);
            if (IsNonDecreasing) return;
        }
        _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonDecreasingExpression expression) 
        => IsNonDecreasing = true;

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonDecreasingExpression expression) 
        => IsNonDecreasing = true;

    /// <inheritdoc />
    public virtual void Visit(ToLeftContinuousExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToRightContinuousExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(WithZeroOriginExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(WithOriginAtExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        expression.Operand.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        expression.Operand.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            IsNonDecreasing = false;
            if (((CurveExpression)e).IsNonNegative)
            {
                e.Accept(this);
                if (!IsNonDecreasing)
                    break;
            }
            else break;
        }
        if(!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            IsNonDecreasing = false;
            if (((CurveExpression)e).IsNonNegative)
            {
                e.Accept(this);
                if (!IsNonDecreasing)
                    break;
            }
            else break;
        }
        if(!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ConvolutionExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            IsNonDecreasing = false;
            if (((CurveExpression)e).IsNonNegative)
            {
                e.Accept(this);
                if (!IsNonDecreasing)
                    break;
            }
            else break;
        }
        if(!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(DeconvolutionExpression expression)
    {
        if (((CurveExpression)expression.LeftOperand).IsNonNegative)
        {
            expression.LeftOperand.Accept(this);
            if (IsNonDecreasing)
            {
                // expression._isNonNegative = true;
                // expression._isNonDecreasing = true;
                return;
            }
        }

        _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MaxPlusConvolutionExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxPlusDeconvolutionExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CompositionExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(DelayByExpression expression) 
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(ForwardByExpression expression) 
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(VerticalShiftExpression expression) 
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightOperand.Compute() > 0) expression.LeftOperand.Accept(this);
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Floor is itself a non-decreasing function, so the composition with a non-decreasing curve is non-decreasing.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        expression.Operand.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Ceiling is itself a non-decreasing function, so the composition with a non-decreasing curve is non-decreasing.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        expression.Operand.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }
}
