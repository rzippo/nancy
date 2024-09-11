using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check the concavity of the value of a curve expression. Implemented minimizing the amount of
/// computations.
/// </summary>
public class IsZeroAtZeroVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsZeroAtZero;

    public virtual void Visit(ConcreteCurveExpression expression) =>
        IsZeroAtZero = expression.Value.IsZeroAtZero();

    private void _throughCurveComputation(IGenericExpression<Curve> expression) =>
        IsZeroAtZero = expression.Compute().IsZeroAtZero();

    public virtual void Visit(NegateExpression expression)
        => expression.Expression.Accept(this);

    public virtual void Visit(ToNonNegativeExpression expression)
        => IsZeroAtZero = expression.Expression.Compute().ValueAt(Rational.Zero) <= Rational.Zero;

    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        // The SAC is 0 in 0 only if the argument is >= 0 in 0
        expression.Expression.Accept(this);
        if (!IsZeroAtZero)
        {
            IsZeroAtZero = expression.Expression.Value.ValueAt(Rational.Zero) > Rational.Zero;
        }
    }

    public virtual void Visit(SuperAdditiveClosureExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToUpperNonDecreasingExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToLowerNonDecreasingExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToLeftContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(ToRightContinuousExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(WithZeroOriginExpression expression) => IsZeroAtZero = true;

    public virtual void Visit(LowerPseudoInverseExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(UpperPseudoInverseExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsZeroAtZero)
                break;
        }

        if (!IsZeroAtZero) _throughCurveComputation(expression);
    }

    public virtual void Visit(SubtractionExpression expression)
    {
        expression.LeftExpression.Accept(this);
        if (IsZeroAtZero)
        {
            expression.RightExpression.Accept(this);
        }

        if (!IsZeroAtZero) _throughCurveComputation(expression);
    }

    public virtual void Visit(MinimumExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsZeroAtZero)
                break;
        }

        if (!IsZeroAtZero) _throughCurveComputation(expression);
    }

    public virtual void Visit(MaximumExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsZeroAtZero)
                break;
        }

        if (!IsZeroAtZero) _throughCurveComputation(expression);
    }

    public virtual void Visit(ConvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(DeconvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusConvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(MaxPlusDeconvolutionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(CompositionExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(DelayByExpression expression) => expression.LeftExpression.Accept(this);

    public virtual void Visit(AnticipateByExpression expression) => _throughCurveComputation(expression);

    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() == 0) IsZeroAtZero = true;
        else expression.LeftExpression.Accept(this);
    }
}