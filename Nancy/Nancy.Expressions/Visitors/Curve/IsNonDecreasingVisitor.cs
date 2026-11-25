using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is non-decreasing.
/// See <see cref="Curve.IsNonDecreasing"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is non-decreasing without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsNonDecreasingVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsNonDecreasing;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) 
        => IsNonDecreasing = expression.Value.IsNonDecreasing;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsNonDecreasing = expression.Compute().IsNonDecreasing;

    /// <summary>
    /// Tests if the negated curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation reverses inequalities, so the negated curve is non-decreasing if and only if the operand is non-increasing, [TBP-PROPERTIES, Proposition 19(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsNonIncreasing"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsNonDecreasing = operand.IsNonIncreasing;
    }

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the subadditive closure is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The subadditive closure of a non-negative, non-decreasing curve is non-decreasing, [DNC18, §3.2, p.44].
    /// This method checks whether the operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the closure and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative && operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the upper non-decreasing closure is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.ToUpperNonDecreasing(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The upper non-decreasing closure is non-decreasing by definition, [DNC18, Definition 3.2 and Lemma 3.1].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(ToUpperNonDecreasingExpression expression) 
        => IsNonDecreasing = true;

    /// <summary>
    /// Tests if the lower non-decreasing closure is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.ToLowerNonDecreasing(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The lower non-decreasing closure takes the infimum over $u \ge t$, which is non-decreasing as that set shrinks, [TBP-PROPERTIES, Proposition 19(5)].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(ToLowerNonDecreasingExpression expression) 
        => IsNonDecreasing = true;

    /// <inheritdoc />
    public virtual void Visit(ToLeftContinuousExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
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
    /// <remarks>
    /// The lower pseudo-inverse can be computed only for non-decreasing operands, and if so the result is always non-decreasing.
    /// Otherwise, this call propagates <see cref="ArgumentException"/> from the failed computation.
    /// </remarks>
    public virtual void Visit(LowerPseudoInverseExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    /// <remarks>
    /// The upper pseudo-inverse can be computed only for non-decreasing operands, and if so the result is always non-decreasing.
    /// Otherwise, this call propagates <see cref="ArgumentException"/> from the failed computation.
    /// </remarks>
    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the sum is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of non-negative, non-decreasing curves is non-decreasing, [DNC18, §3.2, p.44].
    /// This method checks whether every operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonNegative && operand.IsNonDecreasing))
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the minimum is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A minimum of non-negative, non-decreasing curves is non-decreasing, [DNC18, Lemma 2.3].
    /// This method checks whether every operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the minimum and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonNegative && operand.IsNonDecreasing))
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the (min,+) convolution is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (min,+) convolution of non-negative, non-decreasing curves is non-decreasing, [DNC18, Lemma 2.3].
    /// This method checks whether every operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the (min,+) convolution and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonNegative && operand.IsNonDecreasing))
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the (min,+) deconvolution is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Deconvolution(Curve, Curve, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// For a non-decreasing $f$, each difference $f(t+u)-g(u)$ is non-decreasing in $t$, so their supremum is non-decreasing, [TBP-PROPERTIES, Proposition 19(7)].
    /// This method checks whether the left operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the deconvolution and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DeconvolutionExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonNegative && operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

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
    /// Tests if the delayed curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A delay prepends zeros, which do not exceed the values of a non-negative, non-decreasing operand, [TBP-PROPERTIES, Proposition 19(4)].
    /// This method checks whether the operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the delayed curve and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonNegative && operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve brought forward is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Removing a prefix preserves non-decrease, [TBP-PROPERTIES, Proposition 19(4)].
    /// This method checks whether the operand is non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve brought forward and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// Removing a prefix can also make the curve non-decreasing.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the vertically shifted curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.VerticalShift(Unipi.Nancy.Numerics.Rational, bool, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// After a finite vertical shift, the curve is non-decreasing if and only if the operand is, [TBP-PROPERTIES, Proposition 19(1)].
    /// For a finite shift, this method returns the operand’s <see cref="CurveExpression.IsNonDecreasing"/> property.
    /// Otherwise, for a shift of positive or negative infinity, it computes the shifted curve and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(VerticalShiftExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor.IsFinite) IsNonDecreasing = operand.IsNonDecreasing;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <summary>
    /// Tests if the scaled curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is non-decreasing, [TBP-PROPERTIES, Proposition 19(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// Multiplication by a positive finite factor preserves non-decrease, while multiplication by a negative finite factor exchanges non-decrease and non-increase, [TBP-PROPERTIES, Proposition 19(1)].
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsNonDecreasing"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsNonIncreasing"/> property.
    /// </para>
    /// <para>
    /// Multiplication by $+\infty$ maps negative values to $-\infty$, zero to zero and positive values to $+\infty$, preserving non-decrease, [TBP-PROPERTIES, Proposition 19(2)].
    /// For a factor of positive infinity, this method returns <see langword="true"/> if the operand is non-decreasing.
    /// </para>
    /// <para>
    /// For a factor of negative infinity, or positive infinity when the operand is not non-decreasing, this method computes the scaled curve and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// Infinite scaling may remove decreases within values of the same sign.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsNonDecreasing = true;
        else if (factor.IsFinite && factor > 0) IsNonDecreasing = operand.IsNonDecreasing;
        else if (factor.IsFinite) IsNonDecreasing = operand.IsNonIncreasing;
        else if (factor == Rational.PlusInfinity)
        {
            if (operand.IsNonDecreasing)
                IsNonDecreasing = true;
            else
                _throughCurveComputation(expression);
        }
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Floor()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding down preserves non-decrease, [TBP-PROPERTIES, Proposition 19(3)].
    /// This method checks whether the operand is non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is non-decreasing.
    /// See <see cref="Curve.IsNonDecreasing"/> and <see cref="Curve.Ceil()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding up preserves non-decrease, [TBP-PROPERTIES, Proposition 19(3)].
    /// This method checks whether the operand is non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsNonDecreasing"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing)
            IsNonDecreasing = true;
        else
            _throughCurveComputation(expression);
    }

}
