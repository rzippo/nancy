using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is to shift a curve horizontally to the right,
/// i.e. computing $g(t) = f(t - T)$.
/// </summary>
/// <remarks>
/// Computing the expression will throw an <see cref="ArgumentException"/> 
/// if the shift argument turns out to be infinite.
/// </remarks>
public record HorizontalShiftExpression : CurveBinaryExpression<Curve, Rational>
{
    /// <summary>
    /// Constructs an expression which shifts a curve horizontally to the right.
    /// Computes $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    public HorizontalShiftExpression(
        Curve curveL,
        string nameL,
        Rational delay,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), new RationalNumberExpression(delay), expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which shifts a curve horizontally to the right.
    /// Computes $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    public HorizontalShiftExpression(
        Curve curveL,
        string nameL,
        RationalExpression rightExpression,
        string expressionName = "",
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightExpression, expressionName, settings)
    {
    }

    /// <summary>
    /// Constructs an expression which shifts a curve horizontally to the right.
    /// Computes $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/>
    /// if the shift argument turns out to be infinite.
    /// </remarks>
    public HorizontalShiftExpression(
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