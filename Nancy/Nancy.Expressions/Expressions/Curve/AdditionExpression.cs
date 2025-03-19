using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the addition (n-ary operation)
/// </summary>
public record AdditionExpression : CurveNAryExpression
{
    /// <summary>
    /// Creates an addition expression
    /// </summary>
    /// <param name="expressions">The operands (expressions) of the addition</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public AdditionExpression(IReadOnlyCollection<IGenericExpression<Curve>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates an addition expression from a collection of curves
    /// </summary>
    /// <param name="curves">Collection of curves representing the operands of the addition</param>
    /// <param name="names">Collection of the names of the curves operands</param>
    /// <param name="expressionName">The name of the expression</param>
    /// <param name="settings">Settings for the expression definition and evaluation</param>
    public AdditionExpression(IReadOnlyCollection<Curve> curves,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null) : base(
        curves, names, expressionName, settings)
    {
    }

    public override void Accept(ICurveExpressionVisitor visitor)
        => visitor.Visit(this);
}