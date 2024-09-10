using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check the convexity of the value of a curve expression. Implemented minimizing the amount of
/// computations.
/// </summary>
public class IsConvexVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsConvex;

    public virtual void Visit(ConcreteCurveExpression expression) => IsConvex = expression.Value.IsConvex;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) =>
        IsConvex = expression.Compute().IsConvex;
    
    public virtual void Visit(NegateExpression expression)
    {
        expression.Expression.Accept(this);
        if(IsConvex)
            IsConvex = !IsConvex;
        else
            IsConvex = expression.Compute().IsConvex;
    }

    public virtual void Visit(ToNonNegativeExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(SubAdditiveClosureExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(SuperAdditiveClosureExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToUpperNonDecreasingExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToLowerNonDecreasingExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToLeftContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToRightContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(WithZeroOriginExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(LowerPseudoInverseExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(UpperPseudoInverseExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsConvex)
                break;
        }

        if (!IsConvex) _throughCurveComputation(expression);
    }

    public virtual void Visit(SubtractionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MinimumExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaximumExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsConvex)
                break;
        }

        if (!IsConvex) _throughCurveComputation(expression);
    }

    public virtual void Visit(ConvolutionExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsConvex)
                break;
        }

        if (!IsConvex) _throughCurveComputation(expression);
    }

    public virtual void Visit(DeconvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusConvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusDeconvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(CompositionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(DelayByExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(AnticipateByExpression expression) => _throughCurveComputation(expression);
    
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() > 0) expression.LeftExpression.Accept(this);
        else _throughCurveComputation(expression);
    }
}