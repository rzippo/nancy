using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Nodes;

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
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="time">The amount to forward by.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
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
    /// <param name="curveL">The left operand curve.</param>
    /// <param name="nameL">The name to assign to the left operand.</param>
    /// <param name="rightOperand">The amount to forward by.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public ForwardByExpression(
        Curve curveL, 
        string nameL, 
        RationalExpression rightOperand,
        string expressionName = "", 
        ExpressionSettings? settings = null) 
        : this(new ConcreteCurveExpression(curveL, nameL), rightOperand, expressionName, settings)
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
    /// <param name="leftOperand">The left operand expression.</param>
    /// <param name="rightOperand">The amount to forward by.</param>
    /// <param name="expressionName">The name of the expression.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public ForwardByExpression(
        CurveExpression leftOperand,
        RationalExpression rightOperand,
        string expressionName = "", 
        ExpressionSettings? settings = null)
        : base(leftOperand, rightOperand, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ICurveExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}