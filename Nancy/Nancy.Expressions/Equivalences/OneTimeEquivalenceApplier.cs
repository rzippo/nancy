using System.Collections.Generic;
using System.Linq;
using Unipi.Nancy.Expressions.Utility;
using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;
using Unipi.Nancy.Numerics;

namespace Unipi.Nancy.Expressions.Equivalences;

/// <summary>
/// Applies an equivalence to an expression.
/// </summary>
/// <remarks>
/// The object holds the law and nothing else, so it can be applied as many times as wanted.
/// The bindings a match produced are returned in the result, not kept on the object.
/// </remarks>
public class OneTimeEquivalenceApplier
{
    /// <summary>
    /// The equivalence to apply.
    /// </summary>
    public required Equivalence Equivalence { get; init; }

    /// <summary>
    /// Checks the equivalence at the root of <paramref name="expression"/> and, if it matches, substitutes the other side.
    /// </summary>
    /// <param name="expression">The expression on which the equivalence is to be applied.</param>
    /// <param name="checkType">The direction in which the equivalence is applied.</param>
    /// <returns>The result, carrying the new expression and the bindings, or no match.</returns>
    public EquivalenceApplyResult Apply(
        IExpression expression,
        CheckType checkType = CheckType.CheckLeftOnly)
    {
        foreach (var (pattern, substitute) in SidesToTry(checkType))
        {
            var bindings = new LawMatchContext(Equivalence);
            if (!ExpressionPatternMatcher.TryMatchLaw(pattern, expression, true, bindings, out var leftover))
                continue;
            if (!bindings.AllHypothesesSatisfied())
                continue;

            var newExpression = (IGenericExpression<Curve>)Instantiate(substitute, bindings);
            return new EquivalenceApplyResult
            {
                NewExpression = newExpression,
                IsMatch = true,
                Bindings = bindings.Bindings,
                NotMatchedExpressions = leftover
            };
        }

        return new EquivalenceApplyResult { IsMatch = false };
    }

    private IEnumerable<(IExpression Pattern, IExpression Substitute)> SidesToTry(CheckType checkType)
    {
        switch (checkType)
        {
            case CheckType.CheckLeftOnly:
                yield return (Equivalence.LeftSideExpression, Equivalence.RightSideExpression);
                break;
            case CheckType.CheckRightOnly:
                yield return (Equivalence.RightSideExpression, Equivalence.LeftSideExpression);
                break;
            case CheckType.CheckBothSides:
                yield return (Equivalence.LeftSideExpression, Equivalence.RightSideExpression);
                yield return (Equivalence.RightSideExpression, Equivalence.LeftSideExpression);
                break;
        }
    }

    private static IExpression Instantiate(IExpression substitute, LawMatchContext law)
        => ExpressionRewriter.Instantiate(substitute, law.Bindings);
}
