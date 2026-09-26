namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Precedence level of an MPPG expression, used to decide where parentheses are needed.
/// </summary>
/// <remarks>
/// The MPPG syntax has three levels, and all its infix operators are left-associative.
/// An operand is parenthesized when its level is lower than the level required by the position it appears in.
/// </remarks>
public enum MppgPrecedence
{
    /// <summary>
    /// Level of the sum operators, i.e. <c>+</c>, <c>-</c>, <c>/\</c> and <c>\/</c>.
    /// </summary>
    Sum = 0,

    /// <summary>
    /// Level of the product operators, i.e. <c>*</c>, <c>*^</c>, <c>/</c>, <c>/^</c>, <c>comp</c>, <c>div</c> and <c>mod</c>.
    /// </summary>
    Product = 1,

    /// <summary>
    /// Level of the operands that never need parentheses, i.e. names, literals, call forms and sampling.
    /// </summary>
    Atom = 2
}
