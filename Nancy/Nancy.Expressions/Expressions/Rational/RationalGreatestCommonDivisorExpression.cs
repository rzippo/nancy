﻿using Unipi.Nancy.Expressions.Visitors;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Internals;

/// <summary>
/// Class representing an expression whose root operation is the g.c.d. between rational numbers (or expressions)
/// </summary>
public class RationalGreatestCommonDivisorExpression : RationalNAryExpression
{
    /// <summary>
    /// Creates a g.c.d. expression
    /// </summary>
    public RationalGreatestCommonDivisorExpression(IReadOnlyCollection<IGenericExpression<Rational>> expressions,
        string expressionName = "", ExpressionSettings? settings = null) : base(expressions, expressionName, settings)
    {
    }

    /// <summary>
    /// Creates a g.c.d. expression
    /// </summary>
    public RationalGreatestCommonDivisorExpression(IReadOnlyCollection<Rational> rationals,
        IReadOnlyCollection<string> names, string expressionName = "", ExpressionSettings? settings = null) : base(rationals, names, expressionName, settings)
    {
    }

    public override void Accept(IRationalExpressionVisitor visitor)
        => visitor.Visit(this);
}