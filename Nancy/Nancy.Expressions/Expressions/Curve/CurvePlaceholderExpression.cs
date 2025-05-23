﻿using Unipi.Nancy.Expressions.Visitors;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class describing a placeholder for any curve expression (used for equivalences)
/// </summary>
public record CurvePlaceholderExpression : CurveExpression
{
    /// <summary>
    /// Class describing a placeholder for any curve expression (used for equivalences)
    /// </summary>
    public CurvePlaceholderExpression(
        string curveName,
        ExpressionSettings? settings = null) 
        : base(curveName, settings)
    {
        CurveName = curveName;
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);

    /// <summary>
    /// Name of the curve placeholder.
    /// </summary>
    public string CurveName { get; init; }
}