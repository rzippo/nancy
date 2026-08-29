namespace Unipi.Nancy.Expressions.ExpressionsUtility.Internals;

/// <summary>
/// Dispatches <c>ClearValueCache</c> onto a child of either expression hierarchy.
/// </summary>
/// <remarks>
/// A <see cref="CurveExpression"/> can embed a <see cref="RationalExpression"/> operand, a scale factor for instance, and a <see cref="RationalExpression"/> can embed curve operands, as a horizontal deviation does.
/// The recursive walk crosses that boundary without knowing, at any given node, which side a child belongs to.
/// </remarks>
internal static class ClearValueCacheDispatch
{
    public static void Clear(IExpression child, CacheClearScope scope)
    {
        switch (child)
        {
            case CurveExpression curveChild:
                curveChild.ClearValueCache(scope);
                break;
            case RationalExpression rationalChild:
                rationalChild.ClearValueCache(scope);
                break;
        }
    }
}
