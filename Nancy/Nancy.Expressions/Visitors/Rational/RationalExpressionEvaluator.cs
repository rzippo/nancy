using System.Numerics;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to compute the value of a rational expression.
/// </summary>
public record RationalExpressionEvaluator : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    private Rational _result = Rational.Zero;

    /// <summary>
    /// Visits the expression and returns tht result
    /// </summary>
    public Rational GetResult(RationalExpression expression)
    {
        expression.Accept(this);
        return _result;
    }

    /// <inheritdoc />
    public virtual void Visit(HorizontalDeviationExpression expression)
        => _result = Curve.HorizontalDeviation(expression.LeftOperand.Value, expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(VerticalDeviationExpression expression)
        => _result = Curve.VerticalDeviation(expression.LeftOperand.Value, expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(ZDeviationExpression expression)
        => _result = Curve.ZDeviation(expression.LeftOperand.Value, expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(ValueAtExpression expression)
        => _result = expression.LeftOperand.Value.ValueAt(expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(LeftLimitAtExpression expression)
        => _result = expression.LeftOperand.Value.LeftLimitAt(expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(RightLimitAtExpression expression)
        => _result = expression.LeftOperand.Value.RightLimitAt(expression.RightOperand.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalAdditionExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.Zero, (current, e) => current + e.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalSubtractionExpression expression)
        => _result = expression.LeftOperand.Value - expression.RightOperand.Value;
    
    /// <inheritdoc />
    public virtual void Visit(RationalProductExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.One, (current, e) => current * e.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalDivisionExpression expression)
        => _result = expression.LeftOperand.Value / expression.RightOperand.Value;

    /// <inheritdoc />
    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => _result = expression.FlattenOperands()
            .Select(e => e.Value )
            .Aggregate((current, next) => Rational.LeastCommonMultiple(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => _result = expression.FlattenOperands()
            .Select(e => e.Value )
            .Aggregate((current, next) => Rational.GreatestCommonDivisor(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalMinimumExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.PlusInfinity, (current, e) => Rational.Min(current, e.Value));
    
    /// <inheritdoc />
    public virtual void Visit(RationalMaximumExpression expression)
        => _result = expression.FlattenOperands().Aggregate(Rational.MinusInfinity, (current, e) => Rational.Max(current, e.Value));
    
    /// <inheritdoc />
    public virtual void Visit(RationalNumberExpression expression) => _result = expression.Value;

    /// <inheritdoc />
    public virtual void Visit(NegateRationalExpression expression) => _result = Rational.Negate(expression.Operand.Value);

    /// <inheritdoc />
    public virtual void Visit(InvertRationalExpression expression) => _result = Rational.Invert(expression.Operand.Value);

    public virtual void Visit(RationalAbsoluteValueExpression expression) => _result = Rational.Abs(expression.Operand.Value);

    public virtual void Visit(RationalModuloExpression expression) => _result = expression.LeftOperand.Value % expression.RightOperand.Value;

    public virtual void Visit(RationalPowerExpression expression) => _result = Rational.Pow(expression.LeftOperand.Value, (System.Numerics.BigInteger)expression.RightOperand.Value);

    public virtual void Visit(RationalPlaceholderExpression expression)
        => throw new InvalidOperationException("Can't evaluate an expression with placeholders!");

    /// <inheritdoc />
    public virtual void Visit(RationalFloorExpression expression) => _result = expression.Operand.Value.Floor();

    /// <inheritdoc />
    public virtual void Visit(RationalCeilExpression expression) => _result = expression.Operand.Value.Ceil();

    /// <inheritdoc />
    public virtual void Visit(SupValueExpression expression) => _result = expression.Operand.Value.SupValue();

    /// <inheritdoc />
    public virtual void Visit(InfValueExpression expression) => _result = expression.Operand.Value.InfValue();

    /// <inheritdoc />
    public virtual void Visit(MaxValueExpression expression)
        => _result = expression.Operand.Value.MaxValue() ??
                     throw new InvalidOperationException(
                         "The curve does not attain a maximum value (its supremum is not attained); use SupValue() instead.");

    /// <inheritdoc />
    public virtual void Visit(MinValueExpression expression)
        => _result = expression.Operand.Value.MinValue() ??
                     throw new InvalidOperationException(
                         "The curve does not attain a minimum value (its infimum is not attained); use InfValue() instead.");
}