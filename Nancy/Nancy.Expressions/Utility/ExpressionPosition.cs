using System.Globalization;
using Unipi.Nancy.Expressions.Nodes;

namespace Unipi.Nancy.Expressions.Utility;

/// <summary>
/// Class which models the position of a sub-expression inside a DNC expression. The position is obtained by
/// specifying, using the different methods, the path from the root of the expression tree to the node representing
/// the sub-expression.
/// </summary>
public class ExpressionPosition : IEquatable<ExpressionPosition>
{
    /// <summary>
    /// The sequence of steps from the root expression to the sub-expression.
    /// </summary>
    private readonly IReadOnlyList<PathStep> _steps;

    /// <summary>
    /// Creates an empty position, naming the root of the expression.
    /// </summary>
    public ExpressionPosition()
    {
        _steps = [];
    }

    /// <summary>
    /// Creates the object representing the position of a sub-expression inside a DNC expression.
    /// </summary>
    /// <param name="steps">The steps from the root expression to the sub-expression.</param>
    public ExpressionPosition(IEnumerable<PathStep> steps)
    {
        _steps = steps.ToArray();
    }

    /// <summary>
    /// Creates the object representing the position of a sub-expression inside a DNC expression.
    /// </summary>
    /// <param name="positionPath">The steps from the root expression to the sub-expression, spelled as: "Operand",
    /// "LeftOperand", "RightOperand", or a non-negative decimal index of an n-ary operand (starting from 0).</param>
    /// <exception cref="ArgumentException">Invalid direction string.</exception>
    public ExpressionPosition(IEnumerable<string> positionPath)
    {
        _steps = positionPath.Select(PathStep.Parse).ToArray();
    }

    /// <summary>
    /// The steps from the root expression to the sub-expression.
    /// </summary>
    public IReadOnlyList<PathStep> Steps => _steps;

    /// <summary>
    /// Adds to the path the step through the operand of a unary expression node
    /// </summary>
    public ExpressionPosition InnerOperand() // For unary expressions
        => new(_steps.Append(PathStep.InnerOperand));

    /// <summary>
    /// Adds to the path the step through the operand number <paramref name="index"/> (starting from 0) of an n-ary
    /// expression node
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative.</exception>
    public ExpressionPosition IndexedOperand(int index) // For n-ary expressions
        => new(_steps.Append(PathStep.IndexedOperand(index)));

    /// <summary>
    /// Adds to the path the step through the left operand of a binary expression node
    /// </summary>
    public ExpressionPosition LeftOperand() // For binary expressions
        => new(_steps.Append(PathStep.LeftOperand));

    /// <summary>
    /// Adds to the path the step through the right operand of a binary expression node
    /// </summary>
    public ExpressionPosition RightOperand() // For binary expressions
        => new(_steps.Append(PathStep.RightOperand));

    /// <summary>
    /// Checks the validity of a list of strings representing a path inside an expression
    /// </summary>
    public static bool ValidateExpressionPosition(IEnumerable<string> positionPath)
    {
        return positionPath.All(step => PathStep.TryParse(step, out _));
    }

    /// <summary>
    /// Return the path inside an expression as a list of direction strings
    /// </summary>
    public IEnumerable<string> GetPositionPath() => _steps.Select(step => step.ToString());

    /// <summary>
    /// Parses a position rendered by <see cref="ToString"/>, with steps separated by "/".
    /// </summary>
    /// <param name="position">The rendered position, where an empty string names the root.</param>
    /// <exception cref="ArgumentException">If a step does not name a valid step.</exception>
    public static ExpressionPosition Parse(string position)
        => string.IsNullOrEmpty(position)
            ? new ExpressionPosition()
            : new ExpressionPosition(position.Split('/').Select(PathStep.Parse));

    /// <inheritdoc />
    public override string ToString() => string.Join("/", _steps);

    /// <inheritdoc />
    public bool Equals(ExpressionPosition? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (_steps.Count != other._steps.Count)
            return false;

        for (var i = 0; i < _steps.Count; i++)
        {
            if (!_steps[i].Equals(other._steps[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ExpressionPosition);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var step in _steps)
            hash.Add(step);
        return hash.ToHashCode();
    }
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
    public static string IndexedOperand(int i) => i.ToString(CultureInfo.InvariantCulture);
}
