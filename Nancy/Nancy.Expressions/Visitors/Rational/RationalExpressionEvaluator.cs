﻿using Unipi.Nancy.Expressions.Internals;
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
        => _result = Curve.HorizontalDeviation(expression.LeftExpression.Value, expression.RightExpression.Value);

    /// <inheritdoc />
    public virtual void Visit(VerticalDeviationExpression expression)
        => _result = Curve.HorizontalDeviation(expression.LeftExpression.Value, expression.RightExpression.Value);

    /// <inheritdoc />
    public virtual void Visit(ValueAtExpression expression)
        => _result = expression.LeftExpression.Value.ValueAt(expression.RightExpression.Value);

    /// <inheritdoc />
    public virtual void Visit(LeftLimitAtExpression expression)
        => _result = expression.LeftExpression.Value.LeftLimitAt(expression.RightExpression.Value);

    /// <inheritdoc />
    public virtual void Visit(RightLimitAtExpression expression)
        => _result = expression.LeftExpression.Value.RightLimitAt(expression.RightExpression.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalAdditionExpression expression)
        => _result = expression.Expressions.Aggregate(Rational.Zero, (current, e) => current + e.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalSubtractionExpression expression)
        => _result = expression.LeftExpression.Value - expression.RightExpression.Value;
    
    /// <inheritdoc />
    public virtual void Visit(RationalProductExpression expression)
        => _result = expression.Expressions.Aggregate(Rational.One, (current, e) => current * e.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalDivisionExpression expression)
        => _result = expression.LeftExpression.Value / expression.RightExpression.Value;

    /// <inheritdoc />
    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => _result = expression.Expressions
            .Select(e => e.Value )
            .Aggregate((current, next) => Rational.LeastCommonMultiple(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => _result = expression.Expressions
            .Select(e => e.Value )
            .Aggregate((current, next) => Rational.GreatestCommonDivisor(current, next));

    /// <inheritdoc />
    public virtual void Visit(RationalMinimumExpression expression)
        => _result = expression.Expressions.Aggregate(Rational.PlusInfinity, (current, e) => Rational.Min(current, e.Value));
    
    /// <inheritdoc />
    public virtual void Visit(RationalMaximumExpression expression)
        => _result = expression.Expressions.Aggregate(Rational.MinusInfinity, (current, e) => Rational.Max(current, e.Value));
    
    /// <inheritdoc />
    public virtual void Visit(RationalNumberExpression expression) => _result = expression.Value;

    /// <inheritdoc />
    public virtual void Visit(NegateRationalExpression expression) => _result = Rational.Negate(expression.Expression.Value);

    /// <inheritdoc />
    public virtual void Visit(InvertRationalExpression expression) => _result = Rational.Invert(expression.Expression.Value);

    /// <inheritdoc />
    public virtual void Visit(RationalPlaceholderExpression expression)
        => throw new InvalidOperationException("Can't evaluate an expression with placeholders!");
}