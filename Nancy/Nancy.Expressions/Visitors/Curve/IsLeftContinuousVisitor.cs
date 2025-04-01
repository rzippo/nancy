using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check the left-continuity of the value of a curve expression. Implemented minimizing the amount of
/// computations.
/// </summary>
public class IsLeftContinuousVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsLeftContinuous;
    
    public virtual void Visit(ConcreteCurveExpression expression) 
        => IsLeftContinuous = expression.Value.IsLeftContinuous;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsLeftContinuous = expression.Compute().IsLeftContinuous;

    public virtual void Visit(NegateExpression expression) 
        => expression.Expression.Accept(this);

    public virtual void Visit(ToNonNegativeExpression expression) 
        => _throughCurveComputation(expression);
    
    public virtual void Visit(SubAdditiveClosureExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(SuperAdditiveClosureExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(ToUpperNonDecreasingExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(ToLowerNonDecreasingExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(ToLeftContinuousExpression expression) 
        => IsLeftContinuous = true;

    public virtual void Visit(ToRightContinuousExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(WithZeroOriginExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        expression.Expression.Accept(this);
    }

    public virtual void Visit(UpperPseudoInverseExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(AdditionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(SubtractionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(MinimumExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(MaximumExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(ConvolutionExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            IsLeftContinuous = false;
            if (((CurveExpression)e).IsNonDecreasing)
            {
                e.Accept(this);
                if (!IsLeftContinuous)
                    break;
            }
            else break;
        }
        if(!IsLeftContinuous) _throughCurveComputation(expression);
    }

    public virtual void Visit(DeconvolutionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusConvolutionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusDeconvolutionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(CompositionExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(DelayByExpression expression) 
        => _throughCurveComputation(expression);

    public virtual void Visit(ForwardByExpression expression) 
        => _throughCurveComputation(expression);
    
    public void Visit(ShiftExpression expression)
        => _throughCurveComputation(expression);

    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");
    
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() != 0) expression.LeftExpression.Accept(this);
        else IsLeftContinuous = true;
    }
}