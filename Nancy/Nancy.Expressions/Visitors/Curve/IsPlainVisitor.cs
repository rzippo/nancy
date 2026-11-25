using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is plain.
/// See <see cref="Curve.IsPlain"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is plain without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsPlain"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsPlainVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsPlain;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsPlain = expression.Value.IsPlain;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsPlain = expression.Compute().IsPlain;

    /// <summary>
    /// Tests if the negated curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation keeps finite values finite and exchanges the two infinities, so the result is plain if and only if the operand is, by [BT08, Definition 1]; see [TBP-PROPERTIES, Proposition 22(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsPlain = operand.IsPlain;
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
    /// Tests if the curve with its origin capped at zero is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.WithZeroOrigin()"/>.
    /// </summary>
    /// <remarks>
    /// Setting the origin to $\min(f(0),0)$ preserves plainness, [TBP-PROPERTIES, Proposition 22(3)].
    /// This method checks whether the operand is plain and returns <see langword="true"/> if so.
    /// Otherwise, it computes the modified curve and checks <see cref="Curve.IsPlain"/> on the resulting curve.
    /// Replacing an infinite origin value with a finite one can also make the curve plain.
    /// </remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsPlain)
            IsPlain = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve with the specified origin value is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.WithOriginAt(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A finite origin value preserves plainness, [TBP-PROPERTIES, Proposition 22(3)].
    /// This method returns <see langword="true"/> if the specified origin value is finite and the operand is plain.
    /// For an infinite origin value, or a finite origin value when the operand is not plain, it computes the modified curve and checks <see cref="Curve.IsPlain"/> on the result.
    /// Replacing the origin can also make the curve plain.
    /// </remarks>
    public virtual void Visit(WithOriginAtExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (expression.OriginValue.IsFinite && operand.IsPlain)
            IsPlain = true;
        else
            _throughCurveComputation(expression);
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
    /// Tests if the delayed curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A delay prepends finite zeros and retains every operand value, so the result is plain if and only if the operand is, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        IsPlain = operand.IsPlain;
    }

    /// <summary>
    /// Tests if the curve brought forward is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Removing a prefix preserves plainness, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method checks whether the operand is plain and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve brought forward and checks <see cref="Curve.IsPlain"/> on the resulting curve.
    /// Removing a prefix can also make the curve plain.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsPlain)
            IsPlain = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the horizontally shifted curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.HorizontalShift(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A finite horizontal shift preserves plainness, [TBP-PROPERTIES, Proposition 22(4)].
    /// This method checks whether the operand is plain and returns <see langword="true"/> if so.
    /// Otherwise, it computes the horizontally shifted curve and checks <see cref="Curve.IsPlain"/> on the resulting curve.
    /// Removing a prefix can also make the curve plain.
    /// </remarks>
    public virtual void Visit(HorizontalShiftExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsPlain)
            IsPlain = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the vertically shifted curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.VerticalShift(Unipi.Nancy.Numerics.Rational, bool, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A finite vertical shift keeps finite values finite and leaves infinities unchanged, so the result is plain if and only if the operand is, [TBP-PROPERTIES, Proposition 22(1)].
    /// For a finite shift, this method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// Otherwise, for a shift of positive or negative infinity, it computes the shifted curve and checks <see cref="Curve.IsPlain"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(VerticalShiftExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor.IsFinite) IsPlain = operand.IsPlain;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <summary>
    /// Tests if the scaled curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is plain, [TBP-PROPERTIES, Proposition 22(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// Multiplication by a nonzero finite factor keeps finite values finite and either preserves or exchanges the two infinities, so the result is plain if and only if the operand is, [TBP-PROPERTIES, Proposition 22(1)].
    /// For a nonzero finite factor, this method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsPlain"/> on the resulting curve.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsPlain = true;
        else if (factor.IsFinite) IsPlain = operand.IsPlain;
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.Floor()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding down keeps finite values finite and leaves infinities unchanged, so the result is plain if and only if the operand is, by [BT08, Definition 1]; see [TBP-PROPERTIES, Proposition 22(2)].
    /// This method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsPlain = operand.IsPlain;
    }

    /// <summary>
    /// Tests if the rounded curve is plain.
    /// See <see cref="Curve.IsPlain"/> and <see cref="Curve.Ceil()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding up keeps finite values finite and leaves infinities unchanged, so the result is plain if and only if the operand is, by [BT08, Definition 1]; see [TBP-PROPERTIES, Proposition 22(2)].
    /// This method returns the operand’s <see cref="CurveExpression.IsPlain"/> property.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsPlain = operand.IsPlain;
    }

}
