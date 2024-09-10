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

    public virtual void Visit(ConcreteCurveExpression expression) => IsNonDecreasing = expression.Value.IsNonDecreasing;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) =>
        IsNonDecreasing = expression.Compute().IsNonDecreasing;

    public virtual void Visit(NegateExpression expression)
    { 
        expression.Expression.Accept(this);
        if (IsNonDecreasing) IsNonDecreasing = false;
        else _throughCurveComputation(expression);
    }

    public virtual void Visit(ToNonNegativeExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        if (((CurveExpression)expression.Expression).IsNonNegative)
        {
            expression.Expression.Accept(this);
            if (IsNonDecreasing) return;
        }
        _throughCurveComputation(expression);
    }

    public virtual void Visit(SuperAdditiveClosureExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToUpperNonDecreasingExpression expression) => IsNonDecreasing = true;

    public virtual void Visit(ToLowerNonDecreasingExpression expression) => IsNonDecreasing = true;

    public virtual void Visit(ToLeftContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToRightContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(WithZeroOriginExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        expression.Expression.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }

    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        expression.Expression.Accept(this);
        if (!IsNonDecreasing) _throughCurveComputation(expression);
    }

    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Expressions)
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

    public virtual void Visit(SubtractionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MinimumExpression expression)
    {
        foreach (var e in expression.Expressions)
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

    public virtual void Visit(MaximumExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ConvolutionExpression expression)
    {
        foreach (var e in expression.Expressions)
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

    public virtual void Visit(DeconvolutionExpression expression)
    {
        if (((CurveExpression)expression.LeftExpression).IsNonNegative)
        {
            expression.LeftExpression.Accept(this);
            if (IsNonDecreasing)
            {
                // expression._isNonNegative = true;
                // expression._isNonDecreasing = true;
                return;
            }
        }

        _throughCurveComputation(expression);
    }

    public virtual void Visit(MaxPlusConvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusDeconvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(CompositionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(DelayByExpression expression) => expression.LeftExpression.Accept(this);

    public virtual void Visit(AnticipateByExpression expression) => expression.LeftExpression.Accept(this);

    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() > 0) expression.LeftExpression.Accept(this);
        else _throughCurveComputation(expression);
    }
}