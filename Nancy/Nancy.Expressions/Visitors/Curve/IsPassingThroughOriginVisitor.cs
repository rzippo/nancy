using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve passes through the origin.
/// See <see cref="Curve.IsPassingThroughOrigin"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is passing through the origin without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
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

    /// <summary>
    /// Tests if the negated curve passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation gives $-f(0)=0$ if and only if $f(0)=0$, [TBP-PROPERTIES, Proposition 21(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsPassingThroughOrigin"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsPassingThroughOrigin = operand.IsPassingThroughOrigin;
    }

    /// <summary>
    /// Tests if the non-negative closure passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.ToNonNegative()"/>.
    /// </summary>
    /// <remarks>
    /// The value at the origin becomes $\max(f(0),0)$, which is zero if and only if $f(0)\leq0$, by [DNC18, Definition 3.2].
    /// This method computes the operand and returns whether its value at zero is non-positive.
    /// </remarks>
    public virtual void Visit(ToNonNegativeExpression expression)
        => IsPassingThroughOrigin = expression.Operand.Compute().ValueAt(Rational.Zero) <= Rational.Zero;

    /// <summary>
    /// Tests if the subadditive closure passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// At the origin, the subadditive closure is the infimum of zero and the positive integer multiples of $f(0)$, by [DNC18, Definition 2.11].
    /// It is zero if and only if $f(0)\geq0$, [DNC18, Proposition 2.5].
    /// This method checks whether the operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the operand and returns whether its value at zero is positive.
    /// </remarks>
    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsPassingThroughOrigin = operand.IsPassingThroughOrigin || operand.Compute().ValueAt(Rational.Zero) > Rational.Zero;
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

    /// <summary>
    /// Tests if the curve with its origin capped at zero passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.WithZeroOrigin()"/>.
    /// </summary>
    /// <remarks>
    /// The new origin value is $\min(f(0),0)$, which is zero if and only if $f(0)\geq0$, [TBP-PROPERTIES, Proposition 21(3)].
    /// This method returns <see langword="true"/> if the operand passes through the origin or is non-negative.
    /// Otherwise, it computes the operand and returns whether its value at zero is non-negative.
    /// </remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsPassingThroughOrigin || operand.IsNonNegative) IsPassingThroughOrigin = true;
        else IsPassingThroughOrigin = operand.Compute().ValueAt(Rational.Zero) >= Rational.Zero;
    }

    /// <summary>
    /// Tests if the curve with the specified origin value passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.WithOriginAt(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// The value at zero is replaced by the specified value, [TBP-PROPERTIES, Proposition 21(3)].
    /// This method returns whether that value is zero.
    /// </remarks>
    public virtual void Visit(WithOriginAtExpression expression)
        => IsPassingThroughOrigin = expression.OriginValue == Rational.Zero;

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the sum passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// Operands that are all zero at the origin give a sum that is zero there, [TBP-PROPERTIES, Proposition 21(2)].
    /// This method checks whether every operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsPassingThroughOrigin))
            IsPassingThroughOrigin = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the difference passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Subtraction(Curve, Curve, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// Operands that are all zero at the origin give a difference that is zero there, [TBP-PROPERTIES, Proposition 21(2)].
    /// This method checks whether every operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the difference and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(SubtractionExpression expression)
    {
        var left = (CurveExpression)expression.LeftOperand;
        var right = (CurveExpression)expression.RightOperand;
        if (left.IsPassingThroughOrigin && right.IsPassingThroughOrigin)
            IsPassingThroughOrigin = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the minimum passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// Operands that are all zero at the origin give a minimum that is zero there, [TBP-PROPERTIES, Proposition 21(2)].
    /// This method checks whether every operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the minimum and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsPassingThroughOrigin))
            IsPassingThroughOrigin = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the maximum passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// Operands that are all zero at the origin give a maximum that is zero there, [TBP-PROPERTIES, Proposition 21(2)].
    /// This method checks whether every operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the maximum and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsPassingThroughOrigin))
            IsPassingThroughOrigin = true;
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

    /// <summary>
    /// Tests if the delayed curve passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A positive finite delay prepends zeros, including at the origin, [TBP-PROPERTIES, Proposition 21(4)].
    /// This method returns <see langword="true"/> for a positive finite delay.
    /// Otherwise, it computes the delayed curve and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var delay = expression.RightOperand.Compute();
        if (delay.IsFinite && delay > 0) IsPassingThroughOrigin = true;
        else _throughCurveComputation(expression);
    }

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

    /// <summary>
    /// Tests if the scaled curve passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which passes through the origin, [TBP-PROPERTIES, Proposition 21(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// For a nonzero finite factor $k$, $kf(0)=0$ if and only if $f(0)=0$, [TBP-PROPERTIES, Proposition 21(1)].
    /// For a nonzero finite factor, this method returns the operand’s <see cref="CurveExpression.IsPassingThroughOrigin"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsPassingThroughOrigin = true;
        else if (factor.IsFinite) IsPassingThroughOrigin = operand.IsPassingThroughOrigin;
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Floor()"/>.
    /// </summary>
    /// <remarks>
    /// Since $\lfloor0\rfloor=0$, rounding preserves a zero value at the origin, [TBP-PROPERTIES, Proposition 21(1)].
    /// This method checks whether the operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsPassingThroughOrigin)
            IsPassingThroughOrigin = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve passes through the origin.
    /// See <see cref="Curve.IsPassingThroughOrigin"/> and <see cref="Curve.Ceil()"/>.
    /// </summary>
    /// <remarks>
    /// Since $\lceil0\rceil=0$, rounding preserves a zero value at the origin, [TBP-PROPERTIES, Proposition 21(1)].
    /// This method checks whether the operand passes through the origin and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsPassingThroughOrigin"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsPassingThroughOrigin)
            IsPassingThroughOrigin = true;
        else
            _throughCurveComputation(expression);
    }

}
