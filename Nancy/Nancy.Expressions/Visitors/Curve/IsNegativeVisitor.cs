using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is strictly negative.
/// See <see cref="Curve.IsNegative"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is strictly negative without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsNegative"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsNegativeVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor.
    /// </summary>
    public bool IsNegative;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsNegative = expression.Compute().IsNegative;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsNegative = expression.Value.IsNegative;

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
    /// Tests if the minimum is strictly negative.
    /// See <see cref="Curve.IsNegative"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A minimum is no greater than any operand, so a strictly negative operand makes the minimum strictly negative, [TBP-PROPERTIES, Proposition 24(3)].
    /// This method returns <see langword="true"/> if any operand is strictly negative.
    /// If every operand is non-negative, the minimum is non-negative and therefore is not strictly negative, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method then returns <see langword="false"/> if every operand is non-negative.
    /// If neither condition holds, it computes the minimum and checks <see cref="Curve.IsNegative"/> on the resulting curve.
    /// Different operands can supply the minimum at different times, so these sufficient conditions do not cover every case.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
    {
        if (expression.Operands.Cast<CurveExpression>().Any(operand => operand.IsNegative))
            IsNegative = true;
        else if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonNegative))
            IsNegative = false;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the maximum is strictly negative.
    /// See <see cref="Curve.IsNegative"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A maximum is strictly negative if and only if every operand is strictly negative, [TBP-PROPERTIES, Proposition 24(1)].
    /// This method returns whether every operand has that property.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Count > 0)
            IsNegative = expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNegative);
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
