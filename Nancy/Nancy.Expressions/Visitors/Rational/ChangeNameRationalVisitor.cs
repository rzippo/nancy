using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to update the name of a rational expression.
/// </summary>
/// <param name="newName">The new name of the expression</param>
public class ChangeNameRationalVisitor(string newName) : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public RationalExpression Result = Expressions.FromRational(Rational.Zero);

    public virtual void Visit(HorizontalDeviationExpression expression)
        => Result = Expressions.HorizontalDeviation((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(VerticalDeviationExpression expression)
        => Result = Expressions.VerticalDeviation((CurveExpression)expression.LeftExpression,
            (CurveExpression)expression.RightExpression, newName);

    public virtual void Visit(RationalAdditionExpression expression)
        => Result = new RationalAdditionExpression(expression.Expressions, newName);

    public virtual void Visit(RationalSubtractionExpression expression)
        => Result = new RationalSubtractionExpression(expression.LeftExpression, expression.RightExpression, newName);
    
    public virtual void Visit(RationalProductExpression expression)
        => Result = new RationalProductExpression(expression.Expressions, newName);

    public virtual void Visit(RationalDivisionExpression expression)
        => Result = new RationalDivisionExpression(expression.LeftExpression, expression.RightExpression, newName);

    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => Result = new RationalLeastCommonMultipleExpression(expression.Expressions, newName);

    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => Result = new RationalGreatestCommonDivisorExpression(expression.Expressions, newName);

    public virtual void Visit(RationalNumberExpression expression)
        => Result = Expressions.FromRational(expression.Value, newName);

    public virtual void Visit(NegateRationalExpression expression)
        => Result = Expressions.Negate((RationalExpression)expression.Expression, newName);

    public virtual void Visit(InvertRationalExpression expression)
        => Result = Expressions.Invert((RationalExpression)expression.Expression, newName);

    public virtual void Visit(RationalPlaceholderExpression expression)
        => Result = new RationalPlaceholderExpression(newName);
}