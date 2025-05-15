using Unipi.Nancy.Expressions.Internals;

namespace Unipi.Nancy.Expressions.ExpressionsUtility;

/// <summary>
/// Class which models the position of a sub-expression inside a DNC expression. The position is obtained by
/// specifying, using the different methods, the path from the root of the expression tree to the node representing
/// the sub-expression.
/// </summary>
public class ExpressionPosition
{
    /// <summary>
    /// List of strings representing the directions from the root expression to a sub-expression
    /// </summary>
    private readonly IEnumerable<string> _positionPath = [];

    /// <summary>
    /// Creates the object representing the position of a sub-expression inside a DNC expression
    /// </summary>
    /// <param name="positionPath">List of directions from the root expression to the sub-expression. The admitted
    /// values for the list are: "Operand", "LeftOperand", "RightOperand", a string representing a number (to be used to
    /// index the operand of an n-ary expression, starting from 0).</param>
    /// <exception cref="ArgumentException">Invalid direction string</exception>
    public ExpressionPosition(IEnumerable<string> positionPath) : this()
    {
        var enumerable = positionPath.ToList();
        if (ValidateExpressionPosition(enumerable))
            _positionPath = enumerable;
        else
            throw new ArgumentException("Invalid direction string!", nameof(positionPath));
    }

    /// <summary>
    /// Class which models the position of a sub-expression inside a DNC expression. The position is obtained by
    /// specifying, using the different methods, the path from the root of the expression tree to the node representing
    /// the sub-expression.
    /// </summary>
    public ExpressionPosition()
    {
    }

    /// <summary>
    /// Adds to the path the step through the operand of a unary expression node
    /// </summary>
    public ExpressionPosition InnerOperand() // For unary expressions
        => new(_positionPath.Append("Operand"));

    /// <summary>
    /// Adds to the path the step through the operand number <paramref name="index"/> (starting from 0) of an n-ary
    /// expression node
    /// </summary>
    public ExpressionPosition IndexedOperand(int index) // For n-ary expressions
        => new(_positionPath.Append(index.ToString()));

    /// <summary>
    /// Adds to the path the step through the left operand of a binary expression node
    /// </summary>
    public ExpressionPosition LeftOperand() // For binary expressions
        => new(_positionPath.Append("LeftOperand"));

    /// <summary>
    /// Adds to the path the step through the right operand of a binary expression node
    /// </summary>
    public ExpressionPosition RightOperand() // For binary expressions
        => new(_positionPath.Append("RightOperand"));

    /// <summary>
    /// Checks if the argument is a string representing a number
    /// </summary>
    private static bool IsNumber(string input)
    {
        return int.TryParse(input, out _);
    }

    /// <summary>
    /// Checks if the argument is a valid direction string of a "position path" inside an expression
    /// </summary>
    private static bool IsValidPosition(string input)
    {
        return input == Positions.InnerOperand || input == Positions.LeftOperand || input == Positions.RightOperand || IsNumber(input);
    }

    /// <summary>
    /// Checks the validity of a list of strings representing a path inside an expression
    /// </summary>
    public static bool ValidateExpressionPosition(IEnumerable<string> positionPath)
    {
        return positionPath.All(IsValidPosition);
    }

    /// <summary>
    /// Return the path inside an expression as a list of direction strings
    /// </summary>
    public IEnumerable<string> GetPositionPath() => _positionPath;
}

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
    public static string IndexedOperand(int i) => $"{i}";
}