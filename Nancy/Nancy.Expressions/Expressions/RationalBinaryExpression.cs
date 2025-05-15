using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes binary (nor commutative or associative) expressions whose value is a <see cref="Unipi.Nancy.Numerics.Rational"/> object.
/// </summary>
/// <typeparam name="TLeftOperand">The type of the value of the left operand.</typeparam>
/// <typeparam name="TRightOperand">The type of the value of the right operand.</typeparam>
public abstract record RationalBinaryExpression<TLeftOperand, TRightOperand> : RationalExpression, IGenericBinaryExpression<TLeftOperand, TRightOperand, Rational>
{
    /// <summary>
    /// Class which describes binary (nor commutative or associative) expressions whose value is a <see cref="Unipi.Nancy.Numerics.Rational"/>
    /// object.
    /// </summary>
    protected RationalBinaryExpression(
        IGenericExpression<TLeftOperand> leftExpression,
        IGenericExpression<TRightOperand> rightExpression,
        string ExpressionName = "", 
        ExpressionSettings? Settings = null) 
        : base(ExpressionName, Settings)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    /// <inheritdoc />
    public IGenericExpression<TLeftOperand> LeftExpression { get; init; }

    /// <inheritdoc />
    public IGenericExpression<TRightOperand> RightExpression { get; init; }
}