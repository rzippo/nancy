using System.Text;
using Unipi.Nancy.Expressions.Utility;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// The class allows to define equivalences involving NetCal expressions
/// </summary>
public partial class Equivalence
{
    /// <summary>
    /// Left side of the equivalence.
    /// </summary>
    public IExpression LeftSideExpression { get; init; }

    /// <summary>
    /// Right side of the equivalence.
    /// </summary>
    public IExpression RightSideExpression { get; init; }

    /// <summary>
    /// The conditions the equivalence places on what its placeholders bind to.
    /// </summary>
    /// <remarks>
    /// The value type a condition speaks about is tested inside the condition, not here.
    /// </remarks>
    internal readonly List<Hypothesis> Hypotheses = [];

    /// <summary>
    /// Equivalence constructor
    /// </summary>
    /// <param name="leftSideExpression">The left side of the equivalence</param>
    /// <param name="rightSideExpression">The right side of the equivalence</param>
    /// <exception cref="InvalidOperationException">Exception raised if left or right side don't contain placeholders</exception>
    /// <exception cref="ArgumentException">If the two sides are written over different value types.</exception>
    /// <exception cref="ArgumentException">If a placeholder name is used with two different value types across the two sides.</exception>
    public Equivalence(
        IExpression leftSideExpression,
        IExpression rightSideExpression)
    {
        if (!_endWithPlaceholder(leftSideExpression) || !_endWithPlaceholder(rightSideExpression))
            throw new InvalidOperationException(
                "Can't instantiate Equivalence: both sides must end with a placeholder in at least one branch!");

        var leftValueType = ExpressionValueType.Of(leftSideExpression);
        var rightValueType = ExpressionValueType.Of(rightSideExpression);
        if (leftValueType != rightValueType)
            throw new ArgumentException(
                $"The two sides of the equivalence have different value types: {leftValueType.Name} and {rightValueType.Name}.");

        _throwIfAPlaceholderNameHasTwoValueTypes(leftSideExpression, rightSideExpression);

        LeftSideExpression = leftSideExpression;
        RightSideExpression = rightSideExpression;
    }

    /// <summary>
    /// Add an hypothesis on a single operand of value type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// The condition is false where <paramref name="placeholder"/> is bound to an expression of another value type.
    /// <typeparamref name="T"/> is given explicitly, since it cannot be inferred from a lambda whose parameter type is left out.
    /// </remarks>
    public void AddHypothesis<T>(string placeholder, Predicate<T> h)
        where T : class, IExpression
        => Hypotheses.Add(new Hypothesis(
            [placeholder],
            bound => bound[0] is T operand && h(operand)));

    /// <summary>
    /// Add an hypothesis on two operands of value type <typeparamref name="T"/>.
    /// </summary>
    public void AddHypothesis<T>(string placeholder1, string placeholder2, Func<T, T, bool> h)
        where T : class, IExpression
        => Hypotheses.Add(new Hypothesis(
            [placeholder1, placeholder2],
            bound => bound[0] is T first && bound[1] is T second && h(first, second)));

    /// <summary>
    /// Add an hypothesis on three operands of value type <typeparamref name="T"/>.
    /// </summary>
    public void AddHypothesis<T>(string placeholder1, string placeholder2, string placeholder3,
        Func<T, T, T, bool> h)
        where T : class, IExpression
        => Hypotheses.Add(new Hypothesis(
            [placeholder1, placeholder2, placeholder3],
            bound => bound[0] is T first && bound[1] is T second && bound[2] is T third
                     && h(first, second, third)));

    /// <summary>
    /// Check and apply the equivalence.
    /// </summary>
    /// <param name="expression">The expression on which the equivalence is to be applied.</param>
    /// <param name="checkType">The type-checking mode to use.</param>
    /// <returns>The result.</returns>
    public EquivalenceApplyResult Apply(
        IExpression expression,
        CheckType checkType = CheckType.CheckLeftOnly
    )
    {
        return EquivalenceApplier.Apply(this, expression, checkType);
    }

    private static bool _endWithPlaceholder(IExpression expression)
        => expression is IPlaceholderExpression
           || expression is IExpressionNode node && node.Children.Any(_endWithPlaceholder);

    private static void _throwIfAPlaceholderNameHasTwoValueTypes(IExpression leftSideExpression,
        IExpression rightSideExpression)
    {
        var valueTypes = new Dictionary<string, Type>();
        _collectPlaceholderValueTypes(leftSideExpression, valueTypes);
        _collectPlaceholderValueTypes(rightSideExpression, valueTypes);
    }

    private static void _collectPlaceholderValueTypes(IExpression expression, Dictionary<string, Type> valueTypes)
    {
        if (expression is IPlaceholderExpression)
        {
            var valueType = ExpressionValueType.Of(expression);
            if (valueTypes.TryGetValue(expression.Name, out var existing))
            {
                if (existing != valueType)
                    throw new ArgumentException(
                        $"The placeholder \"{expression.Name}\" is used with two different value types: {existing.Name} and {valueType.Name}.");
            }
            else
            {
                valueTypes.Add(expression.Name, valueType);
            }

            return;
        }

        if (expression is IExpressionNode node)
            foreach (var child in node.Children)
                _collectPlaceholderValueTypes(child, valueTypes);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder stringBuilder = new(LeftSideExpression.ToString());
        stringBuilder.Append(" = ");
        stringBuilder.Append(RightSideExpression);
        return stringBuilder.ToString();
    }
}
