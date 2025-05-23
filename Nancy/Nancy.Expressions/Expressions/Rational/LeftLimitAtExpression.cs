﻿using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression that computes the left-limit value of the curve at a specific time.
/// </summary>
public record LeftLimitAtExpression : RationalBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public LeftLimitAtExpression(
        Curve curve,
        string name,
        Rational x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), new RationalNumberExpression(x), expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public LeftLimitAtExpression(
        Curve curve,
        string name,
        RationalExpression x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : this(new ConcreteCurveExpression(curve, name), x, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates the "scale" expression
    /// </summary>
    public LeftLimitAtExpression(
        CurveExpression curve,
        RationalExpression x,
        string expressionName = "",
        ExpressionSettings? settings = null)
        : base(curve, x, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(IRationalExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}