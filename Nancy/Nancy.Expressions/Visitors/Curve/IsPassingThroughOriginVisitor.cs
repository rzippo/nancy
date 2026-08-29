using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check whether the value of a curve expression passes through the origin, i.e. $f(0) = 0$.
/// Implemented minimizing the amount of computations.
/// </summary>
/// <remarks>
/// Replaces the former <c>IsZeroAtZeroVisitor</c>, renamed for consistency with <see cref="Curve.IsPassingThroughOrigin"/>.
/// </remarks>
public class IsPassingThroughOriginVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsPassingThroughOrigin;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        =>
        IsPassingThroughOrigin = expression.Value.IsPassingThroughOrigin;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        =>
        IsPassingThroughOrigin = expression.Compute().IsPassingThroughOrigin;

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
        => expression.Expression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression)
        => IsPassingThroughOrigin = expression.Expression.Compute().ValueAt(Rational.Zero) <= Rational.Zero;

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        // The SAC is 0 in 0 only if the argument is >= 0 in 0
        expression.Expression.Accept(this);
        if (!IsPassingThroughOrigin)
        {
            IsPassingThroughOrigin = expression.Expression.Value.ValueAt(Rational.Zero) > Rational.Zero;
        }
    }

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLeftContinuousExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToRightContinuousExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(WithZeroOriginExpression expression)
        => IsPassingThroughOrigin = true;

    /// <inheritdoc />
    public virtual void Visit(WithOriginAtExpression expression)
        => IsPassingThroughOrigin = expression.OriginValue == Rational.Zero;

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            e.Accept(this);
            if (!IsPassingThroughOrigin)
                break;
        }

        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression)
    {
        expression.LeftOperand.Accept(this);
        if (IsPassingThroughOrigin)
        {
            expression.RightOperand.Accept(this);
        }

        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            e.Accept(this);
            if (!IsPassingThroughOrigin)
                break;
        }

        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression)
    {
        foreach (var e in expression.Operands)
        {
            e.Accept(this);
            if (!IsPassingThroughOrigin)
                break;
        }

        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ConvolutionExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(DeconvolutionExpression expression)
        => _throughCurveComputation(expression);

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
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(VerticalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightOperand.Compute() == 0) IsPassingThroughOrigin = true;
        else expression.LeftOperand.Accept(this);
    }

    /// <inheritdoc />
    /// <remarks>$\lfloor 0 \rfloor = 0$.</remarks>
    public virtual void Visit(FloorExpression expression)
    {
        expression.Expression.Accept(this);
        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>$\lceil 0 \rceil = 0$.</remarks>
    public virtual void Visit(CeilExpression expression)
    {
        expression.Expression.Accept(this);
        if (!IsPassingThroughOrigin) _throughCurveComputation(expression);
    }
}
