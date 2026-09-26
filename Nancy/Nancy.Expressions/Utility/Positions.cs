using System.Globalization;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// Helper class to build expression paths.
/// </summary>
public static class Positions
{
    
    /// In a <see cref="IGenericBinaryExpression{T1, T2, TResult}"/>, position of its left operand.
    public const string LeftOperand = "LeftOperand";
    
    /// In a <see cref="IGenericBinaryExpression{T1, T2, TResult}"/>, position of its right operand.
    public const string RightOperand = "RightOperand";
    
    /// In a <see cref="IGenericUnaryExpression{T,TResult}"/>, position of its single operand.
    public const string InnerOperand = "Operand";

    /// In a <see cref="IGenericNAryExpression{T,TResult}"/>, position of its <paramref name="i"/>-th operand.
    public static string IndexedOperand(int i) => i.ToString(CultureInfo.InvariantCulture);
}
