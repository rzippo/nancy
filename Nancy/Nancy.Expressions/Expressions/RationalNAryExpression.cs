using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class representing expressions whose value is a <see cref="Rational"/> object and the root is an operation which
/// accepts n (n >= 2) operands (which are rational expressions) and is commutative and associative.
/// </summary>
public abstract record
    RationalNAryExpression : RationalExpression, IGenericNAryExpression<Rational, Rational> // For operators on rationals that are commutative and associative
{
    /// <summary>
    /// The operands of this operator.
    /// </summary>
    public IReadOnlyCollection<IGenericExpression<Rational>> Expressions { get; }

    /// <summary>
    /// Creates the n-ary operation starting from a collection of expression operands.
    /// </summary>
    public RationalNAryExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        Expressions = expressions;
    }

    /// <summary>
    /// Creates the n-ary expression starting from a collection of operands of type <see cref="Rational"/> (converted to
    /// <see cref="RationalNumberExpression"/> objects).
    /// </summary>
    public RationalNAryExpression(
        IReadOnlyCollection<Rational> rationals,
        IReadOnlyCollection<string> names,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        List<IGenericExpression<Rational>> expressions = [];
        foreach (var (rational, name) in rationals.Zip(names, (c, n) => (curve: c, name: n)))
            expressions.Add(new RationalNumberExpression(rational, name));
        Expressions = expressions;
    }

    /// <summary>
    /// Adds another operand to the expression.
    /// </summary>
    public RationalExpression Append(IGenericExpression<Rational> expression, string expressionName = "", ExpressionSettings? settings = null)
    {
        if (GetType() == expression.GetType())
            return (RationalExpression)Activator.CreateInstance(GetType(),
                (IReadOnlyCollection<IGenericExpression<Rational>>)
                [.. Expressions, .. ((RationalNAryExpression)expression).Expressions], expressionName, settings)!;
        return (RationalExpression)Activator.CreateInstance(GetType(),
            (IReadOnlyCollection<IGenericExpression<Rational>>) [.. Expressions, expression], expressionName, settings)!;
    }
}