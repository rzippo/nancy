using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression which computes the addition (n-ary operation).
/// </summary>
public record SequenceAdditionExpression : SequenceNAryExpression
{
    /// <summary>
    /// Creates an addition expression.
    /// </summary>
    /// <param name="expressions">The operands (expressions) of the addition</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public SequenceAdditionExpression(IReadOnlyCollection<IGenericExpression<Sequence>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates an addition expression from a collection of sequences.
    /// </summary>
    /// <param name="sequences">Collection of sequences representing the operands of the addition</param>
    /// <param name="names">Collection of the names of the sequences operands</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public SequenceAdditionExpression(IReadOnlyCollection<Sequence> sequences,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null) : base(
        sequences, names, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
