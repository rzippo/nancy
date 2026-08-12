namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Exception thrown when an expression cannot be rendered as MPPG source text.
/// </summary>
/// <remarks>
/// Nancy expressions can express operations for which the MPPG syntax has no notation.
/// Rather than emitting a string that does not parse, or one that parses into a different expression, <see cref="MppgFormatterVisitor"/> reports the offending sub-expression through this exception.
/// </remarks>
public class MppgFormattingException : NotSupportedException
{
    /// <summary>
    /// The type of the sub-expression that could not be rendered.
    /// </summary>
    public Type ExpressionType { get; }

    /// <summary>
    /// The name of the sub-expression that could not be rendered, which may be empty.
    /// </summary>
    public string ExpressionName { get; }

    /// <summary>
    /// The reason why the sub-expression could not be rendered.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Exception thrown when an expression cannot be rendered as MPPG source text.
    /// </summary>
    /// <param name="expressionType">The type of the sub-expression that could not be rendered.</param>
    /// <param name="expressionName">The name of the sub-expression that could not be rendered, which may be empty.</param>
    /// <param name="reason">The reason why the sub-expression could not be rendered.</param>
    public MppgFormattingException(Type expressionType, string expressionName, string reason)
        : base(BuildMessage(expressionType, expressionName, reason))
    {
        ExpressionType = expressionType;
        ExpressionName = expressionName;
        Reason = reason;
    }

    private static string BuildMessage(Type expressionType, string expressionName, string reason)
    {
        var subject = expressionName.Equals("")
            ? expressionType.Name
            : $"{expressionType.Name} \"{expressionName}\"";
        return $"Cannot render {subject} as MPPG: {reason}";
    }
}
