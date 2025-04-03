using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to update the name of a rational expression.
/// </summary>
public class ChangeNameRationalVisitor : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public RationalExpression Result = Expressions.FromRational(Rational.Zero);

    public string NewName { get; init; }

    /// <summary>
    /// Visitor class used to update the name of a rational expression.
    /// </summary>
    /// <param name="newName">The new name of the expression</param>
    public ChangeNameRationalVisitor(string newName)
    {
        NewName = newName;
    }

    public virtual void Visit(HorizontalDeviationExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(VerticalDeviationExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(ValueAtExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(LeftLimitAtExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(RightLimitAtExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalAdditionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalSubtractionExpression expression)
        => Result = expression with { Name = NewName };
    
    public virtual void Visit(RationalProductExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalDivisionExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(RationalMinimumExpression expression)
        => Result = expression with { Name = NewName };

    public void Visit(RationalMaximumExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalNumberExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(NegateRationalExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(InvertRationalExpression expression)
        => Result = expression with { Name = NewName };

    public virtual void Visit(RationalPlaceholderExpression expression)
        => Result = expression with { Name = NewName };
}