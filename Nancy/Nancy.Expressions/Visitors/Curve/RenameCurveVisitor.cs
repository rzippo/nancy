using Unipi.Nancy.Expressions.Nodes;
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
    /// The new name of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public string? NewName { get; init; }

    /// <summary>
    /// The new generation of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public int? NewGeneration { get; init; }

    /// <summary>
    /// Visitor class used to change the name and/or generation of a curve expression.
    /// </summary>
    /// <param name="newName">The new name of the expression, or <see langword="null"/> to keep it as-is.</param>
    /// <param name="newGeneration">The new generation of the expression, or <see langword="null"/> to keep it as-is.</param>
    public RenameCurveVisitor(string? newName = null, int? newGeneration = null)
    {
        NewName = newName;
        NewGeneration = newGeneration;
    }

    private void CommonVisit(CurveExpression expression)
    {
        Result = expression with
        {
            Name = NewName ?? expression.Name,
            Generation = NewGeneration ?? expression.Generation,
            // Since we know renaming does not change the result,
            // it is safe to explicitly copy over the cache fields
            _value = Volatile.Read(ref expression._value) is { IsValueCreated: true } cache_value ? cache_value : null,
            _isSubAdditive = Volatile.Read(ref expression._isSubAdditive) is { IsValueCreated: true } cache_isSubAdditive ? cache_isSubAdditive : null,
            _isSuperAdditive = Volatile.Read(ref expression._isSuperAdditive) is { IsValueCreated: true } cache_isSuperAdditive ? cache_isSuperAdditive : null,
            _isLeftContinuous = Volatile.Read(ref expression._isLeftContinuous) is { IsValueCreated: true } cache_isLeftContinuous ? cache_isLeftContinuous : null,
            _isRightContinuous = Volatile.Read(ref expression._isRightContinuous) is { IsValueCreated: true } cache_isRightContinuous ? cache_isRightContinuous : null,
            _isNonNegative = Volatile.Read(ref expression._isNonNegative) is { IsValueCreated: true } cache_isNonNegative ? cache_isNonNegative : null,
            _isNonDecreasing = Volatile.Read(ref expression._isNonDecreasing) is { IsValueCreated: true } cache_isNonDecreasing ? cache_isNonDecreasing : null,
            _isIncreasing = Volatile.Read(ref expression._isIncreasing) is { IsValueCreated: true } cache_isIncreasing ? cache_isIncreasing : null,
            _isConcave = Volatile.Read(ref expression._isConcave) is { IsValueCreated: true } cache_isConcave ? cache_isConcave : null,
            _isConvex = Volatile.Read(ref expression._isConvex) is { IsValueCreated: true } cache_isConvex ? cache_isConvex : null,
            _isPassingThroughOrigin = Volatile.Read(ref expression._isPassingThroughOrigin) is { IsValueCreated: true } cache_isPassingThroughOrigin ? cache_isPassingThroughOrigin : null,
            _isUltimatelyFinite = Volatile.Read(ref expression._isUltimatelyFinite) is { IsValueCreated: true } cache_isUltimatelyFinite ? cache_isUltimatelyFinite : null,
            _isPlain = Volatile.Read(ref expression._isPlain) is { IsValueCreated: true } cache_isPlain ? cache_isPlain : null,
            _isUltimatelyPlain = Volatile.Read(ref expression._isUltimatelyPlain) is { IsValueCreated: true } cache_isUltimatelyPlain ? cache_isUltimatelyPlain : null,
            _isUltimatelyAffine = Volatile.Read(ref expression._isUltimatelyAffine) is { IsValueCreated: true } cache_isUltimatelyAffine ? cache_isUltimatelyAffine : null,
            _isUltimatelyConstant = Volatile.Read(ref expression._isUltimatelyConstant) is { IsValueCreated: true } cache_isUltimatelyConstant ? cache_isUltimatelyConstant : null,
            _isWellDefined = Volatile.Read(ref expression._isWellDefined) is { IsValueCreated: true } cache_isWellDefined ? cache_isWellDefined : null,
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