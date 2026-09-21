using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Nodes;

/// <summary>
/// Class representing an expression whose root operation is the convolution (n-ary operation).
/// </summary>
public record SequenceConvolutionExpression : SequenceNAryExpression
{
    /// <summary>
    /// Creates a convolution expression.
    /// </summary>
    /// <param name="expressions">The operands (expressions) of the convolution</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public SequenceConvolutionExpression(IReadOnlyCollection<IGenericExpression<Sequence>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a convolution expression from a collection of sequences.
    /// </summary>
    /// <param name="sequences">Collection of sequences representing the operands of the convolution</param>
    /// <param name="names">Collection of the names of the sequences operands</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public SequenceConvolutionExpression(IReadOnlyCollection<Sequence> sequences,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null) : base(sequences, names, expressionName, settings)
    {
    }

    /// <inheritdoc />
    public override void Accept(ISequenceExpressionVisitor visitor)
        => visitor.Visit(this);

    /// <inheritdoc />
    public override TResult Accept<TResult>(ISequenceExpressionVisitor<TResult> visitor)
        => visitor.Visit(this);
}
