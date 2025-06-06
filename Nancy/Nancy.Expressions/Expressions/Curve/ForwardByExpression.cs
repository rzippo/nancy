﻿using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is to forward a curve by a certain time.
/// Computes $f(t + T)$, with $T \ge 0$.
/// </summary>
/// <remarks>
/// Computing the expression will throw an <see cref="ArgumentException"/> 
/// if the time argument turns out to be either negative or infinite.
/// </remarks>
public record ForwardByExpression : CurveBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Constructs an expression which forwards a curve by a certain time.
    /// Computes $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    public ForwardByExpression(
        Curve curveL, 
        string nameL, 
        Rational time, 
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(time), expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which forwards a curve by a certain time.
    /// Computes $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    public ForwardByExpression(
        Curve curveL, 
        string nameL, 
        RationalExpression rightExpression,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which forwards a curve by a certain time.
    /// Computes $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the time argument turns out to be either negative or infinite.
    /// </remarks>
    public ForwardByExpression(
        CurveExpression leftExpression,
        RationalExpression rightExpression,
        string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}