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
    protected CurveBinaryExpression(
        IGenericExpression<T1> leftExpression,
        IGenericExpression<T2> rightExpression,
        string ExpressionName = "", 
        ExpressionSettings? Settings = null) : base(ExpressionName, Settings)
    {
        LeftExpression = leftExpression;
        RightExpression = rightExpression;
    }

    public IGenericExpression<T1> LeftExpression { get; init; }
    public IGenericExpression<T2> RightExpression { get; init; }
}