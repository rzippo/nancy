using Unipi.Nancy.Expressions.Internals;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

// todo: is this redundant?

/// <summary>
/// Visitor class used to change the name of a curve expression.
/// </summary>
public class RenameCurveVisitor : ICurveExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor
    /// </summary>
    public CurveExpression Result = Expressions.FromCurve(Curve.Zero());

    /// <summary>
    /// The new name of the expression.
    /// </summary>
    public string NewName { get; init; }

    /// <summary>
    /// Visitor class used to change the name of a curve expression.
    /// </summary>
    /// <param name="newName">The new name of the expression.</param>
    public RenameCurveVisitor(string newName)
    {
        NewName = newName;
    }

    private void CommonVisit(CurveExpression expression)
    {
        Result = expression with
        {
            Name = NewName,
            // Since we know renaming does not change the result,
            // it is safe to explicitly copy over the cache fields
            _value = expression._value,
            _isSubAdditive = expression._isSubAdditive,
            _isSuperAdditive = expression._isSuperAdditive,
            _isLeftContinuous = expression._isLeftContinuous,
            _isRightContinuous = expression._isRightContinuous,
            _isNonNegative = expression._isNonNegative,
            _isNonDecreasing = expression._isNonDecreasing,
            _isIncreasing = expression._isIncreasing,
            _isConcave = expression._isConcave,
            _isConvex = expression._isConvex,
            _isPassingThroughOrigin = expression._isPassingThroughOrigin,
            _isUltimatelyFinite = expression._isUltimatelyFinite,
            _isPlain = expression._isPlain,
            _isUltimatelyPlain = expression._isUltimatelyPlain,
            _isUltimatelyAffine = expression._isUltimatelyAffine,
            _isUltimatelyConstant = expression._isUltimatelyConstant,
            _isWellDefined = expression._isWellDefined,
        };
    }

    /// <inheritdoc />
    public virtual void Visit(ConcreteCurveExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(NegateExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToNonNegativeExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SubAdditiveClosureExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SuperAdditiveClosureExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonDecreasingExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonDecreasingExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToUpperNonIncreasingExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLowerNonIncreasingExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToLeftContinuousExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ToRightContinuousExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(WithZeroOriginExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(LowerPseudoInverseExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(UpperPseudoInverseExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(AdditionExpression expression)
        => CommonVisit(expression);


    /// <inheritdoc />
    public virtual void Visit(SubtractionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MinimumExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaximumExpression expression)
        => CommonVisit(expression);


    /// <inheritdoc />
    public virtual void Visit(ConvolutionExpression expression)
        => CommonVisit(expression);


    /// <inheritdoc />
    public virtual void Visit(DeconvolutionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(MaxPlusConvolutionExpression expression)
        => CommonVisit(expression);


    /// <inheritdoc />
    public virtual void Visit(MaxPlusDeconvolutionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(CompositionExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(DelayByExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ForwardByExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(HorizontalShiftExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(VerticalShiftExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(CurvePlaceholderExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(ScaleExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(WithOriginAtExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(FloorExpression expression)
        => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(CeilExpression expression)
        => CommonVisit(expression);
}