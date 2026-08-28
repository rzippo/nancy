using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check whether the value of a curve expression is (strictly) increasing. Implemented minimizing
/// the amount of computations.
/// </summary>
public class IsIncreasingVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsIncreasing;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsIncreasing = expression.Value.IsIncreasing;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsIncreasing = expression.Compute().IsIncreasing;

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
    {
        expression.Expression.Accept(this);
        if (IsIncreasing) IsIncreasing = false;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    /// <remarks>Unlike its non-strict counterpart, the result is not guaranteed to be increasing (it may still
    /// contain flat segments), so no shortcut applies.</remarks>
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
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(WithOriginAtExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    /// <remarks>The sum of increasing curves is increasing.</remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        foreach (var e in expression.Expressions)
        {
            e.Accept(this);
            if (!IsIncreasing)
                break;
        }

        if (!IsIncreasing) _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression)
        => _throughCurveComputation(expression);

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
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(ForwardByExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(VerticalShiftExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() > 0) expression.LeftExpression.Accept(this);
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>Floor is not injective, so strict increase of the operand is not preserved.</remarks>
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    /// <remarks>Ceiling is not injective, so strict increase of the operand is not preserved.</remarks>
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
