using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is non-negative.
/// See <see cref="Curve.IsNonNegative"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine whether the curve is non-negative without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsNonNegativeVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsNonNegative;
    
    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) 
        => IsNonNegative = expression.Value.IsNonNegative;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsNonNegative = expression.Compute().IsNonNegative;

    /// <summary>
    /// Tests if the negated curve is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// Negation reverses signs, so the negated curve is non-negative if and only if the operand is non-positive, [TBP-PROPERTIES, Proposition 20(1)].
    /// This method returns the operand’s <see cref="CurveExpression.IsNonPositive"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsNonNegative = operand.IsNonPositive;
    }

    /// <summary>
    /// Tests if the non-negative closure is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ToNonNegative()"/>.
    /// </summary>
    /// <remarks>
    /// The non-negative closure is the maximum with zero, [DNC18, Definition 3.2].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(ToNonNegativeExpression expression) 
        => IsNonNegative = true;

    /// <summary>
    /// Tests if the subadditive closure is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.SubAdditiveClosure(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The subadditive closure of a non-negative, non-decreasing curve is non-negative, [DNC18, §3.2, p.44].
    /// This method checks whether the operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the closure and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(SubAdditiveClosureExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing && operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the upper non-decreasing closure is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ToUpperNonDecreasing(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The supremum over earlier times of non-negative values is non-negative, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the upper non-decreasing closure and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ToUpperNonDecreasingExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonDecreasingExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the upper non-increasing closure is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ToUpperNonIncreasing(ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The supremum over later times of non-negative values is non-negative, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the upper non-increasing closure and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the curve obtained by taking left limits is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ToLeftContinuous()"/>.
    /// </summary>
    /// <remarks>
    /// A left limit of non-negative values is non-negative, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve obtained by taking left limits and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ToLeftContinuousExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve obtained by taking right limits is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ToRightContinuous()"/>.
    /// </summary>
    /// <remarks>
    /// A right limit of non-negative values is non-negative, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve obtained by taking right limits and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ToRightContinuousExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve with its origin capped at zero is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.WithZeroOrigin()"/>.
    /// </summary>
    /// <remarks>
    /// Setting the origin to $\min(f(0),0)$ preserves non-negativity when the operand is non-negative, [TBP-PROPERTIES, Proposition 20(5)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the modified curve and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(WithZeroOriginExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the curve with the specified origin value is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.WithOriginAt(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A negative origin value makes the curve not non-negative, while a non-negative replacement preserves non-negativity of a non-negative operand, [TBP-PROPERTIES, Proposition 20(5)].
    /// This method returns <see langword="false"/> if the specified origin value is negative.
    /// For a non-negative origin value, it checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the modified curve and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(WithOriginAtExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (expression.OriginValue < 0)
            IsNonNegative = false;
        else if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the sum is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of non-negative, non-decreasing curves is non-negative, [DNC18, §3.2, p.44].
    /// This method checks whether every operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonDecreasing && operand.IsNonNegative))
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the minimum is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Minimum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A minimum is non-negative if and only if every operand is non-negative, [TBP-PROPERTIES, Proposition 20(2)].
    /// This method returns whether every operand is non-negative.
    /// </remarks>
    public virtual void Visit(MinimumExpression expression)
        => IsNonNegative = expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonNegative);

    /// <summary>
    /// Tests if the maximum is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A maximum bounds each operand from above, so a non-negative operand makes the maximum non-negative, [TBP-PROPERTIES, Proposition 20(2)].
    /// This method returns <see langword="true"/> if any operand is non-negative.
    /// A nonempty finite maximum of strictly negative curves is strictly negative, [TBP-PROPERTIES, Proposition 20(2)].
    /// If every operand has <see cref="CurveExpression.IsNegative"/> equal to <see langword="true"/>, this method returns <see langword="false"/>.
    /// If neither condition holds, it computes the maximum and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// Operands that are negative at different times can still have a non-negative maximum.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Cast<CurveExpression>().Any(operand => operand.IsNonNegative))
            IsNonNegative = true;
        else if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNegative))
            IsNonNegative = false;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the (min,+) convolution is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (min,+) convolution of non-negative curves is non-negative, [DNC18, Lemma 2.3].
    /// This method checks whether every operand is non-negative and non-decreasing and returns <see langword="true"/> if so.
    /// Otherwise, it computes the (min,+) convolution and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonDecreasing && operand.IsNonNegative))
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the (min,+) deconvolution is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Deconvolution(Curve, Curve, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// The deconvolution at $t$ is at least $f(t)-g(0)$, [TBP-PROPERTIES, Proposition 20(6)].
    /// If $f$ is non-decreasing, this is at least $f(0)-g(0)$, so a defined, non-negative origin difference suffices.
    /// This method first checks whether the left operand is non-decreasing.
    /// If so, it returns <see langword="true"/> when the left operand is non-negative and the right operand's value at zero is non-positive, or when their origin difference is defined and non-negative.
    /// Origin values are obtained from <see cref="CurveExpression.IsPassingThroughOrigin"/> when that property is true; otherwise, the corresponding operand is computed.
    /// Otherwise, it computes the deconvolution and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(DeconvolutionExpression expression)
    {
        var left = (CurveExpression)expression.LeftOperand;
        var right = (CurveExpression)expression.RightOperand;
        if (left.IsNonDecreasing)
        {
            var rightAtZero = right.IsPassingThroughOrigin ? 0 : right.Compute().ValueAt(0);
            if (rightAtZero <= 0 && left.IsNonNegative)
            {
                IsNonNegative = true;
                return;
            }

            var leftAtZero = left.IsPassingThroughOrigin ? 0 : left.Compute().ValueAt(0);
            // Compare without subtracting: equal infinities have no defined difference.
            if (leftAtZero > rightAtZero || (leftAtZero.IsFinite && leftAtZero == rightAtZero))
            {
                IsNonNegative = true;
                return;
            }
        }

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
    /// Tests if the delayed curve is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.DelayBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// A delay retains every operand value and prepends only zeros, so the result is non-negative if and only if the operand is, [TBP-PROPERTIES, Proposition 20(4)].
    /// This method returns the operand’s <see cref="CurveExpression.IsNonNegative"/> property.
    /// </remarks>
    public virtual void Visit(DelayByExpression expression) 
    {
        var operand = (CurveExpression)expression.LeftOperand;
        IsNonNegative = operand.IsNonNegative;
    }

    /// <summary>
    /// Tests if the curve brought forward is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.ForwardBy(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// Removing a prefix preserves non-negativity, [TBP-PROPERTIES, Proposition 20(4)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the curve brought forward and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// Removing a negative prefix can make the remaining curve non-negative.
    /// </remarks>
    public virtual void Visit(ForwardByExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
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
    /// Tests if the scaled curve is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is non-negative, [TBP-PROPERTIES, Proposition 20(1)].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// Multiplication by a positive finite factor preserves signs, while multiplication by a negative finite factor reverses them, [TBP-PROPERTIES, Proposition 20(1)].
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsNonNegative"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsNonPositive"/> property.
    /// </para>
    /// <para>
    /// Multiplication by $+\infty$ maps negative values to $-\infty$, zero to zero and positive values to $+\infty$, so the result is non-negative if and only if the operand is, [TBP-PROPERTIES, Proposition 20(1)].
    /// For a factor of positive infinity, this method returns the operand’s <see cref="CurveExpression.IsNonNegative"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of negative infinity, this method computes the scaled curve and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsNonNegative = true;
        else if (factor.IsFinite && factor > 0) IsNonNegative = operand.IsNonNegative;
        else if (factor.IsFinite) IsNonNegative = operand.IsNonPositive;
        else if (factor == Rational.PlusInfinity) IsNonNegative = operand.IsNonNegative;
        else _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Floor()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding down preserves non-negativity, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(FloorExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the rounded curve is non-negative.
    /// See <see cref="Curve.IsNonNegative"/> and <see cref="Curve.Ceil()"/>.
    /// </summary>
    /// <remarks>
    /// Rounding up preserves non-negativity, [TBP-PROPERTIES, Proposition 20(3)].
    /// This method checks whether the operand is non-negative and returns <see langword="true"/> if so.
    /// Otherwise, it computes the rounded curve and checks <see cref="Curve.IsNonNegative"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(CeilExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonNegative)
            IsNonNegative = true;
        else
            _throughCurveComputation(expression);
    }

}
