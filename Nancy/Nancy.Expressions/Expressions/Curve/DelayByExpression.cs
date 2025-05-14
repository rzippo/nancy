using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is to delay a curve, adding a 0-valued padding at the start.
/// Computes $f(t - T)$, with $T \ge 0$.
/// </summary>
/// <remarks>
/// Computing the expression will throw an <see cref="ArgumentException"/> 
/// if the delay argument turns out to be either negative or infinite.
/// </remarks>
public record DelayByExpression : CurveBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Constructs an expression which delays a curve, adding a 0-valued padding at the start.
    /// Computes $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    public DelayByExpression(
        Curve curveL,
        string nameL,
        Rational delay,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(delay), expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which delays a curve, adding a 0-valued padding at the start.
    /// Computes $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    public DelayByExpression(
        Curve curveL,
        string nameL,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which delays a curve, adding a 0-valued padding at the start.
    /// Computes $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> 
    /// if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    public DelayByExpression(
        CurveExpression leftExpression,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : base(leftExpression, rightExpression, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
    
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}