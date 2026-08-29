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
        IGenericExpression<TLeftOperand> leftOperand,
        IGenericExpression<TRightOperand> rightOperand,
        string ExpressionName = "", 
        ExpressionSettings? Settings = null) 
        : base(ExpressionName, Settings)
    {
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
    }

    /// <inheritdoc />
    public IGenericExpression<TLeftOperand> LeftOperand { get; init; }

    /// <inheritdoc />
    public IGenericExpression<TRightOperand> RightOperand { get; init; }

    /// <inheritdoc cref="IGenericBinaryExpression{T1,T2,TResult}.LeftExpression"/>
    [Obsolete("Renamed to LeftOperand.")]
    public IGenericExpression<TLeftOperand> LeftExpression => LeftOperand;

    /// <inheritdoc cref="IGenericBinaryExpression{T1,T2,TResult}.RightExpression"/>
    [Obsolete("Renamed to RightOperand.")]
    public IGenericExpression<TRightOperand> RightExpression => RightOperand;
}