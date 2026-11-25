using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is strictly increasing.
/// See <see cref="Curve.IsIncreasing"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine strict increase without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsIncreasing"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
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

    /// <summary>
    /// Tests if the negated curve is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation reverses strict inequalities, so the negated curve is strictly increasing if and only if the operand is strictly decreasing, [TBP-PROPERTIES, Proposition 16(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsDecreasing"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsIncreasing = operand.IsDecreasing;
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

    /// <summary>
    /// Tests if the sum is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of strictly increasing curves is strictly increasing, [TBP-PROPERTIES, Proposition 16(2)].
    /// This method checks whether every operand is strictly increasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsIncreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsIncreasing))
            IsIncreasing = true;
        else
            _throughCurveComputation(expression);
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

    /// <summary>
    /// Tests if the delayed curve is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A positive finite delay prepends a constant zero interval, so the result is not strictly increasing, [TBP-PROPERTIES, Proposition 16(4)].
    /// This method returns <see langword="false"/> for a positive finite delay.
    /// Otherwise, it computes the delayed curve and checks <see cref="Curve.IsIncreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var delay = expression.RightOperand.Compute();
        if (delay.IsFinite && delay > 0) IsIncreasing = false;
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve brought forward is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Removing a prefix preserves strict increase, [TBP-PROPERTIES, Proposition 16(5)].
    /// This method checks whether the operand is strictly increasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve brought forward and checks <see cref="Curve.IsIncreasing"/> on the resulting curve, since removing a prefix can also make the curve strictly increasing.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsIncreasing)
            IsIncreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the vertically shifted curve is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.VerticalShift(Unipi.Nancy.Numerics.Rational, bool, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite vertical shift, the curve is increasing if and only if the operand is, [TBP-PROPERTIES, Proposition 16(3)].
    /// For a finite shift, this method returns the operand’s <see cref="CurveExpression.IsIncreasing"/> property.
    /// Otherwise, for a shift of positive or negative infinity, it computes the shifted curve and checks <see cref="Curve.IsIncreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(VerticalShiftExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor.IsFinite) IsIncreasing = operand.IsIncreasing;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <summary>
    /// Tests if the scaled curve is strictly increasing.
    /// See <see cref="Curve.IsIncreasing"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is not strictly increasing, [TBP-PROPERTIES, Proposition 16(3)].
    /// This method therefore returns <see langword="false"/> for a zero factor.
    /// </para>
    /// <para>
    /// Multiplication by a positive finite factor preserves strict increase, while multiplication by a negative finite factor exchanges strict increase and strict decrease, [TBP-PROPERTIES, Proposition 16(3)].
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsIncreasing"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsDecreasing"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsIncreasing"/> on the resulting curve.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsIncreasing = false;
        else if (factor.IsFinite && factor > 0) IsIncreasing = operand.IsIncreasing;
        else if (factor.IsFinite) IsIncreasing = operand.IsDecreasing;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
