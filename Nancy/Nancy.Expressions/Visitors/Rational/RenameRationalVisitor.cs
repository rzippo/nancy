﻿using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to change the name of a rational expression.
/// </summary>
public class RenameRationalVisitor : IRationalExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public RationalExpression Result = Expressions.FromRational(Rational.Zero);

    public string NewName { get; init; }

    /// <summary>
    /// Visitor class used to change the name of a rational expression.
    /// </summary>
    /// <param name="newName">The new name of the expression</param>
    public RenameRationalVisitor(string newName)
    {
        NewName = newName;
    }

    private void CommonVisit(RationalExpression expression)
    {
        Result = expression with
        {
            Name = NewName,
            // Since we know renaming does not change the result,
            // it is safe to explicitly copy over the cache fields
            _value = expression.Value,
        };
    }

    public virtual void Visit(HorizontalDeviationExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(VerticalDeviationExpression expression)
        => CommonVisit(expression);

    public void Visit(ValueAtExpression expression)
        => CommonVisit(expression);

    public void Visit(LeftLimitAtExpression expression)
        => CommonVisit(expression);

    public void Visit(RightLimitAtExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalAdditionExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalSubtractionExpression expression)
        => CommonVisit(expression);
    
    public virtual void Visit(RationalProductExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalDivisionExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalLeastCommonMultipleExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalGreatestCommonDivisorExpression expression)
        => CommonVisit(expression);

    public void Visit(RationalMinimumExpression expression)
        => CommonVisit(expression);

    public void Visit(RationalMaximumExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalNumberExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(NegateRationalExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(InvertRationalExpression expression)
        => CommonVisit(expression);

    public virtual void Visit(RationalPlaceholderExpression expression)
        => CommonVisit(expression);
}