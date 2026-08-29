using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class representing expressions whose value is a <see cref="Curve"/> object and the root is an operation which
/// accepts n (n >= 2) operands (which are curve expressions) and is commutative and associative.
/// </summary>
public abstract record
    CurveNAryExpression : CurveExpression, IGenericNAryExpression<Curve, Curve> // For operators on curves that are commutative and associative
{
    /// <inheritdoc />
    public IReadOnlyCollection<IGenericExpression<Curve>> Operands { get; }

    /// <inheritdoc cref="IGenericNAryExpression{T1,TResult}.Expressions"/>
    [Obsolete("Renamed to Operands.")]
    public IReadOnlyCollection<IGenericExpression<Curve>> Expressions => Operands;

    /// <summary>
    /// Creates the n-ary expression starting from a collection of expression operands
    /// </summary>
    public CurveNAryExpression(
        IReadOnlyCollection<IGenericExpression<Curve>> operands,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        Operands = operands;
    }

    /// <summary>
    /// Creates the n-ary expression starting from a collection of operands of type <see cref="Curve"/> (converted to
    /// <see cref="ConcreteCurveExpression"/> objects)
    /// </summary>
    public CurveNAryExpression(
        IReadOnlyCollection<Curve> curves,
        IReadOnlyCollection<string> names,
        string expressionName = "", 
        ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        List<IGenericExpression<Curve>> operands = [];
        foreach (var (curve, name) in curves.Zip(names, (c, n) => (curve: c, name: n)))
            operands.Add(new ConcreteCurveExpression(curve, name));
        Operands = operands;
    }

    /// <summary>
    /// True if <paramref name="other"/> is the same operator over the same operands, as an unordered multiset.
    /// </summary>
    /// <remarks>
    /// <c>Addition(a, b)</c> equals <c>Addition(b, a)</c>, the operator being commutative.
    /// Each operand is paired off against a candidate confirmed by a real <see cref="object.Equals(object?)"/> call, with hash equality serving as a cheap filter first, so two distinct operands that collide on their hash still compare correctly.
    /// </remarks>
    public virtual bool Equals(CurveNAryExpression? other)
    {
        if (other is null || !base.Equals(other))
            return false;
        if (Operands.Count != other.Operands.Count)
            return false;

        var remaining = other.Operands.ToList();
        foreach (var operand in Operands)
        {
            var hash = operand.GetHashCode();
            var index = remaining.FindIndex(candidate => candidate.GetHashCode() == hash && operand.Equals(candidate));
            if (index < 0)
                return false;
            remaining.RemoveAt(index);
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        foreach (var operandHash in Operands.Select(operand => operand.GetHashCode()).OrderBy(h => h))
            hash.Add(operandHash);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Adds another operand to the expression
    /// </summary>
    public CurveExpression Append(IGenericExpression<Curve> operand, string expressionName = "", ExpressionSettings? settings = null)
    {
        if (GetType() == operand.GetType() && string.IsNullOrEmpty(operand.Name))
            return (CurveExpression)Activator.CreateInstance(GetType(),
                (IReadOnlyCollection<IGenericExpression<Curve>>)
                [.. Operands, .. ((CurveNAryExpression)operand).Operands], expressionName, settings)!;
        return (CurveExpression)Activator.CreateInstance(GetType(),
            (IReadOnlyCollection<IGenericExpression<Curve>>) [.. Operands, operand], expressionName, settings)!;
    }

    /// <summary>
    /// The widest possible operand list for this operator, descending into a child of the same concrete operator type whether or not it carries a bound <see cref="IExpression.Name"/>.
    /// </summary>
    /// <remarks>
    /// A named boundary marks the tree's own shape, and <see cref="Operands"/> stops there by construction; this recurses through it, for a caller that wants every operand the operator ultimately combines.
    /// Uncached by design, so it costs a walk on each call and adds no field or invalidation question.
    /// Caching it later, along the lines of <see cref="CurveExpression.Value"/>, becomes worthwhile if profiling shows repeated calls on the same node are a real cost.
    /// </remarks>
    public IReadOnlyCollection<IGenericExpression<Curve>> FlattenOperands()
    {
        List<IGenericExpression<Curve>> result = [];
        foreach (var operand in Operands)
        {
            if (operand.GetType() == GetType())
                result.AddRange(((CurveNAryExpression)operand).FlattenOperands());
            else
                result.Add(operand);
        }
        return result;
    }
}