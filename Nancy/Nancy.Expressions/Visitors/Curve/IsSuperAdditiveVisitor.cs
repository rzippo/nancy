using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is superadditive.
/// See <see cref="Curve.IsSuperAdditive"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is superadditive without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsSuperAdditive"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsSuperAdditiveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsSuperAdditive;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => IsSuperAdditive = expression.Value.IsSuperAdditive;

    private void _throughCurveComputation(IGenericExpression<Curve> expression)
        => IsSuperAdditive = expression.Compute().IsSuperAdditive;

    /// <summary>
    /// Tests if the negated curve is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation turns subadditivity into superadditivity, [DNC18, Section 2.4]; see also [TBP-PROPERTIES, Proposition 23(1)].
    /// This method checks whether the operand is subadditive and returns <see langword="true"/> if so.
    /// Otherwise, it computes the negated curve and checks <see cref="Curve.IsSuperAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsSubAdditive)
            IsSuperAdditive = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the superadditive closure is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.SuperAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The superadditive closure is superadditive, [DNC18, Proposition 2.5 and Section 2.4].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => IsSuperAdditive = true;

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
    /// Tests if the sum is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of superadditive curves is superadditive, [DNC18, Proposition 2.4 and Section 2.4].
    /// This method checks whether every operand is superadditive and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsSuperAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsSuperAdditive))
            IsSuperAdditive = true;
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

    /// <summary>
    /// Tests if the convolution is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.MaxPlusConvolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (max,+) convolution of superadditive curves is superadditive, [DNC18, Proposition 2.4 and Section 2.4].
    /// This method checks whether every operand is superadditive and returns <see langword="true"/> if so.
    /// Otherwise, it computes the convolution and checks <see cref="Curve.IsSuperAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MaxPlusConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsSuperAdditive))
            IsSuperAdditive = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CompositionExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the delayed curve is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Delaying a non-negative, non-decreasing, superadditive curve preserves superadditivity: the prepended zeros satisfy the inequality, and beyond the delay it follows from superadditivity and monotonicity, [TBP-PROPERTIES, Proposition 23(3)].
    /// This method checks whether the operand is non-negative, non-decreasing and superadditive, and returns <see langword="true"/> if so.
    /// Otherwise, it computes the delayed curve and checks <see cref="Curve.IsSuperAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonNegative && operand.IsNonDecreasing && operand.IsSuperAdditive) IsSuperAdditive = true;
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
    /// Tests if the scaled curve is superadditive.
    /// See <see cref="Curve.IsSuperAdditive"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is superadditive, [TBP-PROPERTIES, Proposition 23(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// For a positive finite factor, the scaled curve is superadditive if and only if the operand is; a negative finite factor exchanges subadditivity and superadditivity, [TBP-PROPERTIES, Proposition 23(1)].
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsSuperAdditive"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsSubAdditive"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsSuperAdditive"/>.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsSuperAdditive = true;
        else if (factor.IsFinite && factor > 0) IsSuperAdditive = operand.IsSuperAdditive;
        else if (factor.IsFinite) IsSuperAdditive = operand.IsSubAdditive;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
