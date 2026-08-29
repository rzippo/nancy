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
    public IReadOnlyCollection<IGenericExpression<Rational>> Operands { get; }

    /// <inheritdoc cref="IGenericNAryExpression{T1,TResult}.Expressions"/>
    [Obsolete("Renamed to Operands.")]
    public IReadOnlyCollection<IGenericExpression<Rational>> Expressions => Operands;

    /// <summary>
    /// Creates the n-ary operation starting from a collection of expression operands.
    /// </summary>
    public RationalNAryExpression(
        IReadOnlyCollection<IGenericExpression<Rational>> operands,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        Operands = operands;
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
        List<IGenericExpression<Rational>> operands = [];
        foreach (var (rational, name) in rationals.Zip(names, (c, n) => (curve: c, name: n)))
            operands.Add(new RationalNumberExpression(rational, name));
        Operands = operands;
    }

    /// <summary>
    /// Adds another operand to the expression.
    /// </summary>
    public RationalExpression Append(IGenericExpression<Rational> operand, string expressionName = "", ExpressionSettings? settings = null)
    {
        if (GetType() == operand.GetType() && string.IsNullOrEmpty(operand.Name))
            return (RationalExpression)Activator.CreateInstance(GetType(),
                (IReadOnlyCollection<IGenericExpression<Rational>>)
                [.. Operands, .. ((RationalNAryExpression)operand).Operands], expressionName, settings)!;
        return (RationalExpression)Activator.CreateInstance(GetType(),
            (IReadOnlyCollection<IGenericExpression<Rational>>) [.. Operands, operand], expressionName, settings)!;
    }

    /// <summary>
    /// The widest possible operand list for this operator, descending into a child of the same concrete operator type whether or not it carries a bound <see cref="IExpression.Name"/>.
    /// </summary>
    /// <remarks>
    /// A named boundary marks the tree's own shape, and <see cref="Operands"/> stops there by construction; this recurses through it, for a caller that wants every operand the operator ultimately combines.
    /// Uncached by design, so it costs a walk on each call and adds no field or invalidation question.
    /// Caching it later, along the lines of <see cref="RationalExpression.Value"/>, becomes worthwhile if profiling shows repeated calls on the same node are a real cost.
    /// </remarks>
    public IReadOnlyCollection<IGenericExpression<Rational>> FlattenOperands()
    {
        List<IGenericExpression<Rational>> result = [];
        foreach (var operand in Operands)
        {
            if (operand.GetType() == GetType())
                result.AddRange(((RationalNAryExpression)operand).FlattenOperands());
            else
                result.Add(operand);
        }
        return result;
    }
}