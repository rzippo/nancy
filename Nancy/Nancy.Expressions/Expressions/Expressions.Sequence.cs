using System.Runtime.CompilerServices;
using Unipi.Nancy.Expressions;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

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
    
    // todo: add method to check equivalent expressions without computing them
    
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
    /// Creates a new expression composed of the horizontal deviation operation between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
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
    /// Creates a new expression composed of the vertical deviation operation between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static RationalExpression VerticalDeviation(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalDeviationExpression(sequenceL, nameL, expressionR, expressionName, settings);
    
    #endregion VerticalDeviation

    #region Negate

    /// <summary>
    /// Adds the opposite operator to the expression passed as argument.
    /// </summary>
    public static SequenceExpression Negate(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.Negate(expressionName, settings);

    /// <summary>
    /// Adds the opposite operator to the sequence passed as argument (internally converted to <see cref="ConcreteSequenceExpression"/>).
    /// </summary>
    public static SequenceExpression Negate(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceNegateExpression(sequence, name, expressionName, settings);

    #endregion Negate

    #region ToNonNegative

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its non-negative version.
    /// </summary>
    public static SequenceExpression ToNonNegative(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.ToNonNegative(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument the operation to compute its non-negative version.
    /// </summary>
    public static SequenceExpression ToNonNegative(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToNonNegativeExpression(sequence, name, expressionName, settings);

    #endregion ToNonNegative

    #region Floor

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its floor, $\lfloor f(t) \rfloor$.
    /// </summary>
    public static SequenceExpression Floor(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.Floor(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument the operation to compute its floor, $\lfloor f(t) \rfloor$.
    /// </summary>
    public static SequenceExpression Floor(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceFloorExpression(sequence, name, expressionName, settings);

    #endregion Floor

    #region Ceil

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute its ceiling, $\lceil f(t) \rceil$.
    /// </summary>
    public static SequenceExpression Ceil(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.Ceil(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument the operation to compute its ceiling, $\lceil f(t) \rceil$.
    /// </summary>
    public static SequenceExpression Ceil(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCeilExpression(sequence, name, expressionName, settings);

    #endregion Ceil

    #region ToLeftContinuous

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute a left continuous version of it.
    /// </summary>
    public static SequenceExpression ToLeftContinuous(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.ToLeftContinuous(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument (converted to <see cref="ConcreteSequenceExpression"/>) the operation to compute a left continuous version of it.
    /// </summary>
    public static SequenceExpression ToLeftContinuous(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToLeftContinuousExpression(sequence, name, expressionName, settings);

    #endregion ToLeftContinuous

    #region ToRightContinuous

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute a right continuous version of it.
    /// </summary>
    public static SequenceExpression ToRightContinuous(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.ToRightContinuous(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument (converted to <see cref="ConcreteSequenceExpression"/>) the operation to compute a right continuous version of it.
    /// </summary>
    public static SequenceExpression ToRightContinuous(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceToRightContinuousExpression(sequence, name, expressionName, settings);

    #endregion ToRightContinuous

    #region LowerPseudoInverse

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public static SequenceExpression LowerPseudoInverse(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.LowerPseudoInverse(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument (converted to <see cref="ConcreteSequenceExpression"/>) the operation to compute the lower pseudo-inverse function, $f^{-1}_\downarrow(x) = \inf \left\{ t : f(t) \ge x \right\} = \sup \left\{ t : f(t) &lt; x \right\}$.
    /// </summary>
    public static SequenceExpression LowerPseudoInverse(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceLowerPseudoInverseExpression(sequence, name, expressionName, settings);

    #endregion LowerPseudoInverse

    #region UpperPseudoInverse

    /// <summary>
    /// Adds to the expression passed as argument the operation to compute the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public static SequenceExpression UpperPseudoInverse(SequenceExpression operand, string expressionName = "",
        ExpressionSettings? settings = null)
        => operand.UpperPseudoInverse(expressionName, settings);

    /// <summary>
    /// Adds to the sequence passed as argument (converted to <see cref="ConcreteSequenceExpression"/>) the operation to compute the upper pseudo-inverse function, $f^{-1}_\uparrow(x) = \inf\{ t : f(t) > x \} = \sup\{ t : f(t) \le x \}$.
    /// </summary>
    public static SequenceExpression UpperPseudoInverse(Sequence sequence, [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceUpperPseudoInverseExpression(sequence, name, expressionName, settings);

    #endregion UpperPseudoInverse

    #region Addition

    /// <summary>
    /// Creates a new expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Addition(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the expression and the sequence (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Addition(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Addition(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the two sequences (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Addition(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceAdditionExpression([sequenceL, sequenceR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the sequence <paramref name="sequenceL"/> (converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Addition(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Addition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the sequences passed as argument in the collection <paramref name="sequences"/>.
    /// </summary>
    public static SequenceExpression Addition(IReadOnlyCollection<Sequence> sequences, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceAdditionExpression(sequences, names, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the addition between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Addition(IReadOnlyCollection<SequenceExpression> sequenceExpressions,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceAdditionExpression(sequenceExpressions, expressionName, settings);

    #endregion Addition

    #region Subtraction

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Subtraction(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Subtraction(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Subtraction(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Subtraction(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Subtraction(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceSubtractionExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the subtraction between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Subtraction(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceSubtractionExpression(sequenceL, nameL, expressionR, expressionName, settings);

    #endregion Subtraction

    #region Minimum

    /// <summary>
    /// Creates a new expression composed of the minimum between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Minimum(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Minimum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the expression and the sequence (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Minimum(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Minimum(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the two sequences (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Minimum(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMinimumExpression([sequenceL, sequenceR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the sequence <paramref name="sequenceL"/> (converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Minimum(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Minimum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the sequences passed as argument in the collection <paramref name="sequences"/>.
    /// </summary>
    public static SequenceExpression Minimum(IReadOnlyCollection<Sequence> sequences, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMinimumExpression(sequences, names, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the minimum between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Minimum(IReadOnlyCollection<SequenceExpression> sequenceExpressions,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMinimumExpression(sequenceExpressions, expressionName, settings);

    #endregion Minimum

    #region Maximum

    /// <summary>
    /// Creates a new expression composed of the maximum between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Maximum(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Maximum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the expression and the sequence (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Maximum(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Maximum(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the two sequences (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Maximum(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaximumExpression([sequenceL, sequenceR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the sequence <paramref name="sequenceL"/> (converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Maximum(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Maximum(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the sequences passed as argument in the collection <paramref name="sequences"/>.
    /// </summary>
    public static SequenceExpression Maximum(IReadOnlyCollection<Sequence> sequences, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaximumExpression(sequences, names, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the maximum between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Maximum(IReadOnlyCollection<SequenceExpression> sequenceExpressions,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaximumExpression(sequenceExpressions, expressionName, settings);

    #endregion Maximum

    #region Convolution

    /// <summary>
    /// Creates a new expression composed of the convolution between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Convolution(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Convolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the expression and the sequence (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Convolution(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Convolution(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the two sequences (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Convolution(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceConvolutionExpression([sequenceL, sequenceR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the sequence <paramref name="sequenceL"/> (converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Convolution(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Convolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the sequences passed as argument in the collection <paramref name="sequences"/>.
    /// </summary>
    public static SequenceExpression Convolution(IReadOnlyCollection<Sequence> sequences, IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceConvolutionExpression(sequences, names, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the convolution between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Convolution(IReadOnlyCollection<SequenceExpression> sequenceExpressions,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceConvolutionExpression(sequenceExpressions, expressionName, settings);

    #endregion Convolution

    #region Deconvolution

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the two expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Deconvolution(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Deconvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Deconvolution(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Deconvolution(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Deconvolution(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceDeconvolutionExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the deconvolution between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Deconvolution(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceDeconvolutionExpression(sequenceL, nameL, expressionR, expressionName, settings);

    #endregion Deconvolution

    #region MaxPlusConvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.MaxPlusConvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the expression and the sequence (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.MaxPlusConvolution(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the two sequences (converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaxPlusConvolutionExpression([sequenceL, sequenceR], [nameL, nameR], expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the sequence <paramref name="sequenceL"/> (converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).MaxPlusConvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the sequences passed as argument in the collection <paramref name="sequences"/>.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(IReadOnlyCollection<Sequence> sequences,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaxPlusConvolutionExpression(sequences, names, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus convolution between the expressions passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusConvolution(IReadOnlyCollection<SequenceExpression> sequenceExpressions,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaxPlusConvolutionExpression(sequenceExpressions, expressionName, settings);

    #endregion MaxPlusConvolution

    #region MaxPlusDeconvolution

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the two expressions passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusDeconvolution(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.MaxPlusDeconvolution(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusDeconvolution(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.MaxPlusDeconvolution(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusDeconvolution(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaxPlusDeconvolutionExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the max-plus deconvolution between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression MaxPlusDeconvolution(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceMaxPlusDeconvolutionExpression(sequenceL, nameL, expressionR, expressionName, settings);

    #endregion MaxPlusDeconvolution

    #region Composition

    /// <summary>
    /// Creates a new expression composed of the composition between the two expressions passed as arguments.
    /// </summary>
    public static SequenceExpression Composition(SequenceExpression expressionL, SequenceExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Composition(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Composition(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Composition(sequence, name, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    public static SequenceExpression Composition(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCompositionExpression(sequenceL, nameL, sequenceR, nameR, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the composition between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    public static SequenceExpression Composition(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Composition(expressionR, expressionName, settings);

    #endregion Composition

    #region Concat

    /// <summary>
    /// Creates a new expression composed of the concatenation between the two expressions passed as arguments.
    /// </summary>
    /// <remarks>
    /// <paramref name="preserveDelay"/> keeps the delay of the right operand as a gap, and <paramref name="preserveShift"/> keeps the value it starts from as a jump at the join.
    /// </remarks>
    public static SequenceExpression Concat(SequenceExpression expressionL, SequenceExpression expressionR,
        bool preserveDelay = false, bool preserveShift = false,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Concat(expressionR, preserveDelay, preserveShift, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the concatenation between the expression and the sequence (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    /// <remarks>
    /// <paramref name="preserveDelay"/> keeps the delay of the right operand as a gap, and <paramref name="preserveShift"/> keeps the value it starts from as a jump at the join.
    /// </remarks>
    public static SequenceExpression Concat(SequenceExpression operand, Sequence sequence,
        [CallerArgumentExpression("sequence")] string name = "",
        bool preserveDelay = false, bool preserveShift = false,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Concat(sequence, name, preserveDelay, preserveShift, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the concatenation between the two sequences (internally converted to <see cref="ConcreteSequenceExpression"/>) passed as arguments.
    /// </summary>
    /// <remarks>
    /// <paramref name="preserveDelay"/> keeps the delay of the right operand as a gap, and <paramref name="preserveShift"/> keeps the value it starts from as a jump at the join.
    /// </remarks>
    public static SequenceExpression Concat(Sequence sequenceL, Sequence sequenceR,
        [CallerArgumentExpression("sequenceL")] string nameL = "", [CallerArgumentExpression("sequenceR")] string nameR = "",
        bool preserveDelay = false, bool preserveShift = false,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceConcatExpression(sequenceL, nameL, sequenceR, nameR, preserveDelay, preserveShift, expressionName, settings);

    /// <summary>
    /// Creates a new expression composed of the concatenation between the sequence <paramref name="sequenceL"/> (internally converted to <see cref="ConcreteSequenceExpression"/>) and the expression <paramref name="expressionR"/> passed as arguments.
    /// </summary>
    /// <remarks>
    /// <paramref name="preserveDelay"/> keeps the delay of the right operand as a gap, and <paramref name="preserveShift"/> keeps the value it starts from as a jump at the join.
    /// </remarks>
    public static SequenceExpression Concat(Sequence sequenceL, SequenceExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        bool preserveDelay = false, bool preserveShift = false,
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceConcatExpression(sequenceL, nameL, expressionR, preserveDelay, preserveShift, expressionName, settings);

    #endregion Concat

    #region Delay

    /// <summary>
    /// Creates a new expression that delays the sequence expression <paramref name="expressionL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Delay"/>
    public static SequenceExpression Delay(SequenceExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Delay(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the sequence expression <paramref name="operand"/> by the rational <paramref name="delay"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Delay"/>
    public static SequenceExpression Delay(SequenceExpression operand, Rational delay,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Delay(delay, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the sequence <paramref name="sequenceL"/> by the rational <paramref name="delay"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Delay"/>
    public static SequenceExpression Delay(Sequence sequenceL, Rational delay,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceDelayExpression(sequenceL, nameL, delay, expressionName, settings);

    /// <summary>
    /// Creates a new expression that delays the sequence <paramref name="sequenceL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t - T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the delay argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Delay"/>
    public static SequenceExpression Delay(Sequence sequenceL, RationalExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Delay(expressionR, expressionName, settings);

    #endregion Delay

    #region Forward

    /// <summary>
    /// Creates a new expression that forwards the sequence expression <paramref name="expressionL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Forward"/>
    public static SequenceExpression Forward(SequenceExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Forward(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the sequence expression <paramref name="operand"/> by the rational <paramref name="time"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Forward"/>
    public static SequenceExpression Forward(SequenceExpression operand, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Forward(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the sequence <paramref name="sequenceL"/> by the rational <paramref name="time"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Forward"/>
    public static SequenceExpression Forward(Sequence sequenceL, Rational time,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceForwardExpression(sequenceL, nameL, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that forwards the sequence <paramref name="sequenceL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t + T)$, with $T \ge 0$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the time argument turns out to be either negative or infinite.
    /// </remarks>
    /// <seealso cref="Sequence.Forward"/>
    public static SequenceExpression Forward(Sequence sequenceL, RationalExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Forward(expressionR, expressionName, settings);

    #endregion Forward

    #region HorizontalShift

    /// <summary>
    /// Creates a new expression that shifts the sequence expression <paramref name="expressionL"/> horizontally to the right by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public static SequenceExpression HorizontalShift(SequenceExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.HorizontalShift(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence expression <paramref name="operand"/> horizontally to the right by the rational <paramref name="time"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public static SequenceExpression HorizontalShift(SequenceExpression operand, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.HorizontalShift(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence <paramref name="sequenceL"/> horizontally to the right by the rational <paramref name="time"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public static SequenceExpression HorizontalShift(Sequence sequenceL, Rational time,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceHorizontalShiftExpression(sequenceL, nameL, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence <paramref name="sequenceL"/> horizontally to the right by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t - T)$.
    /// </summary>
    /// <remarks>
    /// Computing the expression will throw an <see cref="ArgumentException"/> if the shift argument turns out to be infinite.
    /// </remarks>
    /// <seealso cref="Sequence.HorizontalShift"/>
    public static SequenceExpression HorizontalShift(Sequence sequenceL, RationalExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).HorizontalShift(expressionR, expressionName, settings);

    #endregion HorizontalShift

    #region VerticalShift

    /// <summary>
    /// Creates a new expression that shifts the sequence expression <paramref name="expressionL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t) + K$.
    /// </summary>
    public static SequenceExpression VerticalShift(SequenceExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.VerticalShift(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence expression <paramref name="operand"/> by the rational <paramref name="shift"/>, i.e., computing $f(t) + K$.
    /// </summary>
    public static SequenceExpression VerticalShift(SequenceExpression operand, Rational shift,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.VerticalShift(shift, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence <paramref name="sequenceL"/> by the rational <paramref name="shift"/>, i.e., computing $f(t) + K$.
    /// </summary>
    public static SequenceExpression VerticalShift(Sequence sequenceL, Rational shift,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceVerticalShiftExpression(sequenceL, nameL, shift, expressionName, settings);

    /// <summary>
    /// Creates a new expression that shifts the sequence <paramref name="sequenceL"/> by the rational expression <paramref name="expressionR"/>, i.e., computing $f(t) + K$.
    /// </summary>
    public static SequenceExpression VerticalShift(Sequence sequenceL, RationalExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).VerticalShift(expressionR, expressionName, settings);

    #endregion VerticalShift

    #region Scale

    /// <summary>
    /// Creates a new expression that scales the sequence expression <paramref name="expressionL"/> by the rational expression <paramref name="expressionR"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression Scale(SequenceExpression expressionL, RationalExpression expressionR,
        string expressionName = "", ExpressionSettings? settings = null)
        => expressionL.Scale(expressionR, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the sequence expression <paramref name="operand"/> by the rational <paramref name="scaleFactor"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression Scale(SequenceExpression operand, Rational scaleFactor,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.Scale(scaleFactor, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the sequence <paramref name="sequenceL"/> by the rational <paramref name="scaleFactor"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression Scale(Sequence sequenceL, Rational scaleFactor,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceScaleExpression(sequenceL, nameL, scaleFactor, expressionName, settings);

    /// <summary>
    /// Creates a new expression that scales the sequence <paramref name="sequenceL"/> by the rational expression <paramref name="expressionR"/>, i.e. $k \cdot f(t)$.
    /// </summary>
    public static SequenceExpression Scale(Sequence sequenceL, RationalExpression expressionR,
        [CallerArgumentExpression("sequenceL")] string nameL = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequenceL, nameL).Scale(expressionR, expressionName, settings);

    #endregion Scale

    #region Cut

    /// <summary>
    /// Creates an expression that cuts this curve expression over the given interval, crossing from a curve expression to a sequence one.
    /// </summary>
    public static SequenceExpression Cut(CurveExpression expression, Interval interval,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Cut(interval, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts the curve passed as argument over the given interval, crossing from a curve expression to a sequence one.
    /// </summary>
    public static SequenceExpression Cut(Curve curve, Interval interval,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new CurveCutExpression(curve, name, interval, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts this sequence expression over the given interval.
    /// </summary>
    public static SequenceExpression Cut(SequenceExpression expression, Interval interval,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.Cut(interval, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts the sequence passed as argument over the given interval.
    /// </summary>
    public static SequenceExpression Cut(Sequence sequence, Interval interval,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCutExpression(sequence, name, interval, expressionName, settings);

    #endregion Cut

    #region CutToNeighbourhood

    /// <summary>
    /// Creates an expression that cuts this curve expression over a neighbourhood of $[a, b]$, crossing from a curve expression to a sequence one.
    /// </summary>
    public static SequenceExpression CutToNeighbourhood(CurveExpression expression, Rational cutStart, Rational cutEnd,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.CutToNeighbourhood(cutStart, cutEnd, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts the curve passed as argument over a neighbourhood of $[a, b]$, crossing from a curve expression to a sequence one.
    /// </summary>
    public static SequenceExpression CutToNeighbourhood(Curve curve, Rational cutStart, Rational cutEnd,
        [CallerArgumentExpression("curve")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new CurveCutToNeighbourhoodExpression(curve, name, cutStart, cutEnd, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts this sequence expression over a neighbourhood of $[a, b]$.
    /// </summary>
    public static SequenceExpression CutToNeighbourhood(SequenceExpression expression, Rational cutStart, Rational cutEnd,
        string expressionName = "", ExpressionSettings? settings = null)
        => expression.CutToNeighbourhood(cutStart, cutEnd, expressionName, settings);

    /// <summary>
    /// Creates an expression that cuts the sequence passed as argument over a neighbourhood of $[a, b]$.
    /// </summary>
    public static SequenceExpression CutToNeighbourhood(Sequence sequence, Rational cutStart, Rational cutEnd,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceCutToNeighbourhoodExpression(sequence, name, cutStart, cutEnd, expressionName, settings);

    #endregion CutToNeighbourhood

    #region ValueAt

    /// <summary>
    /// Creates a new expression that computes the value of the sequence expression passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression ValueAt(SequenceExpression operand, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.ValueAt(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the value of the sequence expression passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression ValueAt(SequenceExpression operand, RationalExpression timeExpression,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.ValueAt(timeExpression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the value of the sequence passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression ValueAt(Sequence sequence, Rational time,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceValueAtExpression(sequence, name, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the value of the sequence passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression ValueAt(Sequence sequence, RationalExpression timeExpression,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequence, name).ValueAt(timeExpression, expressionName, settings);

    #endregion ValueAt

    #region LeftLimitAt

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence expression passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression LeftLimitAt(SequenceExpression operand, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.LeftLimitAt(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence expression passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression LeftLimitAt(SequenceExpression operand, RationalExpression timeExpression,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.LeftLimitAt(timeExpression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression LeftLimitAt(Sequence sequence, Rational time,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceLeftLimitAtExpression(sequence, name, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the left-limit value of the sequence passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression LeftLimitAt(Sequence sequence, RationalExpression timeExpression,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequence, name).LeftLimitAt(timeExpression, expressionName, settings);

    #endregion LeftLimitAt

    #region RightLimitAt

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence expression passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression RightLimitAt(SequenceExpression operand, Rational time,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.RightLimitAt(time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence expression passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression RightLimitAt(SequenceExpression operand, RationalExpression timeExpression,
        string expressionName = "", ExpressionSettings? settings = null)
        => operand.RightLimitAt(timeExpression, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence passed as argument at <paramref name="time"/>.
    /// </summary>
    public static RationalExpression RightLimitAt(Sequence sequence, Rational time,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => new SequenceRightLimitAtExpression(sequence, name, time, expressionName, settings);

    /// <summary>
    /// Creates a new expression that computes the right-limit value of the sequence passed as argument at the time given by <paramref name="timeExpression"/>.
    /// </summary>
    public static RationalExpression RightLimitAt(Sequence sequence, RationalExpression timeExpression,
        [CallerArgumentExpression("sequence")] string name = "",
        string expressionName = "", ExpressionSettings? settings = null)
        => FromSequence(sequence, name).RightLimitAt(timeExpression, expressionName, settings);

    #endregion RightLimitAt
}
