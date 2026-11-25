using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is subadditive.
/// See <see cref="Curve.IsSubAdditive"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is subadditive without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsSubAdditive"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsSubAdditiveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsSubAdditive;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) 
        => IsSubAdditive = expression.Value.IsSubAdditive;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsSubAdditive = expression.Compute().IsSubAdditive;

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the subadditive closure is subadditive.
    /// See <see cref="Curve.IsSubAdditive"/> and <see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The subadditive closure is subadditive, [DNC18, Proposition 2.5].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(SubAdditiveClosureExpression expression) 
        => IsSubAdditive = true;

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
    /// Tests if the sum is subadditive.
    /// See <see cref="Curve.IsSubAdditive"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of subadditive curves is subadditive, [DNC18, Proposition 2.4].
    /// This method checks whether every operand is subadditive and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsSubAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsSubAdditive))
            IsSubAdditive = true;
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

    /// <summary>
    /// Tests if the convolution is subadditive.
    /// See <see cref="Curve.IsSubAdditive"/> and <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (min,+) convolution of subadditive curves is subadditive, [DNC18, Proposition 2.4].
    /// This method checks whether every operand is subadditive and returns <see langword="true"/> if so.
    /// Otherwise, it computes the convolution and checks <see cref="Curve.IsSubAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsSubAdditive))
            IsSubAdditive = true;
        else
            _throughCurveComputation(expression);
    }

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

    /// <summary>
    /// Tests if the curve brought forward is subadditive.
    /// See <see cref="Curve.IsSubAdditive"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Bringing a non-decreasing, subadditive curve forward preserves subadditivity, [TBP-PROPERTIES, Proposition 23(2)].
    /// This method checks whether the operand is non-decreasing and subadditive, and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve brought forward and checks <see cref="Curve.IsSubAdditive"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonDecreasing && operand.IsSubAdditive) IsSubAdditive = true;
        else _throughCurveComputation(expression);
    }

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
    /// Tests if the scaled curve is subadditive.
    /// See <see cref="Curve.IsSubAdditive"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is subadditive, [TBP-PROPERTIES, Proposition 23(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// For a positive finite factor, the scaled curve is subadditive if and only if the operand is; a negative finite factor exchanges subadditivity and superadditivity, [TBP-PROPERTIES, Proposition 23(1)].
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsSubAdditive"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsSuperAdditive"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsSubAdditive"/>.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsSubAdditive = true;
        else if (factor.IsFinite && factor > 0) IsSubAdditive = operand.IsSubAdditive;
        else if (factor.IsFinite) IsSubAdditive = operand.IsSuperAdditive;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
