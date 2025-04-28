using Unipi.Nancy.Expressions.Equivalences;
using Unipi.Nancy.Expressions.ExpressionsUtility;
using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

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
    /// Replaces every occurence of a sub-expression in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPattern">The sub-expression to look for in the main expression for being replaced</param>
    /// <param name="newExpressionToReplace">The new sub-expression</param>
    /// <returns>New expression object with replaced sub-expressions</returns>
    public IGenericExpression<TExpressionResult> ReplaceByValue<T1>(
        IGenericExpression<T1> expressionPattern,
        IGenericExpression<T1> newExpressionToReplace,
        bool ignoreNotMatchedExpressions = false
    );

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="expressionPosition">Position of the expression to be replaced</param>
    /// <param name="newExpression">The new sub-expression</param>
    /// <returns>New expression object with replaced sub-expression</returns>
    public IGenericExpression<TExpressionResult> ReplaceByPosition<T1>(
        ExpressionPosition expressionPosition,
        IGenericExpression<T1> newExpression
    );

    /// <summary>
    /// Replaces the sub-expression at a certain position in the expression to which the method is applied.
    /// </summary>
    /// <param name="positionPath">Position of the expression to be replaced. The position is expressed as a path from
    /// the root of the expression by using a list of strings "Operand" for unary operators, "LeftOperand"/"RightOperand"
    /// for binary operators, "Operand(index)" for n-ary operators</param>
    /// <param name="newExpression">The new sub-expression</param>
    /// <returns>New expression object with replaced sub-expression</returns>
    public IGenericExpression<TExpressionResult> ReplaceByPosition<T1>(
        IEnumerable<string> positionPath,
        IGenericExpression<T1> newExpression
    );

    /// <summary>
    /// Changes the name of the expression.
    /// </summary>
    /// <param name="expressionName">The new name of the expression</param>
    /// <returns>The expression (new object) with the new name</returns>
    public IGenericExpression<TExpressionResult> WithName(string expressionName);

    /// <summary>
    /// Applies an equivalence to the current expression.
    /// </summary>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// </returns>
    public IGenericExpression<TExpressionResult> ApplyEquivalence(Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);

    /// <summary>
    /// Applies an equivalence to the current expression, allowing the user to specify the position in the expression in
    /// which the equivalence should be applied.
    /// </summary>
    /// <param name="positionPath">Position of the sub-expression to be replaced with an equivalent one.
    /// The position is expressed as a path from the root of the expression by using a list of strings "Operand" for
    /// unary operators, "LeftOperand"/"RightOperand" for binary operators, "Operand(index)" for n-ary operators</param>
    /// <param name="equivalence">The equivalence to be applied to (a sub-part of) the expression.</param>
    /// <param name="checkType">Since the equivalence is described by a left-side expression and a right-side
    /// expression, this parameter identifies the direction of application of the equivalence (match of the left side,
    /// and substitution with the right side, or vice versa, or both).</param>
    /// <returns>The new equivalent expression if the equivalence can be applied, the original expression otherwise.
    /// </returns>
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
    /// </returns>
    public IGenericExpression<TExpressionResult> ApplyEquivalenceByPosition(ExpressionPosition expressionPosition,
        Equivalence equivalence,
        CheckType checkType = CheckType.CheckLeftOnly);
}