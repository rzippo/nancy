using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Tests if the curve is convex.
/// See <see cref="Curve.IsConvex"/>.
/// </summary>
/// <remarks>
/// Uses the rules documented on this visitor’s <c>Visit</c> overloads to determine convexity without computing the expression when possible.
/// Otherwise, computes the expression and checks <see cref="Curve.IsConvex"/> on the resulting curve.
/// Overloads without a documented rule compute the expression.
/// </remarks>
public class IsConvexVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public bool IsConvex;

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression) => IsConvex = expression.Value.IsConvex;

    private void _throughCurveComputation(IGenericExpression<Curve> expression) =>
        IsConvex = expression.Compute().IsConvex;
    
    /// <summary>
    /// Tests if the negated curve is convex.
    /// See <see cref="Curve.IsConvex"/> and <see cref="Curve.Negate()"/>.
    /// </summary>
    /// <remarks>
    /// The negation of a curve is convex if and only if the curve is concave, [DNC18, §3.3.2, following Definition 3.5].
    /// [TBP-PROPERTIES, Proposition 10] also covers an infinite value at the origin.
    /// This method returns the operand’s <see cref="CurveExpression.IsConcave"/> property.
    /// </remarks>
    public virtual void Visit(NegateExpression expression)
    {
        var operand = (CurveExpression)expression.Operand;
        IsConvex = operand.IsConcave;
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
    /// Tests if the sum is convex.
    /// See <see cref="Curve.IsConvex"/> and <see cref="Curve.Addition(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A sum of convex curves is convex, [DNC18, Proposition 3.13(1)].
    /// [TBP-PROPERTIES, Proposition 11] also covers operands whose value at the origin is $+\infty$.
    /// This method checks whether every operand is convex and returns <see langword="true"/> if so.
    /// Otherwise, it computes the sum and checks <see cref="Curve.IsConvex"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(AdditionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsConvex))
            IsConvex = true;
        else
            _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression) 
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression) 
        => _throughCurveComputation(expression);

    /// <summary>
    /// Tests if the maximum is convex.
    /// See <see cref="Curve.IsConvex"/> and <see cref="Curve.Maximum(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A maximum of convex curves is convex, [DNC18, Proposition 3.13(2)].
    /// [TBP-PROPERTIES, Proposition 12] also covers operands whose value at the origin is $+\infty$.
    /// This method checks whether every operand is convex and returns <see langword="true"/> if so.
    /// Otherwise, it computes the maximum and checks <see cref="Curve.IsConvex"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(MaximumExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsConvex))
            IsConvex = true;
        else
            _throughCurveComputation(expression);
    }

    /// <summary>
    /// Tests if the (min,+) convolution is convex.
    /// See <see cref="Curve.IsConvex"/> and <see cref="Curve.Convolution(IEnumerable{Curve}, ComputationSettings)"/>.
    /// </summary>
    /// <remarks>
    /// A (min,+) convolution of convex curves is convex, [DNC18, Proposition 3.13(3)].
    /// [TBP-PROPERTIES, Proposition 15] also covers operands whose value at the origin is $+\infty$.
    /// This method checks whether every operand is convex and returns <see langword="true"/> if so.
    /// Otherwise, it computes the convolution and checks <see cref="Curve.IsConvex"/> on the resulting curve.
    /// </remarks>
    public virtual void Visit(ConvolutionExpression expression)
    {
        if (expression.Operands.Count > 0 && expression.Operands.Cast<CurveExpression>().All(operand => operand.IsConvex))
            IsConvex = true;
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
    /// Tests if the scaled curve is convex.
    /// See <see cref="Curve.IsConvex"/> and <see cref="Curve.Scale(Unipi.Nancy.Numerics.Rational)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The zero curve is convex by [DNC18, Definition 3.5].
    /// Scaling it gives the zero curve, including for an infinite factor under Nancy’s convention that zero times infinity is zero; see [TBP-PROPERTIES, Proposition 13].
    /// If the operand is already known to be zero, this method returns <see langword="true"/> without computing it.
    /// </para>
    /// <para>
    /// Multiplication by zero gives the zero curve, including at infinite operand values under the same convention; see [TBP-PROPERTIES, Proposition 13].
    /// This method therefore returns <see langword="true"/> for a zero factor.
    /// </para>
    /// <para>
    /// Multiplication by a positive finite factor preserves convexity, while multiplication by a negative finite factor exchanges convexity and concavity; these follow directly from [DNC18, Definitions 3.4–3.5].
    /// [TBP-PROPERTIES, Proposition 13] also covers an infinite value at the origin.
    /// For a positive finite factor, this method returns the operand’s <see cref="CurveExpression.IsConvex"/> property.
    /// For a negative finite factor, it returns the operand’s <see cref="CurveExpression.IsConcave"/> property.
    /// </para>
    /// <para>
    /// Otherwise, for a factor of positive or negative infinity, this method computes the scaled curve and checks its convexity using <see cref="Curve.IsConvex"/>.
    /// </para>
    /// </remarks>
    public virtual void Visit(ScaleExpression expression)
    {
        var operand = (CurveExpression)expression.LeftOperand;
        if (operand.ZeroIfKnown == true)
        {
            IsConvex = true;
            return;
        }

        var factor = expression.RightOperand.Compute();
        if (factor == 0) IsConvex = true;
        else if (factor.IsFinite && factor > 0) IsConvex = operand.IsConvex;
        else if (factor.IsFinite) IsConvex = operand.IsConcave;
        else _throughCurveComputation(expression);
    }

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => _throughCurveComputation(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => _throughCurveComputation(expression);
}
