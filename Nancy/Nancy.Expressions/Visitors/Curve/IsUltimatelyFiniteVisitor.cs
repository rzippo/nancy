using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor used to check whether the value of a curve expression is ultimately finite. Implemented minimizing the
/// amount of computations.
/// </summary>
public class IsUltimatelyFiniteVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsUltimatelyFinite;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsUltimatelyFinite = expression.Value.IsUltimatelyFinite;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsUltimatelyFinite = expression.Compute().IsUltimatelyFinite;

    /// <inheritdoc />
    /// <remarks>Negation does not change whether the tail is finite or infinite.</remarks>
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
    public virtual void Visit(ToLeftContinuousExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToRightContinuousExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    /// <remarks>A vertical repositioning does not change whether the tail is finite or infinite.</remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
        => expression.Expression.Accept(this);

    /// <inheritdoc />
    /// <remarks>A vertical repositioning does not change whether the tail is finite or infinite.</remarks>
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
    /// <remarks>A time shift does not change whether the tail is finite or infinite.</remarks>
    public virtual void Visit(DelayByExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    /// <remarks>A time shift does not change whether the tail is finite or infinite.</remarks>
    public virtual void Visit(ForwardByExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    /// <remarks>A time shift does not change whether the tail is finite or infinite.</remarks>
    public virtual void Visit(HorizontalShiftExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    /// <remarks>A vertical shift does not change whether the tail is finite or infinite.</remarks>
    public virtual void Visit(VerticalShiftExpression expression)
        => expression.LeftExpression.Accept(this);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
    {
        if (expression.RightExpression.Compute() != 0) expression.LeftExpression.Accept(this);
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>$\lfloor x \rfloor$ is finite iff $x$ is finite.</remarks>
    public virtual void Visit(FloorExpression expression)
        => expression.Expression.Accept(this);

    /// <inheritdoc />
    /// <remarks>$\lceil x \rceil$ is finite iff $x$ is finite.</remarks>
    public virtual void Visit(CeilExpression expression)
        => expression.Expression.Accept(this);
}
