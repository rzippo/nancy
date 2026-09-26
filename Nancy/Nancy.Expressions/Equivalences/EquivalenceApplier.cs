using System.Collections.Generic;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Applies an equivalence to an expression.
/// </summary>
/// <remarks>
/// There is nothing to hold between two applications: the equivalence and the expression both arrive as arguments, and the bindings a match produced are returned in the result.
/// </remarks>
public static class EquivalenceApplier
{
    /// <summary>
    /// Checks <paramref name="equivalence"/> at the root of <paramref name="expression"/> and, if it matches, substitutes the other side.
    /// </summary>
    /// <param name="equivalence">The equivalence to apply.</param>
    /// <param name="expression">The expression on which the equivalence is to be applied.</param>
    /// <param name="checkType">The direction in which the equivalence is applied.</param>
    /// <returns>The result, carrying the new expression and the bindings, or no match.</returns>
    public static EquivalenceApplyResult Apply(
        Equivalence equivalence,
        IExpression expression,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        foreach (var (pattern, substitute) in SidesToTry(equivalence, checkType))
        {
            var law = new LawMatchContext(equivalence);
            if (!ExpressionPatternMatcher.TryMatchLaw(pattern, expression, true, law, out var leftover))
                continue;
            if (!law.AllHypothesesSatisfied())
                continue;

            var newExpression = (IGenericExpression<Curve>)ExpressionRewriter.Instantiate(substitute, law.Bindings);
            return new EquivalenceApplyResult
            {
                NewExpression = newExpression,
                IsMatch = true,
                Bindings = law.Bindings,
                NotMatchedExpressions = leftover
            };
        }

        return new EquivalenceApplyResult { IsMatch = false };
    }

    private static IEnumerable<(IExpression Pattern, IExpression Substitute)> SidesToTry(
        Equivalence equivalence,
        CheckType checkType)
    {
        switch (checkType)
        {
            case CheckType.CheckLeftOnly:
                yield return (equivalence.LeftSideExpression, equivalence.RightSideExpression);
                break;
            case CheckType.CheckRightOnly:
                yield return (equivalence.RightSideExpression, equivalence.LeftSideExpression);
                break;
            case CheckType.CheckBothSides:
                yield return (equivalence.LeftSideExpression, equivalence.RightSideExpression);
                yield return (equivalence.RightSideExpression, equivalence.LeftSideExpression);
                break;
        }
    }
}
