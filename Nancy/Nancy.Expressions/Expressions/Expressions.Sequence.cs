using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

public static partial class Expressions
{
    /// <summary>
    /// Creates a <see cref="ConcreteSequenceExpression"/> object from a <see cref="Sequence"/> object.
    /// </summary>
    public static ConcreteSequenceExpression FromSequence(Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "") =>
        new(sequence, name);
    
    /// <summary>
    /// Creates a placeholder standing for any sequence expression.
    /// </summary>
    /// <param name="name">The name of the placeholder, which is its whole identity.</param>
    /// <param name="settings">Settings for the expression definition and evaluation.</param>
    public static SequencePlaceholderExpression SequencePlaceholder(string name, ExpressionSettings? settings = null)
        => new(name, settings);

    
    /// <summary>
    /// Verifies if two sequence expressions (<paramref name="e1"/> and <paramref name="e2"/>) are equivalent, i.e., their values are equivalent.
    /// </summary>
    public static bool Equivalent(SequenceExpression e1, SequenceExpression e2)
        => e1.Equivalent(e2);
    
    //todo: add method to check equivalent expressions without computing them
    
    #region HorizontalDeviation

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the two expressions passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalDeviationExpression(expressionL, expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(SequenceExpression expression, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalDeviationExpression(expression, new ConcreteSequenceExpression(sequence, name), expressionName,
            settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalDeviationExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the horizontal deviation operation between the sequence <paramref name="sequenceL"/>
    /// (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/>
    /// passed as arguments.
    /// </summary>
    public static RationalExpression HorizontalDeviation(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalDeviationExpression(sequenceL, nameL, expressionR, expressionName, settings);
    
    #endregion HorizontalDeviation
    
    #region VerticalDeviation

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the two expressions passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalDeviationExpression(expressionL, expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(SequenceExpression expression, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalDeviationExpression(expression, new ConcreteSequenceExpression(sequence, name), expressionName,
            settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalDeviationExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the vertical deviation operation between the sequence <paramref name="sequenceL"/>
    /// (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/>
    /// passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalDeviationExpression(sequenceL, nameL, expressionR, expressionName, settings);
    
    #endregion VerticalDeviation
}
