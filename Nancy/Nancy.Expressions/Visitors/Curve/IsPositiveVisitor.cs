using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is strictly positive.
/// See <see cref="Curve.IsPositive"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is strictly positive without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsPositive"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsPositiveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor.
    /// </summary>
    public bool IsPositive;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsPositive = expression.Compute().IsPositive;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsPositive = expression.Value.IsPositive;

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
        => _throughCurveComputation(expression);

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
    public virtual void Visit(AdditionExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the minimum is strictly positive.
    /// See <see cref="Curve.IsPositive"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A minimum is strictly positive if and only if every operand is strictly positive, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method returns whether every operand has that property.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
    {
        if (expression.Operands.Count > 0)
            IsPositive = expression.Operands.Cast<CurveExpression>().All(operand => operand.IsPositive);
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the maximum is strictly positive.
    /// See <see cref="Curve.IsPositive"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A maximum bounds each operand from above, so a strictly positive operand makes the maximum strictly positive, [TBP-PROPERTIES, Proposition 24(2)].
    /// This method returns <see langword="true"/> if any operand is strictly positive.
    /// If every operand is non-positive, the maximum is non-positive and therefore is not strictly positive, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method then returns <see langword="false"/> if every operand is non-positive.
    /// If neither condition holds, it computes the maximum and checks <see cref="Curve.IsPositive"/> on the resulting curve.
    /// Different operands can supply the maximum at different times, so these sufficient conditions do not cover every case.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Cast<CurveExpression>().Any(operand => operand.IsPositive))
            IsPositive = true;
        else if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonPositive))
            IsPositive = false;
        else
            _throughCurveComputation(expression);
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
        => _throughCurveComputation(expression);

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
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
