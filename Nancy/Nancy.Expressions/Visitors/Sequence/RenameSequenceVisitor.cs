using Unipi.Nancy.Expressions.Nodes;
using Unipi.Nancy.MinPlusAlgebra;

namespace Unipi.Nancy.Expressions.Visitors;

/// <summary>
/// Visitor class used to change the name and/or generation of a sequence expression.
/// </summary>
public class RenameSequenceVisitor : ISequenceExpressionVisitor
{
    /// <summary>
    /// Field used as intermediate and final result of the visitor.
    /// </summary>
    public SequenceExpression Result = Expressions.FromSequence(Sequence.Zero(0, 1));

    /// <summary>
    /// The new name of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public string? NewName { get; init; }

    /// <summary>
    /// The new generation of the expression, or <see langword="null"/> to keep the visited expression's own.
    /// </summary>
    public int? NewGeneration { get; init; }

    /// <summary>
    /// Visitor class used to change the name and/or generation of a sequence expression.
    /// </summary>
    /// <param name="newName">The new name of the expression, or <see langword="null"/> to keep it as-is.</param>
    /// <param name="newGeneration">The new generation of the expression, or <see langword="null"/> to keep it as-is.</param>
    public RenameSequenceVisitor(string? newName = null, int? newGeneration = null)
    {
        NewName = newName;
        NewGeneration = newGeneration;
    }

    private void CommonVisit(SequenceExpression expression)
    {
        Result = expression with
        {
            Name = NewName ?? expression.Name,
            Generation = NewGeneration ?? expression.Generation,
            // Since we know renaming does not change the result, it is safe to explicitly copy over the cache fields.
            _value = expression._value,
            _isLeftContinuous = expression._isLeftContinuous,
            _isRightContinuous = expression._isRightContinuous,
            _isNonNegative = expression._isNonNegative,
            _isNonDecreasing = expression._isNonDecreasing,
            _isIncreasing = expression._isIncreasing,
        };
    }

    /// <inheritdoc />
    public virtual void Visit(ConcreteSequenceExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(CurveCutExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(CurveCutToNeighbourhoodExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceCutExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceCutToNeighbourhoodExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceAdditionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceSubtractionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceToNonNegativeExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMinimumExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMaximumExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceConvolutionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMaxPlusConvolutionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceDeconvolutionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceMaxPlusDeconvolutionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceNegateExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceFloorExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceCeilExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceToLeftContinuousExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceToRightContinuousExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceLowerPseudoInverseExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceUpperPseudoInverseExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceScaleExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceDelayExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceForwardExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceHorizontalShiftExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceVerticalShiftExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequenceCompositionExpression expression) => CommonVisit(expression);

    /// <inheritdoc />
    public virtual void Visit(SequencePlaceholderExpression expression) => CommonVisit(expression);
}
