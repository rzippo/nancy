using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is ultimately constant.
/// See <see cref="Curve.IsUltimatelyConstant"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is ultimately constant without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsUltimatelyConstant"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsUltimatelyConstantVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsUltimatelyConstant;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsUltimatelyConstant = expression.Value.IsUltimatelyConstant;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsUltimatelyConstant = expression.Compute().IsUltimatelyConstant;

    /// <summary>
    /// Tests if the negated curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// The negated curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
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

    /// <summary>
    /// Tests if the curve with its origin capped at zero is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.WithZeroOrigin()"/>.
    /// </summary>
    /// <remarks>
    /// Changing the origin leaves the curve unchanged at positive times, so the result is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(3)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
    }

    /// <summary>
    /// Tests if the curve with the specified origin value is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.WithOriginAt(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Changing the origin leaves the curve unchanged at positive times, so the result is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(3)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(WithOriginAtExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
    }

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

    /// <summary>
    /// Tests if the delayed curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite time shift, the curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
    }

    /// <summary>
    /// Tests if the curve brought forward is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite time shift, the curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
    }

    /// <summary>
    /// Tests if the horizontally shifted curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.HorizontalShift(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite time shift, the curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </remarks>
    public virtual void Visit(HorizontalShiftExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        IsUltimatelyConstant = operand.IsUltimatelyConstant;
    }

    /// <summary>
    /// Tests if the vertically shifted curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.VerticalShift(Unipi.Nancy.Numerics.Rational, bool, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite vertical shift, the curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(1)].
    /// For a finite shift, this method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// Otherwise, for a shift of positive or negative infinity, it computes the shifted curve and checks <see cref="Curve.IsUltimatelyConstant"/>.
    /// </remarks>
    public virtual void Visit(VerticalShiftExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor.IsFinite) IsUltimatelyConstant = operand.IsUltimatelyConstant;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <summary>
    /// Tests if the scaled curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is ultimately constant, [TBP-PROPERTIES, Proposition 22(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// For a nonzero finite factor, the scaled curve is ultimately constant if and only if the operand is, [TBP-PROPERTIES, Proposition 22(1)].
    /// For a nonzero finite factor, this method returns the operand’s <see cref="CurveExpression.IsUltimatelyConstant"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsUltimatelyConstant"/>.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsUltimatelyConstant = true;
        else if (factor.IsFinite) IsUltimatelyConstant = operand.IsUltimatelyConstant;
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.Floor()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding down a finite constant tail gives a finite constant tail, [TBP-PROPERTIES, Proposition 22(2)].
    /// This method checks whether the operand is ultimately constant and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsUltimatelyConstant"/> on the resulting curve.
    /// Rounding can also make a non-constant tail constant.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsUltimatelyConstant)
            IsUltimatelyConstant = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is ultimately constant.
    /// See <see cref="Curve.IsUltimatelyConstant"/> and <see cref="Curve.Ceil()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding up a finite constant tail gives a finite constant tail, [TBP-PROPERTIES, Proposition 22(2)].
    /// This method checks whether the operand is ultimately constant and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsUltimatelyConstant"/> on the resulting curve.
    /// Rounding can also make a non-constant tail constant.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsUltimatelyConstant)
            IsUltimatelyConstant = true;
        else
            _throughCurveComputation(expression);
    }

}
