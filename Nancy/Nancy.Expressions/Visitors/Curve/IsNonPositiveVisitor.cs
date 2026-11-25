using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is non-positive.
/// See <see cref="Curve.IsNonPositive"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is non-positive without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsNonPositive"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsNonPositiveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor.
    /// </summary>
    public bool IsNonPositive;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsNonPositive = expression.Compute().IsNonPositive;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsNonPositive = expression.Value.IsNonPositive;

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
    /// Tests if the minimum is non-positive.
    /// See <see cref="Curve.IsNonPositive"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A minimum is no greater than any operand, so a non-positive operand makes the minimum non-positive, [TBP-PROPERTIES, Proposition 24(3)].
    /// This method returns <see langword="true"/> if any operand is non-positive.
    /// If every operand is strictly positive, the minimum is strictly positive and therefore is not non-positive, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method then returns <see langword="false"/> if every operand is strictly positive.
    /// If neither condition holds, it computes the minimum and checks <see cref="Curve.IsNonPositive"/> on the resulting curve.
    /// Different operands can supply the minimum at different times, so these sufficient conditions do not cover every case.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
    {
        if (expression.Operands.Cast<CurveExpression>().Any(operand => operand.IsNonPositive))
            IsNonPositive = true;
        else if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsPositive))
            IsNonPositive = false;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the maximum is non-positive.
    /// See <see cref="Curve.IsNonPositive"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A maximum is non-positive if and only if every operand is non-positive, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method returns whether every operand has that property.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Count > 0)
            IsNonPositive = expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonPositive);
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
