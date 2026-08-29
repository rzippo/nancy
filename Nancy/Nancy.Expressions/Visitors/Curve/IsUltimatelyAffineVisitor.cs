using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check whether the value of a curve expression is ultimately affine. Implemented minimizing the
/// amount of computations.
/// </summary>
public class IsUltimatelyAffineVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsUltimatelyAffine;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsUltimatelyAffine = expression.Value.IsUltimatelyAffine;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsUltimatelyAffine = expression.Compute().IsUltimatelyAffine;

    /// <inheritdoc />
    /// <remarks>Negating an affine tail of slope $m$ yields an affine tail of slope $-m$.</remarks>
    public virtual void Visit(NegateExpression expression)
        => expression.Expression.Accept(this);

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
    /// <remarks>A vertical repositioning of an affine tail is still affine, with the same slope.</remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
        => expression.Expression.Accept(this);

    /// <inheritdoc />
    /// <remarks>A vertical repositioning of an affine tail is still affine, with the same slope.</remarks>
    public virtual void Visit(WithOriginAtExpression expression)
        => expression.Expression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(AdditionExpression expression)
        => _throughCurveComputation(expression);

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
    /// <remarks>A time shift preserves an affine tail (and its slope).</remarks>
    public virtual void Visit(DelayByExpression expression)
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    /// <remarks>A time shift preserves an affine tail (and its slope).</remarks>
    public virtual void Visit(ForwardByExpression expression)
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    /// <remarks>A time shift preserves an affine tail (and its slope).</remarks>
    public virtual void Visit(HorizontalShiftExpression expression)
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    /// <remarks>A vertical shift preserves an affine tail (and its slope).</remarks>
    public virtual void Visit(VerticalShiftExpression expression)
        => expression.LeftOperand.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <inheritdoc />
    /// <remarks>Scaling by a non-zero factor preserves an affine tail (the slope is scaled accordingly).</remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightOperand.Compute() != 0) expression.LeftOperand.Accept(this);
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>Floor turns a non-constant affine tail into a staircase, so no shortcut applies in general.</remarks>
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    /// <remarks>Ceiling turns a non-constant affine tail into a staircase, so no shortcut applies in general.</remarks>
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
