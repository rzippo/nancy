using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is right-continuous.
/// See <see cref="Curve.IsRightContinuous"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine right-continuity without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsRightContinuous"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsRightContinuousVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsRightContinuous;
    
    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
    {
        IsRightContinuous = expression.Value.IsRightContinuous;
    }

    private void _throughCurveComputation(IGenericExpression<Curve> expression) 
        =>
        IsRightContinuous = expression.Compute().IsRightContinuous;
    
    /// <summary>
    /// Tests if the negated curve is right-continuous.
    /// See <see cref="Curve.IsRightContinuous"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// The negated curve is right-continuous if and only if the operand is, [TBP-PROPERTIES, Proposition 17].
    /// This method returns the operand’s <see cref="CurveExpression.IsRightContinuous"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression) 
    {
        var operand = (CurveExpression)expression.Operand;
        IsRightContinuous = operand.IsRightContinuous;
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

    /// <summary>
    /// Tests if the curve obtained by taking right limits is right-continuous.
    /// See <see cref="Curve.IsRightContinuous"/> and <see cref="Curve.ToRightContinuous()"/>.
    /// </summary>
    /// <remarks>
    /// Taking right limits produces a right-continuous curve, [TBP-PROPERTIES, Proposition 17].
    /// This method therefore returns <see langword="true"/>.
    /// </remarks>
    public virtual void Visit(ToRightContinuousExpression expression) 
        => IsRightContinuous = true;

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
    /// <remarks>
    /// The upper pseudo-inverse can be computed only for non-decreasing operands, and if so the result is always right-continuous.
    /// Otherwise, this call propagates <see cref="ArgumentException"/> from the failed computation.
    /// </remarks>
    public virtual void Visit(UpperPseudoInverseExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        if (operand.IsNonDecreasing) IsRightContinuous = true;
        else _throughCurveComputation(expression);
    }

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

    /// <summary>
    /// Tests if the (max,+) convolution is right-continuous.
    /// See <see cref="Curve.IsRightContinuous"/> and <see cref="Curve.MaxPlusConvolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (max,+) convolution of non-decreasing, right-continuous curves is right-continuous, [Zippo23, Proposition 4.4].
    /// [TBP-PROPERTIES, Proposition 18] gives an independent proof with explicit hypotheses for infinite values.
    /// This method checks whether every operand is non-decreasing and right-continuous and returns <see langword="true"/> if so.
    /// Otherwise, it computes the convolution and checks <see cref="Curve.IsRightContinuous"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MaxPlusConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsNonDecreasing && operand.IsRightContinuous))
            IsRightContinuous = true;
        else
            _throughCurveComputation(expression);
    }

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
        => throw new InvalidOperationException(GetType() + ": Cannot perform the check on a placeholder expression!");

    /// <summary>
    /// Tests if the scaled curve is right-continuous.
    /// See <see cref="Curve.IsRightContinuous"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Multiplication by zero gives the zero curve, which is right-continuous, [TBP-PROPERTIES, Proposition 17].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// For a nonzero finite factor, the scaled curve is right-continuous if and only if the operand is, [TBP-PROPERTIES, Proposition 17].
    /// For a nonzero finite factor, this method returns the operand’s <see cref="CurveExpression.IsRightContinuous"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks its right-continuity using <see cref="Curve.IsRightContinuous"/>.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var factor = expression.RightOperand.Compute();
        var operand = (CurveExpression)expression.LeftOperand;
        if (factor == 0) IsRightContinuous = true;
        else if (factor.IsFinite) IsRightContinuous = operand.IsRightContinuous;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
