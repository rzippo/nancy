using Nancy.Expressions.Expressions;
using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions;

/// <summary>
/// Class representing expressions whose value is a <see cref="Curve"/> object and the root is an operation which
/// accepts n (n >= 2) operands (which are curve expressions) and is commutative and associative.
/// </summary>
public abstract class
    CurveNAryExpression : CurveExpression, IGenericNAryExpression<Curve, Curve> // For operators on curves that are commutative and associative
{
    public IReadOnlyCollection<IGenericExpression<Curve>> Expressions { get; }

    /// <summary>
    /// Creates the n-ary expression starting from a collection of expression operands
    /// </summary>
    public CurveNAryExpression(
        IReadOnlyCollection<IGenericExpression<Curve>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressionName, settings)
    {
        Expressions = expressions;
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
        List<IGenericExpression<Curve>> expressions = [];
        foreach (var (curve, name) in curves.Zip(names, (c, n) => (curve: c, name: n)))
            expressions.Add(new ConcreteCurveExpression(curve, name));
        Expressions = expressions;
    }

    /// <summary>
    /// Adds another operand to the expression
    /// </summary>
    public CurveExpression Append(IGenericExpression<Curve> expression, string expressionName = "", ExpressionSettings? settings = null)
    {
        if (GetType() == expression.GetType())
            return (CurveExpression)Activator.CreateInstance(GetType(),
                (IReadOnlyCollection<IGenericExpression<Curve>>)
                [.. Expressions, .. ((CurveNAryExpression)expression).Expressions], expressionName, settings)!;
        return (CurveExpression)Activator.CreateInstance(GetType(),
            (IReadOnlyCollection<IGenericExpression<Curve>>) [.. Expressions, expression], expressionName, settings)!;
    }
}