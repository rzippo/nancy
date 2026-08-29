using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class which describes binary (nor commutative or associative) expressions whose value is a <see cref="Curve"/>
/// object.
/// </summary>
/// <typeparam name="T1">The type of the value of the left operand</typeparam>
/// <typeparam name="T2">The type of the value of the right operand</typeparam>
public abstract record CurveBinaryExpression<T1, T2> : CurveExpression, IGenericBinaryExpression<T1, T2, Curve>
{
    /// <inheritdoc/>
    protected CurveBinaryExpression(
        IGenericExpression<T1> leftOperand,
        IGenericExpression<T2> rightOperand,
        string ExpressionName = "",
        ExpressionSettings? Settings = null) : base(ExpressionName, Settings)
    {
        LeftOperand = leftOperand;
        RightOperand = rightOperand;
    }

    /// <inheritdoc />
    public IGenericExpression<T1> LeftOperand { get; init; }

    /// <inheritdoc />
    public IGenericExpression<T2> RightOperand { get; init; }

    /// <inheritdoc cref="IGenericBinaryExpression{T1,T2,TResult}.LeftExpression"/>
    [Obsolete("Renamed to LeftOperand.")]
    public IGenericExpression<T1> LeftExpression => LeftOperand;

    /// <inheritdoc cref="IGenericBinaryExpression{T1,T2,TResult}.RightExpression"/>
    [Obsolete("Renamed to RightOperand.")]
    public IGenericExpression<T2> RightExpression => RightOperand;
}