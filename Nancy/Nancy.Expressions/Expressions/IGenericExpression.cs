using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Interface which defines the rules each Nancy expression must follow.
/// </summary>
/// <typeparam name="TExpressionResult">
/// The type the expression evaluates to, either <see cref="Curve"/> or <see cref="Rational"/>.
/// </typeparam>
public interface IGenericExpression<out TExpressionResult> : IExpression
{
    /// <summary>
    /// The value of the expression
    /// </summary>
    public TExpressionResult Value { get; }

    /// <summary>
    /// Settings for the expression (contains also the settings for the evaluation of the expression).
    /// </summary>
    public ExpressionSettings? Settings { get; }

    /// <summary>
    /// Computes the value the expression evaluates to.
    /// </summary>
    public TExpressionResult Compute();
    
    /// <summary>
    /// Method used for implementing the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    public void Accept(IExpressionVisitor<TExpressionResult> visitor);
    
    /// <summary>
    /// Method used for implementing the Visitor design pattern: the visited object must "accept" the visitor object.
    /// </summary>
    /// <param name="visitor">The Visitor object</param>
    public TResult Accept<TResult>(IExpressionVisitor<TExpressionResult, TResult> visitor);


    /// <summary>
    /// Replaces every occurrence of a sub-expression in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced</param>
    /// <param name="newExpressionToReplace">The new sub-expression</param>
    /// <param name="ignoreNotMatchedExpressions">Whether unmatched expressions should be ignored.</param>
    /// <returns>New expression object with replaced sub-expressions.
    /// When the pattern matches nothing, this is the original expression, unchanged.</returns>
    public IGenericExpression<TExpressionResult> ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    );

    /// <summary>
    /// Replaces every occurrence of a sub-expression in the expression to which the method is applied, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced</param>
    /// <param name="newExpressionToReplace">The new sub-expression</param>
    /// <param name="ignoreNotMatchedExpressions">Whether unmatched expressions should be ignored.</param>
    /// <returns>The result, carrying the new expression, how many sites were replaced, and where.
    /// When the pattern matches nothing, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.</returns>
    public ExpressionRewriteResult ReplaceByValueWithResult<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    );

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="newExpression">The new sub-expression</param>
    /// <returns>New expression object with replaced sub-expression.
    /// A valid position is always replaced; a position that does not fit the expression is rejected.</returns>
    public IGenericExpression<TExpressionResult> ReplaceByPosition<T1>(
        ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpression
    );

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="newExpression">The new sub-expression</param>
    /// <returns>The result, carrying the new expression and the one position that was replaced.
    /// A valid position is always replaced; a position that does not fit the expression is rejected.</returns>
    public ExpressionRewriteResult ReplaceByPositionWithResult<T1>(
        ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpression
    );

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="positionPath">Position of the expression to be replaced. The position is expressed as a path from
    /// the root of the expression by using a list of strings "Operand" for unary operators, "LeftOperand"/"RightOperand"
    /// for binary operators, the operand's index, from 0, for n-ary operators</param>
    /// <param name="newExpression">The new sub-expression</param>
    /// <returns>New expression object with replaced sub-expression.
    /// A valid position is always replaced; a position that does not fit the expression is rejected.</returns>
    [Obsolete("Use the overload taking an ExpressionPosition instead.")]
    public IGenericExpression<TExpressionResult> ReplaceByPosition<T1>(
        IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpression
    );

    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>The expression (new object) with the new name</returns>
    /// <remarks>
    /// Returns a new instance; does not mutate the receiver.
    /// </remarks>
    public IGenericExpression<TExpressionResult> WithName(string expressionName);

    /// <summary>
    /// Changes the <see cref="IExpression.Generation"/> of the expression.
    /// </summary>
    /// <param name="generation">The new generation of the expression</param>
    /// <returns>The expression (new object) with the new generation</returns>
    /// <remarks>
    /// Returns a new instance; does not mutate the receiver.
    /// </remarks>
    public IGenericExpression<TExpressionResult> WithGeneration(int generation);

    /// <summary>
    /// Applies an equivalence to the current expression.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// When the equivalence matches nothing, this is the original expression, unchanged.</returns>
    public IGenericExpression<TExpressionResult> ApplyEquivalence(Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);

    /// <summary>
    /// Applies an equivalence to the current expression, at every site where it matches, and returns what the rewrite did.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The result, carrying the new expression, how many sites were rewritten, where, and the bindings.
    /// When the equivalence matches nothing, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.</returns>
    public ExpressionRewriteResult ApplyEquivalenceWithResult(Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in
    /// which the equivalence should be applied.
    /// </summary>
    /// <param name="positionPath">Position of the sub-expression to be replaced with an equivalent one.
    /// The position is expressed as a path from the root of the expression by using a list of strings "Operand" for
    /// unary operators, "LeftOperand"/"RightOperand" for binary operators, the operand's index, from 0, for n-ary operators</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// When the equivalence matches nothing at the position, this is the original expression, unchanged.</returns>
    [Obsolete("Use the overload taking an ExpressionPosition instead.")]
    public IGenericExpression<TExpressionResult> ApplyEquivalenceByPosition(IEnumerable<string> positionPath, Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in
    /// which the equivalence should be applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// When the equivalence matches nothing at the position, this is the original expression, unchanged.</returns>
    public IGenericExpression<TExpressionResult> ApplyEquivalenceByPosition(ExpressionPosition expressionPosition,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);

    /// <summary>
    /// Applies an equivalence to the current expression at a certain position, and returns what the rewrite did.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side expression, this parameter identifies the direction of application of the equivalence (match of the left side, and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The result, carrying the new expression, the one position if it was rewritten, and the bindings.
    /// When the equivalence matches nothing at the position, the expression is the original, unchanged, and <see cref="ExpressionRewriteResult.Matched"/> is <see langword="false"/>.</returns>
    public ExpressionRewriteResult ApplyEquivalenceByPositionWithResult(ExpressionPosition expressionPosition,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);
}